using BiliRoku.MediaInfolib;
using System;
using System.ComponentModel;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using BiliRoku.Commentlib;
using static System.IO.Path;

namespace BiliRoku.Bililivelib
{
    public class FlvDownloader
    {
        public bool IsDownloading { get; private set; }

        private readonly string _roomid;
        private readonly string _savePath;
        private readonly bool _saveComment;
        private WebClient _wc;
        private readonly CommentProvider _cmtProvider;
        private CommentBuilder _xmlBuilder;
        
        private int _bitrate;
        private int _duration;

        public event DownloadInfoEvt Info;

        private long _nextCheck = 300000;

        public FlvDownloader(string roomid, string savePath, bool saveComment, CommentProvider cmtProvider)
        {
            _roomid = roomid;
            _savePath = savePath;
            _saveComment = saveComment;
            _cmtProvider = cmtProvider;
        }

        public void Start(string uri)
        {
            //初始化
            _nextCheck = 300000;

            _wc = new WebClient();
            _wc.Headers.Add("Accept: */*");
            _wc.Headers.Add("User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/78.0.3904.108 Safari/537.36");
            _wc.Headers.Add("Accept-Language: zh-CN,zh;q=0.8,en;q=0.6,ja;q=0.4");
            _wc.Headers.Add("Origin: https://live.bilibili.com");
            _wc.Headers.Add($"Referer: https://live.bilibili.com/blanc/{_roomid}?liteVersion=true");
            _wc.Headers.Add("Sec-Fetch-Site: cross-site");
            _wc.Headers.Add("Sec-Fetch-Mode: cors");
            _wc.DownloadFileCompleted += StopDownload;
            _wc.DownloadProgressChanged += ShowProgress;

            IsDownloading = true;
            
            //如果目录不存在，那么先创建目录。
            // ReSharper disable AssignNullToNotNullAttribute
            if (!System.IO.Directory.Exists(GetDirectoryName(_savePath)))
            {
                System.IO.Directory.CreateDirectory(GetDirectoryName(_savePath));
            }
            // ReSharper restore AssignNullToNotNullAttribute
            var startTimestamp = Convert.ToInt64((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalMilliseconds);
            _wc.DownloadFileAsync(new Uri(uri), _savePath);
            //如果勾选了“同时保存弹幕”，则开始下载弹幕
            if (!_saveComment) return;
            var xmlPath = ChangeExtension(_savePath, "xml");
            _xmlBuilder = new CommentBuilder(xmlPath, startTimestamp, _cmtProvider);
            try
            {
                _xmlBuilder.Start();
            }
            catch
            {
                throw;
            }
        }

        private void ShowProgress(object sender, DownloadProgressChangedEventArgs e)
        {
            if (e.BytesReceived >= _nextCheck)
            {
                GetFlvInfo();
                _nextCheck += 300000;
            }
            var bytes = e.BytesReceived;

            var durationTimeText = new StringBuilder();
            durationTimeText.Append((_duration/(1000*60*60)).ToString("00")).Append(":")
                .Append((_duration/(1000*60)%60).ToString("00")).Append(":")
                .Append((_duration/1000%60).ToString("00"));
            var durationText = durationTimeText.ToString();

            Info?.Invoke(this, new DownloadInfoArgs() { Bytes = bytes, Bitrate = _bitrate, Duration = durationText });
        }

        private void StopDownload(object sender, AsyncCompletedEventArgs e)
        {
            Stop();
        }

        public void Stop(bool force = false)
        {
            _wc.CancelAsync();
            _wc.Dispose();
            if (force)
            {
                _xmlBuilder?.QuickStop();
            }
            else
            {
                _xmlBuilder?.Stop();
            }
            _xmlBuilder = null;
            IsDownloading = false;
        }

        private async void GetFlvInfo()
        {
            await Task.Run(() =>
            {
                try
                {
                    var mi = new MediaInfo();
                    mi.Open(_savePath);
                    var durationStr = mi.Get(StreamKind.General, 0, "Duration");
                    var bitrateStr = mi.Get(StreamKind.Video, 0, "BitRate");

                    int.TryParse(bitrateStr, out _bitrate);
                    bitrateStr = mi.Get(StreamKind.Audio, 0, "BitRate");
                    int.TryParse(bitrateStr, out int audioBitrate);
                    _bitrate += audioBitrate;
                    mi.Close();
                    int.TryParse(durationStr, out _duration);
                }
                catch
                {
                    //忽略此处的错误。
                }
            });
        }
    }
}
