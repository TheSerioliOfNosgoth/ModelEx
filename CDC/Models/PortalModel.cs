using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDC
{
	class PortalModel : IModel
	{
		protected DataFile _dataFile;
		protected string _name;
		protected string _modelTypePrefix;
		protected Platform _platform;
		protected Geometry _geometry;
		protected Geometry _extraGeometry;
		protected Polygon[] _polygons;
		protected Bone[] _bones;
		protected Tree[] _trees;
		protected Material[] _materials;

		public string Name { get { return _name; } }
		public string ModelTypePrefix { get { return _modelTypePrefix; } }
		public Polygon[] Polygons { get { return _polygons; } }
		public Geometry Geometry { get { return _geometry; } }
		public Geometry ExtraGeometry { get { return _extraGeometry; } }
		public Bone[] Bones { get { return _bones; } }
		public Tree[] Groups { get { return _trees; } }
		public Material[] Materials { get { return _materials; } }
		public Platform Platform { get { return _platform; } }

		public PortalModel(DataFile dataFile, string modelName, Platform platform, Vector min, Vector max, Vector[] t1, Vector[] t2)
		{
			_dataFile = dataFile;
			_name = modelName;
			_modelTypePrefix = "bb_";
			_platform  = platform;

			uint vertexCount = 14;  // 8 for the box, 6 for the quad.
			uint polygonCount = 14; // 12 for the box, 2 for the quad.
			uint indexCount = 42;   // 36 for the box, 6 for the quad.

			_geometry = new Geometry();
			_extraGeometry = new Geometry();

			_geometry.Normals = new Vector[1] { new Vector(1.0f, 1.0f, 1.0f) };
			_geometry.Colours = new uint[1] { 0xFFFFFFFF };
			_geometry.ColoursAlt = new uint[1] { 0xFFFFFFFF };
			_geometry.Vertices = new Vertex[vertexCount];
			_geometry.PositionsRaw = new Vector[]
			{
				new Vector { x = min.x, y = max.y, z = max.z },
				new Vector { x = max.x, y = max.y, z = max.z },
				new Vector { x = min.x, y = min.y, z = max.z },
				new Vector { x = max.x, y = min.y, z = max.z },
				new Vector { x = min.x, y = max.y, z = min.z },
				new Vector { x = max.x, y = max.y, z = min.z },
				new Vector { x = min.x, y = min.y, z = min.z },
				new Vector { x = max.x, y = min.y, z = min.z },
				t1[0],
				t1[1],
				t1[2],
				t2[0],
				t2[1],
				t2[2],
			};
			_geometry.PositionsPhys = new Vector[vertexCount];
			_geometry.PositionsAltPhys = new Vector[vertexCount];
			_geometry.UVs = new UV[indexCount];

			for (int i = 0; i < vertexCount; i++)
			{
				_geometry.PositionsPhys[i] = _geometry.PositionsRaw[i];
				_geometry.PositionsAltPhys[i] = _geometry.PositionsRaw[i];
				_geometry.Vertices[i] = new Vertex()
				{
					positionID = i,
				};
			}

			Material boxMaterial = new Material();
			boxMaterial.ID = 0;
			boxMaterial.visible = true;
			boxMaterial.textureUsed = false;
			boxMaterial.colour = 0xFFFF0000;

			Material quadMaterial = new Material();
			quadMaterial.ID = 1;
			quadMaterial.visible = true;
			quadMaterial.textureUsed = false;
			quadMaterial.colour = 0xFF0000FF;

			int[] indices =
			{
				0,  2,  1,  // 0 Box Top
				1,  2,  3,  // 1 Box Top
				4,  5,  6,  // 0 Box Bottom
				5,  7,  6,  // 1 Box Bottom
				0,  4,  2,  // 0 Box Left
				4,  6,  2,  // 1 Box Left
				1,  3,  5,  // 0 Box Right
				5,  3,  7,  // 1 Box Right
				0,  1,  4,  // 0 Box Back
				1,  5,  4,  // 1 Box Back
				6,  7,  2,  // 0 Box Front
				7,  3,  2,  // 1 Box Front
				8,  9,  10, // 0 Triangle 1
				11, 12, 13, // 0 Triangle 2
			};

			int v = 0;
			_polygons = new Polygon[polygonCount];
			for (int i = 0; i < polygonCount; i++)
			{
				_polygons[i] = new Polygon()
				{
					material = i < 12 ? boxMaterial : quadMaterial,
					v1 = _geometry.Vertices[indices[v++]],
					v2 = _geometry.Vertices[indices[v++]],
					v3 = _geometry.Vertices[indices[v++]],
				};
			}

			Tree tree = new Tree();
			tree.mesh = new Mesh();
			tree.mesh.indexCount = indexCount;
			tree.mesh.polygonCount = polygonCount;
			tree.mesh.polygons = _polygons;

			tree.mesh.vertices = new Vertex[indexCount];
			for (UInt16 poly = 0; poly < polygonCount; poly++)
			{
				tree.mesh.vertices[(3 * poly) + 0] = _polygons[poly].v1;
				tree.mesh.vertices[(3 * poly) + 1] = _polygons[poly].v2;
				tree.mesh.vertices[(3 * poly) + 2] = _polygons[poly].v3;
			}

			_bones = new Bone[0];
			_materials = new Material[2] { boxMaterial, quadMaterial };
			_trees = new Tree[1] { tree };
		}

		public PortalModel(DataFile dataFile, string modelName, Platform platform, Vector min, Vector max, Vector[] quad)
		{
			_dataFile = dataFile;
			_name = modelName;
			_modelTypePrefix = "bb_";
			_platform  = platform;

			uint vertexCount = 14;  // 8 for the box, 6 for the quad.
			uint polygonCount = 14; // 12 for the box, 2 for the quad.
			uint indexCount = 42;   // 36 for the box, 6 for the quad.

			_geometry = new Geometry();
			_extraGeometry = new Geometry();

			_geometry.Normals = new Vector[1] { new Vector(1.0f, 1.0f, 1.0f) };
			_geometry.Colours = new uint[1] { 0xFFFFFFFF };
			_geometry.ColoursAlt = new uint[1] { 0xFFFFFFFF };
			_geometry.Vertices = new Vertex[vertexCount];
			_geometry.PositionsRaw = new Vector[]
			{
				new Vector { x = min.x, y = max.y, z = max.z },
				new Vector { x = max.x, y = max.y, z = max.z },
				new Vector { x = min.x, y = min.y, z = max.z },
				new Vector { x = max.x, y = min.y, z = max.z },
				new Vector { x = min.x, y = max.y, z = min.z },
				new Vector { x = max.x, y = max.y, z = min.z },
				new Vector { x = min.x, y = min.y, z = min.z },
				new Vector { x = max.x, y = min.y, z = min.z },
				quad[0],
				quad[1],
				quad[2],
				quad[0],
				quad[2],
				quad[3],
			};
			_geometry.PositionsPhys = new Vector[vertexCount];
			_geometry.PositionsAltPhys = new Vector[vertexCount];
			_geometry.UVs = new UV[indexCount];

			for (int i = 0; i < vertexCount; i++)
			{
				_geometry.PositionsPhys[i] = _geometry.PositionsRaw[i];
				_geometry.PositionsAltPhys[i] = _geometry.PositionsRaw[i];
				_geometry.Vertices[i] = new Vertex()
				{
					positionID = i,
				};
			}

			Material boxMaterial = new Material();
			boxMaterial.ID = 0;
			boxMaterial.visible = true;
			boxMaterial.textureUsed = false;
			boxMaterial.colour = 0xFFFF0000;

			Material quadMaterial = new Material();
			quadMaterial.ID = 1;
			quadMaterial.visible = true;
			quadMaterial.textureUsed = false;
			quadMaterial.colour = 0xFF0000FF;

			int[] indices =
			{
				0,  2,  1,  // 0 Box Top
				1,  2,  3,  // 1 Box Top
				4,  5,  6,  // 0 Box Bottom
				5,  7,  6,  // 1 Box Bottom
				0,  4,  2,  // 0 Box Left
				4,  6,  2,  // 1 Box Left
				1,  3,  5,  // 0 Box Right
				5,  3,  7,  // 1 Box Right
				0,  1,  4,  // 0 Box Back
				1,  5,  4,  // 1 Box Back
				6,  7,  2,  // 0 Box Front
				7,  3,  2,  // 1 Box Front
				8,  9,  10, // 0 Triangle 1
				11, 12, 13, // 0 Triangle 2
			};

			int v = 0;
			_polygons = new Polygon[polygonCount];
			for (int i = 0; i < polygonCount; i++)
			{
				_polygons[i] = new Polygon()
				{
					material = i < 12 ? boxMaterial : quadMaterial,
					v1 = _geometry.Vertices[indices[v++]],
					v2 = _geometry.Vertices[indices[v++]],
					v3 = _geometry.Vertices[indices[v++]],
				};
			}

			Tree tree = new Tree();
			tree.mesh = new Mesh();
			tree.mesh.indexCount = indexCount;
			tree.mesh.polygonCount = polygonCount;
			tree.mesh.polygons = _polygons;

			tree.mesh.vertices = new Vertex[indexCount];
			for (UInt16 poly = 0; poly < polygonCount; poly++)
			{
				tree.mesh.vertices[(3 * poly) + 0] = _polygons[poly].v1;
				tree.mesh.vertices[(3 * poly) + 1] = _polygons[poly].v2;
				tree.mesh.vertices[(3 * poly) + 2] = _polygons[poly].v3;
			}

			_bones = new Bone[0];
			_materials = new Material[2] { boxMaterial, quadMaterial };
			_trees = new Tree[1] { tree };
		}

		public string GetTextureName(int materialIndex, ExportOptions options)
		{
			return "";
		}
	}
}
