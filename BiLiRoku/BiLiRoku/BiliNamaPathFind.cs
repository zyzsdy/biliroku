using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace BiLiRoku
{
    class BiliNamaPathFind
    {
        const string appkey = "2379cb56649e081f";
        const string selectkey = "a6aa1ae3fe6f5caad56c1f79a615d9fb";
        string useragent = Version.UA;

        string roomid;
        RichTextBox infoBox = null;
        public string trueURL { get; set; }

        public bool Init(string roomid, RichTextBox infoBox = null)
        {
            this.infoBox = infoBox;
            string trueRoomid = getTrueRoomid(roomid);
            this.roomid = trueRoomid;
            string apiPath = CalcApiPath();
            return GetTrueURL(apiPath);
        }

        private string getTrueRoomid(string roomid)
        {
            if(infoBox != null)
            {
                infoBox.AppendText("[INFO " + DateTime.Now.ToString("HH:mm:ss") + "] 尝试获取真实房间号。\n");
            }
            string roomWebPageUrl = "http://live.bilibili.com/" + roomid;
            WebClient wc = new WebClient();
            wc.Headers.Add("Accept: text/html");
            wc.Headers.Add("User-Agent: " + useragent);
            wc.Headers.Add("Accept-Language: zh-CN,zh;q=0.8,en;q=0.6,ja;q=0.4");

            //发送HTTP请求获取真实房间号
            string roomHtml = null;

            try
            {
                roomHtml = wc.DownloadString(roomWebPageUrl);
            }
            catch (Exception e)
            {
                if (infoBox != null)
                {
                    infoBox.AppendText("[ERROR " + DateTime.Now.ToString("HH:mm:ss") + "] 打开直播页面失败：" + e.Message + "\n");
                }
                return "0000";
            }

            //从HTML中提取真实房间号

            string pattern = @"(?<=var ROOMID = )(\d+)(?=;)";
            MatchCollection colls = Regex.Matches(roomHtml, pattern);
            foreach (Match mat in colls)
            {
                if (infoBox != null)
                {
                    infoBox.AppendText("[INFO " + DateTime.Now.ToString("HH:mm:ss") + "] 真实房间号：" + mat.Value + "\n");
                }
                return mat.Value;
            }

            if (infoBox != null)
            {
                infoBox.AppendText("[ERROR " + DateTime.Now.ToString("HH:mm:ss") + "] 获取真实房间号失败\n");
            }
            return "0000";
        }

        private bool GetTrueURL(string apiPath)
        {
            string xmlResult = null;

            //访问API获取xml
            WebClient wc = new WebClient();
            wc.Headers.Add("Accept: */*");
            wc.Headers.Add("User-Agent: " + useragent);
            wc.Headers.Add("Accept-Language: zh-CN,zh;q=0.8,en;q=0.6,ja;q=0.4");

            try {
                xmlResult = wc.DownloadString(apiPath);
            }
            catch (Exception e)
            {
                if (infoBox != null)
                {
                    infoBox.AppendText("[ERROR " + DateTime.Now.ToString("HH:mm:ss") + "] 发送解析请求失败：" + e.Message + "\n");
                }
                return false;
            }

            //解析xml
            XmlDocument playUrlXml = new XmlDocument();
            try
            {
                playUrlXml.LoadXml(xmlResult);
            }
            catch(Exception e)
            {
                if (infoBox != null)
                {
                    infoBox.AppendText("[ERROR " + DateTime.Now.ToString("HH:mm:ss") + "] 解析XML失败：" + e.Message + "\n");
                }
                return false;
            }

            //获得解析结果
            XmlNode result = playUrlXml.DocumentElement.SelectSingleNode("/video/result");
            if(result.InnerText != "suee")
            {
                if (infoBox != null)
                {
                    infoBox.AppendText("[ERROR " + DateTime.Now.ToString("HH:mm:ss") + "] 解析地址失败。\n");
                }
                return false;
            }
            else
            {
                XmlNode turlNode = playUrlXml.DocumentElement.SelectSingleNode("/video/durl/url");
                trueURL = turlNode.InnerText;

                if (infoBox != null)
                {
                    infoBox.AppendText("[INFO " + DateTime.Now.ToString("HH:mm:ss") + "] 解析地址成功：" + trueURL + "\n");
                }
                return true;
            }
        }

        private string CalcApiPath()
        {
            //生成参数串
            StringBuilder apiParams = new StringBuilder();
            apiParams.Append("appkey=").Append(appkey).Append("&")
                .Append("cid=").Append(roomid).Append("&")
                .Append("player=1&quality=0&ts=");
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);//UNIX时间戳
            apiParams.Append(Convert.ToInt64(ts.TotalSeconds).ToString());

            string apiParam = apiParams.ToString();//原始参数串

            //计算签名
            string waitForSign = apiParam + selectkey;
            byte[] waitForSignBytes = Encoding.UTF8.GetBytes(waitForSign);
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] signBytes = md5.ComputeHash(waitForSignBytes);

            string sign = "";
            for(int i = 0; i < signBytes.Length; i++)
            {
                sign += signBytes[i].ToString("x");
            }

            //最终API
            return "http://live.bilibili.com/api/playurl?" + apiParam + "&sign=" + sign;
        }

    }
}
;