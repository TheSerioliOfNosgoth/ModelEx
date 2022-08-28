using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Security.Cryptography;
using TPage = BenLincoln.TheLostWorlds.CDTextures.PlaystationTexturePage;
using TPages = BenLincoln.TheLostWorlds.CDTextures.PlaystationTextureDictionary;

namespace BenLincoln.TheLostWorlds.CDTextures
{
	public class SoulReaverPlaystationCRMTextureFile : BenLincoln.TheLostWorlds.CDTextures.TextureFile
	{
		public struct SoulReaverPlaystationTextureData
		{
			public int[] u;             // 0-255 each
			public int[] v;             // 0-255 each 
			public int textureID;       // 0-8
			public ushort CLUT;
			public int paletteColumn;   // 0-32
			public int paletteRow;      // 0-255
			public uint materialColor;
			public bool textureUsed;
			public bool visible;
			public ushort tPage;
		}

		protected TPages _TPages;
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

		public void BuildTexturesFromGreyscalePallete(TPages tPages)
		{
			_TPages = tPages;
			_TPages.Initialize(_TextureData, _ImageWidth, _ImageHeight, _TotalWidth, true);

			_TextureCount = _TPages.Count;
			_Textures = new Bitmap[_TPages.Count];

			for (int i = 0; i < _TextureCount; i++)
			{
				_Textures[i] = GetTextureAsBitmap(i, _TPages[i].GetGreyscalePallete());
			}
		}

		public void BuildTexturesFromPolygonData(SoulReaverPlaystationTextureData[] texData, TPages tPages, bool drawGreyScaleFirst, bool quantizeBounds, CDC.Objects.ExportOptions options)
		{
			_TPages = tPages;
			_TPages.Initialize(_TextureData, _ImageWidth, _ImageHeight, _TotalWidth, options.AlwaysUseGreyscaleForMissingPalettes);

			_TextureCount = _TPages.Count;
			_Textures = new Bitmap[_TPages.Count];

			bool debugTextureCollisions = false;

			// Initialize the textures
			if (drawGreyScaleFirst)
			{
				for (int i = 0; i < _TextureCount; i++)
				{
					_Textures[i] = GetTextureAsBitmap(i, _TPages[i].GetGreyscalePallete());
				}
			}
			else
			{
				Color chromaKey = Color.FromArgb(1, 128, 128, 128);
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

			// Use the polygon data to color in all possible parts of the textures
			foreach (SoulReaverPlaystationTextureData poly in texData)
			{
				TPage texturePage = _TPages[poly.textureID];
				Color[] palette = texturePage.GetPallete(poly.CLUT);
				PlaystationPixelList polyPixelList = texturePage.GetPixelList();

				bool dumpPreviousTextureVersion = false;
				List<byte[]> textureHashes = new List<byte[]>();
				Bitmap previousTexture = (Bitmap)_Textures[poly.textureID].Clone();

				int uMin = 255;
				int uMax = 0;
				int vMin = 255;
				int vMax = 0;

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
				//int numTransparentColors = 0;
				//foreach (Color c in palette)
				//{
				//    if (c.A == 0)
				//    {
				//        numTransparentColors++;
				//    }
				//}
				//if (numTransparentColors > 1)
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

						Color pixelColor = palette[pixel];
						uint materialAlpha = (poly.materialColor & 0xFF000000) >> 24;
						//materialAlpha = 255;
						//if (materialAlpha < 255)
						//{
						//    pixelColor = Color.FromArgb((byte)materialAlpha, palette[pixel].R, palette[pixel].G, palette[pixel].B);
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
							_Textures[poly.textureID].SetPixel(x, y, pixelColor);
						}
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

			// Fill in uncolored parts of each texture using the most common palette.
			#region Unused Sections
			for (int texNum = 0; texNum < _Textures.Length; texNum++)
			{
				TPage texturePage = _TPages[texNum];
				Color[] commonPalette = texturePage.GetCommonPalette();
				bool hasAtLeastOneVisiblePixel = false;

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
							Color pixelColor = commonPalette[pixel];
							if (pixChroma)
							{
								_Textures[texNum].SetPixel(x, y, pixelColor);
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
			#endregion

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

			TPage texturePage = _TPages[index];
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
				tex = GetTextureAsBitmap(index, _TPages[index].GetGreyscalePallete());
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

		public void ExportAllPaletteVariations(TPages tPages, bool alwaysUseGreyscaleForMissingPalettes)
		{
			if (tPages == null)
			{
				return;
			}

			tPages.Initialize(_TextureData, _ImageWidth, _ImageHeight, _TotalWidth, alwaysUseGreyscaleForMissingPalettes);

			Bitmap[] textures = new Bitmap[tPages.Count];
			Color chromaKey = Color.FromArgb(1, 128, 128, 128);
			for (int i = 0; i < tPages.Count; i++)
			{
				textures[i] = new Bitmap(_ImageWidth, _ImageHeight);
				for (int y = 0; y < _ImageHeight; y++)
				{
					for (int x = 0; x < _ImageWidth; x++)
					{
						textures[i].SetPixel(x, y, chromaKey);
					}
				}
			}

			foreach (PlaystationColorTable colorTable in tPages.GetColorTables())
			{
				for (int texNum = 0; texNum < textures.Length; texNum++)
				{
					TPage texturePage = tPages[texNum];
					Color[] palette = colorTable.colors;

					Bitmap exportTemp = (Bitmap)textures[texNum].Clone();
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
								Color pixelColor = palette[pixel];
								if (pixChroma)
								{
									exportTemp.SetPixel(x, y, pixelColor);
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

						string fileName = string.Format(@"Texture_Debugging\Texture-{0:X8}-count_{1:X8}-clut_{2:X8}.png", texNum, tPages.GetClutRefCount(colorTable.clut), colorTable.clut);
						exportTemp.Save(fileName, ImageFormat.Png);
					}
				}
			}
		}
	}
}
