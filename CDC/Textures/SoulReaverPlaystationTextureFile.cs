using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace BenLincoln.TheLostWorlds.CDTextures
{
    public class SoulReaverPlaystationTextureFile : BenLincoln.TheLostWorlds.CDTextures.TextureFile
    {
        protected byte[][] _TextureData;
        protected Bitmap[] _Textures;
        protected int _TotalWidth;
        protected int _TotalHeight;
        protected Dictionary<int, Dictionary<ushort, Bitmap>> _TexturesByCLUT;

        public Dictionary<int, Dictionary<ushort, Bitmap>> TexturesByCLUT
        {
            get
            {
                return _TexturesByCLUT;
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

        public struct SoulReaverPlaystationPolygonTextureData
        {
            public int[] u;               // 0-255 each
            public int[] v;               // 0-255 each 
            public int textureID;          // 0-8
            public ushort CLUT;
            public int paletteColumn;      // 0-32
            public int paletteRow;         // 0-255
            public uint materialColour;
            public bool textureUsed;
            public bool visible;
        }

        public SoulReaverPlaystationTextureFile(string path)
            : base(path)
        {
            _FileType = TextureFileType.SoulReaverPlaystation;
            _FileTypeName = "Soul Reaver (Playstation) Indexed Texture File";

            Point size = GetImageSize();
            _TotalWidth = size.X;
            _TotalHeight = size.Y;

            _TextureCount = _GetTextureCount();
            _Textures = new Bitmap[_TextureCount];
            _TextureData = ReadTextureData(LoadTextureData());
            _TexturesByCLUT = new Dictionary<int, Dictionary<ushort, Bitmap>>();
        }

        protected override int _GetTextureCount()
        {
            if (_FileInfo.Length < (36))
            {
                throw new TextureFileException("The file '" + _FilePath + "' does not contain enough data for it to be " +
                    "a texture file for the Playstation version of Soul Reaver.");
            }
            long textureCountLong;
            float textureCountFloat;
            textureCountLong = (_FileInfo.Length - 36) / 32768;
            textureCountFloat = ((float)_FileInfo.Length - 36f) / 32768f;
            if ((float)textureCountLong != textureCountFloat)
            {
                textureCountLong++;
            }
            return (int)textureCountLong;
        }

        protected byte[] LoadTextureData()
        {
            byte[] data;

            try
            {
                FileStream inStream = new FileStream(_FilePath, FileMode.Open, FileAccess.Read);
                BinaryReader inReader = new BinaryReader(inStream);

                data = new byte[(_TotalWidth / 2) * _TotalHeight];

                inStream.Seek(36, SeekOrigin.Begin);

                for (int i = 0; i < data.Length; i++)
                {
                    data[i] = inReader.ReadByte();
                }

                inReader.Close();
                inStream.Close();
            }
            catch (Exception ex)
            {
                throw new TextureFileException("Error reading texture.", ex);
            }

            return data;
        }

        protected byte[][] ReadTextureData(byte[] inData)
        {
            int numTextures = (inData.Length / 32768);
            int remainder = inData.Length % 32768;
            if (remainder > 0)
            {
                numTextures++;
            }
            byte[][] textures = new byte[numTextures][];
            for (int textureNum = 0; textureNum < numTextures; textureNum++)
            {
                textures[textureNum] = new byte[32768];
            }
            int byteNum = 0;
            for (int y = 0; y < _TotalHeight; y++)
            {
                for (int textureNum = 0; textureNum < numTextures; textureNum++)
                {
                    int startX = 128 * textureNum;
                    int endX = startX + 128;
                    int maxX = ((_TotalWidth / 2) - startX);
                    int padX = 0;
                    if (maxX < 128)
                    {
                        padX = 128 - maxX;
                        endX = startX + maxX;
                    }
                    for (int x = startX; x < endX; x++)
                    {
                        textures[textureNum][(y * 128) + (x - startX)] = inData[byteNum];
                        byteNum++;
                    }
                    for (int i = 0; i < padX; i++)
                    {
                        textures[textureNum][(y * 128) + (endX - startX) + i] = (byte)0;
                    }
                }
            }

            return textures;
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

        public void BuildTexturesFromPolygonData(SoulReaverPlaystationPolygonTextureData[] texData, bool drawGreyScaleFirst, bool quantizeBounds, CDC.Objects.ExportOptions options)
        {
            // hashtable to store counts of palette usage
            Hashtable palettes = new Hashtable();

            bool debugTextureCollisions = false;

            // initialize textures
            if (drawGreyScaleFirst)
            {
                for (int i = 0; i < _TextureCount; i++)
                {
                    _Textures[i] = GetTextureAsBitmap(i, GetGreyscalePalette());
                }
            }
            else
            {
                Color chromaKey = Color.FromArgb(1, 128, 128, 128);
                //Color chromaKey = Color.FromArgb(255, 128, 128, 128);
                for (int i = 0; i < _TextureCount; i++)
                {
                    _Textures[i] = new Bitmap(256, 256);
                    for (int y = 0; y < 256; y++)
                    {
                        for (int x = 0; x < 256; x++)
                        {
                            _Textures[i].SetPixel(x, y, chromaKey);
                        }
                    }
                }
            }

            Dictionary<int, Dictionary<int, ulong>> handledPixels = new Dictionary<int, Dictionary<int, ulong>>();
            //Dictionary<int, ulong> handledPixels = new Dictionary<int, ulong>();

            // use the polygon data to colour in all possible parts of the textures
            foreach (SoulReaverPlaystationPolygonTextureData poly in texData)
            {
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
                Color[] palette = GetPalette(poly.paletteColumn, poly.paletteRow);

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
                bool paletteIsValid = true;
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

                // if specified, quantize the rectangle's boundaries 
                //if (quantizeBounds)
                //{
                //    while ((uMin % width) > 0)
                //    {
                //        uMin--;
                //    }
                //    while ((uMax % width) > 0)
                //    {
                //        uMax++;
                //    }
                //    while ((vMin % height) > 0)
                //    {
                //        vMin--;
                //    }
                //    while ((vMax % height) > 0)
                //    {
                //        vMax++;
                //    }
                //    if (uMin < 0)
                //    {
                //        uMin = 0;
                //    }
                //    if (uMax > 256)
                //    {
                //        uMax = 256;
                //    }
                //    if (vMin < 0)
                //    {
                //        vMin = 0;
                //    }
                //    if (vMax > 256)
                //    {
                //        vMax = 256;
                //    }
                //}

                //// if specified, quantize the rectangle's boundaries to a multiple of 16
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
                    for (int x = uMin; x < uMax; x+= 2)
                    {
                        int dataOffset = (y * 128) + (x / 2);
                        byte currentByte = _TextureData[poly.textureID][dataOffset];
                        int leftPixel = (int)currentByte & 0x0F;
                        int rightPixel = (int)((currentByte & 0xF0) >> 4);
                        Color leftPixelColour = palette[leftPixel];
                        Color rightPixelColour = palette[rightPixel];
                        //uint materialAlpha = (poly.materialColour & 0xFF000000) >> 24;
                        ////materialAlpha = 255;
                        //if (materialAlpha < 255)
                        //{
                        //    leftPixelColour = Color.FromArgb((byte)materialAlpha, palette[leftPixel].R, palette[leftPixel].G, palette[leftPixel].B);
                        //    rightPixelColour = Color.FromArgb((byte)materialAlpha, palette[rightPixel].R, palette[leftPixel].G, palette[leftPixel].B);
                        //}
                        //ulong checkPixels = ((ulong)materialAlpha << 56) | ((ulong)palette[leftPixel].R << 48) | ((ulong)palette[leftPixel].G << 40)
                        //    | ((ulong)palette[leftPixel].B << 32) | ((ulong)materialAlpha << 24) | ((ulong)palette[rightPixel].R << 16)
                        //    | ((ulong)palette[rightPixel].G << 8) | ((ulong)palette[rightPixel].B);
                        ulong checkPixels = ((ulong)palette[leftPixel].A << 56) | ((ulong)palette[leftPixel].R << 48) | ((ulong)palette[leftPixel].G << 40)
                            | ((ulong)palette[leftPixel].B << 32) | ((ulong)palette[rightPixel].A << 24) | ((ulong)palette[rightPixel].R << 16)
                            | ((ulong)palette[rightPixel].G << 8) | ((ulong)palette[rightPixel].B);
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
                            _Textures[poly.textureID].SetPixel(x, y, leftPixelColour);
                            _Textures[poly.textureID].SetPixel((x + 1), y, rightPixelColour);
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
                    string paletteIDString = poly.textureID.ToString() + "-" + poly.paletteColumn.ToString() + "-" + poly.paletteRow.ToString();
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

            int texNum = 0;

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
                        int palColumn = int.Parse(palDecode2[1]);
                        int palRow = int.Parse(palDecode2[2]);
                        Color[] pal = GetPalette(palColumn, palRow);

                        for (texNum = 0; texNum <= _Textures.GetUpperBound(0); texNum++)
                        {
                            Bitmap exportTemp = (Bitmap)_Textures[texNum].Clone();
                            bool exportThisTexture = false;
                            for (int y = 0; y < 256; y++)
                            {
                                for (int x = 0; x < 256; x += 2)
                                {
                                    bool leftPixChroma = (exportTemp.GetPixel(x, y).A == 1);
                                    bool rightPixChroma = (exportTemp.GetPixel((x + 1), y).A == 1);
                                    if (leftPixChroma || rightPixChroma)
                                    {
                                        exportThisTexture = true;
                                        int dataOffset = (y * 128) + (x / 2);
                                        byte currentByte = _TextureData[texNum][dataOffset];
                                        int leftPixel = (int)currentByte & 0x0F;
                                        int rightPixel = (int)((currentByte & 0xF0) >> 4);
                                        Color leftPixelColour = pal[leftPixel];
                                        Color rightPixelColour = pal[rightPixel];
                                        if (leftPixChroma)
                                        {
                                            exportTemp.SetPixel(x, y, leftPixelColour);
                                        }
                                        if (rightPixChroma)
                                        {
                                            exportTemp.SetPixel((x + 1), y, rightPixelColour);
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
                                string fileName = string.Format(@"Texture_Debugging\Texture-{0:X8}-count_{1:X8}-palette_texture_ID_{2:X8}-row_{3:X8}-column_{4:X8}.png", texNum, useCount, tID, palColumn, palRow);
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
                    int pc = int.Parse(pd[1]);
                    int pr = int.Parse(pd[2]);
                    if (!allTexturesWithPalettes.Contains(txID))
                    {
                        if ((pc != 0) && (pr != 0))
                        {
                            allTexturesWithPalettes.Add(txID);
                        }
                    }
                }



                for (texNum = 0; texNum <= _Textures.GetUpperBound(0); texNum++)
                {
                    // find the most frequently-used palette
                    string palID = "";
                    int palCount = 0;
                    int mostCommonPaletteColumn = 0;
                    int mostCommonPaletteRow = 0;
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
                            int pc = int.Parse(pd[1]);
                            int pr = int.Parse(pd[2]);
                            int pCount = (int)palettes[pID];
                            if (pCount > palCount)
                            {
                                palID = pID;
                                if ((pc != 0) || (pr != 0))
                                {
                                    mostCommonPaletteColumn = pc;
                                    mostCommonPaletteRow = pr;
                                }
                                // don't use palettes with column/row zero or with odd column numbers unless there's no other choice
                                //if ((pc != 0) && (pr != 0) && (pc % 2 == 0))
                                //{
                                //    break;
                                //}
                            }
                        }
                    }
                    Color[] commonPalette = GetPalette(mostCommonPaletteColumn, mostCommonPaletteRow);

                    // use a greyscale palette instead of column 0, row 0, because column 0, row 0 is always garbage that makes 
                    // the texture impossible to view properly, and full of random transparent pixels
                    if (options.AlwaysUseGreyscaleForMissingPalettes || ((mostCommonPaletteColumn == 0) && (mostCommonPaletteRow == 0)))
                    {
                        commonPalette = GetGreyscalePalette();
                    }

                    for (int y = 0; y < 256; y++)
                    {
                        for (int x = 0; x < 256; x += 2)
                        {
                            if (!hasAtLeastOneVisiblePixel)
                            {
                                if (_Textures[texNum].GetPixel(x, y).A > 1)
                                {
                                    hasAtLeastOneVisiblePixel = true;
                                }
                                else if (_Textures[texNum].GetPixel(x + 1, y).A > 1)
                                {
                                    hasAtLeastOneVisiblePixel = true;
                                }
                            }
                            bool leftPixChroma = (_Textures[texNum].GetPixel(x, y).A == 1);
                            bool rightPixChroma = (_Textures[texNum].GetPixel((x + 1), y).A == 1);
                            if (leftPixChroma || rightPixChroma)
                            {
                                int dataOffset = (y * 128) + (x / 2);
                                byte currentByte = _TextureData[texNum][dataOffset];
                                int leftPixel = (int)currentByte & 0x0F;
                                int rightPixel = (int)((currentByte & 0xF0) >> 4);
                                Color leftPixelColour = commonPalette[leftPixel];
                                Color rightPixelColour = commonPalette[rightPixel];
                                if (leftPixChroma)
                                {
                                    _Textures[texNum].SetPixel(x, y, leftPixelColour);
                                }
                                if (rightPixChroma)
                                {
                                    _Textures[texNum].SetPixel((x + 1), y, rightPixelColour);
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
            //return GetTexture(index, GetGreyscalePalette());
            return _Textures[index];
        }

        //public Bitmap GetTexture(int index)
        //{
        //    return _Textures[index];
        //}

        protected System.Drawing.Bitmap GetTextureAsBitmap(int index, Color[] palette)
        {
            Bitmap retBitmap = new Bitmap(256, 256);

            int byteNum = 0;
            for (int y = 0; y < 256; y++)
            {
                for (int x = 0; x < 256; x += 2)
                {
                    byte currentByte = _TextureData[index][byteNum];
                    int leftPixel = (int)currentByte & 0x0F;
                    int rightPixel = (int)((currentByte & 0xF0) >> 4);
                    retBitmap.SetPixel(x, y, palette[leftPixel]);
                    retBitmap.SetPixel(x + 1, y, palette[rightPixel]);
                    byteNum++;
                }
            }

            return retBitmap;
        }

        protected Point GetImageSize()
        {
            Point size;
            try
            {
                FileStream inStream = new FileStream(_FilePath, FileMode.Open, FileAccess.Read);
                BinaryReader inReader = new BinaryReader(inStream);
                inStream.Seek(28, SeekOrigin.Begin);
                int totalWidth = (int)(inReader.ReadUInt32() * 4);
                int totalHeight = (int)inReader.ReadUInt32();
                inReader.Close();
                inStream.Close();
                size = new Point(totalWidth, totalHeight);
            }
            catch (Exception ex)
            {
                throw new TextureFileException("Error getting image size.", ex);
            }
            return size;
        }

        protected Color[] GetGreyscalePalette()
        {
            Color[] greyPalette = new Color[16];
            for (int i = 0; i < 16; i++)
            {
                int luma = 16 * i;  // 256 / 16 = 16
                greyPalette[i] = Color.FromArgb(luma, luma, luma);
            }
            return greyPalette;
        }

        protected Color[] GetPalette(int paletteColumn, int paletteRow)
        {
            Color[] palette = new Color[16];

            int textureID = paletteColumn / 4;
            int localColumn = paletteColumn % 4;

            int realOffset = (localColumn * 32) + (paletteRow * 128);

            for (int i = 0; i < 16; i++)
            {
                int arrayPos = realOffset + (i * 2);
                ushort currentVal = (ushort)(
                    (ushort)(_TextureData[textureID][arrayPos]) |
                    ((ushort)(_TextureData[textureID][arrayPos + 1]) << 8)
                    );

                ushort alpha = 255;
                ushort red = currentVal;
                ushort green = currentVal;
                ushort blue = currentVal;

                alpha = (ushort)(currentVal >> 15);
                if (alpha != 0)
                {
                    alpha = 255;
                }

                blue = (ushort)(((ushort)(currentVal << 1) >> 11) << 3);
                green = (ushort)(((ushort)(currentVal << 6) >> 11) << 3);
                red = (ushort)(((ushort)(currentVal << 11) >> 11) << 3);

                palette[i] = Color.FromArgb(alpha, red, green, blue);
            }

            return palette;
        }

        public override MemoryStream GetDataAsStream(int index)
        {
            Bitmap tex = new Bitmap(1, 1);
            if (_Textures[index] == null)
            {
                tex = GetTextureAsBitmap(index, GetGreyscalePalette());
            }
            else
            {
                tex = _Textures[index];
            }

            MemoryStream stream = new MemoryStream();
            tex.Save(stream, ImageFormat.Png);
            return stream;
        }

        public override MemoryStream GetTextureWithCLUTAsStream(int textureID, ushort clut)
        {
            Bitmap tex = new Bitmap(1, 1);
            if (_TexturesByCLUT.ContainsKey(textureID))
            {
                if (_TexturesByCLUT[textureID].ContainsKey(clut))
                {
                    tex = _TexturesByCLUT[textureID][clut];
                }
                else
                {
                    Console.WriteLine(string.Format("Error: did not find CLUT {0:X4} variation of texture ID {1} - returning the default version of this texture", clut, textureID));
                    return GetDataAsStream(textureID);
                }
            }
            else
            {
                Console.WriteLine(string.Format("Error: did not find any texture-by-CLUT data for texture ID {0} - returning the default version of this texture", textureID));
                return GetDataAsStream(textureID);
            }

            MemoryStream stream = new MemoryStream();
            tex.Save(stream, ImageFormat.Png);
            return stream;
        }

        public override void ExportFile(int index, string outPath)
        {
            Bitmap tex = new Bitmap(1, 1);
            if (_Textures[index] == null)
            {
                tex = GetTextureAsBitmap(index, GetGreyscalePalette());
            }
            else
            {
                tex = _Textures[index];
            }
            tex.Save(outPath, System.Drawing.Imaging.ImageFormat.Png);
        }
    }
}
