using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Security.Cryptography;
using TPage = BenLincoln.TheLostWorlds.CDTextures.PSXTexturePage;
using TPages = BenLincoln.TheLostWorlds.CDTextures.PSXTextureDictionary;

namespace BenLincoln.TheLostWorlds.CDTextures
{
	public class PSXTextureFile : TextureFile
	{
		protected struct BoundingRectangle
		{
			public int uMin;
			public int uMax;
			public int vMin;
			public int vMax;
		}

		protected TPages _TPages;
		protected ushort[,] _TextureData;
		protected Bitmap[] _Textures;
		protected int _TotalWidth;
		protected int _TotalHeight;
		protected int _XShift;
		protected Dictionary<int, Dictionary<ushort, Bitmap>> _TexturesByCLUT;
		protected readonly int _ImageWidth = 256;
		protected readonly int _ImageHeight = 256;

		public Dictionary<int, Dictionary<ushort, Bitmap>> TexturesByCLUT { get { return _TexturesByCLUT; } }

		public PSXTextureFile(string path)
			: base(path)
		{
			_TexturesByCLUT = new Dictionary<int, Dictionary<ushort, Bitmap>>();
		}

		protected override int _GetTextureCount()
		{
			return _TextureCount;
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
			_TPages.Initialize(_TextureData, _ImageWidth, _ImageHeight, _XShift, true);

			_TextureCount = _TPages.Count;
			_Textures = new Bitmap[_TPages.Count];

			for (int i = 0; i < _TextureCount; i++)
			{
				_Textures[i] = GetTextureAsBitmap(i, _TPages[i].GetGreyscalePallete());
			}
		}

		public void BuildTexturesFromPolygonData(TPages tPages, bool drawGreyScaleFirst, bool quantizeBounds, CDC.ExportOptions options)
		{
			_TPages = tPages;
			_TPages.Initialize(_TextureData, _ImageWidth, _ImageHeight, _XShift, options.AlwaysUseGreyscaleForMissingPalettes);

			_TextureCount = _TPages.Count;
			_Textures = new Bitmap[_TPages.Count];

			bool debugTextureCollisions = false;

			// Initialize the textures
			#region Initialize Textures
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
			#endregion

			// Use the polygon data to color in all possible parts of the textures
			#region Sections used by Polygons
			List<PSXTextureTile> texData = _TPages.tiles;
			foreach (PSXTextureTile poly in texData)
			{
				TPage texturePage = _TPages[poly.textureID];
				Color[] palette = texturePage.GetPallete(poly.clut);
				PSXPixelList polyPixelList = texturePage.GetPixelList();

				bool dumpPreviousTextureVersion = false;
				List<byte[]> textureHashes = null;
				Bitmap previousTexture = null;
				if (debugTextureCollisions)
				{
					textureHashes = new List<byte[]>();
					previousTexture = (Bitmap)_Textures[poly.textureID].Clone();
				}

				if (options.UseEachUniqueTextureCLUTVariation)
				{
					CreateARGBTextureFromCLUTIfNew(poly.textureID, poly.clut, palette);
				}

				GetPolygonUVRectangle(in poly, out BoundingRectangle boundingRectangle, quantizeBounds);

				for (int y = boundingRectangle.vMin; y <= boundingRectangle.vMax; y++)
				{
					for (int x = boundingRectangle.uMin; x <= boundingRectangle.uMax; x++)
					{
						int pixel = texturePage.pixels[y, x];
						Color pixelColor = palette[pixel];
						ulong checkPixels = (ulong)unchecked(pixelColor.ToArgb()) << 32;
						checkPixels |= 1;
						bool writePixels = true;
						if (polyPixelList[y, x] != 0)
						{
							if (polyPixelList[y, x] != checkPixels)
							{
								bool drawPixels = true;

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
									if ((boundingRectangle.uMin != 0) && (boundingRectangle.vMin != 0))
									{
										dumpPreviousTextureVersion = true;
									}
									//Console.WriteLine(string.Format("Warning: pixels[{0},{1}] in texture with ID {2} have already been written with a different value. Existing: 0x{3:X16}, New:  0x{4:X16}", y, x, poly.textureID, polyPixelList[y, x], checkPixels));
								}
								else
								{
									//Console.WriteLine(string.Format("Warning: not updating pixels[{0},{1}] in texture with ID {2}, because the new values are for a polygon that is not drawn/invisible, or has no texture. Existing: 0x{3:X16}, Ignored:  0x{4:X16}", y, x, poly.textureID, polyPixelList[y, x], checkPixels));
									writePixels = false;
								}
							}
						}
						else
						{
							polyPixelList[y, x] = checkPixels;
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

						// Dump each overwritten version of the texture for debugging
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
			#endregion

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

			// Dump all textures as PNGs for debugging
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
			tPages.Initialize(_TextureData, _ImageWidth, _ImageHeight, _XShift, alwaysUseGreyscaleForMissingPalettes);

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

			foreach (PSXColorTable colorTable in tPages.GetColorTables())
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

		protected void GetPolygonUVRectangle(in PSXTextureTile poly, out BoundingRectangle boundingRectangle, bool quantizeBounds)
		{
			boundingRectangle = new BoundingRectangle()
			{
				uMin = 255,
				uMax = 0,
				vMin = 255,
				vMax = 0
			};

			// Get the rectangle defined by the minimum and maximum U and V coords
			foreach (int u in poly.u)
			{
				boundingRectangle.uMin = Math.Min(boundingRectangle.uMin, u);
				boundingRectangle.uMax = Math.Max(boundingRectangle.uMax, u);
			}

			foreach (int v in poly.v)
			{
				boundingRectangle.vMin = Math.Min(boundingRectangle.vMin, v);
				boundingRectangle.vMax = Math.Max(boundingRectangle.vMax, v);
			}

			// If specified, quantize the rectangle's boundaries to a multiple of 16
			if (quantizeBounds)
			{
				int quantizeRes = 16;
				while ((boundingRectangle.uMin % quantizeRes) > 0)
				{
					boundingRectangle.uMin--;
				}

				while ((boundingRectangle.uMax % quantizeRes) < (quantizeRes - 1))
				//while ((boundingRectangle.uMax % quantizeRes) > 0)
				{
					boundingRectangle.uMax++;
				}

				while ((boundingRectangle.vMin % quantizeRes) > 0)
				{
					boundingRectangle.vMin--;
				}

				while ((boundingRectangle.vMax % quantizeRes) < (quantizeRes - 1))
				//while ((boundingRectangle.vMax % quantizeRes) > 0)
				{
					boundingRectangle.vMax++;
				}

				if (boundingRectangle.uMin < 0)
				{
					boundingRectangle.uMin = 0;
				}

				// 255?
				if (boundingRectangle.uMax > 255)
				{
					boundingRectangle.uMax = 255;
				}

				if (boundingRectangle.vMin < 0)
				{
					boundingRectangle.vMin = 0;
				}

				// 255?
				if (boundingRectangle.vMax > 255)
				{
					boundingRectangle.vMax = 255;
				}
			}
		}
	}
}
