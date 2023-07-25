using System;
using System.Collections.Generic;

namespace ModelEx
{
	class MeshParserCDCNormals :
		IMeshParser<PositionVertex>
	{
		CDC.DataFile _dataFile;
		CDC.IModel _cdcModel;
		CDC.Tree _cdcGroup;
		List<int> _polygonList = new List<int>();
		public List<SubMesh> SubMeshes { get; } = new List<SubMesh>();
		public Mesh Mesh { get; protected set; }

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
				int totalIndexCount = (int)_cdcGroup.mesh.polygonCount;
				for (int p = 0; p < totalIndexCount; p++)
				{
					if (_cdcGroup.mesh.polygons[p].material.ID == materialIndex)
					{
						_polygonList.Add(p);
						_polygonList.Add(p);
						indexCount += 2;
					}
				}

				if (indexCount > 0)
				{
					string subMeshName = meshName + "-subMesh-" + materialIndex.ToString();
					SubMesh subMesh = new SubMesh
					{
						Name = subMeshName,
						MaterialIndex = materialIndex,
						IndexCount = indexCount,
						StartIndexLocation = startIndexLocation,
						BaseVertexLocation = startIndexLocation
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

		public string MeshName { get; private set; }

		public string Technique { get; private set; }

		public int VertexCount { get { return _polygonList.Count; } }

		public void FillVertex(int v, out PositionVertex vertex)
		{
			ref CDC.Polygon exPolygon = ref _cdcGroup.mesh.polygons[_polygonList[v]];

			ref CDC.Vertex exV1 = ref exPolygon.v1;
			ref CDC.Vertex exV2 = ref exPolygon.v2;
			ref CDC.Vertex exV3 = ref exPolygon.v3;

			CDC.Geometry exG1 = exV1.isExtraGeometry ? _cdcModel.ExtraGeometry : _cdcModel.Geometry;
			CDC.Geometry exG2 = exV2.isExtraGeometry ? _cdcModel.ExtraGeometry : _cdcModel.Geometry;
			CDC.Geometry exG3 = exV3.isExtraGeometry ? _cdcModel.ExtraGeometry : _cdcModel.Geometry;

			vertex.Position = new SlimDX.Vector3();

			vertex.Position.X += exG1.PositionsPhys[exV1.positionID].x;
			vertex.Position.Y += exG1.PositionsPhys[exV1.positionID].z;
			vertex.Position.Z += exG1.PositionsPhys[exV1.positionID].y;

			vertex.Position.X += exG2.PositionsPhys[exV2.positionID].x;
			vertex.Position.Y += exG2.PositionsPhys[exV2.positionID].z;
			vertex.Position.Z += exG2.PositionsPhys[exV2.positionID].y;

			vertex.Position.X += exG3.PositionsPhys[exV3.positionID].x;
			vertex.Position.Y += exG3.PositionsPhys[exV3.positionID].z;
			vertex.Position.Z += exG3.PositionsPhys[exV3.positionID].y;

			vertex.Position.X /= 3.0f;
			vertex.Position.Y /= 3.0f;
			vertex.Position.Z /= 3.0f;
			vertex.Position.X *= 0.01f;
			vertex.Position.Y *= 0.01f;
			vertex.Position.Z *= 0.01f;

			if (v % 2 != 0)
			{
				SlimDX.Vector3 normal = SlimDX.Vector3.Normalize(vertex.Position);
				vertex.Position += normal;
			}
		}
	}
}
