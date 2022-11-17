using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CDC
{
    class TRLBGObjectModel : TRLModel
    {
        protected UInt32 _coloursStart;

        public TRLBGObjectModel(BinaryReader reader, DataFile dataFile, UInt32 dataStart, UInt32 modelData, String modelName, Platform ePlatform, UInt32 version)
            : base(reader, dataFile, dataStart, modelData, modelName, ePlatform, version)
        {
            reader.BaseStream.Position = _modelData;

            _vertexScale.x = reader.ReadSingle();
            _vertexScale.y = reader.ReadSingle();
            _vertexScale.z = reader.ReadSingle();

            reader.BaseStream.Position += 0x24;
            _polygonStart = reader.ReadUInt32(); // is actually BGObjectStripInfo[3] but other 2 are LOD models

            reader.BaseStream.Position += 0x10;
            _vertexStart = reader.ReadUInt32();
            _vertexCount = reader.ReadUInt32();
            _coloursStart = reader.ReadUInt32();

            _groupCount = 1;

            _trees = new Tree[_groupCount];
        }

        protected override void ReadVertices(BinaryReader reader, ExportOptions options)
        {
            reader.BaseStream.Position = _coloursStart;

            for (int i = 0; i < _vertexCount; i++)
            {
                UInt32 colour = reader.ReadUInt32();

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

            base.ReadVertices(reader, options);
        }

        protected override void ReadVertex(BinaryReader reader, int v, ExportOptions options)
        {
            base.ReadVertex(reader, v, options);

            _geometry.PositionsPhys[v] = _geometry.PositionsRaw[v] * _vertexScale;
            _geometry.PositionsAltPhys[v] = _geometry.PositionsPhys[v];

            _geometry.Vertices[v].colourID = v;
            _geometry.Vertices[v].UVID = v;

            reader.BaseStream.Position += 2;

            Int16 vU = reader.ReadInt16();
            Int16 vV = reader.ReadInt16();

            _geometry.UVs[v].u = vU * 0.00024414062f;
            _geometry.UVs[v].v = vV * 0.00024414062f;
        }

        protected override void ReadPolygons(BinaryReader reader, ExportOptions options)
        {
            List<Mesh> xMeshes = new List<Mesh>();

            UInt32 nextStrip = _polygonStart;
            UInt32 polygonCount = 0;

            do
            {
                reader.BaseStream.Position = nextStrip;

                UInt32 numVertices = reader.ReadUInt32();

                if (numVertices == 0)
                {
                    break;
                }

                polygonCount += numVertices;

                var xMesh = new Mesh();
                xMesh.indexCount = numVertices;
                xMesh.polygonCount = numVertices / 3;
                xMesh.vertices = new Vertex[xMesh.indexCount];
                xMesh.polygons = new Polygon[numVertices];

                reader.BaseStream.Position += 8;

                UInt32 tpageid = reader.ReadUInt32();

                reader.BaseStream.Position += 8;
                nextStrip = reader.ReadUInt32();

                var xMaterial = new Material();
                xMaterial.visible = true;
                xMaterial.textureUsed = true;
                xMaterial.textureID = ((UInt16)(tpageid & 0x1FFF));

                var material = _materialsList.FirstOrDefault(x => x.textureID == xMaterial.textureID && x.textureUsed);

                if (material == default)
                {
                    _materialsList.Add(xMaterial);
                    _materialCount = (uint)_materialsList.Count;
                }
                else
                {
                    xMaterial = material;
                }

                for (int i = 0; i < numVertices / 3; i++)
                {
                    xMesh.polygons[i].v1 = _geometry.Vertices[reader.ReadUInt16()];
                    xMesh.polygons[i].v2 = _geometry.Vertices[reader.ReadUInt16()];
                    xMesh.polygons[i].v3 = _geometry.Vertices[reader.ReadUInt16()];

                    xMesh.polygons[i].material = xMaterial;
                }

                xMeshes.Add(xMesh);
            }
            while (nextStrip != 0);

            _polygonCount = polygonCount;
            _polygons = new Polygon[_polygonCount];
            _trees = new Tree[xMeshes.Count];

            polygonCount = 0;
            for (int i = 0; i < xMeshes.Count; i++)
            {
                var xMesh = xMeshes[i];

                for (UInt32 poly = 0; poly < xMesh.polygonCount; poly++)
                {
                    _polygons[polygonCount++] = xMesh.polygons[poly];
                    xMesh.vertices[(3 * poly) + 0] = xMesh.polygons[poly].v1;
                    xMesh.vertices[(3 * poly) + 1] = xMesh.polygons[poly].v2;
                    xMesh.vertices[(3 * poly) + 2] = xMesh.polygons[poly].v3;
                }

                var tree = new Tree();
                tree.mesh = xMesh;

                _trees[i] = tree;
            }
        }
    }
}
