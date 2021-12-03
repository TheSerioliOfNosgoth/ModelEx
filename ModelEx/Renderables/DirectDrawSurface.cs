using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ModelEx
{
	enum DDSD : uint
	{
		CAPS = 0x1,
		HEIGHT = 0x2,
		WIDTH = 0x4,
		PITCH = 0x8,
		PIXELFORMAT = 0x1000,
		MIPMAPCOUNT = 0x20000,
		LINEARSIZE = 0x80000,
		DEPTH = 0x800000,
	}

	enum DDSCAPS : uint
	{
		COMPLEX = 0x8,
		MIPMAP = 0x400000,
		TEXTURE = 0x1000,
	}

	enum DDSCAPS2 : uint
	{
		CUBEMAP = 0x200,
		CUBEMAP_POSITIVEX = 0x400,
		CUBEMAP_NEGATIVEX = 0x800,
		CUBEMAP_POSITIVEY = 0x1000,
		CUBEMAP_NEGATIVEY = 0x2000,
		CUBEMAP_POSITIVEZ = 0x4000,
		CUBEMAP_NEGATIVEZ = 0x8000,
		VOLUME = 0x200000,
	}

	public enum DDSFormat : uint
	{
		Invalid = 0,
		DXT1 = 0x31545844,
		DXT2 = 0x32545844,
		DXT3 = 0x33545844,
		DXT4 = 0x34545844,
		DXT5 = 0x35545844,
		DX10 = 0x30545844,
	}

	enum DDPF : uint
	{
		ALPHAPIXELS = 0x1,
		ALPHA = 0x2,
		FOURCC = 0x4,
		RGB = 0x40,
		YUV = 0x200,
		LUMINANCE = 0x20000,
	};

	public class DirectDrawSurface
	{
		public UInt32 dxtcMagic = 0;
		public UInt32 dxtcSize = 0;

		public UInt32 dxtcFlags = 0;

		public UInt32 dxtcWidth = 0;
		public UInt32 dxtcHeight = 0;

		public UInt32 dxtcPitchOrLinearSize = 0;
		public UInt32 dxtcDepth = 0;
		public UInt32 dxtcMipMapCount = 0;

		// - Begin DDS_PIXELFORMAT
		public UInt32 dxtcSize2 = 0;
		public UInt32 dxtcFlags2 = 0;

		public UInt32 dxtcFourCC = 0;

		public UInt32 dxtcBPP = 0;
		public UInt32 dxtcMaskRed = 0;
		public UInt32 dxtcMaskGreen = 0;
		public UInt32 dxtcMaskBlue = 0;
		public UInt32 dxtcMaskAlpha = 0;
		// - End DDS_PIXELFORMAT

		public UInt32 dxtcCaps1 = 0;
		public UInt32 dxtcCaps2 = 0;
		public UInt32 dxtcCaps3 = 0;
		public UInt32 dxtcCaps4 = 0;
		public UInt32 dxtcTextureStage = 0;

		public DDSFormat Format
		{
			get
			{
				switch (dxtcFourCC)
				{
					case (uint)DDSFormat.DXT1:
					case (uint)DDSFormat.DXT2:
					case (uint)DDSFormat.DXT3:
					case (uint)DDSFormat.DXT4:
					case (uint)DDSFormat.DXT5:
					case (uint)DDSFormat.DX10:
						return (DDSFormat)dxtcFourCC;
					default:
						return DDSFormat.Invalid;
				}
			}
		}

		public bool IsCompressed
		{
			get
			{
				return ((DDPF)dxtcFlags2 & DDPF.FOURCC) != 0;
			}
		}

		public bool HasMipMaps
		{
			get
			{
				return ((DDSCAPS)dxtcCaps1 & DDSCAPS.MIPMAP) != 0;
			}
		}

		public MemoryStream data = null;

		public DirectDrawSurface()
		{
		}

		~DirectDrawSurface()
		{
			if (data != null)
			{
				data.Close();
			}
		}

		public void ReadHeader(string fileName)
		{
			FileStream file = new FileStream(fileName, FileMode.Open, FileAccess.Read);
			BinaryReader reader = new BinaryReader(file);

			try
			{
				dxtcMagic = reader.ReadUInt32();
				if (dxtcMagic != 0x20534444)
				{
					throw new Exception("This is not a dds file.");
				}

				dxtcSize = reader.ReadUInt32();
				if (dxtcSize != 124u)
				{
					throw new Exception("DDS_HEADER size is not 124.");
				}

				dxtcFlags = reader.ReadUInt32();
				dxtcHeight = reader.ReadUInt32();
				dxtcWidth = reader.ReadUInt32();
				dxtcPitchOrLinearSize = reader.ReadUInt32();
				dxtcDepth = reader.ReadUInt32();
				dxtcMipMapCount = reader.ReadUInt32();
				// 11 uints of zero
				for (int i = 0; i < 11; i++)
				{
					reader.ReadUInt32();
				}

				// - Begin DDS_PIXELFORMAT
				dxtcSize2 = reader.ReadUInt32();
				if (dxtcSize2 != 32u)
				{
					throw new Exception("DDS_PIXELFORMAT header size is not 32.");
				}

				dxtcFlags2 = reader.ReadUInt32();
				dxtcFourCC = reader.ReadUInt32();
				dxtcBPP = reader.ReadUInt32();
				dxtcMaskRed = reader.ReadUInt32();
				dxtcMaskGreen = reader.ReadUInt32();
				dxtcMaskBlue = reader.ReadUInt32();
				dxtcMaskAlpha = reader.ReadUInt32();
				// - End DDS_PIXELFORMAT

				dxtcCaps1 = reader.ReadUInt32();
				dxtcCaps2 = reader.ReadUInt32();
				dxtcCaps3 = reader.ReadUInt32();
				dxtcCaps4 = reader.ReadUInt32();
				dxtcTextureStage = reader.ReadUInt32();

				if (IsCompressed)
				{
					if (Format == DDSFormat.Invalid)
					{
						throw new Exception("Invalid four character code");
					}

					// No intention to support DDS_HEADER_DXT10 at the moment.  Would read it in here.
					if (Format == DDSFormat.DX10)
					{
						throw new Exception("DDS_HEADER_DXT10 not supported");
					}

					//if ((dxtcFlags & ((uint)DDSD.LINEARSIZE)) == 0)
					//{
					//    throw new Exception("DDSD flag 'DDSD_LINEARSIZE' not found.  Required for compressed textures.");
					//}
				}
				else
				{
					if ((dxtcFlags & ((uint)DDSD.WIDTH)) == 0)
					{
						throw new Exception("DDSD flag 'DDSD_WIDTH' not found.  Required for uncompressed textures.");
					}

					if ((dxtcFlags2 & (uint)DDPF.RGB) == 0)
					{
						throw new Exception("DDS_PIXELFORMAT flag 'DDPF_RBG' not found. Required for uncompressed textures.");
					}

					// Some textures might not have alpha.  Might be OK to allow this.
					//if ((dxtcFlags2 & (uint)DDPF.ALPHAPIXELS) == 0)
					//{
					//    throw new Exception("DDS_PIXELFORMAT flag 'DDPF_ALPHAPIXELS' not found.");
					//}

					if ((dxtcFlags2 & (uint)DDPF.ALPHA) != 0) // Used by very old versions.
					{
						throw new Exception("DDS_PIXELFORMAT flag 'DDPF_ALPHA' found.  Alpha channel only uncompressed data not supported.");
					}

					if ((dxtcFlags2 & (uint)DDPF.YUV) != 0) // Used by very old versions.
					{
						throw new Exception("DDS_PIXELFORMAT flag 'DDPF_YUV' found.  YUV uncompressed data not supported.");
					}

					if ((dxtcFlags2 & (uint)DDPF.LUMINANCE) != 0) // Used by very old versions.
					{
						throw new Exception("DDS_PIXELFORMAT flag 'DDPF_LUMINANCE' found.  Single channel color uncompressed data not supported.");
					}
				}

				uint DDS_HEADER_FLAGS_TEXTURE = (uint)(DDSD.CAPS | DDSD.HEIGHT | DDSD.WIDTH | DDSD.PIXELFORMAT);
				if ((dxtcFlags & DDS_HEADER_FLAGS_TEXTURE) != DDS_HEADER_FLAGS_TEXTURE)
				{
					throw new Exception("DDSD flag 'DDS_HEADER_FLAGS_TEXTURE' not found.");
				}

				if ((dxtcFlags & ((uint)DDSD.DEPTH)) != 0)
				{
					throw new Exception("DDSD flag 'DDSD_DEPTH' found.  Depth textures not supported.");
				}

				if ((dxtcCaps1 & (uint)DDSCAPS.TEXTURE) == 0)
				{
					throw new Exception("DDSCAPS flag 'DDSCAPS_TEXTURE' not found.");
				}

				if ((dxtcCaps1 & (uint)DDSCAPS.MIPMAP) == 0 && (dxtcCaps1 & (uint)DDSCAPS.COMPLEX) != 0)
				{
					throw new Exception("DDSCAPS flag 'DDSCAPS_MIPMAP' found, but DDSCAPS flag 'DDSCAPS_COMPLEX' not found.");
				}

				if ((dxtcCaps2 & (uint)DDSCAPS2.CUBEMAP) != 0)
				{
					throw new Exception("DDSCAPS2 flag 'DDSCAPS2.CUBEMAP' found.  Cube maps not supported.");
				}

				if ((dxtcCaps2 & (uint)DDSCAPS2.VOLUME) != 0)
				{
					throw new Exception("DDSCAPS2 flag 'DDSCAPS2.VOLUME' found.  Volume textures not supported.");
				}

				int streamLength = 0;
				int targetSize = (int)reader.BaseStream.Length - 128;
				if (IsCompressed)
				{
					int width = (int)dxtcWidth;
					int height = (int)dxtcHeight;
					int blockSize = (Format == DDSFormat.DXT1) ? 8 : 16;
					int mipMapCount = (!HasMipMaps) ? 1 : (int)dxtcMipMapCount;

					streamLength = 0;
					for (int i = 0; i < mipMapCount; i++)
					{
						int mipMapWidth = width >> i;
						int mipMapHeight = height >> i;
						int mipMapSize = (int)(Math.Max(1, ((mipMapWidth + 3) / 4)) * Math.Max(1, ((mipMapHeight + 3) / 4)) * blockSize);
						streamLength += mipMapSize;
					}

					data = new MemoryStream(targetSize);
					file.CopyTo(data);
					data.Position = 0;
				}
			}
			finally
			{
				reader.Close();
				file.Close();
			}
		}
	}
}