using BiliRoku.Commentlib;
using System;
using System.Threading.Tasks;

namespace BiliRoku.Bililivelib
{
    internal class Downloader
    {
        public bool IsRunning { get; private set; }
        private readonly MainWindow _mw;
        private string _roomid;
        private string _flvUrl;
        private CommentProvider _commentProvider;
        private FlvDownloader _flvDownloader;
        private long _recordedSize;

        private bool _downloadCommentOption = true;
        private bool _autoStart = true;

        public Downloader(MainWindow mw)
        {
            _mw = mw;
            IsRunning = false;
        }

        public async void Start()
        {
            _mw.SetProcessingBtn();
            if (IsRunning)
            {
                _mw.AppendLogln("ERROR", "已经是运行状态了。");
                return;
            }
            //设置运行状态。
            IsRunning = true;

            //读取设置
            var originalRoomId = _mw.roomIdBox.Text;
            var savepath = _mw.savepathBox.Text;
            _downloadCommentOption = _mw.saveCommentCheckBox.IsChecked ?? true;
            _autoStart = _mw.waitForStreamCheckBox.IsChecked ?? true;

            //准备查找下载地址
            var pathFinder = new PathFinder(_mw);
            //查找真实房间号
            _roomid = await pathFinder.GetRoomid(originalRoomId);
            if (_roomid != null)
            {
                _mw.SetStartBtn();
            }else
            {
                _mw.AppendLogln("ERROR", "未取得真实房间号");
                Stop();
                return; //停止并退出
            }
            //查找真实下载地址
            try
            {
                _flvUrl = await pathFinder.GetTrueUrl(_roomid);
            }catch
            {
                _mw.AppendLogln("ERROR", "未取得下载地址");
                Stop();
                return; //停止并退出
            }

            var cmtProvider = ReceiveComment();
            _flvDownloader = new FlvDownloader(_roomid, savepath, _downloadCommentOption, cmtProvider);
            _flvDownloader.Info += _flvDownloader_Info;
            CheckStreaming();
            try
            {
                _flvDownloader.Start(_flvUrl);
            }
            catch (Exception e)
            {
                _mw.AppendLogln("ERROR", "下载视频流时出错：" + e.Message);
                Stop();
            }
        }

        private void _flvDownloader_Info(object sender, DownloadInfoArgs e)
        {
            _mw.Dispatcher.Invoke(() =>
            {
                _mw.RecordTimeStatus.Content = e.Duration;
                _mw.BitrateStatus.Content = e.Bitrate / 1000.0f + "Kbps";
                _mw.SizeStatus.Content = FormatSize(e.Bytes);
            });
            _recordedSize = e.Bytes;
        }

        private async void CheckStreaming()
        {
            await Task.Delay(2000);
            if (_recordedSize <= 1)
            {
                if (_flvDownloader.IsDownloading)
                {
                    _flvDownloader.Stop();
                }
                _mw.Dispatcher.Invoke(() =>
                {
                    _mw.LiveStatus.Content = "未直播";
                });
            }
            else
            {
                _mw.Dispatcher.Invoke(() =>
                {
                    _mw.LiveStatus.Content = "正在直播";
                });
            }
        }

        private static string FormatSize(long size)
        {
            if (size <= 1024)
            {
                return size.ToString("F2") + "B";
            }
            if (size <= 1048576)
            {
                return (size / 1024.0).ToString("F2") + "KB";
            }
            if (size <= 1073741824)
            {
                return (size / 1048576.0).ToString("F2") + "MB";
            }
            if (size <= 1099511627776)
            {
                return (size / 1073741824.0).ToString("F2") + "GB";
            }
            return (size / 1099511627776.0).ToString("F2") + "TB";
        }

        public void Stop()
        {
            if (IsRunning)
            {
                IsRunning = false;
                _recordedSize = 0;
                if (_flvDownloader != null)
                {
                    _flvDownloader.Stop();
                    _flvDownloader = null;
                }
                _commentProvider?.Disconnect();
                _mw.AppendLogln("INFO", "停止");
            }else
            {
                _mw.AppendLogln("ERROR", "已经是停止状态了");
            }
            _mw.SetStopBtn();
        }

        private CommentProvider ReceiveComment()
        {
            try
            {
                _commentProvider = new CommentProvider(_roomid, _mw);
                _commentProvider.OnDisconnected += CommentProvider_OnDisconnected;
                _commentProvider.OnReceivedRoomCount += CommentProvider_OnReceivedRoomCount;
                _commentProvider.OnReceivedComment += CommentProvider_OnReceivedComment;
                _commentProvider.Connect();
                return _commentProvider;
            }catch(Exception e)
            {
                _mw.AppendLogln("ERROR", "弹幕服务器出错：" + e.Message);
                return null;
            }
        }

        private async void CommentProvider_OnReceivedComment(object sender, ReceivedCommentArgs e)
        {
            //接收到弹幕时的处理。
            if (e.Comment.MsgType != MsgTypeEnum.LiveStart)
            {
                if (e.Comment.MsgType != MsgTypeEnum.LiveEnd) return;
                _mw.AppendLogln("INFO", "[主播结束直播]");
                _flvDownloader?.Stop();
                if (!_autoStart)
                {
                    Stop();
                }
                else
                {
                    _mw.Dispatcher.Invoke(() => { _mw.LiveStatus.Content = "未直播"; });
                }
            }
            else
            {
                if (!_autoStart || _flvDownloader.IsDownloading) return;
                _flvDownloader.IsDownloading = true;
                _mw.AppendLogln("INFO", "[主播开始直播]");

                //准备查找下载地址
                var pathFinder = new PathFinder(_mw);
                
                //查找真实下载地址
                try
                {
                    _flvUrl = await pathFinder.GetTrueUrl(_roomid);
                }
                catch
                {
                    _mw.AppendLogln("ERROR", "未取得下载地址");
                    Stop();
                    return; //停止并退出
                }

                _mw.AppendLogln("INFO", $"新下载地址：{_flvUrl}");

                _flvDownloader.Start(_flvUrl);
                _mw.Dispatcher.Invoke(() => { _mw.LiveStatus.Content = "正在直播"; });
            }
        }

        private void CommentProvider_OnReceivedRoomCount(object sender, ReceivedRoomCountArgs e)
        {
            _mw.Dispatcher.Invoke(() =>
            {
                _mw.ViewerCountStatus.Content = e.UserCount.ToString();
            });
        }

        private void CommentProvider_OnDisconnected(object sender, DisconnectEvtArgs e)
        {
            _mw.AppendLogln("INFO", "弹幕服务器断开");

            //如果不是用户触发的，则尝试重连。
            if (!IsRunning) return;
            _mw.AppendLogln("INFO", "尝试重新连接弹幕服务器");
            _commentProvider.Connect();
        }
    }
}
