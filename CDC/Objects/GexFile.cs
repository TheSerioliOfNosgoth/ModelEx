using System;
using System.IO;
using System.Collections.Generic;
using CDC.Objects.Models;

namespace CDC.Objects
{
    public class GexFile : SRFile
    {
        public const UInt32 RETAIL_VERSION = 0x00000002;

        protected SRFile[] _objects;
        public SRFile[] Objects { get { return _objects; } }

        protected GexFile(String name, CDC.Objects.ExportOptions options, BinaryReader xReader)
        {
            _name = name;
            _game = Game.Gex;
            _asset = Asset.Object;
            _dataStart = (UInt32)xReader.BaseStream.Position;

            ReadObjectData(xReader, options);
        }

        public GexFile(String strFileName, CDC.Objects.ExportOptions options)
            : base(strFileName, Game.Gex, options)
        {
        }

        protected override void ReadHeaderData(BinaryReader xReader, CDC.Objects.ExportOptions options)
        {
            _dataStart = 0;

            // Could use unit version number instead of thing below.
            // Check that's what SR2 does.
            //xReader.BaseStream.Position = m_uDataStart + 0xF0;
            //UInt32 unitVersionNumber = xReader.ReadUInt32();
            //if (unitVersionNumber != 0x3C20413B)

            // Moved to ResolvePointers due to not knowing how else to tell.
            //xReader.BaseStream.Position = 0x00000000;
            //if (xReader.ReadUInt32() == 0x00000000)
            //{
            //    m_eFileType = FileType.Unit;
            //}
            //else
            //{
            //    m_eFileType = FileType.Object;
            //}
        }

        protected override void ReadObjectData(BinaryReader xReader, CDC.Objects.ExportOptions options)
        {
            // Object name
            xReader.BaseStream.Position = _dataStart + 0x00000024;
            xReader.BaseStream.Position = xReader.ReadUInt32();
            String strModelName = new String(xReader.ReadChars(8));
            _name = Utility.CleanName(strModelName);

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
            xReader.BaseStream.Position = _dataStart + 0x00000008;
            _modelCount = xReader.ReadUInt16();
            _animCount = xReader.ReadUInt16();
            _modelStart = xReader.ReadUInt32();
            _animStart = xReader.ReadUInt32();

            _models = new GexModel[_modelCount];
            Platform ePlatform = _platform;
            for (UInt16 m = 0; m < _modelCount; m++)
            {
                Console.WriteLine(string.Format("Debug: reading object model {0} / {1}", m, (_modelCount - 1)));
                _models[m] = GexObjectModel.Load(xReader, _dataStart, _modelStart, _name, _platform, m, _version, options);
            }
            if (options.ForcedPlatform == CDC.Platform.None)
            {
                _platform = ePlatform;
            }
        }

        protected override void ReadUnitData(BinaryReader xReader, CDC.Objects.ExportOptions options)
        {
            bool validVersion = false;

            if (options.ForcedPlatform != Platform.None)
            {
                _platform = options.ForcedPlatform;
            }

            if (!validVersion)
            {
                xReader.BaseStream.Position = _dataStart + 0x08;
                _version = xReader.ReadUInt32();
                if (_version == RETAIL_VERSION)
                {
                    validVersion = true;
                }
            }

            if (!validVersion)
            {
                throw new Exception("Wrong version number for level x");
            }

            // Instance types
            xReader.BaseStream.Position = _dataStart + 0x3C;
            _instanceTypeStart = xReader.ReadUInt32();
            xReader.BaseStream.Position = _instanceTypeStart;
            List<String> xInstanceList = new List<String>();
            List<SRFile> xObjectList = new List<SRFile>();
            while (true)
            {
                UInt32 objectAddress = xReader.ReadUInt32();
                if (objectAddress == _instanceTypeStart)
                {
                    break;
                }

                long oldPos = xReader.BaseStream.Position;

                xReader.BaseStream.Position = objectAddress + 0x00000024;
                xReader.BaseStream.Position = xReader.ReadUInt32();
                String strInstanceTypeName = new String(xReader.ReadChars(8));
                strInstanceTypeName = Utility.CleanObjectName(strInstanceTypeName);

                xReader.BaseStream.Position = objectAddress;
                GexFile gexObject = new GexFile(strInstanceTypeName, options, xReader);

                xInstanceList.Add(strInstanceTypeName);
                xObjectList.Add(gexObject);

                xReader.BaseStream.Position = oldPos;
            }
            _instanceTypeNames = xInstanceList.ToArray();
            _objects = xObjectList.ToArray();

            // Instances
            xReader.BaseStream.Position = _dataStart + 0x7C;
            _instanceCount = xReader.ReadUInt32();
            _instanceStart = xReader.ReadUInt32();
            _instanceNames = new String[_instanceCount];
            for (int i = 0; i < _instanceCount; i++)
            {
                xReader.BaseStream.Position = _instanceStart + 0x34 * i;

                UInt32 objectAddress = xReader.ReadUInt32();
                if (objectAddress == 0)
                {
                    _instanceNames[i] = "[Unknown]";
                }
                else
                {
                    xReader.BaseStream.Position = objectAddress + 0x00000024;
                    xReader.BaseStream.Position = xReader.ReadUInt32();
                    String strInstanceName = new String(xReader.ReadChars(8));
                    _instanceNames[i] = Utility.CleanObjectName(strInstanceName);
                }
            }

            // Unit name. No names in Gex :(
            //xReader.BaseStream.Position = _dataStart + 0x98;
            //xReader.BaseStream.Position = xReader.ReadUInt32();
            //String strModelName = new String(xReader.ReadChars(8));
            //_name = Utility.CleanName(strModelName);

            // Texture type
            if (options.ForcedPlatform == CDC.Platform.None)
            {
                _platform = Platform.PSX;
            }

            // Model data
            xReader.BaseStream.Position = _dataStart;
            _modelCount = 1;
            _modelStart = _dataStart;
            _models = new GexModel[_modelCount];
            xReader.BaseStream.Position = _modelStart;
            UInt32 m_uModelData = xReader.ReadUInt32();

            // Material data
            Console.WriteLine("Debug: reading area model 0");
            _models[0] = GexUnitModel.Load(xReader, _dataStart, m_uModelData, _name, _platform, _version, options);
        }

        protected override void ResolvePointers(BinaryReader xReader, BinaryWriter xWriter)
        {
            UInt32 uDataStart = ((xReader.ReadUInt32() >> 9) << 11) + 0x00000800;
            if (xReader.ReadUInt32() == 0x00000000)
            {
                _asset = Asset.Unit;
            }
            else
            {
                _asset = Asset.Object;
            }

            xReader.BaseStream.Position = uDataStart;
            xWriter.BaseStream.Position = 0;

            xReader.BaseStream.CopyTo(xWriter.BaseStream);
        }
    }
}