using System;
using System.IO;
using System.Collections.Generic;
using TPages = BenLincoln.TheLostWorlds.CDTextures.PSXTextureDictionary;

namespace CDC.Objects.Models
{
	public class SR1UnitModel : SR1Model
	{
		protected UInt32 m_uBspTreeCount;
		protected UInt32 m_uBspTreeStart;
		protected UInt32 m_uSpectralVertexStart;
		protected UInt32 m_uSpectralColourStart;

		public SR1UnitModel(BinaryReader reader, UInt32 dataStart, UInt32 modelData, String strModelName, Platform ePlatform, UInt32 version, TPages tPages)
			: base(reader, dataStart, modelData, strModelName, ePlatform, version, tPages)
		{
			readTextureFT3Attributes = true;

			_modelTypePrefix = "a_";
			// struct _Terrain

			if (_version == SR1File.PROTO_19981025_VERSION)
			{
				reader.BaseStream.Position = _modelData;

				_groupCount = 1;
				m_uBspTreeCount = 1;
				m_uBspTreeStart = reader.ReadUInt32();

				reader.BaseStream.Position = _modelData + 0x1C;
			}
			else
			{
				// Alpha 1999-02-16:
				/*
                    long vplLength; // size=0, offset=0
                    unsigned char *vpList; // size=0, offset=4
                    long numIntros; // size=0, offset=8
                    struct Intro *introList; // size=76, offset=12
                 */

				// 1999-07-14
				/*
                    short UnitChangeFlags; // size=0, offset=0
                    short spad; // size=0, offset=2
                    long lpad2; // size=0, offset=4
                    long numIntros; // size=0, offset=8
                    struct Intro *introList; // size=76, offset=12
                 */

				reader.BaseStream.Position = _modelData + 0x10;
			}

			// long numVertices;
			_vertexCount = reader.ReadUInt32();
			// long numFaces;
			_polygonCount = reader.ReadUInt32();
			// long numNormals; 
			//reader.BaseStream.Position += 0x04;
			long numNormals = reader.ReadUInt32();
			// struct _TVertex *vertexList;
			_vertexStart = _dataStart + reader.ReadUInt32();
			// struct _TFace *faceList; 
			_polygonStart = _dataStart + reader.ReadUInt32();
			// struct _Normal *normalList;
			uint normalStart = reader.ReadUInt32();
			// struct DrMoveAniTex *aniList;
			uint drMoveAniTex = reader.ReadUInt32();
			// 1999-02-16: struct _BSPNode *sbspRoot; // size=44, offset=44 
			// 1999-07-14: long pad 
			uint sbspRoot = reader.ReadUInt32();
			// void *StreamUnits;
			uint streamUnitStart = reader.ReadUInt32();

			if (_version == SR1File.PROTO_19981025_VERSION)
			{
				reader.BaseStream.Position += 0x04;
			}

			// struct TextureFT3 *StartTextureList;
			_materialStart = _dataStart + reader.ReadUInt32();
			_materialCount = 0;

			if (_version == SR1File.PROTO_19981025_VERSION ||
				_version == SR1File.ALPHA_19990123_VERSION_1_X ||
				_version == SR1File.ALPHA_19990123_VERSION_1 ||
				_version == SR1File.ALPHA_19990204_VERSION_2 ||
				_version == SR1File.ALPHA_19990216_VERSION_3 ||
				_version == SR1File.BETA_19990512_VERSION)
			{
				// struct TextureFT3 *EndTextureList;
				// struct _SBSPLeaf *sbspStartLeaves;
				// struct _SBSPLeaf *sbspEndLeaves;

				reader.BaseStream.Position += 0x0C;
			}
			else
			{
				// struct TextureFT3 *EndTextureList;
				reader.BaseStream.Position += 0x04;
			}

			if (_version != SR1File.PROTO_19981025_VERSION)
			{
				// struct _MorphVertex *MorphDiffList;
				m_uSpectralVertexStart = _dataStart + reader.ReadUInt32();
				// struct _MorphColor *MorphColorList;
				m_uSpectralColourStart = _dataStart + reader.ReadUInt32();
				// long numBSPTrees
				m_uBspTreeCount = reader.ReadUInt32();
				// struct BSPTree *BSPTreeArray;
				m_uBspTreeStart = _dataStart + reader.ReadUInt32();
				_groupCount = m_uBspTreeCount;
				// short *morphNormalIdx;
				// struct _MultiSignal *signals;
			}

			_trees = new Tree[_groupCount];
		}

		public static SR1UnitModel Load(BinaryReader reader, UInt32 dataStart, UInt32 modelData, String strModelName, Platform ePlatform, UInt32 version, TPages tPages, ExportOptions options)
		{
			SR1UnitModel xModel = new SR1UnitModel(reader, dataStart, modelData, strModelName, ePlatform, version, tPages);
			xModel.ReadData(reader, options);
			return xModel;
		}

		protected override void ReadVertex(BinaryReader reader, int v, ExportOptions options)
		{
			base.ReadVertex(reader, v, options);

			_geometry.PositionsPhys[v] = _geometry.PositionsRaw[v];
			_geometry.PositionsAltPhys[v] = _geometry.PositionsPhys[v];

			_geometry.Vertices[v].colourID = v;

			reader.BaseStream.Position += 2;
			uint vColour = reader.ReadUInt32() | 0xFF000000;
			if (options.IgnoreVertexColours)
			{
				_geometry.Colours[v] = 0xFFFFFFFF;
			}
			else
			{
				_geometry.Colours[v] = vColour;
			}

			if (_platform != Platform.Dreamcast)
			{
				Utility.FlipRedAndBlue(ref _geometry.Colours[v]);
			}

			_geometry.ColoursAlt[v] = _geometry.Colours[v];
		}

		protected override void ReadVertices(BinaryReader reader, ExportOptions options)
		{
			base.ReadVertices(reader, options);

			ReadSpectralData(reader, options);
		}

		protected virtual void ReadSpectralData(BinaryReader reader, ExportOptions options)
		{
			if (m_uSpectralColourStart != 0)
			{
				// Spectral Colours
				reader.BaseStream.Position = m_uSpectralColourStart;
				for (int v = 0; v < _vertexCount; v++)
				{
					if (reader.BaseStream.Position >= reader.BaseStream.Length)
					{
						Console.WriteLine(string.Format("Error: reached end of file after reading {0}/{1} Spectral colours", v + 1, _vertexCount));
						break;
					}
					UInt32 uShiftColour = reader.ReadUInt16();
					UInt32 uAlpha = _geometry.ColoursAlt[v] & 0xFF000000;
					UInt32 uRed = ((uShiftColour >> 0) & 0x1F) << 0x13;
					UInt32 uGreen = ((uShiftColour >> 5) & 0x1F) << 0x0B;
					UInt32 uBlue = ((uShiftColour >> 10) & 0x1F) << 0x03;
					_geometry.ColoursAlt[v] = uAlpha | uRed | uGreen | uBlue;
				}
			}

			if (m_uSpectralVertexStart != 0)
			{
				// Spectral Verticices
				reader.BaseStream.Position = m_uSpectralVertexStart + 0x06;
				int sVertex = reader.ReadInt16();
				reader.BaseStream.Position = m_uSpectralVertexStart;
				int sVertexCount = 0;
				while (sVertex != 0xFFFF)
				{
					if (reader.BaseStream.Position >= reader.BaseStream.Length)
					{
						Console.WriteLine(string.Format("Error: reached end of file after reading reading {0} Spectral vertices", sVertexCount));
						break;
					}
					ShiftVertex xShiftVertex;
					xShiftVertex.basePos.x = (float)reader.ReadInt16();
					xShiftVertex.basePos.y = (float)reader.ReadInt16();
					xShiftVertex.basePos.z = (float)reader.ReadInt16();
					sVertex = reader.ReadUInt16();

					if (sVertex == 0xFFFF)
					{
						break;
					}

					xShiftVertex.offset.x = (float)reader.ReadInt16();
					xShiftVertex.offset.y = (float)reader.ReadInt16();
					xShiftVertex.offset.z = (float)reader.ReadInt16();
					_geometry.PositionsAltPhys[sVertex] = xShiftVertex.offset + xShiftVertex.basePos;
					sVertexCount++;
				}
			}
		}

		protected virtual void ReadPolygon(BinaryReader reader, int p, ExportOptions options)
		{
			UInt32 uPolygonPosition = (UInt32)reader.BaseStream.Position;
			// struct _TFace

			// struct _Face face; 
			_polygons[p].v1 = _geometry.Vertices[reader.ReadUInt16()];
			_polygons[p].v2 = _geometry.Vertices[reader.ReadUInt16()];
			_polygons[p].v3 = _geometry.Vertices[reader.ReadUInt16()];
			_polygons[p].material = new Material();

			// unless the user has explicitly requested distinct materials for each flag, remove use of anything ignored at this level
			if (!options.DistinctMaterialsForAllFlags)
			{
				_polygons[p].material.polygonFlagsUsedMask &= 0xFD;
				_polygons[p].material.sortPushUsedMask = 0x00;
			}


			long flagOffset = reader.BaseStream.Position + 2048;
			// unsigned char attr;
			byte flags = reader.ReadByte();

			if (options.IgnorePolygonFlag2ForTerrain)
			{
				// ignore flag 0x02 for terrain (it means "this is the other half of a quad" or something similar)
				flags = (byte)(flags & 0xFD);
			}

			//Console.WriteLine(string.Format("\t\t\tDebug: read Polygon flags: {0}", Convert.ToString(flags, 2).PadLeft(8, '0')));
			//Console.WriteLine(string.Format("0x{0:X2} (@0x{1:X4}, D:{1}\t:::", flags, flagOffset));
			// char sortPush;
			byte sortPush = reader.ReadByte();
			// unsigned short normal;
			UInt16 polygonNormal = reader.ReadUInt16();

			_polygons[p].normal = polygonNormal;
			_polygons[p].material.polygonFlags = flags;
			_polygons[p].material.sortPush = sortPush;

			UInt32 materialOffset;
			if (_version == SR1File.PROTO_19981025_VERSION)
			{
				reader.BaseStream.Position += 2;
				materialOffset = reader.ReadUInt32();

				if (materialOffset >= _materialStart)
				{
					materialOffset -= _materialStart;
				}
				else
				{
					// What would this mean?
					// It's not an offset in proto, so is there a real material is should point to?
					materialOffset = 0xFFFFFFFF;
				}
			}
			else
			{
				// unsigned short textoff;
				materialOffset = reader.ReadUInt16();
				//Console.WriteLine(string.Format("\t\t\tDebug: read textoff: 0x{0:X4}", materialOffset));
				//_polygons[p].material.textureUsed &= (Boolean)(materialOffset != 0xFFFF && materialOffset != 0xFFFFFFFF);
			}

			bool isTranslucent = false;

			_polygons[p].material.visible = true;

			if (materialOffset == 0xFFFF || materialOffset == 0xFFFFFFFF)
			{
				_polygons[p].material.visible = false;
			}

			bool ignoreMaterial0 = false;
			if (_version == SR1File.RETAIL_VERSION)
			{
				// if material 0 is not ignored in this version, it will cause visible collision boxes in nightb3
				// OTOH, it causes the gates in nighta1 to disappear
				//ignoreMaterial0 = true;
			}
			else
			{
				//ignoreMaterial0 = false;
			}

			if (ignoreMaterial0)
			{
				if (materialOffset == 0x00000000)
				{
					_polygons[p].material.visible = false;
				}
			}

			if ((flags & 0x01) == 0x01)
			{
				//_polygons[p].material.visible = false;
				//_polygons[p].material.textureUsed = false;
			}

			if (((flags & 0x02) == 0x02))
			{
			}

			if ((flags & 0x04) == 0x04)
			{
				_polygons[p].material.visible = false;
				//_polygons[p].material.textureUsed = false;
			}

			if (((flags & 0x08) == 0x08))
			{
				_polygons[p].material.emissivity = 1.0f;
			}

			if (((flags & 0x10) == 0x10))
			{
			}

			// 20 is not used in any known version of the game
			if ((flags & 0x20) == 0x20)
			{
				isTranslucent = true;
				_polygons[p].material.opacity = CDC.Material.OPACITY_TRANSLUCENT;
			}

			if ((flags & 0x40) == 0x40)
			{
				//_polygons[p].material.visible = false;
				//_polygons[p].material.textureUsed = false;
			}
			// 80 is not used in any known version of the game


			if (!_polygons[p].material.visible)
			{
				_polygons[p].material.textureUsed = false;
			}
			else
			{
				_polygons[p].material.textureUsed = true;
			}

			//if (Platform == Platform.Dreamcast)
			//{
			//    _polygons[p].material.colour = 0xFFFFFFFF;
			//    _polygons[p].colour = _polygons[p].material.colour;
			//}
			//else
			//{
			//    _polygons[p].colour = _polygons[p].material.colour;
			//}

			HandlePolygonInfo(reader, p, options, flags, materialOffset, isTranslucent);

			if (_version == SR1File.PROTO_19981025_VERSION)
			{
				reader.BaseStream.Position = uPolygonPosition + 0x10;
			}
			else
			{
				reader.BaseStream.Position = uPolygonPosition + 0x0C;
			}
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
				//Console.WriteLine(string.Format("\t\tReading polygon {0}", p));
				ReadPolygon(reader, p, options);
			}

			List<Mesh> xMeshes = new List<Mesh>();
			List<int> xMeshPositions = new List<int>();
			List<UInt32> treePolygons = new List<UInt32>((Int32)_vertexCount * 3);

			if (_version == SR1File.PROTO_19981025_VERSION)
			{
				_trees[0] = ReadBSPTree(0, 0.ToString(), reader, treePolygons, m_uBspTreeStart, _trees[0], xMeshes, xMeshPositions, 0, 0, 0, 0);
			}
			else
			{
				for (UInt32 t = 0; t < m_uBspTreeCount; t++)
				{
					// struct BSPTree
					reader.BaseStream.Position = m_uBspTreeStart + (t * 0x24);
					// struct _BSPNode *bspRoot;
					UInt32 uDataPos = _dataStart + reader.ReadUInt32();
					// struct _BSPLeaf *startLeaves;
					// struct _BSPLeaf *endLeaves; 
					// struct _Position globalOffset;
					reader.BaseStream.Position += 0x0E;
					//short flags;
					ushort rootTreeFlags = reader.ReadUInt16();
					//Console.WriteLine(string.Format("\t\t\t\t\tDebug: read BSP tree flags {0}", Convert.ToString(flags, 2).PadLeft(8, '0')));
					bool drawTester = ((rootTreeFlags & 1) != 1);
					// struct _Position localOffset;
					reader.BaseStream.Position += 0x06;
					// short ID;
					UInt16 usBspID = reader.ReadUInt16();
					// long splineID;
					// struct _Instance *instanceSpline;
					//if (_trees[t] == null)
					//{
					//    _trees[t] = new Tree();
					//}
					//_trees[t].sr1Flags = flags;
					_trees[t] = ReadBSPTree(t, t.ToString(), reader, treePolygons, uDataPos, _trees[t], xMeshes, xMeshPositions, 0, rootTreeFlags, 0, 0);
				}
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

			int currentPosition = 0;
			for (int m = 0; m < xMeshes.Count; m++)
			{
				FinaliseMesh(treePolygons, currentPosition, xMeshes[m]);
				currentPosition = xMeshPositions[m];
			}
		}

		protected override void ReadMaterial(BinaryReader reader, int p, UInt32 colourOrMaterialPosition, ExportOptions options)
		{
			// WIP
			UInt32 materialPosition = colourOrMaterialPosition + _materialStart;
			if (_version == SR1File.RETAIL_VERSION &&
				(((materialPosition - _materialStart) % 0x0C) != 0) &&
				 ((materialPosition - _materialStart) % 0x14) == 0)
			{
				_platform = Platform.Dreamcast;
			}

			reader.BaseStream.Position = materialPosition;
			base.ReadMaterial(reader, p, colourOrMaterialPosition, options);
			//_polygons[p].material.textureAttributes = _polygons[p].sr1TextureFT3Attributes;
		}

		protected virtual Tree ReadBSPTree(uint rootTreeNum, string treeNodeID, BinaryReader reader, List<UInt32> treePolygons, UInt32 uDataPos, Tree xParentTree, List<Mesh> xMeshes,
			List<int> xMeshPositions, UInt32 uDepth, UInt16 treeRootFlags, UInt16 parentNodeFlags, UInt16 parentNodeFlagsORd)
		{
			if (uDataPos == 0)
			{
				return null;
			}

			if (_version == SR1File.PROTO_19981025_VERSION)
			{
				reader.BaseStream.Position = uDataPos + 0x12;
			}
			else
			{
				reader.BaseStream.Position = uDataPos + 0x0E;
			}
			bool isLeaf = ((reader.ReadByte() & 0x02) == 0x02);
			Int32 iSubTreeCount = 2;

			Tree xTree = null;
			Mesh xMesh = null;

			UInt32 uMaxDepth = 0;

			if (uDepth <= uMaxDepth)
			{
				xTree = new Tree();
				xMesh = new Mesh();
				xMesh.startIndex = 0;
				xTree.mesh = xMesh;

				if (xParentTree != null)
				{
					xParentTree.Push(xTree);
				}
			}
			else
			{
				xTree = xParentTree;
				xMesh = xParentTree.mesh;
			}
			xMesh.sr1BSPTreeFlags = treeRootFlags;

			if (isLeaf)
			{
				xTree.isLeaf = true;

				if (_version == SR1File.PROTO_19981025_VERSION)
				{
					reader.BaseStream.Position = uDataPos + 0x0C;
				}
				else
				{
					reader.BaseStream.Position = uDataPos + 0x08;
				}

				ReadBSPLeaf(reader, treePolygons, xMesh, treeRootFlags, parentNodeFlags, parentNodeFlagsORd, rootTreeNum, treeNodeID);
			}
			else
			{
				// reader.BaseStream.Position = uDataPos + 0x14;
				// struct _BSPNode
				// struct _Sphere_noSq sphere;
				// short a;
				// short b;
				// short c;

				if (_version == SR1File.PROTO_19981025_VERSION)
				{
					reader.BaseStream.Position = uDataPos + 0x12;
				}
				else
				{
					reader.BaseStream.Position = uDataPos + 0x0E;
				}
				UInt16 nodeFlags = reader.ReadUInt16();
				UInt16 nodeFlagsORd = (UInt16)(parentNodeFlagsORd | nodeFlags);
				//Console.WriteLine(string.Format("\t\t\t\t\tDebug: read BSP node flags {0}", Convert.ToString(nodeFlags, 2).PadLeft(8, '0')));
				//xMesh.sr1BSPNodeFlags.Add(nodeFlags);
				//long d;
				reader.BaseStream.Position += 4;
				// void *front;
				// void *back; 
				UInt32[] auSubTreePositions = new UInt32[2];
				for (Int32 s = 0; s < iSubTreeCount; s++)
				{
					auSubTreePositions[s] = reader.ReadUInt32();
				}

				for (Int32 s = 0; s < iSubTreeCount; s++)
				{
					ReadBSPTree(rootTreeNum, string.Format("{0}-{1}", treeNodeID, s), reader, treePolygons, auSubTreePositions[s], xTree, xMeshes, xMeshPositions, uDepth + 1, treeRootFlags, nodeFlags, nodeFlagsORd);
				}
			}

			if (uDepth <= uMaxDepth)
			{
				if (xMesh != null && xMesh.indexCount > 0)
				{
					xMeshes.Add(xMesh);
					xMeshPositions.Add(treePolygons.Count);
				}
			}

			return xTree;
		}

		protected virtual void ReadBSPLeaf(BinaryReader reader, List<UInt32> treePolygons, Mesh xMesh, UInt16 baseTreeFlags, UInt16 parentNodeFlags, UInt16 allParentNodeFlagsORd, uint rootBSPTreeNum, string parentNodeID)
		{
			// struct _BSPLeaf 
			// struct _TFace *faceList;
			UInt32 polygonPos = _dataStart + reader.ReadUInt32();
			UInt32 polygonID;
			if (_version == SR1File.PROTO_19981025_VERSION)
			{
				polygonID = (polygonPos - _polygonStart) / 0x10;
			}
			else
			{
				polygonID = (polygonPos - _polygonStart) / 0x0C;
			}
			// short numFaces;
			UInt16 polyCount = reader.ReadUInt16();
			// short flags; 
			UInt16 flags = reader.ReadUInt16();
			//Console.WriteLine(string.Format("\t\t\t\t\tDebug: read BSP leaf flags {0}", Convert.ToString(flags, 2).PadLeft(8, '0')));
			if (xMesh != null)
			{
				xMesh.sr1BSPLeafFlags.Add(flags);
			}

			_polygons[polygonID].material.BSPTreeRootFlags = baseTreeFlags;
			_polygons[polygonID].material.BSPTreeParentNodeFlags = parentNodeFlags;
			_polygons[polygonID].material.BSPTreeAllParentNodeFlagsORd = allParentNodeFlagsORd;
			_polygons[polygonID].material.BSPTreeLeafFlags = flags;
			_polygons[polygonID].RootBSPTreeNumber = rootBSPTreeNum;
			_polygons[polygonID].material.RootBSPTreeNumber = rootBSPTreeNum;
			_polygons[polygonID].BSPNodeID = parentNodeID;

			//if (rootBSPTreeNum != 0)
			//{
			//    Console.WriteLine(string.Format("Non-zero root BSP tree number: {0}", rootBSPTreeNum));
			//}
			//if (baseTreeFlags != 0)
			//{
			//    Console.WriteLine(string.Format("Non-zero baseTreeFlags: {0:X4}", baseTreeFlags));
			//}
			//if (parentNodeFlags != 0)
			//{
			//    Console.WriteLine(string.Format("Non-zero parentNodeFlags: {0:X4}", parentNodeFlags));
			//}
			//if (allParentNodeFlagsORd != 0)
			//{
			//    Console.WriteLine(string.Format("Non-zero allParentNodeFlagsORd: {0:X4}", allParentNodeFlagsORd));
			//}
			//if (flags != 0)
			//{
			//    Console.WriteLine(string.Format("Non-zero leaf flags: {0:X4}", flags));
			//}

			for (UInt16 p = 0; p < polyCount; p++)
			{
				//_polygons[polygonID + p].material.visible = true;
				_polygons[polygonID + p].material.BSPTreeRootFlags = baseTreeFlags;
				_polygons[polygonID + p].material.BSPTreeParentNodeFlags = parentNodeFlags;
				_polygons[polygonID + p].material.BSPTreeAllParentNodeFlagsORd = allParentNodeFlagsORd;
				_polygons[polygonID + p].material.BSPTreeLeafFlags = flags;
				_polygons[polygonID + p].RootBSPTreeNumber = rootBSPTreeNum;
				_polygons[polygonID + p].BSPNodeID = parentNodeID;

				//BSP Tree material handling.
				// 1 = Hidden?
				// 2 = No collisions?
				//if ((_polygons[polygonID + p].material.BSPTreeRootFlags & 0x3) == 0x01)
				//{
				// See EVENT_GetTGroupValue, WARPGATE_DrawWarpGateRim
				//_polygons[polygonID + p].material.visible = false;
				//_polygons[polygonID + p].material.textureUsed = false;
				//}

				//BSP Tree material handling
				//if ((_polygons[polygonID + p].material.BSPTreeParentNodeFlags & 0x1) != 0x1)
				//{
				//_polygons[polygonID + p].material.visible = false;
				//}

				treePolygons.Add(polygonID + p);

				if (xMesh != null)
				{
					xMesh.indexCount += 3;
				}
			}
		}

		protected virtual void FinaliseMesh(List<UInt32> treePolygons, int firstPolygon, Mesh xMesh)
		{
			xMesh.polygonCount = xMesh.indexCount / 3;
			xMesh.polygons = new Polygon[xMesh.polygonCount];
			for (int p = 0; p < xMesh.polygonCount; p++)
			{
				UInt32 polygonID = treePolygons[firstPolygon + p];
				xMesh.polygons[p] = _polygons[polygonID];
			}

			xMesh.vertices = new Vertex[xMesh.indexCount];
			for (UInt16 poly = 0; poly < xMesh.polygonCount; poly++)
			{
				xMesh.vertices[(3 * poly) + 0] = xMesh.polygons[poly].v1;
				xMesh.vertices[(3 * poly) + 1] = xMesh.polygons[poly].v2;
				xMesh.vertices[(3 * poly) + 2] = xMesh.polygons[poly].v3;
			}
		}
	}
}
