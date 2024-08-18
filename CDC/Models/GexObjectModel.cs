using System;
using System.IO;
using TPages = BenLincoln.TheLostWorlds.CDTextures.PSXTextureDictionary;

namespace CDC
{
	public class GexObjectModel : GexModel
	{
		public GexObjectModel(BinaryReader reader, DataFile dataFile, UInt32 dataStart, UInt32 modelData, String modelName, Platform ePlatform, UInt32 version, TPages tPages)
			: base(reader, dataFile, dataStart, modelData, modelName, ePlatform, version, tPages)
		{
			reader.BaseStream.Position = _modelData;
			_vertexCount = reader.ReadUInt16();
			reader.BaseStream.Position += 0x02;
			_polygonCount = reader.ReadUInt16();
			_boneCount = reader.ReadUInt16();
			_vertexStart = reader.ReadUInt32();
			_vertexScale.x = 1.0f;
			_vertexScale.y = 1.0f;
			_vertexScale.z = 1.0f;
			// Vertex colours here? Objects have them in Gex?
			reader.BaseStream.Position += 0x08;
			_polygonStart = reader.ReadUInt32();
			_boneStart = reader.ReadUInt32();
			_materialStart = reader.ReadUInt32();
			_materialCount = 0;
			_groupCount = 1;

			_trees = new Tree[_groupCount];
		}

		protected override void ReadVertex(BinaryReader reader, int v, ExportOptions options)
		{
			base.ReadVertex(reader, v, options);

			_geometry.PositionsPhys[v] = _geometry.PositionsRaw[v];
			_geometry.PositionsAltPhys[v] = _geometry.PositionsPhys[v];

			_geometry.Vertices[v].normalID = reader.ReadUInt16();
		}

		protected override void ReadVertices(BinaryReader reader, ExportOptions options)
		{
			base.ReadVertices(reader, options);

			ReadArmature(reader);
			ApplyArmature();
		}

		protected virtual void ReadArmature(BinaryReader reader)
		{
			if (_boneStart == 0 || _boneCount == 0) return;

			reader.BaseStream.Position = _boneStart;
			_bones = new Bone[_boneCount];
			_bones = new Bone[_boneCount];
			for (UInt16 b = 0; b < _boneCount; b++)
			{
				// Get the bone data
				reader.BaseStream.Position += 8;
				_bones[b].vFirst = reader.ReadUInt16();
				_bones[b].vLast = reader.ReadUInt16();
				_bones[b].localPos.x = (float)reader.ReadInt16();
				_bones[b].localPos.y = (float)reader.ReadInt16();
				_bones[b].localPos.z = (float)reader.ReadInt16();
				_bones[b].parentID1 = reader.ReadUInt16();

				// Combine this bone with it's ancestors is there are any
				if ((_bones[b].vFirst != 0xFFFF) && (_bones[b].vLast != 0xFFFF))
				{
					for (UInt16 ancestorID = b; ancestorID != 0xFFFF;)
					{
						_bones[b].worldPos += _bones[ancestorID].localPos;
						if (_bones[ancestorID].parentID1 == ancestorID) break;
						ancestorID = _bones[ancestorID].parentID1;
					}
				}
				reader.BaseStream.Position += 4;
			}
			return;
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

		protected virtual void ReadPolygon(BinaryReader reader, int p, ExportOptions options)
		{
			UInt32 uPolygonPosition = (UInt32)reader.BaseStream.Position;

			_polygons[p].v1 = _geometry.Vertices[reader.ReadUInt16()];
			_polygons[p].v2 = _geometry.Vertices[reader.ReadUInt16()];
			_polygons[p].v3 = _geometry.Vertices[reader.ReadUInt16()];

			_polygons[p].material = new Material();
			_polygons[p].material.visible = true;
			_polygons[p].material.textureUsed = false;
			reader.ReadByte();
			_polygons[p].material.polygonFlags = reader.ReadByte();

			if ((_polygons[p].material.polygonFlags & 0x0002) == 0x0002)
			{
				_polygons[p].material.textureUsed = true;
			}

			if (_polygons[p].material.textureUsed)
			{
				UInt32 materialPosition = reader.ReadUInt32();

				reader.BaseStream.Position = materialPosition;
				ReadMaterial(reader, p, options);

				reader.BaseStream.Position += 0x02;

				_polygons[p].material.colour = reader.ReadUInt32() | 0xFF000000;
			}
			else
			{
				_polygons[p].material.colour = reader.ReadUInt32() | 0xFF000000;
			}

			Utility.FlipRedAndBlue(ref _polygons[p].material.colour);

			reader.BaseStream.Position = uPolygonPosition + 0x0C;
		}

		protected override void ReadPolygons(BinaryReader reader, ExportOptions options)
		{
			if (_polygonStart == 0 || _polygonCount == 0)
			{
				return;
			}

			reader.BaseStream.Position = _polygonStart;

			for (UInt16 p = 0; p < _polygonCount; p++)
			{
				ReadPolygon(reader, p, options);
			}

			ProcessPolygons(options);

			for (UInt32 t = 0; t < _groupCount; t++)
			{
				_trees[t] = new Tree();
				_trees[t].mesh = new Mesh();
				_trees[t].mesh.indexCount = _indexCount;
				_trees[t].mesh.polygonCount = _polygonCount;
				_trees[t].mesh.polygons = _polygons;
				_trees[t].mesh.vertices = _geometry.Vertices;

				_trees[t].mesh.vertices = new Vertex[_indexCount];
				for (UInt16 poly = 0; poly < _polygonCount; poly++)
				{
					_trees[t].mesh.vertices[(3 * poly) + 0] = _polygons[poly].v1;
					_trees[t].mesh.vertices[(3 * poly) + 1] = _polygons[poly].v2;
					_trees[t].mesh.vertices[(3 * poly) + 2] = _polygons[poly].v3;
				}
			}
		}
	}
}