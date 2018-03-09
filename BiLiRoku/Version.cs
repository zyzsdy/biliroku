using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace BiliRoku
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    internal static class Ver
    {
        public const string VER = "1.5.0";
        public const string DATE = "(2017-11-3)";
        public const string DESC = "B站新版对应升级。";
        public static readonly string OS_VER = "(" + WinVer.SystemVersion.Major + "." + WinVer.SystemVersion.Minor + "." + WinVer.SystemVersion.Build + ")";
        public static readonly string UA = "FeelyBlog/1.1 (zyzsdy@foxmail.com) BiliRoku/1.5.0 " + OS_VER + " AppleWebKit/537.36 (KHTML, like Gecko) Chrome/43.0.2357.124 Safari/537.36";
    }

    // 检查更新
    public delegate void InfoEvent(object sender, string info);
    public delegate void CheckResultEvent(object sender, UpdateResultArgs result);
    public class UpdateResultArgs
    {
        public string version;
        public string url;
    }
    class CheckUpdate
    {
        public event InfoEvent OnInfo;
        public event CheckResultEvent OnResult;
        public CheckUpdate()
        {
            Check();
        }

        private void Check()
        {
            Task.Run(() =>
            {
                OnInfo?.Invoke(this, "检查更新。");

                var ApiUrl = "https://api.github.com/repos/zyzsdy/biliroku/releases";
                var wc = new WebClient();
                wc.Headers.Add("Accept: application/json;q=0.9,*/*;q=0.5");
                wc.Headers.Add("User-Agent: " + Ver.UA);
                wc.Headers.Add("Accept-Language: zh-CN,zh;q=0.8,en;q=0.6,ja;q=0.4");
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

                //发送HTTP请求获取Release信息
                string releaseJson = null;

                try
                {
                    var releaseByte = wc.DownloadData(ApiUrl);
                    releaseJson = System.Text.Encoding.GetEncoding("UTF-8").GetString(releaseByte);
                }
                catch (Exception e)
                {
                    OnInfo?.Invoke(this, "检查更新失败：" + e.Message);
                }

                //提取最新版的release信息
                if(releaseJson != null)
                {
                    try
                    {
                        var releaseObj = JArray.Parse(releaseJson);
                        var releaseNote = releaseObj[0];
                        var tag = releaseNote["tag_name"].ToString();
                        var url = releaseNote["html_url"].ToString();
                        Version verCurrent, verNew;
                        verCurrent = Version.Parse(Ver.VER);
                        if(Version.TryParse(tag, out verNew))
                        {
                            if(verNew > verCurrent)
                            {
                                try
                                {
                                    OnResult?.Invoke(this, new UpdateResultArgs
                                    {
                                        version = tag,
                                        url = url
                                    });
                                }catch (Exception e)
                                {
                                    OnInfo?.Invoke(this, "发现新版本，但是出了点罕见错误：" + e.Message);
                                }
                                
                                OnInfo?.Invoke(this, "发现新版本" + tag + "，下载地址：" + url);
                            }else
                            {
                                OnInfo?.Invoke(this, "当前已是最新版本。");
                            }
                        }else
                        {
                            OnInfo?.Invoke(this, "版本信息无法解析。");
                        }
                    }
                    catch (Exception e)
                    {
                        OnInfo?.Invoke(this, "更新信息解析失败：" + e.Message);
                        OnInfo?.Invoke(this, releaseJson);
                    }
                }
            });
        }
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
