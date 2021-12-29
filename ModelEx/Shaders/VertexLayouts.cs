using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SlimDX;

namespace ModelEx
{
	public interface IVertexParser<V>
	{
		int VertexCount { get; }
		void FillVertex(int v, out V vertex);
	}

	public interface IIndexParser<I>
	{
		int IndexCount { get; }
		void FillIndex(int i, out I index);
	}

	public interface IMeshParser<V, I> : IVertexParser<V>, IIndexParser<I>
	{
		string MeshName { get; }
		string Technique { get; }
	}

	public interface IModelParser
	{
		string ModelName { get; }
		List<Material> Materials { get; }
		List<Mesh> Meshes { get; }
		List<SubMesh> SubMeshes { get; }
		List<Node> Groups { get; }
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct PositionColoredVertex
	{
		public Vector3 Position;
		public int Color;

		public PositionColoredVertex(Vector3 position, int color)
		{
			Position = position;
			Color = color;
		}

		public PositionColoredVertex(float x, float y, float z, int color)
		{
			Position = new Vector3(x, y, z);
			Color = color;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct PositionColoredNormalVertex
	{
		public Vector3 Position;
		public int Color;
		public Vector3 Normal;

		public PositionColoredNormalVertex(Vector3 position, int color, Vector3 normal)
		{
			Position = position;
			Color = color;
			Normal = normal;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct PositionNormalVertex
	{
		public Vector3 Position;
		public Vector3 Normal;

		public PositionNormalVertex(Vector3 position, Vector3 normal)
		{
			Position = position;
			Normal = normal;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct PositionNormalTexturedVertex
	{
		public Vector3 Position;
		public Vector3 Normal;
		public Vector2 TextureCoordinates;

		public PositionNormalTexturedVertex(Vector3 position, Vector3 normal, Vector2 textureCoordinates)
		{
			Position = position;
			Normal = normal;
			TextureCoordinates = textureCoordinates;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct PositionColorTexturedVertex
	{
		public Vector3 Position;
		public Color3 Color;
		public Vector2 TextureCoordinates;

		public PositionColorTexturedVertex(Vector3 position, Color3 color, Vector2 textureCoordinates)
		{
			Position = position;
			Color = color;
			TextureCoordinates = textureCoordinates;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct Position2Color2TexturedVertex
	{
		public Vector3 Position0;
		public Vector3 Position1;
		public Color3 Color0;
		public Color3 Color1;
		public Vector2 TextureCoordinates;

		public Position2Color2TexturedVertex(Vector3 position0, Vector3 position1, Color3 color0, Color3 color1, Vector2 textureCoordinates)
		{
			Position0 = position0;
			Position1 = position1;
			Color0 = color0;
			Color1 = color1;
			TextureCoordinates = textureCoordinates;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct PositionTexturedVertex
	{
		public Vector3 Position;
		public Vector2 TextureCoordinates;


		public PositionTexturedVertex(Vector3 position, Vector2 textureCoordinates)
		{
			Position = position;
			TextureCoordinates = textureCoordinates;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct PositionTextured3DVertex
	{
		public Vector3 Position;
		public Vector3 TextureCoordinates;


		public PositionTextured3DVertex(Vector3 position, Vector3 textureCoordinates)
		{
			Position = position;
			TextureCoordinates = textureCoordinates;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct SpriteVertex
	{
		public Vector2 TexCoord;
		public Vector2 TexCoordSize;
		public Vector2 Position;
		public Vector2 Size;
		public int Color;

		public SpriteVertex(Vector2 textureCoordinates, Vector2 textureCoordinatesSize, Vector2 position, Vector2 size, int color)
		{
			TexCoord = textureCoordinates;
			TexCoordSize = textureCoordinatesSize;
			Position = position;
			Size = size;
			Color = color;
		}
	}
}
