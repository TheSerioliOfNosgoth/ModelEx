using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDC
{
	class BoundingBoxModel : IModel
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

		public BoundingBoxModel(DataFile dataFile, string modelName, Platform platform, Vector min, Vector max)
		{
			_dataFile = dataFile;
			_name = modelName;
			_modelTypePrefix = "bb_";
			_platform  = platform;
			_geometry = new Geometry();
			_extraGeometry = new Geometry();

			_geometry.Normals = new Vector[1] { new Vector(1.0f, 1.0f, 1.0f) };
			_geometry.Colours = new uint[1] { 0xFFFFFFFF };
			_geometry.ColoursAlt = new uint[1] { 0xFFFFFFFF };
			_geometry.Vertices = new Vertex[8];
			_geometry.PositionsRaw = new Vector[8]
			{
				new Vector { x = min.x, y = max.y, z = max.z },
				new Vector { x = max.x, y = max.y, z = max.z },
				new Vector { x = min.x, y = min.y, z = max.z },
				new Vector { x = max.x, y = min.y, z = max.z },
				new Vector { x = min.x, y = max.y, z = min.z },
				new Vector { x = max.x, y = max.y, z = min.z },
				new Vector { x = min.x, y = min.y, z = min.z },
				new Vector { x = max.x, y = min.y, z = min.z },
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
			material.ID = 0;
			material.visible = true;
			material.textureUsed = false;
			material.colour = 0xFFFF0000;

			/*Material redMaterial = new Material();
			redMaterial.ID = 0;
			redMaterial.visible = true;
			redMaterial.textureUsed = false;
			redMaterial.colour = 0xFFFF0000;

			Material greenMaterial = new Material();
			greenMaterial.ID = 1;
			greenMaterial.visible = true;
			greenMaterial.textureUsed = false;
			greenMaterial.colour = 0xFF00FF00;

			Material blueMaterial = new Material();
			blueMaterial.ID = 2;
			blueMaterial.visible = true;
			blueMaterial.textureUsed = false;
			blueMaterial.colour = 0xFF0000FF;*/

			int[] indices =
			{
				0, 2, 1, // 0 Top
				1, 2, 3, // 1 Top
				4, 5, 6, // 0 Bottom
				5, 7, 6, // 1 Bottom
				0, 4, 2, // 0 Left
				4, 6, 2, // 1 Left
				1, 3, 5, // 0 Right
				5, 3, 7, // 1 Right
				0, 1, 4, // 0 Back
				1, 5, 4, // 1 Back
				6, 7, 2, // 0 Front
				7, 3, 2, // 1 Front
			};

			int v = 0;
			_polygons = new Polygon[12];
			for (int i = 0; i < 12; i++)
			{
				_polygons[i] = new Polygon()
				{
					material = material,
					// material =  (i < 4) ? redMaterial : (i < 8) ? greenMaterial : blueMaterial,
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

			_bones = new Bone[0];
			//_materials = new Material[3] { redMaterial, greenMaterial, blueMaterial };
			_materials = new Material[1] { material };
			_trees = new Tree[1] { tree };
		}

		public string GetTextureName(int materialIndex, ExportOptions options)
		{
			return "";
		}
	}
}
