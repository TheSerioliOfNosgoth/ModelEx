using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace BenLincoln.TheLostWorlds.CDTextures
{
    public class Gex3PlaystationVRMTextureFile : BenLincoln.TheLostWorlds.CDTextures.TextureFile
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

        public struct Gex3PlaystationPolygonTextureData
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
        protected ushort[] _TextureData; // Get rid of this once I know what I'm doing with the palletes.
        protected Bitmap[] _Textures;
        protected int _TotalWidth;
        protected int _TotalHeight;
        protected Dictionary<int, Dictionary<ushort, Bitmap>> _TexturesByCLUT;
        protected readonly int _HeaderLength = 20;
        protected readonly int _ImageWidth = 256;
        protected readonly int _ImageHeight = 256;

        public Dictionary<int, Dictionary<ushort, Bitmap>> TexturesByCLUT { get { return _TexturesByCLUT; } }

        public Gex3PlaystationVRMTextureFile(string path)
            : base(path)
        {
            _FileType = TextureFileType.Gex3Playstation;
            _FileTypeName = "Gex 3 (Playstation) VRM";

            _TotalWidth = 512;
            _TotalHeight = 512;

            _TextureData = LoadTextureData();
            _TexturesByCLUT = new Dictionary<int, Dictionary<ushort, Bitmap>>();
        }

        protected override int _GetTextureCount()
        {
            return _TextureCount;
        }

        protected ushort[] LoadTextureData()
        {
            ushort[] data;

            try
            {
                FileStream inStream = new FileStream(_FilePath, FileMode.Open, FileAccess.Read);
                BinaryReader inReader = new BinaryReader(inStream);

                data = new ushort[_TotalWidth * _TotalHeight];

                inStream.Seek(_HeaderLength, SeekOrigin.Begin);

                for (int i = 0; i < data.Length; i++)
                {
                    data[i] = inReader.ReadUInt16();
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

        protected TexturePage GetTexturePage(ushort tPage)
        {

            TexturePage texturePage = new TexturePage
            {
                tPage = tPage,
                tp = (tPage >> 7) & 0x001,//(tPage >> 7) & 0x003,
                abr = 0,//(tPage >> 5) & 0x003,
                x = (tPage << 6) & 0x1c0,//(tPage << 6) & 0x7c0,
                y = (tPage << 4) & 0x100,//((tPage << 4) & 0x100) + ((tPage >> 2) & 0x200),
                pixels = null
            };

            ushort[,] pixels = new ushort[_ImageHeight, _ImageWidth];

            int scaledX = texturePage.x;
            int scaledY = texturePage.y;

            for (int y = 0; y < _ImageHeight; y++)
            {
                int yOffset = (scaledY + y) * _TotalWidth;
                for (int x = 0; x < _ImageWidth;)
                {
                    int xOffset = scaledX; // / 2;

                    try
                    {
                        if (texturePage.tp == 0) // 4 bit
                        {
                            xOffset += x / 4;
                            ushort val = _TextureData[yOffset + xOffset];
                            pixels[y, x++] = (ushort)(val & 0x000F);
                            pixels[y, x++] = (ushort)((val & 0x00F0) >> 4);
                            pixels[y, x++] = (ushort)((val & 0x0F00) >> 8);
                            pixels[y, x++] = (ushort)((val & 0xF000) >> 12);
                        }
                        else if (texturePage.tp == 1) // 8 bit
                        {
                            xOffset += x;
                            xOffset /= 2;
                            ushort val = _TextureData[yOffset + xOffset];
                            pixels[y, x++] = (ushort)(val & 0x00FF);
                            pixels[y, x++] = (ushort)((val & 0xFF00) >> 8);
                        }
                        else if (texturePage.tp == 2) // 16 bit
                        {
                            xOffset += x;
                            ushort val = _TextureData[yOffset + xOffset];
                            pixels[y, x++] = val;
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }

            texturePage.pixels = pixels;
            return texturePage;
        }

        protected ColourTable GetTextureClut(ushort clut, int length)
        {
            ColourTable colourTable = new ColourTable
            {
                clut = clut,
                x = (clut & 0x1F) << 4,//(clut & 0x3F) << 4,
                y = (clut >> 6) & 0x1FF,
                colours = null
            };

            Color[] colours = new Color[length];

            int scaledX = colourTable.x;
            int scaledY = colourTable.y;

            int yOffset = scaledY * _TotalWidth;
            for (int x = 0; x < length;)
            {
                int xOffset = scaledX;

                try
                {
                    xOffset += x;
                    ushort val = _TextureData[yOffset + xOffset];

                    ushort alpha = 255;
                    ushort red = val;
                    ushort green = val;
                    ushort blue = val;

                    blue = (ushort)(((ushort)(val << 1) >> 11) << 3);
                    green = (ushort)(((ushort)(val << 6) >> 11) << 3);
                    red = (ushort)(((ushort)(val << 11) >> 11) << 3);

                    alpha = (ushort)(val >> 15);
                    if (alpha != 0 || blue != 0 || green != 0 || red != 0)
                    {
                        alpha = 255;
                    }
                    else
                    {
                        alpha = 0;
                    }


                    colours[x++] = Color.FromArgb(alpha, red, green, blue);
                }
                catch (Exception ex)
                {
                }
            }

            colourTable.colours = colours;
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

        public void BuildTexturesFromGreyscalePallete(uint[] texturePages)
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
                //_Textures = new Bitmap[_TPages.Length];
            }

            for (int i = 0; i < _TextureCount; i++)
            {
                //_Textures[i] = GetTextureAsBitmap(i, GetGreyscalePalette(i));
            }
        }

        public void BuildTexturesFromPolygonData(uint[] texturePages, bool drawGreyScaleFirst, CDC.Objects.ExportOptions options)
        {
            if (_TPages == null)
            {
                List<TexturePage> tPages = new List<TexturePage>();
                List<Bitmap> textures = new List<Bitmap>();

                for (int i = 0; i < texturePages.Length; i++)
                {
                    ushort tPage = (ushort)(texturePages[i] & 0x0000FFFFu);
                    ushort clut = (ushort)((texturePages[i] >> 16) & 0x0000FFFFu);
                    TexturePage texturePage = tPages.Find(x => x.tPage == tPage);
                    if (texturePage == null)
                    {
                        texturePage = GetTexturePage(tPage);
                        tPages.Add(texturePage);
                    }

                    if (!_TexturesByCLUT.ContainsKey(tPage))
                    {
                        _TexturesByCLUT.Add(tPage, new Dictionary<ushort, Bitmap>());
                    }

                    if (!_TexturesByCLUT[tPage].ContainsKey(clut))
                    {
                        Color[] palette = GetTextureClut(clut, texturePage.tp == 0 ? 16 : 256).colours;
                        Bitmap texture = new Bitmap(_ImageWidth, _ImageHeight);

                        for (int y = 0; y < _ImageHeight; y++)
                        {
                            for (int x = 0; x < _ImageWidth; x += 2)
                            {
                                int leftPixel = texturePage.pixels[y, x];
                                int rightPixel = texturePage.pixels[y, x + 1];
                                texture.SetPixel(x, y, palette[leftPixel]);
                                texture.SetPixel(x + 1, y, palette[rightPixel]);
                            }
                        }

                        AddTextureWithCLUT(tPage, clut, texture);
                        textures.Add(texture);
                    }
                }

                _TPages = tPages.ToArray();
                _Textures = textures.ToArray();
                _TextureCount = _Textures.Length;
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

        public override MemoryStream GetDataAsStream(int index)
        {
            Bitmap tex = _Textures[index];
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
