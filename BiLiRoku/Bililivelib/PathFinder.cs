using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

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

                var roomWebPageUrl = "https://api.live.bilibili.com/room/v1/Room/room_init?id=" + originalRoomId;
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
                    AddInfo("ERROR", "直播初始化失败：" + e.Message);
                    return null;
                }

                //从返回结果中提取真实房间号
                

                try
                {
                    var result = JObject.Parse(roomHtml);
                    var roomid = result["data"]["room_id"].ToString();
                    AddInfo("INFO", "真实房间号: " + roomid);
                    return roomid;
                }
                catch (Exception e)
                {
                    AddInfo("ERROR", "获取真实房间号失败：" + e.Message);
                    return null;
                }

                
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
                var apiUrl = "https://api.live.bilibili.com/api/playurl?cid=" + roomid + "&otype=json&quality=0&platform=web";
                SendStat(roomid);

                //访问API获取结果
                var wc = new WebClient();
                wc.Headers.Add("Accept: */*");
                wc.Headers.Add("User-Agent: " + Ver.UA);
                wc.Headers.Add("Accept-Language: zh-CN,zh;q=0.8,en;q=0.6,ja;q=0.4");

                string resultString;

                try
                {
                    resultString = wc.DownloadString(apiUrl);
                }
                catch (Exception e)
                {
                    AddInfo("ERROR", "发送解析请求失败：" + e.Message);
                    throw;
                }

                //解析结果
                try
                {
                    var jsonResult = JObject.Parse(resultString);
                    var trueUrl = jsonResult["durl"][0]["url"].ToString();
                    AddInfo("INFO", "地址解析成功：" + trueUrl);
                    return trueUrl;
                }
                catch (Exception e)
                {
                    AddInfo("ERROR", "解析XML失败：" + e.Message);
                    throw;
                }
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

        //@deprecated
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
