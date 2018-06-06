using System;
using System.Collections.Generic;
using System.IO;

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
        protected UInt32 _connectedUnitCount;
        protected UInt32 _connectedUnitStart;
        protected String[] _connectedUnitNames;
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
        public UInt32 ConectedUnitCount { get { return _connectedUnitCount; } }
        public String[] ConnectedUnit { get { return _connectedUnitNames; } }
        public Game Game { get { return _game; } }
        public Asset Asset { get { return _asset; } }
        public Platform Platform { get { return _platform; } }

        public static StreamWriter m_xLogFile = null;

        protected SRFile(String strFileName, Game game)
        {
            _game = game;

            FileStream xFile = new FileStream(strFileName, FileMode.Open, FileAccess.Read);
            BinaryReader xReader = new BinaryReader(xFile);
            MemoryStream xStream = new MemoryStream((int)xFile.Length);
            BinaryWriter xWriter = new BinaryWriter(xStream);

            //String strDebugFileName = Path.GetDirectoryName(strFileName) + "\\" + Path.GetFileNameWithoutExtension(strFileName) + "-Debug.txt";
            //m_xLogFile = File.CreateText(strDebugFileName);

            ResolvePointers(xReader, xWriter);
            xReader.Close();
            xReader = new BinaryReader(xStream);

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
            //Create a very simple scene a single node with a mesh that has a single face, a triangle and a default material
            Assimp.Scene scene = new Assimp.Scene();
            scene.RootNode = new Assimp.Node("Root");

            Assimp.Mesh triangle = new Assimp.Mesh("", Assimp.PrimitiveType.Triangle);
            triangle.Vertices.Add(new Assimp.Vector3D(1, 0, 0));
            triangle.Vertices.Add(new Assimp.Vector3D(5, 5, 0));
            triangle.Vertices.Add(new Assimp.Vector3D(10, 0, 0));
            triangle.Faces.Add(new Assimp.Face(new int[] { 0, 1, 2 }));
            triangle.MaterialIndex = 0;

            scene.Meshes.Add(triangle);
            scene.RootNode.MeshIndices.Add(0);

            Assimp.Material mat = new Assimp.Material();
            mat.Name = "MyMaterial";
            scene.Materials.Add(mat);

            Assimp.AssimpContext context = new Assimp.AssimpContext();
            context.ExportFile(scene, fileName, "collada", Assimp.PostProcessSteps.None);
            return false;
        }
    }
}
