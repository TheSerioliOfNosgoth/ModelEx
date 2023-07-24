using System;
using System.Collections.Generic;

namespace ModelEx
{
	class MeshParserCDCNormals :
		IMeshParser<PositionVertex, short>
	{
		CDC.DataFile _dataFile;
		CDC.IModel _cdcModel;
		CDC.Tree _cdcGroup;
		List<int> _vertexList = new List<int>();
		List<int> _indexList = new List<int>();
		public List<SubMesh> SubMeshes { get; } = new List<SubMesh>();
		public Mesh Mesh;

		public MeshParserCDCNormals(string objectName, CDC.DataFile dataFile)
		{
			_dataFile = dataFile;
		}

		public void BuildMesh(RenderResource resource, int modelIndex, int groupIndex, int meshIndex)
		{
			_cdcModel = _dataFile.Models[modelIndex];
			_cdcGroup = _cdcModel.Groups[groupIndex];
			string modelName = _cdcModel.Name;
			string groupName = modelName + "-group-" + groupIndex.ToString();
			string meshName = groupName + "-mesh-" + meshIndex.ToString();

			int startIndexLocation = 0;
			for (int materialIndex = 0; materialIndex < _cdcModel.Materials.Length; materialIndex++)
			{
				int indexCount = 0;
				int totalIndexCount = (int)_cdcGroup.mesh.indexCount;
				for (int v = 0; v < totalIndexCount; v++)
				{
					if (_cdcGroup.mesh.polygons[v / 3].material.ID == materialIndex)
					{
						_vertexList.Add(v);
						_indexList.Add(_indexList.Count - startIndexLocation);
						indexCount++;
					}
				}

				if (indexCount > 0)
				{
					string subMeshName = meshName + "-subMesh-" + materialIndex.ToString();
					SubMesh subMesh = new SubMesh
					{
						Name = subMeshName,
						MaterialIndex = materialIndex,
						indexCount = indexCount,
						startIndexLocation = startIndexLocation,
						baseVertexLocation = startIndexLocation
					};
					SubMeshes.Add(subMesh);

					startIndexLocation += indexCount;
				}
			}

			if (SubMeshes.Count > 0)
			{
				MeshName = meshName;
				Technique = "DefaultRender";
				Mesh = new LineCloud(resource, this);
			}
		}

		public string MeshName
		{
			get;
			private set;
		}

		public string Technique
		{
			get;
			private set;
		}

		public int VertexCount { get { return _vertexList.Count; } }

		public void FillVertex(int v, out PositionVertex vertex)
		{
			ref CDC.Vertex exVertex = ref _cdcGroup.mesh.vertices[_vertexList[v]];
			CDC.Geometry exGeometry = exVertex.isExtraGeometry ? _cdcModel.ExtraGeometry : _cdcModel.Geometry;

			vertex.Position = new SlimDX.Vector3()
			{
				X = 0.01f * exGeometry.PositionsPhys[exVertex.positionID].x,
				Y = 0.01f * exGeometry.PositionsPhys[exVertex.positionID].z,
				Z = 0.01f * exGeometry.PositionsPhys[exVertex.positionID].y
			};
		}

		public int IndexCount { get { return _vertexList.Count; } }

		public void FillIndex(int i, out short index)
		{
			index = (short)_indexList[i];
		}
	}
}
