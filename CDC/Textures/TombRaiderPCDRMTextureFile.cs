using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace BenLincoln.TheLostWorlds.CDTextures
{
	public enum DRMFormat
	{
		Uncompressed,
		DXTC1,
		DXTC5
	}

	public struct DRMTextureDefinition
	{
		public long Offset;
		public long Length;
		public int Width;
		public int Height;
		public uint Type;
		public uint ID;
		public uint Flags2;
		public DRMFormat Format;
	}

	public class TombRaiderPCDRMTextureFile : TextureFile
	{
		enum SectionType
		{
			General = 0,
			Animation = 2,
			Texture = 5,
			Wave = 6,
			Dtp = 7
		}

		protected DRMTextureDefinition[] _TextureDefinitions;

		#region Properties

		public DRMTextureDefinition[] TextureDefinitions
		{
			get
			{
				return _TextureDefinitions;
			}
		}

		#endregion

		public TombRaiderPCDRMTextureFile(string path)
			: base(path)
		{
			_FileType = TextureFileType.TombRaiderPC;
			_FileTypeName = "Tomb Raider (PC) DRM";
			GetTextureDefinitions();
		}

		protected override int _GetTextureCount()
		{
			return _TextureCount;
		}

		protected void GetTextureDefinitions()
		{
			List<DRMTextureDefinition> textureDefinitions = new List<DRMTextureDefinition>();

			try
			{
				FileStream iStream = new FileStream(_FilePath, FileMode.Open, FileAccess.Read);
				BinaryReader iReader = new BinaryReader(iStream);

				iReader.BaseStream.Position += 0x04; // Version number

				Int32 numSections = iReader.ReadInt32();
				long dataStart = (numSections * 0x14) + 0x08;

				// read all section headers
				for (int i = 0; i < numSections; i++)
				{
					iReader.BaseStream.Position = (i * 0x14) + 0x08;

					UInt32 sectionSize = iReader.ReadUInt32();
					SectionType sectionType = (SectionType)iReader.ReadByte();

					iReader.BaseStream.Position += 0x03; // skip past pad and version id

					UInt32 packedData = iReader.ReadUInt32();

					UInt32 sectionID = iReader.ReadUInt32();
					UInt32 numRelocations = packedData >> 8;

					if (sectionType == SectionType.Texture)
					{
						iReader.BaseStream.Position = dataStart + 0x04;

						DRMTextureDefinition textureDefinition = new DRMTextureDefinition();

						textureDefinition.Type = iReader.ReadUInt32();
						textureDefinition.Offset = dataStart + 0x18;
						textureDefinition.Length = iReader.ReadUInt32();
						iReader.BaseStream.Position += 0x04;
						textureDefinition.Height = iReader.ReadUInt16();
						textureDefinition.Width = iReader.ReadUInt16();
						iReader.BaseStream.Position += 0x01;
						textureDefinition.Flags2 = iReader.ReadByte();
						textureDefinition.ID = sectionID;

						switch (textureDefinition.Type)
						{
							case 0x15:
								textureDefinition.Format = DRMFormat.Uncompressed;
								break;
							case 0x31545844:
								textureDefinition.Format = DRMFormat.DXTC1;
								break;
							case 0x35545844:
								textureDefinition.Format = DRMFormat.DXTC5;
								break;
							default:
								throw new NotImplementedException("Support for type '" + textureDefinition.Type.ToString() +
									"' files is not yet implemented.");
						}

						textureDefinitions.Add(textureDefinition);
					}

					dataStart += sectionSize + (numRelocations * 8);
				}

				iReader.Close();
				iStream.Close();
			}
			catch (Exception ex)
			{
				throw new TextureFileException("Error enumerating textures.", ex);
			}

			_TextureDefinitions = textureDefinitions.ToArray();
			_TextureCount = _TextureDefinitions.Length;
		}

		public override MemoryStream GetDataAsStream(int index)
		{
			switch (_TextureDefinitions[index].Format)
			{
				case DRMFormat.Uncompressed:
					return GetUncompressedDataAsStream(index);
				default:
					return GetDXTCDataAsStream(index);
			}
		}

		protected MemoryStream GetUncompressedDataAsStream(int index)
		{
			int streamLength = (int)(_TextureDefinitions[index].Length);
			MemoryStream mStream = new MemoryStream(streamLength);
			BinaryWriter mWriter = new BinaryWriter(mStream);

			// get the actual texture data
			try
			{
				FileStream iStream = new FileStream(_FilePath, FileMode.Open, FileAccess.Read);
				BinaryReader iReader = new BinaryReader(iStream);

				iStream.Seek(_TextureDefinitions[index].Offset, SeekOrigin.Begin);
				for (int byteNum = 0; byteNum < streamLength; byteNum++)
				{
					mWriter.Write(iReader.ReadByte());
				}

				iReader.Close();
				iStream.Close();
			}
			catch (Exception ex)
			{
				throw new TextureFileException("Error reading texture data from file.", ex);
			}

			mStream.Seek(0, SeekOrigin.Begin);
			return mStream;
		}

		protected MemoryStream GetDXTCDataAsStream(int index)
		{
			int streamLength = (int)(_TextureDefinitions[index].Length + 128); // size of DXTC header
			MemoryStream mStream = new MemoryStream(streamLength);
			BinaryWriter mWriter = new BinaryWriter(mStream);

			byte[] dxtcZero = new byte[] { 0x00, 0x00, 0x00, 0x00 };

			byte[] dxtcMagic = new byte[] { 0x44, 0x44, 0x53, 0x20 };
			byte[] dxtcSize = new byte[] { 0x7C, 0x00, 0x00, 0x00 }; // is always 124
			mWriter.Write(dxtcMagic);
			mWriter.Write(dxtcSize);

			byte[] dxtcFlags = new byte[] { 0x07, 0x10, 0x0A, 0x00 }; // ???
			mWriter.Write(dxtcFlags);
			//mWriter.Write(_TextureDefinitions[index].Flags1);
			//byte[] dxtcFlags1a = new byte[] { 0x0A, 0x00 };
			//mWriter.Write(dxtcFlags1a);

			uint width = (uint)_TextureDefinitions[index].Width;
			uint height = (uint)_TextureDefinitions[index].Height;
			mWriter.Write(width);
			mWriter.Write(height);

			// uint linearsize
			int blockSize = 16;
			switch (_TextureDefinitions[index].Format)
			{
				case DRMFormat.DXTC1:
					blockSize = 8;
					break;
			}
			uint linearSize = (uint)((width / 4) * (height / 4) * blockSize);
			mWriter.Write(linearSize);

			// uint depth
			mWriter.Write(dxtcZero);

			// uint mipmapcount
			mWriter.Write(_TextureDefinitions[index].Flags2);

			// 11 uints of zero
			for (int i = 0; i < 11; i++)
			{
				mWriter.Write(dxtcZero);
			}

			byte[] dxtcSize2 = new byte[] { 0x20, 0x00, 0x00, 0x00 }; // is always 32
			byte[] dxtcFlags2 = new byte[] { 0x04, 0x00, 0x00, 0x00 }; // ???
			mWriter.Write(dxtcSize2);
			mWriter.Write(dxtcFlags2);
			byte[] dxtcFourCC = new byte[0];
			switch (_TextureDefinitions[index].Format)
			{
				case DRMFormat.DXTC1:
					dxtcFourCC = new byte[] { 0x44, 0x58, 0x54, 0x31 };
					break;
				case DRMFormat.DXTC5:
					dxtcFourCC = new byte[] { 0x44, 0x58, 0x54, 0x35 }; // 0x33 as last digit = DXT3
					break;
			}
			mWriter.Write(dxtcFourCC);

			byte[] dxtcBPP = new byte[] { 0x00, 0x00, 0x00, 0x00 }; // always zero?
			byte[] dxtcMaskRed = new byte[] { 0x00, 0x00, 0x00, 0x00 }; // always zero?
			byte[] dxtcMaskGreen = new byte[] { 0x00, 0x00, 0x00, 0x00 }; // always zero?
			byte[] dxtcMaskBlue = new byte[] { 0x00, 0x00, 0x00, 0x00 }; // always zero?
			byte[] dxtcMaskAlpha = new byte[] { 0x00, 0x00, 0x00, 0x00 }; // always zero?
			mWriter.Write(dxtcBPP);
			mWriter.Write(dxtcMaskRed);
			mWriter.Write(dxtcMaskGreen);
			mWriter.Write(dxtcMaskBlue);
			mWriter.Write(dxtcMaskAlpha);

			byte[] dxtcCaps1 = new byte[] { 0x08, 0x10, 0x40, 0x00 };
			byte[] dxtcCaps2 = new byte[] { 0x00, 0x00, 0x00, 0x00 };
			byte[] dxtcCaps3 = new byte[] { 0x00, 0x00, 0x00, 0x00 };
			byte[] dxtcCaps4 = new byte[] { 0x00, 0x00, 0x00, 0x00 };
			byte[] dxtcTextureStage = new byte[] { 0x00, 0x00, 0x00, 0x00 };
			mWriter.Write(dxtcCaps1);
			mWriter.Write(dxtcCaps2);
			mWriter.Write(dxtcCaps3);
			mWriter.Write(dxtcCaps4);
			mWriter.Write(dxtcTextureStage);

			// get the actual texture data
			try
			{
				FileStream iStream = new FileStream(_FilePath, FileMode.Open, FileAccess.Read);
				BinaryReader iReader = new BinaryReader(iStream);

				iStream.Seek(_TextureDefinitions[index].Offset, SeekOrigin.Begin);
				int length = (int)(_TextureDefinitions[index].Length);
				for (int byteNum = 0; byteNum < length; byteNum++)
				{
					mWriter.Write(iReader.ReadByte());
				}

				iReader.Close();
				iStream.Close();
			}
			catch (Exception ex)
			{
				throw new TextureFileException("Error reading texture data from file.", ex);
			}
			mStream.Seek(0, SeekOrigin.Begin);
			return mStream;
		}

		// for debugging
		public void WriteAllTextureData(string outFolder)
		{
			FileStream iStream = new FileStream(_FilePath, FileMode.Open, FileAccess.Read);
			BinaryReader iReader = new BinaryReader(iStream);
			for (int i = 0; i <= _TextureDefinitions.GetUpperBound(0); i++)
			{
				FileStream oStream = new FileStream(outFolder + @"\Texture-" + String.Format("{0:0000}", i) + "-" +
					String.Format("{0:X2}", _TextureDefinitions[i].Type) + "-" +
					String.Format("{0:X4}", _TextureDefinitions[i].Flags2) + "-" +
					String.Format("{0:0000}", _TextureDefinitions[i].Width) + "x" +
					String.Format("{0:0000}", _TextureDefinitions[i].Height) +
					".dat",
					FileMode.Create, FileAccess.Write);
				BinaryWriter oWriter = new BinaryWriter(oStream);

				iStream.Seek(_TextureDefinitions[i].Offset, SeekOrigin.Begin);
				for (int byteNum = 0; byteNum < _TextureDefinitions[i].Length; byteNum++)
				{
					oWriter.Write(iReader.ReadByte());
				}

				oWriter.Close();
				oStream.Close();
			}
			iReader.Close();
			iStream.Close();
		}

		public override void ExportFile(int index, string outPath)
		{
			switch (_TextureDefinitions[index].Format)
			{
				case DRMFormat.DXTC1:
					base.ExportFile(index, outPath);
					break;
				case DRMFormat.DXTC5:
					base.ExportFile(index, outPath);
					break;
				case DRMFormat.Uncompressed:
					_ErrorOccurred = false;
					_LastErrorMessage = "";
					try
					{
						Bitmap tex = GetUncompressedTextureAsBitmap(index);
						tex.Save(outPath, System.Drawing.Imaging.ImageFormat.Png);
					}
					catch (Exception ex)
					{
						_ErrorOccurred = true;
						_LastErrorMessage = ex.Message;
					}
					break;
			}
		}

		protected Bitmap GetUncompressedTextureAsBitmap(int index)
		{
			MemoryStream iStream = GetUncompressedDataAsStream(index);
			BinaryReader iReader = new BinaryReader(iStream);

			Bitmap tex = new Bitmap(_TextureDefinitions[index].Width, _TextureDefinitions[index].Height);

			for (int y = 0; y < tex.Height; y++)
			{
				for (int x = 0; x < tex.Width; x++)
				{
					int blue = (int)iReader.ReadByte();
					int green = (int)iReader.ReadByte();
					int red = (int)iReader.ReadByte();
					int alpha = (int)iReader.ReadByte();
					tex.SetPixel(x, y, Color.FromArgb(alpha, red, green, blue));
				}
			}

			iReader.Close();
			iStream.Close();

			return tex;
		}

		public MemoryStream GetUncompressedDataAsStream2(int index)
		{
			MemoryStream memoryStream = new MemoryStream();
			Bitmap b = GetTextureAsBitmap(index);
			b.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
			memoryStream.Seek(0, SeekOrigin.Begin);
			return memoryStream;
		}

		protected override System.Drawing.Bitmap _GetTextureAsBitmap(int index)
		{
			switch (_TextureDefinitions[index].Format)
			{
				case DRMFormat.DXTC1:
					{
						MemoryStream stream = GetDXTCDataAsStream(index);
						Soeminnminn.DirectDrawSurface.DDSImage image = new Soeminnminn.DirectDrawSurface.DDSImage(stream);
						return image.BitmapImage;
					}
				case DRMFormat.DXTC5:
					{
						MemoryStream stream = GetDXTCDataAsStream(index);
						Soeminnminn.DirectDrawSurface.DDSImage image = new Soeminnminn.DirectDrawSurface.DDSImage(stream);
						return image.BitmapImage;
					}
				case DRMFormat.Uncompressed:
					return GetUncompressedTextureAsBitmap(index);
			}
			throw new Exception("The method or operation is not implemented.");
		}
	}
}
