using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace BiliRoku
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    internal static class Ver
    {
        public const string VER = "1.4.3";
        public const string DATE = "(2016-11-7)";
        public const string DESC = "修复了主播未开播状态长时间挂机后开播可能下载失败的错误。\nby Kimi-Arthur\n\n修复了自动更新可能存在的问题。";
        public static readonly string OS_VER = "(" + WinVer.SystemVersion.Major + "." + WinVer.SystemVersion.Minor + "." + WinVer.SystemVersion.Build + ")";
        public static readonly string UA = "FeelyBlog/1.1 (zyzsdy@foxmail.com) BiliRoku/1.4.2 " + OS_VER + " AppleWebKit/537.36 (KHTML, like Gecko) Chrome/43.0.2357.124 Safari/537.36";
    }

    internal static class WinVer
    {
        public static readonly Version SystemVersion = GetSystemVersion();

        private static Delegate GetFunctionAddress(IntPtr dllModule, string functionName, Type t)
        {
            var address = WinApi.GetProcAddress(dllModule, functionName);
            return address == IntPtr.Zero ? null : Marshal.GetDelegateForFunctionPointer(address, t);
        }

        private delegate IntPtr RtlGetNtVersionNumbers(ref int dwMajor, ref int dwMinor, ref int dwBuildNumber);

        private static Version GetSystemVersion()
        {
            var hinst = WinApi.LoadLibrary("ntdll.dll");
            var func = (RtlGetNtVersionNumbers)GetFunctionAddress(hinst, "RtlGetNtVersionNumbers", typeof(RtlGetNtVersionNumbers));
            int dwMajor = 0, dwMinor = 0, dwBuildNumber = 0;
            func.Invoke(ref dwMajor, ref dwMinor, ref dwBuildNumber);
            dwBuildNumber &= 0xffff;
            return new Version(dwMajor, dwMinor, dwBuildNumber);
        }
    }

    internal static class WinApi
    {
        [DllImport("Kernel32")]
        public static extern IntPtr LoadLibrary(string funcname);

        [DllImport("Kernel32")]
        public static extern IntPtr GetProcAddress(IntPtr handle, string funcname);
    }
}
