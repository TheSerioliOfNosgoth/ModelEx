using System;
using System.Collections.Generic;
using System.IO;

namespace CDC.Objects.Models
{
    public abstract class SRModel
    {
        protected String _name;
        protected UInt32 _version;
        protected Platform _platform;
        protected UInt32 _dataStart;
        protected UInt32 _modelData;
        protected UInt32 _vertexCount;
        protected UInt32 _vertexStart;
        protected UInt32 _polygonCount;
        protected UInt32 _polygonStart;
        protected UInt32 _boneCount;
        protected UInt32 _boneStart;
        protected UInt32 _groupCount;
        protected UInt32 _materialCount;
        protected UInt32 _materialStart;
        protected UInt32 _indexCount { get { return 3 * _polygonCount; } }
        // Vertices are scaled before any bones are applied.
        // Scaling afterwards will break the characters.
        protected Vector _vertexScale;
        protected Geometry _geometry;
        protected Geometry _extraGeometry;
        protected Polygon[] _polygons;
        protected Bone[] _bones;
        protected Tree[] _trees;
        protected Material[] _materials;
        protected List<Material> _materialsList;

        public String Name { get { return _name; } }
        public UInt32 PolygonCount { get { return _polygonCount; } }
        public Polygon[] Polygons { get { return _polygons; } }
        public UInt32 IndexCount { get { return _indexCount; } }
        public Geometry Geometry { get { return _geometry; } }
        public Geometry ExtraGeometry { get { return _extraGeometry; } }
        public Bone[] Bones { get { return _bones; } }
        public UInt32 GroupCount { get { return _groupCount; } }
        public Tree[] Groups { get { return _trees; } }
        public UInt32 MaterialCount { get { return _materialCount; } }
        public Material[] Materials { get { return _materials; } }
        public Platform Platform { get { return _platform; } }

        protected SRModel(BinaryReader xReader, UInt32 uDataStart, UInt32 uModelData, String strModelName, Platform ePlatform, UInt32 uVersion)
        {
            _name = strModelName;
            _platform = ePlatform;
            _version = uVersion;
            _dataStart = uDataStart;
            _modelData = uModelData;
            _vertexCount = 0;
            _vertexStart = 0;
            _polygonCount = 0;
            _polygonStart = 0;
            _vertexScale.x = 1.0f;
            _vertexScale.y = 1.0f;
            _vertexScale.z = 1.0f;
            _geometry = new Geometry();
            _extraGeometry = new Geometry();
            _materialsList = new List<Material>();
        }

        public String GetTextureName(int materialIndex)
        {
            String textureName = "";
            if (materialIndex >= 0 && materialIndex < MaterialCount)
            {
                Material material = Materials[materialIndex];
                if (material.textureUsed)
                {
                    if (this is SR1Model)
                    {
                        if (Platform == Platform.PSX)
                        {
                            textureName =
                                Name.TrimEnd(new char[] { '_' }).ToLower() + "-" +
                                material.textureID.ToString("0000");
                        }
                        else
                        {
                            textureName =
                                "Texture-" +
                                material.textureID.ToString("00000");
                        }
                    }
                    else if (this is SR2Model)
                    {
                        textureName =
                            Name.TrimEnd(new char[] { '_' }).ToLower() + "-" +
                            material.textureID.ToString("0000");
                    }
                }
            }

            return textureName;
        }
    }
}
