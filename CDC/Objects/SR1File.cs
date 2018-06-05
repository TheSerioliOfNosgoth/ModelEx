using System;
using System.IO;
using System.Collections.Generic;

namespace CDC.Objects
{
    public class SR1File : SRFile
    {
        public const UInt32 BETA_VERSION = 0x3c204139;
        public const UInt32 RETAIL_VERSION = 0x3C20413B;

        #region Model classes

        protected class SR1ObjectModel : SR1Model
        {
            protected SR1ObjectModel(BinaryReader xReader, UInt32 uDataStart, UInt32 uModelData, String strModelName, Platform ePlatform, UInt32 uVersion)
                : base (xReader, uDataStart, uModelData, strModelName, ePlatform, uVersion)
            {
                xReader.BaseStream.Position = _modelData;
                _vertexCount              = xReader.ReadUInt32();
                _vertexStart              = _dataStart + xReader.ReadUInt32();
                _vertexScale.x            = 1.0f;
                _vertexScale.y            = 1.0f;
                _vertexScale.z            = 1.0f;
                xReader.BaseStream.Position += 0x08;
                _polygonCount             = xReader.ReadUInt32();
                _polygonStart             = _dataStart + xReader.ReadUInt32();
                _boneCount                = xReader.ReadUInt32();
                _boneStart                = _dataStart + xReader.ReadUInt32();
                xReader.BaseStream.Position += 0x10;
                _materialStart            = _dataStart + xReader.ReadUInt32();
                _materialCount            = 0;
                _treeCount                = 1;

                _trees = new Tree[_treeCount];
            }

            public static SR1ObjectModel Load(BinaryReader xReader, UInt32 uDataStart, UInt32 uModelData, String strModelName, Platform ePlatform, UInt16 usIndex, UInt32 uVersion)
            {
                xReader.BaseStream.Position = uModelData + (0x00000004 * usIndex);
                uModelData = uDataStart + xReader.ReadUInt32();
                xReader.BaseStream.Position = uModelData;
                SR1ObjectModel xModel = new SR1ObjectModel(xReader, uDataStart, uModelData, strModelName, ePlatform, uVersion);
                xModel.ReadData(xReader);
                return xModel;
            }

            protected override void ReadVertex(BinaryReader xReader, int v)
            {
                base.ReadVertex(xReader, v);

                _positionsPhys[v] = _positionsRaw[v];
                _positionsAltPhys[v] = _positionsPhys[v];

                _vertices[v].normalID = xReader.ReadUInt16();
            }

            protected override void ReadVertices(BinaryReader xReader)
            {
                base.ReadVertices(xReader);

                ReadArmature(xReader);
                ApplyArmature();
            }

            protected virtual void ReadArmature(BinaryReader xReader)
            {
                if (_boneStart == 0 || _boneCount == 0) return;

                xReader.BaseStream.Position = _boneStart;
                _bones = new Bone[_boneCount];
                _bones = new Bone[_boneCount];
                for (UInt16 b = 0; b < _boneCount; b++)
                {
                    // Get the bone data
                    xReader.BaseStream.Position += 8;
                    _bones[b].vFirst = xReader.ReadUInt16();
                    _bones[b].vLast = xReader.ReadUInt16();
                    _bones[b].localPos.x = (float)xReader.ReadInt16();
                    _bones[b].localPos.y = (float)xReader.ReadInt16();
                    _bones[b].localPos.z = (float)xReader.ReadInt16();
                    _bones[b].parentID1 = xReader.ReadUInt16();

                    // Combine this bone with it's ancestors is there are any
                    if ((_bones[b].vFirst != 0xFFFF) && (_bones[b].vLast != 0xFFFF))
                    {
                        for (UInt16 ancestorID = b; ancestorID != 0xFFFF; )
                        {
                            _bones[b].worldPos += _bones[ancestorID].localPos;
                            if (_bones[ancestorID].parentID1 == ancestorID) break;
                            ancestorID = _bones[ancestorID].parentID1;
                        }
                    }
                    xReader.BaseStream.Position += 4;
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
                            _positionsPhys[v] += _bones[b].worldPos;
                            _vertices[v].boneID = b;
                        }
                    }
                }
                return;
            }

            protected virtual void ReadPolygon(BinaryReader xReader, int p)
            {
                UInt32 uPolygonPosition = (UInt32)xReader.BaseStream.Position;

                _polygons[p].v1 = _vertices[xReader.ReadUInt16()];
                _polygons[p].v2 = _vertices[xReader.ReadUInt16()];
                _polygons[p].v3 = _vertices[xReader.ReadUInt16()];

                _polygons[p].material = new Material();
                _polygons[p].material.visible = true;
                _polygons[p].material.textureUsed = (Boolean)(((int)xReader.ReadUInt16() & 0x0200) != 0);

                if (_polygons[p].material.textureUsed)
                {
                    // WIP
                    UInt32 uMaterialPosition = _dataStart + xReader.ReadUInt32();
                    if ((((uMaterialPosition - _materialStart) % 0x10) != 0) &&
                         ((uMaterialPosition - _materialStart) % 0x18) == 0)
                    {
                        _platform = Platform.Dreamcast;
                    }

                    xReader.BaseStream.Position = uMaterialPosition;
                    ReadMaterial(xReader, p);

                    if (_platform == Platform.Dreamcast)
                    {
                        xReader.BaseStream.Position += 0x06;
                    }
                    else
                    {
                        xReader.BaseStream.Position += 0x02;
                    }

                    _polygons[p].material.colour = xReader.ReadUInt32();
                    _polygons[p].material.colour |= 0xFF000000;

                }
                else
                {
                    _polygons[p].material.colour = xReader.ReadUInt32() | 0xFF000000;
                }

                Utility.FlipRedAndBlue(ref _polygons[p].material.colour);

                xReader.BaseStream.Position = uPolygonPosition + 0x0C;
            }

            protected override void ReadPolygons(BinaryReader xReader)
            {
                if (_polygonStart == 0 || _polygonCount == 0)
                {
                    return;
                }

                xReader.BaseStream.Position = _polygonStart;

                MaterialList xMaterialsList = null;

                for (UInt16 p = 0; p < _polygonCount; p++)
                {
                    ReadPolygon(xReader, p);

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

                for (UInt32 t = 0; t < _treeCount; t++)
                {
                    _trees[t] = new Tree();
                    _trees[t].mesh = new Mesh();
                    _trees[t].mesh.indexCount = _indexCount;
                    _trees[t].mesh.polygonCount = _polygonCount;
                    _trees[t].mesh.polygons = _polygons;
                    _trees[t].mesh.vertices = _vertices;

                    // Make the vertices unique - Because I do the same thing in GenerateOutput
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

        protected class SR1UnitModel : SR1Model
        {
            protected UInt32 m_uBspTreeCount;
            protected UInt32 m_uBspTreeStart;
            protected UInt32 m_uSpectralVertexStart;
            protected UInt32 m_uSpectralColourStart;

            protected SR1UnitModel(BinaryReader xReader, UInt32 uDataStart, UInt32 uModelData, String strModelName, Platform ePlatform, UInt32 uVersion)
                : base (xReader, uDataStart, uModelData, strModelName, ePlatform, uVersion)
            {
                xReader.BaseStream.Position = _modelData + 0x10;
                _vertexCount              = xReader.ReadUInt32();
                _polygonCount             = xReader.ReadUInt32();
                xReader.BaseStream.Position += 0x04;
                _vertexStart              = _dataStart + xReader.ReadUInt32();
                _polygonStart             = _dataStart + xReader.ReadUInt32();
                xReader.BaseStream.Position += 0x10;
                _materialStart            = _dataStart + xReader.ReadUInt32();
                _materialCount            = 0;
                xReader.BaseStream.Position += 0x04;

                if (_version == BETA_VERSION)
                {
                    xReader.BaseStream.Position += 0x08;
                }

                m_uSpectralVertexStart      = _dataStart + xReader.ReadUInt32();
                m_uSpectralColourStart      = _dataStart + xReader.ReadUInt32();
                m_uBspTreeCount             = xReader.ReadUInt32();
                m_uBspTreeStart             = _dataStart + xReader.ReadUInt32();
                _treeCount                = m_uBspTreeCount;

                _trees = new Tree[_treeCount];
            }

            public static SR1UnitModel Load(BinaryReader xReader, UInt32 uDataStart, UInt32 uModelData, String strModelName, Platform ePlatform, UInt32 uVersion)
            {
                SR1UnitModel xModel = new SR1UnitModel(xReader, uDataStart, uModelData, strModelName, ePlatform, uVersion);
                xModel.ReadData(xReader);
                return xModel;
            }

            protected override void ReadVertex(BinaryReader xReader, int v)
            {
                base.ReadVertex(xReader, v);

                _positionsPhys[v] = _positionsRaw[v];
                _positionsAltPhys[v] = _positionsPhys[v];

                _vertices[v].colourID = v;

                xReader.BaseStream.Position += 2;
                _colours[v] = xReader.ReadUInt32() | 0xFF000000;

                if (_platform != Platform.Dreamcast)
                {
                    Utility.FlipRedAndBlue(ref _colours[v]);
                }

                _coloursAlt[v] = _colours[v];
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
                        UInt32 uShiftColour = xReader.ReadUInt16();
                        UInt32 uAlpha = _coloursAlt[v] & 0xFF000000;
                        UInt32 uRed = ((uShiftColour >> 0) & 0x1F) << 0x13;
                        UInt32 uGreen = ((uShiftColour >> 5) & 0x1F) << 0x0B;
                        UInt32 uBlue = ((uShiftColour >> 10) & 0x1F) << 0x03;
                        _coloursAlt[v] = uAlpha | uRed | uGreen | uBlue;
                    }
                }

                if (m_uSpectralVertexStart != 0)
                {
                    // Spectral Verticices
                    xReader.BaseStream.Position = m_uSpectralVertexStart + 0x06;
                    int sVertex = xReader.ReadInt16();
                    xReader.BaseStream.Position = m_uSpectralVertexStart;
                    while (sVertex != 0xFFFF)
                    {
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
                        _positionsAltPhys[sVertex] = xShiftVertex.offset + xShiftVertex.basePos;
                    }
                }
            }

            protected virtual void ReadPolygon(BinaryReader xReader, int p)
            {
                UInt32 uPolygonPosition = (UInt32)xReader.BaseStream.Position;

                _polygons[p].v1 = _vertices[xReader.ReadUInt16()];
                _polygons[p].v2 = _vertices[xReader.ReadUInt16()];
                _polygons[p].v3 = _vertices[xReader.ReadUInt16()];
                _polygons[p].material = new Material();

                _polygons[p].material.textureUsed |= (Boolean)(((int)xReader.ReadUInt16() & 0x0004) == 0);
                xReader.BaseStream.Position += 0x02;
                UInt16 uMaterialOffset = xReader.ReadUInt16();
                _polygons[p].material.textureUsed &= (Boolean)(uMaterialOffset != 0xFFFF);

                if (_polygons[p].material.textureUsed)
                {
                    // WIP
                    UInt32 uMaterialPosition = uMaterialOffset + _materialStart;
                    if ((((uMaterialPosition - _materialStart) % 0x0C) != 0) &&
                         ((uMaterialPosition - _materialStart) % 0x14) == 0)
                    {
                        _platform = Platform.Dreamcast;
                    }

                    xReader.BaseStream.Position = uMaterialPosition;
                    ReadMaterial(xReader, p);
                }
                else
                {
                    _polygons[p].material.textureUsed = false;
                    _polygons[p].material.colour = 0xFFFFFFFF;
                }

                Utility.FlipRedAndBlue(ref _polygons[p].material.colour);

                xReader.BaseStream.Position = uPolygonPosition + 0x0C;
            }

            protected override void ReadPolygons(BinaryReader xReader)
            {
                if (_polygonStart == 0 || _polygonCount == 0)
                {
                    return;
                }

                xReader.BaseStream.Position = _polygonStart;

                for (UInt16 p = 0; p < _polygonCount; p++)
                {
                    ReadPolygon(xReader, p);
                }

                MemoryStream xPolyStream = new MemoryStream((Int32)_vertexCount * 3);
                BinaryWriter xPolyWriter = new BinaryWriter(xPolyStream);
                BinaryReader xPolyReader = new BinaryReader(xPolyStream);

                List<Mesh> xMeshes = new List<Mesh>();
                List<Int64> xMeshPositions = new List<Int64>();

                for (UInt32 t = 0; t < m_uBspTreeCount; t++)
                {
                    xReader.BaseStream.Position = m_uBspTreeStart + (t * 0x24);
                    UInt32 uDataPos = _dataStart + xReader.ReadUInt32();
                    xReader.BaseStream.Position += 0x0C;
                    bool drawTester = ((xReader.ReadInt32() & 1) != 1);
                    xReader.BaseStream.Position += 0x06;
                    UInt16 usBspID = xReader.ReadUInt16();

                    _trees[t] = ReadBSPTree(xReader, xPolyWriter, uDataPos, _trees[t], xMeshes, xMeshPositions, 0);
                }

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

                xPolyReader.BaseStream.Position = 0;
                for (int m = 0; m < xMeshes.Count; m++)
                {
                    Mesh xCurrentMesh = xMeshes[m];
                    Int64 iStartPosition = xPolyReader.BaseStream.Position;
                    Int64 iEndPosition = xMeshPositions[m];
                    Int64 iRange = iEndPosition - iStartPosition;
                    UInt32 uIndexCount = (UInt32)iRange / 4;

                    FinaliseMesh(xPolyReader, xCurrentMesh, uIndexCount);
                }
            }

            protected virtual Tree ReadBSPTree(BinaryReader xReader, BinaryWriter xPolyWriter, UInt32 uDataPos, Tree xParentTree, List<Mesh> xMeshes, List<Int64> xMeshPositions, UInt32 uDepth)
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

                if (isLeaf)
                {
                    xTree.isLeaf = true;

                    xReader.BaseStream.Position = uDataPos + 0x08;
                    ReadBSPLeaf(xReader, xPolyWriter, xMesh);
                }
                else
                {
                    xReader.BaseStream.Position = uDataPos + 0x14;

                    UInt32[] auSubTreePositions = new UInt32[2];
                    for (Int32 s = 0; s < iSubTreeCount; s++)
                    {
                        auSubTreePositions[s] = xReader.ReadUInt32();
                    }

                    for (Int32 s = iSubTreeCount - 1; s >= 0; s--)
                    {
                        ReadBSPTree(xReader, xPolyWriter, auSubTreePositions[s], xTree, xMeshes, xMeshPositions, uDepth + 1);
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

            protected virtual void ReadBSPLeaf(BinaryReader xReader, BinaryWriter xPolyWriter, Mesh xMesh)
            {
                UInt32 polygonPos = _dataStart + xReader.ReadUInt32();
                UInt32 polygonID = (polygonPos - _polygonStart) / 0x0C;
                UInt16 polyCount = xReader.ReadUInt16();
                for (UInt16 p = 0; p < polyCount; p++)
                {
                    _polygons[polygonID + p].material.visible = true;

                    xPolyWriter.Write(polygonID + p);

                    if (xMesh != null)
                    {
                        xMesh.indexCount += 3;
                    }
                }
            }

            protected virtual void FinaliseMesh(BinaryReader xPolyReader, Mesh xMesh, UInt32 uIndexCount)
            {
                //xMesh.m_uIndexCount = uIndexCount;
                xMesh.polygonCount = xMesh.indexCount / 3;
                xMesh.polygons = new Polygon[xMesh.polygonCount];
                for (UInt32 p = 0; p < xMesh.polygonCount; p++)
                {
                    UInt32 polygonID = xPolyReader.ReadUInt32();
                    xMesh.polygons[p] = _polygons[polygonID];
                }

                // Make the vertices unique - Because I do the same thing in GenerateOutput
                xMesh.vertices = new Vertex[xMesh.indexCount];
                for (UInt16 poly = 0; poly < xMesh.polygonCount; poly++)
                {
                    xMesh.vertices[(3 * poly) + 0] = xMesh.polygons[poly].v1;
                    xMesh.vertices[(3 * poly) + 1] = xMesh.polygons[poly].v2;
                    xMesh.vertices[(3 * poly) + 2] = xMesh.polygons[poly].v3;
                }
            }
        }

        #endregion

        public SR1File(String strFileName)
            : base(strFileName, Game.SR1)
        {
        }

        protected override void ReadHeaderData(BinaryReader xReader)
        {
            _dataStart = 0;

            // Could use unit version number instead of thing below.
            // Check that's what SR2 does.
            //xReader.BaseStream.Position = m_uDataStart + 0xF0;
            //UInt32 unitVersionNumber = xReader.ReadUInt32();
            //if (unitVersionNumber != 0x3C20413B)

            // Moved to ResolvePointers due to not knowing how else to tell.
            //xReader.BaseStream.Position = 0x00000000;
            //if (xReader.ReadUInt32() == 0x00000000)
            //{
            //    m_eFileType = FileType.Unit;
            //}
            //else
            //{
            //    m_eFileType = FileType.Object;
            //}
        }

        protected override void ReadObjectData(BinaryReader xReader)
        {
            // Object name
            xReader.BaseStream.Position = _dataStart + 0x00000024;
            xReader.BaseStream.Position = _dataStart + xReader.ReadUInt32();
            String strModelName = new String(xReader.ReadChars(8));
            _name = Utility.CleanName(strModelName);

            // Texture type
            xReader.BaseStream.Position = _dataStart + 0x44;
            if (xReader.ReadUInt64() != 0xFFFFFFFFFFFFFFFF)
            {
                _platform = Platform.PSX;
            }
            else
            {
                _platform = Platform.PC;
            }

            // Model data
            xReader.BaseStream.Position = _dataStart + 0x00000008;
            _modelCount = xReader.ReadUInt16();
            _animCount = xReader.ReadUInt16();
            _modelStart = _dataStart + xReader.ReadUInt32();
            _animStart = _dataStart + xReader.ReadUInt32();

            _models = new SR1Model[_modelCount];
            Platform ePlatform = _platform;
            for (UInt16 m = 0; m < _modelCount; m++)
            {
                _models[m] = SR1ObjectModel.Load(xReader, _dataStart, _modelStart, _name, _platform, m, _version);
                if (_models[m].Platform == Platform.Dreamcast)
                {
                    ePlatform = _models[m].Platform;
                }
            }
            _platform = ePlatform;
        }

        protected override void ReadUnitData(BinaryReader xReader)
        {
            // Connected unit names
            xReader.BaseStream.Position = _dataStart;
            UInt32 m_uConnectionData = _dataStart + xReader.ReadUInt32();
            xReader.BaseStream.Position = m_uConnectionData + 0x30;
            xReader.BaseStream.Position = _dataStart + xReader.ReadUInt32();
            _connectedUnitCount = xReader.ReadUInt32();
            _connectedUnitNames = new String[_connectedUnitCount];
            for (int i = 0; i < _connectedUnitCount; i++)
            {
                String strUnitName = new String(xReader.ReadChars(12));
                _connectedUnitNames[i] = Utility.CleanName(strUnitName);
                xReader.BaseStream.Position += 0x50;
            }

            // Instances
            xReader.BaseStream.Position = _dataStart + 0x78;
            _instanceCount = xReader.ReadUInt32();
            _instanceStart = _dataStart + xReader.ReadUInt32();
            _instanceNames = new String[_instanceCount];
            for (int i = 0; i < _instanceCount; i++)
            {
                xReader.BaseStream.Position = _instanceStart + 0x4C * i;
                String strInstanceName = new String(xReader.ReadChars(8));
                _instanceNames[i] = Utility.CleanName(strInstanceName);
            }

            // Instance types
            xReader.BaseStream.Position = _dataStart + 0x8C;
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
            xReader.BaseStream.Position = _dataStart + 0x98;
            xReader.BaseStream.Position = _dataStart + xReader.ReadUInt32();
            String strModelName = new String(xReader.ReadChars(8));
            _name = Utility.CleanName(strModelName);

            // Texture type
            xReader.BaseStream.Position = _dataStart + 0x9C;
            if (xReader.ReadUInt64() != 0xFFFFFFFFFFFFFFFF)
            {
                _platform = Platform.PSX;
            }
            else
            {
                _platform = Platform.PC;
            }

            // Connected unit list. (unreferenced?)
            //xReader.BaseStream.Position = m_uDataStart + 0xC0;
            //m_uConnectedUnitsStart = m_uDataStart + xReader.ReadUInt32() + 0x08;
            //xReader.BaseStream.Position = m_uConnectedUnitsStart;
            //xReader.BaseStream.Position += 0x18;
            //String strUnitName0 = new String(xReader.ReadChars(16));
            //strUnitName0 = strUnitName0.Substring(0, strUnitName0.IndexOf(','));
            //xReader.BaseStream.Position += 0x18;
            //String strUnitName1 = new String(xReader.ReadChars(16));
            //strUnitName1 = strUnitName1.Substring(0, strUnitName1.IndexOf(','));
            //xReader.BaseStream.Position += 0x18;
            //String strUnitName2 = new String(xReader.ReadChars(16));
            //strUnitName2 = strUnitName2.Substring(0, strUnitName2.IndexOf(','));

            // Version number
            xReader.BaseStream.Position = _dataStart + 0xF0;
            _version = xReader.ReadUInt32();
            if (_version != RETAIL_VERSION && _version != BETA_VERSION)
            {
                throw new Exception("Wrong version number for level x");
            }

            // Model data
            xReader.BaseStream.Position = _dataStart;
            _modelCount = 1;
            _modelStart = _dataStart;
            _models = new SR1Model[_modelCount];
            xReader.BaseStream.Position = _modelStart;
            UInt32 m_uModelData = _dataStart + xReader.ReadUInt32();

            // Material data
            _models[0] = SR1UnitModel.Load(xReader, _dataStart, m_uModelData, _name, _platform, _version);

            //if (m_axModels[0].Platform == Platform.Dreamcast ||
            //    m_axModels[1].Platform == Platform.Dreamcast)
            //{
            //    m_ePlatform = Platform.Dreamcast;
            //}
        }

        protected override void ResolvePointers(BinaryReader xReader, BinaryWriter xWriter)
        {
            UInt32 uDataStart = ((xReader.ReadUInt32() >> 9) << 11) + 0x00000800;
            if (xReader.ReadUInt32() == 0x00000000)
            {
                _asset = Asset.Unit;
            }
            else
            {
                _asset = Asset.Object;
            }

            xReader.BaseStream.Position = uDataStart;
            xWriter.BaseStream.Position = 0;

            xReader.BaseStream.CopyTo(xWriter.BaseStream);
        }
    }
}