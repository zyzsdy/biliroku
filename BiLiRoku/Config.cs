using Microsoft.Win32;
using System;
using System.Diagnostics;

namespace BiliRoku
{
    public class Config
    {
        static Config()
        {

        }

        private Config()
        {
            Init();
        }

        public static Config Instance { get; } = new Config();

        private string _version;
        private string _roomId;
        private string _savePath;
        private string _filename;
        private string _refreshTime;
        private string _timeout;
        private string _isDownloadCmt = "True";
        private string _isWaitStreaming = "True";
        private string _isAutoRetry = "True";

        public string Version
        {
            get
            {
                return _version;
            }
            set
            {
                Write("version", value);
                _version = value;
            }
        }
        public string RoomId
        {
            get
            {
                return _roomId;
            }
            set
            {
                Write("room_id", value);
                _roomId = value;
            }
        }
        public bool IsDownloadComment
        {
            get
            {
                return _isDownloadCmt == "True";
            }
            set
            {
                Write("is_download_cmt", value ? "True" : "False");
                _isDownloadCmt = value ? "True" : "False";
            }
        }
        public bool IsWaitStreaming
        {
            get
            {
                return _isWaitStreaming == "True";
            }
            set
            {
                Write("is_wait_streaming", value ? "True" : "False");
                _isWaitStreaming = value ? "True" : "False";
            }
        }
        public bool IsAutoRetry
        {
            get
            {
                return _isAutoRetry == "True";
            }
            set
            {
                Write("is_auto_retry", value ? "True" : "False");
                _isAutoRetry = value ? "True" : "False";
            }
        }
        public string Filename
        {
            get
            {
                return _filename;
            }
            set
            {
                Write("filename", value);
                _filename = value;
            }
        }
        public string SavePath
        {
            get
            {
                return _savePath;
            }
            set
            {
                Write("save_path", value);
                _savePath = value;
            }
        }
        public string RefreshTime
        {
            get
            {
                return _refreshTime;
            }
            set
            {
                Write("refresh_time", value);
                _refreshTime = value;
            }
        }
        public string Timeout
        {
            get
            {
                return _timeout;
            }
            set
            {
                Write("timeout", value);
                _timeout = value;
            }
        }

        private void Init()
        {
            var hkcu = Registry.CurrentUser;
            Debug.Assert(hkcu != null, "hkcu != null");
            hkcu.CreateSubKey("SOFTWARE\\BiliRoku");
            var bilirokuKey = hkcu.OpenSubKey("SOFTWARE\\BiliRoku");
            Debug.Assert(bilirokuKey != null, "bilirokuKey != null");
            var subkeyNames = bilirokuKey.GetValueNames();

            foreach (var keyName in subkeyNames)
            {
                switch (keyName)
                {
                    case "version":
                        _version = bilirokuKey.GetValue("version").ToString();
                        break;
                    case "room_id":
                        _roomId = bilirokuKey.GetValue("room_id").ToString();
                        break;
                    case "is_download_cmt":
                        _isDownloadCmt = bilirokuKey.GetValue("is_download_cmt").ToString();
                        break;
                    case "is_wait_streaming":
                        _isWaitStreaming = bilirokuKey.GetValue("is_wait_streaming").ToString();
                        break;
                    case "is_auto_retry":
                        _isAutoRetry = bilirokuKey.GetValue("is_auto_retry").ToString();
                        break;
                    case "filename":
                        _filename = bilirokuKey.GetValue("filename").ToString();
                        break;
                    case "save_path":
                        _savePath = bilirokuKey.GetValue("save_path").ToString();
                        break;
                    case "refresh_time":
                        _refreshTime = bilirokuKey.GetValue("refresh_time").ToString();
                        break;
                    case "timeout":
                        _timeout = bilirokuKey.GetValue("timeout").ToString();
                        break;
                    default:
                        InfoLogger.SendInfo("Config", "WARNING", "不支持的配置项。");
                        break;
                }
            }
            hkcu.Close();
        }

        private static void Write(string key, string value)
        {
            var hkcu = Registry.CurrentUser;
            var bilirokuKey = hkcu.OpenSubKey("SOFTWARE\\BiliRoku", true);
            bilirokuKey?.SetValue(key, value);
            hkcu.Close();
        }
    }
}
