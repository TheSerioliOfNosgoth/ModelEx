using System;
using System.IO;
using System.Collections.Generic;

namespace CDC.Objects.Models
{
	public class GexUnitModel : GexModel
	{
		protected UInt32 m_uBspTreeCount;
		protected UInt32 m_uBspTreeStart;
		protected UInt32 _vertexColourCount;
		protected UInt32 _vertexColourStart;
		protected UInt32 _polygonEnd;

		public GexUnitModel(BinaryReader xReader, UInt32 uDataStart, UInt32 uModelData, String strModelName, Platform ePlatform, UInt32 uVersion, List<ushort> tPages)
			: base(xReader, uDataStart, uModelData, strModelName, ePlatform, uVersion, tPages)
		{
			xReader.BaseStream.Position = _modelData;

			m_uBspTreeCount = 1;
			m_uBspTreeStart = xReader.ReadUInt32();
			_groupCount = m_uBspTreeCount;

			xReader.BaseStream.Position += 0x14;
			_vertexCount = xReader.ReadUInt32();
			_vertexColourCount = xReader.ReadUInt32();
			_polygonCount = xReader.ReadUInt32();
			xReader.BaseStream.Position += 0x0C;
			_vertexStart = xReader.ReadUInt32();
			_vertexColourStart = xReader.ReadUInt32();
			_polygonStart = xReader.ReadUInt32();
			UInt32 _otherThing = xReader.ReadUInt32(); // Very short. The 0x1B thing.
			xReader.BaseStream.Position += 0x04; // Collision
			_materialStart = xReader.ReadUInt32();
			_materialCount = 0;

			_trees = new Tree[_groupCount];
		}

		public static GexUnitModel Load(BinaryReader xReader, UInt32 uDataStart, UInt32 uModelData, String strModelName, Platform ePlatform, UInt32 uVersion, List<ushort> tPages, CDC.Objects.ExportOptions options)
		{
			GexUnitModel xModel = new GexUnitModel(xReader, uDataStart, uModelData, strModelName, ePlatform, uVersion, tPages);
			xModel.ReadData(xReader, options);
			return xModel;
		}

		protected override void ReadData(BinaryReader xReader, CDC.Objects.ExportOptions options)
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
			ReadVertices(xReader, options);
			ReadVertexColours(xReader, options);

			// Get the polygons
			_polygons = new Polygon[_polygonCount];
			_geometry.UVs = new UV[_indexCount];
			ReadPolygons(xReader, options);

			// Generate the output
			GenerateOutput();
		}

		protected override void ReadVertex(BinaryReader xReader, int v, CDC.Objects.ExportOptions options)
		{
			base.ReadVertex(xReader, v, options);

			_geometry.PositionsPhys[v] = _geometry.PositionsRaw[v];
			_geometry.PositionsAltPhys[v] = _geometry.PositionsPhys[v];

			_geometry.Vertices[v].colourID = xReader.ReadUInt16();
		}

		protected override void ReadVertices(BinaryReader xReader, CDC.Objects.ExportOptions options)
		{
			base.ReadVertices(xReader, options);
		}

		protected void ReadVertexColours(BinaryReader xReader, CDC.Objects.ExportOptions options)
		{
			if (_vertexColourStart == 0 || _vertexColourCount == 0)
			{
				return;
			}

			xReader.BaseStream.Position = _vertexColourStart;

			for (int c = 0; c < _vertexColourCount; c++)
			{
				uint vColour = xReader.ReadUInt32() | 0xFF000000;
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

		protected virtual void ReadPolygon(BinaryReader xReader, int p, CDC.Objects.ExportOptions options)
		{
			UInt32 uPolygonPosition = (UInt32)xReader.BaseStream.Position;

			_polygons[p].v1 = _geometry.Vertices[xReader.ReadUInt16()];
			_polygons[p].v2 = _geometry.Vertices[xReader.ReadUInt16()];
			_polygons[p].v3 = _geometry.Vertices[xReader.ReadUInt16()];
			_polygons[p].material = new Material();

			byte flags0 = xReader.ReadByte();
			byte flags1 = xReader.ReadByte();
			xReader.BaseStream.Position += 2;

			_polygons[p].material.polygonFlags = flags0;

			UInt16 uMaterialOffset = xReader.ReadUInt16();

			_polygons[p].material.visible = true;
			if (uMaterialOffset == 0xFFFF)
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
				UInt32 uMaterialPosition = uMaterialOffset + _materialStart;

				xReader.BaseStream.Position = uMaterialPosition;

				ReadMaterial(xReader, p, options);
			}

			Utility.FlipRedAndBlue(ref _polygons[p].material.colour);

			xReader.BaseStream.Position = uPolygonPosition + 0x0C;

			if ((flags1 & 0x04) != 0)
			{
				Vertex tempVertex = _polygons[p].v2;
				_polygons[p].v2 = _polygons[p].v3;
				_polygons[p].v3 = tempVertex;
			}
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

			List<Mesh> xMeshes = new List<Mesh>();
			List<int> xMeshPositions = new List<int>();
			List<UInt32> treePolygons = new List<UInt32>((Int32)_vertexCount * 3);

			_trees[0] = ReadBSPTree(xReader, treePolygons, m_uBspTreeStart, _trees[0], xMeshes, xMeshPositions, 0);

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

		protected virtual Tree ReadBSPTree(BinaryReader xReader, List<UInt32> treePolygons, UInt32 uDataPos, Tree xParentTree, List<Mesh> xMeshes, List<int> xMeshPositions, UInt32 uDepth)
		{
			if (uDataPos == 0)
			{
				return null;
			}

			xReader.BaseStream.Position = uDataPos + 0x0C;
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

			if (isLeaf)
			{
				xTree.isLeaf = true;

				xReader.BaseStream.Position = uDataPos;
				ReadBSPLeaf(xReader, treePolygons, xMesh);
			}
			else
			{
				xReader.BaseStream.Position = uDataPos + 0x10;

				UInt32[] auSubTreePositions = new UInt32[2];
				for (Int32 s = 0; s < iSubTreeCount; s++)
				{
					auSubTreePositions[s] = xReader.ReadUInt32();
				}

				for (Int32 s = 0; s < iSubTreeCount; s++)
				{
					ReadBSPTree(xReader, treePolygons, auSubTreePositions[s], xTree, xMeshes, xMeshPositions, uDepth + 1);
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

		protected virtual void ReadBSPLeaf(BinaryReader xReader, List<UInt32> treePolygons, Mesh xMesh)
		{
			xReader.BaseStream.Position += 0x0D;
			UInt16 polyCount = xReader.ReadByte();
			xReader.BaseStream.Position += 0x02;
			UInt32 polygonPos = xReader.ReadUInt32();
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