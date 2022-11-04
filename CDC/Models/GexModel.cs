using System;
using System.IO;
using TPages = BenLincoln.TheLostWorlds.CDTextures.PSXTextureDictionary;
using TextureTile = BenLincoln.TheLostWorlds.CDTextures.PSXTextureTile;

namespace CDC.Objects.Models
{
	public abstract class GexModel : SRModel
	{
		#region Normals
		protected static Int32[,] s_aiNormals =
		{
			{0, 0, 4096},
			{-1930, -3344, -1365},
			{3861, 0, -1365},
			{-1930, 3344, -1365},
			{-353, -613, 4034},
			{-697, -1207, 3851},
			{-1019, -1765, 3552},
			{-1311, -2270, 3146},
			{-1563, -2707, 2645},
			{-1768, -3063, 2065},
			{-1920, -3326, 1423},
			{-2014, -3489, 738},
			{-2047, -3547, 30},
			{-2019, -3498, -677},
			{707, 0, 4034},
			{1394, 0, 3851},
			{2039, 0, 3552},
			{2622, 0, 3146},
			{3126, 0, 2645},
			{3536, 0, 2065},
			{3840, 0, 1423},
			{4028, 0, 738},
			{4095, 0, 30},
			{4039, 0, -677},
			{-353, 613, 4034},
			{-697, 1207, 3851},
			{-1019, 1765, 3552},
			{-1311, 2270, 3146},
			{-1563, 2707, 2645},
			{-1768, 3063, 2065},
			{-1920, 3326, 1423},
			{-2014, 3489, 738},
			{-2047, 3547, 30},
			{-2019, 3498, -677},
			{-1311, -3498, -1678},
			{-653, -3547, -1941},
			{24, -3489, -2145},
			{701, -3326, -2285},
			{1358, -3063, -2355},
			{1973, -2707, -2355},
			{2529, -2270, -2285},
			{3009, -1765, -2145},
			{3398, -1207, -1941},
			{3685, -613, -1678},
			{3685, 613, -1678},
			{3398, 1207, -1941},
			{3009, 1765, -2145},
			{2529, 2270, -2285},
			{1973, 2707, -2355},
			{1358, 3063, -2355},
			{701, 3326, -2285},
			{24, 3489, -2145},
			{-653, 3547, -1941},
			{-1311, 3498, -1678},
			{-2373, 2885, -1678},
			{-2745, 2339, -1941},
			{-3033, 1723, -2145},
			{-3231, 1055, -2285},
			{-3331, 355, -2355},
			{-3331, -355, -2355},
			{-3231, -1055, -2285},
			{-3033, -1723, -2145},
			{-2745, -2339, -1941},
			{-2373, -2885, -1678},
			{364, -631, 4030},
			{33, -1270, 3893},
			{1083, -664, 3893},
			{-273, -1899, 3618},
			{787, -1364, 3780},
			{1781, -712, 3618},
			{-544, -2497, 3200},
			{520, -2080, 3489},
			{1541, -1490, 3489},
			{2435, -777, 3200},
			{-767, -3043, 2631},
			{293, -2785, 2989},
			{1331, -2306, 3111},
			{2265, -1646, 2989},
			{3019, -857, 2631},
			{-939, -3504, 1901},
			{110, -3426, 2240},
			{1151, -3100, 2416},
			{2108, -2547, 2416},
			{2912, -1808, 2240},
			{3504, -938, 1901},
			{-1067, -3821, 1017},
			{-52, -3906, 1230},
			{966, -3739, 1364},
			{1922, -3330, 1409},
			{2755, -2706, 1364},
			{3409, -1907, 1230},
			{3843, -985, 1017},
			{-1174, -3923, 42},
			{-238, -4088, 51},
			{711, -4033, 58},
			{1622, -3760, 61},
			{2445, -3285, 61},
			{3137, -2632, 58},
			{3660, -1838, 51},
			{3985, -944, 42},
			{-1265, -3792, -889},
			{-457, -3928, -1064},
			{368, -3900, -1194},
			{1179, -3709, -1275},
			{1941, -3363, -1302},
			{2622, -2876, -1275},
			{3193, -2269, -1194},
			{3631, -1567, -1064},
			{3917, -800, -889},
			{364, 631, 4030},
			{1083, 664, 3893},
			{33, 1270, 3893},
			{1781, 712, 3618},
			{787, 1364, 3780},
			{-273, 1899, 3618},
			{2435, 777, 3200},
			{1541, 1490, 3489},
			{520, 2080, 3489},
			{-544, 2497, 3200},
			{3019, 857, 2631},
			{2265, 1646, 2989},
			{1331, 2306, 3111},
			{293, 2785, 2989},
			{-767, 3043, 2631},
			{3504, 938, 1901},
			{2912, 1808, 2240},
			{2108, 2547, 2416},
			{1151, 3100, 2416},
			{110, 3426, 2240},
			{-939, 3504, 1901},
			{3843, 985, 1017},
			{3409, 1907, 1230},
			{2755, 2706, 1364},
			{1922, 3330, 1409},
			{966, 3739, 1364},
			{-52, 3906, 1230},
			{-1067, 3821, 1017},
			{3985, 944, 42},
			{3660, 1838, 51},
			{3137, 2632, 58},
			{2445, 3285, 61},
			{1622, 3760, 61},
			{711, 4033, 58},
			{-238, 4088, 51},
			{-1174, 3923, 42},
			{3917, 800, -889},
			{3631, 1567, -1064},
			{3193, 2269, -1194},
			{2622, 2876, -1275},
			{1941, 3363, -1302},
			{1179, 3709, -1275},
			{368, 3900, -1194},
			{-457, 3928, -1064},
			{-1265, 3792, -889},
			{-729, 0, 4030},
			{-1117, 606, 3893},
			{-1117, -606, 3893},
			{-1507, 1186, 3618},
			{-1575, 0, 3780},
			{-1507, -1186, 3618},
			{-1890, 1719, 3200},
			{-2061, 589, 3489},
			{-2061, -589, 3489},
			{-1890, -1719, 3200},
			{-2252, 2186, 2631},
			{-2558, 1138, 2989},
			{-2663, 0, 3111},
			{-2558, -1138, 2989},
			{-2252, -2186, 2631},
			{-2565, 2565, 1901},
			{-3022, 1618, 2240},
			{-3260, 552, 2416},
			{-3260, -552, 2416},
			{-3022, -1618, 2240},
			{-2565, -2565, 1901},
			{-2775, 2835, 1017},
			{-3356, 1998, 1230},
			{-3721, 1032, 1364},
			{-3845, 0, 1409},
			{-3721, -1032, 1364},
			{-3356, -1998, 1230},
			{-2775, -2835, 1017},
			{-2810, 2979, 42},
			{-3421, 2250, 51},
			{-3848, 1400, 58},
			{-4067, 475, 61},
			{-4067, -475, 61},
			{-3848, -1400, 58},
			{-3421, -2250, 51},
			{-2810, -2979, 42},
			{-2652, 2992, -889},
			{-3173, 2360, -1064},
			{-3562, 1630, -1194},
			{-3802, 832, -1275},
			{-3883, 0, -1302},
			{-3802, -832, -1275},
			{-3562, -1630, -1194},
			{-3173, -2360, -1064},
			{-2652, -2992, -889},
			{-1778, -3080, -2031},
			{-2174, -2553, -2351},
			{-1124, -3159, -2351},
			{-2482, -1926, -2627},
			{-1519, -2632, -2745},
			{-427, -3112, -2627},
			{-2683, -1207, -2849},
			{-1812, -1959, -3107},
			{-790, -2548, -3107},
			{295, -2927, -2849},
			{-2758, -404, -3000},
			{-1968, -1132, -3408},
			{-1022, -1771, -3548},
			{3, -2270, -3408},
			{1028, -2591, -3000},
			{-2690, 470, -3052},
			{-1953, -147, -3596},
			{-1074, -755, -3879},
			{-117, -1308, -3879},
			{848, -1766, -3596},
			{1753, -2094, -3052},
			{-2472, 1388, -2955},
			{-1751, 963, -3575},
			{-917, 476, -3963},
			{-23, -40, -4095},
			{871, -555, -3963},
			{1710, -1034, -3575},
			{2438, -1447, -2955},
			{-2131, 2266, -2664},
			{-1403, 2070, -3243},
			{-599, 1763, -3647},
			{237, 1361, -3855},
			{1060, 886, -3855},
			{1827, 363, -3647},
			{2495, -179, -3243},
			{3028, -712, -2664},
			{-1729, 2987, -2203},
			{-1013, 2965, -2637},
			{-255, 2819, -2960},
			{513, 2555, -3159},
			{1261, 2184, -3227},
			{1956, 1722, -3159},
			{2569, 1188, -2960},
			{3075, 604, -2637},
			{3452, -4, -2203}
		};
		#endregion

		protected TPages _tPages;

		protected GexModel(BinaryReader reader, UInt32 dataStart, UInt32 modelData, String strModelName, Platform ePlatform, UInt32 version, TPages tPages) :
			base(reader, dataStart, modelData, strModelName, ePlatform, version)
		{
			_tPages = tPages;
		}

		protected virtual void ReadData(BinaryReader reader, CDC.Objects.ExportOptions options)
		{
			// Get the normals
			_geometry.Normals = new Vector[s_aiNormals.Length / 3];
			for (int n = 0; n < _geometry.Normals.Length; n++)
			{
				_geometry.Normals[n].x = ((float)s_aiNormals[n, 0] / 4096.0f);
				_geometry.Normals[n].y = ((float)s_aiNormals[n, 1] / 4096.0f);
				_geometry.Normals[n].z = ((float)s_aiNormals[n, 2] / 4096.0f);
			}

			// Get the vertices
			_geometry.Vertices = new Vertex[_vertexCount];
			_geometry.PositionsRaw = new Vector[_vertexCount];
			_geometry.PositionsPhys = new Vector[_vertexCount];
			_geometry.PositionsAltPhys = new Vector[_vertexCount];
			_geometry.Colours = new UInt32[_vertexCount];
			_geometry.ColoursAlt = new UInt32[_vertexCount];
			ReadVertices(reader, options);

			// Get the polygons
			_polygons = new Polygon[_polygonCount];
			_geometry.UVs = new UV[_indexCount];
			ReadPolygons(reader, options);

			// Generate the output
			GenerateOutput();
		}

		protected virtual void ReadVertex(BinaryReader reader, int v, CDC.Objects.ExportOptions options)
		{
			_geometry.Vertices[v].positionID = v;

			// Read the local coordinates
			_geometry.PositionsRaw[v].x = (float)reader.ReadInt16();
			_geometry.PositionsRaw[v].y = (float)reader.ReadInt16();
			_geometry.PositionsRaw[v].z = (float)reader.ReadInt16();
		}

		protected virtual void ReadVertices(BinaryReader reader, CDC.Objects.ExportOptions options)
		{
			if (_vertexStart == 0 || _vertexCount == 0)
			{
				return;
			}

			reader.BaseStream.Position = _vertexStart;

			for (int v = 0; v < _vertexCount; v++)
			{
				ReadVertex(reader, v, options);
			}

			return;
		}

		protected abstract void ReadPolygons(BinaryReader reader, CDC.Objects.ExportOptions options);

		protected virtual void ReadMaterial(BinaryReader reader, int p, CDC.Objects.ExportOptions options)
		{
			int v1 = (p * 3) + 0;
			int v2 = (p * 3) + 1;
			int v3 = (p * 3) + 2;

			_polygons[p].v1.UVID = v1;
			_polygons[p].v2.UVID = v2;
			_polygons[p].v3.UVID = v3;

			Byte v1U = reader.ReadByte();
			Byte v1V = reader.ReadByte();

			_polygons[p].material.clutValue = reader.ReadUInt16();

			Byte v2U = reader.ReadByte();
			Byte v2V = reader.ReadByte();

			_polygons[p].material.texturePage = reader.ReadUInt16();

			Byte v3U = reader.ReadByte();
			Byte v3V = reader.ReadByte();

			_geometry.UVs[v1].u = ((float)v1U) / 255.0f;
			_geometry.UVs[v1].v = ((float)v1V) / 255.0f;
			_geometry.UVs[v2].u = ((float)v2U) / 255.0f;
			_geometry.UVs[v2].v = ((float)v2V) / 255.0f;
			_geometry.UVs[v3].u = ((float)v3U) / 255.0f;
			_geometry.UVs[v3].v = ((float)v3V) / 255.0f;

			_polygons[p].material.colour = 0xFFFFFFFF;

			TextureTile tile = new TextureTile()
			{
				textureID = _polygons[p].material.textureID,
				tPage = (ushort)(_polygons[p].material.texturePage),
				clut = (ushort)(_polygons[p].material.clutValue),
				textureUsed = _polygons[p].material.textureUsed,
				visible = _polygons[p].material.visible,
				u = new int[] { v1U, v2U, v3U },
				v = new int[] { v1V, v2V, v3V },
			};

			_tPages.AddTextureTile2(tile);
			_polygons[p].material.textureID = _tPages.AddTextureTile(tile);

			return;
		}

		protected virtual void GenerateOutput()
		{
			// Make the vertices unique
			_geometry.Vertices = new Vertex[_indexCount];
			for (UInt32 p = 0; p < _polygonCount; p++)
			{
				_geometry.Vertices[(3 * p) + 0] = _polygons[p].v1;
				_geometry.Vertices[(3 * p) + 1] = _polygons[p].v2;
				_geometry.Vertices[(3 * p) + 2] = _polygons[p].v3;
			}

			// Build the materials array
			_materials = new Material[_materialCount];
			UInt16 mNew = 0;

			foreach (Material xMaterial in _materialsList)
			{
				_materials[mNew] = xMaterial;
				_materials[mNew].ID = mNew;
				mNew++;
			}

			return;
		}

		protected void ProcessPolygons(CDC.Objects.ExportOptions options)
		{
			MaterialList materialList = null;

			for (UInt16 p = 0; p < _polygonCount; p++)
			{
				HandleDebugRendering(p, options);

				if (materialList == null)
				{
					materialList = new MaterialList(_polygons[p].material);
					_materialsList.Add(_polygons[p].material);
				}
				else
				{
					Material newMaterial = materialList.AddToList(_polygons[p].material);
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
		}
	}
}
