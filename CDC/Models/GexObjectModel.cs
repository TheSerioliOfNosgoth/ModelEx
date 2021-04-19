using System;
using System.IO;
using System.Collections.Generic;

namespace CDC.Objects.Models
{
    public class GexObjectModel : GexModel
    {
        public GexObjectModel(BinaryReader xReader, UInt32 uDataStart, UInt32 uModelData, String strModelName, Platform ePlatform, UInt32 uVersion)
            : base(xReader, uDataStart, uModelData, strModelName, ePlatform, uVersion)
        {
            xReader.BaseStream.Position = _modelData;
            _vertexCount = xReader.ReadUInt16();
            xReader.BaseStream.Position += 0x02;
            _polygonCount = xReader.ReadUInt16();
            _boneCount = xReader.ReadUInt16();
            _vertexStart = xReader.ReadUInt32();
            _vertexScale.x = 1.0f;
            _vertexScale.y = 1.0f;
            _vertexScale.z = 1.0f;
            // Vertex colours here? Objects have them in Gex?
            xReader.BaseStream.Position += 0x08;
            _polygonStart = xReader.ReadUInt32();
            _boneStart = xReader.ReadUInt32();
            _materialStart = xReader.ReadUInt32();
            _materialCount = 0;
            _groupCount = 1;

            _trees = new Tree[_groupCount];
        }

        public static GexObjectModel Load(BinaryReader xReader, UInt32 uDataStart, UInt32 uModelData, String strModelName, Platform ePlatform, UInt16 usIndex, UInt32 uVersion, CDC.Objects.ExportOptions options)
        {
            xReader.BaseStream.Position = uModelData + (0x00000004 * usIndex);
            uModelData = xReader.ReadUInt32();
            xReader.BaseStream.Position = uModelData;
            GexObjectModel xModel = new GexObjectModel(xReader, uDataStart, uModelData, strModelName, ePlatform, uVersion);
            xModel.ReadData(xReader, options);
            return xModel;
        }

        protected override void ReadVertex(BinaryReader xReader, int v, CDC.Objects.ExportOptions options)
        {
            base.ReadVertex(xReader, v, options);

            _geometry.PositionsPhys[v] = _geometry.PositionsRaw[v];
            _geometry.PositionsAltPhys[v] = _geometry.PositionsPhys[v];

            _geometry.Vertices[v].normalID = xReader.ReadUInt16();
        }

        protected override void ReadVertices(BinaryReader xReader, CDC.Objects.ExportOptions options)
        {
            base.ReadVertices(xReader, options);

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
                    for (UInt16 ancestorID = b; ancestorID != 0xFFFF;)
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
                        _geometry.PositionsPhys[v] += _bones[b].worldPos;
                        _geometry.Vertices[v].boneID = b;
                    }
                }
            }
            return;
        }

        protected virtual void ReadPolygon(BinaryReader xReader, int p, CDC.Objects.ExportOptions options)
        {
            UInt32 uPolygonPosition = (UInt32)xReader.BaseStream.Position;

            _polygons[p].v1 = _geometry.Vertices[xReader.ReadUInt16()];
            _polygons[p].v2 = _geometry.Vertices[xReader.ReadUInt16()];
            _polygons[p].v3 = _geometry.Vertices[xReader.ReadUInt16()];

            _polygons[p].material = new Material();
            _polygons[p].material.visible = true;
            _polygons[p].material.textureUsed = false;
            xReader.ReadByte();
            _polygons[p].material.polygonFlags = xReader.ReadByte();

            if ((_polygons[p].material.polygonFlags & 0x02) == 0x02)
            {
                _polygons[p].material.textureUsed = true;
            }

            if (_polygons[p].material.textureUsed)
            {
                UInt32 uMaterialPosition = xReader.ReadUInt32();

                xReader.BaseStream.Position = uMaterialPosition;
                ReadMaterial(xReader, p, options);

                xReader.BaseStream.Position += 0x02;

                _polygons[p].material.colour = xReader.ReadUInt32() | 0xFF000000;
            }
            else
            {
                _polygons[p].material.colour = xReader.ReadUInt32() | 0xFF000000;
            }

            Utility.FlipRedAndBlue(ref _polygons[p].material.colour);

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
                ReadPolygon(xReader, p, options);
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
    }
}