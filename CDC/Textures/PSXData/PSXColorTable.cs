using System.Drawing;

namespace BenLincoln.TheLostWorlds.CDTextures
{
	public class PSXColorTable
	{
		public readonly ushort clut;
		protected int X;
		protected int Y;
		public Color[] colors;

		public PSXColorTable(ushort clut)
		{
			this.clut = clut;
		}

		public PSXColorTable(int numColors)
		{
			// 16 color tables will be padded to 256.
			colors = new Color[256];
			for (int i = 0; i < numColors; i++)
			{
				int luma = ((i * 256) / numColors) & 0xFF;
				colors[i] = Color.FromArgb(luma, luma, luma);
			}
		}

		public void Initialize(ushort[,] textureData, int xShift)
		{
			int width = textureData.GetUpperBound(1) + 1;
			int height = textureData.GetUpperBound(0) + 1;

			X = (clut & 0x3F) << 4; // 0x003F
			Y = clut >> 6; // 0xFFC0 >> 6 = 0x03FF
			X %= 512;
			X += width - xShift;
			X %= width;
			colors = new Color[256];

			for (int x = 0; x < 256; x++)
			{
				ushort val = 0;
				int wrappedWidth = (X + x) % width;
				if (Y < height)
				{
					val = textureData[Y, wrappedWidth];
				}

				ushort alpha = (ushort)(val >> 15);
				ushort blue = (ushort)(((ushort)(val << 1) >> 11) << 3);
				ushort green = (ushort)(((ushort)(val << 6) >> 11) << 3);
				ushort red = (ushort)(((ushort)(val << 11) >> 11) << 3);

				alpha &= 0xFF;
				blue &= 0xFF;
				green &= 0xFF;
				red &= 0xFF;

				if (alpha != 0 || blue != 0 || green != 0 || red != 0)
				{
					alpha = 255;
				}
				else
				{
					alpha = 0;
				}

				colors[x] = Color.FromArgb(alpha, red, green, blue);
			}
		}
	}
}
