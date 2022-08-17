using System.Drawing;

namespace BenLincoln.TheLostWorlds.CDTextures
{
	public class PlaystationColorTable
	{
		public readonly ushort clut;
		protected int X;
		protected int Y;
		public Color[] colours;

		public PlaystationColorTable(ushort clut)
		{
			this.clut = clut;
		}

		public void Initialize(ushort[,] textureData, int length, int totalWidth)
		{
			X = ((clut & 0x3F) << 4) % totalWidth; // % 1024;
			Y = clut >> 6;
			colours = new Color[length];

			int width = textureData.GetUpperBound(1);
			int height = textureData.GetUpperBound(0);

			if (Y > height)
			{
				return;
			}

			if (X > width)
			{
				return;
			}

			for (int x = 0; x < length; x++)
			{
				ushort val = textureData[Y, X + x];
				ushort alpha = (ushort)(val >> 15);
				ushort blue = (ushort)(((ushort)(val << 1) >> 11) << 3);
				ushort green = (ushort)(((ushort)(val << 6) >> 11) << 3);
				ushort red = (ushort)(((ushort)(val << 11) >> 11) << 3);

				if (alpha != 0 || blue != 0 || green != 0 || red != 0)
				{
					alpha = 255;
				}
				else
				{
					alpha = 0;
				}

				colours[x] = Color.FromArgb(alpha, red, green, blue);
			}
		}
	}
}
