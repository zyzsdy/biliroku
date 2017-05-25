﻿using System;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace BiliRoku.Bililivelib
{
    internal class PathFinder
    {
        private readonly MainWindow _mw;
        public PathFinder(MainWindow mw)
        {
            _mw = mw;
        }

        private void AddInfo(string level, string info)
        {
            _mw.AppendLogln(level, info);
        }

        public Task<string> GetRoomid(string originalRoomId)
        {
            return Task.Run(() => {
                AddInfo("INFO", "尝试获取真实房间号");

                var roomWebPageUrl = "http://live.bilibili.com/" + originalRoomId;
                var wc = new WebClient();
                wc.Headers.Add("Accept: text/html");
                wc.Headers.Add("User-Agent: " + Ver.UA);
                wc.Headers.Add("Accept-Language: zh-CN,zh;q=0.8,en;q=0.6,ja;q=0.4");

                //发送HTTP请求获取真实房间号
                string roomHtml;

                try
                {
                    roomHtml = wc.DownloadString(roomWebPageUrl);
                }
                catch (Exception e)
                {
                    AddInfo("ERROR", "打开直播页面失败：" + e.Message);
                    return null;
                }

                //从HTML中提取真实房间号
                const string pattern = @"(?<=var ROOMID = )(\d+)(?=;)";
                var colls = Regex.Matches(roomHtml, pattern);
                foreach (Match mat in colls)
                {
                    AddInfo("INFO", "真实房间号: " + mat.Value);
                    return mat.Value;
                }

                AddInfo("ERROR", "获取真实房间号失败");
                return null;
            });
        }

        internal Task<string> GetTrueUrl(string roomid)
        {
            return Task.Run(()=> {
                if (roomid == null)
                {
                    AddInfo("ERROR", "房间号获取错误。");
                    throw new Exception("No roomid");
                }
                var apiUrl = GetApiUrl(roomid);
                SendStat(roomid);

                //准备获取xml
                string xmlResult;

                //访问API获取xml
                var wc = new WebClient();
                wc.Headers.Add("Accept: */*");
                wc.Headers.Add("User-Agent: " + Ver.UA);
                wc.Headers.Add("Accept-Language: zh-CN,zh;q=0.8,en;q=0.6,ja;q=0.4");

                try
                {
                    xmlResult = wc.DownloadString(apiUrl);
                }
                catch (Exception e)
                {
                    AddInfo("ERROR", "发送解析请求失败：" + e.Message);
                    throw;
                }

                //解析xml
                var playUrlXml = new XmlDocument();
                try
                {
                    playUrlXml.LoadXml(xmlResult);
                }
                catch (Exception e)
                {
                    AddInfo("ERROR", "解析XML失败：" + e.Message);
                    throw;
                }

                //获得解析结果
                var result = playUrlXml.DocumentElement?.SelectSingleNode("/video/result");
                if (result != null && result.InnerText != "suee")
                {
                    AddInfo("ERROR", "解析地址失败。");
                    throw new Exception("No Avaliable download url in xml infomation.");
                }
                var turlNode = playUrlXml.DocumentElement?.SelectSingleNode("/video/durl/url");
                if (turlNode == null) throw new NullReferenceException();
                var trueUrl = turlNode.InnerText;

                AddInfo("INFO", "地址解析成功：" + trueUrl);
                return trueUrl;
            });
        }

        private static void SendStat(string roomid)
        {
            var api = $"https://zyzsdy.com/biliroku/stat?os={Ver.OS_VER}&id={roomid}&ver={Ver.VER}";

            var wc = new WebClient();
            wc.Headers.Add("Accept: */*");
            wc.Headers.Add("User-Agent: " + Ver.UA);
            wc.Headers.Add("Accept-Language: zh-CN,zh;q=0.8,en;q=0.6,ja;q=0.4");
            try
            {
                wc.DownloadStringAsync(new Uri(api));
            }
            catch
            {
                //ignore
            }
        }

        private static string GetApiUrl(string roomid)
        {
            //生成参数串
            var apiParams = new StringBuilder();
            apiParams.Append("appkey=").Append(BiliApiKeyInfo.AppKey).Append("&")
                .Append("cid=").Append(roomid).Append("&")
                .Append("player=1&quality=0&ts=");
            var ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);//UNIX时间戳
            apiParams.Append(Convert.ToInt64(ts.TotalSeconds).ToString());

            var apiParam = apiParams.ToString();//原始参数串

            //计算签名
            var waitForSign = apiParam + BiliApiKeyInfo.SecretKey;
            var waitForSignBytes = Encoding.UTF8.GetBytes(waitForSign);
            MD5 md5 = new MD5CryptoServiceProvider();
            var signBytes = md5.ComputeHash(waitForSignBytes);

            var sign = signBytes.Aggregate("", (current, t) => current + t.ToString("x"));

            //最终API
            return "http://live.bilibili.com/api/playurl?" + apiParam + "&sign=" + sign;
        }
    }
}
