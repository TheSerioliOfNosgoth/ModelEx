using System;
using System.IO;
using TPages = BenLincoln.TheLostWorlds.CDTextures.PSXTextureDictionary;

namespace CDC
{
	public class SR1ObjectModel : SR1Model
	{
		enum PolygonFlags : byte
		{
			TextureUsed = 0x02,
			Emissive = 0x08,
			Hidden0 = 0x10,
		}

		enum TextureAttributes : ushort
		{
			AlphaMaskedTerrain = 0x0010,
			TranslucentTerrain = 0x0040,
			Translucent0 = 0x2000,
			Emmisive = 0x8000,
		}

		public SR1ObjectModel(BinaryReader reader, DataFile dataFile, UInt32 dataStart, UInt32 modelData, String modelName, Platform ePlatform, UInt32 version, TPages tPages)
			: base(reader, dataFile, dataStart, modelData, modelName, ePlatform, version, tPages)
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

		protected override void ReadPolygon(BinaryReader reader, int p, ExportOptions options)
		{
			uint polygonPosition = (uint)reader.BaseStream.Position;

			// struct _MFace
			#region Read _MFace

			// struct _Face face
			int v1 = reader.ReadUInt16();
			int v2 = reader.ReadUInt16();
			int v3 = reader.ReadUInt16();

			// unsigned char normal
			byte normal = reader.ReadByte();
			// unsigned char flags
			byte flags = reader.ReadByte();
			// long color;
			uint color = reader.ReadUInt32();

			#endregion

			Material material = new Material();
			material.visible = true;
			material.textureUsed = false;
			material.isTranslucent = false;
			material.isEmissive = false;
			material.UseAlphaMask = true;
			material.polygonFlags = flags;
			material.sortPush = 0;
			material.colour = color;

			_polygons[p].material = material;
			_polygons[p].materialOffset = color;
			_polygons[p].v1 = _geometry.Vertices[v1];
			_polygons[p].v2 = _geometry.Vertices[v2];
			_polygons[p].v3 = _geometry.Vertices[v3];
			_polygons[p].normal = normal;

			// Unless the user has explicitly requested distinct materials for each flag, remove use of anything ignored at this level
			if (!options.DistinctMaterialsForAllFlags)
			{
				material.polygonFlagsUsedMask &= (byte)(PolygonFlags.TextureUsed | PolygonFlags.Emissive | PolygonFlags.Hidden0);
			}

			reader.BaseStream.Position = polygonPosition + 0x0C;
		}

		protected override void ProcessPolygon(int p, ExportOptions options)
		{
			ref Polygon polygon = ref _polygons[p];
			ref Material material = ref polygon.material;

			if ((material.polygonFlags & (byte)PolygonFlags.TextureUsed) != 0)
			{
				material.textureUsed = true;
			}

			if ((material.polygonFlags & (byte)PolygonFlags.Emissive) != 0)
			{
				material.isEmissive = true;
			}

			if ((material.polygonFlags & (byte)PolygonFlags.Hidden0) != 0)
			{
				material.visible = false;
			}

			// alphamasked terrain
			if ((material.textureAttributes & (ushort)TextureAttributes.AlphaMaskedTerrain) != 0)
			{
				material.UseAlphaMask = true;
			}

			// translucent terrain, e.g. water, glass
			if ((material.textureAttributes & (ushort)TextureAttributes.TranslucentTerrain) != 0)
			{
				material.isTranslucent = true;
			}

			if ((material.textureAttributes & (ushort)TextureAttributes.Translucent0) != 0)
			{
				material.isTranslucent = true;
			}

			// lighting effects? i.e. invisible, animated polygon that only affects vertex colours?
			if ((material.textureAttributes & (ushort)TextureAttributes.Emmisive) != 0)
			{
				material.isEmissive = true;
			}

			if (material.isTranslucent)
			{
				material.opacity = CDC.Material.OPACITY_TRANSLUCENT;
			}

			if (material.isEmissive)
			{
				material.emissivity = 1.0f;
			}

			if (!material.textureUsed)
			{
				polygon.materialOffset = 0xFFFFFFFF;
			}

			if (!material.visible)
			{
				material.textureUsed = false;
			}

			Utility.FlipRedAndBlue(ref material.colour);
			material.colour |= 0xFF000000;
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

			ProcessPolygons(reader, options);

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

		protected override void ReadMaterial(BinaryReader reader, int p, ExportOptions options)
		{
			ref Polygon polygon = ref _polygons[p];
			ref Material material = ref polygon.material;

			if (!material.textureUsed)
			{
				return;
			}

			UInt32 materialPosition = _dataStart + polygon.materialOffset;
			if ((((materialPosition - _materialStart) % 0x10) != 0) &&
				 ((materialPosition - _materialStart) % 0x18) == 0)
			{
				_platform = Platform.Dreamcast;
			}

			reader.BaseStream.Position = materialPosition;
			base.ReadMaterial(reader, p, options);

			if (_platform == Platform.Dreamcast)
			{
				reader.BaseStream.Position += 0x06;
			}
			else
			{
				reader.BaseStream.Position += 0x02;
			}

			material.colour = reader.ReadUInt32();
			Utility.FlipRedAndBlue(ref material.colour);
			material.colour |= 0xFF000000;
		}
	}
}