using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace ModelEx
{
    class Win32Icons
    {
        [DllImport("Shell32.dll")]
        private static extern IntPtr SHGetFileInfo
        (
            string pszPath,
            uint dwFileAttributes,
            out SHFILEINFO psfi,
            uint cbfileInfo,
            SHGFI uFlags
        );

        [DllImport("Shell32.dll")]
        private static extern int SHGetStockIconInfo(
            SHSTOCKICONID siid,
            SHGSI uFlags,
            ref SHSTOCKICONINFO psii
        );

        [DllImport("User32.dll")]
        private static extern int DestroyIcon(IntPtr hIcon);

        public enum SHSTOCKICONID
        {
            DocNoAssoc = 0,
            DocAssoc = 1,
            Application = 2,
            Folder = 3,
            FolderOpen = 4,
            Drive525 = 5,
            Drive35 = 6,
            DriveRemove = 7,
            DriveFixed = 8,
            DriveNet = 9,
            DriveNetDisabled = 10,
            DriveCD = 11,
            DriveRAM = 12,
            World = 13,
            Server = 15,
            Printer = 16,
            MyNetwork = 17,
            Find = 22,
            Help = 23,
            Share = 28,
            Link = 29,
            SlowFile = 30,
            Recycler = 31,
            RecyclerFull = 32,
            MediaCDAudio = 40,
            Lock = 47,
            AutoList = 49,
            PrinterNet = 50,
            ServerShare = 51,
            PrinterFax = 52,
            PrinterFaxNet = 53,
            PrinterFile = 54,
            Stack = 55,
            MediaSVCD = 56,
            StuffedFolder = 57,
            DriveUnknown = 58,
            DriveDVD = 59,
            MediaDVD = 60,
            MediaDVDRAM = 61,
            MediaDVDRW = 62,
            MediaDVDR = 63,
            MediaDVDROM = 64,
            MediaCDAudioPLUS = 65,
            MediaCDRW = 66,
            MediaCDR = 67,
            MediaCDBurn = 68,
            MediaBlankCD = 69,
            MediaCDROM = 70,
            AudioFiles = 71,
            ImageFiles = 72,
            VideoFiles = 73,
            MixedFiles = 74,
            FolderBack = 75,
            FolderFront = 76,
            Shield = 77,
            Warning = 78,
            Info = 79,
            Error = 80,
            Key = 81,
            Software = 82,
            Rename = 83,
            Delete = 84,
            MediaAudioDVD = 85,
            MediaMovieDVD = 86,
            MediaEnhancedCD = 87,
            MediaEnhancedDVD = 88,
            MediaHDDVD = 89,
            MediaBluRay = 90,
            MediaVCD = 91,
            MediaDVDPlusR = 92,
            MediaDVDPlusRW = 93,
            DesktopPC = 94,
            MobilePC = 95,
            Users = 96,
            MediaSmartMedia = 97,
            MediaCompactFlash = 98,
            DeviceCellPhone = 99,
            DeviceCamera = 100,
            DeviceVideoCamera = 101,
            DeviceAudioPlayer = 102,
            NetworkConnect = 103,
            Internet = 104,
            ZipFile = 105,
            Settings = 106,
            DriveHDDVD = 132,
            DriveBD = 133,
            MediaHDDVDROM = 134,
            MediaHDDVDR = 135,
            MediaHDDVDRAM = 136,
            MediaBDROM = 137,
            MediaBDR = 138,
            MediaBDRE = 139,
            ClusteredDrive = 140,
            MaxIcons = 175
        }

        private enum SHGFI
        {
            SmallIcon = 0x00000001,
            LargeIcon = 0x00000000,
            Icon = 0x00000100,
            DisplayName = 0x00000200,
            Typename = 0x00000400,
            SysIconIndex = 0x00004000,
            UseFileAttributes = 0x00000010
        }

        public enum SHGSI
        {
            IconLocation = 0,
            Icon = 0x000000100,
            SysIconIndex = 0x000004000,
            LinkOverlay = 0x000008000,
            Selected = 0x000010000,
            LargeIcon = 0x000000000,
            SmallIcon = 0x000000001,
            ShellIconSize = 0x000000004
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct SHFILEINFO
        {
            public void ZeroMemory()
            {
                hIcon = IntPtr.Zero; iIcon = 0; dwAttributes = 0; szDisplayName = ""; szTypeName = "";
            }

            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        };

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct SHSTOCKICONINFO
        {
            public void ZeroMemory()
            {
                cbSize = 0; hIcon = IntPtr.Zero; iSysImageIndex = 0; iIcon = 0; szPath = "";
            }

            public uint cbSize;
            public IntPtr hIcon;
            public int iSysImageIndex;
            public int iIcon;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szPath;
        }

        public static Bitmap GetFileIcon(string fileName, bool largeIcon)
        {
            SHFILEINFO info = new SHFILEINFO();
            info.ZeroMemory();
            int cbFileInfo = Marshal.SizeOf(info);
            SHGFI flags;

            if (largeIcon)
            {
                flags = SHGFI.Icon | SHGFI.LargeIcon | SHGFI.UseFileAttributes;
            }
            else
            {
                flags = SHGFI.Icon | SHGFI.SmallIcon | SHGFI.UseFileAttributes;
            }

            IntPtr result = SHGetFileInfo(fileName, 256, out info, (uint)cbFileInfo, flags);
            if (result.Equals(IntPtr.Zero))
            {
                return null;
            }

            Icon icon = Icon.FromHandle(info.hIcon);
            Bitmap bitmap = icon.ToBitmap();
            DestroyIcon(info.hIcon);

            return bitmap;
        }

        public static Bitmap GetDirectoryIcon(string dirName, bool largeIcon)
        {
            SHFILEINFO info = new SHFILEINFO();
            info.ZeroMemory();
            int cbFileInfo = Marshal.SizeOf(info);
            SHGFI flags;

            if (largeIcon)
            {
                flags = SHGFI.Icon | SHGFI.LargeIcon;
            }
            else
            {
                flags = SHGFI.Icon | SHGFI.SmallIcon;
            }

            IntPtr result = SHGetFileInfo(dirName, 0, out info, (uint)cbFileInfo, flags);
            if (result.Equals(IntPtr.Zero))
            {
                return null;
            }

            Icon icon = Icon.FromHandle(info.hIcon);
            Bitmap bitmap = icon.ToBitmap();
            DestroyIcon(info.hIcon);

            return bitmap;
        }

        public static Bitmap GetStockIcon(SHSTOCKICONID stockIconId, bool largeIcon)
        {
            SHSTOCKICONINFO info = new SHSTOCKICONINFO();
            info.ZeroMemory();
            info.cbSize = (uint)Marshal.SizeOf(info);
            SHGSI flags = SHGSI.Icon;

            if (largeIcon)
            {
                flags |= SHGSI.LargeIcon;
            }
            else
            {
                flags |= SHGSI.SmallIcon;
            }

            int result = SHGetStockIconInfo(stockIconId, flags, ref info);
            if (result != 0)
            {
                return null;
            }

            Icon icon = Icon.FromHandle(info.hIcon);
            Bitmap bitmap = icon.ToBitmap();
            DestroyIcon(info.hIcon);

            return bitmap;
        }
    }
}
