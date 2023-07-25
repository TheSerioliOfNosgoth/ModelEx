using System;
using System.Collections.Generic;

namespace ModelEx
{
	class MeshParser :
		IMeshParserIndexed<PositionColorTexturedVertex, short>
	{
		struct BasicVertex
		{
			public float X;
			public float Y;
			public float Z;
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
	}
}
