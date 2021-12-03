using System;
using System.Collections.Generic;
using System.Text;

namespace BenLincoln.TheLostWorlds.CDTextures
{
	public class TextureFileException : System.ApplicationException
	{
		public TextureFileException(string message)
			: base(message)
		{
		}

		public TextureFileException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}
}
