using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using SlimDX.Direct3D11;
using SlimDX;
using Buffer = SlimDX.Direct3D11.Buffer;
using DeviceManager = ModelEx.DeviceManager;
using EffectSprite = ModelEx.EffectSprite;
using SpriteVertex = ModelEx.SpriteVertex;
using ShaderManager = ModelEx.ShaderManager;

namespace SpriteTextRenderer
{
	public enum CoordinateType
	{
		UNorm,
		SNorm,
		Relative,
		Absolute
	}

	public class SpriteSegment
	{
		public ShaderResourceView Texture;
		public List<SpriteVertex> Sprites = new List<SpriteVertex>();
	}

	public class SpriteRenderer : IDisposable
	{
		protected Buffer vertexBuffer;

		protected int vertexStride = 0;
		protected int maxVertices;

		protected EffectSprite effect = ShaderManager.Instance.effectSprite;

		private Viewport viewport;

		public bool AllowReorder { get; set; }

		public Vector2 ScreenSize { get; set; }

		private List<SpriteSegment> sprites = new List<SpriteSegment>();
		private Dictionary<ShaderResourceView, List<SpriteSegment>> textureSprites = new Dictionary<ShaderResourceView, List<SpriteSegment>>();

		private int spriteCount = 0;

		public SpriteRenderer(int maxVertices = 128)
		{
			this.maxVertices = maxVertices;

			AllowReorder = true;

			Initialize();

			RefreshViewport();
		}

		private void Initialize()
		{
			vertexStride = Marshal.SizeOf(typeof(SpriteVertex));
			int SizeOfVertexBufferInBytes = maxVertices * vertexStride;

			vertexBuffer = new Buffer(
				DeviceManager.Instance.device,
				SizeOfVertexBufferInBytes,
				ResourceUsage.Dynamic,
				BindFlags.VertexBuffer,
				CpuAccessFlags.Write, ResourceOptionFlags.None,
				vertexStride);

			vertexBuffer.DebugName = "Sprites Vertexbuffer";
		}

		public void RefreshViewport()
		{
			viewport = DeviceManager.Instance.context.Rasterizer.GetViewports()[0];
		}

		public void ClearReorderBuffer()
		{
			textureSprites.Clear();
		}

		private Vector2 ConvertCoordinate(Vector2 coordinate, CoordinateType coordinateType)
		{
			switch (coordinateType)
			{
				case CoordinateType.SNorm:
					return coordinate;
				case CoordinateType.UNorm:
					coordinate.X = (coordinate.X - 0.5f) * 2;
					coordinate.Y = -(coordinate.Y - 0.5f) * 2;
					return coordinate;
				case CoordinateType.Relative:
					coordinate.X = coordinate.X / ScreenSize.X * 2 - 1;
					coordinate.Y = -(coordinate.Y / ScreenSize.Y * 2 - 1);
					return coordinate;
				case SpriteTextRenderer.CoordinateType.Absolute:
					coordinate.X = coordinate.X / viewport.Width * 2 - 1;
					coordinate.Y = -(coordinate.Y / viewport.Height * 2 - 1);
					return coordinate;
			}
			return Vector2.Zero;
		}

		public void Draw(ShaderResourceView texture, Vector2 position, Vector2 size, CoordinateType coordinateType)
		{
			Draw(texture, position, size, new Color4(1, 1, 1, 1), coordinateType);
		}

		public void Draw(ShaderResourceView texture, Vector2 position, Vector2 size, Color4 color, CoordinateType coordinateType)
		{
			Draw(texture, position, size, Vector2.Zero, new Vector2(1, 1), color, coordinateType);
		}

		public void Draw(ShaderResourceView texture, Vector2 position, Vector2 size, Vector2 texCoords, Vector2 texCoordsSize, Color4 color, CoordinateType coordinateType)
		{
			if (texture == null)
				return;
			var data = new SpriteVertex();
			data.Position = ConvertCoordinate(position, coordinateType);
			data.Size = ConvertCoordinate(position + size, coordinateType) - data.Position;
			data.Size.X = Math.Abs(data.Size.X);
			data.Size.Y = Math.Abs(data.Size.Y);
			data.TexCoord = texCoords;
			data.TexCoordSize = texCoordsSize;
			data.Color = color.ToArgb();

			if (AllowReorder)
			{
				//Is there already a sprite for this texture?
				if (textureSprites.ContainsKey(texture))
				{
					//Add the sprite to the last segment for this texture
					var Segment = textureSprites[texture].Last();
					AddIn(Segment, data);
				}
				else
					//Add a new segment for this texture
					AddNew(texture, data);
			}
			else
				//Add a new segment for this texture
				AddNew(texture, data);
		}

		private void AddNew(ShaderResourceView texture, SpriteVertex data)
		{
			//Create new segment with initial values
			var newSegment = new SpriteSegment();
			newSegment.Texture = texture;
			newSegment.Sprites.Add(data);
			sprites.Add(newSegment);

			//Create reference for segment in dictionary
			if (!textureSprites.ContainsKey(texture))
				textureSprites.Add(texture, new List<SpriteSegment>());

			textureSprites[texture].Add(newSegment);
			CheckForFullBuffer();
		}

		private void CheckForFullBuffer()
		{
			spriteCount++;
			if (spriteCount >= maxVertices)
				Flush();
		}

		private void AddIn(SpriteSegment segment, SpriteVertex data)
		{
			segment.Sprites.Add(data);
			CheckForFullBuffer();
		}

		public void Flush()
		{
			if (spriteCount == 0)
				return;

			System.Threading.Monitor.Enter(DeviceManager.Instance.device);

			//Construct vertexbuffer
			var data = DeviceManager.Instance.context.MapSubresource(vertexBuffer, MapMode.WriteDiscard, MapFlags.None);
			foreach (var segment in sprites)
			{
				var vertices = segment.Sprites.ToArray();
				data.Data.WriteRange(vertices);
			}
			DeviceManager.Instance.context.UnmapSubresource(vertexBuffer, 0);


			//Initialize render calls

			DeviceManager.Instance.context.InputAssembler.InputLayout = effect.layout;
			DeviceManager.Instance.context.InputAssembler.PrimitiveTopology = PrimitiveTopology.PointList;
			DeviceManager.Instance.context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(vertexBuffer, vertexStride, 0));

			//Draw
			int offset = 0;
			foreach (var segment in sprites)
			{
				int count = segment.Sprites.Count;
				effect.Texture = segment.Texture;
				effect.Apply(0);
				DeviceManager.Instance.context.Draw(count, offset);
				offset += count;
			}

			System.Threading.Monitor.Exit(DeviceManager.Instance.device);

			//System.Diagnostics.Debug.Print(SpriteCount + " Sprites gezeichnet.");

			//Reset buffers
			spriteCount = 0;
			sprites.Clear();
			textureSprites.Clear();
		}

		#region IDisposable Support
		private bool disposed = false;
		private void Dispose(bool disposing)
		{
			if (!this.disposed)
			{
				if (disposing)
				{
					//There are no managed resources to dispose
				}

				vertexBuffer.Dispose();
			}
			this.disposed = true;
		}

		public void Dispose()
		{
			Dispose(true);

			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
