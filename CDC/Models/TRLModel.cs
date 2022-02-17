using System;
using System.Collections.Generic;
using System.IO;

namespace CDC.Objects.Models
{
	public abstract class TRLModel : SRModel
	{
		protected class TRLTriangleList
		{
			public Material material;
			public UInt32 polygonCount;
			public UInt32 polygonStart;
			public UInt16 groupID;
			public UInt32 next;
		}

		protected class TRLMaterial
		{
			public UInt32 textureID;
			public UInt32 vbBaseOffset;
		}

		protected TRLModel(BinaryReader reader, UInt32 dataStart, UInt32 modelData, String strModelName, Platform ePlatform, UInt32 version) :
			base(reader, dataStart, modelData, strModelName, ePlatform, version)
		{
		}

		protected virtual void ReadData(BinaryReader reader, CDC.Objects.ExportOptions options)
		{
			// Get the vertices
			_geometry.Vertices = new Vertex[_vertexCount];
			_geometry.PositionsRaw = new Vector[_vertexCount];
			_geometry.PositionsPhys = new Vector[_vertexCount];
			_geometry.PositionsAltPhys = new Vector[_vertexCount];
			_geometry.Normals = new Vector[_vertexCount];
			_geometry.Colours = new UInt32[_vertexCount];
			_geometry.ColoursAlt = new UInt32[_vertexCount];
			_geometry.UVs = new UV[_vertexCount];
			ReadVertices(reader, options);

			// Get the polygons
			_polygons = new Polygon[_polygonCount];
			ReadPolygons(reader, options);

			HandleDebugRendering(options);

			// Generate the output
			GenerateOutput();
		}

		protected virtual void ReadVertex(BinaryReader reader, int v, CDC.Objects.ExportOptions options)
		{
			_geometry.Vertices[v].positionID = v;

			// Read the local coordinates
			_geometry.PositionsRaw[v].x = (float)reader.ReadInt16();
			_geometry.PositionsRaw[v].y = (float)reader.ReadInt16();
			_geometry.PositionsRaw[v].z = (float)reader.ReadInt16();
		}

		protected virtual void ReadVertices(BinaryReader reader, CDC.Objects.ExportOptions options)
		{
			if (_vertexStart == 0 || _vertexCount == 0)
			{
				return;
			}

			reader.BaseStream.Position = _vertexStart;

			for (int v = 0; v < _vertexCount; v++)
			{
				ReadVertex(reader, v, options);
			}

			return;
		}

		protected abstract void ReadPolygons(BinaryReader reader, CDC.Objects.ExportOptions options);

		protected virtual void GenerateOutput()
		{
			// Make the vertices unique
			_geometry.Vertices = new Vertex[_indexCount];
			for (UInt32 p = 0; p < _polygonCount; p++)
			{
				_geometry.Vertices[(3 * p) + 0] = _polygons[p].v1;
				_geometry.Vertices[(3 * p) + 1] = _polygons[p].v2;
				_geometry.Vertices[(3 * p) + 2] = _polygons[p].v3;
			}

			// Build the materials array
			_materials = new Material[_materialCount];
			UInt16 mNew = 0;

			foreach (Material xMaterial in _materialsList)
			{
				_materials[mNew] = xMaterial;
				_materials[mNew].ID = mNew;
				mNew++;
			}

			return;
		}
	}
}
