using System;
using System.IO;
using System.Collections.Generic;
using CDC.Objects.Models;

namespace CDC.Objects
{
	public class DefianceFile : SRFile
	{
		public DefianceFile(String strFileName, CDC.Objects.ExportOptions options)
			: base(strFileName, Game.Defiance, options)
		{
		}

		protected override void ReadHeaderData(BinaryReader reader, CDC.Objects.ExportOptions options)
		{
			_dataStart = 0;

			reader.BaseStream.Position = 0x000000AC;
			if (reader.ReadUInt32() == 0x04C2046E)
			{
				_asset = Asset.Unit;
			}
			else
			{
				_asset = Asset.Object;
			}
		}

		protected override void ReadObjectData(BinaryReader reader, CDC.Objects.ExportOptions options)
		{
			// Object name. No names in Defiance :(
			//reader.BaseStream.Position = _dataStart + 0x00000024;
			//reader.BaseStream.Position = _dataStart + reader.ReadUInt32();
			//String strModelName = new String(reader.ReadChars(8));
			//_name = Utility.CleanObjectName(strModelName);

			// Texture type
			//reader.BaseStream.Position = _dataStart + 0x44;
			//if (reader.ReadUInt64() != 0xFFFFFFFFFFFFFFFF)
			//{
			//    _platform = Platform.PSX;
			//}
			//else
			//{
			if (options.ForcedPlatform == CDC.Platform.None)
			{
				_platform = Platform.PC;
			}
			else
			{
				_platform = options.ForcedPlatform;
			}
			//}

			// Model data
			reader.BaseStream.Position = _dataStart + 0x00000028;
			_modelCount = reader.ReadUInt16();
			_modelCount = 1; // There are multiple models, but Defiance might have too many. Override for now.
			_animCount = 0; //reader.ReadUInt16();
			reader.BaseStream.Position += 0x02;
			_modelStart = _dataStart + reader.ReadUInt32();
			_animStart = 0; //_dataStart + reader.ReadUInt32();

			_models = new DefianceModel[_modelCount];
			for (UInt16 m = 0; m < _modelCount; m++)
			{
				_models[m] = DefianceObjectModel.Load(reader, _dataStart, _modelStart, _name, _platform, m, _version, options);
			}
		}

		protected override void ReadUnitData(BinaryReader reader, CDC.Objects.ExportOptions options)
		{
			// Adjacent units are seperate from portals.
			// There can be multiple portals to the same unit.
			// Portals
			reader.BaseStream.Position = _dataStart + 0x10;
			UInt32 m_uConnectionData = _dataStart + reader.ReadUInt32(); // Same as m_uModelData?
			reader.BaseStream.Position = m_uConnectionData + 0x24;
			portalCount = reader.ReadUInt32();
			reader.BaseStream.Position = _dataStart + reader.ReadUInt32();
			_portalNames = new String[portalCount];
			for (int i = 0; i < portalCount; i++)
			{
				String strUnitName = new String(reader.ReadChars(16));
				_portalNames[i] = Utility.CleanName(strUnitName);
				reader.BaseStream.Position += 0x90;
			}

			// Intros
			//reader.BaseStream.Position = _dataStart + 0x44;
			//_introCount = reader.ReadUInt32();
			//_introStart = _dataStart + reader.ReadUInt32();
			//_intros = new Intro[_introCount];
			//for (int i = 0; i < _introCount; i++)
			//{
			//	reader.BaseStream.Position = _introStart + 0x60 * i;
			//	String strIntroName = new String(reader.ReadChars(8));
			//	_intros[i].name = Utility.CleanObjectName(strIntroName) + "-" + _intros[i].ID;
			//}

			// Object Names
			//reader.BaseStream.Position = _dataStart + 0x4C;
			//_objectNameStart = _dataStart + reader.ReadUInt32();
			//reader.BaseStream.Position = _objectNameStart;
			//List<String> introList = new List<String>();
			//while (reader.ReadByte() != 0xFF)
			//{
			//	reader.BaseStream.Position--;
			//	String strObjectName = new String(reader.ReadChars(8));
			//	introList.Add(Utility.CleanObjectName(strObjectName));
			//	reader.BaseStream.Position += 0x08;
			//}
			//_objectNames = introList.ToArray();

			// Unit name
			reader.BaseStream.Position = _dataStart + 0x84;
			reader.BaseStream.Position = _dataStart + reader.ReadUInt32();
			String strModelName = new String(reader.ReadChars(10)); // Need to check
			_name = Utility.CleanName(strModelName);

			// Texture type
			//reader.BaseStream.Position = _dataStart + 0x9C;
			//if (reader.ReadUInt64() != 0xFFFFFFFFFFFFFFFF)
			//{
			//    _platform = Platform.PSX;
			//}
			//else
			//{
			if (options.ForcedPlatform == CDC.Platform.None)
			{
				_platform = Platform.PC;
			}
			else
			{
				_platform = options.ForcedPlatform;
			}
			//}

			// Model data
			reader.BaseStream.Position = _dataStart + 0x10;
			_modelCount = 1;
			_modelStart = _dataStart + 0x10;
			_models = new DefianceModel[_modelCount];
			reader.BaseStream.Position = _modelStart;
			UInt32 m_uModelData = _dataStart + reader.ReadUInt32();

			// Material data
			Console.WriteLine("Debug: reading area model 0");
			_models[0] = DefianceUnitModel.Load(reader, _dataStart, m_uModelData, _name, _platform, _version, options);

			//if (m_axModels[0].Platform == Platform.Dreamcast ||
			//    m_axModels[1].Platform == Platform.Dreamcast)
			//{
			//    _platform = Platform.Dreamcast;
			//}
		}

		protected override void ResolvePointers(BinaryReader reader, BinaryWriter writer)
		{
			/*
			 * BlockInfo // AKA SectionInfo
			 * {
			 *      uint32 size;
			 *      uint32 type;
			 *      uint32 id;
			 *  }
			 *  
			 *  BlockList // AKA SectionList
			 *  {
			 *      uint32 versionNumber;
			 *      uint32 numBlocks;
			 *      BlockInfo blockList[0];
			 *  }
			 */

			reader.BaseStream.Position = 0;
			writer.BaseStream.Position = 0;

			// This should be 11, otherwise it gives:
			// "Wrong mkloadob version %s %x\nrebuild needed\n"
			reader.BaseStream.Position += 0x04;

			UInt32 uRegionCount = reader.ReadUInt32();

			UInt32 uTotal = 0;
			UInt32[] auRegionSizes = new UInt32[uRegionCount];
			UInt32[] auRegionPositions = new UInt32[uRegionCount];
			for (UInt32 r = 0; r < uRegionCount; r++)
			{
				auRegionSizes[r] = reader.ReadUInt32();
				auRegionPositions[r] = uTotal;
				uTotal += auRegionSizes[r];
				uTotal += 0x10; // An extra 16 bytes for metadata.
				reader.BaseStream.Position += 0x08;
			}

			UInt32 uRegionDataSize = uRegionCount * 0x0C;
			UInt32 uPointerData = (uRegionDataSize + 0x17) & 0xFFFFFFF0;
			for (UInt32 currentRegion = 0; currentRegion < uRegionCount; currentRegion++)
			{
				reader.BaseStream.Position = uPointerData;
				UInt32 uPointerCount = reader.ReadUInt32();
				UInt32 uPointerDataSize = uPointerCount * 0x04;
				UInt32 uObjectData = uPointerData + ((uPointerDataSize + 0x13) & 0xFFFFFFF0);
				UInt32 uObjectDataSize = (auRegionSizes[currentRegion] + 0x0F) & 0xFFFFFFF0;

				reader.BaseStream.Position = uObjectData;
				writer.BaseStream.Position = auRegionPositions[currentRegion];

				writer.Write(0x0001BADE); // Seems like it's always 0x0001BADE
				writer.Write(0x00000002); // Seems like it's always 0x00000002
				writer.Write(auRegionSizes[currentRegion]);
				writer.Write(auRegionPositions[currentRegion] + auRegionSizes[currentRegion]);

				Byte[] auObjectData = reader.ReadBytes((Int32)auRegionSizes[currentRegion]);
				writer.Write(auObjectData);

				reader.BaseStream.Position = uPointerData + 0x04;
				UInt32[] auAddresses = new UInt32[uPointerCount];
				for (UInt32 p = 0; p < uPointerCount; p++)
				{
					auAddresses[p] = reader.ReadUInt32();
				}

				UInt32[] auValues = new UInt32[uPointerCount];
				for (UInt32 p = 0; p < uPointerCount; p++)
				{
					reader.BaseStream.Position = uObjectData + auAddresses[p];
					UInt32 regionIndexAndOffset = reader.ReadUInt32();
					UInt32 offset = regionIndexAndOffset & 0x003FFFFF;
					UInt32 regionIndex = regionIndexAndOffset >> 0x16;

					auAddresses[p] += auRegionPositions[currentRegion] + 0x10;
					auValues[p] = auRegionPositions[regionIndex] + offset + 0x10;

					writer.BaseStream.Position = auAddresses[p];
					writer.Write(auValues[p]);
				}

				uPointerData = uObjectData + uObjectDataSize;
			}
		}
	}
}