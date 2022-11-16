using System;
using System.Collections.Generic;
using System.IO;

namespace CDC
{
	public abstract class TRLModel : Model
	{
		protected class TRLTriangleList
		{
			public Material material;
			public UInt32 polygonCount;
			public UInt32 polygonStart;
			public UInt16 groupID;
			public UInt32 next;
		}

		protected class TRLMaterial
		{
			public UInt32 textureID;
			public UInt32 vbBaseOffset;
		}

		protected DataFile _dataFile;
		protected string _name;
		protected string _modelTypePrefix;
		protected uint _version;
		protected Platform _platform;
		protected uint _dataStart;
		protected uint _modelData;
		protected uint _vertexCount;
		protected uint _vertexStart;
		protected uint _polygonCount;
		protected uint _polygonStart;
		protected uint _boneCount;
		protected uint _boneStart;
		protected uint _groupCount;
		protected uint _materialCount;
		protected uint _materialStart;
		protected uint _indexCount { get { return 3 * _polygonCount; } }
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

		public override string Name { get { return _name; } }
		public override string ModelTypePrefix { get { return _modelTypePrefix; } }
		public override Polygon[] Polygons { get { return _polygons; } }
		public override Geometry Geometry { get { return _geometry; } }
		public override Geometry ExtraGeometry { get { return _extraGeometry; } }
		public override Bone[] Bones { get { return _bones; } }
		public override Tree[] Groups { get { return _trees; } }
		public override Material[] Materials { get { return _materials; } }
		public override Platform Platform { get { return _platform; } }

		protected TRLModel(BinaryReader reader, DataFile dataFile, UInt32 dataStart, UInt32 modelData, String modelName, Platform ePlatform, UInt32 version)
		{
			_dataFile = dataFile;
			_name = modelName;
			_modelTypePrefix = "";
			_platform = ePlatform;
			_version = version;
			_dataStart = dataStart;
			_modelData = modelData;
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

		public virtual void ReadData(BinaryReader reader, ExportOptions options)
		{
			// Get the vertices
			_geometry.Vertices = new Vertex[_vertexCount];
			_geometry.PositionsRaw = new Vector[_vertexCount];
			_geometry.PositionsPhys = new Vector[_vertexCount];
			_geometry.PositionsAltPhys = new Vector[_vertexCount];
			_geometry.Normals = new Vector[_vertexCount];
			_geometry.Colours = new UInt32[_vertexCount];
			_geometry.ColoursAlt = new UInt32[_vertexCount];
			_geometry.UVs = new UV[_vertexCount];
			ReadVertices(reader, options);

			// Get the polygons
			_polygons = new Polygon[_polygonCount];
			ReadPolygons(reader, options);

			for (uint p = 0; p < _polygonCount; p++)
			{
				HandleDebugRendering((int)p, options);
			}

			// Generate the output
			GenerateOutput();
		}

		protected virtual void ReadVertex(BinaryReader reader, int v, ExportOptions options)
		{
			_geometry.Vertices[v].positionID = v;

			// Read the local coordinates
			_geometry.PositionsRaw[v].x = (float)reader.ReadInt16();
			_geometry.PositionsRaw[v].y = (float)reader.ReadInt16();
			_geometry.PositionsRaw[v].z = (float)reader.ReadInt16();
		}

		protected virtual void ReadVertices(BinaryReader reader, ExportOptions options)
		{
			if (_vertexStart == 0 || _vertexCount == 0)
			{
				return;
			}

			reader.BaseStream.Position = _vertexStart;

			for (int v = 0; v < _vertexCount; v++)
			{
				ReadVertex(reader, v, options);
			}

			return;
		}

		protected abstract void ReadPolygons(BinaryReader reader, ExportOptions options);

		protected virtual void GenerateOutput()
		{
			// Make the vertices unique
			_geometry.Vertices = new Vertex[_indexCount];
			for (uint p = 0; p < _polygonCount; p++)
			{
				_geometry.Vertices[(3 * p) + 0] = _polygons[p].v1;
				_geometry.Vertices[(3 * p) + 1] = _polygons[p].v2;
				_geometry.Vertices[(3 * p) + 2] = _polygons[p].v3;
			}

			// Build the materials array
			_materials = new Material[_materialCount];
			UInt16 mNew = 0;

			foreach (Material xMaterial in _materialsList)
			{
				_materials[mNew] = xMaterial;
				_materials[mNew].ID = mNew;
				mNew++;
			}

			return;
		}

		public override string GetTextureName(int materialIndex, ExportOptions options)
		{
			string textureName = "";
			if (materialIndex >= 0 && materialIndex < _materials.Length)
			{
				Material material = _materials[materialIndex];
				if (material.textureUsed)
				{
					textureName = Utility.GetPS2TextureName(_dataFile.Name, material.textureID);
				}
			}

			return textureName;
		}
	}
}
