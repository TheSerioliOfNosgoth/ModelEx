using System.Collections.Generic;
using TPage = BenLincoln.TheLostWorlds.CDTextures.PlaystationTexturePage;

namespace BenLincoln.TheLostWorlds.CDTextures
{
	public class PlaystationTextureDictionary
	{
		List<TPage> tPages = new List<TPage>();
		protected List<PlaystationColorTable> colorTables = new List<PlaystationColorTable>();
		protected Dictionary<ushort, int> clutRefs = new Dictionary<ushort, int>();

		public TPage this[int i] { get { return tPages[i]; } }

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

			ushort clutID = (ushort)colorTables.FindIndex(x => x.clut == clutValue);
			if (clutID == 0xFFFF)
			{
				PlaystationColorTable colorTable = new PlaystationColorTable(clutValue);
				colorTables.Add(colorTable);
			}

			if (clutRefs.ContainsKey(clutValue))
			{
				clutRefs[clutValue]++;
			}
			else
			{
				clutRefs[clutValue] = 1;
			}

			return textureID;
		}

		public void Initialize(ushort[,] textureData, int imageWidth, int imageHeight, int totalWidth, bool alwaysUseGreyscaleForMissingPalettes)
		{
			foreach (PlaystationColorTable colorTable in colorTables)
			{
				colorTable.Initialize(textureData, totalWidth);
			}

			ushort commonCLUT = 0;
			foreach(KeyValuePair<ushort, int> clutRef in clutRefs)
			{
				if (clutRef.Value > commonCLUT)
				{
					commonCLUT = clutRef.Key;
				}
			}

			foreach (TPage tPage in tPages)
			{
				tPage.Initialize(textureData, imageWidth, imageHeight, totalWidth, commonCLUT, alwaysUseGreyscaleForMissingPalettes);
			}
		}

		public IReadOnlyCollection<PlaystationColorTable> GetColorTables()
		{
			return colorTables;
		}

		public int GetClutRefCount(ushort clut)
		{
			if (clutRefs.ContainsKey(clut))
			{
				return clutRefs[clut];
			}

			return 0;
		}
	}
}
