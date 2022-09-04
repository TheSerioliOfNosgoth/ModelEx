using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BenLincoln.TheLostWorlds.CDTextures
{
	public class PSXPixelList
	{
		ulong[,] pixels = new ulong[256, 256];
		
		public ulong this[int y, int x]
		{ get { return pixels[y, x]; } set { pixels[y, x] = value; } }

		public PSXPixelList()
		{

		}
	}
}
