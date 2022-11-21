using System;
using System.IO;
using System.Collections.Generic;

namespace CDC
{
	public class DefianceObjectModel : DefianceModel
	{
		protected uint _colourStart;

		public DefianceObjectModel(BinaryReader reader, DataFile dataFile, uint dataStart, uint modelData, String modelName, Platform ePlatform, uint version)
			: base(reader, dataFile, dataStart, modelData, modelName, ePlatform, version)
		{
			reader.BaseStream.Position = _modelData + 0x04;
			uint uBoneCount1 = reader.ReadUInt32();
			uint uBoneCount2 = reader.ReadUInt32();
			_boneCount = uBoneCount1 + uBoneCount2;
			_boneStart = reader.ReadUInt32();
			_vertexScale.x = reader.ReadSingle();
			_vertexScale.y = reader.ReadSingle();
			_vertexScale.z = reader.ReadSingle();
			reader.BaseStream.Position += 0x04;
			_vertexCount = reader.ReadUInt32();
			_vertexStart = _dataStart + reader.ReadUInt32();
			reader.BaseStream.Position += 0x08;
			_polygonCount = 0; // reader.ReadUInt32();
			_polygonStart = 0; // _dataStart + reader.ReadUInt32();
			reader.BaseStream.Position += 0x28;
			_materialStart = _dataStart + reader.ReadUInt32();
			reader.BaseStream.Position += 0x04;
			//_materialStart = 0; // ^^Whatever that was, it's a dword and then an array of shorts. 
			_materialCount = 0;
			_colourStart = _dataStart + reader.ReadUInt32();
			_groupCount = 1;

			_trees = new Tree[_groupCount];
		}

		protected override void ReadTypeAVertex(BinaryReader reader, int v, ExportOptions options)
		{
			base.ReadTypeAVertex(reader, v, options);

			_geometry.PositionsPhys[v] = _geometry.PositionsRaw[v] * _vertexScale;
			_geometry.PositionsAltPhys[v] = _geometry.PositionsPhys[v];

			_geometry.Vertices[v].normalID = reader.ReadByte();
			reader.BaseStream.Position += 0x01;

			reader.BaseStream.Position += 0x02; // boneID

			_geometry.Vertices[v].UVID = v;

			ushort vU = reader.ReadUInt16();
			ushort vV = reader.ReadUInt16();

			_geometry.UVs[v].u = Utility.BizarreFloatToNormalFloat(vU);
			_geometry.UVs[v].v = Utility.BizarreFloatToNormalFloat(vV);
		}

		protected override void ReadTypeAVertices(BinaryReader reader, ExportOptions options)
		{
			base.ReadTypeAVertices(reader, options);

			reader.BaseStream.Position = _colourStart;
			for (ushort v = 0; v < _vertexCount; v++)
			{
				_geometry.Colours[v] = reader.ReadUInt32();
				_geometry.ColoursAlt[v] = _geometry.Colours[v];
			}

			ReadArmature(reader);
			ApplyArmature();
		}

		protected virtual void ReadArmature(BinaryReader reader)
		{
			if (_boneStart == 0 || _boneCount == 0) return;

			reader.BaseStream.Position = _boneStart;
			_bones = new Bone[_boneCount];
			for (ushort b = 0; b < _boneCount; b++)
			{
				// Get the bone data
				_bones[b].localPos.x = reader.ReadSingle();
				_bones[b].localPos.y = reader.ReadSingle();
				_bones[b].localPos.z = reader.ReadSingle();

				float unknown = reader.ReadSingle();
				_bones[b].flags = reader.ReadUInt32();

				_bones[b].vFirst = reader.ReadUInt16();
				_bones[b].vLast = reader.ReadUInt16();

				_bones[b].parentID1 = reader.ReadUInt16();
				_bones[b].parentID2 = reader.ReadUInt16();

				//if (parent1 != 0xFFFF && parent2 != 0xFFFF &&
				//    parent2 != 0)
				if (_bones[b].flags == 8)
				{
					_bones[b].parentID1 = _bones[b].parentID2;
				}

				reader.BaseStream.Position += 0x04;
			}

			for (ushort b = 0; b < _boneCount; b++)
			{
				// Combine this bone with it's ancestors if there are any
				if ((_bones[b].vFirst != 0xFFFF) && (_bones[b].vLast != 0xFFFF))
				{
					//for (ushort ancestorID = b; ancestorID != 0xFFFF; )
					//{
					//    _bones[b].worldPos += _bones[ancestorID].localPos;
					//    if (_bones[ancestorID].parentID1 == ancestorID) break;
					//    ancestorID = _bones[ancestorID].parentID1;
					//}

					_bones[b].worldPos = CombineParent(b);
				}
			}
			return;
		}

		protected Vector CombineParent(ushort bone)
		{
			if (bone == 0xFFFF)
			{
				return new Vector(0.0f, 0.0f, 0.0f);
			}

			Vector vector1 = CombineParent(_bones[bone].parentID1);
			Vector vector2 = CombineParent(_bones[bone].parentID2);
			Vector vector3 = _bones[bone].localPos;
			vector3 += vector1;
			//vector3 += vector2;
			return vector3;
		}

		protected virtual void ApplyArmature()
		{
			if ((_vertexStart == 0 || _vertexCount == 0) ||
				(_boneStart == 0 || _boneCount == 0)) return;

			for (ushort b = 0; b < _boneCount; b++)
			{
				if ((_bones[b].vFirst != 0xFFFF) && (_bones[b].vLast != 0xFFFF))
				{
					for (ushort v = _bones[b].vFirst; v <= _bones[b].vLast; v++)
					{
						_geometry.PositionsPhys[v] += _bones[b].worldPos;
						_geometry.Vertices[v].boneID = b;
					}
				}
			}
			return;
		}

		protected override void ReadPolygons(BinaryReader reader, ExportOptions options)
		{
			if (_materialStart == 0)
			{
				return;
			}

			List<DefianceTriangleList> triangleListList = new List<DefianceTriangleList>();
			uint materialPosition = _materialStart;
			_groupCount = 0;
			while (materialPosition != 0)
			{
				reader.BaseStream.Position = materialPosition;
				DefianceTriangleList triangleList = new DefianceTriangleList();

				if (ReadTriangleList(reader, ref triangleList))
				{
					triangleListList.Add(triangleList);
					_polygonCount += triangleList.polygonCount;

					if ((uint)triangleList.groupID > _groupCount)
					{
						_groupCount = triangleList.groupID;
					}
				}

				_materialsList.Add(triangleList.material);

				materialPosition = triangleList.next;
			}

			_materialCount = (uint)_materialsList.Count;

			_groupCount++;
			_trees = new Tree[_groupCount];
			for (uint t = 0; t < _groupCount; t++)
			{
				_trees[t] = new Tree();
				_trees[t].mesh = new Mesh();

				foreach (DefianceTriangleList triangleList in triangleListList)
				{
					if (t == (uint)triangleList.groupID)
					{
						_trees[t].mesh.polygonCount += triangleList.polygonCount;
					}
				}

				_trees[t].mesh.indexCount = _trees[t].mesh.polygonCount * 3;
				_trees[t].mesh.polygons = new Polygon[_trees[t].mesh.polygonCount];
				_trees[t].mesh.vertices = new Vertex[_trees[t].mesh.indexCount];
			}

			for (uint t = 0; t < _groupCount; t++)
			{
				uint tp = 0;
				foreach (DefianceTriangleList triangleList in triangleListList)
				{
					if (t != (uint)triangleList.groupID)
					{
						continue;
					}

					reader.BaseStream.Position = triangleList.polygonStart;
					for (int pl = 0; pl < triangleList.polygonCount; pl++)
					{
						_trees[t].mesh.polygons[tp].v1 = _geometry.Vertices[reader.ReadUInt16()];
						_trees[t].mesh.polygons[tp].v2 = _geometry.Vertices[reader.ReadUInt16()];
						_trees[t].mesh.polygons[tp].v3 = _geometry.Vertices[reader.ReadUInt16()];
						_trees[t].mesh.polygons[tp].material = triangleList.material;
						tp++;
					}
				}

				for (ushort poly = 0; poly < _trees[t].mesh.polygonCount; poly++)
				{
					_trees[t].mesh.vertices[(3 * poly) + 0] = _trees[t].mesh.polygons[poly].v1;
					_trees[t].mesh.vertices[(3 * poly) + 1] = _trees[t].mesh.polygons[poly].v2;
					_trees[t].mesh.vertices[(3 * poly) + 2] = _trees[t].mesh.polygons[poly].v3;
				}
			}

			_polygons = new Polygon[_polygonCount];
			uint p = 0;
			foreach (DefianceTriangleList triangleList in triangleListList)
			{
				reader.BaseStream.Position = triangleList.polygonStart;
				for (int pl = 0; pl < triangleList.polygonCount; pl++)
				{
					_polygons[p].v1 = _geometry.Vertices[reader.ReadUInt16()];
					_polygons[p].v2 = _geometry.Vertices[reader.ReadUInt16()];
					_polygons[p].v3 = _geometry.Vertices[reader.ReadUInt16()];
					_polygons[p].material = triangleList.material;
					p++;
				}
			}
		}

		protected virtual bool ReadTriangleList(BinaryReader reader, ref DefianceTriangleList triangleList)
		{
			triangleList.polygonCount = (uint)reader.ReadUInt16() / 3;
			triangleList.groupID = reader.ReadUInt16(); // Used by MON_SetAccessories and INSTANCE_UnhideAllDrawGroups
			ushort tpageid = reader.ReadUInt16();
			ushort xWord1 = reader.ReadUInt16();
			uint xDWord0 = reader.ReadUInt32();
			uint xDWord1 = reader.ReadUInt32();
			triangleList.material = new Material();
			triangleList.material.visible = ((xWord1 & 0xFF00) == 0);
			triangleList.material.textureID = (ushort)(tpageid & 0x0FFF);
			triangleList.material.colour = 0xFFFFFFFF;
			if (triangleList.material.textureID > 0)
			{
				triangleList.material.textureUsed = true;
			}
			else
			{
				triangleList.material.textureUsed = false;
			}
			triangleList.next = reader.ReadUInt32();
			triangleList.polygonStart = (uint)reader.BaseStream.Position;

			if (triangleList.polygonCount == 0)
			{
				triangleList.next = 0;
			}

			return (triangleList.material.visible);
		}
	}
}