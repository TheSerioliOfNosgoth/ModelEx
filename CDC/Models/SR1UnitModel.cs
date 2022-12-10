using System;
using System.IO;
using System.Collections.Generic;
using TPages = BenLincoln.TheLostWorlds.CDTextures.PSXTextureDictionary;

namespace CDC
{
	public class SR1UnitModel : SR1Model
	{
		enum PolygonFlags : byte
		{
			Backfacing = 0x02,
			Hidden0 = 0x04,
			Emissive = 0x08,
			Translucent = 0x20,
		}

		enum TextureAttributes : ushort
		{
			AlphaMaskedTerrain = 0x0010,
			TranslucentTerrain = 0x0040,
			Translucent0 = 0x2000,
			Emmisive = 0x8000,
		}

		protected UInt32 _bspTreeCount;
		protected UInt32 _bspTreeStart;
		protected UInt32 _spectralVertexStart;
		protected UInt32 _spectralColourStart;

		public SR1UnitModel(BinaryReader reader, DataFile dataFile, UInt32 dataStart, UInt32 modelData, String modelName, Platform ePlatform, UInt32 version, TPages tPages)
			: base(reader, dataFile, dataStart, modelData, modelName, ePlatform, version, tPages)
		{
			readTextureFT3Attributes = true;

			_modelTypePrefix = "a_";
			// struct _Terrain

			if (_version == SR1File.PROTO_19981025_VERSION)
			{
				reader.BaseStream.Position = _modelData;

				_groupCount = 1;
				_bspTreeCount = 1;
				_bspTreeStart = reader.ReadUInt32();

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
				_version == SR1File.ALPHA_19990414_VERSION_4 ||
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
				_spectralVertexStart = _dataStart + reader.ReadUInt32();
				// struct _MorphColor *MorphColorList;
				_spectralColourStart = _dataStart + reader.ReadUInt32();
				// long numBSPTrees
				_bspTreeCount = reader.ReadUInt32();
				// struct BSPTree *BSPTreeArray;
				_bspTreeStart = _dataStart + reader.ReadUInt32();
				_groupCount = _bspTreeCount;
				// short *morphNormalIdx;
				// struct _MultiSignal *signals;
			}

			_trees = new Tree[_groupCount];
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
			if (_spectralColourStart != 0)
			{
				// Spectral Colours
				reader.BaseStream.Position = _spectralColourStart;
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

			if (_spectralVertexStart != 0)
			{
				// Spectral Verticices
				reader.BaseStream.Position = _spectralVertexStart + 0x06;
				int sVertex = reader.ReadInt16();
				reader.BaseStream.Position = _spectralVertexStart;
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

		protected override void ReadPolygon(BinaryReader reader, int p, ExportOptions options)
		{
			uint polygonPosition = (uint)reader.BaseStream.Position;
			bool textureUsed = false;

			// struct _TFace
			#region _TFace

			// struct _Face face;
			int v1 = reader.ReadUInt16();
			int v2 = reader.ReadUInt16();
			int v3 = reader.ReadUInt16();

			// unsigned char attr;
			byte attr = reader.ReadByte();
			if (options.IgnoreBackfacingFlagForTerrain)
			{
				attr &= (byte)~PolygonFlags.Backfacing;
			}

			// char sortPush;
			byte sortPush = reader.ReadByte();

			// unsigned short normal;
			ushort normal = reader.ReadUInt16();

			// unsigned short textoff;
			uint textoff;
			if (_version == SR1File.PROTO_19981025_VERSION)
			{
				reader.BaseStream.Position += 2;
				textoff = reader.ReadUInt32();

				if (textoff != 0xFFFFFFFF && textoff >= _materialStart)
				{
					textoff -= _materialStart;
					textureUsed = true;
				}

				// What would this mean?
				// It's not an offset in proto, so is there a real material is should point to?
				// Probably a multisignal.
			}
			else
			{
				textoff = reader.ReadUInt16();

				if (textoff != 0xFFFF)
				{
					textureUsed = true;
				}
			}

			#endregion

			Material material = new Material();
			material.visible = true;
			material.textureUsed = textureUsed;
			material.isTranslucent = false;
			material.isEmissive = false;
			material.UseAlphaMask = false;
			material.polygonFlags = attr;
			material.sortPush = sortPush;

			_polygons[p].material = material;
			_polygons[p].materialOffset = textoff;
			_polygons[p].v1 = _geometry.Vertices[v1];
			_polygons[p].v2 = _geometry.Vertices[v2];
			_polygons[p].v3 = _geometry.Vertices[v3];
			_polygons[p].normal = normal;

			// Unless the user has explicitly requested distinct materials for each flag, remove use of anything ignored at this level
			if (!options.DistinctMaterialsForAllFlags)
			{
				material.polygonFlagsUsedMask &= (byte)(PolygonFlags.Backfacing | PolygonFlags.Hidden0 | PolygonFlags.Emissive | PolygonFlags.Translucent);
			}

			if (_version == SR1File.PROTO_19981025_VERSION)
			{
				reader.BaseStream.Position = polygonPosition + 0x10;
			}
			else
			{
				reader.BaseStream.Position = polygonPosition + 0x0C;
			}
		}

		protected override void ProcessPolygon(int p, ExportOptions options)
		{
			ref Polygon polygon = ref _polygons[p];
			ref Material material = ref polygon.material;
			
			if (material.BSPRootTreeID == -1)
			{
				material.visible = false;
				material.textureUsed = false;
				material.colour = 0x00000000;
				Utility.FlipRedAndBlue(ref material.colour);
				return;
			}

			if ((material.polygonFlags & (byte)PolygonFlags.Hidden0) != 0)
			{
				material.visible = false;
			}

			if ((material.polygonFlags & (byte)PolygonFlags.Emissive) != 0)
			{
				material.isEmissive = true;
			}

			if ((material.polygonFlags & (byte)PolygonFlags.Translucent) != 0)
			{
				material.isTranslucent = true;
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

			List<Mesh> meshes = new List<Mesh>();
			List<int> meshPositions = new List<int>();
			List<UInt32> treePolygons = new List<UInt32>((Int32)_vertexCount * 3);

			if (_version == SR1File.PROTO_19981025_VERSION)
			{
				_trees[0] = ReadBSPTree(0, 0.ToString(), reader, treePolygons, _bspTreeStart, _trees[0], meshes, meshPositions, 0, 0, 0, 0);
			}
			else
			{
				for (UInt32 t = 0; t < _bspTreeCount; t++)
				{
					// struct BSPTree
					reader.BaseStream.Position = _bspTreeStart + (t * 0x24);
					// struct _BSPNode *bspRoot;
					UInt32 uDataPos = _dataStart + reader.ReadUInt32();
					// struct _BSPLeaf *startLeaves;
					// struct _BSPLeaf *endLeaves; 
					reader.BaseStream.Position += 0x08;

					// struct _Position globalOffset;
					Vector globalOffset = new Vector();
					globalOffset.x = (float)reader.ReadInt16();
					globalOffset.y = (float)reader.ReadInt16();
					globalOffset.z = (float)reader.ReadInt16();

					//short flags;
					ushort rootTreeFlags = reader.ReadUInt16();
					//Console.WriteLine(string.Format("\t\t\t\t\tDebug: read BSP tree flags {0}", Convert.ToString(flags, 2).PadLeft(8, '0')));
					// struct _Position localOffset;
					reader.BaseStream.Position += 0x06;
					// short ID;
					short BspID = reader.ReadInt16();
					// long splineID;
					// struct _Instance *instanceSpline;
					_trees[t] = ReadBSPTree(BspID, t.ToString(), reader, treePolygons, uDataPos, _trees[t], meshes, meshPositions, 0, rootTreeFlags, 0, 0);

					if (_trees[t] != null)
					{
						_trees[t].globalOffset = globalOffset;
					}
				}
			}

			ProcessPolygons(reader, options);

			int currentPosition = 0;
			for (int m = 0; m < meshes.Count; m++)
			{
				FinaliseMesh(treePolygons, currentPosition, meshes[m]);
				currentPosition = meshPositions[m];
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

			UInt32 materialPosition = _materialStart + polygon.materialOffset;
			if (_version == SR1File.RETAIL_VERSION &&
				(((materialPosition - _materialStart) % 0x0C) != 0) &&
				 ((materialPosition - _materialStart) % 0x14) == 0)
			{
				_platform = Platform.Dreamcast;
			}

			reader.BaseStream.Position = materialPosition;
			base.ReadMaterial(reader, p, options);

			Utility.FlipRedAndBlue(ref material.colour);
			material.colour |= 0xFF000000;
		}

		protected virtual Tree ReadBSPTree(short rootBSPTreeID, string treeNodeID, BinaryReader reader, List<UInt32> treePolygons, UInt32 dataPos, Tree parentTree, List<Mesh> meshes,
			List<int> meshPositions, UInt32 depth, UInt16 treeRootFlags, UInt16 parentNodeFlags, UInt16 parentNodeFlagsORd)
		{
			if (dataPos == 0)
			{
				return null;
			}

			if (_version == SR1File.PROTO_19981025_VERSION)
			{
				reader.BaseStream.Position = dataPos + 0x12;
			}
			else
			{
				reader.BaseStream.Position = dataPos + 0x0E;
			}
			bool isLeaf = ((reader.ReadByte() & 0x02) == 0x02);
			Int32 iSubTreeCount = 2;

			Tree xTree = null;
			Mesh xMesh = null;

			UInt32 uMaxDepth = 0;

			if (depth <= uMaxDepth)
			{
				xTree = new Tree();
				xMesh = new Mesh();
				xMesh.startIndex = 0;
				xTree.mesh = xMesh;

				if (parentTree != null)
				{
					parentTree.Push(xTree);
				}
			}
			else
			{
				xTree = parentTree;
				xMesh = parentTree.mesh;
			}
			xMesh.sr1BSPTreeFlags = treeRootFlags;

			if (isLeaf)
			{
				xTree.isLeaf = true;

				if (_version == SR1File.PROTO_19981025_VERSION)
				{
					reader.BaseStream.Position = dataPos + 0x0C;
				}
				else
				{
					reader.BaseStream.Position = dataPos + 0x08;
				}

				ReadBSPLeaf(reader, treePolygons, xMesh, treeRootFlags, parentNodeFlags, parentNodeFlagsORd, rootBSPTreeID, treeNodeID);
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
					reader.BaseStream.Position = dataPos + 0x12;
				}
				else
				{
					reader.BaseStream.Position = dataPos + 0x0E;
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
					ReadBSPTree(rootBSPTreeID, string.Format("{0}-{1}", treeNodeID, s), reader, treePolygons, auSubTreePositions[s], xTree, meshes, meshPositions, depth + 1, treeRootFlags, nodeFlags, nodeFlagsORd);
				}
			}

			if (depth <= uMaxDepth)
			{
				if (xMesh != null && xMesh.indexCount > 0)
				{
					meshes.Add(xMesh);
					meshPositions.Add(treePolygons.Count);
				}
			}

			return xTree;
		}

		protected virtual void ReadBSPLeaf(BinaryReader reader, List<UInt32> treePolygons, Mesh xMesh, UInt16 baseTreeFlags, UInt16 parentNodeFlags, UInt16 allParentNodeFlagsORd, short rootBSPTreeID, string parentNodeID)
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

			if (xMesh != null)
			{
				xMesh.sr1BSPLeafFlags.Add(flags);
			}

			_polygons[polygonID].material.BSPTreeRootFlags = baseTreeFlags;
			_polygons[polygonID].material.BSPTreeParentNodeFlags = parentNodeFlags;
			_polygons[polygonID].material.BSPTreeAllParentNodeFlagsORd = allParentNodeFlagsORd;
			_polygons[polygonID].material.BSPTreeLeafFlags = flags;
			_polygons[polygonID].rootBSPTreeID = rootBSPTreeID;
			_polygons[polygonID].material.BSPRootTreeID = rootBSPTreeID;
			_polygons[polygonID].BSPNodeID = parentNodeID;

			for (UInt16 p = 0; p < polyCount; p++)
			{
				_polygons[polygonID + p].material.BSPTreeRootFlags = baseTreeFlags;
				_polygons[polygonID + p].material.BSPTreeParentNodeFlags = parentNodeFlags;
				_polygons[polygonID + p].material.BSPTreeAllParentNodeFlagsORd = allParentNodeFlagsORd;
				_polygons[polygonID + p].material.BSPTreeLeafFlags = flags;
				_polygons[polygonID + p].rootBSPTreeID = rootBSPTreeID;
				_polygons[polygonID + p].material.BSPRootTreeID = rootBSPTreeID;
				_polygons[polygonID + p].BSPNodeID = parentNodeID;

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
