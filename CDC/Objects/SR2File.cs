using System;
using System.IO;
using System.Collections.Generic;

namespace CDC.Objects
{
    public class SR2File : SRFile
    {
        #region Model classes

        protected class SR2ObjectModel : SR2Model
        {
            protected UInt32 m_uColourStart;

            protected SR2ObjectModel(BinaryReader xReader, UInt32 uDataStart, UInt32 uModelData, String strModelName, Platform ePlatform, UInt32 uVersion)
                : base (xReader, uDataStart, uModelData, strModelName, ePlatform, uVersion)
            {
                xReader.BaseStream.Position = _modelData + 0x04;
                UInt32 uBoneCount1          = xReader.ReadUInt32();
                UInt32 uBoneCount2          = xReader.ReadUInt32();
                _boneCount                = uBoneCount1 + uBoneCount2;
                _boneStart                = xReader.ReadUInt32();
                _vertexScale.x            = xReader.ReadSingle();
                _vertexScale.y            = xReader.ReadSingle();
                _vertexScale.z            = xReader.ReadSingle();
                xReader.BaseStream.Position += 0x04;
                _vertexCount              = xReader.ReadUInt32();
                _vertexStart              = _dataStart + xReader.ReadUInt32();
                xReader.BaseStream.Position += 0x08;
                _polygonCount             = 0; // xReader.ReadUInt32();
                _polygonStart             = 0; // m_uDataStart + xReader.ReadUInt32();
                xReader.BaseStream.Position += 0x18;
                m_uColourStart              = xReader.ReadUInt32();
                xReader.BaseStream.Position += 0x0C;
                _materialStart            = _dataStart + xReader.ReadUInt32();
                _materialCount            = 0;
                _groupCount                = 1;

                _trees = new Tree[_groupCount];
            }

            public static SR2ObjectModel Load(BinaryReader xReader, UInt32 uDataStart, UInt32 uModelData, String strModelName, Platform ePlatform, UInt16 usIndex, UInt32 uVersion)
            {
                xReader.BaseStream.Position = uModelData + (0x00000004 * usIndex);
                uModelData = uDataStart + xReader.ReadUInt32();
                xReader.BaseStream.Position = uModelData;
                SR2ObjectModel xModel = new SR2ObjectModel(xReader, uDataStart, uModelData, strModelName, ePlatform, uVersion);
                xModel.ReadData(xReader);
                return xModel;
            }

            protected override void ReadVertex(BinaryReader xReader, int v)
            {
                base.ReadVertex(xReader, v);

                _positionsPhys[v] = _positionsRaw[v] * _vertexScale;
                _positionsAltPhys[v] = _positionsPhys[v];

                _vertices[v].normalID = xReader.ReadUInt16();
                xReader.BaseStream.Position += 0x02;

                _vertices[v].UVID = v;

                UInt16 vU = xReader.ReadUInt16();
                UInt16 vV = xReader.ReadUInt16();

                _uvs[v].u = Utility.BizarreFloatToNormalFloat(vU);
                _uvs[v].v = Utility.BizarreFloatToNormalFloat(vV);
            }

            protected override void ReadVertices(BinaryReader xReader)
            {
                base.ReadVertices(xReader);

                xReader.BaseStream.Position = m_uColourStart;
                for (UInt16 v = 0; v < _vertexCount; v++)
                {
                    _colours[v] = xReader.ReadUInt32();
                    _coloursAlt[v] = _colours[v];
                }

                ReadArmature(xReader);
                ApplyArmature();
            }

            protected virtual void ReadArmature(BinaryReader xReader)
            {
                if (_boneStart == 0 || _boneCount == 0) return;

                xReader.BaseStream.Position = _boneStart;
                _bones = new Bone[_boneCount];
                for (UInt16 b = 0; b < _boneCount; b++)
                {
                    // Get the bone data
                    _bones[b].localPos.x = xReader.ReadSingle();
                    _bones[b].localPos.y = xReader.ReadSingle();
                    _bones[b].localPos.z = xReader.ReadSingle();

                    float unknown = xReader.ReadSingle();
                    _bones[b].flags = xReader.ReadUInt32();

                    _bones[b].vFirst = xReader.ReadUInt16();
                    _bones[b].vLast = xReader.ReadUInt16();

                    _bones[b].parentID1 = xReader.ReadUInt16();
                    _bones[b].parentID2 = xReader.ReadUInt16();

                    //if (parent1 != 0xFFFF && parent2 != 0xFFFF &&
                    //    parent2 != 0)
                    if (_bones[b].flags == 8)
                    {
                        _bones[b].parentID1 = _bones[b].parentID2;
                    }

                    xReader.BaseStream.Position += 0x04;
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
                            _positionsPhys[v] += _bones[b].worldPos;
                            _vertices[v].boneID = b;
                        }
                    }
                }
                return;
            }

            protected override void ReadPolygons(BinaryReader xReader)
            {
                if (_materialStart == 0)
                {
                    return;
                }

                List<SR2TriangleList> xTriangleListList = new List<SR2TriangleList>();
                UInt32 uMaterialPosition = _materialStart;
                _groupCount = 0;
                while (uMaterialPosition != 0)
                {
                    xReader.BaseStream.Position = uMaterialPosition;
                    SR2TriangleList xTriangleList = new SR2TriangleList();

                    if (ReadTriangleList(xReader, ref xTriangleList)/* && xTriangleList.m_usGroupID == 0*/)
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

                    foreach (SR2TriangleList xTriangleList in xTriangleListList)
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
                    foreach (SR2TriangleList xTriangleList in xTriangleListList)
                    {
                        if (t != (UInt32)xTriangleList.m_usGroupID)
                        {
                            continue;
                        }

                        xReader.BaseStream.Position = xTriangleList.m_uPolygonStart;
                        for (int pl = 0; pl < xTriangleList.m_uPolygonCount; pl++)
                        {
                            _trees[t].mesh.polygons[tp].v1 = _vertices[xReader.ReadUInt16()];
                            _trees[t].mesh.polygons[tp].v2 = _vertices[xReader.ReadUInt16()];
                            _trees[t].mesh.polygons[tp].v3 = _vertices[xReader.ReadUInt16()];
                            _trees[t].mesh.polygons[tp].material = xTriangleList.m_xMaterial;
                            tp++;
                        }
                    }

                    // Make the vertices unique - Because I do the same thing in GenerateOutput
                    for (UInt16 poly = 0; poly < _trees[t].mesh.polygonCount; poly++)
                    {
                        _trees[t].mesh.vertices[(3 * poly) + 0] = _trees[t].mesh.polygons[poly].v1;
                        _trees[t].mesh.vertices[(3 * poly) + 1] = _trees[t].mesh.polygons[poly].v2;
                        _trees[t].mesh.vertices[(3 * poly) + 2] = _trees[t].mesh.polygons[poly].v3;
                    }
                }

                _polygons = new Polygon[_polygonCount];
                UInt32 p = 0;
                foreach (SR2TriangleList xTriangleList in xTriangleListList)
                {
                    xReader.BaseStream.Position = xTriangleList.m_uPolygonStart;
                    for (int pl = 0; pl < xTriangleList.m_uPolygonCount; pl++)
                    {
                        _polygons[p].v1 = _vertices[xReader.ReadUInt16()];
                        _polygons[p].v2 = _vertices[xReader.ReadUInt16()];
                        _polygons[p].v3 = _vertices[xReader.ReadUInt16()];
                        _polygons[p].material = xTriangleList.m_xMaterial;
                        p++;
                    }
                }
            }

            protected virtual bool ReadTriangleList(BinaryReader xReader, ref SR2TriangleList xTriangleList)
            {
                xTriangleList.m_uPolygonCount = (UInt32)xReader.ReadUInt16() / 3;
                xTriangleList.m_usGroupID = xReader.ReadUInt16(); // Used by MON_SetAccessories and INSTANCE_UnhideAllDrawGroups
                xTriangleList.m_uPolygonStart = (UInt32)(xReader.BaseStream.Position) + 0x0C;
                UInt16 xWord0 = xReader.ReadUInt16();
                UInt16 xWord1 = xReader.ReadUInt16();
                UInt32 xDWord0 = xReader.ReadUInt32();
                xTriangleList.m_xMaterial = new Material();
                xTriangleList.m_xMaterial.visible = ((xWord1 & 0x0800) == 0);
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
                xTriangleList.m_uNext = xReader.ReadUInt32();

                return (xTriangleList.m_xMaterial.visible);
            }
        }

        protected class SR2UnitModel : SR2Model
        {
            protected UInt32 m_uOctTreeCount;
            protected UInt32 m_uOctTreeStart;
            protected UInt32 m_uSpectralVertexStart;
            protected UInt32 m_uSpectralColourStart;

            protected SR2UnitModel(BinaryReader xReader, UInt32 uDataStart, UInt32 uModelData, String strModelName, Platform ePlatform, UInt32 uVersion)
                : base (xReader, uDataStart, uModelData, strModelName, ePlatform, uVersion)
            {
                // xReader.BaseStream.Position += 0x04;
                // m_uInstanceCount = xReader.ReadUInt32();
                // m_uInstanceStart = m_uDataStart + xReader.ReadUInt32();
                xReader.BaseStream.Position = _modelData + 0x0C;

                _vertexCount              = xReader.ReadUInt32();
                _polygonCount             = 0; // xReader.ReadUInt32(); // Length = 0x14
                xReader.BaseStream.Position += 0x08;
                _vertexStart              = _dataStart + xReader.ReadUInt32();
                _polygonStart             = 0; // m_uDataStart + xReader.ReadUInt32();
                xReader.BaseStream.Position += 0x14;
                xReader.BaseStream.Position += 0x04;
                m_uSpectralVertexStart      = _dataStart + xReader.ReadUInt32();
                xReader.BaseStream.Position += 0x04;
                m_uSpectralColourStart      = _dataStart + xReader.ReadUInt32();
                _materialStart            = 0;
                _materialCount            = 0;
                m_uOctTreeCount             = xReader.ReadUInt32();
                m_uOctTreeStart             = _dataStart + xReader.ReadUInt32();
                _groupCount                = m_uOctTreeCount;

                //m_uVertexCount = xReader.ReadUInt32();
                //m_uPolygonCount = 0; // xReader.ReadUInt32(); // Length = 0x14
                //xReader.BaseStream.Position += 0x08;
                //m_uVertexStart = m_uDataStart + xReader.ReadUInt32();
                //m_uPolygonStart = 0; // m_uDataStart + xReader.ReadUInt32();
                //xReader.BaseStream.Position += 0x14;
                //m_uMaterialStart = m_uDataStart + xReader.ReadUInt32(); // Vertex colours
                //m_uMaterialStart = m_uDataStart + xReader.ReadUInt32(); // Vertex colours
                //m_uMaterialStart = m_uDataStart + xReader.ReadUInt32(); // Vertex colours
                //m_uMaterialStart = 0;
                //m_uMaterialCount = 0;
                //m_uSpectralColourStart = m_uDataStart + xReader.ReadUInt32();
                //m_uSpectralVertexStart = 0; // m_uDataStart + xReader.ReadUInt32();
                //m_uOctTreeCount = xReader.ReadUInt32();
                //m_uOctTreeStart = m_uDataStart + xReader.ReadUInt32();
                //m_uTreeCount = m_uOctTreeCount;

                _trees = new Tree[_groupCount];
            }

            public static SR2UnitModel Load(BinaryReader xReader, UInt32 uDataStart, UInt32 uModelData, String strModelName, Platform ePlatform, UInt32 uVersion)
            {
                SR2UnitModel xModel = new SR2UnitModel(xReader, uDataStart, uModelData, strModelName, ePlatform, uVersion);
                xModel.ReadData(xReader);
                return xModel;
            }

            protected override void ReadVertex(BinaryReader xReader, int v)
            {
                base.ReadVertex(xReader, v);

                _positionsPhys[v] = _positionsRaw[v];
                _positionsAltPhys[v] = _positionsPhys[v];

                _vertices[v].colourID = v;

                _colours[v] = xReader.ReadUInt32();
                _coloursAlt[v] = _colours[v];

                _vertices[v].UVID = v;

                UInt16 vU = xReader.ReadUInt16();
                UInt16 vV = xReader.ReadUInt16();

                // This is the broken one in pillars4c.
                // Search for in Cheat Engine 03 3E 17 3F DF F1 F6 D4 D3 C7 00 00 36 4B 39 FF
                if (v == 13370)
                {
                    _uvs[v].u = Utility.BizarreFloatToNormalFloat(vU); // 0x3E03 = 0.127929688 with Ben's formula
                    _uvs[v].v = Utility.BizarreFloatToNormalFloat(vV); // 0x3F17 = 0.58984375 with Ben's formula
                    //m_axUVs[v].u = 0.0f;
                    //m_axUVs[v].v = 0.0f;
                }
                else
                {
                    _uvs[v].u = Utility.BizarreFloatToNormalFloat(vU);
                    _uvs[v].v = Utility.BizarreFloatToNormalFloat(vV);
                }
            }

            protected override void ReadVertices(BinaryReader xReader)
            {
                base.ReadVertices(xReader);

                ReadSpectralData(xReader);
            }

            protected virtual void ReadSpectralData(BinaryReader xReader)
            {
                if (m_uSpectralColourStart != 0)
                {
                    // Spectral Colours
                    xReader.BaseStream.Position = m_uSpectralColourStart;
                    for (int v = 0; v < _vertexCount; v++)
                    {
                        UInt32 uShiftColour = xReader.ReadUInt32();
                        UInt32 uAlpha = _coloursAlt[v] & 0xFF000000;
                        UInt32 uRGB = uShiftColour & 0x00FFFFFF;
                        _coloursAlt[v] = uAlpha | uRGB;
                    }
                }

                if (m_uSpectralVertexStart != 0)
                {
                    // Spectral vertices
                    xReader.BaseStream.Position = _modelData + 0x2C;
                    UInt32 uCurrentIndexPosition = xReader.ReadUInt32();
                    UInt32 uCurrentSpectralVertex = m_uSpectralVertexStart;
                    while (true)
                    {
                        xReader.BaseStream.Position = uCurrentIndexPosition;
                        Int32 iVertex = xReader.ReadInt32();
                        uCurrentIndexPosition = (UInt32)xReader.BaseStream.Position;

                        if(iVertex == -1)
                        {
                            break;
                        }

                        xReader.BaseStream.Position = uCurrentSpectralVertex;
                        ShiftVertex xShiftVertex;
                        xShiftVertex.basePos.x = (float)xReader.ReadInt16();
                        xShiftVertex.basePos.y = (float)xReader.ReadInt16();
                        xShiftVertex.basePos.z = (float)xReader.ReadInt16();
                        uCurrentSpectralVertex = (UInt32)xReader.BaseStream.Position;

                        _positionsAltPhys[iVertex] = xShiftVertex.basePos;
                    }

                    //// Spectral Verticices
                    //xReader.BaseStream.Position = m_uSpectralVertexStart + 0x06;
                    //int sVertex = xReader.ReadInt16();
                    //xReader.BaseStream.Position = m_uSpectralVertexStart;
                    //while (sVertex != 0xFFFF)
                    //{
                    //    ExShiftVertex xShiftVertex;
                    //    xShiftVertex.basePos.x = (float)xReader.ReadInt16();
                    //    xShiftVertex.basePos.y = (float)xReader.ReadInt16();
                    //    xShiftVertex.basePos.z = (float)xReader.ReadInt16();
                    //    sVertex = xReader.ReadUInt16();

                    //    if (sVertex == 0xFFFF)
                    //    {
                    //        break;
                    //    }

                    //    xShiftVertex.offset.x = (float)xReader.ReadInt16();
                    //    xShiftVertex.offset.y = (float)xReader.ReadInt16();
                    //    xShiftVertex.offset.z = (float)xReader.ReadInt16();
                    //    m_axPositions[sVertex].localPos = xShiftVertex.offset + xShiftVertex.basePos;
                    //    m_axPositions[sVertex].worldPos = m_axPositions[sVertex].localPos;
                    //}
                }
            }

            protected override void ReadPolygons(BinaryReader xReader)
            {
                Material xMaterial = new Material();
                xMaterial.textureID = 0;
                xMaterial.colour = 0xFFFFFFFF;
                _materialsList.Add(xMaterial);

                MemoryStream xPolyStream = new MemoryStream((Int32)_vertexCount * 3);
                BinaryWriter xPolyWriter = new BinaryWriter(xPolyStream);
                BinaryReader xPolyReader = new BinaryReader(xPolyStream);

                MemoryStream xTextureStream = new MemoryStream((Int32)_vertexCount * 3);
                BinaryWriter xTextureWriter = new BinaryWriter(xTextureStream);
                BinaryReader xTextureReader = new BinaryReader(xTextureStream);

                List<Mesh> xMeshes = new List<Mesh>();
                List<Int64> xMeshPositions = new List<Int64>();

                for (UInt32 t = 0; t < m_uOctTreeCount; t++)
                {
                    xReader.BaseStream.Position = m_uOctTreeStart + (t * 0x60);

                    xReader.BaseStream.Position += 0x2C;
                    bool drawTester = ((xReader.ReadUInt32() & 1) != 1);
                    UInt32 uOctID = xReader.ReadUInt32();
                    xReader.BaseStream.Position += 0x10;
                    UInt32 uDataPos = xReader.ReadUInt32();
                    xReader.BaseStream.Position += 0x08;
                    // In each terrain group, vertices start from part way through the array.
                    UInt32 uStartIndex = xReader.ReadUInt32();

                    _trees[t] = ReadOctTree(xReader, xPolyWriter, xTextureWriter, uDataPos, _trees[t], xMeshes, xMeshPositions, 0, (UInt16)uStartIndex);
                }

                _polygonCount = (UInt32)xPolyReader.BaseStream.Position / 6;
                _polygons = new Polygon[_polygonCount];
                UInt32 uPolygon = 0;

                xPolyReader.BaseStream.Position = 0;
                xTextureReader.BaseStream.Position = 0;
                for (int m = 0; m < xMeshes.Count; m++)
                {
                    Mesh xCurrentMesh = xMeshes[m];
                    Int64 iStartPosition = xPolyReader.BaseStream.Position;
                    Int64 iEndPosition = xMeshPositions[m];
                    Int64 iRange = iEndPosition - iStartPosition;
                    while (iRange % 6 > 0) iRange++;
                    UInt32 uIndexCount = (UInt32)iRange / 2;

                    FinaliseMesh(xPolyReader, xTextureReader, xCurrentMesh, xMaterial, uIndexCount, ref uPolygon);
                }
                _materialCount = (UInt32)_materialsList.Count;

                return;
            }

            protected virtual Tree ReadOctTree(BinaryReader xReader, BinaryWriter xPolyWriter, BinaryWriter xTextureWriter, UInt32 uDataPos, Tree xParentTree, List<Mesh> xMeshes, List<Int64> xMeshPositions, UInt32 uDepth, UInt16 uStartIndex)
            {
                if (uDataPos == 0)
                {
                    return null;
                }

                xReader.BaseStream.Position = uDataPos + 0x34;
                Int32 iSubTreeCount = xReader.ReadInt32();

                Tree xTree = null;
                Mesh xMesh = null;

                UInt32 uMaxDepth = 0;

                if (uDepth <= uMaxDepth)
                {
                    xTree = new Tree();
                    xMesh = new Mesh();
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

                if (iSubTreeCount == 0)
                {
                    xTree.isLeaf = true;

                    xReader.BaseStream.Position = uDataPos + 0x30;
                    ReadOctLeaf(xReader, xPolyWriter, xTextureWriter, xMesh, uStartIndex);
                }
                else
                {
                    UInt32[] auSubTreePositions = new UInt32[iSubTreeCount];
                    for (Int32 s = 0; s < iSubTreeCount; s++)
                    {
                        auSubTreePositions[s] = xReader.ReadUInt32();
                    }

                    for (Int32 s = iSubTreeCount - 1; s >= 0; s--)
                    {
                        ReadOctTree(xReader, xPolyWriter, xTextureWriter, auSubTreePositions[s], xTree, xMeshes, xMeshPositions, uDepth + 1, uStartIndex);
                    }
                }

                if (uDepth <= uMaxDepth)
                {
                    if (xMesh != null && xMesh.indexCount > 0)
                    {
                        xMeshes.Add(xMesh);
                        xMeshPositions.Add(xPolyWriter.BaseStream.Position);
                    }
                }

                return xTree;
            }

            protected virtual void ReadOctLeaf(BinaryReader xReader, BinaryWriter xPolyWriter, BinaryWriter xTextureWriter, Mesh xMesh, UInt16 uStartIndex)
            {
                UInt32 uLeafData = _dataStart + xReader.ReadUInt32();
                xReader.BaseStream.Position = uLeafData;

                UInt32 uNextStrip = (UInt32)xReader.BaseStream.Position;
                while (true)
                {
                    bool bShouldWrite = true; // For debug.

                    UInt32 uLength = xReader.ReadUInt32();
                    if (uLength == 0 || uLength == 0xFFFFFFFF)
                    {
                        break;
                    }

                    uNextStrip += uLength;

                    UInt32 uIndexCount = xReader.ReadUInt32();
                    if (uIndexCount == 0)
                    {
                        continue;
                    }

                    UInt16[] axStripIndices = new UInt16[uIndexCount];
                    for (UInt32 i = 0; i < uIndexCount; i++)
                    {
                        axStripIndices[i] = (UInt16)(uStartIndex + xReader.ReadUInt16());
                    }

                    if (xReader.BaseStream.Position % 4 != 0)
                    {
                        xReader.BaseStream.Position += 0x02;
                    }

                    while (true)
                    {                        
                        // 0xFFFF wrong?  Try uTestNextStrip
                        UInt32 uIndexCount2 = xReader.ReadUInt32();
                        if (/*(uIndexCount2 & 0x0000FFFF) == 0x0000FFFF || (uIndexCount2 & 0xFFFF0000) == 0xFFFF0000 ||*/ uIndexCount2 == 0)
                        {
                            //if (xReader.BaseStream.Position % 4 != 0)
                            //{
                            //    xReader.BaseStream.Position += 0x02;
                            //}
                            break;
                        }

                        xReader.BaseStream.Position += 0x04;
                        UInt32 uTextureID = xReader.ReadUInt32();
                        xReader.BaseStream.Position += 0x04;

                        UInt32 uTestNextStrip = xReader.ReadUInt32();

                        UInt16[] axStripIndices2 = new UInt16[uIndexCount2];
                        for (UInt32 i = 0; i < uIndexCount2; i++)
                        {
                            axStripIndices2[i] = xReader.ReadUInt16();
                        }

                        if (bShouldWrite)
                        {
                            for (UInt16 i = 0; i < uIndexCount2; i++)
                            {
                                xPolyWriter.Write(axStripIndices[axStripIndices2[i]]);
                                if (xPolyWriter.BaseStream.Position % 6 == 0)
                                {
                                    xTextureWriter.Write(uTextureID);
                                }
                            }

                            if (xMesh != null)
                            {
                                xMesh.indexCount += uIndexCount2;
                            }
                        }

                        xReader.BaseStream.Position = uTestNextStrip;
                    }

                    xReader.BaseStream.Position = uNextStrip;
                }

                // Was this a special second set of polys?  Animated ones?
                while (true)
                {
                    bool bShouldWrite = true; // For debug.

                    UInt32 uIndexCount = xReader.ReadUInt32();
                    if (uIndexCount == 0)
                    {
                        break;
                    }

                    xReader.BaseStream.Position += 0x04;
                    UInt32 uTextureID = xReader.ReadUInt32();

                    xReader.BaseStream.Position += 0x04;
                    uNextStrip = xReader.ReadUInt32();

                    UInt16[] axStripIndices = new UInt16[uIndexCount];
                    for (UInt32 i = 0; i < uIndexCount; i++)
                    {
                        axStripIndices[i] = (UInt16)(uStartIndex + xReader.ReadUInt16());
                    }

                    if (bShouldWrite)
                    {
                        for (UInt16 i = 0; i < uIndexCount; i++)
                        {
                            xPolyWriter.Write(axStripIndices[i]);
                            if (xPolyWriter.BaseStream.Position % 6 == 0)
                            {
                                xTextureWriter.Write(uTextureID);
                            }
                        }

                        if (xMesh != null)
                        {
                            xMesh.indexCount += uIndexCount;
                        }
                    }

                    xReader.BaseStream.Position = uNextStrip;
                }
            }

            protected virtual void FinaliseMesh(BinaryReader xPolyReader, BinaryReader xTextureReader, Mesh xMesh, Material xMaterial, UInt32 uIndexCount, ref UInt32 uPolygon)
            {
                //uIndexCount &= 0x0000FFFF;
                //uIndexCount /= 3;
                //uIndexCount *= 3;

                //xMesh.m_uIndexCount = uIndexCount;
                xMesh.polygonCount = xMesh.indexCount / 3;
                xMesh.polygons = new Polygon[xMesh.polygonCount];
                for (UInt32 p = 0; p < xMesh.polygonCount; p++)
                {
                    UInt16 uV1 = xPolyReader.ReadUInt16();
                    UInt16 uV2 = xPolyReader.ReadUInt16();
                    UInt16 uV3 = xPolyReader.ReadUInt16();

                    xMesh.polygons[p].v1 = _vertices[uV1];
                    xMesh.polygons[p].v2 = _vertices[uV2];
                    xMesh.polygons[p].v3 = _vertices[uV3];

                    xMaterial = new Material();

                    xMaterial.visible = true;

                    UInt16 xWord1 = xTextureReader.ReadUInt16();
                    UInt16 xWord2 = xTextureReader.ReadUInt16();
                    xMaterial.textureID = (UInt16)(xWord1 & 0x0FFF);
                    xMaterial.colour = 0xFFFFFFFF;
                    if (xMaterial.textureID > 0 && xMaterial.visible)
                    {
                        xMaterial.textureUsed = true;
                    }
                    else
                    {
                        xMaterial.textureUsed = false;
                    }

                    bool bFoundMaterial = false;
                    for (int m = 0; m < _materialsList.Count; m++)
                    {
                        if (_materialsList[m].colour == xMaterial.colour &&
                            _materialsList[m].textureID == xMaterial.textureID &&
                            _materialsList[m].textureUsed == xMaterial.textureUsed)
                        {
                            bFoundMaterial = true;
                            xMaterial = _materialsList[m];
                            break;
                        }
                    }

                    if (!bFoundMaterial)
                    {
                        _materialsList.Add(xMaterial);
                    }

                    xMesh.polygons[p].material = xMaterial;
                }

                // Make the vertices unique - Because I do the same thing in GenerateOutput
                xMesh.vertices = new Vertex[xMesh.indexCount];
                for (UInt32 poly = 0; poly < xMesh.polygonCount; poly++)
                {
                    _polygons[uPolygon++] = xMesh.polygons[poly];
                    xMesh.vertices[(3 * poly) + 0] = xMesh.polygons[poly].v1;
                    xMesh.vertices[(3 * poly) + 1] = xMesh.polygons[poly].v2;
                    xMesh.vertices[(3 * poly) + 2] = xMesh.polygons[poly].v3;
                }
            }
        }

        #endregion

        public SR2File(String strFileName) 
            : base(strFileName, Game.SR2)
        {
        }

        protected override void ReadHeaderData(BinaryReader xReader)
        {
            _dataStart = 0;

            xReader.BaseStream.Position = 0x00000080;
            if (xReader.ReadUInt32() == 0x04C2041D)
            {
                _asset = Asset.Unit;
            }
            else
            {
                _asset = Asset.Object;
            }
        }

        protected override void ReadObjectData(BinaryReader xReader)
        {
            // Object name
            xReader.BaseStream.Position = _dataStart + 0x00000024;
            xReader.BaseStream.Position = _dataStart + xReader.ReadUInt32();
            String strModelName = new String(xReader.ReadChars(8));
            _name = Utility.CleanName(strModelName);

            // Texture type
            //xReader.BaseStream.Position = m_uDataStart + 0x44;
            //if (xReader.ReadUInt64() != 0xFFFFFFFFFFFFFFFF)
            //{
            //    m_ePlatform = Platform.PSX;
            //}
            //else
            //{
            _platform = Platform.PC;
            //}

            // Model data
            xReader.BaseStream.Position = _dataStart + 0x0000000C;
            _modelCount = 1; //xReader.ReadUInt16();
            _animCount = 0; //xReader.ReadUInt16();
            _modelStart = _dataStart + xReader.ReadUInt32();
            _animStart = 0; //m_uDataStart + xReader.ReadUInt32();

            _models = new SR2Model[_modelCount];
            for (UInt16 m = 0; m < _modelCount; m++)
            {
                _models[m] = SR2ObjectModel.Load(xReader, _dataStart, _modelStart, _name, _platform, m, _version);
            }
        }

        protected override void ReadUnitData(BinaryReader xReader)
        {
            // Connected unit names
            xReader.BaseStream.Position = _dataStart;
            UInt32 m_uConnectionData = _dataStart + xReader.ReadUInt32();
            xReader.BaseStream.Position = m_uConnectionData + 0x24;
            _connectedUnitCount = xReader.ReadUInt32();
            xReader.BaseStream.Position = _dataStart + xReader.ReadUInt32();
            _connectedUnitNames = new String[_connectedUnitCount];
            for (int i = 0; i < _connectedUnitCount; i++)
            {
                String strUnitName = new String(xReader.ReadChars(12));
                _connectedUnitNames[i] = Utility.CleanName(strUnitName);
                xReader.BaseStream.Position += 0x84;
            }

            // Instances
            xReader.BaseStream.Position = _dataStart + 0x44;
            _instanceCount = xReader.ReadUInt32();
            _instanceStart = _dataStart + xReader.ReadUInt32();
            _instanceNames = new String[_instanceCount];
            for (int i = 0; i < _instanceCount; i++)
            {
                xReader.BaseStream.Position = _instanceStart + 0x60 * i;
                String strInstanceName = new String(xReader.ReadChars(8));
                _instanceNames[i] = Utility.CleanName(strInstanceName);
            }

            // Instance types
            xReader.BaseStream.Position = _dataStart + 0x4C;
            _instanceTypeStart = _dataStart + xReader.ReadUInt32();
            xReader.BaseStream.Position = _instanceTypeStart;
            List<String> xInstanceList = new List<String>();
            while (xReader.ReadByte() != 0xFF)
            {
                xReader.BaseStream.Position--;
                String strInstanceTypeName = new String(xReader.ReadChars(8));
                xInstanceList.Add(Utility.CleanName(strInstanceTypeName));
                xReader.BaseStream.Position += 0x08;
            }
            _instanceTypeNames = xInstanceList.ToArray();

            // Unit name
            xReader.BaseStream.Position = _dataStart + 0x50;
            xReader.BaseStream.Position = _dataStart + xReader.ReadUInt32();
            String strModelName = new String(xReader.ReadChars(10)); // Need to check
            _name = Utility.CleanName(strModelName);

            // Texture type
            //xReader.BaseStream.Position = m_uDataStart + 0x9C;
            //if (xReader.ReadUInt64() != 0xFFFFFFFFFFFFFFFF)
            //{
            //    m_ePlatform = Platform.PSX;
            //}
            //else
            //{
                _platform = Platform.PC;
            //}

            // Model data
            xReader.BaseStream.Position = _dataStart;
            _modelCount = 1;
            _modelStart = _dataStart;
            _models = new SR2Model[_modelCount];
            xReader.BaseStream.Position = _modelStart;
            UInt32 m_uModelData = _dataStart + xReader.ReadUInt32();

            // Material data
            _models[0] = SR2UnitModel.Load(xReader, _dataStart, m_uModelData, _name, _platform, _version);

            //if (m_axModels[0].Platform == Platform.Dreamcast ||
            //    m_axModels[1].Platform == Platform.Dreamcast)
            //{
            //    m_ePlatform = Platform.Dreamcast;
            //}
        }

        protected override void ResolvePointers(BinaryReader xReader, BinaryWriter xWriter)
        {
            xReader.BaseStream.Position = 0;
            xWriter.BaseStream.Position = 0;

            xReader.BaseStream.Position += 0x04;
            UInt32 uRegionCount = xReader.ReadUInt32();

            UInt32 uTotal = 0;
            UInt32[] auRegionSizes = new UInt32[uRegionCount];
            UInt32[] auRegionPositions = new UInt32[uRegionCount];
            for (UInt32 r = 0; r < uRegionCount; r++)
            {
                auRegionSizes[r] = xReader.ReadUInt32();
                auRegionPositions[r] = uTotal;
                uTotal += auRegionSizes[r];
                xReader.BaseStream.Position += 0x08;
            }

            UInt32 uRegionDataSize = uRegionCount * 0x0C;
            UInt32 uPointerData = (uRegionDataSize & 0x00000003) + ((uRegionDataSize + 0x17) & 0xFFFFFFF0);
            for (UInt32 r = 0; r < uRegionCount; r++)
            {
                xReader.BaseStream.Position = uPointerData;
                UInt32 uPointerCount = xReader.ReadUInt32();
                UInt32 uPointerDataSize = uPointerCount * 0x04;
                UInt32 uObjectData = uPointerData + ((uPointerDataSize + 0x13) & 0xFFFFFFF0);
                UInt32 uObjectDataSize = (auRegionSizes[r] + 0x0F) & 0xFFFFFFF0;

                xReader.BaseStream.Position = uObjectData;
                xWriter.BaseStream.Position = auRegionPositions[r];
                Byte[] auObjectData = xReader.ReadBytes((Int32)auRegionSizes[r]);
                xWriter.Write(auObjectData);

                xReader.BaseStream.Position = uPointerData + 0x04;
                UInt32[] auAddresses = new UInt32[uPointerCount];
                for (UInt32 p = 0; p < uPointerCount; p++)
                {
                    auAddresses[p] = xReader.ReadUInt32();
                }

                UInt32[] auValues = new UInt32[uPointerCount];
                for (UInt32 p = 0; p < uPointerCount; p++)
                {
                    xReader.BaseStream.Position = uObjectData + auAddresses[p];
                    UInt32 uValue1 = xReader.ReadUInt32();
                    UInt32 uValue2 = uValue1 & 0x003FFFFF;
                    UInt32 uValue3 = uValue1 >> 0x16;

                    auAddresses[p] += auRegionPositions[r];
                    auValues[p] = auRegionPositions[uValue3] + uValue2;

                    xWriter.BaseStream.Position = auAddresses[p];
                    xWriter.Write(auValues[p]);
                }

                uPointerData = uObjectData + uObjectDataSize;
            }
        }
    }
}