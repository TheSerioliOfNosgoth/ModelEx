using System;
using System.IO;
using System.Collections.Generic;
using CDC.Objects.Models;

namespace CDC.Objects
{
	public class SR2File : SRFile
	{
		public SR2File(String dataFile, Platform platform, ExportOptions options)
			: base(dataFile, Game.SR2, platform, options)
		{
		}

		protected override void ReadObjectData(BinaryReader reader, ExportOptions options)
		{
			// Object name
			reader.BaseStream.Position = _dataStart + 0x00000024;
			reader.BaseStream.Position = _dataStart + reader.ReadUInt32();
			String strModelName = new String(reader.ReadChars(8));
			_name = Utility.CleanObjectName(strModelName);

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

			// Model data
			reader.BaseStream.Position = _dataStart + 0x00000008;
			_modelCount = reader.ReadUInt16();
			_modelCount = 1; // There are multiple models, but Defiance might have too many. Override for now.
			_animCount = 0; //reader.ReadUInt16();
			reader.BaseStream.Position += 0x02;
			_modelStart = _dataStart + reader.ReadUInt32();
			_animStart = 0; //_dataStart + reader.ReadUInt32();

			_models = new SR2Model[_modelCount];
			for (UInt16 m = 0; m < _modelCount; m++)
			{
				_models[m] = SR2ObjectModel.Load(reader, _dataStart, _modelStart, _name, _platform, m, _version, options);
			}
		}

		protected override void ReadUnitData(BinaryReader reader, ExportOptions options)
		{
			// Adjacent units are seperate from portals.
			// There can be multiple portals to the same unit.
			// Portals
			reader.BaseStream.Position = _dataStart;
			UInt32 m_uConnectionData = _dataStart + reader.ReadUInt32(); // Same as m_uModelData?
			reader.BaseStream.Position = m_uConnectionData + 0x24;
			portalCount = reader.ReadUInt32();
			reader.BaseStream.Position = _dataStart + reader.ReadUInt32();
			_portalNames = new String[portalCount];
			for (int i = 0; i < portalCount; i++)
			{
				String strUnitName = new String(reader.ReadChars(16));
				_portalNames[i] = Utility.CleanName(strUnitName);
				reader.BaseStream.Position += 0x80;
			}

			// Intros
			bool hasRaziel = false;
			reader.BaseStream.Position = _dataStart + 0x44;
			_introCount = reader.ReadUInt32();
			_introStart = _dataStart + reader.ReadUInt32();
			_intros = new Intro[_introCount];
			for (int i = 0; i < _introCount; i++)
			{
				reader.BaseStream.Position = _introStart + 0x60 * i;
				String introName = new String(reader.ReadChars(16));
				//float pi = (float)Math.PI;
				float tau = (float)(Math.PI * 2.0);
				_intros[i].rotation.x = reader.ReadSingle() * tau;
				_intros[i].rotation.y = reader.ReadSingle() * tau;
				_intros[i].rotation.z = reader.ReadSingle() * tau;
				reader.BaseStream.Position += 0x04;
				_intros[i].position.x = reader.ReadSingle();
				_intros[i].position.y = reader.ReadSingle();
				_intros[i].position.z = reader.ReadSingle();
				reader.BaseStream.Position += 0x04;
				reader.BaseStream.Position += 0x10;
				_intros[i].introNum = reader.ReadInt32();
				_intros[i].uniqueID = reader.ReadInt32();
				_intros[i].name = Utility.CleanObjectName(introName) + "-" + _intros[i].uniqueID;
				_intros[i].fileName = Utility.CleanObjectName(introName);

				if (_intros[i].fileName == "raziel")
                {
					hasRaziel = true;
                }
			}

			// Object Names
			reader.BaseStream.Position = _dataStart + 0x4C;
			_objectNameStart = _dataStart + reader.ReadUInt32();
			reader.BaseStream.Position = _objectNameStart;
			List<String> objectNames = new List<String>();
			while (reader.ReadByte() != 0xFF)
			{
				reader.BaseStream.Position--;
				String strObjectName = new String(reader.ReadChars(8));
				objectNames.Add(Utility.CleanObjectName(strObjectName));
				reader.BaseStream.Position += 0x08;
			}

			if (hasRaziel && !objectNames.Contains("raziel"))
            {
				objectNames.Add("raziel");
            }

			_objectNames = objectNames.ToArray();

			// Unit name
			reader.BaseStream.Position = _dataStart + 0x50;
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

			// Model data
			reader.BaseStream.Position = _dataStart;
			_modelCount = 1;
			_modelStart = _dataStart;
			_models = new SR2Model[_modelCount];
			reader.BaseStream.Position = _modelStart;
			UInt32 m_uModelData = _dataStart + reader.ReadUInt32();

			// Material data
			Console.WriteLine("Debug: reading area model 0");
			_models[0] = SR2UnitModel.Load(reader, _dataStart, m_uModelData, _name, _platform, _version, options);

			//if (m_axModels[0].Platform == Platform.Dreamcast ||
			//    m_axModels[1].Platform == Platform.Dreamcast)
			//{
			//    _platform = Platform.Dreamcast;
			//}
		}

		protected override void ResolvePointers(BinaryReader reader, BinaryWriter writer)
		{
			reader.BaseStream.Position = 0;
			writer.BaseStream.Position = 0;

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
				reader.BaseStream.Position += 0x08;
			}

			UInt32 uRegionDataSize = uRegionCount * 0x0C;
			UInt32 uPointerData = (uRegionDataSize & 0x00000003) + ((uRegionDataSize + 0x17) & 0xFFFFFFF0);
			for (UInt32 r = 0; r < uRegionCount; r++)
			{
				reader.BaseStream.Position = uPointerData;
				UInt32 uPointerCount = reader.ReadUInt32();
				UInt32 uPointerDataSize = uPointerCount * 0x04;
				UInt32 uObjectData = uPointerData + ((uPointerDataSize + 0x13) & 0xFFFFFFF0);
				UInt32 uObjectDataSize = (auRegionSizes[r] + 0x0F) & 0xFFFFFFF0;

				reader.BaseStream.Position = uObjectData;
				writer.BaseStream.Position = auRegionPositions[r];
				Byte[] auObjectData = reader.ReadBytes((Int32)auRegionSizes[r]);
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
					UInt32 uValue1 = reader.ReadUInt32();
					UInt32 uValue2 = uValue1 & 0x003FFFFF;
					UInt32 uValue3 = uValue1 >> 0x16;

					auAddresses[p] += auRegionPositions[r];
					auValues[p] = auRegionPositions[uValue3] + uValue2;

					writer.BaseStream.Position = auAddresses[p];
					writer.Write(auValues[p]);
				}

				uPointerData = uObjectData + uObjectDataSize;
			}

			writer.BaseStream.Position = 0x00000080;
			byte[] versionCheck = new byte[4];
			writer.BaseStream.Read(versionCheck, 0, 4);
			writer.BaseStream.Position = 0;
			if (BitConverter.ToUInt32(versionCheck, 0) == 0x04C2041D)
			{
				_asset = Asset.Unit;
			}
			else
			{
				_asset = Asset.Object;
			}
		}
	}
}