using System.Collections.Generic;
using TPage = BenLincoln.TheLostWorlds.CDTextures.PlaystationTexturePage;

namespace BenLincoln.TheLostWorlds.CDTextures
{
	public class PlaystationTextureDictionary
	{
		List<TPage> tPages = new List<TPage>();

		public TPage this[int i]
		{
			get
			{
				return tPages[i];
			}
		}

		public int Count { get { return tPages.Count; } }

		public ushort AddTexturePage(ushort texturePage, ushort clutValue)
		{
			TPage tPage;
			ushort textureID = (ushort)tPages.FindIndex(x => x.tPage == texturePage);
			if (textureID == 0xFFFF)
			{
				textureID = (ushort)tPages.Count;
				tPage = new TPage(texturePage);
				tPages.Add(tPage);
			}
			else
			{
				tPage = tPages[textureID];
			}

			tPage.AddColorTable(clutValue);

			return textureID;
		}

		public void Initialize(ushort[,] textureData, int imageWidth, int imageHeight, int totalWidth)
		{
			foreach (TPage tPage in tPages)
			{
				tPage.Initialize(textureData, imageWidth, imageHeight, totalWidth);
			}
		}
	}
}
