using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiliRoku
{
    public delegate void InfoEventHandler(InfoArgs info);
    public class InfoArgs
    {
        public string source;
        public string level;
        public string info;
    }

    static class InfoLogger
    {
        static public event InfoEventHandler OnInfo;
        static public void SendInfo(string source, string level, string info)
        {
            OnInfo?.Invoke(new InfoArgs
            {
                source = source,
                level = level,
                info = info
            });
        }
    }
}
