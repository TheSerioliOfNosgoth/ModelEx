using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;

namespace CDC
{
	public class Utility
	{
		public static string CleanName(string name)
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

		public static void FlipRedAndBlue(ref uint colour)
		{
			uint tempColour = colour;
			colour =
				(tempColour & 0xFF000000) |
				((tempColour << 16) & 0x00FF0000) |
				(tempColour & 0x0000FF00) |
				((tempColour >> 16) & 0x000000FF);
			return;
		}

		public static float BizarreFloatToNormalFloat(ushort usBizarreFloat)
		{
			//uint usSign = (uint)usBizarreFloat & 0x00008000;
			//usSign >>= 15;
			//uint usExponent = (uint)usBizarreFloat & 0x00007C00;
			//usExponent >>= 10;
			//usExponent -= 15;
			//uint usSignificand = (uint)usBizarreFloat & 0x000003FF;
			//float fFraction = 1f;
			//for (int i = 0; i < 10; i++)
			//{
			//    uint usCurrent = usSignificand;
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

		public static float BizarreFloatToNormalFloat2(ushort usBizarreFloat)
		{
			uint floatAsInt = ((uint)usBizarreFloat) << 16;
			byte[] bytes = BitConverter.GetBytes(floatAsInt);
			float result = BitConverter.ToSingle(bytes, 0);

			return result;
		}

		public static float[] UInt32ARGBToFloatARGB(uint argb)
		{
			float[] result = new float[4];

			result[0] = (float)((argb & 0xFF000000) >> 24) / 255.0f;
			result[1] = (float)((argb & 0x00FF0000) >> 16) / 255.0f;
			result[2] = (float)((argb & 0x0000FF00) >> 8) / 255.0f;
			result[3] = (float)((argb & 0x000000FF)) / 255.0f;

			return result;
		}

		public static uint FloatARGBToUInt32ARGB(float[] argb)
		{
			uint result;

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

		public static string GetTextureFileLocation(ExportOptions options, string defaultTextureFileName, string modelFileName)
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

		public static string GetTextureNameDefault(string objectName, int textureID)
		{
			string textureName = string.Format("{0}_{1:X4}", objectName.TrimEnd(new char[] { '_' }).ToLower(), textureID);
			return textureName;
		}

		public static string GetPlayStationTextureNameDefault(string objectName, int textureID)
		{
			return GetTextureNameDefault(objectName, textureID);
		}

		public static string GetPlayStationTextureNameWithCLUT(string objectName, int textureID, ushort clut)
		{
			string textureName = string.Format("{0}_{1:X4}_{2:X4}", objectName.TrimEnd(new char[] { '_' }).ToLower(), textureID, clut);
			return textureName;
		}

		public static string GetSoulReaverPCOrDreamcastTextureName(string objectName, int textureID)
		{
			return GetTextureNameDefault(objectName, textureID);
		}

		public static string GetPS2TextureName(string objectName, int textureID)
		{
			return GetTextureNameDefault(objectName, textureID);
		}

		public static void ColourPolygonFromFlags(ref Polygon polygon, uint flags, uint redBit, uint greenBit, uint blueBit)
		{
			if ((flags & redBit) == redBit)
			{
				polygon.material.colour |= 0x00FF0000;
			}
			if ((flags & greenBit) == greenBit)
			{
				polygon.material.colour |= 0x0000FF00;
			}
			if ((flags & blueBit) == blueBit)
			{
				polygon.material.colour |= 0x000000FF;
			}
		}

		public static uint GetColourFromHash(byte[] hash)
		{
			uint result = 0xFF000000;
			result |= ((uint)hash[0] << 16);
			result |= ((uint)hash[1] << 8);
			result |= ((uint)hash[2]);
			return result;
		}

		public static void ColourPolygonFromHash(ref Polygon polygon, byte[] hash)
		{
			//polygon.material.colour &= 0xFF00FFFF;
			//polygon.material.colour |= ((uint)hash[0] << 16);
			//polygon.material.colour &= 0xFFFF00FF;
			//polygon.material.colour |= ((uint)hash[1] << 8);
			//polygon.material.colour &= 0xFFFFFF00;
			//polygon.material.colour |= ((uint)hash[2]);
			polygon.material.colour = GetColourFromHash(hash);
		}

		public static byte[] GetHashOfUInt(uint value)
		{
			byte[] valueBytes = new byte[4];
			valueBytes[0] = (byte)((value & 0xFF000000) >> 24);
			valueBytes[1] = (byte)((value & 0x00FF0000) >> 16);
			valueBytes[2] = (byte)((value & 0x0000FF00) >> 8);
			valueBytes[3] = (byte)((value & 0x000000FF));
			byte[] hash = new MD5CryptoServiceProvider().ComputeHash(valueBytes);
			return hash;
		}

		public static void ColourPolygonFromUInt(ref Polygon polygon, uint value)
		{
			ColourPolygonFromHash(ref polygon, GetHashOfUInt(value));
		}

		public static void ColourPolygonFromString(ref Polygon polygon, string value)
		{
			string hashInput = "default";
			if (value != null)
			{
				hashInput = value;
			}
			byte[] valueBytes = System.Text.Encoding.Unicode.GetBytes(hashInput);
			byte[] hash = new MD5CryptoServiceProvider().ComputeHash(valueBytes);
			ColourPolygonFromHash(ref polygon, hash);
		}
	}
}
