using System;
using System.IO;
using System.Collections.Generic;

namespace CDC.Objects.Models
{
	public class TRLObjectModel : TRLModel
	{
		protected UInt32 m_uColourStart;

		protected TRLObjectModel(BinaryReader reader, UInt32 dataStart, UInt32 modelData, String strModelName, Platform ePlatform, UInt32 version)
			: base(reader, dataStart, modelData, strModelName, ePlatform, version)
		{
			reader.BaseStream.Position = _modelData + 0x04;
			UInt32 uBoneCount1 = reader.ReadUInt32();
			UInt32 uBoneCount2 = reader.ReadUInt32();
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
			m_uColourStart = _dataStart + reader.ReadUInt32();
			_groupCount = 1;

			_trees = new Tree[_groupCount];
		}

		public static TRLObjectModel Load(BinaryReader reader, UInt32 dataStart, UInt32 modelData, String strModelName, Platform ePlatform, UInt16 usIndex, UInt32 version, CDC.Objects.ExportOptions options)
		{
			reader.BaseStream.Position = modelData + (0x00000004 * usIndex);
			modelData = dataStart + reader.ReadUInt32();
			reader.BaseStream.Position = modelData;
			TRLObjectModel xModel = new TRLObjectModel(reader, dataStart, modelData, strModelName, ePlatform, version);
			xModel.ReadData(reader, options);
			return xModel;
		}

		protected override void ReadTypeAVertex(BinaryReader reader, int v, CDC.Objects.ExportOptions options)
		{
			base.ReadTypeAVertex(reader, v, options);

			_geometry.PositionsPhys[v] = _geometry.PositionsRaw[v] * _vertexScale;
			_geometry.PositionsAltPhys[v] = _geometry.PositionsPhys[v];

			_geometry.Vertices[v].normalID = reader.ReadByte();
			reader.BaseStream.Position += 0x03;

			_geometry.Vertices[v].UVID = v;

			UInt16 vU = reader.ReadUInt16();
			UInt16 vV = reader.ReadUInt16();

			_geometry.UVs[v].u = Utility.BizarreFloatToNormalFloat(vU);
			_geometry.UVs[v].v = Utility.BizarreFloatToNormalFloat(vV);
		}

		protected override void ReadTypeAVertices(BinaryReader reader, CDC.Objects.ExportOptions options)
		{
			base.ReadTypeAVertices(reader, options);

			reader.BaseStream.Position = m_uColourStart;
			for (UInt16 v = 0; v < _vertexCount; v++)
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
			for (UInt16 b = 0; b < _boneCount; b++)
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

			for (UInt16 b = 0; b < _boneCount; b++)
			{
				// Combine this bone with it's ancestors if there are any
				if ((_bones[b].vFirst != 0xFFFF) && (_bones[b].vLast != 0xFFFF))
				{
					//for (UInt16 ancestorID = b; ancestorID != 0xFFFF; )
					//{
					//    m_axBones[b].worldPos += m_axBones[ancestorID].localPos;
					//    if (m_axBones[ancestorID].parentID1 == ancestorID) break;
					//    ancestorID = m_axBones[ancestorID].parentID1;
					//}

					_bones[b].worldPos = CombineParent(b);
				}
			}
			return;
		}

		protected Vector CombineParent(UInt16 bone)
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

			for (UInt16 b = 0; b < _boneCount; b++)
			{
				if ((_bones[b].vFirst != 0xFFFF) && (_bones[b].vLast != 0xFFFF))
				{
					for (UInt16 v = _bones[b].vFirst; v <= _bones[b].vLast; v++)
					{
						_geometry.PositionsPhys[v] += _bones[b].worldPos;
						_geometry.Vertices[v].boneID = b;
					}
				}
			}
			return;
		}

		protected override void ReadPolygons(BinaryReader reader, CDC.Objects.ExportOptions options)
		{
			if (_materialStart == 0)
			{
				return;
			}

			List<TRLTriangleList> xTriangleListList = new List<TRLTriangleList>();
			UInt32 uMaterialPosition = _materialStart;
			_groupCount = 0;
			while (uMaterialPosition != 0)
			{
				reader.BaseStream.Position = uMaterialPosition;
				TRLTriangleList xTriangleList = new TRLTriangleList();

				if (ReadTriangleList(reader, ref xTriangleList)/* && xTriangleList.m_usGroupID == 0*/)
				{
					xTriangleListList.Add(xTriangleList);
					_polygonCount += xTriangleList.m_uPolygonCount;

					if ((UInt32)xTriangleList.m_usGroupID > _groupCount)
					{
						_groupCount = xTriangleList.m_usGroupID;
					}
				}

				_materialsList.Add(xTriangleList.m_xMaterial);

				uMaterialPosition = xTriangleList.m_uNext;
			}

			_materialCount = (UInt32)_materialsList.Count;

			_groupCount++;
			_trees = new Tree[_groupCount];
			for (UInt32 t = 0; t < _groupCount; t++)
			{
				_trees[t] = new Tree();
				_trees[t].mesh = new Mesh();

				foreach (TRLTriangleList xTriangleList in xTriangleListList)
				{
					if (t == (UInt32)xTriangleList.m_usGroupID)
					{
						_trees[t].mesh.polygonCount += xTriangleList.m_uPolygonCount;
					}
				}

				_trees[t].mesh.indexCount = _trees[t].mesh.polygonCount * 3;
				_trees[t].mesh.polygons = new Polygon[_trees[t].mesh.polygonCount];
				_trees[t].mesh.vertices = new Vertex[_trees[t].mesh.indexCount];
			}

			for (UInt32 t = 0; t < _groupCount; t++)
			{
				UInt32 tp = 0;
				foreach (TRLTriangleList xTriangleList in xTriangleListList)
				{
					if (t != (UInt32)xTriangleList.m_usGroupID)
					{
						continue;
					}

					reader.BaseStream.Position = xTriangleList.m_uPolygonStart;
					for (int pl = 0; pl < xTriangleList.m_uPolygonCount; pl++)
					{
						_trees[t].mesh.polygons[tp].v1 = _geometry.Vertices[reader.ReadUInt16()];
						_trees[t].mesh.polygons[tp].v2 = _geometry.Vertices[reader.ReadUInt16()];
						_trees[t].mesh.polygons[tp].v3 = _geometry.Vertices[reader.ReadUInt16()];
						_trees[t].mesh.polygons[tp].material = xTriangleList.m_xMaterial;
						tp++;
					}
				}

				for (UInt16 poly = 0; poly < _trees[t].mesh.polygonCount; poly++)
				{
					_trees[t].mesh.vertices[(3 * poly) + 0] = _trees[t].mesh.polygons[poly].v1;
					_trees[t].mesh.vertices[(3 * poly) + 1] = _trees[t].mesh.polygons[poly].v2;
					_trees[t].mesh.vertices[(3 * poly) + 2] = _trees[t].mesh.polygons[poly].v3;
				}
			}

			_polygons = new Polygon[_polygonCount];
			UInt32 p = 0;
			foreach (TRLTriangleList xTriangleList in xTriangleListList)
			{
				reader.BaseStream.Position = xTriangleList.m_uPolygonStart;
				for (int pl = 0; pl < xTriangleList.m_uPolygonCount; pl++)
				{
					_polygons[p].v1 = _geometry.Vertices[reader.ReadUInt16()];
					_polygons[p].v2 = _geometry.Vertices[reader.ReadUInt16()];
					_polygons[p].v3 = _geometry.Vertices[reader.ReadUInt16()];
					_polygons[p].material = xTriangleList.m_xMaterial;
					p++;
				}
			}
		}

		protected virtual bool ReadTriangleList(BinaryReader reader, ref TRLTriangleList xTriangleList)
		{
			xTriangleList.m_uPolygonCount = (UInt32)reader.ReadUInt16() / 3;
			xTriangleList.m_usGroupID = reader.ReadUInt16(); // Used by MON_SetAccessories and INSTANCE_UnhideAllDrawGroups
			xTriangleList.m_uPolygonStart = (UInt32)(reader.BaseStream.Position) + 0x10;
			UInt16 xWord0 = reader.ReadUInt16();
			UInt16 xWord1 = reader.ReadUInt16();
			UInt32 xDWord0 = reader.ReadUInt32();
			UInt32 xDWord1 = reader.ReadUInt32();
			xTriangleList.m_xMaterial = new Material();
			xTriangleList.m_xMaterial.visible = ((xWord1 & 0xFF00) == 0);
			xTriangleList.m_xMaterial.textureID = (UInt16)(xWord0 & 0x0FFF);
			xTriangleList.m_xMaterial.colour = 0xFFFFFFFF;
			if (xTriangleList.m_xMaterial.textureID > 0)
			{
				xTriangleList.m_xMaterial.textureUsed = true;
			}
			else
			{
				xTriangleList.m_xMaterial.textureUsed = false;
				//xMaterial.colour = 0x00000000;
			}
			xTriangleList.m_uNext = reader.ReadUInt32();

			if (xTriangleList.m_uPolygonCount == 0)
			{
				xTriangleList.m_uNext = 0;
			}

			return (xTriangleList.m_xMaterial.visible);
		}
	}
}