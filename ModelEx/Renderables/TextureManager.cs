using System;
using System.Collections.Generic;
using SlimDX;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
//using FreeImageAPI;
using System.Drawing;
using System.IO;

namespace ModelEx
{
    public class TextureManager
    {
        private static TextureManager instance = null;
        public static TextureManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new TextureManager();
                }
                return instance;
            }
        }

        private TextureManager()
        {
        }

        Dictionary<string, Texture2D> FileTextureDictionary = new Dictionary<string, Texture2D>();
        Dictionary<string, ShaderResourceView> FileShaderResourceViewDictionary = new Dictionary<string, ShaderResourceView>();

        public string GetLast(string source, int tail_length)
        {
            if (tail_length >= source.Length)
                return source;
            return source.Substring(source.Length - tail_length);
        }

        public Texture2D Texture2DFromBitmap(Bitmap bmp)
        {
            Texture2D texture;

            System.Drawing.Imaging.BitmapData bitmapData = bmp.LockBits(
                new Rectangle(0, 0, bmp.Width, bmp.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);

            DataStream dataStream = new DataStream(bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, true, false);
            DataRectangle dataRectangle = new DataRectangle(bitmapData.Stride, dataStream);

            try
            {
                //Load the texture
                texture = new Texture2D(DeviceManager.Instance.device, new Texture2DDescription()
                {
                    BindFlags = BindFlags.ShaderResource,
                    CpuAccessFlags = CpuAccessFlags.None,
                    Format = Format.B8G8R8A8_UNorm,
                    OptionFlags = ResourceOptionFlags.None,
                    MipLevels = 1,
                    Usage = ResourceUsage.Immutable,
                    Width = bmp.Width,
                    Height = bmp.Height,
                    ArraySize = 1,
                    SampleDescription = new SampleDescription(1, 0)
                }, dataRectangle);
            }
            finally
            {
                //Free bitmap-access resources
                dataStream.Dispose();
                bmp.UnlockBits(bitmapData);
            }

            return texture;
        }

        public Texture2D Texture2DFromDDSFile(SlimDX.Direct3D11.Device device, String fileName)
        {
            Texture2D texture = null;

            try
            {
                DirectDrawSurface dds = new DirectDrawSurface();
                dds.ReadHeader(fileName);

                Format format = Format.Unknown;
                if(dds.Format == DDSFormat.DXT1)
                {
                    format = Format.BC1_UNorm;
                }
                if (dds.Format == DDSFormat.DXT5)
                {
                    format = Format.BC3_UNorm;
                }
                int blockSize = (dds.Format == DDSFormat.DXT1) ? 8 : 16;

                Texture2DDescription desc = new Texture2DDescription();
                desc.BindFlags = BindFlags.ShaderResource;
                desc.CpuAccessFlags = CpuAccessFlags.None;
                desc.Format = format;
                desc.OptionFlags = ResourceOptionFlags.None;
                desc.MipLevels = (int)dds.dxtcMipMapCount + 1;
                desc.Width = (int)dds.dxtcWidth;
                desc.Height = (int)dds.dxtcHeight;
                desc.ArraySize = 1;
                desc.SampleDescription = new SampleDescription(1, 0);
                desc.Usage = ResourceUsage.Default;

                int width = (int)dds.dxtcWidth;
                int height = (int)dds.dxtcHeight;
                int mipMapCount = (!dds.HasMipMaps) ? 1 : (int)dds.dxtcMipMapCount;

                DataRectangle[] data = new DataRectangle[desc.MipLevels];
                for (int i = 0; i < desc.MipLevels; i++)
                {
                    int mipMapWidth = width >> i;
                    int mipMapHeight = height >> i;
                    int mipMapSize = (int)(Math.Max(1, ((mipMapWidth + 3) / 4)) * Math.Max(1, ((mipMapHeight + 3) / 4)) * blockSize);
                    int pitch = (int)(Math.Max(1, ((mipMapWidth + 3) / 4)) * blockSize);

                    DataStream dataStream = new DataStream((long)mipMapSize, true, true);
                    for (int b = 0; b < mipMapSize; b++)
                    {
                        dataStream.Write((byte)dds.data.ReadByte());
                    }
                    // dds.data.CopyTo(data[i].Data, mipMapSize);
                    dataStream.Position = 0;
                    data[i] = new DataRectangle(pitch, dataStream);
                }

                texture = new Texture2D(device, desc, data);

                for (int i = 0; i < desc.MipLevels; i++)
                {
                    data[i].Data.Dispose();
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            return texture;
        }

        public void AddTexture(Stream stream, string fileName)
        {
            try
            {
                if (fileName.Contains("\\"))
                {
                    fileName = System.IO.Path.GetFileName(fileName);
                }

                if (!FileTextureDictionary.ContainsKey(fileName))
                {
                    Texture2D texture;
                    long oldPosition = stream.Position;
                    stream.Position = 0;
                    texture = Texture2D.FromStream(DeviceManager.Instance.device, stream, (int)stream.Length);
                    stream.Position = oldPosition;

                    FileTextureDictionary.Add(fileName, texture);

                    if (!FileShaderResourceViewDictionary.ContainsKey(fileName))
                    {
                        ShaderResourceView textureView = new ShaderResourceView(DeviceManager.Instance.device, texture);
                        FileShaderResourceViewDictionary.Add(fileName, textureView);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public void AddTexture(string path, string fileName)
        {
            try
            {
                if (fileName.Contains("\\"))
                {
                    fileName = System.IO.Path.GetFileName(fileName);
                }

                if (!FileTextureDictionary.ContainsKey(fileName))
                {
                    string fullPath = path + "\\" + fileName;

                    Texture2D texture;
                    string format = GetLast(fullPath, 3);

                    if (File.Exists(fullPath))
                    {
                        //if (format == "dds")
                        //{
                        //    texture = Texture2DFromDDSFile(DeviceManager.Instance.device, fullPath);
                        //}
                        //else if (format == "tga")
                        //{
                        //    FIBITMAP dib = new FIBITMAP();
                        //    dib = FreeImage.Load(FREE_IMAGE_FORMAT.FIF_TARGA, fullPath, FREE_IMAGE_LOAD_FLAGS.DEFAULT);
                        //    Bitmap bmp = FreeImage.GetBitmap(dib);
                        //    bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);
                        //    texture = Texture2DFromBitmap(bmp);
                        //}
                        //else
                        {
                            texture = Texture2D.FromFile(DeviceManager.Instance.device, fullPath);
                        }

                        FileTextureDictionary.Add(fileName, texture);

                        if (!FileShaderResourceViewDictionary.ContainsKey(fileName))
                        {
                            ShaderResourceView textureView = new ShaderResourceView(DeviceManager.Instance.device, texture);
                            FileShaderResourceViewDictionary.Add(fileName, textureView);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public Texture2D GetTexture(string fileName)
        {
            if (fileName.Contains("\\"))
            {
                fileName = System.IO.Path.GetFileName(fileName);
            }

            if (FileTextureDictionary.ContainsKey(fileName))
            {
                return FileTextureDictionary[fileName];
            }

            return null;
        }

        public ShaderResourceView GetShaderResourceView(string fileName)
        {
            if (fileName.Contains("\\"))
            {
                fileName = System.IO.Path.GetFileName(fileName);
            }

            if (FileShaderResourceViewDictionary.ContainsKey(fileName))
            {
                return FileShaderResourceViewDictionary[fileName];
            }

            return null;
        }
    }
}
