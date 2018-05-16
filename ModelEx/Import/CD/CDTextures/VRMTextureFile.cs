using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BenLincoln.TheLostWorlds.CDTextures
{
    public enum VRMFormat
    {
        Uncompressed,
        DXTC1,
        DXTC5,
        PS2_8Bit_Indexed,
        PS2_ARGB
    }
    
    public struct VRMTextureDefinition
    {
        public long Offset;
        public long Length;
        public int BPP;
        public int Width;
        public int Height;
        public ushort Type;
        public ushort Flags1;
        public uint Flags2;
        public VRMFormat Format;
        public VRMPaletteDefinition Palette;
        public VRMSubTextureDefinition[] SubTextures;
    }

    public struct VRMPaletteDefinition
    {
        public long Offset;
        public long Length;
        public int BPP;
    }

    public struct VRMSubTextureDefinition
    {
        public long Offset;
        public long Length;
        public uint Type;
        public int Width;
        public int Height;
    }

    public abstract class VRMTextureFile : TextureFile
    {
        protected int _HeaderLength;
        protected VRMTextureDefinition[] _TextureDefinitions;

        #region Properties

        public VRMTextureDefinition[] TextureDefinitions
        {
            get
            {
                return _TextureDefinitions;
            }
        }

        #endregion

        public VRMTextureFile(string path)
            : base(path)
        {
        }

        protected override int _GetTextureCount()
        {
            if (_FileInfo.Length < (_HeaderLength))
            {
                throw new TextureFileException("The file '" + _FilePath + "' does not contain enough data for it to be " +
                    "a VRM texture file of type '" + _FileTypeName + "'.");
            }
            try
            {
                FileStream iStream = new FileStream(_FilePath, FileMode.Open, FileAccess.Read);
                BinaryReader iReader = new BinaryReader(iStream);
                ushort numTextures = iReader.ReadUInt16();
                iReader.Close();
                iStream.Close();
                return numTextures;
            }
            catch (Exception ex)
            {
                throw new TextureFileException("Error obtaining the number of textures in '" + _FilePath + "'.", ex);
            }
        }

        public static TextureFileType GetVRMType(string path)
        {
            try
            {
                FileStream iStream = new FileStream(path, FileMode.Open, FileAccess.Read);
                if (iStream.Length < 32)
                {
                    iStream.Close();
                    return TextureFileType.Unknown;
                }
                BinaryReader iReader = new BinaryReader(iStream);

                iStream.Seek(16, SeekOrigin.Begin);

                ulong testVal = iReader.ReadUInt64();

                iReader.Close();
                iStream.Close();

                if (testVal == 0)
                {
                    return TextureFileType.SoulReaver2PC;
                }
                else
                {
                    return TextureFileType.SoulReaver2Playstation2;
                }
            }
            catch (Exception ex)
            {
                throw new TextureFileException("Error reading file '" + path + "' to determine type.", ex);
            }
        }
    }
}
