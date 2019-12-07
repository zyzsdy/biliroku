using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BiliRoku.Bililivelib
{
    public class RoomInfo
    {
        public string realRoomid;
        public string title;
        public bool liveStatus;
        public string username;
    }

    static public class PathFinder
    {
        static public Task<RoomInfo> GetRoomInfo(string originalRoomId)
        {
            return Task.Run(() => {
                //InfoLogger.SendInfo(originalRoomId, "DEBUG", "正在刷新信息");

                var roomWebPageUrl = "https://api.live.bilibili.com/room/v1/Room/get_info?id=" + originalRoomId;
                var wc = new WebClient();
                wc.Headers.Add("Accept: */*");
                wc.Headers.Add("User-Agent: " + Ver.UA);
                wc.Headers.Add("Accept-Language: zh-CN,zh;q=0.8,en;q=0.6,ja;q=0.4");

                //发送HTTP请求
                byte[] roomHtml;

                try
                {
                    roomHtml = wc.DownloadData(roomWebPageUrl);
                }
                catch (Exception e)
                {
                    InfoLogger.SendInfo(originalRoomId, "ERROR", "获取房间信息失败：" + e.Message);
                    return null;
                }

                //解析返回结果
                try
                {
                    var roomJson = Encoding.UTF8.GetString(roomHtml);
                    var result = JObject.Parse(roomJson);
                    var uid = result["data"]["uid"].ToString();

                    var userInfoUrl = "https://api.bilibili.com/x/web-interface/card?mid=" + uid;
                    var uwc = new WebClient();
                    uwc.Headers.Add("Accept: */*");
                    uwc.Headers.Add("User-Agent: " + Ver.UA);
                    uwc.Headers.Add("Accept-Language: zh-CN,zh;q=0.8,en;q=0.6,ja;q=0.4");

                    byte[] userHtml;
                    try
                    {
                        userHtml = uwc.DownloadData(userInfoUrl);
                    }
                    catch (Exception e)
                    {
                        InfoLogger.SendInfo(originalRoomId, "ERROR", "获取用户信息失败：" + e.Message);
                        return null;
                    }

                    var userJson = Encoding.UTF8.GetString(userHtml);
                    var userResult = JObject.Parse(userJson);
                    var userName = userResult["data"]["card"]["name"].ToString();

                    var roominfo = new RoomInfo
                    {
                        realRoomid = result["data"]["room_id"].ToString(),
                        title = result["data"]["title"].ToString(),
                        liveStatus = result["data"]["live_status"].ToString() == "1" ? true : false,
                        username = userName
                    };
                    return roominfo;
                }
                catch (Exception e)
                {
                    InfoLogger.SendInfo(originalRoomId, "ERROR", "房间信息解析失败：" + e.Message);
                    return null;
                }

                
            });
        }

        static public Task<string> GetTrueUrl(string roomid)
        {
            return Task.Run(()=> {
                if (roomid == null)
                {
                    InfoLogger.SendInfo(roomid, "ERROR", "房间号获取错误。");
                    throw new Exception("No roomid");
                }
                var apiUrl = "https://api.live.bilibili.com/room/v1/Room/playUrl?cid=" + roomid + "&otype=json&qn=10000&platform=web";
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
                    InfoLogger.SendInfo(roomid, "ERROR", "发送解析请求失败：" + e.Message);
                    throw;
                }

                //解析结果
                try
                {
                    var jsonResult = JObject.Parse(resultString);
                    var trueUrl = jsonResult["data"]["durl"][0]["url"].ToString();
                    InfoLogger.SendInfo(roomid, "INFO", "地址解析成功：" + trueUrl);
                    return trueUrl;
                }
                catch (Exception e)
                {
                    InfoLogger.SendInfo(roomid, "ERROR", "视频流地址解析失败：" + e.Message);
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
                .Append("player=1&qn=10000&ts=");
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
