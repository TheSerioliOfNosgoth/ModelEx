using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace CDC.Objects.Models
{
	public abstract class SRModel
	{
		protected String _name;
		protected string _modelTypePrefix;
		protected uint _version;
		protected Platform _platform;
		protected uint _dataStart;
		protected uint _modelData;
		protected uint _vertexCount;
		protected uint _vertexStart;
		protected uint _polygonCount;
		protected uint _polygonStart;
		protected uint _boneCount;
		protected uint _boneStart;
		protected uint _groupCount;
		protected uint _materialCount;
		protected uint _materialStart;
		protected uint _indexCount { get { return 3 * _polygonCount; } }
		// Vertices are scaled before any bones are applied.
		// Scaling afterwards will break the characters.
		protected Vector _vertexScale;
		protected Geometry _geometry;
		protected Geometry _extraGeometry;
		protected Polygon[] _polygons;
		protected Bone[] _bones;
		protected Tree[] _trees;
		protected Material[] _materials;
		protected List<Material> _materialsList;

		public String Name { get { return _name; } }
		public string ModelTypePrefix { get { return _modelTypePrefix; } }
		public uint PolygonCount { get { return _polygonCount; } }
		public Polygon[] Polygons { get { return _polygons; } }
		public uint IndexCount { get { return _indexCount; } }
		public Geometry Geometry { get { return _geometry; } }
		public Geometry ExtraGeometry { get { return _extraGeometry; } }
		public Bone[] Bones { get { return _bones; } }
		public uint GroupCount { get { return _groupCount; } }
		public Tree[] Groups { get { return _trees; } }
		public uint MaterialCount { get { return _materialCount; } }
		public Material[] Materials { get { return _materials; } }
		public Platform Platform { get { return _platform; } }

		protected SRModel(BinaryReader reader, uint dataStart, uint modelData, String strModelName, Platform ePlatform, uint version)
		{
			_name = strModelName;
			_modelTypePrefix = "";
			_platform = ePlatform;
			_version = version;
			_dataStart = dataStart;
			_modelData = modelData;
			_vertexCount = 0;
			_vertexStart = 0;
			_polygonCount = 0;
			_polygonStart = 0;
			_vertexScale.x = 1.0f;
			_vertexScale.y = 1.0f;
			_vertexScale.z = 1.0f;
			_geometry = new Geometry();
			_extraGeometry = new Geometry();
			_materialsList = new List<Material>();
		}

		public static string GetTextureNameDefault(string objectName, int textureID)
		{
			String textureName = string.Format("{0}_{1:X4}", objectName.TrimEnd(new char[] { '_' }).ToLower(), textureID);
			return textureName;
		}

		public static string GetPlayStationTextureNameDefault(string objectName, int textureID)
		{
			return GetTextureNameDefault(objectName, textureID);
		}

		public static string GetPlayStationTextureNameWithCLUT(string objectName, int textureID, ushort clut)
		{
			String textureName = string.Format("{0}_{1:X4}_{2:X4}", objectName.TrimEnd(new char[] { '_' }).ToLower(), textureID, clut);
			return textureName;
		}

		public static string GetSoulReaverPCOrDreamcastTextureName(string objectName, int textureID)
		{
			return GetTextureNameDefault(objectName, textureID);
		}

		public static string GetPS2TextureName(string objectName, int textureID)
		{
			return GetTextureNameDefault(objectName, textureID);
		}

		public static String GetTextureName(SRModel srModel, int materialIndex, CDC.Objects.ExportOptions options)
		{
			CDC.Material material = srModel.Materials[materialIndex];
			String textureName = "";
			if (material.textureUsed)
			{
				if (srModel is GexModel)
				{
					if (options.UseEachUniqueTextureCLUTVariation)
					{
						textureName = GetPlayStationTextureNameWithCLUT(srModel.Name, material.textureID, material.clutValue);
					}
					else
					{
						textureName = GetPlayStationTextureNameDefault(srModel.Name, material.textureID);
					}
				}
				else if (srModel is SR1Model)
				{
					if (srModel.Platform == CDC.Platform.PSX)
					{
						if (options.UseEachUniqueTextureCLUTVariation)
						{
							textureName = GetPlayStationTextureNameWithCLUT(srModel.Name, material.textureID, material.clutValue);
						}
						else
						{
							textureName = GetPlayStationTextureNameDefault(srModel.Name, material.textureID);
						}
					}
					else
					{
						textureName = GetSoulReaverPCOrDreamcastTextureName(srModel.Name, material.textureID);
					}
				}
				else if (srModel is SR2Model || srModel is DefianceModel || srModel is TRLModel)
				{
					textureName = GetPS2TextureName(srModel.Name, material.textureID);
				}
			}

			return textureName;
		}

		public String GetTextureName(int materialIndex, CDC.Objects.ExportOptions options)
		{
			String textureName = "";
			if (materialIndex >= 0 && materialIndex < MaterialCount)
			{
				Material material = Materials[materialIndex];
				if (material.textureUsed)
				{
					//if (this is SR1Model)
					//{
					//    if (Platform == Platform.PSX)
					//    {
					//        textureName =
					//            Name.TrimEnd(new char[] { '_' }).ToLower() + "_" +
					//            string.Format("{0:D4}", material.textureID);
					//    }
					//    else
					//    {
					//        textureName = string.Format("Texture-{0:D5}", material.textureID);
					//    }
					//}
					//else if (this is SR2Model)
					//{
					//    textureName =
					//        Name.TrimEnd(new char[] { '_' }).ToLower() + "_" +
					//        string.Format("{0:D4}", material.textureID);
					//}
					return GetTextureName(this, materialIndex, options);
				}
			}

			return textureName;
		}

		protected void ColourPolygonFromFlags(int polygonNum, uint flags, uint redBit, uint greenBit, uint blueBit)
		{
			if ((flags & redBit) == redBit)
			{
				_polygons[polygonNum].material.colour |= 0x00FF0000;
			}
			if ((flags & greenBit) == greenBit)
			{
				_polygons[polygonNum].material.colour |= 0x0000FF00;
			}
			if ((flags & blueBit) == blueBit)
			{
				_polygons[polygonNum].material.colour |= 0x000000FF;
			}
		}

		protected uint GetColourFromHash(byte[] hash)
		{
			uint result = 0xFF000000;
			result |= ((uint)hash[0] << 16);
			result |= ((uint)hash[1] << 8);
			result |= ((uint)hash[2]);
			return result;
		}

		protected void ColourPolygonFromHash(int polygonNum, byte[] hash)
		{
			//_polygons[polygonNum].material.colour &= 0xFF00FFFF;
			//_polygons[polygonNum].material.colour |= ((uint)hash[0] << 16);
			//_polygons[polygonNum].material.colour &= 0xFFFF00FF;
			//_polygons[polygonNum].material.colour |= ((uint)hash[1] << 8);
			//_polygons[polygonNum].material.colour &= 0xFFFFFF00;
			//_polygons[polygonNum].material.colour |= ((uint)hash[2]);
			_polygons[polygonNum].material.colour = GetColourFromHash(hash);
		}

		protected byte[] GetHashOfUInt(uint value)
		{
			byte[] valueBytes = new byte[4];
			valueBytes[0] = (byte)((value & 0xFF000000) >> 24);
			valueBytes[1] = (byte)((value & 0x00FF0000) >> 16);
			valueBytes[2] = (byte)((value & 0x0000FF00) >> 8);
			valueBytes[3] = (byte)((value & 0x000000FF));
			byte[] hash = new MD5CryptoServiceProvider().ComputeHash(valueBytes);
			return hash;
		}

		protected void ColourPolygonFromUInt(int polygonNum, uint value)
		{
			ColourPolygonFromHash(polygonNum, GetHashOfUInt(value));
		}

		protected void ColourPolygonFromString(int polygonNum, string value)
		{
			string hashInput = "default";
			if (value != null)
			{
				hashInput = value;
			}
			byte[] valueBytes = System.Text.Encoding.Unicode.GetBytes(hashInput);
			byte[] hash = new MD5CryptoServiceProvider().ComputeHash(valueBytes);
			ColourPolygonFromHash(polygonNum, hash);
		}

		protected virtual void HandleDebugRendering(int p, CDC.Objects.ExportOptions options)
		{
			if (options.RenderMode == RenderMode.Standard ||
				options.RenderMode == RenderMode.Wireframe)
			{
				return;
			}

			_polygons[p].material.textureUsed = false;
			if ((options.MakeAllPolygonsVisible) || (options.MakeAllPolygonsOpaque) || (!options.DiscardNonVisible))
			{
				_polygons[p].material.visible = true;
			}
			//_polygons[p].material.visible = true;
			_polygons[p].material.emissivity = 0.0f;
			if (options.RenderMode == RenderMode.NoTextures)
			{
				//return;
				//_polygons[p].material.colour = |= 0xFF000000;
			}

			//_polygons[p].material.colour = (TransparentMaterial & 0xFF000000) | 0x404040;

			if (options.SetAllPolygonColoursToValue)
			{
				//_polygons[p].material.colour |= 0x0000FF00;
				_polygons[p].material.colour = Utility.FloatARGBToUInt32ARGB(new float[] { options.PolygonColourAlpha, options.PolygonColourRed, options.PolygonColourGreen, options.PolygonColourBlue });
				_polygons[p].material.opacity = options.PolygonColourAlpha;
			}
			else
			{
				_polygons[p].material.colour = 0xFF404040;
				_polygons[p].material.opacity = 0.75f;
			}
			//_polygons[p].v1.colourID = 0;
			//_polygons[p].v2.colourID = 0;
			//_polygons[p].v3.colourID = 0;

			#region Polygon flags
			if (options.RenderMode == RenderMode.DebugPolygonFlagsHash)
			{
				ColourPolygonFromUInt(p, _polygons[p].material.polygonFlags);
			}
			if (options.RenderMode == RenderMode.DebugPolygonFlags1)
			{
				ColourPolygonFromFlags(p, _polygons[p].material.polygonFlags, 0x0001, 0x0002, 0x0004);
				// 0x01 = hidden polygon (portal, collision box, etc.), not sure why sometimes it's this and sometimes 0x40
				// 0x02 = inverted half of quad? It's one triangle out of every quad in a lot of models (but not all)
				// 0x04 = hidden polygon
			}
			if (options.RenderMode == RenderMode.DebugPolygonFlags2)
			{
				ColourPolygonFromFlags(p, _polygons[p].material.polygonFlags, 0x0008, 0x0010, 0x0020);
				// 0x08 = translucent? glowing? - used for the water in tower10 (1999-02-16), lthbeam  (1999-02-16), morlock eyes (1999-05-12), Kain's feet (1999-05-12)
				// 0x10 = glowing? reflective? specular? That Z-depth thing Andrew mentioned? - water in cityout3 is cyan, not used in cathy69, pipes and platforms in intvaly1 are green, used for the columns in tower10 (1999-02-16)
				// 0x20 = translucent - water in cityout3 is cyan, not used in cathy69, water in intvaly1 is blue, lava in lair1 is blue
			}
			if (options.RenderMode == RenderMode.DebugPolygonFlags3)
			{
				ColourPolygonFromFlags(p, _polygons[p].material.polygonFlags, 0x0040, 0x0080, 0x0100);
				// 0x40 = hidden polygon (portal, collision box, etc.), not sure why sometimes it's this and sometimes 0x01
			}
			if (options.RenderMode == RenderMode.DebugPolygonFlagsSoulReaverA)
			{
				byte tempFlags = _polygons[p].material.polygonFlags;
				if ((_polygons[p].material.polygonFlags & 0x40) == 0x40)
				{
					tempFlags |= 0x01;
				}
				ColourPolygonFromFlags(p, _polygons[p].material.polygonFlags, 0x0001, 0x0020, 0x0010);
				// hidden polygons == red
				// transparent/translucent == green
				// glowing? reflective? == blue
			}
			#endregion

			#region Texture
			#region Texture attributes
			if (options.RenderMode == RenderMode.DebugTextureAttributesHash)
			{
				ColourPolygonFromUInt(p, _polygons[p].material.textureAttributes);
			}
			if (options.RenderMode == RenderMode.DebugTextureAttributes1)
			{
				ColourPolygonFromFlags(p, _polygons[p].material.textureAttributes, 0x0001, 0x0002, 0x0004);
			}
			if (options.RenderMode == RenderMode.DebugTextureAttributes2)
			{
				ColourPolygonFromFlags(p, _polygons[p].material.textureAttributes, 0x0008, 0x0010, 0x0020);
				// 0x10 = alphamasked terrain
				// the glass shell around the lighthouse in intvaly1 has this flag
				// tiny squares below the wall sconces in nighta2 (1999-01-23) have this flag too 
				// stair coverings and the bridge in cliff1 (release version) have this flag
				// flags in city2 (NTSC release)
			}
			if (options.RenderMode == RenderMode.DebugTextureAttributes3)
			{
				ColourPolygonFromFlags(p, _polygons[p].material.textureAttributes, 0x0040, 0x0080, 0x0100);
				// 0x40 = translucent terrain, e.g. water, glass
			}
			if (options.RenderMode == RenderMode.DebugTextureAttributes4)
			{
				ColourPolygonFromFlags(p, _polygons[p].material.textureAttributes, 0x0200, 0x0400, 0x0800);
				// 0x0200 = climbable walls? city2 (NTSC release)
			}
			if (options.RenderMode == RenderMode.DebugTextureAttributes5)
			{
				ColourPolygonFromFlags(p, _polygons[p].material.textureAttributes, 0x1000, 0x2000, 0x4000);
				// 0x1000 = parts of the gate in nighta1 (1999-02-16) have this flag
				// 0x2000 = waterfall in intvaly1 (1999-01-23) has this flag - maybe animated texture, or translucency?
				//          Parts of the water in cliff1 have it too
				// 0x4000 = most of the intvaly1 terrain mesh has this flag
			}
			if (options.RenderMode == RenderMode.DebugTextureAttributes6)
			{
				ColourPolygonFromFlags(p, _polygons[p].material.textureAttributes, 0x10000, 0x8000, 0x20000);
				// 0x8000 = lighting effects? i.e. invisible, animated polygon that only affects vertex colours?
			}
			#endregion

			#region Texture attributes A
			if (options.RenderMode == RenderMode.DebugTextureAttributesAHash)
			{
				ColourPolygonFromUInt(p, _polygons[p].material.textureAttributesA);
			}
			if (options.RenderMode == RenderMode.DebugTextureAttributesA1)
			{
				ColourPolygonFromFlags(p, _polygons[p].material.textureAttributesA, 0x0001, 0x0002, 0x0004);
			}
			if (options.RenderMode == RenderMode.DebugTextureAttributesA2)
			{
				ColourPolygonFromFlags(p, _polygons[p].material.textureAttributesA, 0x0008, 0x0010, 0x0020);
			}
			if (options.RenderMode == RenderMode.DebugTextureAttributesA3)
			{
				ColourPolygonFromFlags(p, _polygons[p].material.textureAttributesA, 0x0040, 0x0080, 0x0100);
			}
			if (options.RenderMode == RenderMode.DebugTextureAttributesA4)
			{
				ColourPolygonFromFlags(p, _polygons[p].material.textureAttributesA, 0x0200, 0x0400, 0x0800);
			}
			if (options.RenderMode == RenderMode.DebugTextureAttributesA5)
			{
				ColourPolygonFromFlags(p, _polygons[p].material.textureAttributesA, 0x1000, 0x2000, 0x4000);
			}
			if (options.RenderMode == RenderMode.DebugTextureAttributesA6)
			{
				ColourPolygonFromFlags(p, _polygons[p].material.textureAttributesA, 0x10000, 0x8000, 0x10000);
			}
			#endregion

			#region CLUT

			ushort clutNonRowColumnBits = (ushort)((_polygons[p].material.clutValue & 0xC000) >> 11);
			clutNonRowColumnBits |= (ushort)((_polygons[p].material.clutValue & 0x00E0) >> 5);

			if (options.RenderMode == RenderMode.DebugCLUTNonRowColBitsHash)
			{
				ColourPolygonFromUInt(p, clutNonRowColumnBits);
			}
			if (options.RenderMode == RenderMode.DebugCLUTNonRowColBits1)
			{
				ColourPolygonFromFlags(p, clutNonRowColumnBits, 0x0001, 0x0002, 0x0004);
			}
			if (options.RenderMode == RenderMode.DebugCLUTNonRowColBits2)
			{
				ColourPolygonFromFlags(p, clutNonRowColumnBits, 0x0008, 0x0010, 0x10000);
			}
			if (options.RenderMode == RenderMode.DebugCLUTHash)
			{
				ColourPolygonFromUInt(p, _polygons[p].material.clutValue);
			}
			if (options.RenderMode == RenderMode.DebugCLUT1)
			{
				ColourPolygonFromFlags(p, _polygons[p].material.clutValue, 0x0001, 0x0002, 0x0004);
			}
			if (options.RenderMode == RenderMode.DebugCLUT2)
			{
				ColourPolygonFromFlags(p, _polygons[p].material.clutValue, 0x0008, 0x0010, 0x0020);
			}
			if (options.RenderMode == RenderMode.DebugCLUT3)
			{
				ColourPolygonFromFlags(p, _polygons[p].material.clutValue, 0x0040, 0x0080, 0x0100);
			}
			if (options.RenderMode == RenderMode.DebugCLUT4)
			{
				ColourPolygonFromFlags(p, _polygons[p].material.clutValue, 0x0200, 0x0400, 0x0800);
			}
			if (options.RenderMode == RenderMode.DebugCLUT5)
			{
				ColourPolygonFromFlags(p, _polygons[p].material.clutValue, 0x1000, 0x2000, 0x4000);
			}
			if (options.RenderMode == RenderMode.DebugCLUT6)
			{
				ColourPolygonFromFlags(p, _polygons[p].material.clutValue, 0x10000, 0x8000, 0x10000);
			}
			#endregion

			#region Texture page
			if (options.RenderMode == RenderMode.DebugTexturePageHash)
			{
				ColourPolygonFromUInt(p, _polygons[p].material.texturePage);
			}
			if (options.RenderMode == RenderMode.DebugTexturePageUpper28BitsHash)
			{
				//ColourPolygonFromUInt(p, (_polygons[p].material.texturePage & 0xFFFFFF00));
				ColourPolygonFromUInt(p, (_polygons[p].material.texturePage & 0xFFFFFFF0));
			}
			if (options.RenderMode == RenderMode.DebugTexturePageUpper5BitsHash)
			{
				//ColourPolygonFromUInt(p, (_polygons[p].material.texturePage & 0xFFFFFF00));
				ColourPolygonFromUInt(p, (_polygons[p].material.texturePage & 0xFFFFF800));
			}
			if (options.RenderMode == RenderMode.DebugTexturePage1)
			{
				ColourPolygonFromFlags(p, _polygons[p].material.texturePage, 0x0001, 0x0002, 0x0004);
			}
			if (options.RenderMode == RenderMode.DebugTexturePage2)
			{
				ColourPolygonFromFlags(p, _polygons[p].material.texturePage, 0x0008, 0x0010, 0x0020);
				// 0x0020 == glowing?
			}
			if (options.RenderMode == RenderMode.DebugTexturePage3)
			{
				ColourPolygonFromFlags(p, _polygons[p].material.texturePage, 0x0040, 0x0080, 0x0100);
				// 0x0040 == glowing brightly?
			}
			if (options.RenderMode == RenderMode.DebugTexturePage4)
			{
				ColourPolygonFromFlags(p, _polygons[p].material.texturePage, 0x0200, 0x0400, 0x0800);
			}
			if (options.RenderMode == RenderMode.DebugTexturePage5)
			{
				ColourPolygonFromFlags(p, _polygons[p].material.texturePage, 0x1000, 0x2000, 0x4000);
			}
			if (options.RenderMode == RenderMode.DebugTexturePage6)
			{
				ColourPolygonFromFlags(p, _polygons[p].material.texturePage, 0x10000, 0x8000, 0x10000);
			}
			#endregion
			#endregion

			#region Sort push flags
			if (options.RenderMode == RenderMode.DebugSortPushHash)
			{
				ColourPolygonFromUInt(p, _polygons[p].material.sortPush);
			}
			if (options.RenderMode == RenderMode.DebugSortPushFlags1)
			{
				ColourPolygonFromFlags(p, _polygons[p].material.sortPush, 0x0001, 0x0002, 0x0004);
			}
			if (options.RenderMode == RenderMode.DebugSortPushFlags2)
			{
				ColourPolygonFromFlags(p, _polygons[p].material.sortPush, 0x0008, 0x0010, 0x0020);
			}
			if (options.RenderMode == RenderMode.DebugSortPushFlags3)
			{
				ColourPolygonFromFlags(p, _polygons[p].material.sortPush, 0x0040, 0x0080, 0x0100);
			}
			#endregion

			if (options.RenderMode == RenderMode.AverageVertexAlpha)
			{
				_polygons[p].material.opacity = 0.75f;
				float[] fV1 = Utility.UInt32ARGBToFloatARGB(_geometry.Colours[_polygons[p].v1.colourID]);
				float[] fV2 = Utility.UInt32ARGBToFloatARGB(_geometry.Colours[_polygons[p].v2.colourID]);
				float[] fV3 = Utility.UInt32ARGBToFloatARGB(_geometry.Colours[_polygons[p].v3.colourID]);
				float[] fVAverage = new float[4];
				fVAverage[0] = 1.0f;
				fVAverage[1] = (fV1[0] + fV2[0] + fV3[0]) / 3.0f;
				fVAverage[2] = 1.0f;
				fVAverage[3] = (fV1[0] + fV2[0] + fV3[0]) / 3.0f;
				_polygons[p].material.colour = Utility.FloatARGBToUInt32ARGB(fVAverage);
			}

			if (options.RenderMode == RenderMode.PolygonAlpha)
			{
				float[] fPoly = Utility.UInt32ARGBToFloatARGB(_polygons[p].material.colour);
				float[] fVAverage = new float[4];
				fVAverage[0] = 0.75f;
				fVAverage[1] = fPoly[0];
				fVAverage[2] = 1.0f;
				fVAverage[3] = fPoly[0];
				_polygons[p].material.colour = Utility.FloatARGBToUInt32ARGB(fVAverage);
				_polygons[p].material.opacity = 0.75f;
			}

			if (options.RenderMode == RenderMode.PolygonOpacity)
			{
				float[] fVAverage = new float[4];
				fVAverage[0] = 0.75f;
				fVAverage[1] = _polygons[p].material.opacity;
				fVAverage[2] = 1.0f;
				fVAverage[3] = _polygons[p].material.opacity;
				_polygons[p].material.colour = Utility.FloatARGBToUInt32ARGB(fVAverage);
				_polygons[p].material.opacity = 0.75f;
			}

			if (options.RenderMode == RenderMode.DebugBoneIDHash)
			{
				uint boneIDColourV1 = GetColourFromHash(GetHashOfUInt((uint)_polygons[p].v1.boneID));
				uint boneIDColourV2 = GetColourFromHash(GetHashOfUInt((uint)_polygons[p].v2.boneID));
				uint boneIDColourV3 = GetColourFromHash(GetHashOfUInt((uint)_polygons[p].v3.boneID));
				_geometry.Colours[_polygons[p].v1.colourID] = boneIDColourV1;
				_geometry.Colours[_polygons[p].v2.colourID] = boneIDColourV2;
				_geometry.Colours[_polygons[p].v3.colourID] = boneIDColourV3;
				if ((boneIDColourV1 == boneIDColourV2) && (boneIDColourV2 == boneIDColourV3))
				{
					_polygons[p].material.opacity = 1.0f;
					_polygons[p].material.colour = boneIDColourV1;
				}
				else
				{
					if (options.InterpolatePolygonColoursWhenColouringBasedOnVertices)
					{
						// average the vertex colours and make the polygon translucent to highlight the fact that it's dependent on multiple bones.
						_polygons[p].material.opacity = 0.25f;
						float[] fV1 = Utility.UInt32ARGBToFloatARGB(boneIDColourV1);
						float[] fV2 = Utility.UInt32ARGBToFloatARGB(boneIDColourV2);
						float[] fV3 = Utility.UInt32ARGBToFloatARGB(boneIDColourV3);
						float[] fVAverage = new float[4];
						fVAverage[0] = (fV1[0] + fV2[0] + fV3[0]) / 3.0f;
						fVAverage[1] = (fV1[1] + fV2[1] + fV3[1]) / 3.0f;
						fVAverage[2] = (fV1[2] + fV2[2] + fV3[2]) / 3.0f;
						fVAverage[3] = (fV1[3] + fV2[3] + fV3[3]) / 3.0f;
						_polygons[p].material.colour = Utility.FloatARGBToUInt32ARGB(fVAverage);
					}
					else
					{
						_polygons[p].material.opacity = 0.5f;
						_polygons[p].material.colour = 0xFFFFFFFF;
					}
				}
			}

			#region BSP tree
			if (options.RenderMode == RenderMode.DebugBSPRootTreeNumber)
			{
				ColourPolygonFromUInt(p, unchecked((uint)_polygons[p].rootBSPTreeID));
			}

			if (options.RenderMode == RenderMode.DebugBSPTreeNodeID)
			{
				ColourPolygonFromString(p, _polygons[p].BSPNodeID);
			}

			#region BSP tree root flags
			if (options.RenderMode == RenderMode.DebugBSPRootTreeFlagsHash)
			{
				ColourPolygonFromUInt(p, _polygons[p].material.BSPTreeRootFlags);
			}
			if (options.RenderMode == RenderMode.DebugBSPRootTreeFlags1)
			{
				ColourPolygonFromFlags(p, _polygons[p].material.BSPTreeRootFlags, 0x0001, 0x0002, 0x0004);
			}
			if (options.RenderMode == RenderMode.DebugBSPRootTreeFlags2)
			{
				ColourPolygonFromFlags(p, _polygons[p].material.BSPTreeRootFlags, 0x0008, 0x0010, 0x0020);
			}
			if (options.RenderMode == RenderMode.DebugBSPRootTreeFlags3)
			{
				ColourPolygonFromFlags(p, _polygons[p].material.BSPTreeRootFlags, 0x0040, 0x0080, 0x0100);
			}
			if (options.RenderMode == RenderMode.DebugBSPRootTreeFlags4)
			{
				ColourPolygonFromFlags(p, _polygons[p].material.BSPTreeRootFlags, 0x0200, 0x0400, 0x0800);
			}
			if (options.RenderMode == RenderMode.DebugBSPRootTreeFlags5)
			{
				ColourPolygonFromFlags(p, _polygons[p].material.BSPTreeRootFlags, 0x1000, 0x2000, 0x4000);
			}
			if (options.RenderMode == RenderMode.DebugBSPRootTreeFlags6)
			{
				ColourPolygonFromFlags(p, _polygons[p].material.BSPTreeRootFlags, 0x10000, 0x8000, 0x10000);
			}
			#endregion

			#region BSP tree parent flags
			if (options.RenderMode == RenderMode.DebugBSPTreeImmediateParentFlagsHash)
			{
				ColourPolygonFromUInt(p, _polygons[p].material.BSPTreeParentNodeFlags);
			}
			if (options.RenderMode == RenderMode.DebugBSPTreeImmediateParentFlags1)
			{
				ColourPolygonFromFlags(p, _polygons[p].material.BSPTreeParentNodeFlags, 0x0001, 0x0002, 0x0004);
			}
			if (options.RenderMode == RenderMode.DebugBSPTreeImmediateParentFlags2)
			{
				ColourPolygonFromFlags(p, _polygons[p].material.BSPTreeParentNodeFlags, 0x0008, 0x0010, 0x0020);
			}
			if (options.RenderMode == RenderMode.DebugBSPTreeImmediateParentFlags3)
			{
				ColourPolygonFromFlags(p, _polygons[p].material.BSPTreeParentNodeFlags, 0x0040, 0x0080, 0x0100);
			}
			if (options.RenderMode == RenderMode.DebugBSPTreeImmediateParentFlags4)
			{
				ColourPolygonFromFlags(p, _polygons[p].material.BSPTreeParentNodeFlags, 0x0200, 0x0400, 0x0800);
			}
			if (options.RenderMode == RenderMode.DebugBSPTreeImmediateParentFlags5)
			{
				ColourPolygonFromFlags(p, _polygons[p].material.BSPTreeParentNodeFlags, 0x1000, 0x2000, 0x4000);
			}
			if (options.RenderMode == RenderMode.DebugBSPTreeImmediateParentFlags6)
			{
				ColourPolygonFromFlags(p, _polygons[p].material.BSPTreeParentNodeFlags, 0x10000, 0x8000, 0x10000);
			}
			#endregion

			#region BSP tree parent flags ORd
			ushort allParentFlagsORd = _polygons[p].material.BSPTreeAllParentNodeFlagsORd;
			if (options.BSPRenderingIncludeLeafFlagsWhenORing)
			{
				allParentFlagsORd |= _polygons[p].material.BSPTreeLeafFlags;
			}
			if (options.BSPRenderingIncludeRootTreeFlagsWhenORing)
			{
				allParentFlagsORd |= _polygons[p].material.BSPTreeRootFlags;
			}
			if (options.RenderMode == RenderMode.DebugBSPTreeAllParentFlagsORdHash)
			{
				ColourPolygonFromUInt(p, allParentFlagsORd);
			}
			if (options.RenderMode == RenderMode.DebugBSPTreeAllParentFlagsORd1)
			{
				ColourPolygonFromFlags(p, allParentFlagsORd, 0x0001, 0x0002, 0x0004);
			}
			if (options.RenderMode == RenderMode.DebugBSPTreeImmediateParentFlags2)
			{
				ColourPolygonFromFlags(p, allParentFlagsORd, 0x0008, 0x0010, 0x0020);
			}
			if (options.RenderMode == RenderMode.DebugBSPTreeImmediateParentFlags3)
			{
				ColourPolygonFromFlags(p, allParentFlagsORd, 0x0040, 0x0080, 0x0100);
			}
			if (options.RenderMode == RenderMode.DebugBSPTreeImmediateParentFlags4)
			{
				ColourPolygonFromFlags(p, allParentFlagsORd, 0x0200, 0x0400, 0x0800);
			}
			if (options.RenderMode == RenderMode.DebugBSPTreeImmediateParentFlags5)
			{
				ColourPolygonFromFlags(p, allParentFlagsORd, 0x1000, 0x2000, 0x4000);
			}
			if (options.RenderMode == RenderMode.DebugBSPTreeImmediateParentFlags6)
			{
				ColourPolygonFromFlags(p, allParentFlagsORd, 0x10000, 0x8000, 0x10000);
			}
			#endregion

			#region BSP tree leaf flags
			if (options.RenderMode == RenderMode.DebugBSPTreeLeafFlagsHash)
			{
				ColourPolygonFromUInt(p, _polygons[p].material.BSPTreeLeafFlags);
			}
			if (options.RenderMode == RenderMode.DebugBSPTreeLeafFlags1)
			{
				ColourPolygonFromFlags(p, _polygons[p].material.BSPTreeLeafFlags, 0x0001, 0x0002, 0x0004);
			}
			if (options.RenderMode == RenderMode.DebugBSPTreeLeafFlags2)
			{
				ColourPolygonFromFlags(p, _polygons[p].material.BSPTreeLeafFlags, 0x0008, 0x0010, 0x0020);
			}
			if (options.RenderMode == RenderMode.DebugBSPTreeLeafFlags3)
			{
				ColourPolygonFromFlags(p, _polygons[p].material.BSPTreeLeafFlags, 0x0040, 0x0080, 0x0100);
			}
			if (options.RenderMode == RenderMode.DebugBSPTreeLeafFlags4)
			{
				ColourPolygonFromFlags(p, _polygons[p].material.BSPTreeLeafFlags, 0x0200, 0x0400, 0x0800);
			}
			if (options.RenderMode == RenderMode.DebugBSPTreeLeafFlags5)
			{
				ColourPolygonFromFlags(p, _polygons[p].material.BSPTreeLeafFlags, 0x1000, 0x2000, 0x4000);
			}
			if (options.RenderMode == RenderMode.DebugBSPTreeLeafFlags6)
			{
				ColourPolygonFromFlags(p, _polygons[p].material.BSPTreeLeafFlags, 0x10000, 0x8000, 0x10000);
			}
			#endregion
			#endregion

			// default to grey for non-coloured elements - black makes it too hard to see anything in ModelEx

			if ((_polygons[p].material.colour & 0x00FFFFFF) == 0x00000000)
			{
				_polygons[p].material.colour |= 0x80D0D0D0;
			}
		}
	}
}
