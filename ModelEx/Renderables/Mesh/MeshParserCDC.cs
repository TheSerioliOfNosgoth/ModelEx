using System;
using System.Collections.Generic;

namespace ModelEx
{
	class MeshParserCDC :
		IMeshParserIndexed<PositionNormalTexturedVertex, short>,
		IMeshParserIndexed<PositionColorTexturedVertex, short>,
		IMeshParserIndexed<Position2Color2TexturedVertex, short>
	{
		CDC.DataFile _dataFile;
		CDC.IModel _cdcModel;
		CDC.Tree _cdcGroup;
		List<int> _vertexList = new List<int>();
		List<int> _indexList = new List<int>();
		public List<SubMesh> SubMeshes { get; } = new List<SubMesh>();
		public Mesh Mesh;

		public MeshParserCDC(string objectName, CDC.DataFile dataFile)
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
				if (_dataFile.Asset == CDC.Asset.Unit)
				{
					//Mesh = new MeshPCT(this);
					Mesh = new MeshMorphingUnit(resource, this);
				}
				else
				{
					Mesh = new MeshPNT(resource, this);
				}
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

		public void FillVertex(int v, out PositionNormalTexturedVertex vertex)
		{
			ref CDC.Vertex exVertex = ref _cdcGroup.mesh.vertices[_vertexList[v]];
			CDC.Geometry exGeometry = exVertex.isExtraGeometry ? _cdcModel.ExtraGeometry : _cdcModel.Geometry;

			vertex.Position = new SlimDX.Vector3()
			{
				X = 0.01f * exGeometry.PositionsPhys[exVertex.positionID].x,
				Y = 0.01f * exGeometry.PositionsPhys[exVertex.positionID].z,
				Z = 0.01f * exGeometry.PositionsPhys[exVertex.positionID].y
			};

			vertex.Normal = new SlimDX.Vector3()
			{
				X = exGeometry.Normals[exVertex.normalID].x,
				Y = exGeometry.Normals[exVertex.normalID].z,
				Z = exGeometry.Normals[exVertex.normalID].y
			};
			vertex.Normal.Normalize();

			vertex.TextureCoordinates = new SlimDX.Vector2()
			{
				X = exGeometry.UVs[exVertex.UVID].u,
				Y = exGeometry.UVs[exVertex.UVID].v
			};
		}

		public void FillVertex(int v, out PositionColorTexturedVertex vertex)
		{
			ref CDC.Vertex exVertex = ref _cdcGroup.mesh.vertices[_vertexList[v]];
			CDC.Geometry exGeometry = exVertex.isExtraGeometry ? _cdcModel.ExtraGeometry : _cdcModel.Geometry;

			vertex.Position = new SlimDX.Vector3()
			{
				X = 0.01f * exGeometry.PositionsPhys[exVertex.positionID].x,
				Y = 0.01f * exGeometry.PositionsPhys[exVertex.positionID].z,
				Z = 0.01f * exGeometry.PositionsPhys[exVertex.positionID].y
			};

			vertex.Color = new SlimDX.Color3()
			{
				//Alpha = ((_srModel.Colours[vertex.colourID] & 0xFF000000) >> 24) / 255.0f,
				Red = ((exGeometry.Colours[exVertex.colourID] & 0x00FF0000) >> 16) / 255.0f,
				Green = ((exGeometry.Colours[exVertex.colourID] & 0x0000FF00) >> 8) / 255.0f,
				Blue = ((exGeometry.Colours[exVertex.colourID] & 0x000000FF) >> 0) / 255.0f
			};

			vertex.TextureCoordinates = new SlimDX.Vector2()
			{
				X = exGeometry.UVs[exVertex.UVID].u,
				Y = exGeometry.UVs[exVertex.UVID].v
			};
		}

		public void FillVertex(int v, out Position2Color2TexturedVertex vertex)
		{
			ref CDC.Vertex exVertex = ref _cdcGroup.mesh.vertices[_vertexList[v]];
			CDC.Geometry exGeometry = exVertex.isExtraGeometry ? _cdcModel.ExtraGeometry : _cdcModel.Geometry;

			vertex.Position0 = new SlimDX.Vector3()
			{
				X = 0.01f * exGeometry.PositionsPhys[exVertex.positionID].x,
				Y = 0.01f * exGeometry.PositionsPhys[exVertex.positionID].z,
				Z = 0.01f * exGeometry.PositionsPhys[exVertex.positionID].y
			};

			vertex.Position1 = new SlimDX.Vector3()
			{
				X = 0.01f * exGeometry.PositionsAltPhys[exVertex.positionID].x,
				Y = 0.01f * exGeometry.PositionsAltPhys[exVertex.positionID].z,
				Z = 0.01f * exGeometry.PositionsAltPhys[exVertex.positionID].y
			};

			vertex.Color0 = new SlimDX.Color3()
			{
				//Alpha = ((_srModel.VertexData.Colours[vertex.colourID] & 0xFF000000) >> 24) / 255.0f,
				Red = ((exGeometry.Colours[exVertex.colourID] & 0x00FF0000) >> 16) / 255.0f,
				Green = ((exGeometry.Colours[exVertex.colourID] & 0x0000FF00) >> 8) / 255.0f,
				Blue = ((exGeometry.Colours[exVertex.colourID] & 0x000000FF) >> 0) / 255.0f
			};

			vertex.Color1 = new SlimDX.Color3()
			{
				//Alpha = ((_srModel.ColoursAlt[vertex.colourID] & 0xFF000000) >> 24) / 255.0f,
				Red = ((exGeometry.ColoursAlt[exVertex.colourID] & 0x00FF0000) >> 16) / 255.0f,
				Green = ((exGeometry.ColoursAlt[exVertex.colourID] & 0x0000FF00) >> 8) / 255.0f,
				Blue = ((exGeometry.ColoursAlt[exVertex.colourID] & 0x000000FF) >> 0) / 255.0f
			};

			vertex.TextureCoordinates = new SlimDX.Vector2()
			{
				X = exGeometry.UVs[exVertex.UVID].u,
				Y = exGeometry.UVs[exVertex.UVID].v
			};
		}

		public int IndexCount { get { return _vertexList.Count; } }

		public void FillIndex(int i, out short index)
		{
			index = (short)_indexList[i];
		}
	}
}
