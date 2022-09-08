using System;
using System.IO;

namespace BenLincoln.TheLostWorlds.CDTextures
{
	public class Gex3PSXVRMTextureFile : PSXTextureFile
	{
		public Gex3PSXVRMTextureFile(string path)
			: base(path)
		{
			_FileType = TextureFileType.Gex3Playstation;
			_FileTypeName = "Gex 3 (Playstation) VRM";
			_TotalWidth = 512;
			_TotalHeight = 512;
			LoadTextureData();
		}

		protected void LoadTextureData()
		{
			try
			{
				FileStream stream = new FileStream(_FilePath, FileMode.Open, FileAccess.Read);
				BinaryReader reader = new BinaryReader(stream);

				reader.BaseStream.Position = 20;

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
