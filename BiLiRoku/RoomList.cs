using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using BiliRoku.Bililivelib;
using BiliRoku.Commentlib;

namespace BiliRoku
{
    public delegate void DestoryRoomTaskHandler(object sender);

    public class RoomTask : INotifyPropertyChanged
    {
        private bool live_status = false;
        private bool record_status = false;
        private bool refreshing = false;
        private bool force_stoping = false;
        private bool init_ready = false;
        private string realRoomid;
        private Downloader downloader;
        private CommentProvider commentProvider;

        public string Roomid { get; }
        public string Title { get; set; }
        public string Username { get; set; }
        public string OnlineValue { get; set; }
        public string IsLiveStatus
        {
            get
            {
                return live_status ? "Visible" : "Hidden";
            }
        }
        public string NotLiveStatus
        {
            get
            {
                return live_status ? "Hidden" : "Visible";
            }
        }
        public string Refreshing
        {
            get
            {
                return refreshing ? "Visible" : "Hidden";
            }
        }
        public string RecordTime { get; set; }
        public string RecordSize { get; set; }
        public string BitRate { get; set; }
        public string MainButtonText
        {
            get
            {
                return record_status ? "停止" : "开始";
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public event DestoryRoomTaskHandler Destroyed;
        protected void PropertyChange(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public RoomTask(string roomid)
        {
            this.Roomid = roomid;

            Init();
        }

        public async void Init()
        {
            await RefreshInfo();
            commentProvider = ReceiveComment();
            init_ready = true;
        }

        public async Task RefreshInfo()
        {
            refreshing = true;
            PropertyChange("Refreshing");
            var RoomInfo = await PathFinder.GetRoomInfo(Roomid);
            if(RoomInfo != null)
            {
                if (RoomInfo.realRoomid == null)
                {
                    realRoomid = Roomid;
                }
                else
                {
                    realRoomid = RoomInfo.realRoomid;
                }

                Title = RoomInfo.title;
                if (Config.Instance.IsWaitStreaming)
                {
                    if (RoomInfo.liveStatus == true && live_status == false)
                    {
                        //开播了
                        if (init_ready == false)
                        {
                            WaitForStart();
                        }
                        else if (record_status == false) Start();
                    }
                    else if (RoomInfo.liveStatus == false && live_status == true)
                    {
                        //下播了
                        if (record_status == true) Stop();
                    }
                }
                live_status = RoomInfo.liveStatus;
                Username = RoomInfo.username;
                refreshing = false;

                PropertyChange("Title");
                PropertyChange("Username");
                PropertyChange("IsLiveStatus");
                PropertyChange("NotLiveStatus");
                PropertyChange("Refreshing");
            }
        }

        public async void WaitForStart()
        {
            await Task.Run(async () =>
            {
                if(init_ready == false)
                {
                    while (!init_ready)
                    {
                        await Task.Delay(1000);
                    }
                }
                if (record_status == false) Start();
            });
        }

        public void Destroy()
        {
            EndProcess();
            Destroyed?.Invoke(this);
        }

        public void EndProcess()
        {
            force_stoping = true;
            downloader?.Stop();
            commentProvider?.Disconnect();
        }

        public string CompilePath()
        {
            var config = Config.Instance;
            var path = System.IO.Path.Combine(config.SavePath, config.Filename);

            var safeTitle = SafetyFileName(Title);
            var safeUsername = SafetyFileName(Username);

            path = path.Replace("{roomid}", Roomid);
            path = path.Replace("{title}", safeTitle);
            path = path.Replace("{username}", safeUsername);
            path = path.Replace("{Y}", DateTime.Now.Year.ToString());
            path = path.Replace("{M}", DateTime.Now.Month.ToString());
            path = path.Replace("{d}", DateTime.Now.Day.ToString());
            path = path.Replace("{H}", DateTime.Now.Hour.ToString());
            path = path.Replace("{m}", DateTime.Now.Minute.ToString());
            path = path.Replace("{s}", DateTime.Now.Second.ToString());
            return path;
        }

        public static string SafetyFileName(string fString)
        {
            var invalidChars = System.IO.Path.GetInvalidFileNameChars();
            var invalidCharIndex = fString.IndexOfAny(invalidChars, 0);
            if (invalidCharIndex == -1) return fString;

            var safeString = new StringBuilder();
            var replaceIndex = 0;

            do
            {
                safeString.Append(fString, replaceIndex, invalidCharIndex - replaceIndex);
                safeString.Append("_");

                replaceIndex = invalidCharIndex + 1;
                invalidCharIndex = fString.IndexOfAny(invalidChars, replaceIndex);
            } while (invalidCharIndex != -1);

            safeString.Append(fString, replaceIndex, fString.Length - replaceIndex);
            return safeString.ToString();
        }

        private void Start()
        {
            var config = Config.Instance;

            if (config.SavePath == null || config.SavePath == "" || config.Filename == null || config.Filename == "")
            {
                MessageBox.Show("请先打开设置对话框，设定保存目录。");
                return;
            }
            downloader = new Downloader(realRoomid, commentProvider);
            downloader.OnDownloadInfoUpdate += Downloader_OnDownloadInfoUpdate;
            downloader.OnStop += Downloader_OnStop;

            record_status = true;
            downloader.Start(CompilePath());
            PropertyChange("MainButtonText");
        }

        private void Downloader_OnStop(object sender)
        {
            Stop();

            //如果不是用户触发的，检查状态后重试。
            if (force_stoping)
            {
                force_stoping = false;
                return;
            }
            var config = Config.Instance;
            if (config.IsAutoRetry && live_status)
            {
                //触发重试
                AutoRetry();
            }
        }

        private async void AutoRetry()
        {
            var config = Config.Instance;
            InfoLogger.SendInfo(Roomid, "INFO", "等待 " + config.RefreshTime + " 秒后重试。");
            await Task.Delay(int.Parse(config.RefreshTime ?? "30") * 1000);
            if(record_status == false) Start(); //保证同时只有一个下载（否则会下坏）
        }

        private void Stop()
        {
            downloader?.Stop();

            record_status = false;
            PropertyChange("MainButtonText");
            RecordSize = "";
            PropertyChange("RecordSize");
            RecordTime = "";
            PropertyChange("RecordTime");
            BitRate = "";
            PropertyChange("BitRate");
            OnlineValue = "";
            PropertyChange("OnlineValue");
        }

        public void StartButton()
        {
            if (record_status)
            {
                force_stoping = true;
                Stop();
            }
            else
            {
                Start();
            }
            
        }

        private void Downloader_OnDownloadInfoUpdate(object sender, DownloadInfoArgs e)
        {
            RecordSize = Downloader.FormatSize(e.Bytes);
            PropertyChange("RecordSize");
            RecordTime = e.Duration;
            PropertyChange("RecordTime");
            BitRate = e.Bitrate / 1000.0f + "Kbps";
            PropertyChange("BitRate");
        }

        private CommentProvider ReceiveComment()
        {
            try
            {
                var _commentProvider = new CommentProvider(realRoomid);
                _commentProvider.OnDisconnected += CommentProvider_OnDisconnected;
                _commentProvider.OnReceivedRoomCount += CommentProvider_OnReceivedRoomCount;
                _commentProvider.OnReceivedComment += CommentProvider_OnReceivedComment;
                _commentProvider.Connect();
                return _commentProvider;
            }
            catch (Exception e)
            {
                InfoLogger.SendInfo(Roomid, "ERROR", "弹幕服务器出错：" + e.Message);
                return null;
            }
        }

        private void CommentProvider_OnReceivedComment(object sender, ReceivedCommentArgs e)
        {
            try
            {
                //DEBUG: 弹幕显示测试
                //InfoLogger.SendInfo(Roomid, "收到弹幕", e.Comment.CommentUser + ": " + e.Comment.CommentText);
                //接收到弹幕时的处理。
                if (e.Comment.MsgType != MsgTypeEnum.LiveStart)
                {
                    if (e.Comment.MsgType != MsgTypeEnum.LiveEnd) return;
                    InfoLogger.SendInfo(Roomid, "INFO", "[主播结束直播]");
                    Stop();
                }
                else
                {
                    InfoLogger.SendInfo(Roomid, "INFO", "[主播开始直播]");
                    //重新开始下载直播
                    force_stoping = true;
                    Stop();
                    Start();
                }
            }
            catch (Exception ex)
            {
                InfoLogger.SendInfo(Roomid, "ERROR", "在收取弹幕时发生未知错误：" + ex.Message);
            }
        }

        private void CommentProvider_OnReceivedRoomCount(object sender, ReceivedRoomCountArgs e)
        {
            OnlineValue = e.UserCount.ToString();
            PropertyChange("OnlineValue");
        }

        private void CommentProvider_OnDisconnected(object sender, DisconnectEvtArgs e)
        {
            InfoLogger.SendInfo(Roomid, "INFO", "弹幕服务器断开");

            //如果不是用户触发的，则尝试重连。
            if (!force_stoping) return;
            InfoLogger.SendInfo(Roomid, "INFO", "尝试重新连接弹幕服务器");
            commentProvider.Connect();
        }
    }

    public class RoomList : ObservableCollection<RoomTask>
    {
        private Config config;

        public RoomList()
        {
            config = Config.Instance;
            if (config.IsWaitStreaming)
            {
                StartRefreshRoomStatus();
            }
        }

        public void AddRoom(string roomid, bool restore = false)
        {
            if(this.Count(i => i.Roomid == roomid) > 0)
            {
                if(restore == false) MessageBox.Show("直播间ID已经存在。", "添加失败", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                return;
            }
            var roomtask = new RoomTask(roomid);
            roomtask.Destroyed += Roomtask_Destroyed;
            Add(roomtask);
            if (restore == false) config.RoomId = String.Join(",", this.Select(i => i.Roomid));
        }

        private void Roomtask_Destroyed(object sender)
        {
            Remove((RoomTask)sender);
            config.RoomId = String.Join(",", this.Select(i => i.Roomid));
        }

        public void RestoreRooms()
        {
            if (config.RoomId != null)
            {
                var roomids = config.RoomId.Split(',');
                foreach (var roomid in roomids)
                {
                    AddRoom(roomid, true);
                }
            }
        }

        public async void StartRefreshRoomStatus()
        {
            await Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(int.Parse(config.RefreshTime ?? "30") * 1000);
                    RefreshInfo();
                }
            });
        }

        public void RefreshInfo()
        {
            foreach (var roomtask in this)
            {
                roomtask.RefreshInfo();
            }
        }

        public void DestroyAll()
        {
            foreach (var roomtask in this)
            {
                roomtask.EndProcess();
            }
        }
    }
}
