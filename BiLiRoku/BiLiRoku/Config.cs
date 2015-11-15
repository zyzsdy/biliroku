using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BiLiRoku
{
    class Config
    {
        private string version = null;
        private string room_id = null;
        private string save_location = null;

        public string Version
        {
            get
            {
                return version;
            }
            set
            {
                this.write("version", value);
                this.version = value;
            }
        }
        public string RoomId
        {
            get
            {
                return this.room_id;
            }
            set
            {
                this.write("room_id", value);
                this.room_id = value;
            }
        }
        public string SaveLocation
        {
            get
            {
                return this.save_location;
            }
            set
            {
                this.write("save_location", value);
                this.save_location = value;
            }
        }

        public Config()
        {
            init();
        }
        private void init()
        {
            RegistryKey hkcu = Registry.CurrentUser;
            RegistryKey software = hkcu.CreateSubKey("SOFTWARE\\BiliRoku");
            RegistryKey bilirokuKey = hkcu.OpenSubKey("SOFTWARE\\BiliRoku");
            string[] subkeyNames = bilirokuKey.GetValueNames();

            foreach(string keyName in subkeyNames)
            {
                if(keyName == "version")
                {
                    this.version = bilirokuKey.GetValue("version").ToString();
                }
                else if(keyName == "room_id")
                {
                    this.room_id = bilirokuKey.GetValue("room_id").ToString();
                }
                else if(keyName == "save_location")
                {
                    this.save_location = bilirokuKey.GetValue("save_location").ToString();
                }
            }
            hkcu.Close();
        }

        private void write(string key, string value)
        {
            RegistryKey hkcu = Registry.CurrentUser;
            RegistryKey bilirokuKey = hkcu.OpenSubKey("SOFTWARE\\BiliRoku", true);
            bilirokuKey.SetValue(key, value);
            hkcu.Close();
        }
    }
}
