using System;
using System.Collections.Generic;
using System.IO;
using CDC.Objects.Models;

namespace CDC.Objects
{
    public abstract class SRFile
    {
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

        protected SRFile(String strFileName, Game game)
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

            ReadHeaderData(xReader);

            if (_asset == Asset.Object)
            {
                ReadObjectData(xReader);
            }
            else
            {
                ReadUnitData(xReader);
            }

            xReader.Close();

            if (m_xLogFile != null)
            {
                m_xLogFile.Close();
                m_xLogFile = null;
            }
        }

        protected abstract void ReadHeaderData(BinaryReader xReader);

        protected abstract void ReadObjectData(BinaryReader xReader);

        protected abstract void ReadUnitData(BinaryReader xReader);

        protected abstract void ResolvePointers(BinaryReader xReader, BinaryWriter xWriter);

        public bool ExportToFile(String fileName)
        {
            string name = Utility.CleanName(Name).TrimEnd(new char[] { '_' });

            Assimp.Node rootNode = new Assimp.Node(name);
            List<Assimp.Material> materials = new List<Assimp.Material>();
            List<Assimp.Mesh> meshes = new List<Assimp.Mesh>();

            for (int modelIndex = 0; modelIndex < ModelCount; modelIndex++)
            {
                SRModel model = Models[modelIndex];
                string modelName = name + "-" + modelIndex;

                Assimp.Node modelNode = new Assimp.Node(modelName);

                for (int groupIndex = 0; groupIndex < model.GroupCount; groupIndex++)
                {
                    Tree group = model.Groups[groupIndex];
                    if (group == null)
                    {
                        continue;
                    }

                    string groupName = name + "-" + modelIndex + "-" + groupIndex;

                    Assimp.Node groupNode = new Assimp.Node(groupName);

                    for (int materialIndex = 0; materialIndex < model.MaterialCount; materialIndex++)
                    {
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
                            Assimp.Mesh mesh = new Assimp.Mesh(meshName);
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
                                    mesh.Vertices.Add(new Assimp.Vector3D(pos.x, pos.y, pos.z));

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

                            string materialName = name + "-" + modelIndex + "-" + groupIndex + "-" + materialIndex;
                            Assimp.Material material = GetAssimpMaterial(materialName, model, materialIndex);

                            mesh.MaterialIndex = materials.Count;
                            materials.Add(material);

                            groupNode.MeshIndices.Add(meshes.Count);
                            meshes.Add(mesh);
                            #endregion
                        }
                    }

                    modelNode.Children.Add(groupNode);
                }

                rootNode.Children.Add(modelNode);
            }

            Assimp.Scene scene = new Assimp.Scene();
            scene.RootNode = rootNode;
            scene.Materials.AddRange(materials);
            scene.Meshes.AddRange(meshes);

            Assimp.AssimpContext context = new Assimp.AssimpContext();
            context.ExportFile(scene, fileName, "collada", Assimp.PostProcessSteps.None);

            return true;
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
            Assimp.Vector3D uv3D = new Assimp.Vector3D()
            {
                X = uv.u,
                Y = uv.u,
                Z = 0.0f
            };

            return uv3D;
        }

        protected Assimp.Material GetAssimpMaterial(String name, SRModel model, int materialIndex)
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
                textureDiffuse.FilePath = model.GetTextureName(materialIndex) + ".dds";
                textureDiffuse.TextureIndex = 0;
                textureDiffuse.TextureType = Assimp.TextureType.Diffuse;
                textureDiffuse.WrapModeU = Assimp.TextureWrapMode.Clamp;
                textureDiffuse.WrapModeV = Assimp.TextureWrapMode.Clamp;

                // Bugged.
                //material.TextureDiffuse = textureDiffuse;
                // Works, but looks like there might be a better way.
                //string texturePropName = Assimp.Unmanaged.AiMatKeys.TEXTURE_BASE;
                //Assimp.MaterialProperty textureProp = new Assimp.MaterialProperty(texturePropName, model.GetTextureName(materialIndex) + ".dds", Assimp.TextureType.Diffuse, 0);
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
