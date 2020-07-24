using System;
using System.IO;
using System.Collections.Generic;

namespace CDC.Objects.Models
{
    public class SR1UnitModel : SR1Model
    {
        protected UInt32 m_uBspTreeCount;
        protected UInt32 m_uBspTreeStart;
        protected UInt32 m_uSpectralVertexStart;
        protected UInt32 m_uSpectralColourStart;

        public SR1UnitModel(BinaryReader xReader, UInt32 uDataStart, UInt32 uModelData, String strModelName, Platform ePlatform, UInt32 uVersion)
            : base(xReader, uDataStart, uModelData, strModelName, ePlatform, uVersion)
        {
            _modelTypePrefix = "a_";
            // struct _Terrain
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
            xReader.BaseStream.Position = _modelData + 0x10;

            // long numVertices;
            _vertexCount = xReader.ReadUInt32();
            // long numFaces;
            _polygonCount = xReader.ReadUInt32();
            // long numNormals; 
            //xReader.BaseStream.Position += 0x04;
            long numNormals = xReader.ReadUInt32();
            // struct _TVertex *vertexList;
            _vertexStart = _dataStart + xReader.ReadUInt32();
            // struct _TFace *faceList; 
            _polygonStart = _dataStart + xReader.ReadUInt32();
            // struct _Normal *normalList;
            uint normalStart = xReader.ReadUInt32();
            // struct DrMoveAniTex *aniList;
            uint drMoveAniTex = xReader.ReadUInt32();
            // 1999-02-16: struct _BSPNode *sbspRoot; // size=44, offset=44 
            // 1999-07-14: long pad 
            uint sbspRoot = xReader.ReadUInt32();
            // void *StreamUnits;
            uint streamUnitStart = xReader.ReadUInt32();

            // struct TextureFT3 *StartTextureList;
            _materialStart = _dataStart + xReader.ReadUInt32();
            _materialCount = 0;

            if (_version == SR1File.ALPHA_19990123_VERSION_1_X ||
                _version == SR1File.ALPHA_19990123_VERSION_1 ||
                _version == SR1File.ALPHA_19990204_VERSION_2 ||
                _version == SR1File.ALPHA_19990216_VERSION_3 ||
                _version == SR1File.BETA_19990512_VERSION)
            {
                // struct TextureFT3 *EndTextureList;
                // struct _SBSPLeaf *sbspStartLeaves;
                // struct _SBSPLeaf *sbspEndLeaves;

                xReader.BaseStream.Position += 0x0C;
            }
            else
            {
                // struct TextureFT3 *EndTextureList;
                xReader.BaseStream.Position += 0x04;
            }

            // struct _MorphVertex *MorphDiffList;
            m_uSpectralVertexStart = _dataStart + xReader.ReadUInt32();
            // struct _MorphColor *MorphColorList;
            m_uSpectralColourStart = _dataStart + xReader.ReadUInt32();
            // long numBSPTrees
            m_uBspTreeCount = xReader.ReadUInt32();
            // struct BSPTree *BSPTreeArray;
            m_uBspTreeStart = _dataStart + xReader.ReadUInt32();
            _groupCount = m_uBspTreeCount;
            // short *morphNormalIdx;
            // struct _MultiSignal *signals;

            _trees = new Tree[_groupCount];
        }

        public static SR1UnitModel Load(BinaryReader xReader, UInt32 uDataStart, UInt32 uModelData, String strModelName, Platform ePlatform, UInt32 uVersion, CDC.Objects.ExportOptions options)
        {
            SR1UnitModel xModel = new SR1UnitModel(xReader, uDataStart, uModelData, strModelName, ePlatform, uVersion);
            xModel.ReadData(xReader, options);
            return xModel;
        }

        protected override void ReadVertex(BinaryReader xReader, int v, CDC.Objects.ExportOptions options)
        {
            base.ReadVertex(xReader, v, options);

            _geometry.PositionsPhys[v] = _geometry.PositionsRaw[v];
            _geometry.PositionsAltPhys[v] = _geometry.PositionsPhys[v];

            _geometry.Vertices[v].colourID = v;

            xReader.BaseStream.Position += 2;
            uint vColour = xReader.ReadUInt32() | 0xFF000000;
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

        protected override void ReadVertices(BinaryReader xReader, CDC.Objects.ExportOptions options)
        {
            base.ReadVertices(xReader, options);

            ReadSpectralData(xReader, options);
        }

        protected virtual void ReadSpectralData(BinaryReader xReader, CDC.Objects.ExportOptions options)
        {
            if (m_uSpectralColourStart != 0)
            {
                // Spectral Colours
                xReader.BaseStream.Position = m_uSpectralColourStart;
                for (int v = 0; v < _vertexCount; v++)
                {
                    if (xReader.BaseStream.Position >= xReader.BaseStream.Length)
                    {
                        Console.WriteLine(string.Format("Error: reached end of file after reading {0}/{1} Spectral colours", v + 1, _vertexCount));
                        break;
                    }
                    UInt32 uShiftColour = xReader.ReadUInt16();
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
                xReader.BaseStream.Position = m_uSpectralVertexStart + 0x06;
                int sVertex = xReader.ReadInt16();
                xReader.BaseStream.Position = m_uSpectralVertexStart;
                int sVertexCount = 0;
                while (sVertex != 0xFFFF)
                {
                    if (xReader.BaseStream.Position >= xReader.BaseStream.Length)
                    {
                        Console.WriteLine(string.Format("Error: reached end of file after reading reading {0} Spectral vertices", sVertexCount));
                        break;
                    }
                    ShiftVertex xShiftVertex;
                    xShiftVertex.basePos.x = (float)xReader.ReadInt16();
                    xShiftVertex.basePos.y = (float)xReader.ReadInt16();
                    xShiftVertex.basePos.z = (float)xReader.ReadInt16();
                    sVertex = xReader.ReadUInt16();

                    if (sVertex == 0xFFFF)
                    {
                        break;
                    }

                    xShiftVertex.offset.x = (float)xReader.ReadInt16();
                    xShiftVertex.offset.y = (float)xReader.ReadInt16();
                    xShiftVertex.offset.z = (float)xReader.ReadInt16();
                    _geometry.PositionsAltPhys[sVertex] = xShiftVertex.offset + xShiftVertex.basePos;
                    sVertexCount++;
                }
            }
        }

        protected override void HandleMaterialRead(BinaryReader xReader, int p, CDC.Objects.ExportOptions options, byte flags, UInt32 colourOrMaterialPosition)
        {
            // WIP
            UInt32 uMaterialPosition = colourOrMaterialPosition + _materialStart;
            if (_version == SR1File.RETAIL_VERSION &&
                (((uMaterialPosition - _materialStart) % 0x0C) != 0) &&
                 ((uMaterialPosition - _materialStart) % 0x14) == 0)
            {
                _platform = Platform.Dreamcast;
            }

            xReader.BaseStream.Position = uMaterialPosition;
            ReadMaterial(xReader, p, options, true);
            //_polygons[p].material.textureAttributes = _polygons[p].sr1TextureFT3Attributes;
        }

        protected virtual void ReadPolygon(BinaryReader xReader, int p, CDC.Objects.ExportOptions options)
        {
            UInt32 uPolygonPosition = (UInt32)xReader.BaseStream.Position;
            // struct _TFace

            // struct _Face face; 
            _polygons[p].v1 = _geometry.Vertices[xReader.ReadUInt16()];
            _polygons[p].v2 = _geometry.Vertices[xReader.ReadUInt16()];
            _polygons[p].v3 = _geometry.Vertices[xReader.ReadUInt16()];
            _polygons[p].material = new Material();

            // unless the user has explicitly requested distinct materials for each flag, remove use of anything ignored at this level
            if (!options.DistinctMaterialsForAllFlags)
            {
                _polygons[p].material.polygonFlagsUsedMask &= 0xFD;
                _polygons[p].material.sortPushUsedMask = 0x00;
            }
            

            long flagOffset = xReader.BaseStream.Position + 2048;
            // unsigned char attr;
            byte flags = xReader.ReadByte();
            if (options.IgnorePolygonFlag2ForTerrain)
            {
                // ignore flag 0x02 for terrain (it means "this is the other half of a quad" or something similar)
                flags = (byte)(flags & 0xFD);
            }

            //Console.WriteLine(string.Format("\t\t\tDebug: read Polygon flags: {0}", Convert.ToString(flags, 2).PadLeft(8, '0')));
            //Console.WriteLine(string.Format("0x{0:X2} (@0x{1:X4}, D:{1}\t:::", flags, flagOffset));
            // char sortPush;
            byte sortPush = xReader.ReadByte();
            // unsigned short normal;
            UInt16 polygonNormal = xReader.ReadUInt16();

            _polygons[p].normal = polygonNormal;
            _polygons[p].material.polygonFlags = flags;
            _polygons[p].material.sortPush = sortPush;

            // unsigned short textoff;
            UInt16 uMaterialOffset = xReader.ReadUInt16();
            //Console.WriteLine(string.Format("\t\t\tDebug: read textoff: 0x{0:X4}", uMaterialOffset));
            //_polygons[p].material.textureUsed &= (Boolean)(uMaterialOffset != 0xFFFF);

            bool isTranslucent = false;

            _polygons[p].material.visible = true;
            if (uMaterialOffset == 0xFFFF)
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
                if (uMaterialOffset == 0x0000)
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

            HandlePolygonInfo(xReader, p, options, flags, uMaterialOffset, isTranslucent);

            xReader.BaseStream.Position = uPolygonPosition + 0x0C;
        }

        protected override void ReadPolygons(BinaryReader xReader, CDC.Objects.ExportOptions options)
        {
            if (_polygonStart == 0 || _polygonCount == 0)
            {
                return;
            }

            xReader.BaseStream.Position = _polygonStart;

            for (UInt16 p = 0; p < _polygonCount; p++)
            {
                //Console.WriteLine(string.Format("\t\tReading polygon {0}", p));
                ReadPolygon(xReader, p, options);
            }

            List<Mesh> xMeshes = new List<Mesh>();
            List<int> xMeshPositions = new List<int>();
            List<UInt32> treePolygons = new List<UInt32>((Int32)_vertexCount * 3);

            for (UInt32 t = 0; t < m_uBspTreeCount; t++)
            {
                // struct BSPTree
                xReader.BaseStream.Position = m_uBspTreeStart + (t * 0x24);
                // struct _BSPNode *bspRoot;
                UInt32 uDataPos = _dataStart + xReader.ReadUInt32();
                // struct _BSPLeaf *startLeaves;
                // struct _BSPLeaf *endLeaves; 
                // struct _Position globalOffset;
                xReader.BaseStream.Position += 0x12;
                //short flags;
                ushort rootTreeFlags = xReader.ReadUInt16();
                //Console.WriteLine(string.Format("\t\t\t\t\tDebug: read BSP tree flags {0}", Convert.ToString(flags, 2).PadLeft(8, '0')));
                bool drawTester = ((rootTreeFlags & 1) != 1);
                // struct _Position localOffset;
                xReader.BaseStream.Position += 0x06;
                // short ID;
                UInt16 usBspID = xReader.ReadUInt16();
                // long splineID;
                // struct _Instance *instanceSpline;
                //if (_trees[t] == null)
                //{
                //    _trees[t] = new Tree();
                //}
                //_trees[t].sr1Flags = flags;
                _trees[t] = ReadBSPTree(t, t.ToString(), xReader, treePolygons, uDataPos, _trees[t], xMeshes, xMeshPositions, 0, rootTreeFlags, 0, 0);                
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

        protected virtual Tree ReadBSPTree(uint rootTreeNum, string treeNodeID, BinaryReader xReader, List<UInt32> treePolygons, UInt32 uDataPos, Tree xParentTree, List<Mesh> xMeshes, 
            List<int> xMeshPositions, UInt32 uDepth, UInt16 treeRootFlags, UInt16 parentNodeFlags, UInt16 parentNodeFlagsORd)
        {
            if (uDataPos == 0)
            {
                return null;
            }

            xReader.BaseStream.Position = uDataPos + 0x0E;
            bool isLeaf = ((xReader.ReadByte() & 0x02) == 0x02);
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

                xReader.BaseStream.Position = uDataPos + 0x08;
                ReadBSPLeaf(xReader, treePolygons, xMesh, treeRootFlags, parentNodeFlags, parentNodeFlagsORd, rootTreeNum, treeNodeID);
            }
            else
            {
                // xReader.BaseStream.Position = uDataPos + 0x14;
                // struct _BSPNode
                // struct _Sphere_noSq sphere;
                // short a;
                // short b;
                // short c;
                xReader.BaseStream.Position = uDataPos + 0xE;
                UInt16 nodeFlags = xReader.ReadUInt16();
                UInt16 nodeFlagsORd = (UInt16)(parentNodeFlagsORd | nodeFlags);
                //Console.WriteLine(string.Format("\t\t\t\t\tDebug: read BSP node flags {0}", Convert.ToString(nodeFlags, 2).PadLeft(8, '0')));
                //xMesh.sr1BSPNodeFlags.Add(nodeFlags);
                //long d;
                xReader.BaseStream.Position += 4;

                // void *front;
                // void *back; 
                UInt32[] auSubTreePositions = new UInt32[2];
                for (Int32 s = 0; s < iSubTreeCount; s++)
                {
                    auSubTreePositions[s] = xReader.ReadUInt32();
                }

                for (Int32 s = 0; s < iSubTreeCount; s++)
                {
                    ReadBSPTree(rootTreeNum, string.Format("{0}-{1}", treeNodeID, s), xReader, treePolygons, auSubTreePositions[s], xTree, xMeshes, xMeshPositions, uDepth + 1, treeRootFlags, nodeFlags, nodeFlagsORd);
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

        protected virtual void ReadBSPLeaf(BinaryReader xReader, List<UInt32> treePolygons, Mesh xMesh, UInt16 baseTreeFlags, UInt16 parentNodeFlags, UInt16 allParentNodeFlagsORd, uint rootBSPTreeNum, string parentNodeID)
        {
            // struct _BSPLeaf 
            // struct _TFace *faceList;
            UInt32 polygonPos = _dataStart + xReader.ReadUInt32();
            UInt32 polygonID = (polygonPos - _polygonStart) / 0x0C;
            // short numFaces;
            UInt16 polyCount = xReader.ReadUInt16();
            // short flags; 
            UInt16 flags = xReader.ReadUInt16();
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

                //BSP Tree material handling
                if ((_polygons[polygonID + p].material.BSPTreeParentNodeFlags & 0x1) != 0x1)
                {
                    _polygons[polygonID + p].material.visible = false;
                }

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
 