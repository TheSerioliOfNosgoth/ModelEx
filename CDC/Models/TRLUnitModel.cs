using System;
using System.IO;
using System.Collections.Generic;

namespace CDC
{
	public class TRLUnitModel : TRLModel
	{
		protected UInt32 _octTreeCount;
		protected UInt32 _octTreeStart;
		protected UInt32 _vmoVertexStart;
		protected UInt32 _vmoVertexCount;

		public TRLUnitModel(BinaryReader reader, DataFile dataFile, UInt32 dataStart, UInt32 modelData, String modelName, Platform ePlatform, UInt32 version)
			: base(reader, dataFile, dataStart, modelData, modelName, ePlatform, version)
		{
			// reader.BaseStream.Position += 0x08;
			// _introCount = reader.ReadUInt32();
			// _introStart = _dataStart + reader.ReadUInt32();
			reader.BaseStream.Position = _modelData + 0x14;
			_octTreeCount = reader.ReadUInt32();
			_octTreeStart = _dataStart + reader.ReadUInt32();
			reader.BaseStream.Position = _modelData + 0x44;
			_vertexStart = _dataStart + reader.ReadUInt32();
			_vmoVertexStart = _dataStart + reader.ReadUInt32();
			reader.BaseStream.Position = _modelData + 0x54;
			_vertexCount = reader.ReadUInt32();
			reader.BaseStream.Position = _modelData + 0x5C;
			_vmoVertexCount = reader.ReadUInt32();
			_polygonCount = 0;
			_polygonStart = 0;
			_materialStart = 0;
			_materialCount = 0;
			_groupCount = _octTreeCount;

			_trees = new Tree[_groupCount];
		}

		public override void ReadData(BinaryReader reader, ExportOptions options)
		{
			// Get the extra vertices
			_extraGeometry.Vertices = new Vertex[_vmoVertexCount];
			_extraGeometry.PositionsRaw = new Vector[_vmoVertexCount];
			_extraGeometry.PositionsPhys = new Vector[_vmoVertexCount];
			_extraGeometry.PositionsAltPhys = new Vector[_vmoVertexCount];
			_extraGeometry.Colours = new UInt32[_vmoVertexCount];
			_extraGeometry.ColoursAlt = new UInt32[_vmoVertexCount];
			_extraGeometry.UVs = new UV[_vmoVertexCount];

			base.ReadData(reader, options);
		}

		protected override void ReadVertex(BinaryReader reader, int v, ExportOptions options)
		{
			base.ReadVertex(reader, v, options);

			reader.BaseStream.Position += 0x02;

			_geometry.PositionsPhys[v] = _geometry.PositionsRaw[v];
			_geometry.PositionsAltPhys[v] = _geometry.PositionsPhys[v];

			_geometry.Vertices[v].colourID = v;

			uint vColour = reader.ReadUInt32();

			if (options.IgnoreVertexColours)
			{
				_geometry.Colours[v] = 0xFFFFFFFF;
			}
			else
			{
				_geometry.Colours[v] = vColour;
				_geometry.ColoursAlt[v] = _geometry.Colours[v];
			}

			_geometry.Vertices[v].UVID = v;

			Int16 vU = reader.ReadInt16();
			Int16 vV = reader.ReadInt16();

			_geometry.UVs[v].u = vU * 0.00024414062f;
			_geometry.UVs[v].v = vV * 0.00024414062f;

			reader.BaseStream.Position += 0x04;
		}

		protected virtual void ReadMorphVertex(BinaryReader reader, int v, ExportOptions options)
		{
			_extraGeometry.Vertices[v].positionID = v;

			// Read the local coordinates
			_extraGeometry.PositionsRaw[v].x = (float)reader.ReadInt16();
			_extraGeometry.PositionsRaw[v].y = (float)reader.ReadInt16();
			_extraGeometry.PositionsRaw[v].z = (float)reader.ReadInt16();

			reader.BaseStream.Position += 0x02;

			_extraGeometry.PositionsPhys[v] = _extraGeometry.PositionsRaw[v];
			_extraGeometry.PositionsAltPhys[v] = _extraGeometry.PositionsPhys[v];

			_extraGeometry.Vertices[v].colourID = v;

			uint vColour = reader.ReadUInt32();

			if (options.IgnoreVertexColours)
			{
				_extraGeometry.Colours[v] = 0xFFFFFFFF;
			}
			else
			{
				_extraGeometry.Colours[v] = vColour;
				_extraGeometry.ColoursAlt[v] = _extraGeometry.Colours[v];
			}

			_extraGeometry.Vertices[v].UVID = v;

			Int16 vU = reader.ReadInt16();
			Int16 vV = reader.ReadInt16();

			_extraGeometry.UVs[v].u = vU * 0.00024414062f;
			_extraGeometry.UVs[v].v = vV * 0.00024414062f;

			reader.BaseStream.Position += 0x14;
		}

		protected override void ReadVertices(BinaryReader reader, ExportOptions options)
		{
			base.ReadVertices(reader, options);

			// Read the VMO vertices
			reader.BaseStream.Position = _vmoVertexStart;

			for (int v = 0; v < _vmoVertexCount; v++)
			{
				ReadMorphVertex(reader, v, options);
			}
		}

		protected override void ReadPolygons(BinaryReader reader, ExportOptions options)
		{
			Material xMaterial = new Material();
			xMaterial.textureID = 0;
			xMaterial.colour = 0xFFFFFFFF;
			_materialsList.Add(xMaterial);

			List<Mesh> xMeshes = new List<Mesh>();
			List<int> xMeshPositions = new List<int>();
			List<TreePolygon> treePolygons = new List<TreePolygon>((Int32)_vertexCount * 3);

			for (UInt32 t = 0; t < _octTreeCount; t++)
			{
				reader.BaseStream.Position = _octTreeStart + (t * 0xB0);

				Vector globalOffset = new Vector();
				globalOffset.x = reader.ReadSingle();
				globalOffset.y = reader.ReadSingle();
				globalOffset.z = reader.ReadSingle();

				reader.BaseStream.Position += 0x18;
				UInt32 uOctID = reader.ReadUInt32();
				reader.BaseStream.Position += 0x1C;
				UInt32 uDataPos = reader.ReadUInt32();
				reader.BaseStream.Position += 0x48;
				UInt32 materialListPos = reader.ReadUInt32();

				reader.BaseStream.Position = materialListPos;
				UInt32 uNumMaterials = reader.ReadUInt32();
				List<TRLMaterial> materials = new List<TRLMaterial>();

				for (UInt32 m = 0; m < uNumMaterials; m++)
				{
					TRLMaterial material = new TRLMaterial();
					ReadMaterial(reader, ref material);
					materials.Add(material);
				}

				_trees[t] = ReadOctTree(reader, treePolygons, uDataPos, _trees[t], xMeshes, xMeshPositions, 0, materials);

				if (_trees[t] != null)
				{
					_trees[t].globalOffset = globalOffset;
				}
			}

			_polygonCount = (UInt32)treePolygons.Count;
			_polygons = new Polygon[_polygonCount];

			int currentPosition = 0, currentPolygon = 0;
			for (int m = 0; m < xMeshes.Count; m++)
			{
				FinaliseMesh(treePolygons, currentPosition, xMeshes[m], xMaterial, ref currentPolygon);
				currentPosition = xMeshPositions[m];
			}

			_materialCount = (UInt32)_materialsList.Count;

			return;
		}

		protected virtual void ReadMaterial(BinaryReader reader, ref TRLMaterial material)
		{
			material.textureID = reader.ReadUInt32();
			material.flags = reader.ReadUInt32();
			material.vbBaseOffset = reader.ReadUInt32();
			material.useExtraGeometry = (material.flags & 0x1C) != 0;
			reader.BaseStream.Position += 0x08;
		}

		protected virtual Tree ReadOctTree(BinaryReader reader, List<TreePolygon> treePolygons, UInt32 uDataPos, Tree xParentTree, List<Mesh> xMeshes, List<int> xMeshPositions, UInt32 uDepth, List<TRLMaterial> materials)
		{
			if (uDataPos == 0)
			{
				return null;
			}

			reader.BaseStream.Position = uDataPos + 0x14;
			Int32 iSubTreeCount = reader.ReadInt32();

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

			if (iSubTreeCount == 0)
			{
				xTree.isLeaf = true;

				reader.BaseStream.Position = uDataPos + 0x10;
				ReadOctLeaf(reader, treePolygons, xMesh, materials);
			}
			else
			{
				reader.BaseStream.Position = uDataPos + 0x18;

				UInt32[] auSubTreePositions = new UInt32[iSubTreeCount];
				for (Int32 s = 0; s < iSubTreeCount; s++)
				{
					auSubTreePositions[s] = reader.ReadUInt32();
				}

				for (Int32 s = 0; s < iSubTreeCount; s++)
				{
					ReadOctTree(reader, treePolygons, auSubTreePositions[s], xTree, xMeshes, xMeshPositions, uDepth + 1, materials);
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

		protected virtual void ReadOctLeaf(BinaryReader reader, List<TreePolygon> treePolygons, Mesh xMesh, List<TRLMaterial> materials)
		{
			UInt32 uNextStrip = _dataStart + reader.ReadUInt32();
			reader.BaseStream.Position = uNextStrip;

			int counter = 0;
			while (true)
			{
				bool bShouldWrite = true; // For debug.

				UInt32 uIndexCount = reader.ReadUInt32();

				counter++;

				reader.BaseStream.Position = uNextStrip + 0x2C;

				UInt16[] axStripIndices = new UInt16[uIndexCount];
				for (UInt32 i = 0; i < uIndexCount; i++)
				{
					axStripIndices[i] = reader.ReadUInt16();
				}

				reader.BaseStream.Position = uNextStrip + 0x14;
				UInt32 materialID = reader.ReadUInt32();

				if (bShouldWrite)
				{
					UInt16 i = 0;
					while (i < uIndexCount)
					{
						TreePolygon newPolygon = new TreePolygon();
						TRLMaterial trlMaterial = materials[(int)materialID];
						newPolygon.v1 = axStripIndices[i++];
						newPolygon.v2 = axStripIndices[i++];
						newPolygon.v3 = axStripIndices[i++];
						newPolygon.textureID = trlMaterial.textureID;
						newPolygon.vbBaseOffset = trlMaterial.vbBaseOffset;
						newPolygon.useExtraGeometry = trlMaterial.useExtraGeometry;
						treePolygons.Add(newPolygon);
					}

					if (xMesh != null)
					{
						xMesh.indexCount += uIndexCount;
					}
				}

				reader.BaseStream.Position = uNextStrip + 0x28;
				uNextStrip = reader.ReadUInt32();

				if (uNextStrip == 0 || uIndexCount == 0)
				{
					break;
				}

				reader.BaseStream.Position = uNextStrip;
			}
		}

		protected virtual void FinaliseMesh(List<TreePolygon> treePolygons, int firstPolygon, Mesh xMesh, Material xMaterial, ref int currentPolygon)
		{
			xMesh.polygonCount = xMesh.indexCount / 3;
			xMesh.polygons = new Polygon[xMesh.polygonCount];
			for (int p = 0; p < xMesh.polygonCount; p++)
			{
				UInt32 uV1 = treePolygons[firstPolygon + p].v1;
				UInt32 uV2 = treePolygons[firstPolygon + p].v2;
				UInt32 uV3 = treePolygons[firstPolygon + p].v3;

				if (treePolygons[firstPolygon + p].useExtraGeometry == false)
				{
					xMesh.polygons[p].v1 = _geometry.Vertices[uV1 + treePolygons[firstPolygon + p].vbBaseOffset];
					xMesh.polygons[p].v2 = _geometry.Vertices[uV2 + treePolygons[firstPolygon + p].vbBaseOffset];
					xMesh.polygons[p].v3 = _geometry.Vertices[uV3 + treePolygons[firstPolygon + p].vbBaseOffset];
				}
				else
				{
					xMesh.polygons[p].v1 = _extraGeometry.Vertices[uV1 + treePolygons[firstPolygon + p].vbBaseOffset];
					xMesh.polygons[p].v2 = _extraGeometry.Vertices[uV2 + treePolygons[firstPolygon + p].vbBaseOffset];
					xMesh.polygons[p].v3 = _extraGeometry.Vertices[uV3 + treePolygons[firstPolygon + p].vbBaseOffset];
					xMesh.polygons[p].v1.isExtraGeometry = true;
					xMesh.polygons[p].v2.isExtraGeometry = true;
					xMesh.polygons[p].v3.isExtraGeometry = true;
				}

				xMaterial = new Material();

				xMaterial.visible = true;
				xMaterial.textureID = (UInt16)(treePolygons[firstPolygon + p].textureID & 0x1FFF);
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

			xMesh.vertices = new Vertex[xMesh.indexCount];
			for (UInt32 poly = 0; poly < xMesh.polygonCount; poly++)
			{
				_polygons[currentPolygon++] = xMesh.polygons[poly];
				xMesh.vertices[(3 * poly) + 0] = xMesh.polygons[poly].v1;
				xMesh.vertices[(3 * poly) + 1] = xMesh.polygons[poly].v2;
				xMesh.vertices[(3 * poly) + 2] = xMesh.polygons[poly].v3;
			}
		}
	}
}