using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BiLiRoku
{
    class Version
    {
        public const string VER = "1.2.3";
        public const string DATE = "(2015-11-14)";
        public const string DESC = "1. 支持B站短房间号\n2. 填入的房间号和保存位置在下次启动时仍然保留。";
        public static string UA = "FeelyBlog/1.1 (zyzsdy@foxmail.com) BiliRoku/1.2.3 (" + Environment.OSVersion.ToString() + ") AppleWebKit/537.36 (KHTML, like Gecko) Chrome/43.0.2357.124 Safari/537.36";
    }
}
