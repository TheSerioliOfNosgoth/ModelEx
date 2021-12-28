using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX.Direct3D11;
using SlimDX.D3DCompiler;
using SlimDX;
using Buffer = SlimDX.Direct3D11.Buffer;

namespace SpriteTextRenderer
{
	public enum CoordinateType
	{
		UNorm,
		SNorm,
		Relative,
		Absolute
	}

	public class SpriteRenderer : IDisposable
	{
		protected ModelEx.EffectSprite effect = ModelEx.ShaderManager.Instance.effectSprite;

		private Device device;
		public Device Device { get { return device; } }
		private DeviceContext context;

		private int bufferSize;
		private Viewport viewport;

		DepthStencilState dSState;
		BlendState blendState;

		public bool HandleDepthStencilState { get; set; }

		public bool HandleBlendState { get; set; }

		public bool AllowReorder { get; set; }

		public Vector2 ScreenSize { get; set; }

		private List<SpriteSegment> sprites = new List<SpriteSegment>();
		private Dictionary<ShaderResourceView, List<SpriteSegment>> textureSprites = new Dictionary<ShaderResourceView, List<SpriteSegment>>();

		private int spriteCount = 0;

		private Buffer vb;

		public SpriteRenderer(Device device, int bufferSize = 128)
		{
			this.device = device;
			this.context = device.ImmediateContext;
			this.bufferSize = bufferSize;

			AllowReorder = true;
			HandleDepthStencilState = true;
			HandleBlendState = true;

			Initialize();

			RefreshViewport();
		}

		private void Initialize()
		{
			vb = new Buffer(device, bufferSize * SpriteVertexLayout.Struct.SizeInBytes, ResourceUsage.Dynamic, BindFlags.VertexBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, SpriteVertexLayout.Struct.SizeInBytes);
			vb.DebugName = "Sprites Vertexbuffer";

			var dssd = new DepthStencilStateDescription()
			{
				IsDepthEnabled = false,
				DepthWriteMask = DepthWriteMask.Zero
			};
			dSState = DepthStencilState.FromDescription(Device, dssd);

			var blendDesc = new BlendStateDescription();
			blendDesc.AlphaToCoverageEnable = false;
			blendDesc.IndependentBlendEnable = false;
			blendDesc.RenderTargets[0].BlendOperation = BlendOperation.Add;
			blendDesc.RenderTargets[0].DestinationBlend = BlendOption.InverseSourceAlpha;
			blendDesc.RenderTargets[0].SourceBlend = BlendOption.SourceAlpha;
			blendDesc.RenderTargets[0].BlendEnable = true;
			blendDesc.RenderTargets[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;
			blendDesc.RenderTargets[0].BlendOperationAlpha = BlendOperation.Add;
			blendDesc.RenderTargets[0].SourceBlendAlpha = BlendOption.SourceAlpha;
			blendDesc.RenderTargets[0].DestinationBlendAlpha = BlendOption.InverseSourceAlpha;
			blendState = BlendState.FromDescription(device, blendDesc);
		}

		public void RefreshViewport()
		{
			viewport = device.ImmediateContext.Rasterizer.GetViewports()[0];
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
			var data = new SpriteVertexLayout.Struct();
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

		private void AddNew(ShaderResourceView texture, SpriteVertexLayout.Struct data)
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
			if (spriteCount >= bufferSize)
				Flush();
		}

		private void AddIn(SpriteSegment segment, SpriteVertexLayout.Struct data)
		{
			segment.Sprites.Add(data);
			CheckForFullBuffer();
		}

		public void Flush()
		{
			if (spriteCount == 0)
				return;

			System.Threading.Monitor.Enter(device);
			//Update DepthStencilState if necessary
			DepthStencilState oldDSState = null;
			BlendState oldBlendState = null;
			if (HandleDepthStencilState)
			{
				oldDSState = Device.ImmediateContext.OutputMerger.DepthStencilState;
				Device.ImmediateContext.OutputMerger.DepthStencilState = dSState;
			}
			if (HandleBlendState)
			{
				oldBlendState = Device.ImmediateContext.OutputMerger.BlendState;
				Device.ImmediateContext.OutputMerger.BlendState = blendState;
			}

			//Construct vertexbuffer
			var data = context.MapSubresource(vb, MapMode.WriteDiscard, MapFlags.None);
			foreach (var segment in sprites)
			{
				var vertices = segment.Sprites.ToArray();
				data.Data.WriteRange(vertices);
			}
			context.UnmapSubresource(vb, 0);


			//Initialize render calls

			device.ImmediateContext.InputAssembler.InputLayout = effect.layout;
			device.ImmediateContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.PointList;
			device.ImmediateContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(vb, SpriteVertexLayout.Struct.SizeInBytes, 0));

			//Draw
			int offset = 0;
			foreach (var segment in sprites)
			{
				int count = segment.Sprites.Count;
				effect.Texture = segment.Texture;
				effect.Apply(0);
				device.ImmediateContext.Draw(count, offset);
				offset += count;
			}

			if (HandleDepthStencilState)
			{
				Device.ImmediateContext.OutputMerger.DepthStencilState = oldDSState;
			}
			if (HandleBlendState)
			{
				Device.ImmediateContext.OutputMerger.BlendState = oldBlendState;
			}

			System.Threading.Monitor.Exit(device);

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

				dSState.Dispose();
				blendState.Dispose();

				vb.Dispose();
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
