using System;
using System.IO;
using System.Collections.Generic;

namespace CDC
{
	public class TRLFile : DataFile
	{
		enum SectionType
		{
			General = 0,
			Animation = 2,
			Texture = 5,
			Wave = 6,
			Dtp = 7
		}

		public class Relocation
		{
			public int Section;
			public uint Offset;
		}

		class Section
		{
			public int Size;
			public long Offset;
			public long NewOffset;
			public SectionType Type;
			public uint Id;
			public uint NumRelocations;
			public readonly List<Relocation> Relocations = new List<Relocation>();
		}

		Int16[] _objectIDs;
		SortedList<int, string> _objectNamesList = new SortedList<int, string>();

		public TRLFile(String dataFileName, Platform platform, ExportOptions options)
			: base(dataFileName, Game.TRL, platform, options)
		{
		}

		public TRLFile(String dataFileName, String objectListFile, Platform platform, ExportOptions options)
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
			// Object name. No names in TRL :(
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

			// Model data
			reader.BaseStream.Position = _dataStart + 0x00000018;
			_modelCount = reader.ReadUInt16();
			_modelCount = 1; // There are multiple models, but TRL might have too many. Override for now.
			_animCount = 0; //reader.ReadUInt16();
			reader.BaseStream.Position += 0x06;
			_modelStart = _dataStart + reader.ReadUInt32();
			_animStart = 0; //_dataStart + reader.ReadUInt32();

			_models = new IModel[_modelCount];
			for (UInt16 m = 0; m < _modelCount; m++)
			{
				reader.BaseStream.Position = _modelStart + (m * 4);
				uint modelData = _dataStart + reader.ReadUInt32();
				reader.BaseStream.Position = modelData;

				string modelName = _name + "-" + m.ToString();
				TRLObjectModel model = new TRLObjectModel(reader, this, _dataStart, modelData, modelName, _platform, _version);
				model.ReadData(reader, options);
				_models[m] = model;
			}
		}

		protected override void ReadUnitData(BinaryReader reader, ExportOptions options)
		{
			// Portals
			reader.BaseStream.Position = _dataStart;
			UInt32 terrainData = _dataStart + reader.ReadUInt32(); // Same as _modelData?
			reader.BaseStream.Position = terrainData + 0x0C;
			_portalCount = reader.ReadUInt32();
			reader.BaseStream.Position = _dataStart + reader.ReadUInt32();
			_portals = new Portal[_portalCount];
			for (int i = 0; i < _portalCount; i++)
			{
				Portal portal = new Portal();
				portal.toLevelName = new String(reader.ReadChars(30));
				portal.toLevelName = Utility.CleanName(portal.toLevelName);
				short toSignalID = reader.ReadInt16();
				portal.toLevelName += "," + toSignalID;
				portal.mSignalID = reader.ReadInt16();
				reader.BaseStream.Position += 0x02; // streamID
				reader.BaseStream.Position += 0x04; // closeVertList
				reader.BaseStream.Position += 0x04; // activeDist
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

			// BGObjects
			reader.BaseStream.Position = terrainData + 0x30;
			_bgObjectCount = reader.ReadUInt32();
			_bgObjectStart = reader.ReadUInt32();

			// BGInstances
			reader.BaseStream.Position = terrainData + 0x28;
			_bgInstanceCount = reader.ReadUInt32();
			_bgInstanceStart = reader.ReadUInt32();

			_bgInstances = new BGInstance[_bgInstanceCount];
			for (int i = 0; i < _bgInstanceCount; i++)
			{
				reader.BaseStream.Position = _bgInstanceStart + (0xF0 * i);

				_bgInstances[i].matrix = new Matrix()
				{
					v0 = new Vector4 { x = reader.ReadSingle(), y = reader.ReadSingle(), z = reader.ReadSingle(), w = reader.ReadSingle() },
					v1 = new Vector4 { x = reader.ReadSingle(), y = reader.ReadSingle(), z = reader.ReadSingle(), w = reader.ReadSingle() },
					v2 = new Vector4 { x = reader.ReadSingle(), y = reader.ReadSingle(), z = reader.ReadSingle(), w = reader.ReadSingle() },
					v3 = new Vector4 { x = reader.ReadSingle(), y = reader.ReadSingle(), z = reader.ReadSingle(), w = reader.ReadSingle() },
				};
				reader.BaseStream.Position += 0x80;
				_bgInstances[i].bgObject = reader.ReadUInt32();
				reader.BaseStream.Position += 0x10;
				_bgInstances[i].id = reader.ReadUInt16();
				_bgInstances[i].modelIndex = (int)(_bgInstances[i].bgObject - _bgObjectStart) / 0x60;
				_bgInstances[i].name = "bgInstance(" + _bgInstances[i].modelIndex + ")-" + _bgInstances[i].id;
			}

			// Intros
			reader.BaseStream.Position = _dataStart + 0x74;
			_introCount = reader.ReadUInt32();
			_introStart = _dataStart + reader.ReadUInt32();
			_intros = new Intro[_introCount];
			for (int i = 0; i < _introCount; i++)
			{
				reader.BaseStream.Position = _introStart + 0x70 * i;
				_intros[i].rotation.x = reader.ReadSingle();
				_intros[i].rotation.y = reader.ReadSingle();
				_intros[i].rotation.z = reader.ReadSingle();
				reader.BaseStream.Position += 0x4;
				_intros[i].position.x = reader.ReadSingle();
				_intros[i].position.y = reader.ReadSingle();
				_intros[i].position.z = reader.ReadSingle();
				reader.BaseStream.Position += 0x4;
				reader.BaseStream.Position += 0x30;
				_intros[i].objectID = reader.ReadInt16();
				_intros[i].introNum = reader.ReadInt16();
				_intros[i].uniqueID = reader.ReadInt32();
				_intros[i].name = "Intro(" + _intros[i].objectID + ")-" + _intros[i].uniqueID;
				_intros[i].fileName = "";
			}

			// Object Names
			reader.BaseStream.Position = _dataStart + 0x7C;
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
			reader.BaseStream.Position = _dataStart + 0x80;
			reader.BaseStream.Position = _dataStart + reader.ReadUInt32();
			String strModelName = new String(reader.ReadChars(32)); //  128 for Underworld.
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

			_modelStart = _dataStart;
			_modelCount = (ushort)(1 + _portalCount + _bgObjectCount);
			_models = new IModel[_modelCount];
			reader.BaseStream.Position = _modelStart;
			uint modelData = _dataStart + reader.ReadUInt32();

			TRLUnitModel terrainModel = new TRLUnitModel(reader, this, _dataStart, modelData, _name, _platform, _version);
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

			uint bgObjectData = _bgObjectStart;
			for (int i = 0; i < _bgObjectCount; i++)
			{
				TRLBGObjectModel bgObjectModel = new TRLBGObjectModel(
					reader,
					this,
					_dataStart,
					bgObjectData,
					"bgobject-" + i,
					_platform,
					_version
				);

				bgObjectModel.ReadData(reader, options);
				_models[modelIndex++] = bgObjectModel;
				bgObjectData += 0x60;
			}
		}

		protected override void ResolvePointers(BinaryReader reader, BinaryWriter writer)
		{
			reader.BaseStream.Position = 0;
			writer.BaseStream.Position = 0;

			reader.BaseStream.Position += 0x04; // Version number

			List<Section> sections = new List<Section>();

			Int32 numSections = reader.ReadInt32();

			// read all section headers
			for (int i = 0; i < numSections; i++)
			{
				Section section = new Section();
				section.Size = reader.ReadInt32();
				section.Type = (SectionType)reader.ReadByte();

				reader.BaseStream.Position += 3; // skip past pad and version id

				UInt32 packedData = reader.ReadUInt32();

				section.Id = reader.ReadUInt32();
				section.NumRelocations = packedData >> 8;

				reader.BaseStream.Position += 4; // skip past specMask

				sections.Add(section);
			}

			// go trough all sections and relocations
			foreach (Section section in sections)
			{
				// relocation data is before section
				for (int i = 0; i < section.NumRelocations; i++)
				{
					UInt16 typeAndSectionInfo = reader.ReadUInt16();
					UInt16 type = reader.ReadUInt16();
					UInt32 offset = reader.ReadUInt32();

					Relocation relocation = new Relocation
					{
						Section = typeAndSectionInfo >> 3,
						Offset = offset
					};

					section.Relocations.Add(relocation);
				}

				section.Offset = reader.BaseStream.Position;

				// copy the section data to the new stream and store the offet it was written to
				section.NewOffset = writer.BaseStream.Position;
				Byte[] data = reader.ReadBytes(section.Size);
				writer.Write(data);
			}

			// relocate all sections
			foreach (Section section in sections)
			{
				foreach (Relocation relocation in section.Relocations)
				{
					reader.BaseStream.Position = section.Offset + relocation.Offset;
					var targetSection = sections[relocation.Section];

					// read the offset
					byte[] pointer = new byte[4];
					reader.BaseStream.Read(pointer, 0, 4);

					UInt32 offset = BitConverter.ToUInt32(pointer, 0);

					// add together section offset and offset
					writer.BaseStream.Position = section.NewOffset + relocation.Offset;
					pointer = BitConverter.GetBytes(targetSection.NewOffset + offset);

					// write back relocated pointer
					writer.BaseStream.Write(pointer, 0, 4);
				}
			}

			writer.BaseStream.Position = 0x000000A8;
			byte[] versionCheck = new byte[4];
			writer.BaseStream.Read(versionCheck, 0, 4);
			writer.BaseStream.Position = 0;
			if (BitConverter.ToUInt32(versionCheck, 0) == 0x04C204BB)
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