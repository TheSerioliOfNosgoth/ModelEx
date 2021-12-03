using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace BenLincoln.TheLostWorlds.CDTextures
{
	public abstract class SoulReaverMonolithicTextureFile : TextureFile
	{
		protected int _HeaderLength;
		protected int _TextureLength;

		public SoulReaverMonolithicTextureFile(string path)
			: base(path)
		{
		}

		public override void ExportFile(int index, string outPath)
		{
			Bitmap tex = GetTextureAsBitmap(index);
			tex.Save(outPath, System.Drawing.Imaging.ImageFormat.Png);
		}

		protected override int _GetTextureCount()
		{
			if (_FileInfo.Length < (_HeaderLength + _TextureLength))
			{
				throw new TextureFileException("The file '" + _FilePath + "' does not contain enough data for it to be " +
					"a texture file for the " + _FileTypeName + " version of Soul Reaver ("
					+ _HeaderLength + " byte header + " + _TextureLength + " bytes per texture).");
			}
			long textureCountLong;
			float textureCountFloat;
			textureCountLong = (_FileInfo.Length - _HeaderLength) / _TextureLength;
			textureCountFloat = ((float)_FileInfo.Length - (float)_HeaderLength) / (float)_TextureLength;
			if ((float)textureCountLong != textureCountFloat)
			{
				throw new TextureFileException("The file '" + _FilePath + "' does not appear to be a valid " +
					"texture file for the " + _FileTypeName + " version of Soul Reaver. File lengths for this type should be " +
					+_HeaderLength + " bytes plus a whole number multiple of " + _TextureLength + " bytes.");
			}
			return (int)textureCountLong;
		}
	}
}
