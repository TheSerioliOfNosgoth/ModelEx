using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDC
{
	class BoundingBox : IModel
	{
		protected String _name;
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

		public BoundingBox(string strModelName, Platform platform, Vector min, Vector max)
		{
			_name = strModelName;
			_modelTypePrefix = "bb_";
			_platform  = platform;
			_geometry = new Geometry();
			_extraGeometry = new Geometry();

			float half = 500f;

			_geometry.Normals = new Vector[1] { new Vector(1.0f, 1.0f, 1.0f) };
			_geometry.Colours = new uint[1] { 0xFFFFFFFF };
			_geometry.ColoursAlt = new uint[1] { 0xFFFFFFFF };
			_geometry.Vertices = new Vertex[8];
			_geometry.PositionsRaw = new Vector[8]
			{
				new Vector { x = -half, y =  half, z =  half }, // left, top, back		0
				new Vector { x =  half, y =  half, z =  half }, // right, top, back		1
				new Vector { x = -half, y = -half, z =  half }, // left, bottom, back	2
				new Vector { x =  half, y = -half, z =  half }, // right, bottom, back	3
				new Vector { x = -half, y =  half, z = -half }, // left, top, front		4
				new Vector { x =  half, y =  half, z = -half }, // right, top, front	5
				new Vector { x = -half, y = -half, z = -half }, // left, bottom, front	6
				new Vector { x =  half, y = -half, z = -half }, // right, bottom, front	7
			};
			_geometry.PositionsPhys = new Vector[8];
			_geometry.PositionsAltPhys = new Vector[8];
			_geometry.UVs = new UV[36];

			for (int i = 0; i < 8; i++)
			{
				_geometry.PositionsPhys[i] = _geometry.PositionsRaw[i];
				_geometry.PositionsAltPhys[i] = _geometry.PositionsRaw[i];
				_geometry.Vertices[i] = new Vertex()
				{
					positionID = i,
				};
			}

			Material material = new Material();
			material.visible = true;
			material.textureUsed = false;
			material.colour = 0xFFFF0000;

			int[] indices =
			{
				0, 1, 2, // 0 Front
				1, 3, 2, // 1 Front
				4, 6, 5, // 0 Left
				5, 6, 7, // 1 Left
				0, 2, 4, // 0 Back
				4, 2, 6, // 1 Back
				1, 5, 3, // 0 Right
				5, 7, 3, // 1 Right
				0, 4, 1, // 0 Top
				1, 4, 5, // 1 Top
				6, 2, 7, // 0 Bottom
				7, 2, 3, // 1 Bottom
			};

			int v = 0;
			_polygons = new Polygon[12];
			for (int i = 0; i < 12; i++)
			{
				_polygons[i] = new Polygon()
				{
					material = material,
					v1 = _geometry.Vertices[indices[v++]],
					v2 = _geometry.Vertices[indices[v++]],
					v3 = _geometry.Vertices[indices[v++]],
				};
			}

			Tree tree = new Tree();
			tree.mesh = new Mesh();
			tree.mesh.indexCount = 36;
			tree.mesh.polygonCount = 12;
			tree.mesh.polygons = _polygons;
			tree.mesh.vertices = _geometry.Vertices;

			tree.mesh.vertices = new Vertex[36];
			for (UInt16 poly = 0; poly < 12; poly++)
			{
				tree.mesh.vertices[(3 * poly) + 0] = _polygons[poly].v1;
				tree.mesh.vertices[(3 * poly) + 1] = _polygons[poly].v2;
				tree.mesh.vertices[(3 * poly) + 2] = _polygons[poly].v3;
			}

			_materials = new Material[1] { material };
			_trees = new Tree[1] { tree };
		}

		public string GetTextureName(int materialIndex, ExportOptions options)
		{
			return "";
		}
	}
}
