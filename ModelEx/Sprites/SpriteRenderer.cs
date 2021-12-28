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
    /// <summary>
    /// Specifies, how coordinates are interpreted.
    /// </summary>
    /// <remarks>
    /// <para>Sprites (and with that text) can be drawn in several coordinate systems. The user can choose, which system
    /// fits his needs best. There are basically two types of coordinate system:</para>
    /// <para><b>Type 1 systems</b><br/>
    /// <img src="../Coordinate1.jpg" alt="Type 1 coordinate system"/><br/>
    /// The origin of T1 systems is located at the top left corner of the screen. The x-axis points to the right,
    /// the y-axis points downwards. All T1 systems differ in the axes' scaling. <see cref="CoordinateType.UNorm"/>
    /// uses unsigned normalized coordinates. <see cref="CoordinateType.Absolute"/> uses the screen's pixel coordinates.
    /// Therefore, the SpriteRenderer needs the D3DDevice's viewport. For performance reasons the viewport will not be
    /// queried repeatedly, but only once at the construction of the <see cref="SpriteRenderer"/> or on a call to 
    /// <see cref="SpriteRenderer.RefreshViewport"/>. <see cref="CoordinateType.Relative"/> uses a T1 coordinate 
    /// system of custom size.
    /// </para>
    /// <para><b>Type 2 systems</b><br/>
    /// <img src="../Coordinate2.jpg" alt="Type 2 coordinate system"/><br/>
    /// The origin of T2 systems is at the screen center. The x-axis points to the right, the y-axis points upwards.
    /// I.e. this coordinate system uses a flipped y-axis. Because the bottom coordinate is calculated with Top + Size,
    /// T2 coordinates usually have negative vertical sizes. <see cref="CoordinateType.SNorm"/> uses signed normalized
    /// coordinates.
    /// </para>
    /// 
    /// </remarks>
    public enum CoordinateType
    {
        /// <summary>
        /// Coordinates are in the range from 0 to 1. (0, 0) is the top left corner; (1, 1) is the bottom right corner.
        /// </summary>
        UNorm,
        /// <summary>
        /// Coordinates are in the range from -1 to 1. (-1, -1) is the bottom left corner; (1, 1) is the top right corner. This is the DirectX standard interpretation.
        /// </summary>
        SNorm,
        /// <summary>
        /// Coordinates are in the range of the relative screen size. (0, 0) is the top left corner; (ScreenSize.X, ScreenSize.Y) is the bottom right corner. A variable screen size is used. Use <see cref="SpriteRenderer.ScreenSize"/>.
        /// </summary>
        Relative,
        /// <summary>
        /// Coordinates are in the range of the actual screen size. (0, 0) is the top left corner; (Viewport.Width, Viewport.Height) is the bottom right corner. Use <see cref="SpriteRenderer.RefreshViewport"/> for updates to the used viewport.
        /// </summary>
        Absolute
    }

    /// <summary>
    /// This class is responsible for rendering 2D sprites. Typically, only one instance of this class is necessary.
    /// </summary>
    public class SpriteRenderer : IDisposable
    {
        private Device device;
        /// <summary>
        /// Returns the Direct3D device that this SpriteRenderer was created for.
        /// </summary>
        public Device Device { get { return device; } }
        private DeviceContext context;

        private Effect Fx;
        private EffectPass pass;
        private InputLayout inputLayout;
        private EffectResourceVariable textureVariable;
        private int bufferSize;
        private Viewport viewport;

        DepthStencilState dSState;
        BlendState blendState;

        /// <summary>
        /// Gets or sets, if this SpriteRenderer handles DepthStencilState
        /// </summary>
        /// <remarks>
        /// <para>
        /// Sprites have to be drawn with depth test disabled. If HandleDepthStencilState is set to true, the
        /// SpriteRenderer sets the DepthStencilState to a predefined state before drawing and resets it to
        /// the previous state after that. Set this value to false, if you want to handle states yourself.
        /// </para>
        /// <para>
        /// The default value is true.
        /// </para>
        /// </remarks>
        public bool HandleDepthStencilState { get; set; }

        /// <summary>
        /// Gets or sets, if this SpriteRenderer handles BlendState
        /// </summary>
        /// <remarks>
        /// <para>
        /// Sprites have to be drawn with simple alpha blending. If HandleBlendState is set to true, the
        /// SpriteRenderer sets the BlendState to a predefined state before drawing and resets it to
        /// the previous state after that. Set this value to false, if you want to handle states yourself.
        /// </para>
        /// <para>
        /// The default value is true.
        /// </para>
        /// </remarks>
        public bool HandleBlendState { get; set; }

        /// <summary>
        /// Set to true, if the order of draw calls can be rearranged for better performance.
        /// </summary>
        /// <remarks>
        /// Sprites are not drawn immediately, but only on a call to <see cref="SpriteRenderer.Flush"/>.
        /// Rendering performance can be improved, if the order of sprites can be changed, so that sprites
        /// with the same texture can be drawn with one draw call. However, this will not preserve the z-order.
        /// Use <see cref="SpriteRenderer.ClearReorderBuffer"/> to force a set of sprites to be drawn before another set.
        /// </remarks>
        /// <example>
        /// Consider the following pseudo code:
        /// <code>
        /// Draw left intense red circle
        /// Draw middle light red circle
        /// Draw right intense red circle
        /// </code>
        /// <para>With AllowReorder set to true, this will result in the following image:<br/>
        /// <img src="../Reorder1.jpg" alt=""/><br/>
        /// That is because the last circle is reordered to be drawn together with the first circle.
        /// </para>
        /// <para>With AllowReorder set to false, this will result in the following image:<br/>
        /// <img src="../Reorder2.jpg" alt=""/><br/>
        /// No optimization is applied. Performance may be slightly worse than with reordering.
        /// </para>
        /// </example>
        public bool AllowReorder { get; set; }

        /// <summary>
        /// When using relative coordinates, the screen size has to be set. Typically the screen size in pixels is used. However, other values are possible as well.
        /// </summary>
        public Vector2 ScreenSize { get; set; }

        /// <summary>
        /// A list of all sprites to draw. Sprites are drawn in the order in this list.
        /// </summary>
        private List<SpriteSegment> sprites = new List<SpriteSegment>();
        /// <summary>
        /// Allows direct access to the according SpriteSegments based on the texture
        /// </summary>
        private Dictionary<ShaderResourceView, List<SpriteSegment>> textureSprites = new Dictionary<ShaderResourceView,List<SpriteSegment>>();

        /// <summary>
        /// The number of currently buffered sprites
        /// </summary>
        private int spriteCount = 0;

        private Buffer vb;

        /// <summary>
        /// Create a new SpriteRenderer instance.
        /// </summary>
        /// <param name="device">Direct3D device, which will be used for rendering</param>
        /// <param name="bufferSize">The number of elements that can be stored in the sprite buffer.</param>
        /// <remarks>
        /// Sprites are not drawn immediately, but buffered instead. The buffer size defines, how much sprites can be buffered.
        /// If the buffer is full, according draw calls will be issued on the GPU clearing the buffer. Its size should be as big as
        /// possible without wasting empty space.
        /// </remarks>
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
            using (var code = ShaderBytecode.CompileFromFile("Shaders/Sprite.fx", "fx_5_0"))
            {
                Fx = new Effect(device, code);
            }

            pass = Fx.GetTechniqueByIndex(0).GetPassByIndex(0);
            inputLayout = new InputLayout(device, pass.Description.Signature, SpriteVertexLayout.Description);
            inputLayout.DebugName = "Input Layout for Sprites";

            textureVariable = Fx.GetVariableByName("Tex").AsResource();

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

        /// <summary>
        /// Updates the viewport used for absolute positioning. The first current viewport of the device's rasterizer will be used.
        /// </summary>
        public void RefreshViewport()
        {
            viewport = device.ImmediateContext.Rasterizer.GetViewports()[0];
        }

        /// <summary>
        /// Closes a reorder session. Further draw calls will not be drawn together with previous draw calls.
        /// </summary>
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

        /// <summary>
        /// Draws a complete texture on the screen.
        /// </summary>
        /// <param name="texture">The shader resource view of the texture to draw</param>
        /// <param name="position">Position of the top left corner of the texture in the chosen coordinate system</param>
        /// <param name="size">Size of the texture in the chosen coordinate system</param>
        /// <param name="coordinateType">A custom coordinate system in which to draw the texture</param>
        public void Draw(ShaderResourceView texture, Vector2 position, Vector2 size, CoordinateType coordinateType)
        {
            Draw(texture, position, size, new Color4(1, 1, 1, 1), coordinateType);
        }

        /// <summary>
        /// Draws a complete texture on the screen.
        /// </summary>
        /// <param name="texture">The shader resource view of the texture to draw</param>
        /// <param name="position">Position of the top left corner of the texture in the chosen coordinate system</param>
        /// <param name="size">Size of the texture in the chosen coordinate system</param>
        /// <param name="coordinateType">A custom coordinate system in which to draw the texture</param>
        /// <param name="color">The color with which to multiply the texture</param>
        public void Draw(ShaderResourceView texture, Vector2 position, Vector2 size, Color4 color, CoordinateType coordinateType)
        {
            Draw(texture, position, size, Vector2.Zero, new Vector2(1, 1), color, coordinateType);
        }

        /// <summary>
        /// Draws a region of a texture on the screen.
        /// </summary>
        /// <param name="texture">The shader resource view of the texture to draw</param>
        /// <param name="position">Position of the top left corner of the texture in the chosen coordinate system</param>
        /// <param name="size">Size of the texture in the chosen coordinate system</param>
        /// <param name="coordinateType">A custom coordinate system in which to draw the texture</param>
        /// <param name="color">The color with which to multiply the texture</param>
        /// <param name="texCoords">Texture coordinates for the top left corner</param>
        /// <param name="texCoordsSize">Size of the region in texture coordinates</param>
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

        /// <summary>
        /// If the buffer is full, then draw all sprites and clear it.
        /// </summary>
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

        /// <summary>
        /// This method causes the SpriteRenderer to immediately draw all buffered sprites.
        /// </summary>
        /// <remarks>
        /// This method should be called at the end of a frame in order to draw the last sprites that are in the buffer.
        /// </remarks>
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
 
            device.ImmediateContext.InputAssembler.InputLayout = inputLayout;
            device.ImmediateContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.PointList;
            device.ImmediateContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(vb, SpriteVertexLayout.Struct.SizeInBytes, 0));

            //Draw
            int offset = 0;
            foreach (var segment in sprites)
            {
                int count = segment.Sprites.Count;
                textureVariable.SetResource(segment.Texture);
                pass.Apply(context);
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

                Fx.Dispose();
                inputLayout.Dispose();
                dSState.Dispose();
                blendState.Dispose();

                vb.Dispose();
            }
            this.disposed = true;
        }

        /// <summary>
        /// Disposes of the SpriteRenderer.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
