using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace BenLincoln.Playstation2
{
	public class PS2ImageData
	{
		public static int[] BuildIDTex8ClutIndex()
		{
			int[] clutIndex = new int[256];
			int currentIndex = 0;
			for (int i = 0; i < 8; i++)
			{
				for (int j = 0; j < 8; j++)
				{
					clutIndex[(i * 32) + j] = currentIndex;
					currentIndex++;
				}
				for (int j = 0; j < 8; j++)
				{
					clutIndex[(i * 32) + 16 + j] = currentIndex;
					currentIndex++;
				}
				for (int j = 0; j < 8; j++)
				{
					clutIndex[(i * 32) + 8 + j] = currentIndex;
					currentIndex++;
				}
				for (int j = 0; j < 8; j++)
				{
					clutIndex[(i * 32) + 24 + j] = currentIndex;
					currentIndex++;
				}
			}
			return clutIndex;
		}

		public static byte[] PS2DefianceUnSwizzle(byte[] inData, int imgWidth, int imgHeight, int imgBPP)
		{
			//return inData;
			byte[] outData;
			outData = new byte[inData.Length];

			Hashtable map1 = new Hashtable();
			Hashtable map2 = new Hashtable();

			map1.Add(0, new Point(0, 0));
			map1.Add(1, new Point(4, 1));
			map1.Add(2, new Point(8, 0));
			map1.Add(3, new Point(12, 1));
			map1.Add(4, new Point(1, 0));
			map1.Add(5, new Point(5, 1));
			map1.Add(6, new Point(9, 0));
			map1.Add(7, new Point(13, 1));
			map1.Add(8, new Point(2, 0));
			map1.Add(9, new Point(6, 1));
			map1.Add(10, new Point(10, 0));
			map1.Add(11, new Point(14, 1));
			map1.Add(12, new Point(3, 0));
			map1.Add(13, new Point(7, 1));
			map1.Add(14, new Point(11, 0));
			map1.Add(15, new Point(15, 1));
			map1.Add(16, new Point(4, 0));
			map1.Add(17, new Point(0, 1));
			map1.Add(18, new Point(12, 0));
			map1.Add(19, new Point(8, 1));
			map1.Add(20, new Point(5, 0));
			map1.Add(21, new Point(1, 1));
			map1.Add(22, new Point(13, 0));
			map1.Add(23, new Point(9, 1));
			map1.Add(24, new Point(6, 0));
			map1.Add(25, new Point(2, 1));
			map1.Add(26, new Point(14, 0));
			map1.Add(27, new Point(10, 1));
			map1.Add(28, new Point(7, 0));
			map1.Add(29, new Point(3, 1));
			map1.Add(30, new Point(15, 0));
			map1.Add(31, new Point(11, 1));

			map2.Add(0, new Point(4, 0));
			map2.Add(1, new Point(0, 1));
			map2.Add(2, new Point(12, 0));
			map2.Add(3, new Point(8, 1));
			map2.Add(4, new Point(5, 0));
			map2.Add(5, new Point(1, 1));
			map2.Add(6, new Point(13, 0));
			map2.Add(7, new Point(9, 1));
			map2.Add(8, new Point(6, 0));
			map2.Add(9, new Point(2, 1));
			map2.Add(10, new Point(14, 0));
			map2.Add(11, new Point(10, 1));
			map2.Add(12, new Point(7, 0));
			map2.Add(13, new Point(3, 1));
			map2.Add(14, new Point(15, 0));
			map2.Add(15, new Point(11, 1));
			map2.Add(16, new Point(0, 0));
			map2.Add(17, new Point(4, 1));
			map2.Add(18, new Point(8, 0));
			map2.Add(19, new Point(12, 1));
			map2.Add(20, new Point(1, 0));
			map2.Add(21, new Point(5, 1));
			map2.Add(22, new Point(9, 0));
			map2.Add(23, new Point(13, 1));
			map2.Add(24, new Point(2, 0));
			map2.Add(25, new Point(6, 1));
			map2.Add(26, new Point(10, 0));
			map2.Add(27, new Point(14, 1));
			map2.Add(28, new Point(3, 0));
			map2.Add(29, new Point(7, 1));
			map2.Add(30, new Point(11, 0));
			map2.Add(31, new Point(15, 1));

			int[] map = new int[]
			{

				0,  1,  2,  3,  4,  5,  6,  7,  8,  9,  10, 11, 12, 13, 14, 15,
				16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31
			};

			int inPos = 0;

			for (int y = 0; y < imgHeight; y += 8)
			{
				for (int x = 0; x < imgWidth; x += 16)
				{
					for (int i = 0; i < 32; i++)
					{
						int yPos = y;
						int xPos = x;

						Point mapPoint = (Point)map1[map[i]];

						xPos += mapPoint.X;
						yPos += mapPoint.Y;

						outData[(yPos * imgWidth) + xPos] = inData[inPos];
						inPos++;
					}
				}
				for (int x = 0; x < imgWidth; x += 16)
				{
					for (int i = 0; i < 32; i++)
					{
						int yPos = y + 2;
						int xPos = x;

						Point mapPoint = (Point)map1[map[i]];

						xPos += mapPoint.X;
						yPos += mapPoint.Y;

						outData[(yPos * imgWidth) + xPos] = inData[inPos];
						inPos++;
					}
				}
				for (int x = 0; x < imgWidth; x += 16)
				{
					for (int i = 0; i < 32; i++)
					{
						int yPos = y + 4;
						int xPos = x;

						Point mapPoint = (Point)map2[map[i]];

						xPos += mapPoint.X;
						yPos += mapPoint.Y;

						outData[(yPos * imgWidth) + xPos] = inData[inPos];
						inPos++;
					}
				}
				for (int x = 0; x < imgWidth; x += 16)
				{
					for (int i = 0; i < 32; i++)
					{
						int yPos = y + 6;
						int xPos = x;

						Point mapPoint = (Point)map2[map[i]];

						xPos += mapPoint.X;
						yPos += mapPoint.Y;

						outData[(yPos * imgWidth) + xPos] = inData[inPos];
						inPos++;
					}
				}
			}

			//flip rows
			for (int y = 0; y < imgHeight; y += 4)
			{
				byte[] rowBuffer = new byte[imgWidth];
				//copy second row to buffer
				for (int i = 0; i < imgWidth; i++)
				{
					rowBuffer[i] = outData[((y + 1) * imgWidth) + i];
				}
				//copy third row to second
				for (int i = 0; i < imgWidth; i++)
				{
					outData[((y + 1) * imgWidth) + i] = outData[((y + 2) * imgWidth) + i];
				}
				//copy buffer to third row
				for (int i = 0; i < imgWidth; i++)
				{
					outData[((y + 2) * imgWidth) + i] = rowBuffer[i];
				}
			}

			return outData;
		}

		public static byte[] UnSwizzleImageBytes(byte[] inData, int imgWidth, int imgHeight, int imgBPP)
		{
			byte[] outData;
			outData = new byte[inData.GetUpperBound(0) + 1];

			int colCount = imgWidth / 16 * (imgBPP / 8);
			int rowCount = imgHeight / 8;

			int currentInPos = 0;
			int currentOutPos = 0;

			for (int i = 0; i < rowCount; i++)
			{
				for (int j = 0; j < colCount; j++)
				{
					for (int k = 0; k < 128; k++)
					{
						currentOutPos = (i * colCount * 128) + (j * 16) + (k % 16) + ((k / 16) * colCount * 16);
						outData[currentOutPos] = inData[currentInPos];
						currentInPos++;
					}
				}
			}
			return outData;
		}

		public static byte[] RGB565SwapRandB(byte[] inData)
		{
			byte[] outData = new byte[inData.Length];
			if ((inData.Length % 2) == 1)
			{
				throw new ArgumentException("The number of bytes in the array must be even.");
			}
			for (int i = 0; i < inData.Length; i += 2)
			{
				int left1 = inData[i] & 0x1F;
				int left2 = inData[i] & 0xE0;
				int right1 = inData[i + 1] & 0xF8;
				int right2 = inData[i + 1] & 0x07;

				left1 <<= 3;
				right1 >>= 3;

				outData[i] = (byte)(right1 | left2);
				outData[i + 1] = (byte)(right2 | left1);
			}
			return outData;
		}
	}
}
