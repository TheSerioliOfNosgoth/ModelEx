using System;
using System.Collections.Generic;

namespace ModelEx
{
	class MeshParser :
		IMeshParser<PositionColorTexturedVertex, short>
	{
		struct BasicVertex
		{
			public float X;
			public float Y;
			public float Z;

			public BasicVertex(float x, float y, float z)
			{
				X = x;
				Y = y;
				Z = z;
			}

			// Normalize the vertex to lie on a sphere of the given radius
			public BasicVertex Normalize(float radius)
			{
				float length = (float)Math.Sqrt(X * X + Y * Y + Z * Z);
				return new BasicVertex(X / length * radius, Y / length * radius, Z / length * radius);
			}
		}

		List<BasicVertex> _vertexList = new List<BasicVertex>();
		List<int> _indexList = new List<int>();
		public List<SubMesh> SubMeshes { get; } = new List<SubMesh>();
		public Mesh Mesh;
		public string MeshName { get; private set; }
		public string Technique { get; private set; }
		public int VertexCount { get { return _vertexList.Count; } }
		public int IndexCount { get { return _indexList.Count; } }

		public MeshParser(string meshName)
		{
			MeshName = meshName;
		}

		public void BuildCube(RenderResource resource)
		{
			float half = 0.5f;

			BasicVertex[] vertices =
			{
				new BasicVertex { X = -half, Y =  half, Z =  half }, // left, top, back			0
				new BasicVertex { X =  half, Y =  half, Z =  half }, // right, top, back		1
				new BasicVertex { X = -half, Y = -half, Z =  half }, // left, bottom, back		2
				new BasicVertex { X =  half, Y = -half, Z =  half }, // right, bottom, back		3
				new BasicVertex { X = -half, Y =  half, Z = -half }, // left, top, front		4
				new BasicVertex { X =  half, Y =  half, Z = -half }, // right, top, front		5
				new BasicVertex { X = -half, Y = -half, Z = -half }, // left, bottom, front		6
				new BasicVertex { X =  half, Y = -half, Z = -half }, // right, bottom, front	7
			};

			_vertexList.AddRange(vertices);

			#region
			/*int[] indices =
			{
				0, 1, 2,
				1, 3, 2,
				0, 2, 4,
				4, 2, 6,
				4, 6, 5,
				5, 6, 7,
				1, 5, 3,
				5, 7, 3,
				0, 4, 1,
				1, 4, 5,
				2, 3, 6,
				3, 7, 6,
			};*/
			#endregion

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

			_indexList.AddRange(indices);

			Technique = "DefaultRender";

			Mesh = new MeshPCT(resource, this);

			SubMesh subMeshA = new SubMesh
			{
				Name = MeshName + "-0",
				MaterialIndex = 0,
				indexCount = 12,
				startIndexLocation = 0,
				baseVertexLocation = 0
			};

			SubMeshes.Add(subMeshA);

			SubMesh subMeshB = new SubMesh
			{
				Name = MeshName + "-1",
				MaterialIndex = 1,
				indexCount = 12,
				startIndexLocation = 12,
				baseVertexLocation = 0
			};

			SubMeshes.Add(subMeshB);

			SubMesh subMeshC = new SubMesh
			{
				Name = MeshName + "-2",
				MaterialIndex = 2,
				indexCount = 12,
				startIndexLocation = 24,
				baseVertexLocation = 0
			};

			SubMeshes.Add(subMeshC);
		}

		public void BuildOctahedron(RenderResource resource)
		{
			float v = 1.2f;
			float h = 1.0f;

			BasicVertex[] vertices =
			{
				new BasicVertex { X =  0, Y =  v, Z =  0 },
				new BasicVertex { X = -h, Y =  0, Z =  h },
				new BasicVertex { X = -h, Y =  0, Z = -h },
				new BasicVertex { X =  h, Y =  0, Z = -h },
				new BasicVertex { X =  h, Y =  0, Z =  h },
				new BasicVertex { X =  0, Y = -v, Z =  0 }
			};

			_vertexList.AddRange(vertices);

			int[] indices =
			{
				0, 1, 2,
				5, 3, 2,
				0, 3, 4,
				5, 1, 4,
				5, 2, 1,
				0, 2, 3,
				5, 4, 3,
				0, 4, 1
			};

			_indexList.AddRange(indices);

			Technique = "DefaultRender";

			Mesh = new MeshPCT(resource, this);

			SubMesh subMeshA = new SubMesh
			{
				Name = MeshName + "-0",
				MaterialIndex = 0,
				indexCount = 12,
				startIndexLocation = 0,
				baseVertexLocation = 0
			};

			SubMeshes.Add(subMeshA);

			SubMesh subMeshB = new SubMesh
			{
				Name = MeshName + "-1",
				MaterialIndex = 1,
				indexCount = 12,
				startIndexLocation = 12,
				baseVertexLocation = 0
			};

			SubMeshes.Add(subMeshB);
		}

		public void BuildSphere(RenderResource resource, float radius, int subdivisions)
		{
			var (vertices, faces) = GenerateSphere(radius, subdivisions);

			_vertexList.AddRange(vertices);

			foreach (var face in faces)
			{
				_indexList.AddRange(face);
			}

			Technique = "DefaultRender";

			Mesh = new MeshPCT(resource, this);

			SubMesh subMesh = new SubMesh
			{
				Name = MeshName + "-0",
				MaterialIndex = 0,
				indexCount = _indexList.Count,
				startIndexLocation = 0,
				baseVertexLocation = 0
			};

			SubMeshes.Add(subMesh);
		}

		public void FillVertex(int v, out PositionColorTexturedVertex vertex)
		{
			vertex.Position = new SlimDX.Vector3()
			{
				X = _vertexList[v].X,
				Y = _vertexList[v].Y,
				Z = _vertexList[v].Z
			};

			vertex.Color = new SlimDX.Color3()
			{
				//Alpha = 1.0f,
				Red = 1.0f,
				Green = 1.0f,
				Blue = 1.0f
			};

			vertex.TextureCoordinates = new SlimDX.Vector2()
			{
				X = 0.0f,
				Y = 0.0f
			};
		}

		public void FillIndex(int i, out short index)
		{
			index = (short)_indexList[i];
		}

		// Generate the base icosahedron
		private static (List<BasicVertex>, List<int[]>) GenerateIcosahedron(float radius)
		{
			float phi = (1 + (float)Math.Sqrt(5)) / 2; // Golden ratio

			// Define the 12 vertices of an icosahedron
			List<BasicVertex> vertices = new List<BasicVertex>
			{
				new BasicVertex(-1,  phi,  0).Normalize(radius),
				new BasicVertex( 1,  phi,  0).Normalize(radius),
				new BasicVertex(-1, -phi,  0).Normalize(radius),
				new BasicVertex( 1, -phi,  0).Normalize(radius),
				new BasicVertex( 0, -1,  phi).Normalize(radius),
				new BasicVertex( 0,  1,  phi).Normalize(radius),
				new BasicVertex( 0, -1, -phi).Normalize(radius),
				new BasicVertex( 0,  1, -phi).Normalize(radius),
				new BasicVertex( phi,  0, -1).Normalize(radius),
				new BasicVertex( phi,  0,  1).Normalize(radius),
				new BasicVertex(-phi,  0, -1).Normalize(radius),
				new BasicVertex(-phi,  0,  1).Normalize(radius)
			};

			// Define the 20 triangular faces
			List<int[]> faces = new List<int[]>
			{
				new int[] { 0, 11, 5 }, new int[] { 0, 5, 1 }, new int[] { 0, 1, 7 }, new int[] { 0, 7, 10 }, new int[] { 0, 10, 11 },
				new int[] { 1, 5, 9 }, new int[] { 5, 11, 4 }, new int[] { 11, 10, 2 }, new int[] { 10, 7, 6 }, new int[] { 7, 1, 8 },
				new int[] { 3, 9, 4 }, new int[] { 3, 4, 2 }, new int[] { 3, 2, 6 }, new int[] { 3, 6, 8 }, new int[] { 3, 8, 9 },
				new int[] { 4, 9, 5 }, new int[] { 2, 4, 11 }, new int[] { 6, 2, 10 }, new int[] { 8, 6, 7 }, new int[] { 9, 8, 1 }
			};

			return (vertices, faces);
		}

		// Subdivide a triangular face into four smaller triangles
		private static List<int[]> Subdivide(List<BasicVertex> vertices, List<int[]> faces, float radius)
		{
			Dictionary<(int, int), int> midpointCache = new Dictionary<(int, int), int>();

			int GetMidpoint(int v1, int v2)
			{
				var key = v1 < v2 ? (v1, v2) : (v2, v1);
				if (!midpointCache.ContainsKey(key))
				{
					BasicVertex midpoint = new BasicVertex(
						(vertices[v1].X + vertices[v2].X) / 2,
						(vertices[v1].Y + vertices[v2].Y) / 2,
						(vertices[v1].Z + vertices[v2].Z) / 2
					).Normalize(radius);
					vertices.Add(midpoint);
					midpointCache[key] = vertices.Count - 1;
				}
				return midpointCache[key];
			}

			List<int[]> newFaces = new List<int[]>();
			foreach (var face in faces)
			{
				int a = GetMidpoint(face[0], face[1]);
				int b = GetMidpoint(face[1], face[2]);
				int c = GetMidpoint(face[2], face[0]);

				newFaces.Add(new int[] { face[0], a, c });
				newFaces.Add(new int[] { face[1], b, a });
				newFaces.Add(new int[] { face[2], c, b });
				newFaces.Add(new int[] { a, b, c });
			}

			return newFaces;
		}

		// Generate a sphere by subdividing an icosahedron
		private static (List<BasicVertex>, List<int[]>) GenerateSphere(float radius, int subdivisions)
		{
			var (vertices, faces) = GenerateIcosahedron(radius);

			for (int i = 0; i < subdivisions; i++)
			{
				faces = Subdivide(vertices, faces, radius);
			}

			return (vertices, faces);
		}
	}
}
