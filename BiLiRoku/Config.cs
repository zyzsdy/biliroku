using Microsoft.Win32;
using System;
using System.Diagnostics;

namespace BiliRoku
{
    internal class Config
    {
        private string _version;
        private string _roomId;
        private string _saveLocation;
        private string _savePath;
        private string _filename;
        private string _isDownloadCmt = "True";
        private string _isWaitStreaming = "True";

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
        public string SaveLocation
        {
            get
            {
                return _saveLocation;
            }
            set
            {
                Write("save_location", value);
                _saveLocation = value;
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

        public Config()
        {
            Init();
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
                    case "save_location":
                        _saveLocation = bilirokuKey.GetValue("save_location").ToString();
                        break;
                    case "is_download_cmt":
                        _isDownloadCmt = bilirokuKey.GetValue("is_download_cmt").ToString();
                        break;
                    case "is_wait_streaming":
                        _isWaitStreaming = bilirokuKey.GetValue("is_wait_streaming").ToString();
                        break;
                    case "filename":
                        _filename = bilirokuKey.GetValue("filename").ToString();
                        break;
                    case "save_path":
                        _savePath = bilirokuKey.GetValue("save_path").ToString();
                        break;
                    default:
                        throw new Exception("Not support this keyname");
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
