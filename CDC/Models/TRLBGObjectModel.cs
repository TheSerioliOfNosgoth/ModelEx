using System;
using System.Collections.Generic;
using System.IO;

namespace CDC
{
    class TRLBGObjectModel : TRLModel
    {
        protected uint _coloursStart;

        public TRLBGObjectModel(BinaryReader reader, DataFile dataFile, uint dataStart, uint modelData, String modelName, Platform ePlatform, uint version)
            : base(reader, dataFile, dataStart, modelData, modelName, ePlatform, version)
        {
            reader.BaseStream.Position = _modelData;

            _vertexScale.x = reader.ReadSingle();
            _vertexScale.y = reader.ReadSingle();
            _vertexScale.z = reader.ReadSingle();
            reader.BaseStream.Position += 0x24;

            // Is actually BGObjectStripInfo[3] but other 2 are LOD models.
            // So, should it be material start or maybe even model start if this was an object?
            // Do it like objects in Gex?
            _materialStart = reader.ReadUInt32(); // bgObjectStripInfo 0
            reader.BaseStream.Position += 0x04;  // bgObjectStripInfo 1
            reader.BaseStream.Position += 0x04;  // bgObjectStripInfo 2
            reader.BaseStream.Position += 0x04;  // lodDistance 0
            reader.BaseStream.Position += 0x04;  // lodDistance 1

            _vertexStart = reader.ReadUInt32();
            _vertexCount = reader.ReadUInt32();
            _coloursStart = reader.ReadUInt32();
            _groupCount = 1;

            _trees = new Tree[_groupCount];
        }

        protected override void ReadVertex(BinaryReader reader, int v, ExportOptions options)
        {
            base.ReadVertex(reader, v, options);

            _geometry.PositionsPhys[v] = _geometry.PositionsRaw[v] * _vertexScale;
            _geometry.PositionsAltPhys[v] = _geometry.PositionsPhys[v];

            _geometry.Vertices[v].colourID = v;
            _geometry.Vertices[v].UVID = v;

            reader.BaseStream.Position += 2;

            short vU = reader.ReadInt16();
            short vV = reader.ReadInt16();

            _geometry.UVs[v].u = vU * 0.00024414062f;
            _geometry.UVs[v].v = vV * 0.00024414062f;
        }

        protected override void ReadVertices(BinaryReader reader, ExportOptions options)
        {
            base.ReadVertices(reader, options);

            reader.BaseStream.Position = _coloursStart;

            for (int i = 0; i < _vertexCount; i++)
            {
                uint colour = reader.ReadUInt32();

                if (options.IgnoreVertexColours)
                {
                    _geometry.Colours[i] = 0xFFFFFFFF;
                }
                else
                {
                    _geometry.Colours[i] = colour;
                    _geometry.ColoursAlt[i] = _geometry.Colours[i];
                }
            }
        }

        protected override void ReadPolygons(BinaryReader reader, ExportOptions options)
        {
            if (_materialStart == 0)
            {
                return;
            }

            List<TRLTriangleList> triangleListList = new List<TRLTriangleList>();
            uint materialPosition = _materialStart;
            _groupCount = 0;

            while (materialPosition != 0)
            {
                reader.BaseStream.Position = materialPosition;

                TRLTriangleList triangleList = new TRLTriangleList();
                ReadTriangleList(reader, ref triangleList);
                triangleListList.Add(triangleList);
                _polygonCount += triangleList.polygonCount;

                _materialsList.Add(triangleList.material);

                materialPosition = triangleList.next;
            }

            _materialCount = (uint)_materialsList.Count;

            _groupCount++;
            _trees = new Tree[_groupCount];
            _trees[0] = new Tree();
            _trees[0].mesh = new Mesh();

            foreach (TRLTriangleList triangleList in triangleListList)
            {
                _trees[0].mesh.polygonCount += triangleList.polygonCount;
            }

            _trees[0].mesh.indexCount = _trees[0].mesh.polygonCount * 3;
            _trees[0].mesh.polygons = new Polygon[_trees[0].mesh.polygonCount];
            _trees[0].mesh.vertices = new Vertex[_trees[0].mesh.indexCount];

            uint tp = 0;
            foreach (TRLTriangleList triangleList in triangleListList)
            {
                reader.BaseStream.Position = triangleList.polygonStart;
                for (int pl = 0; pl < triangleList.polygonCount; pl++)
                {
                    _trees[0].mesh.polygons[tp].v1 = _geometry.Vertices[reader.ReadUInt16()];
                    _trees[0].mesh.polygons[tp].v2 = _geometry.Vertices[reader.ReadUInt16()];
                    _trees[0].mesh.polygons[tp].v3 = _geometry.Vertices[reader.ReadUInt16()];
                    _trees[0].mesh.polygons[tp].material = triangleList.material;
                    tp++;
                }
            }

            for (ushort poly = 0; poly < _trees[0].mesh.polygonCount; poly++)
            {
                _trees[0].mesh.vertices[(3 * poly) + 0] = _trees[0].mesh.polygons[poly].v1;
                _trees[0].mesh.vertices[(3 * poly) + 1] = _trees[0].mesh.polygons[poly].v2;
                _trees[0].mesh.vertices[(3 * poly) + 2] = _trees[0].mesh.polygons[poly].v3;
            }

            _polygons = new Polygon[_polygonCount];
            uint p = 0;
            foreach (TRLTriangleList triangleList in triangleListList)
            {
                reader.BaseStream.Position = triangleList.polygonStart;
                for (int pl = 0; pl < triangleList.polygonCount; pl++)
                {
                    _polygons[p].v1 = _geometry.Vertices[reader.ReadUInt16()];
                    _polygons[p].v2 = _geometry.Vertices[reader.ReadUInt16()];
                    _polygons[p].v3 = _geometry.Vertices[reader.ReadUInt16()];
                    _polygons[p].material = triangleList.material;
                    p++;
                }
            }
        }

        protected virtual bool ReadTriangleList(BinaryReader reader, ref TRLTriangleList triangleList)
        {
            triangleList.polygonCount = reader.ReadUInt32() / 3;
            triangleList.groupID = 0;
            reader.BaseStream.Position += 8;
            uint tpageid = reader.ReadUInt32();
            reader.BaseStream.Position += 8;
            triangleList.material = new Material();
            triangleList.material.visible = true;
            triangleList.material.textureUsed = true;
            triangleList.material.textureID = (ushort)(tpageid & 0x1FFF);
            triangleList.material.colour = 0xFFFFFFFF;
            if (triangleList.material.textureID > 0)
            {
                triangleList.material.textureUsed = true;
            }
            else
            {
                triangleList.material.textureUsed = false;
            }

            if ((tpageid & 0x0001E000) == 0x00012000)
			{
                triangleList.material.colour = 0x00000000;
                triangleList.material.visible = false;
                triangleList.material.textureUsed = false;
			}

            triangleList.next = reader.ReadUInt32();
            triangleList.polygonStart = (uint)reader.BaseStream.Position;

            if (triangleList.polygonCount == 0)
            {
                triangleList.next = 0;
            }

            return (triangleList.material.visible);
        }
    }
}
