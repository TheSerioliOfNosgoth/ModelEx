using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace BenLincoln.TheLostWorlds.CDTextures
{
    public enum TextureFileType
    {
        Unknown,
        SoulReaverPlaystation,
        SoulReaverPC,
        SoulReaverDreamcast,
        SoulReaver2Playstation2,
        SoulReaver2PC
    }

    public abstract class TextureFile
    {
        protected string _FilePath;
        protected TextureFileType _FileType;
        protected string _FileTypeName;
        protected FileInfo _FileInfo;
        protected int _TextureCount;

        protected bool _ErrorOccurred;
        protected string _LastErrorMessage;

        #region Properties

        public string FilePath
        {
            get
            {
                return _FilePath;
            }
        }

        public TextureFileType FileType
        {
            get
            {
                return _FileType;
            }
        }

        public string FileTypeName
        {
            get
            {
                return _FileTypeName;
            }
        }

        public FileInfo FileInfo
        {
            get
            {
                return _FileInfo;
            }
        }

        public int TextureCount
        {
            get
            {
                return _TextureCount;
            }
        }

        public bool ErrorOccurred
        {
            get
            {
                return _ErrorOccurred;
            }
        }

        public string LastErrorMessage
        {
            get
            {
                return _LastErrorMessage;
            }
        }

        #endregion

        public TextureFile(string path)
        {
            _ErrorOccurred = false;
            _LastErrorMessage = "";
            _FilePath = path;
            if (path == null || path == "")
            {
                throw new Exception("No file specified.");
            }

            if (File.Exists(path))
            {
                try
                {
                    _FileInfo = new FileInfo(path);
                }
                catch (IOException iEx)
                {
                    throw new IOException("Unable to access the file '" + path + "'", iEx);
                }
            }
            else
            {
                throw new IOException("File '" + path + "' not found.");
            }
        }

        protected abstract int _GetTextureCount();
        // implemented in subclasses to handle specific file types

        public Bitmap GetTextureAsBitmap(int index)
        {
            if (index < 0)
            {
                throw new IndexOutOfRangeException("Texture index cannot be negative.");
            }
            if (index > (_TextureCount - 1))
            {
                string errorMessage = "Texture " + index.ToString() +
                    " was requested but the file '" + _FilePath + "' only contains enough data for " + _TextureCount.ToString() +
                    " textures.";
                //throw new IndexOutOfRangeException(errorMessage);
                Console.WriteLine(errorMessage);
                return _GetTextureAsBitmap(0);
            }
            return _GetTextureAsBitmap(index);
        }

        protected abstract Bitmap _GetTextureAsBitmap(int index);
        // implemented in subclasses to handle specific file types

        public abstract MemoryStream GetDataAsStream(int index);
        // implemented in subclasses to handle specific file types

        public virtual MemoryStream GetTextureWithCLUTAsStream(int textureID, ushort clut)
        {
            Console.WriteLine("GetTextureWithCLUTAsStream is not implemented for this class");
            return null;
        }

        public virtual void ExportFile(int index, string outPath)
        {
            _ErrorOccurred = false;
            _LastErrorMessage = "";
            try
            {
                MemoryStream iStream = GetDataAsStream(index);
                BinaryReader iReader = new BinaryReader(iStream);
                FileStream oStream = new FileStream(outPath,
                    FileMode.Create, FileAccess.Write);
                BinaryWriter oWriter = new BinaryWriter(oStream);
                for (int byteNum = 0; byteNum < iStream.Length; byteNum++)
                {
                    oWriter.Write(iReader.ReadByte());
                }

                oWriter.Close();
                oStream.Close();
                iReader.Close();
                iStream.Close();
            }
            catch (Exception ex)
            {
                _ErrorOccurred = true;
                _LastErrorMessage = ex.Message;
            }
        }

        // for threaded exports
        public virtual void ExportFileThreaded(object parms)
        {
            try
            {
                ArrayList parmList = (ArrayList)parms;
                ExportFile((int)parmList[0], (string)parmList[1]);
            }
            catch (InvalidCastException)
            {
                throw new InvalidCastException("Passed object must be an arraylist with the first element being an int and the second a string.");
            }
        }

        public static TextureFileType GetFileType(string path)
        {
            string fileExtension = Path.GetExtension(path).ToLower();

            switch (fileExtension)
            {
                case ".vrm":
                    // determine if PC / Playstation / etc
                    return VRMTextureFile.GetVRMType(path);
                case ".big":
                    return TextureFileType.SoulReaverPC;
                case ".vq":
                    return TextureFileType.SoulReaverDreamcast;
                case ".crm":
                    return TextureFileType.SoulReaverPlaystation;
                default:
                    return TextureFileType.Unknown;
            }
        }
    }
}
