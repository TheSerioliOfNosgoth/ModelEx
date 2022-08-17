using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Security.Cryptography;

namespace BenLincoln.TheLostWorlds.CDTextures
{
	public class SoulReaverPlaystationCRMTextureFile : BenLincoln.TheLostWorlds.CDTextures.TextureFile
	{
		protected class TexturePage
		{
			public ushort tPage;
			public int tp;
			public int abr;
			public int x;
			public int y;
			public ushort[,] pixels;
		}

		protected class ColourTable
		{
			public ushort clut;
			public int x;
			public int y;
			public Color[] colours;
		}

		public struct SoulReaverPlaystationTextureData
		{
			public int[] u;             // 0-255 each
			public int[] v;             // 0-255 each 
			public int textureID;       // 0-8
			public ushort CLUT;
			public int paletteColumn;   // 0-32
			public int paletteRow;      // 0-255
			public uint materialColour;
			public bool textureUsed;
			public bool visible;
			public ushort tPage;
		}

		protected TexturePage[] _TPages;
		protected ushort[,] _TextureData;
		protected Bitmap[] _Textures;
		protected int _TotalWidth;
		protected int _TotalHeight;
		protected Dictionary<int, Dictionary<ushort, Bitmap>> _TexturesByCLUT;
		protected readonly int _ImageWidth = 256;
		protected readonly int _ImageHeight = 256;

		public Dictionary<int, Dictionary<ushort, Bitmap>> TexturesByCLUT { get { return _TexturesByCLUT; } }

		public SoulReaverPlaystationCRMTextureFile(string path)
			: base(path)
		{
			_FileType = TextureFileType.Gex3Playstation;
			_FileTypeName = "Soul Reaver (Playstation) Indexed Texture File";
			_TexturesByCLUT = new Dictionary<int, Dictionary<ushort, Bitmap>>();
			LoadTextureData();
		}

		protected override int _GetTextureCount()
		{
			return _TextureCount;
		}

		protected void LoadTextureData()
		{
			try
			{
				FileStream stream = new FileStream(_FilePath, FileMode.Open, FileAccess.Read);
				BinaryReader reader = new BinaryReader(stream);

				stream.Seek(28, SeekOrigin.Begin);
				_TotalWidth = reader.ReadInt32();
				_TotalHeight = reader.ReadInt32();

				stream.Seek(36, SeekOrigin.Begin);
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

		protected TexturePage GetTexturePage(ushort tPage)
		{
			TexturePage texturePage = new TexturePage
			{
				tPage = tPage,
				tp = 0, //(tPage >> 7) & 0x03,//(tPage >> 7) & 0x003,
				abr = 0,
				x = ((tPage << 6) & 0x7c0) % _TotalWidth, // % 1024
				y = ((tPage << 4) & 0x100) + ((tPage >> 2) & 0x200),
				pixels = new ushort[_ImageHeight, _ImageWidth]
			};

			int width = _TextureData.GetUpperBound(1);
			int height = _TextureData.GetUpperBound(0);

			if (texturePage.y > _TextureData.GetUpperBound(0))
			{
				return texturePage;
			}

			if (texturePage.x > _TextureData.GetUpperBound(1))
			{
				return texturePage;
			}

			for (int y = 0; y < _ImageHeight; y++)
			{
				for (int x = 0; x < _ImageWidth;)
				{
					if (texturePage.tp == 0) // 4 bit
					{
						ushort val = _TextureData[texturePage.y + y, texturePage.x + (x / 4)];
						texturePage.pixels[y, x++] = (ushort)(val & 0x000F);
						texturePage.pixels[y, x++] = (ushort)((val & 0x00F0) >> 4);
						texturePage.pixels[y, x++] = (ushort)((val & 0x0F00) >> 8);
						texturePage.pixels[y, x++] = (ushort)((val & 0xF000) >> 12);
					}
					else if (texturePage.tp == 1) // 8 bit
					{
						ushort val = _TextureData[texturePage.y + y, texturePage.x + (x / 2)];
						texturePage.pixels[y, x++] = (ushort)(val & 0x00FF);
						texturePage.pixels[y, x++] = (ushort)((val & 0xFF00) >> 8);
					}
					else if (texturePage.tp == 2) // 16 bit
					{
						ushort val = _TextureData[texturePage.y + y, texturePage.x + x];
						texturePage.pixels[y, x++] = val;
					}
				}
			}

			return texturePage;
		}

		protected ColourTable GetTextureClut(ushort clut, int length)
		{
			ColourTable colourTable = new ColourTable
			{
				clut = clut,
				x = ((clut & 0x3F) << 4) % _TotalWidth, // % 1024
				y = clut >> 6,
				colours = new Color[length]
			};

			int width = _TextureData.GetUpperBound(1);
			int height = _TextureData.GetUpperBound(0);

			if (colourTable.y > _TextureData.GetUpperBound(0))
			{
				return colourTable;
			}

			if (colourTable.x > _TextureData.GetUpperBound(1))
			{
				return colourTable;
			}

			for (int x = 0; x < length; x++)
			{
				ushort val = _TextureData[colourTable.y, colourTable.x + x];
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

				colourTable.colours[x] = Color.FromArgb(alpha, red, green, blue);
			}

			return colourTable;
		}

		protected Color[] GetGreyscalePalette(int index)
		{
			TexturePage tPage = _TPages[(ushort)index];

			int paletteSize = (tPage.tp == 0) ? 16 : 256;
			Color[] greyPalette = new Color[paletteSize];
			for (int i = 0; i < paletteSize; i++)
			{
				int luma = (i * 256) / paletteSize;
				greyPalette[i] = Color.FromArgb(luma, luma, luma);
			}

			return greyPalette;
		}

		protected Color[] GetPalette(int index, ushort clut)
		{
			TexturePage texturePage = _TPages[index];
			return GetTextureClut(clut, texturePage.tp == 0 ? 16 : 256).colours;
		}

		public static bool ByteArraysAreEqual(byte[] arr1, byte[] arr2)
		{
			if (arr1.Length != arr2.Length)
			{
				return false;
			}
			for (int i = 0; i < arr1.Length; i++)
			{
				if (arr1[i] != arr2[i])
				{
					return false;
				}
			}
			return true;
		}

		public static bool ListContainsByteArray(List<byte[]> byteArray, byte[] checkArray)
		{
			foreach (byte[] mem in byteArray)
			{
				if (ByteArraysAreEqual(mem, checkArray))
				{
					return true;
				}
			}
			return false;
		}

		// https://stackoverflow.com/questions/7350679/convert-a-bitmap-into-a-byte-array
		public static byte[] ImageToByteArray(Image img)
		{
			using (var stream = new MemoryStream())
			{
				img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
				return stream.ToArray();
			}
		}

		protected void AddTextureWithCLUT(int textureID, ushort clut, Bitmap texture)
		{
			Dictionary<ushort, Bitmap> textureEntry = new Dictionary<ushort, Bitmap>();
			if (_TexturesByCLUT.ContainsKey(textureID))
			{
				textureEntry = _TexturesByCLUT[textureID];
			}
			if (textureEntry.ContainsKey(clut))
			{
				Console.WriteLine(string.Format("Debug: texture {0:X8}, CLUT {1:X4} already exists in the collection", textureID, clut));
			}
			else
			{
				textureEntry.Add(clut, texture);
			}
			if (_TexturesByCLUT.ContainsKey(textureID))
			{
				_TexturesByCLUT[textureID] = textureEntry;
			}
			else
			{
				_TexturesByCLUT.Add(textureID, textureEntry);
			}
		}

		protected void CreateARGBTextureFromCLUTIfNew(int textureID, ushort clut, Color[] palette)
		{
			bool createTexture = true;
			if (_TexturesByCLUT.ContainsKey(textureID))
			{
				Dictionary<ushort, Bitmap> textureEntry = _TexturesByCLUT[textureID];
				if (textureEntry.ContainsKey(clut))
				{
					createTexture = false;
					//Console.WriteLine(string.Format("Debug: texture {0:X8}, CLUT {1:X4} already exists in the collection - will not recreate", textureID, clut));
				}
			}
			if (createTexture)
			{
				Bitmap tex = GetTextureAsBitmap(textureID, palette);
				AddTextureWithCLUT(textureID, clut, tex);
			}
		}

		public void BuildTexturesFromGreyscalePallete(ushort[] texturePages)
		{
			if (_TPages == null)
			{
				List<TexturePage> tPages = new List<TexturePage>();

				for (int i = 0; i < texturePages.Length; i++)
				{
					ushort tPage = (ushort)(texturePages[i] & 0x0000FFFFu);
					if (tPages.FindIndex(x => x.tPage == tPage) < 0)
					{
						tPages.Add(GetTexturePage(tPage));
					}
				}

				_TPages = tPages.ToArray();
				_TextureCount = _TPages.Length;
				_Textures = new Bitmap[_TPages.Length];
			}

			for (int i = 0; i < _TextureCount; i++)
			{
				_Textures[i] = GetTextureAsBitmap(i, GetGreyscalePalette(i));
			}
		}

		public void BuildTexturesFromPolygonData(SoulReaverPlaystationTextureData[] texData, ushort[] texturePages, bool drawGreyScaleFirst, bool quantizeBounds, CDC.Objects.ExportOptions options)
		{
			if (_TPages == null)
			{
				List<TexturePage> tPages = new List<TexturePage>();

				for (int i = 0; i < texturePages.Length; i++)
				{
					ushort tPage = (ushort)(texturePages[i] & 0x0000FFFFu);
					if (tPages.FindIndex(x => x.tPage == tPage) < 0)
					{
						tPages.Add(GetTexturePage(tPage));
					}
				}

				_TPages = tPages.ToArray();
				_TextureCount = _TPages.Length;
				_Textures = new Bitmap[_TPages.Length];
			}

			// hashtable to store counts of palette usage
			Hashtable palettes = new Hashtable();

			bool debugTextureCollisions = false;

			// initialize textures
			if (drawGreyScaleFirst)
			{
				for (int i = 0; i < _TextureCount; i++)
				{
					_Textures[i] = GetTextureAsBitmap(i, GetGreyscalePalette(i));
				}
			}
			else
			{
				Color chromaKey = Color.FromArgb(1, 128, 128, 128);
				//Color chromaKey = Color.FromArgb(255, 128, 128, 128);
				for (int i = 0; i < _TextureCount; i++)
				{
					_Textures[i] = new Bitmap(_ImageWidth, _ImageHeight);
					for (int y = 0; y < _ImageHeight; y++)
					{
						for (int x = 0; x < _ImageWidth; x++)
						{
							_Textures[i].SetPixel(x, y, chromaKey);
						}
					}
				}
			}

			Dictionary<int, Dictionary<int, ulong>> handledPixels = new Dictionary<int, Dictionary<int, ulong>>();
			//Dictionary<int, ulong> handledPixels = new Dictionary<int, ulong>();

			// use the polygon data to colour in all possible parts of the textures
			foreach (SoulReaverPlaystationTextureData poly in texData)
			{
				TexturePage texturePage = _TPages[poly.textureID];

				bool dumpPreviousTextureVersion = false;
				bool wrotePixels = false;
				List<byte[]> textureHashes = new List<byte[]>();
				Bitmap previousTexture = (Bitmap)_Textures[poly.textureID].Clone();
				Dictionary<int, ulong> polyPixelList = new Dictionary<int, ulong>();
				if (handledPixels.ContainsKey(poly.textureID))
				{
					polyPixelList = handledPixels[poly.textureID];
				}

				int uMin = 255;
				int uMax = 0;
				int vMin = 255;
				int vMax = 0;

				Color[] palette = GetPalette(poly.textureID, poly.CLUT);

				//bool exportAllPaletteVariations = false;
				//if (exportAllPaletteVariations)
				//{
				//    if (!Directory.Exists("Texture_Debugging"))
				//    {
				//        Directory.CreateDirectory("Texture_Debugging");
				//    }
				//    //string fileName = string.Format(@"Texture_Debugging\Texture-{0:X8}-row_{1:X4}-column_{2:X4}-CLUT_{3:X4}.png", poly.textureID, poly.paletteColumn, poly.paletteRow, poly.CLUT);
				//    string fileName = string.Format(@"Texture_Debugging\Texture-{0:X8}-CLUT_{3:X4}-row_{1:X4}-column_{2:X4}.png", poly.textureID, poly.paletteColumn, poly.paletteRow, poly.CLUT);
				//    if (!File.Exists(fileName))
				//    {
				//        Bitmap currentVariation = GetTextureAsBitmap(poly.textureID, palette);
				//        currentVariation.Save(fileName, ImageFormat.Png);
				//    }
				//}
				if (options.UseEachUniqueTextureCLUTVariation)
				{
					CreateARGBTextureFromCLUTIfNew(poly.textureID, poly.CLUT, palette);
				}

				// some basic sanity-checking for bad palette information obtained via incorrect parsing of data
				//bool paletteIsValid = true;
				//if ((poly.paletteColumn % 2) == 1)
				//{
				//    paletteIsValid = false;
				//}
				//int numTransparentColours = 0;
				//foreach (Color c in palette)
				//{
				//    if (c.A == 0)
				//    {
				//        numTransparentColours++;
				//    }
				//}
				//if (numTransparentColours > 1)
				//{
				//    paletteIsValid = false;
				//}
				//if (!paletteIsValid)
				//{
				//    palette = GetGreyscalePalette();
				//    Console.WriteLine(string.Format("Invalid palette detected for texture ID {0}: row {1}, column {2}", poly.textureID, poly.paletteRow, poly.paletteColumn));
				//}
				// get the rectangle defined by the minimum and maximum U and V coords
				foreach (int u in poly.u)
				{
					uMin = Math.Min(uMin, u);
					uMax = Math.Max(uMax, u);
				}
				foreach (int v in poly.v)
				{
					vMin = Math.Min(vMin, v);
					vMax = Math.Max(vMax, v);
				}

				int width = uMax - uMin;
				for (int b = 0; b < 8; b++)
				{
					if ((1 << b) >= width)
					{
						width = 1 << b;
						break;
					}
				}

				int height = vMax - vMin;
				for (int b = 0; b < 8; b++)
				{
					if ((1 << b) >= height)
					{
						height = 1 << b;
						break;
					}
				}

				// if specified, quantize the rectangle's boundaries to a multiple of 16
				if (quantizeBounds)
				{
					//int quantizeRes = 16;
					int quantizeRes = 16;
					while ((uMin % quantizeRes) > 0)
					{
						uMin--;
					}
					while ((uMax % quantizeRes) > 0)
					{
						uMax++;
					}
					while ((vMin % quantizeRes) > 0)
					{
						vMin--;
					}
					while ((vMax % quantizeRes) > 0)
					{
						vMax++;
					}
					if (uMin < 0)
					{
						uMin = 0;
					}
					if (uMax > 256)
					{
						uMax = 256;
					}
					if (vMin < 0)
					{
						vMin = 0;
					}
					if (vMax > 256)
					{
						vMax = 256;
					}
				}

				for (int y = vMin; y < vMax; y++)
				{
					for (int x = uMin; x < uMax; x++)
					{
						int dataOffset = (_ImageHeight * y) + x;
						int pixel = texturePage.pixels[y, x];

						Color pixelColour = palette[pixel];
						uint materialAlpha = (poly.materialColour & 0xFF000000) >> 24;
						//materialAlpha = 255;
						//if (materialAlpha < 255)
						//{
						//    pixelColour = Color.FromArgb((byte)materialAlpha, palette[pixel].R, palette[pixel].G, palette[pixel].B);
						//}
						//ulong checkPixels = ((ulong)materialAlpha << 56) | ((ulong)palette[pixel].R << 48) | ((ulong)palette[pixel].G << 40)
						//    | ((ulong)palette[pixel].B << 32) | ((ulong)materialAlpha << 24);
						ulong checkPixels = ((ulong)palette[pixel].A << 56) | ((ulong)palette[pixel].R << 48) | ((ulong)palette[pixel].G << 40)
							| ((ulong)palette[pixel].B << 32);
						bool writePixels = true;
						if (polyPixelList.ContainsKey(dataOffset))
						{
							if (polyPixelList[dataOffset] != checkPixels)
							{
								bool drawPixels = true;
								//if (materialAlpha == 0)
								//{
								//    //Console.WriteLine("Debug: Material has an alpha of 0 - ignoring");
								//    drawPixels = false;
								//}
								//if (materialAlpha < 0x40)
								//{
								//    //Console.WriteLine("Debug: Material has an alpha of less than 0x40 - ignoring");
								//    drawPixels = false;
								//}
								if (!poly.textureUsed)
								{
									//Console.WriteLine("Debug: polygon does not use a texture - ignoring");
									drawPixels = false;
								}
								if (!poly.visible)
								{
									//Console.WriteLine("Debug: polygon is invisible - ignoring");
									drawPixels = false;
								}
								if (drawPixels)
								{
									if ((uMin != 0) && (vMin != 0))
									{
										dumpPreviousTextureVersion = true;
									}
									//Console.WriteLine(string.Format("Warning: pixels with offset {0} in texture with ID {1} have already been written with a different value. Existing: 0x{2:X16}, New:  0x{3:X16}", dataOffset, poly.textureID, polyPixelList[dataOffset], checkPixels));
								}
								else
								{
									//Console.WriteLine(string.Format("Warning: not updating pixels with offset {0} in texture with ID {1}, because the new values are for a polygon that is not drawn/invisible, or has no texture. Existing: 0x{2:X16}, Ignored:  0x{3:X16}", dataOffset, poly.textureID, polyPixelList[dataOffset], checkPixels));
									writePixels = false;
								}
							}
						}
						else
						{
							polyPixelList.Add(dataOffset, checkPixels);
						}

						if (writePixels)
						{
							_Textures[poly.textureID].SetPixel(x, y, pixelColour);
							wrotePixels = true;
						}
					}
				}
				if (handledPixels.ContainsKey(poly.textureID))
				{
					handledPixels[poly.textureID] = polyPixelList;
				}
				else
				{
					handledPixels.Add(poly.textureID, polyPixelList);
				}

				if (wrotePixels)
				{
					// add or update palette list
					string paletteIDString = poly.textureID.ToString() + "-" + poly.CLUT.ToString();
					if (palettes.Contains(paletteIDString))
					{
						int palCount = (int)palettes[paletteIDString];
						palCount++;
						palettes[paletteIDString] = palCount;
					}
					else
					{
						int newPalCount = 1;
						palettes.Add(paletteIDString, newPalCount);
					}
				}

				if (debugTextureCollisions)
				{
					if (dumpPreviousTextureVersion)
					{
						if (!Directory.Exists("Texture_Debugging"))
						{
							Directory.CreateDirectory("Texture_Debugging");
						}
						// dump each overwritten version of the texture for debugging
						byte[] previousHash = new MD5CryptoServiceProvider().ComputeHash(ImageToByteArray(previousTexture));
						if (!ListContainsByteArray(textureHashes, previousHash))
						{
							int fileNum = 0;
							bool gotFilename = false;
							string fileName = "";
							while (!gotFilename)
							{
								fileName = string.Format(@"Texture_Debugging\Texture-{0:X8}-version_{1:X8}.png", poly.textureID, fileNum);
								if (!File.Exists(fileName))
								{
									gotFilename = true;
								}
								fileNum++;
							}
							if (fileName != "")
							{
								//_Textures[poly.textureID].Save(fileName, ImageFormat.Png);
								previousTexture.Save(fileName, ImageFormat.Png);
							}
							textureHashes.Add(previousHash);
						}
					}
				}
			}

			if (!drawGreyScaleFirst)
			{
				bool exportAllPaletteVariations = false;

				if (exportAllPaletteVariations)
				{
					foreach (string pID in palettes.Keys)
					{
						int useCount = (int)palettes[pID];
						string[] palDecode2 = pID.Split('-');
						int tID = int.Parse(palDecode2[0]);
						ushort clut = ushort.Parse(palDecode2[1]);

						for (int texNum = 0; texNum < _Textures.Length; texNum++)
						{
							TexturePage texturePage = _TPages[texNum];
							Bitmap exportTemp = (Bitmap)_Textures[texNum].Clone();
							Color[] palette = GetPalette(texNum, clut);
							bool exportThisTexture = false;
							for (int y = 0; y < _ImageHeight; y++)
							{
								for (int x = 0; x < _ImageWidth; x++)
								{
									bool pixChroma = (exportTemp.GetPixel(x, y).A == 1);
									if (pixChroma)
									{
										exportThisTexture = true;
										int pixel = texturePage.pixels[y, x];
										Color pixelColour = palette[pixel];
										if (pixChroma)
										{
											exportTemp.SetPixel(x, y, pixelColour);
										}
									}
								}
							}
							if (exportThisTexture)
							{
								if (!Directory.Exists("Texture_Debugging"))
								{
									Directory.CreateDirectory("Texture_Debugging");
								}
								string fileName = string.Format(@"Texture_Debugging\Texture-{0:X8}-count_{1:X8}-palette_texture_ID_{2:X8}-clut_{4:X8}.png", texNum, useCount, tID, clut);
								exportTemp.Save(fileName, ImageFormat.Png);
							}
						}
					}
				}

				List<int> allTexturesWithPalettes = new List<int>();
				foreach (string pID in palettes.Keys)
				{
					string[] pd = pID.Split('-');
					int txID = int.Parse(pd[0]);
					ushort clut = ushort.Parse(pd[1]);
					if (!allTexturesWithPalettes.Contains(txID))
					{
						if (clut != 0)
						{
							allTexturesWithPalettes.Add(txID);
						}
					}
				}

				for (int texNum = 0; texNum < _Textures.Length; texNum++)
				{
					TexturePage texturePage = _TPages[texNum];

					// find the most frequently-used palette
					string palID = "";
					int palCount = 0;
					ushort mostCommonPaletteClut = 0;
					bool hasAtLeastOneVisiblePixel = false;
					foreach (string pID in palettes.Keys)
					{
						string[] pd = pID.Split('-');
						int txID = int.Parse(pd[0]);
						bool considerPalette = true;
						// don't use palettes from other textures unless there were no palettes found for this texture
						if (allTexturesWithPalettes.Contains(texNum))
						{
							if (txID != texNum)
							{
								considerPalette = false;
							}
						}
						if (considerPalette)
						{
							ushort clut = ushort.Parse(pd[1]);
							int pCount = (int)palettes[pID];
							if (pCount > palCount)
							{
								palID = pID;
								if (clut != 0)
								{
									mostCommonPaletteClut = clut;
								}
								// don't use palettes with column/row zero or with odd column numbers unless there's no other choice
								//if ((pc != 0) && (pr != 0) && (pc % 2 == 0))
								//{
								//    break;
								//}
							}
						}
					}
					Color[] commonPalette = GetPalette(texNum, mostCommonPaletteClut);

					// use a greyscale palette instead of column 0, row 0, because column 0, row 0 is always garbage that makes 
					// the texture impossible to view properly, and full of random transparent pixels
					if (options.AlwaysUseGreyscaleForMissingPalettes || mostCommonPaletteClut == 0)
					{
						commonPalette = GetGreyscalePalette(texNum);
					}

					for (int y = 0; y < _ImageHeight; y++)
					{
						for (int x = 0; x < _ImageWidth; x++)
						{
							if (!hasAtLeastOneVisiblePixel)
							{
								if (_Textures[texNum].GetPixel(x, y).A > 1)
								{
									hasAtLeastOneVisiblePixel = true;
								}
							}
							bool pixChroma = (_Textures[texNum].GetPixel(x, y).A == 1);
							if (pixChroma)
							{
								int dataOffset = (_ImageHeight * y) + x;
								int pixel = texturePage.pixels[y, x];
								Color pixelColour = commonPalette[pixel];
								if (pixChroma)
								{
									_Textures[texNum].SetPixel(x, y, pixelColour);
								}
							}
						}
					}
					if ((!hasAtLeastOneVisiblePixel) && (options.UnhideCompletelyTransparentTextures))
					{
						for (int y = 0; y < _Textures[texNum].Width; y++)
						{
							for (int x = 0; x < _Textures[texNum].Height; x++)
							{
								Color pix = _Textures[texNum].GetPixel(x, y);
								if (pix.A < 2)
								{
									_Textures[texNum].SetPixel(x, y, Color.FromArgb(128, pix.R, pix.G, pix.B));
								}
							}
						}
					}
				}
			}

			// dump all textures as PNGs for debugging
			//texNum = 0;
			//foreach (Bitmap tex in _Textures)
			//{
			//    tex.Save(@"C:\Debug\Tex-" + texNum + ".png", ImageFormat.Png);
			//    texNum++;
			//}
		}

		protected override Bitmap _GetTextureAsBitmap(int index)
		{
			return _Textures[index];
		}

		protected Bitmap GetTextureAsBitmap(int index, Color[] palette)
		{
			Bitmap tex = new Bitmap(_ImageWidth, _ImageHeight);

			TexturePage texturePage = _TPages[index];
			for (int y = 0; y < _ImageHeight; y++)
			{
				for (int x = 0; x < _ImageWidth; x++)
				{
					int pixel = texturePage.pixels[y, x];
					tex.SetPixel(x, y, palette[pixel]);
				}
			}

			return tex;
		}

		public override MemoryStream GetDataAsStream(int index)
		{
			Bitmap tex = _Textures[index];
			if (tex == null)
			{
				tex = GetTextureAsBitmap(index, GetGreyscalePalette(index));
			}

			MemoryStream stream = new MemoryStream();
			tex.Save(stream, ImageFormat.Png);
			return stream;
		}

		public override MemoryStream GetTextureWithCLUTAsStream(int textureID, ushort clut)
		{
			if (!_TexturesByCLUT.ContainsKey(textureID))
			{
				Console.WriteLine(string.Format("Error: did not find any texture-by-CLUT data for texture ID {0} - returning the default version of this texture", textureID));
				return GetDataAsStream(textureID);
			}

			if (!_TexturesByCLUT[textureID].ContainsKey(clut))
			{
				Console.WriteLine(string.Format("Error: did not find CLUT {0:X4} variation of texture ID {1} - returning the default version of this texture", clut, textureID));
				return GetDataAsStream(textureID);
			}

			Bitmap tex = _TexturesByCLUT[textureID][clut];
			MemoryStream stream = new MemoryStream();
			tex.Save(stream, ImageFormat.Png);
			return stream;
		}

		public override void ExportFile(int index, string outPath)
		{
			Bitmap tex = _Textures[index];
			tex.Save(outPath, ImageFormat.Png);
		}
	}
}
