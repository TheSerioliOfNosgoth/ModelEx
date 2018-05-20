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
        class SRObjectParser :
            IMeshParser<PositionNormalTexturedVertex, short>,
            IMeshParser<PositionColorTexturedVertex, short>
        {
            SRModel _srModel;
            ExTree _exGroup;
            List<int> _vertexList = new List<int>();
            List<int> _indexList = new List<int>();
            public List<SubMesh> SubMeshes { get; } = new List<SubMesh>();
            public Mesh Mesh;

            public SRObjectParser(SRModel srModel, int groupIndex)
            {
                _srModel = srModel;
                _exGroup = srModel.Groups[groupIndex];
            }

            public void BuildMesh(bool isSR2File, bool isUnit)
            {
                int startIndexLocation = 0;

                for (int materialIndex = 0; materialIndex < _srModel.MaterialCount; materialIndex++)
                {
                    int indexCount = 0;

                    int totalIndexCount = (int)_exGroup.m_xMesh.m_uIndexCount;
                    for (int v = 0; v < totalIndexCount; v++)
                    {
                        if (_exGroup.m_xMesh.m_axPolygons[v / 3].material.ID == materialIndex)
                        {
                            _vertexList.Add(v);
                            _indexList.Add(_indexList.Count - startIndexLocation);
                            indexCount++;
                        }
                    }

                    if (indexCount > 0)
                    {
                        SubMesh subMesh = new SubMesh();
                        subMesh.startIndexLocation = startIndexLocation;
                        subMesh.indexCount = indexCount;
                        subMesh.baseVertexLocation = startIndexLocation;
                        subMesh.MaterialIndex = materialIndex;
                        SubMeshes.Add(subMesh);

                        startIndexLocation += indexCount;
                    }
                }

                if (SubMeshes.Count > 0)
                {
                    if (isUnit)
                    {
                        Mesh = new MeshPCT(this, isSR2File ? "SR2Render" : "SR1Render");
                    }
                    else
                    {
                        Mesh = new MeshPNT(this, isSR2File ? "SR2Render" : "SR1Render");
                    }
                }
            }

            public int VertexCount { get { return _vertexList.Count; } }

            public void FillVertex(int v, out PositionNormalTexturedVertex vertex)
            {
                ref ExVertex exVertex = ref _exGroup.m_xMesh.m_axVertices[_vertexList[v]];

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
                ref ExVertex exVertex = ref _exGroup.m_xMesh.m_axVertices[_vertexList[v]];

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

            String objectName = System.IO.Path.GetFileNameWithoutExtension(fileName);

            Model model = new Model();
            model.Name = objectName;
            model.Root.Name = objectName;

            for (int modelIndex = 0; modelIndex < srFile.m_axModels.Length; modelIndex++)
            {
                SRModel srModel = srFile.m_axModels[modelIndex];
                String modelName = objectName + "-" + modelIndex.ToString();

                progressStage = "Model " + modelIndex.ToString() + " - Creating Materials";
                Thread.Sleep(500);

                for (int materialIndex = 0; materialIndex < srModel.MaterialCount; materialIndex++)
                {
                    Material material = new Material();
                    Color colorDiffuse = Color.FromArgb((int)unchecked(srModel.Materials[materialIndex].colour));
                    material.Diffuse = colorDiffuse;
                    material.TextureFileName = GetTextureName(srModel, materialIndex);
                    model.Materials.Add(material);

                    progressLevel += srModel.IndexCount / srModel.Groups.Length;
                }

                Node modelNode = new Node();
                modelNode.Name = modelName;

                for (int groupIndex = 0; groupIndex < srModel.Groups.Length; groupIndex++)
                {
                    progressStage = "Model " + modelIndex.ToString() + " - Creating Group " + groupIndex.ToString();
                    Thread.Sleep(100);

                    String groupName = modelName + "-group-" + groupIndex.ToString();
                    Node group = new Node();
                    group.Name = groupName;

                    if (srModel.Groups[groupIndex] != null && srModel.Groups[groupIndex].m_xMesh != null)
                    {
                        String meshName = groupName + "-mesh-" + groupIndex.ToString();

                        SRObjectParser parser = new SRObjectParser(srModel, groupIndex);
                        parser.BuildMesh(isSR2File, srFile.m_eFileType == FileType.Unit);
                        parser.Mesh.Name = meshName;
                        foreach (SubMesh subMesh in parser.SubMeshes)
                        {
                            String subMeshName = groupName + "-subMesh-" + subMesh.MaterialIndex.ToString();
                            subMesh.Name = meshName;
                            subMesh.MeshIndex = model.Meshes.Count;
                            group.SubMeshIndices.Add(model.SubMeshes.Count);
                            model.SubMeshes.Add(subMesh);
                        }
                        model.Meshes.Add(parser.Mesh);
                    }

                    modelNode.Nodes.Add(group);
                }

                model.Root.Nodes.Add(modelNode);
            }

            Scene.Instance.AddRenderObject(model);

            progressStage = "Loading Textures";
            System.Threading.Thread.Sleep(1000);

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
