using System.Collections.Generic;
using System.Drawing;

namespace BenLincoln.TheLostWorlds.CDTextures
{
	public class PSXTexturePage
	{
		public readonly ushort tPage;
		protected int tp;
		protected int abr;
		protected int X;
		protected int Y;
		protected List<PSXColorTable> colorTables = new List<PSXColorTable>();
		protected PSXColorTable greyscaleColorTable;
		protected PSXColorTable commonColorTable;
		protected Dictionary<ushort, int> clutRefs = new Dictionary<ushort, int>();
		protected PSXPixelList pixelList = new PSXPixelList();
		public ushort[,] pixels;

		public PSXTexturePage(ushort tPage)
		{
			this.tPage = tPage;
			this.tp = (tPage >> 7) & 0x0003;
			this.abr = (tPage >> 5) & 0x0003;

			greyscaleColorTable = new PSXColorTable((tp == 0) ? 16 : 256);
		}

		public void Initialize(ushort[,] textureData, int imageWidth, int imageHeight, int xShift, ushort mostCommonCLUT, bool alwaysUseGreyscaleForMissingPalettes)
		{
			int width = textureData.GetUpperBound(1) + 1;
			int height = textureData.GetUpperBound(0) + 1;

			X = (tPage << 6) & 0x07c0; // 0x001F << 6 = 0x7C0
			Y = (tPage << 4) & 0x0100 + ((tPage >> 2) & 0x0200); // 0x0010 << 4 == 0x0100, 0x0800 >> 2 = 0x0200
			X %= 512;
			X += width - xShift;
			X %= width;
			pixels = new ushort[imageHeight, imageWidth];

			for (int y = 0; y < imageHeight; y++)
			{
				for (int x = 0; x < imageWidth;)
				{
					if (tp == 0) // 4 bit
					{
						ushort val = 0;
						int wrappedWidth = (X + (x / 4)) % width;
						if ((Y + y) < height)
						{
							val = textureData[Y + y, wrappedWidth];
						}

						pixels[y, x++] = (ushort)(val & 0x000F);
						pixels[y, x++] = (ushort)((val & 0x00F0) >> 4);
						pixels[y, x++] = (ushort)((val & 0x0F00) >> 8);
						pixels[y, x++] = (ushort)((val & 0xF000) >> 12);
					}
					else if (tp == 1) // 8 bit
					{
						ushort val = 0;
						int wrappedWidth = (X + (x / 2)) % width;
						if ((Y + y) < height)
						{
							val = textureData[Y + y, wrappedWidth];
						}

						pixels[y, x++] = (ushort)(val & 0x00FF);
						pixels[y, x++] = (ushort)((val & 0xFF00) >> 8);
					}
					else if (tp == 2) // 16 bit
					{
						ushort val = 0;
						int wrappedWidth = (X + x) % width;
						if ((Y + y) < height)
						{
							val = textureData[Y + y, wrappedWidth];
						}

						pixels[y, x++] = val;
					}
				}
			}

			foreach (PSXColorTable colorTable in colorTables)
			{
				colorTable.Initialize(textureData, xShift);
			}

			if (alwaysUseGreyscaleForMissingPalettes)
			{
				commonColorTable = greyscaleColorTable;
			}
			else
			{
				ushort commonCLUT = 0;
				foreach (KeyValuePair<ushort, int> clutRef in clutRefs)
				{
					if (clutRef.Value > commonCLUT)
					{
						commonCLUT = clutRef.Key;
					}
				}

				if (commonCLUT == 0)
				{
					commonCLUT = mostCommonCLUT;
				}

				commonColorTable = new PSXColorTable(commonCLUT);
				commonColorTable.Initialize(textureData, xShift);
			}
		}

		public void AddColorTable(ushort clut)
		{
			ushort clutID = (ushort)colorTables.FindIndex(x => x.clut == clut);
			if (clutID == 0xFFFF)
			{
				PSXColorTable colorTable = new PSXColorTable(clut);
				colorTables.Add(colorTable);
			}

			if (clutRefs.ContainsKey(clut))
			{
				clutRefs[clut]++;
			}
			else
			{
				clutRefs[clut] = 1;
			}
		}

		public IReadOnlyCollection<PSXColorTable> GetColorTables()
		{
			return colorTables;
		}

		public Color[] GetPallete(ushort clut)
		{
			PSXColorTable colorTable = colorTables.Find(x => x.clut == clut);
			if (colorTable != null)
			{
				return colorTable.colors;
			}

			// The alternative is to crash.
			return GetGreyscalePallete();
		}

		public Color[] GetGreyscalePallete()
		{
			return greyscaleColorTable?.colors;
		}

		public Color[] GetCommonPalette()
		{
			return commonColorTable?.colors;
		}

		public PSXPixelList GetPixelList()
		{
			return pixelList;
		}
	}
}
