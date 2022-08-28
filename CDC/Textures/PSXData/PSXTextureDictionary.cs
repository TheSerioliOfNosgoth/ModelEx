using System.Collections.Generic;
using TPage = BenLincoln.TheLostWorlds.CDTextures.PSXTexturePage;

namespace BenLincoln.TheLostWorlds.CDTextures
{
	public class PSXTextureDictionary
	{
		List<TPage> tPages = new List<TPage>();
		protected List<PSXColorTable> colorTables = new List<PSXColorTable>();
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
				PSXColorTable colorTable = new PSXColorTable(clutValue);
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
			foreach (PSXColorTable colorTable in colorTables)
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

		public IReadOnlyCollection<PSXColorTable> GetColorTables()
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
