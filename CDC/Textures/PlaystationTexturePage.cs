using System.Collections.Generic;
using System.Drawing;

namespace BenLincoln.TheLostWorlds.CDTextures
{
	public class PlaystationTexturePage
	{
		public readonly ushort tPage;
		protected int tp;
		protected int abr;
		protected int X;
		protected int Y;
		protected List<PlaystationColorTable> colorTables = new List<PlaystationColorTable>();
		protected Color[] greyPalette16 = new Color[16];
		protected Color[] greyPalette256 = new Color[256];
		public ushort[,] pixels;

		public PlaystationTexturePage(ushort tPage)
		{
			this.tPage = tPage;
			this.tp = 0; //(tPage >> 7) & 0x03,//(tPage >> 7) & 0x003;
			this.abr = 0;

			for (int i = 0; i < greyPalette16.Length; i++)
			{
				int luma = (i * 256) / greyPalette16.Length;
				greyPalette16[i] = Color.FromArgb(luma, luma, luma);
			}

			for (int i = 0; i < greyPalette256.Length; i++)
			{
				int luma = (i * 256) / greyPalette256.Length;
				greyPalette256[i] = Color.FromArgb(luma, luma, luma);
			}
		}

		public void Initialize(ushort[,] textureData, int imageWidth, int imageHeight, int totalWidth)
		{
			// X = (((tPage & 0x07FF) - 8) % 8) / 2;
			X = ((tPage << 6) & 0x1c0) % totalWidth; // % 1024;
			// Y = 0;
			Y = (tPage << 4) & 0x100; // + ((tPage >> 2) & 0x200);
			pixels = new ushort[imageHeight, imageWidth];

			int width = textureData.GetUpperBound(1) + 1;
			int height = textureData.GetUpperBound(0) + 1;

			for (int y = 0; y < imageHeight; y++)
			{
				for (int x = 0; x < imageWidth;)
				{
					if (tp == 0) // 4 bit
					{
						ushort val = 0;
						if ((Y + y) < height && (X + (x / 4)) < width)
						{
							val = textureData[Y + y, X + (x / 4)];
						}

						pixels[y, x++] = (ushort)(val & 0x000F);
						pixels[y, x++] = (ushort)((val & 0x00F0) >> 4);
						pixels[y, x++] = (ushort)((val & 0x0F00) >> 8);
						pixels[y, x++] = (ushort)((val & 0xF000) >> 12);
					}
					else if (tp == 1) // 8 bit
					{
						ushort val = 0;
						if ((Y + y) < height && (X + (x / 2)) < width)
						{
							val = textureData[Y + y, X + (x / 2)];
						}

						pixels[y, x++] = (ushort)(val & 0x00FF);
						pixels[y, x++] = (ushort)((val & 0xFF00) >> 8);
					}
					else if (tp == 2) // 16 bit
					{
						ushort val = 0;
						if ((Y + y) < height && (X + x) < width)
						{
							val = textureData[Y + y, X + x];
						}

						pixels[y, x++] = val;
					}
				}
			}

			foreach (PlaystationColorTable colorTable in colorTables)
			{
				colorTable.Initialize(textureData, tp == 0 ? 16 : 256, totalWidth);
			}
		}

		public void AddColorTable(ushort clut)
		{
			ushort clutID = (ushort)colorTables.FindIndex(x => x.clut == clut);
			if (clutID == 0xFFFF)
			{
				PlaystationColorTable colorTable = new PlaystationColorTable(clut);
				colorTables.Add(colorTable);
			}
		}

		public Color[] GetPallete(ushort clut)
		{
			PlaystationColorTable colorTable = colorTables.Find(x => x.clut == clut);
			if (colorTable != null)
			{
				return colorTable.colours;
			}

			// The alternative is to crash.
			return GetGreyscalePallete();
		}

		public Color[] GetGreyscalePallete()
		{
			return tp == 0 ? greyPalette16 : greyPalette256;
		}
	}
}
