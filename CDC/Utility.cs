using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace CDC
{
	public class Utility
	{
		public static String CleanName(String name)
		{
			if (name == null)
			{
				return "";
			}

			 name = name.ToLower();

			int index = name.IndexOfAny(new char[] { '\0' });
			if (index >= 0)
			{
				name = name.Substring(0, index);
			}

			return name.Trim();
		}

		public static string CleanObjectName(string name)
		{
			string trimmedName = CleanName(name);
			return trimmedName.TrimEnd(new char[] { '_' });
		}

		public static void FlipRedAndBlue(ref UInt32 colour)
		{
			UInt32 tempColour = colour;
			colour =
				(tempColour & 0xFF000000) |
				((tempColour << 16) & 0x00FF0000) |
				(tempColour & 0x0000FF00) |
				((tempColour >> 16) & 0x000000FF);
			return;
		}

		public static float BizarreFloatToNormalFloat(UInt16 usBizarreFloat)
		{
			//UInt32 usSign = (UInt32)usBizarreFloat & 0x00008000;
			//usSign >>= 15;
			//UInt32 usExponent = (UInt32)usBizarreFloat & 0x00007C00;
			//usExponent >>= 10;
			//usExponent -= 15;
			//UInt32 usSignificand = (UInt32)usBizarreFloat & 0x000003FF;
			//float fFraction = 1f;
			//for (int i = 0; i < 10; i++)
			//{
			//    UInt32 usCurrent = usSignificand;
			//    usCurrent = (byte)(usCurrent << (i + 1));
			//    usCurrent = (byte)(usCurrent >> 10);
			//    fFraction += (float)((float)usCurrent * Math.Pow(2, 0 - (1 + i)));
			//}
			//float fCalcValue = (float)(fFraction * Math.Pow(2, (double)usExponent));
			//return fCalcValue;

			// Converts the 16-bit floating point values used in the DC version of Soul Reaver to normal 32-bit floats
			ushort usExponent;
			int iUnbiasedExponent;
			ushort usSignificand;
			bool bPositive = true;
			ushort usSignCheck = usBizarreFloat;
			usSignCheck = (ushort)(usSignCheck >> 15);
			if (usSignCheck != 0)
			{
				bPositive = false;
			}

			usExponent = usBizarreFloat;
			usExponent = (ushort)(usExponent << 1);
			usExponent = (ushort)(usExponent >> 8);
			iUnbiasedExponent = usExponent - 127;
			usSignificand = usBizarreFloat;
			usSignificand = (ushort)(usSignificand << 9);
			usSignificand = (ushort)(usSignificand >> 9);
			float fFraction = 1f;
			for (int i = 0; i < 7; i++)
			{
				byte cCurrent = (byte)usSignificand;
				cCurrent = (byte)(cCurrent << (i + 1));
				cCurrent = (byte)(cCurrent >> 7);
				fFraction += (float)((float)cCurrent * Math.Pow(2, 0 - (1 + i)));
			}
			float fCalcValue = (float)(fFraction * Math.Pow(2, (double)iUnbiasedExponent));
			if (!bPositive)
			{
				fCalcValue *= -1f;
			}
			return fCalcValue;
		}

		public static float BizarreFloatToNormalFloat2(UInt16 usBizarreFloat)
		{
			UInt32 floatAsInt = ((UInt32)usBizarreFloat) << 16;
			byte[] bytes = BitConverter.GetBytes(floatAsInt);
			float result = BitConverter.ToSingle(bytes, 0);

			return result;
		}

		public static float[] UInt32ARGBToFloatARGB(UInt32 argb)
		{
			float[] result = new float[4];

			result[0] = (float)((argb & 0xFF000000) >> 24) / 255.0f;
			result[1] = (float)((argb & 0x00FF0000) >> 16) / 255.0f;
			result[2] = (float)((argb & 0x0000FF00) >> 8) / 255.0f;
			result[3] = (float)((argb & 0x000000FF)) / 255.0f;

			return result;
		}

		public static UInt32 FloatARGBToUInt32ARGB(float[] argb)
		{
			UInt32 result;

			result = ((uint)(Math.Round(argb[0] * 255.0f))) << 24;
			result |= ((uint)(Math.Round(argb[1] * 255.0f))) << 16;
			result |= ((uint)(Math.Round(argb[2] * 255.0f))) << 8;
			result |= ((uint)(Math.Round(argb[3] * 255.0f)));

			return result;
		}

		public static float ClampToRange(float fValue, float fMin, float fMax)
		{
			if (fValue < fMin)
			{
				return fMin;
			}
			if (fValue > fMax)
			{
				return fMax;
			}
			return fValue;
		}

		public static float WraparoundUVValues(float fValue, float fMin, float fMax)
		{
			float result = fValue;
			if (fValue < fMin)
			{
				while (fValue < fMin)
				{
					result = result + fMax;
				}
			}
			if (fValue > fMax)
			{
				result = result % fMax;
			}
			return result;
		}

		public static void AdjustUVs(ref UV xUV, float fCentreU, float fCentreV, float fSizeAdjust, float fOffsetAdjust)
		{
			if (fCentreU < xUV.u)
			{
				xUV.u = Math.Max(fCentreU, xUV.u - fSizeAdjust);
			}
			if (fCentreU > xUV.u)
			{
				xUV.u = Math.Min(fCentreU, xUV.u + fSizeAdjust);
			}
			xUV.u = ClampToRange(xUV.u + fOffsetAdjust, 0.0f, 255.0f);
			//xUV.u = WraparoundUVValues(xUV.u + fOffsetAdjust, 0.0f, 255.0f);

			if (fCentreV < xUV.v)
			{
				xUV.v = Math.Max(fCentreV, xUV.v - fSizeAdjust);
			}
			if (fCentreV > xUV.v)
			{
				xUV.v = Math.Min(fCentreV, xUV.v + fSizeAdjust);
			}
			xUV.v = ClampToRange(xUV.v + fOffsetAdjust, 0.0f, 255.0f);
			//xUV.v = WraparoundUVValues(xUV.v + fOffsetAdjust, 0.0f, 255.0f);
		}

		public static string GetTextureFileLocation(CDC.Objects.ExportOptions options, string defaultTextureFileName, string modelFileName)
		{
			string result = "";
			List<string> possibleLocations = new List<string>();
			for (int i = 0; i < options.TextureFileLocations.Count; i++)
			{
				possibleLocations.Add(options.TextureFileLocations[i]);
			}

			List<string> searchDirectories = new List<string>();

			string rootDirectory = Path.GetDirectoryName(modelFileName);
			while (rootDirectory != null && rootDirectory != "")
			{
				string parentDirectory = Path.GetFileName(rootDirectory);
				rootDirectory = Path.GetDirectoryName(rootDirectory);
				if (parentDirectory == "kain2")
				{
					string outputDirectory = Path.Combine(rootDirectory, "output");
					searchDirectories.Add(outputDirectory);
					searchDirectories.Add(rootDirectory);
				}
			}

			searchDirectories.Add(Path.GetDirectoryName(modelFileName));
			searchDirectories.Add(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));

			for (int i = 0; i < searchDirectories.Count; i++)
			{
				string textureFileName = Path.Combine(searchDirectories[i], defaultTextureFileName);
				possibleLocations.Add(textureFileName);
			}

			for (int i = 0; i < possibleLocations.Count; i++)
			{
				if (System.IO.File.Exists(possibleLocations[i]))
				{
					result = possibleLocations[i];
					Console.WriteLine(string.Format("Debug: using texture file '{0}'", result));
					break;
				}
			}
			return result;
		}
	}
}
