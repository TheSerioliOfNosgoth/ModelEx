using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace BenLincoln.TheLostWorlds.CDTextures
{
    public class SoulReaverPCTextureFile : BenLincoln.TheLostWorlds.CDTextures.SoulReaverMonolithicTextureFile
    {
        public SoulReaverPCTextureFile(string path)
            : base(path)
        {
            _FileType = TextureFileType.SoulReaverPC;
            _FileTypeName = "Soul Reaver (PC) Uncompressed ARGB 1555 Monolithic Texture File";
            _HeaderLength = 4096;
            _TextureLength = 131072;
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
            ushort iGT, jGT;
            ushort a, r, g, b, pixelData;
            int /*aFactor,*/ rFactor, gFactor, bFactor;
            Bitmap retBitmap;
            Color colour;

            //aFactor = 8;
            rFactor = 3;
            gFactor = 3;
            bFactor = 3;

            colour = new Color();
            retBitmap = new Bitmap(256, 256);

            try
            {
                FileStream inStream = new FileStream(_FilePath, FileMode.Open, FileAccess.Read);
                BinaryReader inReader = new BinaryReader(inStream);
                inStream.Seek(offset, SeekOrigin.Begin);

                for (iGT = 0; iGT <= 255; iGT++)
                {
                    for (jGT = 0; jGT <= 255; jGT++)
                    {
                        pixelData = inReader.ReadUInt16();
                        a = pixelData;
                        r = pixelData;
                        g = pixelData;
                        b = pixelData;

                        //separate out the channels
                        a >>= 15;

                        r <<= 1;
                        r >>= 11;

                        g <<= 6;
                        g >>= 11;

                        b <<= 11;
                        b >>= 11;

                        if (a > 0)
                        {
                            a = (ushort)255;
                        }
                        r = (ushort)(r << rFactor);
                        g = (ushort)(g << gFactor);
                        b = (ushort)(b << bFactor);

                        colour = Color.FromArgb(a, r, g, b);
                        retBitmap.SetPixel(jGT, iGT, colour);
                    }
                }
                inReader.Close();
                inStream.Close();
            }
            catch (Exception ex)
            {
                throw new TextureFileException("Error reading the specified texture.", ex);
            }

            return retBitmap;
        }

        public override MemoryStream GetDataAsStream(int index)
        {
            Bitmap tex = new Bitmap(1, 1);
            tex = GetTextureAsBitmap(index);

            MemoryStream stream = new MemoryStream();
            tex.Save(stream, ImageFormat.Png);
            return stream;
        }
    }
}
