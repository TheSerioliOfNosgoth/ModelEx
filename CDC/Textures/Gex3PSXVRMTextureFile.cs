using System;
using System.IO;

namespace BenLincoln.TheLostWorlds.CDTextures
{
	public class Gex3PSXVRMTextureFile : PSXTextureFile
	{
		public Gex3PSXVRMTextureFile(string path)
			: base(path)
		{
			_FileType = TextureFileType.Gex3Playstation;
			_FileTypeName = "Gex 3 (Playstation) VRM";
			_TotalWidth = 512;
			_TotalHeight = 512;
			LoadTextureData();
		}

		protected void LoadTextureData()
		{
			try
			{
				FileStream stream = new FileStream(_FilePath, FileMode.Open, FileAccess.Read);
				BinaryReader reader = new BinaryReader(stream);

				reader.BaseStream.Position = 20;

				_TextureData = new ushort[_TotalHeight, _TotalWidth];
				for (int y = 0; y < _TotalHeight; y++)
				{
					for (int x = 0; x < _TotalWidth; x++)
					{
						_TextureData[y, x] = reader.ReadUInt16();
					}
				}

				reader.Close();
				stream.Close();
			}
			catch (Exception ex)
			{
				throw new TextureFileException("Error reading texture.", ex);
			}
		}

		/*protected TexturePage GetTexturePage(ushort tPage)
		{

			TexturePage texturePage = new TexturePage
			{
				tPage = tPage,
				tp = (tPage >> 7) & 0x001,//(tPage >> 7) & 0x003,
				abr = 0,//(tPage >> 5) & 0x003,
				x = (tPage << 6) & 0x1c0,//(tPage << 6) & 0x7c0,
				y = (tPage << 4) & 0x100,//((tPage << 4) & 0x100) + ((tPage >> 2) & 0x200),
				pixels = null
			};

			ushort[,] pixels = new ushort[_ImageHeight, _ImageWidth];

			int scaledX = texturePage.x;
			int scaledY = texturePage.y;

			for (int y = 0; y < _ImageHeight; y++)
			{
				int yOffset = (scaledY + y) * _TotalWidth;
				for (int x = 0; x < _ImageWidth;)
				{
					int xOffset = scaledX; // / 2;

					try
					{
						if (texturePage.tp == 0) // 4 bit
						{
							xOffset += x / 4;
							ushort val = _TextureData[yOffset + xOffset];
							pixels[y, x++] = (ushort)(val & 0x000F);
							pixels[y, x++] = (ushort)((val & 0x00F0) >> 4);
							pixels[y, x++] = (ushort)((val & 0x0F00) >> 8);
							pixels[y, x++] = (ushort)((val & 0xF000) >> 12);
						}
						else if (texturePage.tp == 1) // 8 bit
						{
							xOffset += x;
							xOffset /= 2;
							ushort val = _TextureData[yOffset + xOffset];
							pixels[y, x++] = (ushort)(val & 0x00FF);
							pixels[y, x++] = (ushort)((val & 0xFF00) >> 8);
						}
						else if (texturePage.tp == 2) // 16 bit
						{
							xOffset += x;
							ushort val = _TextureData[yOffset + xOffset];
							pixels[y, x++] = val;
						}
					}
					catch (Exception ex)
					{
					}
				}
			}

			texturePage.pixels = pixels;
			return texturePage;
		}

		protected ColourTable GetTextureClut(ushort clut, int length)
		{
			ColourTable colourTable = new ColourTable
			{
				clut = clut,
				x = (clut & 0x1F) << 4,//(clut & 0x3F) << 4,
				y = (clut >> 6) & 0x1FF,
				colours = null
			};

			Color[] colours = new Color[length];

			int scaledX = colourTable.x;
			int scaledY = colourTable.y;

			int yOffset = scaledY * _TotalWidth;
			for (int x = 0; x < length;)
			{
				int xOffset = scaledX;

				try
				{
					xOffset += x;
					ushort val = _TextureData[yOffset + xOffset];

					ushort alpha = 255;
					ushort red = val;
					ushort green = val;
					ushort blue = val;

					blue = (ushort)(((ushort)(val << 1) >> 11) << 3);
					green = (ushort)(((ushort)(val << 6) >> 11) << 3);
					red = (ushort)(((ushort)(val << 11) >> 11) << 3);

					alpha = (ushort)(val >> 15);
					if (alpha != 0 || blue != 0 || green != 0 || red != 0)
					{
						alpha = 255;
					}
					else
					{
						alpha = 0;
					}

					colours[x++] = Color.FromArgb(alpha, red, green, blue);
				}
				catch (Exception ex)
				{
				}
			}

			colourTable.colours = colours;
			return colourTable;
		}*/
	}
}
