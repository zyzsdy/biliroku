using BiliRoku.Commentlib;
using System;
using System.Threading.Tasks;

namespace BiliRoku.Bililivelib
{
    public delegate void DownloadInfoEvt(object sender, DownloadInfoArgs e);
    public delegate void DownloaderStopHandler(object sender);

    public class DownloadInfoArgs
    {
        public long Bytes;
        public string Duration;
        public int Bitrate;
    }

    public class Downloader
    {
        public event DownloadInfoEvt OnDownloadInfoUpdate;
        public event DownloaderStopHandler OnStop;

        public bool IsRunning { get; private set; } = false;
        public FlvDownloader flvDownloader;

        private string _roomid;
        private string _flvUrl;
        private CommentProvider _commentProvider;
        private long _recordedSize;

        private bool _downloadCommentOption = true;
        private bool _autoRetry = true;
        private int _streamTimeout;

        public Downloader(string roomid, CommentProvider cmtProvider)
        {
            _commentProvider = cmtProvider;
            _roomid = roomid;
        }

        public async void Start(string savepath)
        {
            try
            {
                if (IsRunning)
                {
                    InfoLogger.SendInfo(_roomid, "ERROR", "已经是运行状态了。");
                    return;
                }
                //设置运行状态。
                IsRunning = true;

                //读取设置
                var config = Config.Instance;
                _downloadCommentOption = config.IsDownloadComment;
                _autoRetry = config.IsAutoRetry;
                _streamTimeout = int.Parse(config.Timeout ?? "2000");

                //获取真实下载地址
                try
                {
                    _flvUrl = await PathFinder.GetTrueUrl(_roomid);
                }
                catch
                {
                    InfoLogger.SendInfo(_roomid, "ERROR", "未取得下载地址");
                    Stop();
                    return; //停止并退出
                }

                flvDownloader = new FlvDownloader(_roomid, savepath, _downloadCommentOption, _commentProvider);
                flvDownloader.Info += _flvDownloader_Info;
                CheckStreaming();
                try
                {
                    flvDownloader.Start(_flvUrl);
                }
                catch (Exception e)
                {
                    InfoLogger.SendInfo(_roomid, "ERROR", "下载视频流时出错：" + e.Message);
                    Stop();
                }
            }catch(Exception e)
            {
                InfoLogger.SendInfo(_roomid, "ERROR", "未知错误：" + e.Message);
                Stop();
            }
        }

        private void _flvDownloader_Info(object sender, DownloadInfoArgs e)
        {
            OnDownloadInfoUpdate?.Invoke(sender, e);
            _recordedSize = e.Bytes;
        }

        private async void CheckStreaming()
        {
            await Task.Delay(_streamTimeout);
            try
            {
                if (flvDownloader == null)
                {
                    return;
                }
                if (_recordedSize <= 1)
                {
                    InfoLogger.SendInfo(_roomid, "INFO", "接收流超时。");
                    Stop();
                }
            }catch(Exception ex)
            {
                InfoLogger.SendInfo(_roomid, "ERROR", "在检查直播状态时发生未知错误：" + ex.Message);
                Stop();
            }
        }

        public static string FormatSize(long size)
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
                if (flvDownloader != null)
                {
                    flvDownloader.Stop();
                    flvDownloader = null;
                }
                InfoLogger.SendInfo(_roomid, "INFO", "停止");
                OnStop?.Invoke(this);
            }
            else
            {
                InfoLogger.SendInfo(_roomid, "ERROR", "已经是停止状态了");
            }
        }
    }
}
