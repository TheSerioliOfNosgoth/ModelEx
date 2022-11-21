using System;
using System.IO;
using System.Collections.Generic;

namespace CDC
{
	public class DefianceFile : DataFile
	{
		Int16[] _objectIDs;
		SortedList<int, string> _objectNamesList = new SortedList<int, string>();

		public DefianceFile(String dataFileName, Platform platform, ExportOptions options)
			: base(dataFileName, Game.Defiance, platform, options)
		{
		}

		public DefianceFile(String dataFileName, String objectListFile, Platform platform, ExportOptions options)
			: base(dataFileName, Game.Defiance, platform, options)
		{
			LoadObjectList(objectListFile);
		}

		protected override void ReadData(BinaryReader reader, ExportOptions options)
        {
			base.ReadData(reader, options);
        }

		protected override void ReadObjectData(BinaryReader reader, ExportOptions options)
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
			if (_platform == Platform.None)
			{
				_platform = Platform.PC;
			}
			//}

			reader.BaseStream.Position = _dataStart + 0x00000028;
			_modelCount = reader.ReadUInt16();
			_modelCount = 1; // There are multiple models, but Defiance might have too many. Override for now.
			_animCount = 0; //reader.ReadUInt16();
			reader.BaseStream.Position += 0x02;
			_modelStart = _dataStart + reader.ReadUInt32();
			_animStart = 0; //_dataStart + reader.ReadUInt32();

			_models = new IModel[_modelCount];
			for (UInt16 m = 0; m < _modelCount; m++)
			{
				reader.BaseStream.Position = _modelStart + (m * 4);
				uint modelData = _dataStart + reader.ReadUInt32();
				reader.BaseStream.Position = modelData;

				string modelName = _name + "-" + m.ToString();
				DefianceObjectModel model = new DefianceObjectModel(reader, this, _dataStart, modelData, modelName, _platform, _version);
				model.ReadData(reader, options);
				_models[m] = model;
			}
		}

		protected override void ReadUnitData(BinaryReader reader, ExportOptions options)
		{
			// Portals
			reader.BaseStream.Position = _dataStart + 0x10;
			UInt32 terrainData = _dataStart + reader.ReadUInt32(); // Same as _modelData?
			reader.BaseStream.Position = terrainData + 0x24;
			_portalCount = reader.ReadUInt32();
			reader.BaseStream.Position = _dataStart + reader.ReadUInt32();
			_portals = new Portal[_portalCount];
			for (int i = 0; i < _portalCount; i++)
			{
				Portal portal = new Portal();
				portal.toLevelName = new String(reader.ReadChars(32));
				portal.toLevelName = Utility.CleanName(portal.toLevelName);
				portal.mSignalID = reader.ReadInt16();
				reader.BaseStream.Position += 0x02; // streamID/closeVertList?
				reader.BaseStream.Position += 0x04; // streamID/closeVertList?
				reader.BaseStream.Position += 0x04; // activeDistance
				reader.BaseStream.Position += 0x04; // toStreamUnit
				portal.min.x = reader.ReadSingle();
				portal.min.y = reader.ReadSingle();
				portal.min.z = reader.ReadSingle();
				reader.BaseStream.Position += 0x04;
				portal.max.x = reader.ReadSingle();
				portal.max.y = reader.ReadSingle();
				portal.max.z = reader.ReadSingle();
				reader.BaseStream.Position += 0x04;

				Vector q0 = new Vector
				{
					x = reader.ReadSingle(),
					y = reader.ReadSingle(),
					z = reader.ReadSingle(),
				};

				reader.BaseStream.Position += 0x04;

				Vector q1 = new Vector
				{
					x = reader.ReadSingle(),
					y = reader.ReadSingle(),
					z = reader.ReadSingle(),
				};

				reader.BaseStream.Position += 0x04;

				Vector q2 = new Vector
				{
					x = reader.ReadSingle(),
					y = reader.ReadSingle(),
					z = reader.ReadSingle(),
				};

				reader.BaseStream.Position += 0x04;

				Vector q3 = new Vector
				{
					x = reader.ReadSingle(),
					y = reader.ReadSingle(),
					z = reader.ReadSingle(),
				};

				reader.BaseStream.Position += 0x04;

				portal.quad = new Vector[4] { q0, q1, q2, q3 };

				reader.BaseStream.Position += 0x10; // normal

				_portals[i] = portal;
			}

            // Intros
            reader.BaseStream.Position = _dataStart + 0x78;
            _introCount = reader.ReadUInt32();
            _introStart = _dataStart + reader.ReadUInt32();
            _intros = new Intro[_introCount];
            for (int i = 0; i < _introCount; i++)
            {
                reader.BaseStream.Position = _introStart + 0x60 * i;
				_intros[i].rotation.x = reader.ReadSingle();
				_intros[i].rotation.y = reader.ReadSingle();
				_intros[i].rotation.z = reader.ReadSingle();
				reader.BaseStream.Position += 0x04;
				_intros[i].position.x = reader.ReadSingle();
				_intros[i].position.y = reader.ReadSingle();
				_intros[i].position.z = reader.ReadSingle();
				reader.BaseStream.Position += 0x04;
				reader.BaseStream.Position += 0x20;
				_intros[i].objectID = reader.ReadInt16();
				_intros[i].introNum = reader.ReadInt16();
				_intros[i].uniqueID = reader.ReadInt32();
				_intros[i].name = "Intro(" + _intros[i].objectID + ")-" + _intros[i].uniqueID;
                _intros[i].fileName = "";
            }

            // Object Names
            reader.BaseStream.Position = _dataStart + 0x80;
            _objectNameStart = _dataStart + reader.ReadUInt32();
            reader.BaseStream.Position = _objectNameStart;
            List<Int16> objectIDs = new List<Int16>();
            while (true)
            {
                Int16 objectID = reader.ReadInt16();
                if (objectID == 0)
                {
                    break;
                }
                objectIDs.Add(objectID);
            }
			_objectNames = new string[objectIDs.Count];
			_objectIDs = objectIDs.ToArray();

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
			if (_platform == Platform.None)
			{
				_platform = Platform.PC;
			}
			//}

			_modelStart = _dataStart + 0x10;
			_modelCount = (ushort)(1 + _portalCount);
			_models = new IModel[_modelCount];
			reader.BaseStream.Position = _modelStart;
			uint modelData = _dataStart + reader.ReadUInt32();

			DefianceUnitModel terrainModel = new DefianceUnitModel(reader, this, _dataStart, modelData, _name, _platform, _version);
			terrainModel.ReadData(reader, options);
			_models[0] = terrainModel;

			int modelIndex = 1;
			foreach (Portal portal in _portals)
			{
				PortalModel portalModel = new PortalModel(
					this,
					"portal-" + _name + "," + portal.mSignalID + "-" + portal.toLevelName,
					_platform,
					portal.min,
					portal.max,
					portal.quad
				);
				_models[modelIndex++] = portalModel;
			}
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

			writer.BaseStream.Position = 0x000000AC;
			byte[] versionCheck = new byte[4];
			writer.BaseStream.Read(versionCheck, 0, 4);
			writer.BaseStream.Position = 0;
			if (BitConverter.ToUInt32(versionCheck, 0) == 0x04C2046E)
			{
				_asset = Asset.Unit;
			}
			else
			{
				_asset = Asset.Object;
			}
		}

		public void LoadObjectList(String objectListFile)
		{
			if (File.Exists(objectListFile))
			{
				StreamReader reader = File.OpenText(objectListFile);

				int numObjects = Int32.Parse(reader.ReadLine());
				for (int i = 0; i < numObjects; i++)
				{
					string[] line = reader.ReadLine().Split(',');
					_objectNamesList.Add(Int32.Parse(line[0]), line[1]);
				}

				reader.Close();
			}

			for (int i = 0; i < _introCount; i++)
			{
				if (_objectNamesList.ContainsKey(_intros[i].objectID))
				{
					_intros[i].name = _objectNamesList[_intros[i].objectID] + "-" + _intros[i].uniqueID;
					_intros[i].fileName = _objectNamesList[_intros[i].objectID];
				}
			}
		}
	}
}