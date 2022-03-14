using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics.CodeAnalysis;

namespace BiliRoku.Commentlib
{
    public enum MsgTypeEnum
    {
        /// <summary>
        /// 弹幕内容
        /// </summary>
        Comment,

        /// <summary>
        /// 礼物
        /// </summary>
        GiftSend,

        /// <summary>
        /// 礼物排名
        /// </summary>
        GiftTop,

        /// <summary>
        /// 欢迎
        /// </summary>
        Welcome,

        /// <summary>
        /// 直播开始
        /// </summary>
        LiveStart,

        /// <summary>
        /// 直播停止
        /// </summary>
        LiveEnd,

        /// <summary>
        /// 超级留言内容
        /// </summary>
        SuperChatMessage,

        /// <summary>
        /// 未知
        /// </summary>
        Unknown
    }

    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Local")]
    public class CommentModel
    {
        /// <summary>
        /// 弹幕内容
        /// </summary>
        public string CommentText { get; private set; }

        /// <summary>
        /// 弹幕的发送者
        /// </summary>
        public string CommentUser { get; private set; }

        /// <summary>
        /// 消息类型
        /// </summary>
        public MsgTypeEnum MsgType { get; private set; }

        /// <summary>
        /// 礼物用户
        /// </summary>
        public string GiftUser { get; private set; }

        /// <summary>
        /// 礼物名称
        /// </summary>
        public string GiftName { get; private set; }

        /// <summary>
        /// 礼物数量
        /// </summary>
        public string GiftNum { get; private set; }

        /// <summary>
        /// 不明字段
        /// </summary>
        public string Giftrcost { get; set; }

        /// <summary>
        /// 用户是否为房管
        /// </summary>
        public bool IsAdmin { get; set; }

        /// <summary>
        /// 用户是否为老爷
        /// </summary>
        public bool IsVip { get; set; }

        /// <summary>
        /// LiveStart,LiveEnd 事件对应的房间号
        /// </summary>
        public string RoomId { get; set; }
        /// <summary>
        /// 原始数据
        /// </summary>
        public string RawData { get; set; }
        /// <summary>
        /// JSON数据版本号
        /// </summary>
        public int JsonVersion { get; set; }

        public readonly long Time; //收到弹幕的时间

        //用于纯文本弹幕的扩展数据
        public readonly int DmType = 1;
        public readonly int Fontsize;
        public readonly int Color;
        public readonly long SendTimestamp;
        public readonly string UserHash = "";

        public CommentModel(string json, long time, int version = 1)
        {
            Time = time;
            RawData = json;
            JsonVersion = version;
            switch (version)
            {
                case 1:
                    {
                        var obj = JArray.Parse(json);

                        CommentText = obj[1].ToString();
                        CommentUser = obj[2][1].ToString();
                        MsgType = MsgTypeEnum.Comment;
                        break;
                    }
                case 2:
                    {
                        var obj = JObject.Parse(json);

                        var cmd = obj["cmd"].ToString();
                        if (cmd.StartsWith("LIVE"))
                        {
                            MsgType = MsgTypeEnum.LiveStart;
                            RoomId = obj["roomid"].ToString();
                        }else if (cmd.StartsWith("PREPARING"))
                        {
                            MsgType = MsgTypeEnum.LiveEnd;
                            RoomId = obj["roomid"].ToString();
                        }else if (cmd.StartsWith("DANMU_MSG"))
                        {
                            CommentText = obj["info"][1].ToString();
                            CommentUser = obj["info"][2][1].ToString();
                            IsAdmin = obj["info"][2][2].ToString() == "1";
                            IsVip = obj["info"][2][3].ToString() == "1";
                            DmType = Convert.ToInt32(obj["info"][0][1]);
                            Fontsize = Convert.ToInt32(obj["info"][0][2]);
                            Color = Convert.ToInt32(obj["info"][0][3]);
                            SendTimestamp = Convert.ToInt64(obj["info"][0][4]);
                            UserHash = obj["info"][0][7].ToString();
                            MsgType = MsgTypeEnum.Comment;
                        }else if (cmd.StartsWith("SEND_GIFT"))
                        {
                            MsgType = MsgTypeEnum.GiftSend;
                            GiftName = obj["data"]["giftName"].ToString();
                            GiftUser = obj["data"]["uname"].ToString();
                            Giftrcost = obj["data"]["rcost"].ToString();
                            GiftNum = obj["data"]["num"].ToString();
                        }else if (cmd.StartsWith("GIFT_TOP"))
                        {
                            MsgType = MsgTypeEnum.GiftTop;
                        }else if (cmd.StartsWith("WELCOME"))
                        {
                            MsgType = MsgTypeEnum.Welcome;
                            CommentUser = obj["data"]["uname"].ToString();
                            IsVip = true;
                            IsAdmin = obj["data"]["isadmin"].ToString() == "1";
                        }else if (cmd == "SUPER_CHAT_MESSAGE")
                        {
                            MsgType = MsgTypeEnum.SuperChatMessage;
                            var price = obj["data"]["price"].ToString();
                            CommentText = $"SC ￥{price} " + obj["data"]["message"].ToString();
                            CommentUser = obj["data"]["user_info"]["uname"].ToString();
                            Fontsize = 40;
                            SendTimestamp = Convert.ToInt64(obj["data"]["ts"]);
                            InfoLogger.SendInfo("debug", "superchat", CommentText);
                        }
                        else
                        {
                            MsgType = MsgTypeEnum.Unknown;
                        }
                        break;
                    }
                default:
                    throw new Exception("无法解析的弹幕数据");
            }
        }

        public string ToString(long startTime)
        {
            return
                $"<d p=\"{Convert.ToDouble(Time - startTime) / 1000},{DmType},{Fontsize},{Color},{SendTimestamp},0,{UserHash},0\">{System.Security.SecurityElement.Escape(CommentText)}</d>";
        }
    }
}
