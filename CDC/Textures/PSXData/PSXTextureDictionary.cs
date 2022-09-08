using System.Collections.Generic;
using TPage = BenLincoln.TheLostWorlds.CDTextures.PSXTexturePage;

namespace BenLincoln.TheLostWorlds.CDTextures
{
	public class PSXTextureDictionary
	{
		bool isInitialized;
		ushort tPageMask;
		ushort clutMask;
		List<TPage> tPages = new List<TPage>();
		protected List<PSXColorTable> colorTables = new List<PSXColorTable>();
		protected Dictionary<ushort, int> clutRefs = new Dictionary<ushort, int>();
		public List<PSXTextureTile> tiles = new List<PSXTextureTile>();

		public TPage this[int i] { get { return tPages[i]; } }

		public int Count { get { return tPages.Count; } }

		public PSXTextureDictionary(ushort tPageMask, ushort clutMask)
		{
			this.tPageMask = tPageMask;
			this.clutMask = clutMask;
		}

		public ushort AddTextureTile(PSXTextureTile tile)
		{
			tile.tPage &= tPageMask;
			tile.clut &= clutMask;
			tile.textureID = AddTexturePage(tile.tPage, tile.clut);
			//tiles.Add(tile);
			return (ushort)tile.textureID;
		}

		public void AddTextureTile2(PSXTextureTile tile)
		{
			tile.tPage &= tPageMask;
			tile.clut &= clutMask;
			tiles.Add(tile);
		}

		protected ushort AddTexturePage(ushort texturePage, ushort clutValue)
		{
			texturePage &= tPageMask;
			clutValue &= clutMask;

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

		public void Initialize(ushort[,] textureData, int imageWidth, int imageHeight, int xShift, bool alwaysUseGreyscaleForMissingPalettes)
		{
			if (isInitialized)
			{
				return;
			}

			isInitialized = true;

			foreach (PSXColorTable colorTable in colorTables)
			{
				colorTable.Initialize(textureData, xShift);
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
				tPage.Initialize(textureData, imageWidth, imageHeight, xShift, commonCLUT, alwaysUseGreyscaleForMissingPalettes);
			}
		}

		public IReadOnlyCollection<PSXColorTable> GetColorTables()
		{
			return colorTables;
		}

		public int GetClutRefCount(ushort clut)
		{
			clut &= clutMask;

			if (clutRefs.ContainsKey(clut))
			{
				return clutRefs[clut];
			}

			return 0;
		}
	}
}
