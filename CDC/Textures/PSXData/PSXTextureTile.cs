using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BenLincoln.TheLostWorlds.CDTextures
{
	public class PSXTextureTile
	{
		public int textureID;       // 0-8
		public ushort tPage;
		public ushort clut;
		public bool textureUsed;
		public bool visible;
		public int[] u;             // 0-255 each
		public int[] v;             // 0-255 each 
	}
}
