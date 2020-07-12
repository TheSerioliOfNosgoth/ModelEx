using System;
using System.IO;
using System.Collections.Generic;

namespace CDC.Objects.Models
{
    public class DefianceObjectModel : DefianceModel
    {
        protected UInt32 m_uColourStart;

        protected DefianceObjectModel(BinaryReader xReader, UInt32 uDataStart, UInt32 uModelData, String strModelName, Platform ePlatform, UInt32 uVersion)
            : base(xReader, uDataStart, uModelData, strModelName, ePlatform, uVersion)
        {
            xReader.BaseStream.Position = _modelData + 0x04;
            UInt32 uBoneCount1 = xReader.ReadUInt32();
            UInt32 uBoneCount2 = xReader.ReadUInt32();
            _boneCount = uBoneCount1 + uBoneCount2;
            _boneStart = xReader.ReadUInt32();
            _vertexScale.x = xReader.ReadSingle();
            _vertexScale.y = xReader.ReadSingle();
            _vertexScale.z = xReader.ReadSingle();
            xReader.BaseStream.Position += 0x04;
            _vertexCount = xReader.ReadUInt32();
            _vertexStart = _dataStart + xReader.ReadUInt32();
            xReader.BaseStream.Position += 0x08;
            _polygonCount = 0; // xReader.ReadUInt32();
            _polygonStart = 0; // m_uDataStart + xReader.ReadUInt32();
            xReader.BaseStream.Position += 0x28;
            _materialStart = _dataStart + xReader.ReadUInt32();
            xReader.BaseStream.Position += 0x04;
            //_materialStart = 0; // ^^Whatever that was, it's a dword and then an array of shorts. 
            _materialCount = 0;
            m_uColourStart = _dataStart + xReader.ReadUInt32();
            _groupCount = 1;

            _trees = new Tree[_groupCount];
        }

        public static DefianceObjectModel Load(BinaryReader xReader, UInt32 uDataStart, UInt32 uModelData, String strModelName, Platform ePlatform, UInt16 usIndex, UInt32 uVersion, CDC.Objects.ExportOptions options)
        {
            xReader.BaseStream.Position = uModelData + (0x00000004 * usIndex);
            uModelData = uDataStart + xReader.ReadUInt32();
            xReader.BaseStream.Position = uModelData;
            DefianceObjectModel xModel = new DefianceObjectModel(xReader, uDataStart, uModelData, strModelName, ePlatform, uVersion);
            xModel.ReadData(xReader, options);
            return xModel;
        }

        protected override void ReadTypeAVertex(BinaryReader xReader, int v, CDC.Objects.ExportOptions options)
        {
            base.ReadTypeAVertex(xReader, v, options);

            _geometry.PositionsPhys[v] = _geometry.PositionsRaw[v] * _vertexScale;
            _geometry.PositionsAltPhys[v] = _geometry.PositionsPhys[v];

            _geometry.Vertices[v].normalID = xReader.ReadByte();
            xReader.BaseStream.Position += 0x03;

            _geometry.Vertices[v].UVID = v;

            UInt16 vU = xReader.ReadUInt16();
            UInt16 vV = xReader.ReadUInt16();

            _geometry.UVs[v].u = Utility.BizarreFloatToNormalFloat(vU);
            _geometry.UVs[v].v = Utility.BizarreFloatToNormalFloat(vV);
        }

        protected override void ReadTypeAVertices(BinaryReader xReader, CDC.Objects.ExportOptions options)
        {
            base.ReadTypeAVertices(xReader, options);

            xReader.BaseStream.Position = m_uColourStart;
            for (UInt16 v = 0; v < _vertexCount; v++)
            {
                _geometry.Colours[v] = xReader.ReadUInt32();
                _geometry.ColoursAlt[v] = _geometry.Colours[v];
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
                        _geometry.PositionsPhys[v] += _bones[b].worldPos;
                        _geometry.Vertices[v].boneID = b;
                    }
                }
            }
            return;
        }

        protected override void ReadPolygons(BinaryReader xReader, CDC.Objects.ExportOptions options)
        {
            if (_materialStart == 0)
            {
                return;
            }

            List<DefianceTriangleList> xTriangleListList = new List<DefianceTriangleList>();
            UInt32 uMaterialPosition = _materialStart;
            _groupCount = 0;
            while (uMaterialPosition != 0)
            {
                xReader.BaseStream.Position = uMaterialPosition;
                DefianceTriangleList xTriangleList = new DefianceTriangleList();

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

                foreach (DefianceTriangleList xTriangleList in xTriangleListList)
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
                foreach (DefianceTriangleList xTriangleList in xTriangleListList)
                {
                    if (t != (UInt32)xTriangleList.m_usGroupID)
                    {
                        continue;
                    }

                    xReader.BaseStream.Position = xTriangleList.m_uPolygonStart;
                    for (int pl = 0; pl < xTriangleList.m_uPolygonCount; pl++)
                    {
                        _trees[t].mesh.polygons[tp].v1 = _geometry.Vertices[xReader.ReadUInt16()];
                        _trees[t].mesh.polygons[tp].v2 = _geometry.Vertices[xReader.ReadUInt16()];
                        _trees[t].mesh.polygons[tp].v3 = _geometry.Vertices[xReader.ReadUInt16()];
                        _trees[t].mesh.polygons[tp].material = xTriangleList.m_xMaterial;
                        tp++;
                    }
                }

                for (UInt16 poly = 0; poly < _trees[t].mesh.polygonCount; poly++)
                {
                    _trees[t].mesh.vertices[(3 * poly) + 0] = _trees[t].mesh.polygons[poly].v1;
                    _trees[t].mesh.vertices[(3 * poly) + 1] = _trees[t].mesh.polygons[poly].v2;
                    _trees[t].mesh.vertices[(3 * poly) + 2] = _trees[t].mesh.polygons[poly].v3;
                }
            }

            _polygons = new Polygon[_polygonCount];
            UInt32 p = 0;
            foreach (DefianceTriangleList xTriangleList in xTriangleListList)
            {
                xReader.BaseStream.Position = xTriangleList.m_uPolygonStart;
                for (int pl = 0; pl < xTriangleList.m_uPolygonCount; pl++)
                {
                    _polygons[p].v1 = _geometry.Vertices[xReader.ReadUInt16()];
                    _polygons[p].v2 = _geometry.Vertices[xReader.ReadUInt16()];
                    _polygons[p].v3 = _geometry.Vertices[xReader.ReadUInt16()];
                    _polygons[p].material = xTriangleList.m_xMaterial;
                    p++;
                }
            }
        }

        protected virtual bool ReadTriangleList(BinaryReader xReader, ref DefianceTriangleList xTriangleList)
        {
            xTriangleList.m_uPolygonCount = (UInt32)xReader.ReadUInt16() / 3;
            xTriangleList.m_usGroupID = xReader.ReadUInt16(); // Used by MON_SetAccessories and INSTANCE_UnhideAllDrawGroups
            xTriangleList.m_uPolygonStart = (UInt32)(xReader.BaseStream.Position) + 0x10;
            UInt16 xWord0 = xReader.ReadUInt16();
            UInt16 xWord1 = xReader.ReadUInt16();
            UInt32 xDWord0 = xReader.ReadUInt32();
            UInt32 xDWord1 = xReader.ReadUInt32();
            xTriangleList.m_xMaterial = new Material();
            xTriangleList.m_xMaterial.visible = ((xWord1 & 0xFF00) == 0);
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

            if (xTriangleList.m_uPolygonCount == 0)
            {
                xTriangleList.m_uNext = 0;
            }

            return (xTriangleList.m_xMaterial.visible);
        }
    }
}