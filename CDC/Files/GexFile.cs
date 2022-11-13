using System;
using System.IO;
using System.Collections.Generic;
using TPages = BenLincoln.TheLostWorlds.CDTextures.PSXTextureDictionary;

namespace CDC
{
	public class GexFile : DataFile
	{
		public const UInt32 RETAIL_VERSION = 0x00000002;

		// ushort tPageMask = 0x001F (x, 0-31) | 0x0010 (y1, 0-1) | 0x0800 (y2, 0-2) | 0x0180 (tp, 0-3)| 0x0060 (abr, 0-3)
		// ushort clutMask = 0x003F (x, 0-63) | 0xFFC0 (y, 0-1023)
		protected TPages _tPages = new TPages(0x001F | 0x0010 | 0x0800 | 0x0080, 0x003F | 0xFFC0);
		public TPages TPages { get { return _tPages; } }

		protected DataFile[] _objects;
		public DataFile[] Objects { get { return _objects; } }

		protected GexFile(String name, ExportOptions options, BinaryReader reader)
		{
			_name = name;
			_game = Game.Gex;
			_asset = Asset.Object;
			_dataStart = (UInt32)reader.BaseStream.Position;

			ReadObjectData(reader, options);
		}

		public GexFile(String dataFileName, Platform platform, ExportOptions options)
			: base(dataFileName, Game.Gex, platform, options)
		{
		}

		protected override void ReadObjectData(BinaryReader reader, ExportOptions options)
		{
			// Object name
			reader.BaseStream.Position = _dataStart + 0x00000024;
			reader.BaseStream.Position = reader.ReadUInt32();
			String strModelName = new String(reader.ReadChars(8));
			_name = Utility.CleanObjectName(strModelName);

			// Texture type
			if (_platform == Platform.None)
			{
				_platform = Platform.PSX;
			}

			// Model data
			reader.BaseStream.Position = _dataStart + 0x00000008;
			_modelCount = reader.ReadUInt16();
			_animCount = reader.ReadUInt16();
			_modelStart = reader.ReadUInt32();
			_animStart = reader.ReadUInt32();

			_models = new IModel[_modelCount];
			for (UInt16 m = 0; m < _modelCount; m++)
			{
				reader.BaseStream.Position = _modelStart + (m * 4);
				uint modelData = reader.ReadUInt32();
				reader.BaseStream.Position = modelData;

				string modelName = _name + "-" + m.ToString();
				GexObjectModel model = new GexObjectModel(reader, this, _dataStart, modelData, modelName, _platform, _version, _tPages);
				model.ReadData(reader, options);
				_models[m] = model;
			}
		}

		protected override void ReadUnitData(BinaryReader reader, ExportOptions options)
		{
			bool validVersion = false;

			if (!validVersion)
			{
				reader.BaseStream.Position = _dataStart + 0x08;
				_version = reader.ReadUInt32();
				if (_version == RETAIL_VERSION)
				{
					validVersion = true;
				}
			}

			if (!validVersion)
			{
				throw new Exception("Wrong version number for level x");
			}

			// Object Names
			reader.BaseStream.Position = _dataStart + 0x3C;
			_objectNameStart = reader.ReadUInt32();
			reader.BaseStream.Position = _objectNameStart;
			List<string> objectNames = new List<string>();
			List<DataFile> objectList = new List<DataFile>();
			while (true)
			{
				UInt32 objectAddress = reader.ReadUInt32();
				if (objectAddress == _objectNameStart)
				{
					break;
				}

				long oldPos = reader.BaseStream.Position;

				reader.BaseStream.Position = objectAddress + 0x00000024;
				reader.BaseStream.Position = reader.ReadUInt32();
				string objectName = new string(reader.ReadChars(8));
				objectName = Utility.CleanObjectName(objectName);

				reader.BaseStream.Position = objectAddress;
				GexFile gexObject = new GexFile(objectName, options, reader);

				objectNames.Add(objectName);
				objectList.Add(gexObject);

				reader.BaseStream.Position = oldPos;
			}
			_objectNames = objectNames.ToArray();
			_objects = objectList.ToArray();

			// Intros
			reader.BaseStream.Position = _dataStart + 0x7C;
			_introCount = reader.ReadUInt32();
			_introStart = reader.ReadUInt32();
			_intros = new Intro[_introCount];
			for (int i = 0; i < _introCount; i++)
			{
				reader.BaseStream.Position = _introStart + 0x34 * i;

				UInt32 objectStart = reader.ReadUInt32();
				if (objectStart == 0)
				{
					_intros[i].name = "Intro(0)-" + _intros[i].uniqueID;
					_intros[i].fileName = "";
				}
				else
				{
					reader.BaseStream.Position = objectStart + 0x00000024;
					reader.BaseStream.Position = reader.ReadUInt32();
					String introName = new String(reader.ReadChars(8));
					_intros[i].name = Utility.CleanObjectName(introName) + "-" + _intros[i].uniqueID;
					_intros[i].fileName = Utility.CleanObjectName(introName);
				}
			}

			// Unit name. No names in Gex :(
			//reader.BaseStream.Position = _dataStart + 0x98;
			//reader.BaseStream.Position = reader.ReadUInt32();
			//String strModelName = new String(reader.ReadChars(8));
			//_name = Utility.CleanName(strModelName);

			// Texture type
			if (_platform == Platform.None)
			{
				_platform = Platform.PSX;
			}

			reader.BaseStream.Position = _dataStart;
			_modelCount = 1;
			_modelStart = _dataStart;
			_models = new IModel[_modelCount];
			reader.BaseStream.Position = _modelStart;
			uint modelData = reader.ReadUInt32();

			GexUnitModel model = new GexUnitModel(reader, this, _dataStart, modelData, _name, _platform, _version, _tPages);
			model.ReadData(reader, options);
			_models[0] = model;
		}

		protected override void ResolvePointers(BinaryReader reader, BinaryWriter writer)
		{
			UInt32 dataStart = ((reader.ReadUInt32() >> 9) << 11) + 0x00000800;
			_asset = Asset.Unit;

			reader.BaseStream.Position = dataStart;
			writer.BaseStream.Position = 0;

			reader.BaseStream.CopyTo(writer.BaseStream);
		}
	}
}