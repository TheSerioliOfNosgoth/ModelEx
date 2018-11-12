using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using SRFile = CDC.Objects.SRFile;
using SR1File = CDC.Objects.SR1File;
using SR2File = CDC.Objects.SR2File;
using DefianceFile = CDC.Objects.DefianceFile;
using SRModel = CDC.Objects.Models.SRModel;
using SR1Model = CDC.Objects.Models.SR1Model;
using SR2Model = CDC.Objects.Models.SR2Model;
using DefianceModel = CDC.Objects.Models.DefianceModel;
using Tree = CDC.Tree;
using SR1PCTextureFile = BenLincoln.TheLostWorlds.CDTextures.SoulReaverPCTextureFile;
using SR1PSTextureFile = BenLincoln.TheLostWorlds.CDTextures.SoulReaverPlaystationTextureFile;
using SR1DCTextureFile = BenLincoln.TheLostWorlds.CDTextures.SoulReaverDreamcastTextureFile;
using SR2PCTextureFile = BenLincoln.TheLostWorlds.CDTextures.SoulReaver2PCVRMTextureFile;

namespace ModelEx
{
    public class SceneCDC : Scene
    {
        class SRModelParser :
            IModelParser
        {
            string _objectName;
            SRFile _srFile;
            SRModel _srModel;
            public Model Model;
            public List<Material> Materials { get; } = new List<Material>();
            public List<Mesh> Meshes { get; } = new List<Mesh>();
            public List<SubMesh> SubMeshes { get; } = new List<SubMesh>();
            public List<Node> Groups { get; } = new List<Node>();

            public SRModelParser(string objectName, SRFile srFile)
            {
                _objectName = objectName;
                _srFile = srFile;
            }

            public void BuildModel(int modelIndex)
            {
                _srModel = _srFile.Models[modelIndex];
                String modelName = _objectName + "-" + modelIndex.ToString();

                #region Materials
                ProgressStage = "Model " + modelIndex.ToString() + " - Creating Materials";
                Thread.Sleep(500);
                for (int materialIndex = 0; materialIndex < _srModel.MaterialCount; materialIndex++)
                {
                    Material material = new Material();
                    Color colorDiffuse = Color.FromArgb((int)unchecked(_srModel.Materials[materialIndex].colour));
                    material.Diffuse = colorDiffuse;
                    material.TextureFileName = GetTextureName(_srModel, materialIndex);
                    Materials.Add(material);

                    progressLevel += _srModel.IndexCount / _srModel.Groups.Length;
                }
                #endregion

                #region Groups
                for (int groupIndex = 0; groupIndex < _srModel.Groups.Length; groupIndex++)
                {
                    ProgressStage = "Model " + modelIndex.ToString() + " - Creating Group " + groupIndex.ToString();
                    Thread.Sleep(100);

                    Tree srGroup = _srModel.Groups[groupIndex];
                    String groupName = String.Format("{0}-{1}-group-{2}", _objectName, modelIndex, groupIndex);
                    if (srGroup != null && srGroup.mesh != null &&
                        srGroup.mesh.indexCount > 0 && srGroup.mesh.polygonCount > 0)
                    {
                        Node group = new Node();
                        SRMeshParser meshParser = new SRMeshParser(_objectName, _srFile);
                        meshParser.BuildMesh(modelIndex, groupIndex, 0);
                        foreach (SubMesh subMesh in meshParser.SubMeshes)
                        {
                            // If the mesh parser knew the total submeshes for the model,
                            // then this could be done inside BuildMesh.
                            subMesh.MeshIndex = Meshes.Count;
                            group.SubMeshIndices.Add(SubMeshes.Count);
                            SubMeshes.Add(subMesh);
                        }
                        Meshes.Add(meshParser.Mesh);
                        group.Name = groupName;
                        Groups.Add(group);
                    }
                }
                #endregion

                ModelName = modelName;

                if (_srFile.Asset == CDC.Asset.Unit)
                {
                    Model = new Unit(this);
                }
                else
                {
                    Model = new Physical(this);
                }
            }

            public string ModelName
            {
                get;
                private set;
            }
        }

        class SRMeshParser :
            IMeshParser<PositionNormalTexturedVertex, short>,
            IMeshParser<PositionColorTexturedVertex, short>,
            IMeshParser<Position2Color2TexturedVertex, short>
        {
            string _objectName;
            SRFile _srFile;
            SRModel _srModel;
            Tree _srGroup;
            List<int> _vertexList = new List<int>();
            List<int> _indexList = new List<int>();
            public List<SubMesh> SubMeshes { get; } = new List<SubMesh>();
            public Mesh Mesh;

            public SRMeshParser(string objectName, SRFile srFile)
            {
                _objectName = objectName;
                _srFile = srFile;
            }

            public void BuildMesh(int modelIndex, int groupIndex, int meshIndex)
            {
                _srModel = _srFile.Models[modelIndex];
                _srGroup = _srModel.Groups[groupIndex];
                String modelName = String.Format("{0}-{1}", _objectName, modelIndex);
                String groupName = String.Format("{0}-{1}-group-{2}", _objectName, modelIndex, groupIndex);
                String meshName = String.Format("{0}-{1}-group-{2}-mesh-{3}", _objectName, modelIndex, groupIndex, meshIndex);

                int startIndexLocation = 0;
                for (int materialIndex = 0; materialIndex < _srModel.MaterialCount; materialIndex++)
                {
                    int indexCount = 0;
                    int totalIndexCount = (int)_srGroup.mesh.indexCount;
                    for (int v = 0; v < totalIndexCount; v++)
                    {
                        if (_srGroup.mesh.polygons[v / 3].material.ID == materialIndex)
                        {
                            _vertexList.Add(v);
                            _indexList.Add(_indexList.Count - startIndexLocation);
                            indexCount++;
                        }
                    }

                    if (indexCount > 0)
                    {
                        String subMeshName = String.Format("{0}-{1}-group-{2}-submesh-{3}", _objectName, modelIndex, groupIndex, materialIndex);
                        SubMesh subMesh = new SubMesh
                        {
                            Name = subMeshName,
                            MaterialIndex = materialIndex,
                            indexCount = indexCount,
                            startIndexLocation = startIndexLocation,
                            baseVertexLocation = startIndexLocation
                        };
                        SubMeshes.Add(subMesh);

                        startIndexLocation += indexCount;
                    }
                }

                if (SubMeshes.Count > 0)
                {
                    MeshName = meshName;
                    Technique = _srFile.Game == CDC.Game.SR1 ? "SR1Render" : "SR2Render";
                    if (_srFile.Asset == CDC.Asset.Unit)
                    {
                        //Mesh = new MeshPCT(this);
                        Mesh = new MeshMorphingUnit(this);
                    }
                    else
                    {
                        Mesh = new MeshPNT(this);
                    }
                }
            }

            public string MeshName
            {
                get;
                private set;
            }

            public string Technique
            {
                get;
                private set;
            }

            public int VertexCount { get { return _vertexList.Count; } }

            public void FillVertex(int v, out PositionNormalTexturedVertex vertex)
            {
                ref CDC.Vertex exVertex = ref _srGroup.mesh.vertices[_vertexList[v]];

                vertex.Position = new SlimDX.Vector3()
                {
                    X = 0.01f * _srModel.Positions[exVertex.positionID].x,
                    Y = 0.01f * _srModel.Positions[exVertex.positionID].z,
                    Z = 0.01f * _srModel.Positions[exVertex.positionID].y
                };

                vertex.Normal = new SlimDX.Vector3()
                {
                    X = _srModel.Normals[exVertex.normalID].x,
                    Y = _srModel.Normals[exVertex.normalID].z,
                    Z = _srModel.Normals[exVertex.normalID].y
                };
                vertex.Normal.Normalize();

                vertex.TextureCoordinates = new SlimDX.Vector2()
                {
                    X = _srModel.UVs[exVertex.UVID].u,
                    Y = _srModel.UVs[exVertex.UVID].v
                };
            }

            public void FillVertex(int v, out PositionColorTexturedVertex vertex)
            {
                ref CDC.Vertex exVertex = ref _srGroup.mesh.vertices[_vertexList[v]];

                vertex.Position = new SlimDX.Vector3()
                {
                    X = 0.01f * _srModel.Positions[exVertex.positionID].x,
                    Y = 0.01f * _srModel.Positions[exVertex.positionID].z,
                    Z = 0.01f * _srModel.Positions[exVertex.positionID].y
                };

                vertex.Color = new SlimDX.Color3()
                {
                    //Alpha = ((_srModel.Colours[vertex.colourID] & 0xFF000000) >> 24) / 255.0f,
                    Red = ((_srModel.Colours[exVertex.colourID] & 0x00FF0000) >> 16) / 255.0f,
                    Green = ((_srModel.Colours[exVertex.colourID] & 0x0000FF00) >> 8) / 255.0f,
                    Blue = ((_srModel.Colours[exVertex.colourID] & 0x000000FF) >> 0) / 255.0f
                };

                vertex.TextureCoordinates = new SlimDX.Vector2()
                {
                    X = _srModel.UVs[exVertex.UVID].u,
                    Y = _srModel.UVs[exVertex.UVID].v
                };
            }

            public void FillVertex(int v, out Position2Color2TexturedVertex vertex)
            {
                ref CDC.Vertex exVertex = ref _srGroup.mesh.vertices[_vertexList[v]];

                vertex.Position0 = new SlimDX.Vector3()
                {
                    X = 0.01f * _srModel.Positions[exVertex.positionID].x,
                    Y = 0.01f * _srModel.Positions[exVertex.positionID].z,
                    Z = 0.01f * _srModel.Positions[exVertex.positionID].y
                };

                vertex.Position1 = new SlimDX.Vector3()
                {
                    X = 0.01f * _srModel.PositionsAlt[exVertex.positionID].x,
                    Y = 0.01f * _srModel.PositionsAlt[exVertex.positionID].z,
                    Z = 0.01f * _srModel.PositionsAlt[exVertex.positionID].y
                };

                vertex.Color0 = new SlimDX.Color3()
                {
                    //Alpha = ((_srModel.Colours[vertex.colourID] & 0xFF000000) >> 24) / 255.0f,
                    Red = ((_srModel.Colours[exVertex.colourID] & 0x00FF0000) >> 16) / 255.0f,
                    Green = ((_srModel.Colours[exVertex.colourID] & 0x0000FF00) >> 8) / 255.0f,
                    Blue = ((_srModel.Colours[exVertex.colourID] & 0x000000FF) >> 0) / 255.0f
                };

                vertex.Color1 = new SlimDX.Color3()
                {
                    //Alpha = ((_srModel.ColoursAlt[vertex.colourID] & 0xFF000000) >> 24) / 255.0f,
                    Red = ((_srModel.ColoursAlt[exVertex.colourID] & 0x00FF0000) >> 16) / 255.0f,
                    Green = ((_srModel.ColoursAlt[exVertex.colourID] & 0x0000FF00) >> 8) / 255.0f,
                    Blue = ((_srModel.ColoursAlt[exVertex.colourID] & 0x000000FF) >> 0) / 255.0f
                };

                vertex.TextureCoordinates = new SlimDX.Vector2()
                {
                    X = _srModel.UVs[exVertex.UVID].u,
                    Y = _srModel.UVs[exVertex.UVID].v
                };
            }

            public int IndexCount { get { return _vertexList.Count; } }

            public void FillIndex(int i, out short index)
            {
                index = (short)_indexList[i];
            }
        }

        private static long progressLevels = 0;
        private static long progressLevel = 0;

        public static int ProgressPercent
        {
            get
            {
                if (progressLevels <= 0 || progressLevel <= 0)
                {
                    return 0;
                }

                return (int)((100 * progressLevel) / progressLevels);
            }
        }

        public static string ProgressStage { get; private set; } = "Done";

        CDC.Game _game = CDC.Game.SR1;
        List<SRFile> _objectFiles = new List<SRFile>();

        public SceneCDC(CDC.Game game)
            : base()
        {
            _game = game;
        }

        protected static String GetTextureName(SRModel srModel, int materialIndex)
        {
            CDC.Material material = srModel.Materials[materialIndex];
            String textureName = "";
            if (material.textureUsed)
            {
                if (srModel is SR1Model)
                {
                    if (srModel.Platform == CDC.Platform.PSX)
                    {
                        textureName =
                            srModel.Name.TrimEnd(new char[] { '_' }).ToLower() + "-" +
                            material.textureID.ToString("0000");
                    }
                    else
                    {
                        textureName =
                            "Texture-" +
                            material.textureID.ToString("00000");
                    }
                }
                else if (srModel is SR2Model ||
                    srModel is DefianceModel)
                {
                    textureName =
                        srModel.Name.TrimEnd(new char[] { '_' }).ToLower() + "-" +
                        material.textureID.ToString("0000");
                }
            }

            return textureName;
        }

        public override void ImportFromFile(string fileName)
        {
            progressLevel = 0;
            progressLevels = 0;
            ProgressStage = "Reading file";

            SRFile srFile = null;

            #region TryLoad
            if (_game == CDC.Game.SR1)
            {
                try
                {
                    srFile = new SR1File(fileName);
                }
                catch
                {
                    return;
                }
            }
            else if (_game == CDC.Game.SR2)
            {
                try
                {
                    srFile = new SR2File(fileName);
                }
                catch
                {
                    return;
                }
            }
            else
            {
                try
                {
                    srFile = new DefianceFile(fileName);
                }
                catch
                {
                    return;
                }
            }
            #endregion

            #region Progress Levels
            for (int modelIndex = 0; modelIndex < srFile.Models.Length; modelIndex++)
            {
                SRModel srModel = srFile.Models[modelIndex];

                for (int materialIndex = 0; materialIndex < srModel.MaterialCount; materialIndex++)
                {
                    progressLevels += 1;
                }

                for (int groupIndex = 0; groupIndex < srModel.Groups.Length; groupIndex++)
                {
                    if (srModel.Groups[groupIndex] != null && srModel.Groups[groupIndex].mesh != null)
                    {
                        for (int materialIndex = 0; materialIndex < srModel.MaterialCount; materialIndex++)
                        {
                            int vertexCount = (int)srModel.Groups[groupIndex].mesh.indexCount;
                            progressLevels += 1;
                        }
                    }
                }
            }
            #endregion

            String objectName = System.IO.Path.GetFileNameWithoutExtension(fileName);

            for (int modelIndex = 0; modelIndex < srFile.Models.Length; modelIndex++)
            {
                SRModelParser modelParser = new SRModelParser(objectName, srFile);
                modelParser.BuildModel(modelIndex);
                AddRenderObject(modelParser.Model);
            }

            _objectFiles.Add(srFile);

            ProgressStage = "Loading Textures";
            Thread.Sleep(1000);

            #region Textures
            if (srFile.GetType() == typeof(SR2File) ||
                srFile.GetType() == typeof(DefianceFile))
            {
                String textureFileName = System.IO.Path.ChangeExtension(fileName, "vrm");
                try
                {
                    SR2PCTextureFile textureFile = new SR2PCTextureFile(textureFileName);
                    for (int t = 0; t < textureFile.TextureCount; t++)
                    {
                        String textureName =
                            objectName.TrimEnd(new char[] { '_' }).ToLower() + "-" +
                            textureFile.TextureDefinitions[t].Flags1.ToString("0000") + ".dds";

                        System.IO.MemoryStream stream = textureFile.GetDataAsStream(t);
                        //textureFile.ExportFile(t, "C:\\Users\\A\\Desktop\\" + textureName);
                        if (stream != null)
                        {
                            TextureManager.Instance.AddTexture(stream, textureName);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.Write(ex.ToString());
                }
            }
            else
            {
                if (srFile.Platform == CDC.Platform.PC)
                {
                    String textureFileName = System.IO.Path.GetDirectoryName(fileName) + "\\textures.big";
                    try
                    {
                        SR1PCTextureFile textureFile = new SR1PCTextureFile(textureFileName);
                        foreach (SRModel srModel in srFile.Models)
                        {
                            foreach (CDC.Material material in srModel.Materials)
                            {
                                if (material.textureUsed)
                                {
                                    System.IO.MemoryStream stream = textureFile.GetDataAsStream(material.textureID);
                                    if (stream != null)
                                    {
                                        String textureName =
                                            "Texture-" + material.textureID.ToString("00000") + ".dds";
                                        TextureManager.Instance.AddTexture(stream, textureName);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.Write(ex.ToString());
                    }
                }
                else if (srFile.Platform == CDC.Platform.Dreamcast)
                {
                    String textureFileName = System.IO.Path.GetDirectoryName(fileName) + "\\textures.vq";
                    try
                    {
                        SR1DCTextureFile textureFile = new SR1DCTextureFile(textureFileName);
                        foreach (SRModel srModel in srFile.Models)
                        {
                            foreach (CDC.Material material in srModel.Materials)
                            {
                                if (material.textureUsed)
                                {
                                    int textureID = material.textureID;
                                    System.IO.MemoryStream stream = textureFile.GetDataAsStream(textureID);
                                    if (stream != null)
                                    {
                                        String textureName =
                                            "Texture-" + material.textureID.ToString("00000") + ".dds";
                                        TextureManager.Instance.AddTexture(stream, textureName);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.Write(ex.ToString());
                    }
                }
                else
                {
                    String textureFileName = System.IO.Path.ChangeExtension(fileName, "crm");
                    try
                    {
                        SR1PSTextureFile textureFile = new SR1PSTextureFile(textureFileName);

                        UInt32 polygonCountAllModels = 0;
                        foreach (SRModel srModel in srFile.Models)
                        {
                            polygonCountAllModels += srModel.PolygonCount;
                        }

                        SR1PSTextureFile.SoulReaverPlaystationPolygonTextureData[] polygons =
                            new SR1PSTextureFile.SoulReaverPlaystationPolygonTextureData[polygonCountAllModels];

                        int polygonNum = 0;
                        foreach (SRModel srModel in srFile.Models)
                        {
                            foreach (CDC.Polygon polygon in srModel.Polygons)
                            {
                                polygons[polygonNum].paletteColumn = polygon.paletteColumn;
                                polygons[polygonNum].paletteRow = polygon.paletteRow;
                                polygons[polygonNum].u = new int[3];
                                polygons[polygonNum].v = new int[3];

                                polygons[polygonNum].u[0] = (int)(srModel.UVs[polygon.v1.UVID].u * 255);
                                polygons[polygonNum].v[0] = (int)(srModel.UVs[polygon.v1.UVID].v * 255);
                                polygons[polygonNum].u[1] = (int)(srModel.UVs[polygon.v2.UVID].u * 255);
                                polygons[polygonNum].v[1] = (int)(srModel.UVs[polygon.v2.UVID].v * 255);
                                polygons[polygonNum].u[2] = (int)(srModel.UVs[polygon.v3.UVID].u * 255);
                                polygons[polygonNum].v[2] = (int)(srModel.UVs[polygon.v3.UVID].v * 255);

                                polygons[polygonNum].textureID = polygon.material.textureID;

                                polygonNum++;
                            }
                        }
                        textureFile.BuildTexturesFromPolygonData(polygons, false, true);

                        for (int t = 0; t < textureFile.TextureCount; t++)
                        {
                            String textureName =
                                objectName.TrimEnd(new char[] { '_' }).ToLower() + "-" + t.ToString("0000") + ".dds";

                            System.IO.MemoryStream stream = textureFile.GetDataAsStream(t);
                            if (stream != null)
                            {
                                TextureManager.Instance.AddTexture(stream, textureName);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.Write(ex.ToString());
                    }
                }
            }
            #endregion

            progressLevel = progressLevels;
            ProgressStage = "Done";
            Thread.Sleep(1000);
        }

        public override void ExportToFile(string fileName)
        {
            if (_objectFiles[0] != null)
            {
                _objectFiles[0].ExportToFile(fileName);
            }
        }
    }
}
