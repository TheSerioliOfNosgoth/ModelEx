using System;
using System.IO;

namespace BenLincoln.TheLostWorlds.CDTextures
{
	public class SoulReaverPSXCRMTextureFile : PSXTextureFile
	{
		public SoulReaverPSXCRMTextureFile(string path)
			: base(path)
		{
			_FileType = TextureFileType.SoulReaverPlaystation;
			_FileTypeName = "Soul Reaver (Playstation) Indexed Texture File";
			LoadTextureData();
		}

		protected void LoadTextureData()
		{
			try
			{
				FileStream stream = new FileStream(_FilePath, FileMode.Open, FileAccess.Read);
				BinaryReader reader = new BinaryReader(stream);

				reader.BaseStream.Position = 20;
				_XShift = reader.ReadInt32();
				reader.BaseStream.Position += 4;
				_TotalWidth = reader.ReadInt32();
				_TotalHeight = reader.ReadInt32();

				_TextureData = new ushort[_TotalHeight, _TotalWidth];
				for (int y = 0; y < _TotalHeight; y++)
				{
					for (int x = 0; x < _TotalWidth; x++)
					{
						_TextureData[y, x] = reader.ReadUInt16();
					}
				}

				reader.Close();
				stream.Close();
			}
			catch (Exception ex)
			{
				throw new TextureFileException("Error reading texture.", ex);
			}
		}
	}
}
