using System;
using System.IO;
using TPages = BenLincoln.TheLostWorlds.CDTextures.PSXTextureDictionary;

namespace CDC.Objects.Models
{
	public class SR1ObjectModel : SR1Model
	{
		public SR1ObjectModel(BinaryReader reader, UInt32 dataStart, UInt32 modelData, String strModelName, Platform ePlatform, UInt32 version, TPages tPages)
			: base(reader, dataStart, modelData, strModelName, ePlatform, version, tPages)
		{
			readTextureFT3Attributes = false;

			_modelTypePrefix = "o_";
			reader.BaseStream.Position = _modelData;
			_vertexCount = reader.ReadUInt32();
			_vertexStart = _dataStart + reader.ReadUInt32();
			_vertexScale.x = 1.0f;
			_vertexScale.y = 1.0f;
			_vertexScale.z = 1.0f;
			reader.BaseStream.Position += 0x08;
			_polygonCount = reader.ReadUInt32();
			_polygonStart = _dataStart + reader.ReadUInt32();
			_boneCount = reader.ReadUInt32();
			_boneStart = _dataStart + reader.ReadUInt32();
			reader.BaseStream.Position += 0x10;
			_materialStart = _dataStart + reader.ReadUInt32();
			_materialCount = 0;
			_groupCount = 1;

			_trees = new Tree[_groupCount];
		}

		public static SR1ObjectModel Load(BinaryReader reader, UInt32 dataStart, UInt32 modelData, String strModelName, Platform ePlatform, UInt16 usIndex, UInt32 version, TPages tPages, ExportOptions options)
		{
			long newPosition = modelData + (0x00000004 * usIndex);
			if ((newPosition < 0) || (newPosition > reader.BaseStream.Length))
			{
				Console.WriteLine(string.Format("Error: attempt to read a model with usIndex {0} from a stream with length {1}", usIndex, reader.BaseStream.Length));
				return null;
			}
			reader.BaseStream.Position = newPosition;
			modelData = dataStart + reader.ReadUInt32();
			reader.BaseStream.Position = modelData;
			SR1ObjectModel xModel = new SR1ObjectModel(reader, dataStart, modelData, strModelName, ePlatform, version, tPages);
			xModel.ReadData(reader, options);
			return xModel;
		}

		protected override void ReadVertex(BinaryReader reader, int v, ExportOptions options)
		{
			base.ReadVertex(reader, v, options);

			_geometry.PositionsPhys[v] = _geometry.PositionsRaw[v];
			_geometry.PositionsAltPhys[v] = _geometry.PositionsPhys[v];

			if (_version == SR1File.PROTO_19981025_VERSION)
			{
				_geometry.Vertices[v].normalID = reader.ReadUInt16();
				reader.BaseStream.Position += 4;
			}
			else
			{
				_geometry.Vertices[v].normalID = reader.ReadUInt16();
			}
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

		//protected virtual void ReadPolygon(BinaryReader reader, int p, ExportOptions options)
		//{
		//    UInt32 uPolygonPosition = (UInt32)reader.BaseStream.Position;
		//    // struct _MFace

		//    // struct _Face face
		//    _polygons[p].v1 = _vertices[reader.ReadUInt16()];
		//    _polygons[p].v2 = _vertices[reader.ReadUInt16()];
		//    _polygons[p].v3 = _vertices[reader.ReadUInt16()];

		//    _polygons[p].material = new Material();
		//    _polygons[p].material.visible = true;

		//    _polygons[p].material.textureUsed = (Boolean)(((int)reader.ReadUInt16() & 0x0200) != 0);

		//    //// unsigned char normal
		//    //byte polygonNormal = reader.ReadByte();

		//    //// unsigned char flags
		//    //byte flags = reader.ReadByte();

		//    //_polygons[p].material.textureUsed = true;


		//    if (_polygons[p].material.textureUsed)
		//    {
		//        // WIP
		//        UInt32 materialPosition = _dataStart + reader.ReadUInt32();
		//        if ((((materialPosition - _materialStart) % 0x10) != 0) &&
		//             ((materialPosition - _materialStart) % 0x18) == 0)
		//        {
		//            _platform = Platform.Dreamcast;
		//        }

		//        reader.BaseStream.Position = materialPosition;
		//        ReadMaterial(reader, p, options);

		//        if (_platform == Platform.Dreamcast)
		//        {
		//            reader.BaseStream.Position += 0x06;
		//        }
		//        else
		//        {
		//            reader.BaseStream.Position += 0x02;
		//        }

		//        _polygons[p].material.colour = reader.ReadUInt32();
		//        //_polygons[p].material.colour |= 0xFF000000;   //2019-12-22

		//    }
		//    else
		//    {
		//        _polygons[p].material.colour = reader.ReadUInt32() | 0xFF000000;
		//    }

		//    Utility.FlipRedAndBlue(ref _polygons[p].material.colour);

		//    reader.BaseStream.Position = uPolygonPosition + 0x0C;
		//}

		protected virtual void ReadPolygon(BinaryReader reader, int p, ExportOptions options)
		{
			UInt32 uPolygonPosition = (UInt32)reader.BaseStream.Position;
			// struct _MFace

			// struct _Face face
			_polygons[p].v1 = _geometry.Vertices[reader.ReadUInt16()];
			_polygons[p].v2 = _geometry.Vertices[reader.ReadUInt16()];
			_polygons[p].v3 = _geometry.Vertices[reader.ReadUInt16()];

			_polygons[p].material = new Material();
			_polygons[p].material.visible = true;

			//// unsigned char normal
			byte polygonNormal = reader.ReadByte();

			//// unsigned char flags
			long flagOffset = reader.BaseStream.Position + 2048;
			byte flags = reader.ReadByte();
			//Console.WriteLine(string.Format("0x{0:X2} (@0x{1:X4}, D:{1}\t:::", flags, flagOffset));

			_polygons[p].material.polygonFlags = flags;
			_polygons[p].material.textureUsed = false;

			if (((flags & 0x01) == 0x01))
			{
			}

			if (((flags & 0x02) == 0x02))
			{
				_polygons[p].material.textureUsed = true;
			}

			if (((flags & 0x04) == 0x04))
			{
			}

			if (((flags & 0x08) == 0x08))
			{
				_polygons[p].material.emissivity = 1.0f;
			}

			if (((flags & 0x10) == 0x10))
			{
				_polygons[p].material.visible = false;
			}

			// 20 is not used in any known version of the game
			if (((flags & 0x40) == 0x40))
			{
			}
			// 80 is not used in any known version of the game

			// long color;
			UInt32 colourOrMaterialPosition = reader.ReadUInt32();
			if (_polygons[p].material.textureUsed)
			{
				// this seems to be what the game does
				_polygons[p].material.colour = colourOrMaterialPosition | 0xFF000000;
				_polygons[p].colour = _polygons[p].material.colour;
			}
			else
			{
				_polygons[p].material.colour = colourOrMaterialPosition | 0xFF000000;
				_polygons[p].colour = _polygons[p].material.colour;
			}

			_polygons[p].material.UseAlphaMask = true;

			HandlePolygonInfo(reader, p, options, flags, colourOrMaterialPosition, false);

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

			HandleDebugRendering(options);

			MaterialList xMaterialsList = null;

			for (UInt16 p = 0; p < _polygonCount; p++)
			{
				if (xMaterialsList == null)
				{
					xMaterialsList = new MaterialList(_polygons[p].material);
					_materialsList.Add(_polygons[p].material);
				}
				else
				{
					Material newMaterial = xMaterialsList.AddToList(_polygons[p].material);
					if (_polygons[p].material != newMaterial)
					{
						_polygons[p].material = newMaterial;
					}
					else
					{
						_materialsList.Add(_polygons[p].material);
					}
				}
			}

			_materialCount = (UInt32)_materialsList.Count;

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

		protected override void ReadMaterial(BinaryReader reader, int p, UInt32 colourOrMaterialPosition, ExportOptions options)
		{
			// WIP
			UInt32 materialPosition = _dataStart + colourOrMaterialPosition;
			if ((((materialPosition - _materialStart) % 0x10) != 0) &&
				 ((materialPosition - _materialStart) % 0x18) == 0)
			{
				_platform = Platform.Dreamcast;
			}

			reader.BaseStream.Position = materialPosition;
			base.ReadMaterial(reader, p, colourOrMaterialPosition, options);

			if (_platform == Platform.Dreamcast)
			{
				reader.BaseStream.Position += 0x06;
			}
			else
			{
				reader.BaseStream.Position += 0x02;
			}

			_polygons[p].material.colour = reader.ReadUInt32();
			//_polygons[p].material.colour |= 0xFF000000;   //2019-12-22
		}
	}
}