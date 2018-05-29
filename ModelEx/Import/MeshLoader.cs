using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using CDTextureFile = BenLincoln.TheLostWorlds.CDTextures.TextureFile;
using SR1PCTextureFile = BenLincoln.TheLostWorlds.CDTextures.SoulReaverPCTextureFile;
using SR1PSTextureFile = BenLincoln.TheLostWorlds.CDTextures.SoulReaverPlaystationTextureFile;
using SR1DCTextureFile = BenLincoln.TheLostWorlds.CDTextures.SoulReaverDreamcastTextureFile;
using SR2PCTextureFile = BenLincoln.TheLostWorlds.CDTextures.SoulReaver2PCVRMTextureFile;

namespace ModelEx
{
    public class MeshLoader
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
                _srModel = _srFile.m_axModels[modelIndex];
                String modelName = _objectName + "-" + modelIndex.ToString();

                #region Materials
                progressStage = "Model " + modelIndex.ToString() + " - Creating Materials";
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
                    progressStage = "Model " + modelIndex.ToString() + " - Creating Group " + groupIndex.ToString();
                    Thread.Sleep(100);

                    ExTree srGroup = _srModel.Groups[groupIndex];
                    String groupName = String.Format("{0}-{1}-group-{2}", _objectName, modelIndex, groupIndex);
                    if (srGroup != null && srGroup.m_xMesh != null &&
                        srGroup.m_xMesh.m_uIndexCount > 0 && srGroup.m_xMesh.m_uPolygonCount > 0)
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

                if (_srFile.m_eAsset == Asset.Unit)
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
            IMeshParser<PositionColorTexturedVertex, short>
        {
            string _objectName;
            SRFile _srFile;
            SRModel _srModel;
            ExTree _srGroup;
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
                _srModel = _srFile.m_axModels[modelIndex];
                _srGroup = _srModel.Groups[groupIndex];
                String modelName = String.Format("{0}-{1}", _objectName, modelIndex);
                String groupName = String.Format("{0}-{1}-group-{2}", _objectName, modelIndex, groupIndex);
                String meshName = String.Format("{0}-{1}-group-{2}-mesh-{3}", _objectName, modelIndex, groupIndex, meshIndex);

                int startIndexLocation = 0;
                for (int materialIndex = 0; materialIndex < _srModel.MaterialCount; materialIndex++)
                {
                    int indexCount = 0;
                    int totalIndexCount = (int)_srGroup.m_xMesh.m_uIndexCount;
                    for (int v = 0; v < totalIndexCount; v++)
                    {
                        if (_srGroup.m_xMesh.m_axPolygons[v / 3].material.ID == materialIndex)
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
                    Technique = _srFile.m_eGame == Game.SR2 ? "SR2Render" : "SR1Render";
                    if (_srFile.m_eAsset == Asset.Unit)
                    {
                        Mesh = new MeshPCT(this);
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
                ref ExVertex exVertex = ref _srGroup.m_xMesh.m_axVertices[_vertexList[v]];

                vertex.Position = new SlimDX.Vector3()
                {
                    X = 0.01f * _srModel.Positions[exVertex.positionID].worldPos.x,
                    Y = 0.01f * _srModel.Positions[exVertex.positionID].worldPos.z,
                    Z = 0.01f * _srModel.Positions[exVertex.positionID].worldPos.y
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
                ref ExVertex exVertex = ref _srGroup.m_xMesh.m_axVertices[_vertexList[v]];

                vertex.Position = new SlimDX.Vector3()
                {
                    X = 0.01f * _srModel.Positions[exVertex.positionID].worldPos.x,
                    Y = 0.01f * _srModel.Positions[exVertex.positionID].worldPos.z,
                    Z = 0.01f * _srModel.Positions[exVertex.positionID].worldPos.y
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

            public int IndexCount { get { return _vertexList.Count; } }

            public void FillIndex(int i, out short index)
            {
                index = (short)_indexList[i];
            }
        }

        private static long progressLevels = 0;
        private static long progressLevel = 0;
        private static string progressStage = "Done";

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

        public static string ProgressStage
        {
            get { return progressStage; }
        }

        public static void LoadSoulReaverFile(String fileName, bool isSR2File)
        {
            progressLevel = 0;
            progressLevels = 0;
            progressStage = "Reading file";

            SRFile srFile = null;

            #region TryLoad
            if (!isSR2File)
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
            else
            {
                try
                {
                    srFile = new SR2File(fileName);
                    isSR2File = true;
                }
                catch
                {
                    return;
                }
            }
            #endregion

            #region Progress Levels
            for (int modelIndex = 0; modelIndex < srFile.m_axModels.Length; modelIndex++)
            {
                SRModel srModel = srFile.m_axModels[modelIndex];

                for (int materialIndex = 0; materialIndex < srModel.MaterialCount; materialIndex++)
                {
                    progressLevels += srModel.IndexCount / srModel.Groups.Length;
                }

                for (int groupIndex = 0; groupIndex < srModel.Groups.Length; groupIndex++)
                {
                    if (srModel.Groups[groupIndex] != null && srModel.Groups[groupIndex].m_xMesh != null)
                    {
                        for (int materialIndex = 0; materialIndex < srModel.MaterialCount; materialIndex++)
                        {
                            int vertexCount = (int)srModel.Groups[groupIndex].m_xMesh.m_uIndexCount;
                            progressLevels += (vertexCount * 3);
                        }
                    }
                }
            }
            #endregion

            String objectName = System.IO.Path.GetFileNameWithoutExtension(fileName);

            for (int modelIndex = 0; modelIndex < srFile.m_axModels.Length; modelIndex++)
            {
                SRModelParser modelParser = new SRModelParser(objectName, srFile);
                modelParser.BuildModel(modelIndex);
                Scene.Instance.AddRenderObject(modelParser.Model);
            }

            progressStage = "Loading Textures";
            System.Threading.Thread.Sleep(1000);

            #region Textures
            if (srFile.GetType() == typeof(SR2File))
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
                if (srFile.m_ePlatform == Platform.PC)
                {
                    String textureFileName = System.IO.Path.GetDirectoryName(fileName) + "\\textures.big";
                    try
                    {
                        SR1PCTextureFile textureFile = new SR1PCTextureFile(textureFileName);
                        foreach (SRModel srModel in srFile.m_axModels)
                        {
                            foreach (ExMaterial material in srModel.Materials)
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
                else if (srFile.m_ePlatform == Platform.Dreamcast)
                {
                    String textureFileName = System.IO.Path.GetDirectoryName(fileName) + "\\textures.vq";
                    try
                    {
                        SR1DCTextureFile textureFile = new SR1DCTextureFile(textureFileName);
                        foreach (SRModel srModel in srFile.m_axModels)
                        {
                            foreach (ExMaterial material in srModel.Materials)
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
                        foreach (SRModel srModel in srFile.m_axModels)
                        {
                            polygonCountAllModels += srModel.PolygonCount;
                        }

                        SR1PSTextureFile.SoulReaverPlaystationPolygonTextureData[] polygons =
                            new SR1PSTextureFile.SoulReaverPlaystationPolygonTextureData[polygonCountAllModels];

                        int polygonNum = 0;
                        foreach (SRModel srModel in srFile.m_axModels)
                        {
                            foreach (ExPolygon polygon in srModel.Polygons)
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
            progressStage = "Done";
            Thread.Sleep(1000);
        }

        static String GetTextureName(SRModel srModel, int materialIndex)
        {
            ExMaterial material = srModel.Materials[materialIndex];
            String textureName = "";
            if (material.textureUsed)
            {
                if (srModel is SR1Model)
                {
                    if (srModel.Platform == Platform.PSX)
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
                else if (srModel is SR2Model)
                {
                    textureName =
                        srModel.Name.TrimEnd(new char[] { '_' }).ToLower() + "-" +
                        material.textureID.ToString("0000");
                }
            }

            return textureName;
        }
    }
}
