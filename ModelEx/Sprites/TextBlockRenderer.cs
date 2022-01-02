using System;
using System.Collections.Generic;
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
	[Flags]
	public enum TextAlignment
	{
		Top = 1,
		VerticalCenter = 2,
		Bottom = 4,
		Left = 8,
		HorizontalCenter = 16,
		Right = 32
	}

	public class TextBlockRenderer
	{
		private SlimDX.Direct3D11.Device D3DDevice11 = null;
		private SpriteRenderer Sprite;

		private TextFormat Font;
		private RenderTargetProperties rtp;

		private float _FontSize;

		public float FontSize { get { return _FontSize; } }

		public static bool PixCompatible { get; set; }

		static TextBlockRenderer()
		{
			PixCompatible = false;
		}

		private Dictionary<byte, CharTableDescription> CharTables = new Dictionary<byte, CharTableDescription>();

		public TextBlockRenderer(SpriteRenderer sprite, String fontName, SlimDX.DirectWrite.FontWeight fontWeight, SlimDX.DirectWrite.FontStyle fontStyle, FontStretch fontStretch, float fontSize)
		{
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

			Font = ModelEx.TextManager.Instance.WriteFactory.CreateTextFormat(fontName, fontWeight, fontStyle, fontStretch, fontSize, CultureInfo.CurrentCulture.Name);
			System.Threading.Monitor.Exit(D3DDevice11);
			CreateCharTable(0);
		}

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
				tl[i] = new TextLayout(ModelEx.TextManager.Instance.WriteFactory, Convert.ToChar(i + (bytePrefix << 8)).ToString(), Font);
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
			var texture = new Texture2D(ModelEx.TextManager.Instance.D3DDevice10, TexDesc);

			var rtv = new RenderTargetView(ModelEx.TextManager.Instance.D3DDevice10, texture);
			ModelEx.TextManager.Instance.D3DDevice10.ClearRenderTargetView(rtv, new SlimDX.Color4(0, 1, 1, 1));
			//D3DDevice10.ClearRenderTargetView(rtv, new SlimDX.Color4(1, 0, 0, 0));
			Surface surface = texture.AsSurface();
			var target = RenderTarget.FromDXGI(ModelEx.TextManager.Instance.D2DFactory, surface, rtp);
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

		public StringMetrics DrawString(string text, Vector2 position, float realFontSize, Color4 color, CoordinateType coordinateType)
		{
			StringMetrics sm;
			IterateStringEm(text, position, true, realFontSize, color, coordinateType, out sm);
			return sm;
		}

		public StringMetrics DrawString(string text, Vector2 position, Color4 color)
		{
			return DrawString(text, position, FontSize, color, CoordinateType.Absolute);
		}

		public StringMetrics MeasureString(string text)
		{
			StringMetrics sm;
			IterateString(text, Vector2.Zero, false, 1, new Color4(), CoordinateType.Absolute, out sm);
			return sm;
		}

		public StringMetrics MeasureString(string text, float realFontSize, CoordinateType coordinateType)
		{
			StringMetrics sm;
			IterateStringEm(text, Vector2.Zero, false, realFontSize, new Color4(), coordinateType, out sm);
			return sm;
		}

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

		public void Dispose()
		{
			Font.Dispose();
			foreach (var Table in CharTables)
			{
				Table.Value.SRV.Dispose();
				Table.Value.Texture.Dispose();
			}
		}
	}
}
