using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            int index = name.IndexOfAny(new char[] { '\0' });
            if (index >= 0)
            {
                name = name.Substring(0, index);
            }

            return name.Trim();
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
    }
}
