using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BenLincoln.TheLostWorlds.CDTextures
{
	public class PlaystationPixelList
	{
		Dictionary<int, ulong> dictionary = new Dictionary<int, ulong>();
		
		public ulong this[int i]
		{
			get
			{
				return dictionary[i];
			}
		}

		public void Add(int key, ulong value)
		{
			dictionary.Add(key, value);
		}

		public bool ContainsKey(int key)
		{
			return dictionary.ContainsKey(key);
		}
	}
}
