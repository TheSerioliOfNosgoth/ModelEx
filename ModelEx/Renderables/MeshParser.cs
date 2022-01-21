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

		public void BuildMesh()
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

			int[] indices = {
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

			Mesh = new MeshPCT(this);

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
