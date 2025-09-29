using System;
using System.Runtime.InteropServices;

namespace PxViewer.Utils
{
    public static class ShellInterop
    {
        internal static class NativeMethods
        {
            [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
            public static extern IntPtr SHGetFileInfo(
                string pszPath,
                uint dwFileAttributes,
                out ShFileInfo shellFileInfoPointer,
                uint cbFileInfo,
                uint uFlags);

            [DllImport("user32.dll", SetLastError = true)]
            public static extern bool DestroyIcon(IntPtr hIcon);

            [StructLayout(LayoutKind.Sequential)]
            public struct ShFileInfo
            {
                public IntPtr hIcon;
                public IntPtr iIcon;
                public uint dwAttributes;

                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
                public string szDisplayName;

                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
                public string szTypeName;
            }
        }
    }
}