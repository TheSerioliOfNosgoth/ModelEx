using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace BenLincoln.TheLostWorlds.CDTextures
{
	public class SoulReaverDCTextureFile : BenLincoln.TheLostWorlds.CDTextures.SoulReaverMonolithicTextureFile
	{
		public SoulReaverDCTextureFile(string path)
			: base(path)
		{
			_FileType = TextureFileType.SoulReaverDreamcast;
			_FileTypeName = "Soul Reaver (Dreamcast) Vector-Quantized Monolithic Texture File";
			_HeaderLength = 18432;
			_TextureLength = 18432;
			_TextureCount = _GetTextureCount();
		}

		protected override System.Drawing.Bitmap _GetTextureAsBitmap(int index)
		{
			int offset = _HeaderLength + (index * _TextureLength);
			if (offset > (_FileInfo.Length - _TextureLength))
			{
				throw new TextureFileException("An index was specified which resulted in an offset greater " +
					"than the maximum allowed value.");
			}
			//int width = 256;
			//int height = 256;
			//FileStream fStream;
			//BinaryReader bReader;
			ushort[] codeBook;
			byte[] imageData;

			try
			{
				FileStream inStream = new FileStream(_FilePath, FileMode.Open, FileAccess.Read);
				BinaryReader inReader = new BinaryReader(inStream);

				//read in the codebook
				inStream.Seek(offset, SeekOrigin.Begin);
				codeBook = new ushort[1024];
				for (int i = 0; i < 1024; i++)
				{
					codeBook[i] = inReader.ReadUInt16();
				}
				//read in the image data
				imageData = new byte[128 * 128];
				for (int i = 0; i < imageData.GetUpperBound(0); i++)
				{
					imageData[i] = inReader.ReadByte();
				}

				inReader.Close();
				inStream.Close();
				return decodeTwiddledVQ(imageData, codeBook, 256, 256);
			}
			catch (Exception ex)
			{
				throw new TextureFileException("Error reading the specified texture.", ex);
			}
		}

		protected Bitmap decodeTwiddledVQ(byte[] imageData, ushort[] codeBook, int width, int height)
		{
			//this method is based on code from BERO
			Color[] codeBookTable;
			int[] twiddleTable;
			int x, y;
			int alpha;
			int aShift = 15;
			int rShift = 10;
			int gShift = 5;
			int bShift = 0;
			Bitmap decodedImage = new Bitmap(width, height);
			Color colour1, colour2, colour3, colour4;

			//initialize the twiddle table
			twiddleTable = initTwiddleTable();

			//read the codebook table values in from the raw data
			codeBookTable = new Color[4 * 256];
			for (x = 0; x < 1024; x++)
			{
				ushort c = codeBook[x];
				alpha = ((c >> aShift) & 1);
				if (alpha > 0)
				{
					alpha = 255;
				}
				codeBookTable[x] = Color.FromArgb(alpha, ((c >> rShift) & 31) << 3, ((c >> gShift) & 31) << 3, ((c >> bShift) & 31) << 3);
			}

			//decode the vq data based on the code book and detwiddle it using the table

			for (y = 0; y < height; y += 2)
			{
				for (x = 0; x < width; x += 2)
				{
					int c = imageData[twiddleTable[y / 2] | (twiddleTable[x / 2] << 1)];
					int baseEntry = c * 4;
					colour1 = codeBookTable[baseEntry];
					colour2 = codeBookTable[baseEntry + 1];
					colour3 = codeBookTable[baseEntry + 2];
					colour4 = codeBookTable[baseEntry + 3];
					decodedImage.SetPixel(x, y, colour1);   //p3
					decodedImage.SetPixel(x + 1, y, colour3);   //p4
					decodedImage.SetPixel(x, y + 1, colour2);   // p1
					decodedImage.SetPixel(x + 1, y + 1, colour4); // p2
				}
			}

			return decodedImage;
		}

		protected int[] initTwiddleTable()
		{
			//based on code by Marcus Tatest by way of BERO
			int[] twiddleTable;
			int x;

			twiddleTable = new int[1024];

			for (x = 0; x < 32; x++)
			{
				twiddleTable[x] = (x & 1) | ((x & 2) << 1) | ((x & 4) << 2) | ((x & 8) << 3) | ((x & 16) << 4);
			}
			for (x = 32; x < 1024; x++)
			{
				twiddleTable[x] = twiddleTable[x & 31] | (twiddleTable[(x >> 5) & 31] << 10);
			}

			return twiddleTable;
		}

		public override MemoryStream GetDataAsStream(int index)
		{
			Bitmap tex = new Bitmap(1, 1);
			tex = GetTextureAsBitmap(index);
			tex.RotateFlip(RotateFlipType.Rotate90FlipX);

			MemoryStream stream = new MemoryStream();
			tex.Save(stream, ImageFormat.Png);
			return stream;
		}
	}
}
