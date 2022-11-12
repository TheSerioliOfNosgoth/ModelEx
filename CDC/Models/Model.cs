using System;
using System.Collections.Generic;
using System.IO;

namespace CDC
{
	public abstract class Model : IModel
	{
		public abstract string Name { get; }
		public abstract string ModelTypePrefix { get; }
		public abstract Polygon[] Polygons { get; }
		public abstract Geometry Geometry { get; }
		public abstract Geometry ExtraGeometry { get; }
		public abstract Bone[] Bones { get; }
		public abstract Tree[] Groups { get; }
		public abstract Material[] Materials { get; }
		public abstract Platform Platform { get; }

		public abstract string GetTextureName(int materialIndex, ExportOptions options);

		protected virtual void HandleDebugRendering(int p, ExportOptions options)
		{
			if (options.RenderMode == RenderMode.Standard ||
				options.RenderMode == RenderMode.Wireframe)
			{
				return;
			}

			ref Polygon polygon = ref Polygons[p];
			ref Material material = ref polygon.material;

			material.textureUsed = false;
			if ((options.MakeAllPolygonsVisible) || (options.MakeAllPolygonsOpaque) || (!options.DiscardNonVisible))
			{
				material.visible = true;
			}
			//material.visible = true;
			material.emissivity = 0.0f;
			if (options.RenderMode == RenderMode.NoTextures)
			{
				//return;
				//material.colour = |= 0xFF000000;
			}

			//material.colour = (TransparentMaterial & 0xFF000000) | 0x404040;

			if (options.SetAllPolygonColoursToValue)
			{
				//material.colour |= 0x0000FF00;
				material.colour = Utility.FloatARGBToUInt32ARGB(new float[] { options.PolygonColourAlpha, options.PolygonColourRed, options.PolygonColourGreen, options.PolygonColourBlue });
				material.opacity = options.PolygonColourAlpha;
			}
			else
			{
				material.colour = 0xFF404040;
				material.opacity = 0.75f;
			}
			//polygon.v1.colourID = 0;
			//polygon.v2.colourID = 0;
			//polygon.v3.colourID = 0;

			#region Polygon flags
			if (options.RenderMode == RenderMode.DebugPolygonFlagsHash)
			{
				Utility.ColourPolygonFromUInt(ref polygon, material.polygonFlags);
			}
			if (options.RenderMode == RenderMode.DebugPolygonFlags1)
			{
				Utility.ColourPolygonFromFlags(ref polygon, material.polygonFlags, 0x0001, 0x0002, 0x0004);
				// 0x01 = hidden polygon (portal, collision box, etc.), not sure why sometimes it's this and sometimes 0x40
				// 0x02 = inverted half of quad? It's one triangle out of every quad in a lot of models (but not all)
				// 0x04 = hidden polygon
			}
			if (options.RenderMode == RenderMode.DebugPolygonFlags2)
			{
				Utility.ColourPolygonFromFlags(ref polygon, material.polygonFlags, 0x0008, 0x0010, 0x0020);
				// 0x08 = translucent? glowing? - used for the water in tower10 (1999-02-16), lthbeam  (1999-02-16), morlock eyes (1999-05-12), Kain's feet (1999-05-12)
				// 0x10 = glowing? reflective? specular? That Z-depth thing Andrew mentioned? - water in cityout3 is cyan, not used in cathy69, pipes and platforms in intvaly1 are green, used for the columns in tower10 (1999-02-16)
				// 0x20 = translucent - water in cityout3 is cyan, not used in cathy69, water in intvaly1 is blue, lava in lair1 is blue
			}
			if (options.RenderMode == RenderMode.DebugPolygonFlags3)
			{
				Utility.ColourPolygonFromFlags(ref polygon, material.polygonFlags, 0x0040, 0x0080, 0x0100);
				// 0x40 = hidden polygon (portal, collision box, etc.), not sure why sometimes it's this and sometimes 0x01
			}
			if (options.RenderMode == RenderMode.DebugPolygonFlagsSoulReaverA)
			{
				byte tempFlags = material.polygonFlags;
				if ((material.polygonFlags & 0x40) == 0x40)
				{
					tempFlags |= 0x01;
				}
				Utility.ColourPolygonFromFlags(ref polygon, material.polygonFlags, 0x0001, 0x0020, 0x0010);
				// hidden polygons == red
				// transparent/translucent == green
				// glowing? reflective? == blue
			}
			#endregion

			#region Texture
			#region Texture attributes
			if (options.RenderMode == RenderMode.DebugTextureAttributesHash)
			{
				Utility.ColourPolygonFromUInt(ref polygon, material.textureAttributes);
			}
			if (options.RenderMode == RenderMode.DebugTextureAttributes1)
			{
				Utility.ColourPolygonFromFlags(ref polygon, material.textureAttributes, 0x0001, 0x0002, 0x0004);
			}
			if (options.RenderMode == RenderMode.DebugTextureAttributes2)
			{
				Utility.ColourPolygonFromFlags(ref polygon, material.textureAttributes, 0x0008, 0x0010, 0x0020);
				// 0x10 = alphamasked terrain
				// the glass shell around the lighthouse in intvaly1 has this flag
				// tiny squares below the wall sconces in nighta2 (1999-01-23) have this flag too 
				// stair coverings and the bridge in cliff1 (release version) have this flag
				// flags in city2 (NTSC release)
			}
			if (options.RenderMode == RenderMode.DebugTextureAttributes3)
			{
				Utility.ColourPolygonFromFlags(ref polygon, material.textureAttributes, 0x0040, 0x0080, 0x0100);
				// 0x40 = translucent terrain, e.g. water, glass
			}
			if (options.RenderMode == RenderMode.DebugTextureAttributes4)
			{
				Utility.ColourPolygonFromFlags(ref polygon, material.textureAttributes, 0x0200, 0x0400, 0x0800);
				// 0x0200 = climbable walls? city2 (NTSC release)
			}
			if (options.RenderMode == RenderMode.DebugTextureAttributes5)
			{
				Utility.ColourPolygonFromFlags(ref polygon, material.textureAttributes, 0x1000, 0x2000, 0x4000);
				// 0x1000 = parts of the gate in nighta1 (1999-02-16) have this flag
				// 0x2000 = waterfall in intvaly1 (1999-01-23) has this flag - maybe animated texture, or translucency?
				//          Parts of the water in cliff1 have it too
				// 0x4000 = most of the intvaly1 terrain mesh has this flag
			}
			if (options.RenderMode == RenderMode.DebugTextureAttributes6)
			{
				Utility.ColourPolygonFromFlags(ref polygon, material.textureAttributes, 0x10000, 0x8000, 0x20000);
				// 0x8000 = lighting effects? i.e. invisible, animated polygon that only affects vertex colours?
			}
			#endregion

			#region Texture attributes A
			if (options.RenderMode == RenderMode.DebugTextureAttributesAHash)
			{
				Utility.ColourPolygonFromUInt(ref polygon, material.textureAttributesA);
			}
			if (options.RenderMode == RenderMode.DebugTextureAttributesA1)
			{
				Utility.ColourPolygonFromFlags(ref polygon, material.textureAttributesA, 0x0001, 0x0002, 0x0004);
			}
			if (options.RenderMode == RenderMode.DebugTextureAttributesA2)
			{
				Utility.ColourPolygonFromFlags(ref polygon, material.textureAttributesA, 0x0008, 0x0010, 0x0020);
			}
			if (options.RenderMode == RenderMode.DebugTextureAttributesA3)
			{
				Utility.ColourPolygonFromFlags(ref polygon, material.textureAttributesA, 0x0040, 0x0080, 0x0100);
			}
			if (options.RenderMode == RenderMode.DebugTextureAttributesA4)
			{
				Utility.ColourPolygonFromFlags(ref polygon, material.textureAttributesA, 0x0200, 0x0400, 0x0800);
			}
			if (options.RenderMode == RenderMode.DebugTextureAttributesA5)
			{
				Utility.ColourPolygonFromFlags(ref polygon, material.textureAttributesA, 0x1000, 0x2000, 0x4000);
			}
			if (options.RenderMode == RenderMode.DebugTextureAttributesA6)
			{
				Utility.ColourPolygonFromFlags(ref polygon, material.textureAttributesA, 0x10000, 0x8000, 0x10000);
			}
			#endregion

			#region CLUT

			ushort clutNonRowColumnBits = (ushort)((material.clutValue & 0xC000) >> 11);
			clutNonRowColumnBits |= (ushort)((material.clutValue & 0x00E0) >> 5);

			if (options.RenderMode == RenderMode.DebugCLUTNonRowColBitsHash)
			{
				Utility.ColourPolygonFromUInt(ref polygon, clutNonRowColumnBits);
			}
			if (options.RenderMode == RenderMode.DebugCLUTNonRowColBits1)
			{
				Utility.ColourPolygonFromFlags(ref polygon, clutNonRowColumnBits, 0x0001, 0x0002, 0x0004);
			}
			if (options.RenderMode == RenderMode.DebugCLUTNonRowColBits2)
			{
				Utility.ColourPolygonFromFlags(ref polygon, clutNonRowColumnBits, 0x0008, 0x0010, 0x10000);
			}
			if (options.RenderMode == RenderMode.DebugCLUTHash)
			{
				Utility.ColourPolygonFromUInt(ref polygon, material.clutValue);
			}
			if (options.RenderMode == RenderMode.DebugCLUT1)
			{
				Utility.ColourPolygonFromFlags(ref polygon, material.clutValue, 0x0001, 0x0002, 0x0004);
			}
			if (options.RenderMode == RenderMode.DebugCLUT2)
			{
				Utility.ColourPolygonFromFlags(ref polygon, material.clutValue, 0x0008, 0x0010, 0x0020);
			}
			if (options.RenderMode == RenderMode.DebugCLUT3)
			{
				Utility.ColourPolygonFromFlags(ref polygon, material.clutValue, 0x0040, 0x0080, 0x0100);
			}
			if (options.RenderMode == RenderMode.DebugCLUT4)
			{
				Utility.ColourPolygonFromFlags(ref polygon, material.clutValue, 0x0200, 0x0400, 0x0800);
			}
			if (options.RenderMode == RenderMode.DebugCLUT5)
			{
				Utility.ColourPolygonFromFlags(ref polygon, material.clutValue, 0x1000, 0x2000, 0x4000);
			}
			if (options.RenderMode == RenderMode.DebugCLUT6)
			{
				Utility.ColourPolygonFromFlags(ref polygon, material.clutValue, 0x10000, 0x8000, 0x10000);
			}
			#endregion

			#region Texture page
			if (options.RenderMode == RenderMode.DebugTexturePageHash)
			{
				Utility.ColourPolygonFromUInt(ref polygon, material.texturePage);
			}
			if (options.RenderMode == RenderMode.DebugTexturePageUpper28BitsHash)
			{
				//Utility.ColourPolygonFromUInt(ref polygon, (material.texturePage & 0xFFFFFF00));
				Utility.ColourPolygonFromUInt(ref polygon, (material.texturePage & 0xFFFFFFF0));
			}
			if (options.RenderMode == RenderMode.DebugTexturePageUpper5BitsHash)
			{
				//Utility.ColourPolygonFromUInt(p, (material.texturePage & 0xFFFFFF00));
				Utility.ColourPolygonFromUInt(ref polygon, (material.texturePage & 0xFFFFF800));
			}
			if (options.RenderMode == RenderMode.DebugTexturePage1)
			{
				Utility.ColourPolygonFromFlags(ref polygon, material.texturePage, 0x0001, 0x0002, 0x0004);
			}
			if (options.RenderMode == RenderMode.DebugTexturePage2)
			{
				Utility.ColourPolygonFromFlags(ref polygon, material.texturePage, 0x0008, 0x0010, 0x0020);
				// 0x0020 == glowing?
			}
			if (options.RenderMode == RenderMode.DebugTexturePage3)
			{
				Utility.ColourPolygonFromFlags(ref polygon, material.texturePage, 0x0040, 0x0080, 0x0100);
				// 0x0040 == glowing brightly?
			}
			if (options.RenderMode == RenderMode.DebugTexturePage4)
			{
				Utility.ColourPolygonFromFlags(ref polygon, material.texturePage, 0x0200, 0x0400, 0x0800);
			}
			if (options.RenderMode == RenderMode.DebugTexturePage5)
			{
				Utility.ColourPolygonFromFlags(ref polygon, material.texturePage, 0x1000, 0x2000, 0x4000);
			}
			if (options.RenderMode == RenderMode.DebugTexturePage6)
			{
				Utility.ColourPolygonFromFlags(ref polygon, material.texturePage, 0x10000, 0x8000, 0x10000);
			}
			#endregion
			#endregion

			#region Sort push flags
			if (options.RenderMode == RenderMode.DebugSortPushHash)
			{
				Utility.ColourPolygonFromUInt(ref polygon, material.sortPush);
			}
			if (options.RenderMode == RenderMode.DebugSortPushFlags1)
			{
				Utility.ColourPolygonFromFlags(ref polygon, material.sortPush, 0x0001, 0x0002, 0x0004);
			}
			if (options.RenderMode == RenderMode.DebugSortPushFlags2)
			{
				Utility.ColourPolygonFromFlags(ref polygon, material.sortPush, 0x0008, 0x0010, 0x0020);
			}
			if (options.RenderMode == RenderMode.DebugSortPushFlags3)
			{
				Utility.ColourPolygonFromFlags(ref polygon, material.sortPush, 0x0040, 0x0080, 0x0100);
			}
			#endregion

			if (options.RenderMode == RenderMode.AverageVertexAlpha)
			{
				material.opacity = 0.75f;
				float[] fV1 = Utility.UInt32ARGBToFloatARGB(Geometry.Colours[polygon.v1.colourID]);
				float[] fV2 = Utility.UInt32ARGBToFloatARGB(Geometry.Colours[polygon.v2.colourID]);
				float[] fV3 = Utility.UInt32ARGBToFloatARGB(Geometry.Colours[polygon.v3.colourID]);
				float[] fVAverage = new float[4];
				fVAverage[0] = 1.0f;
				fVAverage[1] = (fV1[0] + fV2[0] + fV3[0]) / 3.0f;
				fVAverage[2] = 1.0f;
				fVAverage[3] = (fV1[0] + fV2[0] + fV3[0]) / 3.0f;
				material.colour = Utility.FloatARGBToUInt32ARGB(fVAverage);
			}

			if (options.RenderMode == RenderMode.PolygonAlpha)
			{
				float[] fPoly = Utility.UInt32ARGBToFloatARGB(material.colour);
				float[] fVAverage = new float[4];
				fVAverage[0] = 0.75f;
				fVAverage[1] = fPoly[0];
				fVAverage[2] = 1.0f;
				fVAverage[3] = fPoly[0];
				material.colour = Utility.FloatARGBToUInt32ARGB(fVAverage);
				material.opacity = 0.75f;
			}

			if (options.RenderMode == RenderMode.PolygonOpacity)
			{
				float[] fVAverage = new float[4];
				fVAverage[0] = 0.75f;
				fVAverage[1] = material.opacity;
				fVAverage[2] = 1.0f;
				fVAverage[3] = material.opacity;
				material.colour = Utility.FloatARGBToUInt32ARGB(fVAverage);
				material.opacity = 0.75f;
			}

			if (options.RenderMode == RenderMode.DebugBoneIDHash)
			{
				uint boneIDColourV1 = Utility.GetColourFromHash(Utility.GetHashOfUInt((uint)polygon.v1.boneID));
				uint boneIDColourV2 = Utility.GetColourFromHash(Utility.GetHashOfUInt((uint)polygon.v2.boneID));
				uint boneIDColourV3 = Utility.GetColourFromHash(Utility.GetHashOfUInt((uint)polygon.v3.boneID));
				Geometry.Colours[polygon.v1.colourID] = boneIDColourV1;
				Geometry.Colours[polygon.v2.colourID] = boneIDColourV2;
				Geometry.Colours[polygon.v3.colourID] = boneIDColourV3;
				if ((boneIDColourV1 == boneIDColourV2) && (boneIDColourV2 == boneIDColourV3))
				{
					material.opacity = 1.0f;
					material.colour = boneIDColourV1;
				}
				else
				{
					if (options.InterpolatePolygonColoursWhenColouringBasedOnVertices)
					{
						// average the vertex colours and make the polygon translucent to highlight the fact that it's dependent on multiple bones.
						material.opacity = 0.25f;
						float[] fV1 = Utility.UInt32ARGBToFloatARGB(boneIDColourV1);
						float[] fV2 = Utility.UInt32ARGBToFloatARGB(boneIDColourV2);
						float[] fV3 = Utility.UInt32ARGBToFloatARGB(boneIDColourV3);
						float[] fVAverage = new float[4];
						fVAverage[0] = (fV1[0] + fV2[0] + fV3[0]) / 3.0f;
						fVAverage[1] = (fV1[1] + fV2[1] + fV3[1]) / 3.0f;
						fVAverage[2] = (fV1[2] + fV2[2] + fV3[2]) / 3.0f;
						fVAverage[3] = (fV1[3] + fV2[3] + fV3[3]) / 3.0f;
						material.colour = Utility.FloatARGBToUInt32ARGB(fVAverage);
					}
					else
					{
						material.opacity = 0.5f;
						material.colour = 0xFFFFFFFF;
					}
				}
			}

			#region BSP tree
			if (options.RenderMode == RenderMode.DebugBSPRootTreeNumber)
			{
				Utility.ColourPolygonFromUInt(ref polygon, unchecked((uint)polygon.rootBSPTreeID));
			}

			if (options.RenderMode == RenderMode.DebugBSPTreeNodeID)
			{
				Utility.ColourPolygonFromString(ref polygon, polygon.BSPNodeID);
			}

			#region BSP tree root flags
			if (options.RenderMode == RenderMode.DebugBSPRootTreeFlagsHash)
			{
				Utility.ColourPolygonFromUInt(ref polygon, material.BSPTreeRootFlags);
			}
			if (options.RenderMode == RenderMode.DebugBSPRootTreeFlags1)
			{
				Utility.ColourPolygonFromFlags(ref polygon, material.BSPTreeRootFlags, 0x0001, 0x0002, 0x0004);
			}
			if (options.RenderMode == RenderMode.DebugBSPRootTreeFlags2)
			{
				Utility.ColourPolygonFromFlags(ref polygon, material.BSPTreeRootFlags, 0x0008, 0x0010, 0x0020);
			}
			if (options.RenderMode == RenderMode.DebugBSPRootTreeFlags3)
			{
				Utility.ColourPolygonFromFlags(ref polygon, material.BSPTreeRootFlags, 0x0040, 0x0080, 0x0100);
			}
			if (options.RenderMode == RenderMode.DebugBSPRootTreeFlags4)
			{
				Utility.ColourPolygonFromFlags(ref polygon, material.BSPTreeRootFlags, 0x0200, 0x0400, 0x0800);
			}
			if (options.RenderMode == RenderMode.DebugBSPRootTreeFlags5)
			{
				Utility.ColourPolygonFromFlags(ref polygon, material.BSPTreeRootFlags, 0x1000, 0x2000, 0x4000);
			}
			if (options.RenderMode == RenderMode.DebugBSPRootTreeFlags6)
			{
				Utility.ColourPolygonFromFlags(ref polygon, material.BSPTreeRootFlags, 0x10000, 0x8000, 0x10000);
			}
			#endregion

			#region BSP tree parent flags
			if (options.RenderMode == RenderMode.DebugBSPTreeImmediateParentFlagsHash)
			{
				Utility.ColourPolygonFromUInt(ref polygon, material.BSPTreeParentNodeFlags);
			}
			if (options.RenderMode == RenderMode.DebugBSPTreeImmediateParentFlags1)
			{
				Utility.ColourPolygonFromFlags(ref polygon, material.BSPTreeParentNodeFlags, 0x0001, 0x0002, 0x0004);
			}
			if (options.RenderMode == RenderMode.DebugBSPTreeImmediateParentFlags2)
			{
				Utility.ColourPolygonFromFlags(ref polygon, material.BSPTreeParentNodeFlags, 0x0008, 0x0010, 0x0020);
			}
			if (options.RenderMode == RenderMode.DebugBSPTreeImmediateParentFlags3)
			{
				Utility.ColourPolygonFromFlags(ref polygon, material.BSPTreeParentNodeFlags, 0x0040, 0x0080, 0x0100);
			}
			if (options.RenderMode == RenderMode.DebugBSPTreeImmediateParentFlags4)
			{
				Utility.ColourPolygonFromFlags(ref polygon, material.BSPTreeParentNodeFlags, 0x0200, 0x0400, 0x0800);
			}
			if (options.RenderMode == RenderMode.DebugBSPTreeImmediateParentFlags5)
			{
				Utility.ColourPolygonFromFlags(ref polygon, material.BSPTreeParentNodeFlags, 0x1000, 0x2000, 0x4000);
			}
			if (options.RenderMode == RenderMode.DebugBSPTreeImmediateParentFlags6)
			{
				Utility.ColourPolygonFromFlags(ref polygon, material.BSPTreeParentNodeFlags, 0x10000, 0x8000, 0x10000);
			}
			#endregion

			#region BSP tree parent flags ORd
			ushort allParentFlagsORd = material.BSPTreeAllParentNodeFlagsORd;
			if (options.BSPRenderingIncludeLeafFlagsWhenORing)
			{
				allParentFlagsORd |= material.BSPTreeLeafFlags;
			}
			if (options.BSPRenderingIncludeRootTreeFlagsWhenORing)
			{
				allParentFlagsORd |= material.BSPTreeRootFlags;
			}
			if (options.RenderMode == RenderMode.DebugBSPTreeAllParentFlagsORdHash)
			{
				Utility.ColourPolygonFromUInt(ref polygon, allParentFlagsORd);
			}
			if (options.RenderMode == RenderMode.DebugBSPTreeAllParentFlagsORd1)
			{
				Utility.ColourPolygonFromFlags(ref polygon, allParentFlagsORd, 0x0001, 0x0002, 0x0004);
			}
			if (options.RenderMode == RenderMode.DebugBSPTreeImmediateParentFlags2)
			{
				Utility.ColourPolygonFromFlags(ref polygon, allParentFlagsORd, 0x0008, 0x0010, 0x0020);
			}
			if (options.RenderMode == RenderMode.DebugBSPTreeImmediateParentFlags3)
			{
				Utility.ColourPolygonFromFlags(ref polygon, allParentFlagsORd, 0x0040, 0x0080, 0x0100);
			}
			if (options.RenderMode == RenderMode.DebugBSPTreeImmediateParentFlags4)
			{
				Utility.ColourPolygonFromFlags(ref polygon, allParentFlagsORd, 0x0200, 0x0400, 0x0800);
			}
			if (options.RenderMode == RenderMode.DebugBSPTreeImmediateParentFlags5)
			{
				Utility.ColourPolygonFromFlags(ref polygon, allParentFlagsORd, 0x1000, 0x2000, 0x4000);
			}
			if (options.RenderMode == RenderMode.DebugBSPTreeImmediateParentFlags6)
			{
				Utility.ColourPolygonFromFlags(ref polygon, allParentFlagsORd, 0x10000, 0x8000, 0x10000);
			}
			#endregion

			#region BSP tree leaf flags
			if (options.RenderMode == RenderMode.DebugBSPTreeLeafFlagsHash)
			{
				Utility.ColourPolygonFromUInt(ref polygon, material.BSPTreeLeafFlags);
			}
			if (options.RenderMode == RenderMode.DebugBSPTreeLeafFlags1)
			{
				Utility.ColourPolygonFromFlags(ref polygon, material.BSPTreeLeafFlags, 0x0001, 0x0002, 0x0004);
			}
			if (options.RenderMode == RenderMode.DebugBSPTreeLeafFlags2)
			{
				Utility.ColourPolygonFromFlags(ref polygon, material.BSPTreeLeafFlags, 0x0008, 0x0010, 0x0020);
			}
			if (options.RenderMode == RenderMode.DebugBSPTreeLeafFlags3)
			{
				Utility.ColourPolygonFromFlags(ref polygon, material.BSPTreeLeafFlags, 0x0040, 0x0080, 0x0100);
			}
			if (options.RenderMode == RenderMode.DebugBSPTreeLeafFlags4)
			{
				Utility.ColourPolygonFromFlags(ref polygon, material.BSPTreeLeafFlags, 0x0200, 0x0400, 0x0800);
			}
			if (options.RenderMode == RenderMode.DebugBSPTreeLeafFlags5)
			{
				Utility.ColourPolygonFromFlags(ref polygon, material.BSPTreeLeafFlags, 0x1000, 0x2000, 0x4000);
			}
			if (options.RenderMode == RenderMode.DebugBSPTreeLeafFlags6)
			{
				Utility.ColourPolygonFromFlags(ref polygon, material.BSPTreeLeafFlags, 0x10000, 0x8000, 0x10000);
			}
			#endregion
			#endregion

			// default to grey for non-coloured elements - black makes it too hard to see anything in ModelEx

			if ((material.colour & 0x00FFFFFF) == 0x00000000)
			{
				material.colour |= 0x80D0D0D0;
			}
		}
	}
}
