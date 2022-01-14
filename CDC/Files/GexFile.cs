using System;
using System.IO;
using System.Collections.Generic;
using CDC.Objects.Models;

namespace CDC.Objects
{
	public class GexFile : SRFile
	{
		public const UInt32 RETAIL_VERSION = 0x00000002;

		protected List<ushort> _tPages = new List<ushort>();
		public ushort[] TPages { get { return _tPages.ToArray(); } }

		protected SRFile[] _objects;
		public SRFile[] Objects { get { return _objects; } }

		protected GexFile(String name, CDC.Objects.ExportOptions options, BinaryReader reader)
		{
			_name = name;
			_game = Game.Gex;
			_asset = Asset.Object;
			_dataStart = (UInt32)reader.BaseStream.Position;

			ReadObjectData(reader, options);
		}

		public GexFile(String strFileName, CDC.Objects.ExportOptions options)
			: base(strFileName, Game.Gex, options)
		{
		}

		protected override void ReadHeaderData(BinaryReader reader, CDC.Objects.ExportOptions options)
		{
			_dataStart = 0;

			// Could use unit version number instead of thing below.
			// Check that's what SR2 does.
			//reader.BaseStream.Position = _dataStart + 0xF0;
			//UInt32 unitVersionNumber = reader.ReadUInt32();
			//if (unitVersionNumber != 0x3C20413B)

			// Moved to ResolvePointers due to not knowing how else to tell.
			//reader.BaseStream.Position = 0x00000000;
			//if (reader.ReadUInt32() == 0x00000000)
			//{
			//    m_eFileType = FileType.Unit;
			//}
			//else
			//{
			//    m_eFileType = FileType.Object;
			//}
		}

		protected override void ReadObjectData(BinaryReader reader, CDC.Objects.ExportOptions options)
		{
			// Object name
			reader.BaseStream.Position = _dataStart + 0x00000024;
			reader.BaseStream.Position = reader.ReadUInt32();
			String strModelName = new String(reader.ReadChars(8));
			_name = Utility.CleanObjectName(strModelName);

			// Texture type
			if (options.ForcedPlatform == CDC.Platform.None)
			{
				_platform = Platform.PSX;
			}
			else
			{
				_platform = options.ForcedPlatform;
			}

			// Model data
			reader.BaseStream.Position = _dataStart + 0x00000008;
			_modelCount = reader.ReadUInt16();
			_animCount = reader.ReadUInt16();
			_modelStart = reader.ReadUInt32();
			_animStart = reader.ReadUInt32();

			_models = new GexModel[_modelCount];
			Platform ePlatform = _platform;
			for (UInt16 m = 0; m < _modelCount; m++)
			{
				Console.WriteLine(string.Format("Debug: reading object model {0} / {1}", m, (_modelCount - 1)));
				_models[m] = GexObjectModel.Load(reader, _dataStart, _modelStart, _name, _platform, m, _version, _tPages, options);
			}
			if (options.ForcedPlatform == CDC.Platform.None)
			{
				_platform = ePlatform;
			}
		}

		protected override void ReadUnitData(BinaryReader reader, CDC.Objects.ExportOptions options)
		{
			bool validVersion = false;

			if (options.ForcedPlatform != Platform.None)
			{
				_platform = options.ForcedPlatform;
			}

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
			List<String> introList = new List<String>();
			List<SRFile> xObjectList = new List<SRFile>();
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
				String strObjectName = new String(reader.ReadChars(8));
				strObjectName = Utility.CleanObjectName(strObjectName);

				reader.BaseStream.Position = objectAddress;
				GexFile gexObject = new GexFile(strObjectName, options, reader);

				introList.Add(strObjectName);
				xObjectList.Add(gexObject);

				reader.BaseStream.Position = oldPos;
			}
			_objectNames = introList.ToArray();
			_objects = xObjectList.ToArray();

			// In
			reader.BaseStream.Position = _dataStart + 0x7C;
			_introCount = reader.ReadUInt32();
			_introStart = reader.ReadUInt32();
			_intros = new Intro[_introCount];
			for (int i = 0; i < _introCount; i++)
			{
				reader.BaseStream.Position = _introStart + 0x34 * i;

				UInt32 objectAddress = reader.ReadUInt32();
				if (objectAddress == 0)
				{
					_intros[i].name = "Unknown-" + _intros[i].ID;
				}
				else
				{
					reader.BaseStream.Position = objectAddress + 0x00000024;
					reader.BaseStream.Position = reader.ReadUInt32();
					String strIntroName = new String(reader.ReadChars(8));
					_intros[i].name = Utility.CleanObjectName(strIntroName) + "-" + _intros[i].ID;
				}
			}

			// Unit name. No names in Gex :(
			//reader.BaseStream.Position = _dataStart + 0x98;
			//reader.BaseStream.Position = reader.ReadUInt32();
			//String strModelName = new String(reader.ReadChars(8));
			//_name = Utility.CleanName(strModelName);

			// Texture type
			if (options.ForcedPlatform == CDC.Platform.None)
			{
				_platform = Platform.PSX;
			}

			// Model data
			reader.BaseStream.Position = _dataStart;
			_modelCount = 1;
			_modelStart = _dataStart;
			_models = new GexModel[_modelCount];
			reader.BaseStream.Position = _modelStart;
			UInt32 m_uModelData = reader.ReadUInt32();

			// Material data
			Console.WriteLine("Debug: reading area model 0");
			_models[0] = GexUnitModel.Load(reader, _dataStart, m_uModelData, _name, _platform, _version, _tPages, options);
		}

		protected override void ResolvePointers(BinaryReader reader, BinaryWriter writer)
		{
			UInt32 dataStart = ((reader.ReadUInt32() >> 9) << 11) + 0x00000800;
			if (reader.ReadUInt32() == 0x00000000)
			{
				_asset = Asset.Unit;
			}
			else
			{
				_asset = Asset.Object;
			}

			reader.BaseStream.Position = dataStart;
			writer.BaseStream.Position = 0;

			reader.BaseStream.CopyTo(writer.BaseStream);
		}
	}
}