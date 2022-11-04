using System;
using System.IO;
using System.Collections.Generic;
using TPages = BenLincoln.TheLostWorlds.CDTextures.PSXTextureDictionary;

namespace CDC.Objects.Models
{
	public class GexUnitModel : GexModel
	{
		protected UInt32 m_uBspTreeCount;
		protected UInt32 m_uBspTreeStart;
		protected UInt32 _vertexColourCount;
		protected UInt32 _vertexColourStart;
		protected UInt32 _polygonEnd;

		public GexUnitModel(BinaryReader reader, UInt32 dataStart, UInt32 modelData, String strModelName, Platform ePlatform, UInt32 version, TPages tPages)
			: base(reader, dataStart, modelData, strModelName, ePlatform, version, tPages)
		{
			reader.BaseStream.Position = _modelData;

			m_uBspTreeCount = 1;
			m_uBspTreeStart = reader.ReadUInt32();
			_groupCount = m_uBspTreeCount;

			reader.BaseStream.Position += 0x14;
			_vertexCount = reader.ReadUInt32();
			_vertexColourCount = reader.ReadUInt32();
			_polygonCount = reader.ReadUInt32();
			reader.BaseStream.Position += 0x0C;
			_vertexStart = reader.ReadUInt32();
			_vertexColourStart = reader.ReadUInt32();
			_polygonStart = reader.ReadUInt32();
			UInt32 _otherThing = reader.ReadUInt32(); // Very short. The 0x1B thing.
			reader.BaseStream.Position += 0x04; // Collision
			_materialStart = reader.ReadUInt32();
			_materialCount = 0;

			_trees = new Tree[_groupCount];
		}

		public static GexUnitModel Load(BinaryReader reader, UInt32 dataStart, UInt32 modelData, String strModelName, Platform ePlatform, UInt32 version, TPages tPages, CDC.Objects.ExportOptions options)
		{
			GexUnitModel xModel = new GexUnitModel(reader, dataStart, modelData, strModelName, ePlatform, version, tPages);
			xModel.ReadData(reader, options);
			return xModel;
		}

		protected override void ReadData(BinaryReader reader, CDC.Objects.ExportOptions options)
		{
			// Get the normals
			_geometry.Normals = new Vector[s_aiNormals.Length / 3];
			for (int n = 0; n < _geometry.Normals.Length; n++)
			{
				_geometry.Normals[n].x = ((float)s_aiNormals[n, 0] / 4096.0f);
				_geometry.Normals[n].y = ((float)s_aiNormals[n, 1] / 4096.0f);
				_geometry.Normals[n].z = ((float)s_aiNormals[n, 2] / 4096.0f);
			}

			// Get the vertices
			_geometry.Vertices = new Vertex[_vertexCount];
			_geometry.PositionsRaw = new Vector[_vertexCount];
			_geometry.PositionsPhys = new Vector[_vertexCount];
			_geometry.PositionsAltPhys = new Vector[_vertexCount];
			_geometry.Colours = new UInt32[_vertexColourStart];
			_geometry.ColoursAlt = new UInt32[_vertexColourStart];
			ReadVertices(reader, options);
			ReadVertexColours(reader, options);

			// Get the polygons
			_polygons = new Polygon[_polygonCount];
			_geometry.UVs = new UV[_indexCount];
			ReadPolygons(reader, options);

			// Generate the output
			GenerateOutput();
		}

		protected override void ReadVertex(BinaryReader reader, int v, CDC.Objects.ExportOptions options)
		{
			base.ReadVertex(reader, v, options);

			_geometry.PositionsPhys[v] = _geometry.PositionsRaw[v];
			_geometry.PositionsAltPhys[v] = _geometry.PositionsPhys[v];

			_geometry.Vertices[v].colourID = reader.ReadUInt16();
		}

		protected override void ReadVertices(BinaryReader reader, CDC.Objects.ExportOptions options)
		{
			base.ReadVertices(reader, options);
		}

		protected void ReadVertexColours(BinaryReader reader, CDC.Objects.ExportOptions options)
		{
			if (_vertexColourStart == 0 || _vertexColourCount == 0)
			{
				return;
			}

			reader.BaseStream.Position = _vertexColourStart;

			for (int c = 0; c < _vertexColourCount; c++)
			{
				uint vColour = reader.ReadUInt32() | 0xFF000000;
				if (options.IgnoreVertexColours)
				{
					_geometry.Colours[c] = 0xFFFFFFFF;
				}
				else
				{
					_geometry.Colours[c] = vColour;
				}

				Utility.FlipRedAndBlue(ref _geometry.Colours[c]);

				_geometry.ColoursAlt[c] = _geometry.Colours[c];
			}

			return;
		}

		protected virtual void ReadPolygon(BinaryReader reader, int p, CDC.Objects.ExportOptions options)
		{
			UInt32 uPolygonPosition = (UInt32)reader.BaseStream.Position;

			_polygons[p].v1 = _geometry.Vertices[reader.ReadUInt16()];
			_polygons[p].v2 = _geometry.Vertices[reader.ReadUInt16()];
			_polygons[p].v3 = _geometry.Vertices[reader.ReadUInt16()];
			_polygons[p].material = new Material();

			byte flags0 = reader.ReadByte();
			byte flags1 = reader.ReadByte();
			reader.BaseStream.Position += 2;

			_polygons[p].material.polygonFlags = flags0;

			UInt16 materialOffset = reader.ReadUInt16();

			_polygons[p].material.visible = true;
			if (materialOffset == 0xFFFF)
			{
				_polygons[p].material.visible = false;
			}

			if ((flags0 & 0x01) == 0x01)
			{
				_polygons[p].material.visible = false;
			}

			if (!_polygons[p].material.visible)
			{
				_polygons[p].material.textureUsed = false;
			}
			else
			{
				_polygons[p].material.textureUsed = true;
			}

			if (options.RenderMode == RenderMode.NoTextures)
			{
				_polygons[p].material.clutValueUsedMask = 0;
				_polygons[p].material.texturePageUsedMask = 0;
				_polygons[p].material.BSPTreeRootFlagsUsedMask = 0;
				_polygons[p].material.BSPTreeAllParentNodeFlagsORdUsedMask = 0;
				_polygons[p].material.BSPTreeParentNodeFlagsUsedMask = 0;
				_polygons[p].material.BSPTreeLeafFlagsUsedMask = 0;
			}

			if (_polygons[p].material.textureUsed)
			{
				UInt32 materialPosition = materialOffset + _materialStart;

				reader.BaseStream.Position = materialPosition;

				ReadMaterial(reader, p, options);
			}

			Utility.FlipRedAndBlue(ref _polygons[p].material.colour);

			reader.BaseStream.Position = uPolygonPosition + 0x0C;

			if ((flags1 & 0x04) != 0)
			{
				Vertex tempVertex = _polygons[p].v2;
				_polygons[p].v2 = _polygons[p].v3;
				_polygons[p].v3 = tempVertex;
			}
		}

		protected override void ReadPolygons(BinaryReader reader, CDC.Objects.ExportOptions options)
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

			List<Mesh> xMeshes = new List<Mesh>();
			List<int> xMeshPositions = new List<int>();
			List<UInt32> treePolygons = new List<UInt32>((Int32)_vertexCount * 3);

			_trees[0] = ReadBSPTree(reader, treePolygons, m_uBspTreeStart, _trees[0], xMeshes, xMeshPositions, 0);

			ProcessPolygons(options);

			int currentPosition = 0;
			for (int m = 0; m < xMeshes.Count; m++)
			{
				FinaliseMesh(treePolygons, currentPosition, xMeshes[m]);
				currentPosition = xMeshPositions[m];
			}
		}

		protected virtual Tree ReadBSPTree(BinaryReader reader, List<UInt32> treePolygons, UInt32 uDataPos, Tree xParentTree, List<Mesh> xMeshes, List<int> xMeshPositions, UInt32 uDepth)
		{
			if (uDataPos == 0)
			{
				return null;
			}

			reader.BaseStream.Position = uDataPos + 0x0C;
			bool isLeaf = ((reader.ReadByte() & 0x02) == 0x02);
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

			if (isLeaf)
			{
				xTree.isLeaf = true;

				reader.BaseStream.Position = uDataPos;
				ReadBSPLeaf(reader, treePolygons, xMesh);
			}
			else
			{
				reader.BaseStream.Position = uDataPos + 0x10;

				UInt32[] auSubTreePositions = new UInt32[2];
				for (Int32 s = 0; s < iSubTreeCount; s++)
				{
					auSubTreePositions[s] = reader.ReadUInt32();
				}

				for (Int32 s = 0; s < iSubTreeCount; s++)
				{
					ReadBSPTree(reader, treePolygons, auSubTreePositions[s], xTree, xMeshes, xMeshPositions, uDepth + 1);
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

		protected virtual void ReadBSPLeaf(BinaryReader reader, List<UInt32> treePolygons, Mesh xMesh)
		{
			reader.BaseStream.Position += 0x0D;
			UInt16 polyCount = reader.ReadByte();
			reader.BaseStream.Position += 0x02;
			UInt32 polygonPos = reader.ReadUInt32();
			UInt32 polygonID = (polygonPos - _polygonStart) / 0x0C;

			for (UInt16 p = 0; p < polyCount; p++)
			{
				//_polygons[polygonID + p].material.visible = true;

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