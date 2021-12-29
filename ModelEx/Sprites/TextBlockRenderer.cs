using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX.DirectWrite;
using SlimDX.Direct2D;
using SlimDX.DXGI;
using System.Globalization;
using System.Drawing;
using SlimDX.Direct3D10;
using SlimDX;
using System.IO;

namespace SpriteTextRenderer
{
	/// <summary>
	/// Defines how a text is aligned in a rectangle. Use OR-combinations of vertical and horizontal alignment.
	/// </summary>
	/// <example>
	/// This example aligns the textblock on the top edge of the rectangle horizontally centered:
	/// <code lang="cs">var textAlignment = TextAlignment.Top | TextAlignment.HorizontalCenter</code>
	/// <code lang="vb">Dim textAlignment = TextAlignment.Top Or TextAlignment.HorizontalCenter</code>
	/// </example>
	[Flags]
	public enum TextAlignment
	{
		/// <summary>
		/// The top edge of the text is aligned at the top edge of the rectangle.
		/// </summary>
		Top = 1,
		/// <summary>
		/// The vertical center of the text is aligned at the vertical center of the rectangle.
		/// </summary>
		VerticalCenter = 2,
		/// <summary>
		/// The bottom edge of the text is aligned at the bottom edge of the rectangle.
		/// </summary>
		Bottom = 4,

		/// <summary>
		/// The left edge of the text is aligned at the left edge of the rectangle.
		/// </summary>
		Left = 8,
		/// <summary>
		/// The horizontal center of the text is aligned at the horizontal center of the rectangle. Each line is aligned independently.
		/// </summary>
		HorizontalCenter = 16,
		/// <summary>
		/// The right edge of the text is aligned at the right edge of the rectangle. Each line is aligned independently.
		/// </summary>
		Right = 32
	}

	/// <summary>
	/// This class is responsible for rendering arbitrary text. Every TextRenderer is specialized for a specific font and relies on
	/// a SpriteRenderer for rendering the text.
	/// </summary>
	public class TextBlockRenderer : IDisposable
	{
		private static int ReferenceCount;
		private static SlimDX.Direct3D10_1.Device1 D3DDevice10 = null;
		private SlimDX.Direct3D11.Device D3DDevice11 = null;
		private SpriteRenderer Sprite;

		private TextFormat Font;
		private static SlimDX.DirectWrite.Factory WriteFactory;
		private static SlimDX.Direct2D.Factory D2DFactory;
		private RenderTargetProperties rtp;

		private float _FontSize;

		/// <summary>
		/// Returns the font size that this TextRenderer was created for.
		/// </summary>
		public float FontSize { get { return _FontSize; } }

		/// <summary>
		/// Gets or sets whether this TextRenderer should behave PIX compatibly.
		/// </summary>
		/// <remarks>
		/// PIX compatibility means that no shared resource is used.
		/// However, this will result in no visible text being drawn. 
		/// The geometry itself will be visible in PIX.
		/// </remarks>
		public static bool PixCompatible { get; set; }

		static TextBlockRenderer()
		{
			PixCompatible = false;
		}

		/// <summary>
		/// Contains information about every char table that has been created.
		/// </summary>
		private Dictionary<byte, CharTableDescription> CharTables = new Dictionary<byte, CharTableDescription>();

		/// <summary>
		/// Creates a new text renderer for a specific font.
		/// </summary>
		/// <param name="sprite">The sprite renderer that is used for rendering</param>
		/// <param name="fontName">Name of font. The font has to be installed on the system. 
		/// If no font can be found, a default one is used.</param>
		/// <param name="fontSize">Size in which to prerender the text. FontSize should be equal to render size for best results.</param>
		/// <param name="fontStretch">Font stretch parameter</param>
		/// <param name="fontStyle">Font style parameter</param>
		/// <param name="fontWeight">Font weight parameter</param>
		public TextBlockRenderer(SpriteRenderer sprite, String fontName, SlimDX.DirectWrite.FontWeight fontWeight, SlimDX.DirectWrite.FontStyle fontStyle, FontStretch fontStretch, float fontSize)
		{
			AssertDevice();
			ReferenceCount++;
			this.Sprite = sprite;
			this._FontSize = fontSize;
			D3DDevice11 = ModelEx.DeviceManager.Instance.device;
			System.Threading.Monitor.Enter(D3DDevice11);
			rtp = new RenderTargetProperties()
			{
				HorizontalDpi = 96,
				VerticalDpi = 96,
				Type = RenderTargetType.Default,
				PixelFormat = new PixelFormat(Format.R8G8B8A8_UNorm, AlphaMode.Premultiplied),
				MinimumFeatureLevel = FeatureLevel.Direct3D10
			};

			Font = WriteFactory.CreateTextFormat(fontName, fontWeight, fontStyle, fontStretch, fontSize, CultureInfo.CurrentCulture.Name);
			System.Threading.Monitor.Exit(D3DDevice11);
			CreateCharTable(0);
		}

		/// <summary>
		/// Creates the texture and necessary structures for 256 chars whose unicode number starts with the given byte.
		/// The table containing ASCII has a prefix of 0 (0x00/00 - 0x00/FF).
		/// </summary>
		/// <param name="bytePrefix">The byte prefix of characters.</param>
		private void CreateCharTable(byte bytePrefix)
		{
			var TableDesc = new CharTableDescription();

			//Get appropriate texture size
			int sizeX = (int)(Font.FontSize * 12);
			sizeX = (int)Math.Pow(2, Math.Ceiling(Math.Log(sizeX, 2)));
			//Try how many lines are needed:
			var tl = new TextLayout[256];
			int line = 0, xPos = 0, yPos = 0;
			for (int i = 0; i < 256; ++i)
			{
				tl[i] = new TextLayout(WriteFactory, Convert.ToChar(i + (bytePrefix << 8)).ToString(), Font);
				int charWidth = 2 + (int)Math.Ceiling(tl[i].Metrics.LayoutWidth + tl[i].OverhangMetrics.Left + tl[i].OverhangMetrics.Right);
				int charHeight = 2 + (int)Math.Ceiling(tl[i].Metrics.LayoutHeight + tl[i].OverhangMetrics.Top + tl[i].OverhangMetrics.Bottom);
				line = Math.Max(line, charHeight);
				if (xPos + charWidth >= sizeX)
				{
					xPos = 0;
					yPos += line;
					line = 0;
				}
				xPos += charWidth;
			}
			int sizeY = (int)(line + yPos);
			sizeY = (int)Math.Pow(2, Math.Ceiling(Math.Log(sizeY, 2)));

			//Create Texture
			var TexDesc = new Texture2DDescription()
			{
				ArraySize = 1,
				BindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget,
				CpuAccessFlags = CpuAccessFlags.None,
				Format = Format.R8G8B8A8_UNorm,
				Height = sizeY,
				Width = sizeX,
				MipLevels = 1,
				OptionFlags = ResourceOptionFlags.Shared,
				SampleDescription = new SampleDescription(1, 0),
				Usage = ResourceUsage.Default
			};
			var texture = new Texture2D(D3DDevice10, TexDesc);

			var rtv = new RenderTargetView(D3DDevice10, texture);
			D3DDevice10.ClearRenderTargetView(rtv, new SlimDX.Color4(0, 1, 1, 1));
			//D3DDevice10.ClearRenderTargetView(rtv, new SlimDX.Color4(1, 0, 0, 0));
			Surface surface = texture.AsSurface();
			var target = RenderTarget.FromDXGI(D2DFactory, surface, rtp);
			var color = new SolidColorBrush(target, new SlimDX.Color4(1, 1, 1, 1));

			target.BeginDraw();
			line = 0; xPos = 0; yPos = 0;
			//for (int i = 0; i < 256; ++i)
			for (int i = 0; i < 256; ++i)
			{
				//1 additional pixel on each side
				int charWidth = 2 + (int)Math.Ceiling(tl[i].Metrics.LayoutWidth + tl[i].OverhangMetrics.Left + tl[i].OverhangMetrics.Right);
				int charHeight = 2 + (int)Math.Ceiling(tl[i].Metrics.LayoutHeight + tl[i].OverhangMetrics.Top + tl[i].OverhangMetrics.Bottom);
				line = Math.Max(line, charHeight);
				if (xPos + charWidth >= sizeX)
				{
					xPos = 0;
					yPos += line;
					line = 0;
				}
				var charDesc = new CharDescription();

				charDesc.CharSize = new Vector2(tl[i].Metrics.WidthIncludingTrailingWhitespace, tl[i].Metrics.Height);
				charDesc.OverhangLeft = tl[i].OverhangMetrics.Left + 1;
				charDesc.OverhangTop = tl[i].OverhangMetrics.Top + 1;
				//Make XPos + CD.Overhang.Left an integer number in order to draw at integer positions
				charDesc.OverhangLeft += (float)Math.Ceiling(xPos + charDesc.OverhangLeft) - (xPos + charDesc.OverhangLeft);
				//Make YPos + CD.Overhang.Top an integer number in order to draw at integer positions
				charDesc.OverhangTop += (float)Math.Ceiling(yPos + charDesc.OverhangTop) - (yPos + charDesc.OverhangTop);

				charDesc.OverhangRight = charWidth - charDesc.CharSize.X - charDesc.OverhangLeft;
				charDesc.OverhangBottom = charHeight - charDesc.CharSize.Y - charDesc.OverhangTop;

				charDesc.TexCoordsStart = new Vector2(((float)xPos / sizeX), ((float)yPos / sizeY));
				charDesc.TexCoordsSize = new Vector2((float)charWidth / sizeX, (float)charHeight / sizeY);

				charDesc.TableDescription = TableDesc;

				TableDesc.Chars[i] = charDesc;

				target.DrawTextLayout(new PointF(xPos + charDesc.OverhangLeft, yPos + charDesc.OverhangTop), tl[i], color);
				xPos += charWidth;
				tl[i].Dispose();
			}
			target.EndDraw();

			color.Dispose();

			//This is a workaround for Windows 8.1 machines. 
			//If these lines would not be present, the shared resource would be empty.
			//TODO: find a nicer solution
			using (var ms = new MemoryStream())
				Texture2D.ToStream(texture, ImageFileFormat.Bmp, ms);

			System.Threading.Monitor.Enter(D3DDevice11);
			var dxgiResource = new SlimDX.DXGI.Resource(texture);
			SlimDX.Direct3D11.Texture2D Texture11;
			if (PixCompatible)
			{
				Texture11 = new SlimDX.Direct3D11.Texture2D(D3DDevice11, new SlimDX.Direct3D11.Texture2DDescription()
				{
					ArraySize = 1,
					BindFlags = SlimDX.Direct3D11.BindFlags.ShaderResource | SlimDX.Direct3D11.BindFlags.RenderTarget,
					CpuAccessFlags = SlimDX.Direct3D11.CpuAccessFlags.None,
					Format = Format.R8G8B8A8_UNorm,
					Height = sizeY,
					Width = sizeX,
					MipLevels = 1,
					OptionFlags = SlimDX.Direct3D11.ResourceOptionFlags.Shared,
					SampleDescription = new SampleDescription(1, 0),
					Usage = SlimDX.Direct3D11.ResourceUsage.Default
				});
			}
			else
			{
				Texture11 = D3DDevice11.OpenSharedResource<SlimDX.Direct3D11.Texture2D>(dxgiResource.SharedHandle);
			}
			var srv = new SlimDX.Direct3D11.ShaderResourceView(D3DDevice11, Texture11);
			TableDesc.Texture = Texture11;
			TableDesc.SRV = srv;
			rtv.Dispose();
			System.Threading.Monitor.Exit(D3DDevice11);

			System.Diagnostics.Debug.WriteLine("Created Char Table " + bytePrefix + " in " + sizeX + " x " + sizeY);

			//System.Threading.Monitor.Enter(D3DDevice11);
			//SlimDX.Direct3D11.Texture2D.SaveTextureToFile(Sprite.Device.ImmediateContext, Texture11, SlimDX.Direct3D11.ImageFileFormat.Png, Font.FontFamilyName + "Table" + BytePrefix + ".png");
			//System.Threading.Monitor.Exit(D3DDevice11);

			CharTables.Add(bytePrefix, TableDesc);

			dxgiResource.Dispose();
			target.Dispose();
			surface.Dispose();
			texture.Dispose();
		}

		/// <summary>
		/// Draws the string in the specified coordinate system.
		/// </summary>
		/// <param name="text">The text to draw</param>
		/// <param name="position">A position in the chosen coordinate system where the top left corner of the first character will be</param>
		/// <param name="realFontSize">The real font size in the chosen coordinate system</param>
		/// <param name="color">The color in which to draw the text</param>
		/// <param name="coordinateType">The chosen coordinate system</param>
		/// <returns>The StringMetrics for the rendered text</returns>
		public StringMetrics DrawString(string text, Vector2 position, float realFontSize, Color4 color, CoordinateType coordinateType)
		{
			StringMetrics sm;
			IterateStringEm(text, position, true, realFontSize, color, coordinateType, out sm);
			return sm;
		}

		/// <summary>
		/// Draws the string untransformed in absolute coordinate system.
		/// </summary>
		/// <param name="text">The text to draw</param>
		/// <param name="position">A position in absolute coordinates where the top left corner of the first character will be</param>
		/// <param name="color">The color in which to draw the text</param>
		/// <returns>The StringMetrics for the rendered text</returns>
		public StringMetrics DrawString(string text, Vector2 position, Color4 color)
		{
			return DrawString(text, position, FontSize, color, CoordinateType.Absolute);
		}

		/// <summary>
		/// Measures the untransformed string in absolute coordinate system.
		/// </summary>
		/// <param name="text">The text to measure</param>
		/// <returns>The StringMetrics for the text</returns>
		public StringMetrics MeasureString(string text)
		{
			StringMetrics sm;
			IterateString(text, Vector2.Zero, false, 1, new Color4(), CoordinateType.Absolute, out sm);
			return sm;
		}

		/// <summary>
		/// Measures the string in the specified coordinate system.
		/// </summary>
		/// <param name="text">The text to measure</param>
		/// <param name="realFontSize">The real font size in the chosen coordinate system</param>
		/// <param name="coordinateType">The chosen coordinate system</param>
		/// <returns>The StringMetrics for the text</returns>
		public StringMetrics MeasureString(string text, float realFontSize, CoordinateType coordinateType)
		{
			StringMetrics sm;
			IterateStringEm(text, Vector2.Zero, false, realFontSize, new Color4(), coordinateType, out sm);
			return sm;
		}

		/// <summary>
		/// Draws the string in the specified coordinate system aligned in the given rectangle. The text is not clipped or wrapped.
		/// </summary>
		/// <param name="text">The text to draw</param>
		/// <param name="rect">The rectangle in which to align the text</param>
		/// <param name="align">Alignment of text in rectangle</param>
		/// <param name="realFontSize">The real font size in the chosen coordinate system</param>
		/// <param name="color">The color in which to draw the text</param>
		/// <param name="coordinateType">The chosen coordinate system</param>
		/// <returns>The StringMetrics for the rendered text</returns>
		public StringMetrics DrawString(string text, RectangleF rect, TextAlignment align, float realFontSize, Color4 color, CoordinateType coordinateType)
		{
			//If text is aligned top and left, no adjustment has to be made
			if (align.HasFlag(TextAlignment.Top) && align.HasFlag(TextAlignment.Left))
			{
				return DrawString(text, new Vector2(rect.X, rect.Y), realFontSize, color, coordinateType);
			}

			text = text.Replace("\r", "");
			var rawTextMetrics = MeasureString(text, realFontSize, coordinateType);
			var mMetrics = MeasureString("m", realFontSize, coordinateType);
			float startY;
			if (align.HasFlag(TextAlignment.Top))
				startY = rect.Top;
			else if (align.HasFlag(TextAlignment.VerticalCenter))
				startY = rect.Top + rect.Height / 2 - rawTextMetrics.Size.Y / 2;
			else //Bottom
				startY = rect.Bottom - rawTextMetrics.Size.Y;

			var totalMetrics = new StringMetrics();

			//break text into lines
			var lines = text.Split('\n');

			foreach (var line in lines)
			{
				float startX;
				if (align.HasFlag(TextAlignment.Left))
					startX = rect.X;
				else
				{
					var lineMetrics = MeasureString(line, realFontSize, coordinateType);
					if (align.HasFlag(TextAlignment.HorizontalCenter))
						startX = rect.X + rect.Width / 2 - lineMetrics.Size.X / 2;
					else //Right
						startX = rect.Right - lineMetrics.Size.X;
				}

				var lineMetrics2 = DrawString(line, new Vector2(startX, startY), realFontSize, color, coordinateType);
				float lineHeight;
				if (mMetrics.Size.Y < 0)
					lineHeight = Math.Min(lineMetrics2.Size.Y, mMetrics.Size.Y);
				else
					lineHeight = Math.Max(lineMetrics2.Size.Y, mMetrics.Size.Y);
				startY += lineHeight;
				totalMetrics.Merge(lineMetrics2);
			}

			return totalMetrics;
		}

		/// <summary>
		/// Draws the string unscaled in absolute coordinate system aligned in the given rectangle. The text is not clipped or wrapped.
		/// </summary>
		/// <param name="text">Text to draw</param>
		/// <param name="rect">A position in absolute coordinates where the top left corner of the first character will be</param>
		/// <param name="align">Alignment in rectangle</param>
		/// <param name="color">Color in which to draw the text</param>
		/// <returns>The StringMetrics for the rendered text</returns>
		public StringMetrics DrawString(string text, RectangleF rect, TextAlignment align, Color4 color)
		{
			return DrawString(text, rect, align, FontSize, color, CoordinateType.Absolute);
		}

		private void IterateStringEm(string text, Vector2 position, bool Draw, float realFontSize, Color4 color, CoordinateType coordinateType, out StringMetrics metrics)
		{
			float scale = realFontSize / _FontSize;
			IterateString(text, position, Draw, scale, color, coordinateType, out metrics);
		}

		private void IterateString(string text, Vector2 position, bool draw, float scale, Color4 color, CoordinateType coordinateType, out StringMetrics metrics)
		{
			metrics = new StringMetrics();
			Vector2 startPosition = position;
			float scalY = coordinateType == SpriteTextRenderer.CoordinateType.SNorm ? -1 : 1;
			foreach (char c in text)
			{
				var charDesc = GetCharDescription(c);
				var charMetrics = charDesc.ToStringMetrics(position, scale, scale * scalY);
				if (draw)
				{
					if (charMetrics.FullRectSize.X != 0 && charMetrics.FullRectSize.Y != 0)
					{
						float posY = position.Y - scalY * charMetrics.OverhangTop;
						float posX = position.X - charMetrics.OverhangLeft;
						Sprite.Draw(charDesc.TableDescription.SRV, new Vector2(posX, posY), charMetrics.FullRectSize, charDesc.TexCoordsStart, charDesc.TexCoordsSize, color, coordinateType);
					}
				}

				metrics.Merge(charMetrics);

				position.X += charMetrics.Size.X;

				//Break newlines
				if (c == '\r')
					position.X = metrics.TopLeft.X;

				if (c == '\n')
					position.Y = metrics.BottomRight.Y - charMetrics.Size.Y / 2;
			}
		}

		private CharDescription GetCharDescription(char c)
		{
			int unicode = (int)c;
			byte b = (byte)(c & 0x000000FF);
			byte bytePrefix = (byte)((c & 0x0000FF00) >> 8);
			if (!CharTables.ContainsKey(bytePrefix))
				CreateCharTable(bytePrefix);
			return CharTables[bytePrefix].Chars[b];
		}

		static void AssertDevice()
		{
			if (D3DDevice10 != null)
				return;
			D3DDevice10 = new SlimDX.Direct3D10_1.Device1(DeviceCreationFlags.BgraSupport, SlimDX.Direct3D10_1.FeatureLevel.Level_10_0);
			WriteFactory = new SlimDX.DirectWrite.Factory(SlimDX.DirectWrite.FactoryType.Shared);
			D2DFactory = new SlimDX.Direct2D.Factory(SlimDX.Direct2D.FactoryType.SingleThreaded);
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

				Font.Dispose();
				ReferenceCount--;
				foreach (var Table in CharTables)
				{
					Table.Value.SRV.Dispose();
					Table.Value.Texture.Dispose();
				}
				if (ReferenceCount <= 0)
				{
					D3DDevice10.Dispose();
					WriteFactory.Dispose();
					D2DFactory.Dispose();
				}
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
