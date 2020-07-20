using System;
using System.Collections.Generic;
using System.IO;
using CDC.Objects.Models;

namespace CDC.Objects
{
    public enum RenderMode
    {
        Standard,
        NoTextures,
        Wireframe,  // not implemented
        PointCloud, // not implemented
        DebugPolygonFlags1,
        DebugPolygonFlags2,
        DebugPolygonFlags3,
        DebugPolygonFlagsSoulReaverA,
        DebugPolygonFlagsHash,
        DebugTextureAttributes1,
        DebugTextureAttributes2,
        DebugTextureAttributes3,
        DebugTextureAttributes4,
        DebugTextureAttributes5,
        DebugTextureAttributes6,
        DebugTextureAttributesHash,
        DebugTextureAttributesA1,
        DebugTextureAttributesA2,
        DebugTextureAttributesA3,
        DebugTextureAttributesA4,
        DebugTextureAttributesA5,
        DebugTextureAttributesA6,
        DebugTextureAttributesAHash,
        DebugCLUT1,
        DebugCLUT2,
        DebugCLUT3,
        DebugCLUT4,
        DebugCLUT5,
        DebugCLUT6,
        DebugCLUTHash,
        DebugCLUTNonRowColBits1,
        DebugCLUTNonRowColBits2,
        DebugCLUTNonRowColBitsHash,
        DebugTexturePage1,
        DebugTexturePage2,
        DebugTexturePage3,
        DebugTexturePage4,
        DebugTexturePage5,
        DebugTexturePage6,
        DebugTexturePageHash,
        DebugTexturePageUpper28BitsHash,
        DebugTexturePageUpper5BitsHash,
        DebugBSPRootTreeNumber,
        DebugBSPTreeNodeID,
        DebugBSPRootTreeFlags1,
        DebugBSPRootTreeFlags2,
        DebugBSPRootTreeFlags3,
        DebugBSPRootTreeFlags4,
        DebugBSPRootTreeFlags5,
        DebugBSPRootTreeFlags6,
        DebugBSPRootTreeFlagsHash,
        DebugBSPTreeImmediateParentFlags1,
        DebugBSPTreeImmediateParentFlags2,
        DebugBSPTreeImmediateParentFlags3,
        DebugBSPTreeImmediateParentFlags4,
        DebugBSPTreeImmediateParentFlags5,
        DebugBSPTreeImmediateParentFlags6,
        DebugBSPTreeImmediateParentFlagsHash,
        DebugBSPTreeAllParentFlagsORd1,
        DebugBSPTreeAllParentFlagsORd2,
        DebugBSPTreeAllParentFlagsORd3,
        DebugBSPTreeAllParentFlagsORd4,
        DebugBSPTreeAllParentFlagsORd5,
        DebugBSPTreeAllParentFlagsORd6,
        DebugBSPTreeAllParentFlagsORdHash,
        DebugBSPTreeLeafFlags1,
        DebugBSPTreeLeafFlags2,
        DebugBSPTreeLeafFlags3,
        DebugBSPTreeLeafFlags4,
        DebugBSPTreeLeafFlags5,
        DebugBSPTreeLeafFlags6,
        DebugBSPTreeLeafFlagsHash,
        DebugBoneIDHash,
        DebugSortPushHash,
        DebugSortPushFlags1,
        DebugSortPushFlags2,
        DebugSortPushFlags3,
        AverageVertexAlpha,
        PolygonAlpha,
        PolygonOpacity
    }

    public class ExportOptions
    {
        public bool DiscardPortalPolygons;
        public bool DiscardNonVisible;
        public bool DiscardMeshesWithNoNonZeroFlags;
        public bool ExportSpectral;
        public bool UnhideCompletelyTransparentTextures;
        public bool AlwaysUseGreyscaleForMissingPalettes;
        public bool ExportDoubleSidedMaterials;
        public bool MakeAllPolygonsVisible;
        public bool MakeAllPolygonsOpaque;
        public bool SetAllPolygonColoursToValue;
        public float PolygonColourAlpha;
        public float PolygonColourRed;
        public float PolygonColourGreen;
        public float PolygonColourBlue;
        public bool BSPRenderingIncludeRootTreeFlagsWhenORing;
        public bool BSPRenderingIncludeLeafFlagsWhenORing;
        public bool InterpolatePolygonColoursWhenColouringBasedOnVertices;
        public bool UseEachUniqueTextureCLUTVariation;
        public bool AlsoInferAlphaMaskingFromTexturePixels;
        public bool IgnorePolygonFlag2ForTerrain;
        public bool DistinctMaterialsForAllFlags;
        public bool AdjustUVs;
        public bool IgnoreVertexColours;
        public List<string> TextureFileLocations;
        public RenderMode RenderMode;
        public Platform ForcedPlatform;

        public ExportOptions()
        {
            DiscardPortalPolygons = false;
            DiscardNonVisible = false;
            DiscardMeshesWithNoNonZeroFlags = false;
            ExportSpectral = false;
            UnhideCompletelyTransparentTextures = false;
            AlwaysUseGreyscaleForMissingPalettes = false;
            ExportDoubleSidedMaterials = false;
            MakeAllPolygonsVisible = false;
            MakeAllPolygonsOpaque = false;
            SetAllPolygonColoursToValue = false;
            PolygonColourAlpha = 0.7f;
            PolygonColourRed = 0.0f;
            PolygonColourGreen = 1.0f;
            PolygonColourBlue = 0.0f;
            BSPRenderingIncludeRootTreeFlagsWhenORing = false;
            BSPRenderingIncludeLeafFlagsWhenORing = false;
            InterpolatePolygonColoursWhenColouringBasedOnVertices = false;
            UseEachUniqueTextureCLUTVariation = false;
            AlsoInferAlphaMaskingFromTexturePixels = false;
            IgnorePolygonFlag2ForTerrain = false;
            DistinctMaterialsForAllFlags = true;
            AdjustUVs = false;
            IgnoreVertexColours = false;
            TextureFileLocations = new List<string>();
            RenderMode = RenderMode.Standard;
            ForcedPlatform = Platform.None;
        }

        public bool TextureLoadRequired()
        {
            bool result = true;

            if (RenderMode != RenderMode.Standard)
            {
                result = false;
            }

            return result;
        }
    }

    public abstract class SRFile
    {
        public const string TextureExtension = ".png";
        public const float ExportSizeMultiplier = 1.0f;

        protected String _name;
        protected UInt32 _version;
        protected UInt32 _dataStart;
        protected UInt16 _modelCount;
        protected UInt16 _animCount;
        protected UInt32 _modelStart;
        protected SRModel[] _models;
        protected UInt32 _animStart;
        protected UInt32 _instanceCount;
        protected UInt32 _instanceStart;
        protected String[] _instanceNames;
        protected UInt32 _instanceTypeStart;
        protected String[] _instanceTypeNames;
        protected UInt32 portalCount;
        protected UInt32 _connectedUnitStart;
        protected String[] _portalNames;
        protected Game _game;
        protected Asset _asset;
        protected Platform _platform;

        public String Name { get { return _name; } }
        public UInt32 Version { get { return _version; } }
        public UInt16 ModelCount { get { return _modelCount; } }
        public UInt16 AnimCount { get { return _animCount; } }
        public SRModel[] Models { get { return _models; } }
        public UInt32 InstanceCount { get { return _instanceCount; } }
        public String[] Instances { get { return _instanceNames; } }
        public String[] InstanceTypeNames { get { return _instanceTypeNames; } }
        public UInt32 ConectedUnitCount { get { return portalCount; } }
        public String[] ConnectedUnit { get { return _portalNames; } }
        public Game Game { get { return _game; } }
        public Asset Asset { get { return _asset; } }
        public Platform Platform { get { return _platform; } }

        public static StreamWriter m_xLogFile = null;

        protected SRFile(String strFileName, Game game, CDC.Objects.ExportOptions options)
        {
            _name = Path.GetFileNameWithoutExtension(strFileName);
            _game = game;

            FileStream xFile = new FileStream(strFileName, FileMode.Open, FileAccess.Read);
            BinaryReader xReader = new BinaryReader(xFile, System.Text.Encoding.ASCII);
            MemoryStream xStream = new MemoryStream((int)xFile.Length);
            BinaryWriter xWriter = new BinaryWriter(xStream, System.Text.Encoding.ASCII);

            //String strDebugFileName = Path.GetDirectoryName(strFileName) + "\\" + Path.GetFileNameWithoutExtension(strFileName) + "-Debug.txt";
            //m_xLogFile = File.CreateText(strDebugFileName);

            ResolvePointers(xReader, xWriter);
            xReader.Close();
            xReader = new BinaryReader(xStream, System.Text.Encoding.ASCII);

            ReadHeaderData(xReader, options);

            if (_asset == Asset.Object)
            {
                ReadObjectData(xReader, options);
            }
            else
            {
                ReadUnitData(xReader, options);
            }

            xReader.Close();

            if (m_xLogFile != null)
            {
                m_xLogFile.Close();
                m_xLogFile = null;
            }
        }

        protected abstract void ReadHeaderData(BinaryReader xReader, CDC.Objects.ExportOptions options);

        protected abstract void ReadObjectData(BinaryReader xReader, CDC.Objects.ExportOptions options);

        protected abstract void ReadUnitData(BinaryReader xReader, CDC.Objects.ExportOptions options);

        protected abstract void ResolvePointers(BinaryReader xReader, BinaryWriter xWriter);

        public bool ExportToFile(String fileName, ExportOptions options)
        {
            string name = Utility.CleanName(Name).TrimEnd(new char[] { '_' });

            //Assimp.Node rootNode = new Assimp.Node(name);
            //List<Assimp.Material> materials = new List<Assimp.Material>();
            //List<Assimp.Mesh> meshes = new List<Assimp.Mesh>();
            string perModelFilename = fileName;

            for (int modelIndex = 0; modelIndex < ModelCount; modelIndex++)
            {
                Assimp.Node rootNode = new Assimp.Node(name);
                List<Assimp.Material> materials = new List<Assimp.Material>();
                List<Assimp.Mesh> meshes = new List<Assimp.Mesh>();

                SRModel model = Models[modelIndex];

                string modelName = name + "-" + modelIndex;
                Console.WriteLine(string.Format("Debug: exporting model {0} / {1} ('{2}')", modelIndex, (ModelCount - 1), modelName));
                perModelFilename = fileName;
                if (ModelCount > 1)
                {
                    string extension = Path.GetExtension(fileName);
                    if ((extension.StartsWith(".")) && (extension.Length > 1))
                    {
                        extension = extension.Substring(1);
                    }
                    perModelFilename = string.Format("{0}{1}{2}-model_{3:D4}.{4}", Path.GetDirectoryName(fileName), Path.DirectorySeparatorChar, Path.GetFileNameWithoutExtension(fileName), modelIndex, extension);
                }

                Assimp.Node modelNode = new Assimp.Node(modelName);

                for (int groupIndex = 0; groupIndex < model.GroupCount; groupIndex++)
                {
                    Tree group = model.Groups[groupIndex];
                    if (group == null)
                    {
                        continue;
                    }
                    //if (options.DiscardPortalPolygons && (groupIndex == (model.GroupCount - 1)))
                    //{
                    //    continue;
                    //}

                    string groupName = name + "-" + modelIndex + "-" + groupIndex;

                    Console.WriteLine(string.Format("\tDebug: exporting group {0} / {1} ('{2}'), mesh flags {3}", groupIndex, (model.GroupCount - 1), groupName, Convert.ToString(group.mesh.sr1BSPTreeFlags, 2).PadLeft(8, '0')));
                    if (group.mesh.sr1BSPNodeFlags.Count > 0)
                    {
                        //Console.WriteLine(string.Format("\t\t\tDebug: BSP node flags for this mesh:"));
                        for (int flagNum = 0; flagNum < group.mesh.sr1BSPNodeFlags.Count; flagNum++)
                        {
                            //Console.WriteLine(string.Format("\t\t\t\tBSP node flags {0}", Convert.ToString(group.mesh.sr1BSPNodeFlags[flagNum], 2).PadLeft(8, '0')));
                        }
                    }
                    else
                    {
                        // Console.WriteLine(string.Format("\t\t\tDebug: No BSP node flags for this mesh"));
                    }
                    if (group.mesh.sr1BSPLeafFlags.Count > 0)
                    {
                        //Console.WriteLine(string.Format("\t\t\tDebug: BSP leaf flags for this mesh:"));
                        for (int flagNum = 0; flagNum < group.mesh.sr1BSPLeafFlags.Count; flagNum++)
                        {
                            //Console.WriteLine(string.Format("\t\t\t\tBSP leaf flags {0}", Convert.ToString(group.mesh.sr1BSPLeafFlags[flagNum], 2).PadLeft(8, '0')));
                        }
                    }
                    else
                    {
                        //Console.WriteLine(string.Format("\t\t\tDebug: No BSP leaf flags for this mesh"));
                    }

                    Assimp.Node groupNode = new Assimp.Node(groupName);

                    for (int materialIndex = 0; materialIndex < model.MaterialCount; materialIndex++)
                    {
                        Console.WriteLine(string.Format("\t\tDebug: exporting material {0} / {1}", materialIndex, (model.MaterialCount - 1)));
                        int totalPolygonCount = (int)group.mesh.polygonCount;
                        List<int> polygonList = new List<int>();

                        for (int p = 0; p < totalPolygonCount; p++)
                        {
                            if (group.mesh.polygons[p].material.ID == materialIndex)
                            {
                                polygonList.Add(p);
                            }
                        }

                        int polygonCount = polygonList.Count;
                        if (polygonCount > 0)
                        {
                            #region Mesh
                            string meshName = name + "-" + modelIndex + "-" + groupIndex + "-" + materialIndex;
                            //string materialNamePrefix = string.Format("{0}-{1}", name, materialIndex);
                            string materialNamePrefix = meshName;
                            Assimp.Mesh mesh = new Assimp.Mesh(meshName);
                            bool addMesh = true;
                            mesh.PrimitiveType = Assimp.PrimitiveType.Triangle;

                            ref Polygon[] polygons = ref group.mesh.polygons;
                            int i = 0;
                            for (int p = 0; p < polygonCount; p++)
                            {
                                ref Polygon polygon = ref polygons[polygonList[p]];

                                Vertex[] vertices = { polygon.v1, polygon.v2, polygon.v3 };

                                for (int v = 0; v < vertices.Length; v++)
                                {
                                    ref Vertex vert = ref vertices[v];
                                    Geometry geometry = vert.isExtraGeometry ? model.ExtraGeometry : model.Geometry;

                                    ref Vector[] positions = ref geometry.PositionsPhys;
                                    ref Vector[] normals = ref geometry.Normals;
                                    ref UInt32[] colors = ref geometry.Colours;
                                    ref UV[] uvs = ref geometry.UVs;

                                    ref Vector pos = ref positions[vert.positionID];
                                    //mesh.Vertices.Add(new Assimp.Vector3D(pos.x, pos.y, pos.z));
                                    mesh.Vertices.Add(new Assimp.Vector3D((float)pos.x * ExportSizeMultiplier, (float)pos.y * ExportSizeMultiplier, (float)pos.z * ExportSizeMultiplier));

                                    if (Asset == Asset.Object)
                                    {
                                        ref Vector norm = ref normals[vert.normalID];
                                        mesh.Normals.Add(new Assimp.Vector3D(norm.x, norm.y, norm.z));
                                    }
                                    else
                                    {
                                        Assimp.Color4D col = GetAssimpColorOpaque(colors[vert.colourID]);
                                        mesh.VertexColorChannels[0].Add(col);
                                    }

                                    Assimp.Vector3D uv = GetAssimpUV(uvs[vert.UVID]);
                                    mesh.TextureCoordinateChannels[0].Add(uv);
                                }

                                mesh.Faces.Add(new Assimp.Face(new int[] { i++, i++, i++ }));
                            }

                            //string materialName = name + "-" + modelIndex + "-" + groupIndex + "-" + materialIndex;
                            //Assimp.Material material = GetAssimpMaterial(materialName, model, materialIndex, options);

                            if (addMesh)
                            {
                                Assimp.Material material = GetAssimpMaterial(materialNamePrefix, model, materialIndex, options);

                                mesh.MaterialIndex = materials.Count;
                                materials.Add(material);

                                groupNode.MeshIndices.Add(meshes.Count);
                                meshes.Add(mesh);
                            }
                            #endregion
                        }
                    }

                    modelNode.Children.Add(groupNode);
                }

                rootNode.Children.Add(modelNode);

                Assimp.Scene scene = new Assimp.Scene();
                scene.RootNode = rootNode;
                scene.Materials.AddRange(materials);
                scene.Meshes.AddRange(meshes);

                Assimp.AssimpContext context = new Assimp.AssimpContext();
                context.ExportFile(scene, perModelFilename, "collada", Assimp.PostProcessSteps.None);
            }

            //Assimp.Scene scene = new Assimp.Scene();
            //scene.RootNode = rootNode;
            //scene.Materials.AddRange(materials);
            //scene.Meshes.AddRange(meshes);

            //Assimp.AssimpContext context = new Assimp.AssimpContext();
            //context.ExportFile(scene, fileName, "collada", Assimp.PostProcessSteps.None);

            return true;
        }

        public static float[] UInt32ARGBToFloatARGB(UInt32 argb)
        {
            float[] result = new float[4];

            result[0] = (float)((argb & 0xFF000000) >> 24) / 255.0f;
            result[1] = (float)((argb & 0x00FF0000) >> 16) / 255.0f;
            result[2] = (float)((argb & 0x0000FF00) >> 8) / 255.0f;
            result[3] = (float)((argb & 0x000000FF)) / 255.0f;

            return result;
        }

        public static UInt32 FloatARGBToUInt32ARGB(float[] argb)
        {
            UInt32 result;

            result = ((uint)(Math.Round(argb[0] * 255.0f))) << 24;
            result |= ((uint)(Math.Round(argb[1] * 255.0f))) << 16;
            result |= ((uint)(Math.Round(argb[2] * 255.0f))) << 8;
            result |= ((uint)(Math.Round(argb[3] * 255.0f)));

            return result;
        }

        protected Assimp.Color4D GetAssimpColor(UInt32 color)
        {
            Assimp.Color4D color4D = new Assimp.Color4D()
            {
                A = ((color & 0xFF000000) >> 24) / 255.0f,
                R = ((color & 0x00FF0000) >> 16) / 255.0f,
                G = ((color & 0x0000FF00) >> 8) / 255.0f,
                B = ((color & 0x000000FF) >> 0) / 255.0f
            };
            
            return color4D;
        }

        protected Assimp.Color4D GetAssimpColorOpaque(UInt32 color)
        {
            Assimp.Color4D color4D = new Assimp.Color4D()
            {
                A = 1.0f, // ((A8R8G8B8 & 0x00FF0000) >> 24) / 255.0f,
                R = ((color & 0x00FF0000) >> 16) / 255.0f,
                G = ((color & 0x0000FF00) >> 8) / 255.0f,
                B = ((color & 0x000000FF) >> 0) / 255.0f
            };

            return color4D;
        }

        protected Assimp.Vector3D GetAssimpUV(UV uv)
        {
            if (_platform == Platform.Dreamcast)
            {
                Assimp.Vector3D uv3DDreamcast = new Assimp.Vector3D()
                {
                    X = uv.v,
                    Y = 1.0f - uv.u,
                    Z = 0.0f
                };
                return uv3DDreamcast;
            }
            Assimp.Vector3D uv3D = new Assimp.Vector3D()
            {
                X = uv.u,
                Y = 1.0f - uv.v,
                Z = 0.0f
            };

            return uv3D;
        }

        protected Assimp.Material GetAssimpMaterial(String name, SRModel model, int materialIndex, CDC.Objects.ExportOptions options)
        {
            Assimp.Material material = new Assimp.Material();

            // Bugged.
            //material.Name = name;
            string namePropName = /*Assimp.Unmanaged.AiMatKeys.NAME_BASE*/ "?mat.name";
            material.AddProperty(new Assimp.MaterialProperty(namePropName, name));

            if (model.Materials[materialIndex].textureUsed)
            {
                Assimp.TextureSlot textureDiffuse = new Assimp.TextureSlot();
                textureDiffuse.BlendFactor = 1.0f;
                textureDiffuse.FilePath = model.GetTextureName(materialIndex, options) + TextureExtension;
                textureDiffuse.TextureIndex = 0;
                textureDiffuse.TextureType = Assimp.TextureType.Diffuse;
                textureDiffuse.WrapModeU = Assimp.TextureWrapMode.Clamp;
                textureDiffuse.WrapModeV = Assimp.TextureWrapMode.Clamp;

                // Bugged.
                //material.TextureDiffuse = textureDiffuse;
                // Works, but looks like there might be a better way.
                //string texturePropName = Assimp.Unmanaged.AiMatKeys.TEXTURE_BASE;
                //Assimp.MaterialProperty textureProp = new Assimp.MaterialProperty(texturePropName, model.GetTextureName(materialIndex) + SceneCDC.TextureExtension, Assimp.TextureType.Diffuse, 0);
                //material.AddProperty(textureProp);
                material.AddMaterialTexture(ref textureDiffuse);

                //bool bHasTextureDiffuse = material.HasProperty("$tex.file,1,0,0,0");
                //bool bHasColourDiffuse = material.HasProperty("$clr.diffuse,0,0,0,0");
            }
            else
            {
                // Should I still use this when there's a texture? Texture colours would be multiplied by this colour.
                Assimp.Color4D colorDiffuse = GetAssimpColor(model.Materials[materialIndex].colour);

                // Bugged.
                //material.ColorDiffuse = colorDiffuse;
                string diffusePropName = /*Assimp.Unmanaged.AiMatKeys.COLOR_DIFFUSE_BASE*/ "$clr.diffuse";
                Assimp.MaterialProperty diffuseProp = new Assimp.MaterialProperty(diffusePropName, colorDiffuse);
                material.AddProperty(diffuseProp);
            }

            if (Asset == Asset.Object)
            {
                // Bugged.
                //material.ShadingMode = ShadingMode.Phong;
                string shadingPropName = /*Assimp.Unmanaged.AiMatKeys.SHADING_MODEL_BASE*/"$mat.shadingm";
                Assimp.MaterialProperty shadingProp = new Assimp.MaterialProperty(shadingPropName, (int)Assimp.ShadingMode.Phong);
                shadingProp.SetIntegerValue((int)Assimp.ShadingMode.Phong);
                material.AddProperty(shadingProp);
            }
            else
            {
                // Bugged.
                //material.ShadingMode = ShadingMode.Gouraud;
                string shadingPropName = /*Assimp.Unmanaged.AiMatKeys.SHADING_MODEL_BASE*/"$mat.shadingm";
                Assimp.MaterialProperty shadingProp = new Assimp.MaterialProperty(shadingPropName, (int)Assimp.ShadingMode.Gouraud);
                shadingProp.SetIntegerValue((int)Assimp.ShadingMode.Gouraud);
                material.AddProperty(shadingProp);
            }

            return material;
        }
    }
}
