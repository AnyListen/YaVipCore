using System;
using System.Runtime.InteropServices;

namespace FishMusic.Download
{
    public static class Xl
    {
        [DllImport("xldl.dll", EntryPoint = "XL_QueryTaskInfoEx", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr XL_CreateBTTask(DownBtTaskParam stParam);
        [DllImport("xldl.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr XL_CreateTask([In] DownTaskParam stParam);
        [DllImport("xldl.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool XL_DeleteTask(IntPtr hTask);
        [DllImport("xldl.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool XL_DelTempFile(DownTaskParam stParam);
        [DllImport("xldl.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool XL_GetBtDataFileList(string szFilePath, string szSeedFileFullPath);
        [DllImport("xldl.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool XL_GetFileSizeWithUrl(string lpUrl, long iFileSize);
        [DllImport("xldl.dll", CharSet = CharSet.Unicode)]
        public static extern bool XL_Init();
        [DllImport("xldl.dll", EntryPoint = "XL_QueryTaskInfoEx", CallingConvention = CallingConvention.Cdecl)]
        public static extern long XL_QueryBTFileInfo(IntPtr hTask, BtTaskInfo pTaskInfo);
        [DllImport("xldl.dll", EntryPoint = "XL_QueryTaskInfoEx", CallingConvention = CallingConvention.Cdecl)]
        public static extern long XL_QueryBTFileInfo(IntPtr hTask, UIntPtr dwFileIndex, ulong ullFileSize, ulong ullCompleteSize, UIntPtr dwStatus);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("xldl.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool XL_QueryTaskInfoEx(IntPtr hTask, [Out] DownTaskInfo stTaskInfo);
        [DllImport("xldl.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool XL_SetProxy(DownProxyInfo stProxyInfo);
        [DllImport("xldl.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void XL_SetSpeedLimit(int nKBps);
        [DllImport("xldl.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void XL_SetUploadSpeedLimit(int nTcpKBps, int nOtherKBps);
        [DllImport("xldl.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool XL_SetUserAgent(string pszUserAgent);
        [DllImport("xldl.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool XL_StartTask(IntPtr hTask);
        [DllImport("xldl.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool XL_StopTask(IntPtr hTask);
        [DllImport("xldl.dll", CharSet = CharSet.Unicode)]
        public static extern bool XL_UnInit();

        [StructLayout(LayoutKind.Sequential)]
        public class BtDataFileItem
        {
            public uint path_len;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x100)]
            public string file_path;
            public uint name_len;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x400)]
            public string file_name;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class BtDataFileList
        {
            public uint item_count;
            public IntPtr item_array;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class BtFileInfo
        {
            public ulong file_size;
            public uint path_len;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x100)]
            public string file_path;
            public uint name_len;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x400)]
            public string file_name;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class BtSeedFileInfo
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
            public string info_id;
            public uint title_len;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x400)]
            public string title;
            public uint file_info_count;
            public IntPtr file_info_array;
            public uint tracker_count;
            public IntPtr tracker_info_array;
            public uint publisher_len;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x2000)]
            public string publisher;
            public uint publisher_url_len;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x400)]
            public string publisher_url;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct BtTaskInfo
        {
            public int lTaskStatus;
            public uint dwUsingResCount;
            public uint dwSumResCount;
            public ulong ullRecvBytes;
            public ulong ullSendBytes;
            [MarshalAs(UnmanagedType.Bool)]
            public bool bFileCreated;
            public uint dwSeedCount;
            public uint dwConnectedBTPeerCount;
            public uint dwAllBTPeerCount;
            public uint dwHealthyGrade;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public class DownBtTaskParam
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szSeedFullPath;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szFilePath;
            public uint dwNeedDownloadFileCount;
            public IntPtr dwNeedDownloadFileIndexArray;
            public uint dwTrackerInfoCount;
            public IntPtr pTrackerInfoArray;
            public int IsResume;
        }

        public enum DownProxyAuthType
        {
            ProxyAuthNone,
            ProxyAuthAuto,
            ProxyAuthBase64,
            ProxyAuthNtlm,
            ProxyAuthDegest,
            ProxyAuthUnkown
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public class DownProxyInfo
        {
            [MarshalAs(UnmanagedType.Bool)]
            public bool bIEProxy;
            [MarshalAs(UnmanagedType.Bool)]
            public bool bProxy;
            public DownProxyType stPType;
            public DownProxyAuthType stAType;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x800)]
            public string szHost;
            public int nPort;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
            public string szUser;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
            public string szPwd;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x800)]
            public string szDomain;
        }

        public enum DownProxyType
        {
            ProxyTypeFtp = 4,
            ProxyTypeHttp = 1,
            ProxyTypeIe = 0,
            ProxyTypeSock4 = 2,
            ProxyTypeSock5 = 3,
            ProxyTypeUnkown = 0xff
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public class DownTaskInfo
        {
            public DownTaskStatus stat;
            public TaskErrorType fail_code;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szFilename;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szReserved0;
            public long nTotalSize;
            public long nTotalDownload;
            public float fPercent;
            public int nReserved0;
            public int nSrcTotal;
            public int nSrcUsing;
            public int nReserved1;
            public int nReserved2;
            public int nReserved3;
            public int nReserved4;
            public long nReserved5;
            public long nDonationP2P;
            public long nReserved6;
            public long nDonationOrgin;
            public long nDonationP2S;
            public long nReserved7;
            public long nReserved8;
            public int nSpeed;
            public int nSpeedP2S;
            public int nSpeedP2P;
            public bool bIsOriginUsable;
            public float fHashPercent;
            public int IsCreatingFile;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x40, ArraySubType = UnmanagedType.U4)]
            public uint[] reserved;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 1)]
        public class DownTaskParam
        {
            public int nReserved;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x824)]
            public string szTaskUrl;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x824)]
            public string szRefUrl;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x1000)]
            public string szCookies;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szFilename;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szReserved0;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szSavePath;
            public IntPtr hReserved;
            public int bReserved = 0;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x40)]
            public string szReserved1;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x40)]
            public string szReserved2;
            public int IsOnlyOriginal = 0;
            public uint nReserved1 = 5;
            public int DisableAutoRename = 0;
            public int IsResume = 1;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x800, ArraySubType = UnmanagedType.U4)]
            public uint[] reserved;
        }

        public enum DownTaskStatus
        {
            Noitem,
            TscError,
            TscPause,
            TscDownload,
            TscComplete,
            TscStartpending,
            TscStoppending
        }

        public enum TaskErrorType
        {
            TaskErrorCancel = 0x41,
            TaskErrorDiskCreate = 1,
            TaskErrorDiskDelete = 7,
            TaskErrorDiskFilehash = 6,
            TaskErrorDiskPiecehash = 5,
            TaskErrorDiskRead = 3,
            TaskErrorDiskRename = 4,
            TaskErrorDiskWrite = 2,
            TaskErrorDownInvalid = 0x10,
            TaskErrorHttpmgrNotIp = 0x30,
            TaskErrorIdInvalid = 0x43,
            TaskErrorProxyAuthTypeFailed = 0x21,
            TaskErrorProxyAuthTypeUnkown = 0x20,
            TaskErrorTimeout = 0x40,
            TaskErrorTpCrashed = 0x42,
            TaskErrorUnknown = 0
        }

        [StructLayout(LayoutKind.Sequential)]
        public class tracker_info
        {
            public uint tracker_url_len;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x400)]
            public string tracker_url;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public class TrackerInfo
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x400)]
            public string szTrackerUrl;
        }
    }
}