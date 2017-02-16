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
    internal class FlvDownloader
    {
        public bool IsDownloading;
        
        private readonly string _savePath;
        private readonly string _roomid;
        private string _compiledPath;
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
            _savePath = savePath;
            _roomid = roomid;
            _saveComment = saveComment;
            _cmtProvider = cmtProvider;
        }

        public void Start(string uri)
        {
            //初始化
            _nextCheck = 300000;

            _wc = new WebClient();
            _wc.Headers.Add("Accept: */*");
            _wc.Headers.Add("User-Agent: " + Ver.UA);
            _wc.Headers.Add("Accept-Language: zh-CN,zh;q=0.8,en;q=0.6,ja;q=0.4");
            _wc.DownloadFileCompleted += StopDownload;
            _wc.DownloadProgressChanged += ShowProgress;

            _compiledPath = CompilePath(_savePath, _roomid);
            IsDownloading = true;
            
            //如果目录不存在，那么先创建目录。
            // ReSharper disable AssignNullToNotNullAttribute
            if (!System.IO.Directory.Exists(GetDirectoryName(_compiledPath)))
            {
                System.IO.Directory.CreateDirectory(GetDirectoryName(_compiledPath));
            }
            // ReSharper restore AssignNullToNotNullAttribute
            var startTimestamp = Convert.ToInt64((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalMilliseconds);
            _wc.DownloadFileAsync(new Uri(uri), _compiledPath);
            //如果勾选了“同时保存弹幕”，则开始下载弹幕
            if (!_saveComment) return;
            var xmlPath = ChangeExtension(_compiledPath, "xml");
            _xmlBuilder = new CommentBuilder(xmlPath, startTimestamp, _cmtProvider);
            _xmlBuilder.Start();
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

        public void Stop()
        {
            _wc.CancelAsync();
            _wc.Dispose();
            _xmlBuilder?.Stop();
            _xmlBuilder = null;
            IsDownloading = false;
        }

        public static string CompilePath(string path, string roomid)
        {
            path = path.Replace("{roomid}", roomid);
            path = path.Replace("{Y}", DateTime.Now.Year.ToString());
            path = path.Replace("{M}", DateTime.Now.Month.ToString());
            path = path.Replace("{d}", DateTime.Now.Day.ToString());
            path = path.Replace("{H}", DateTime.Now.Hour.ToString());
            path = path.Replace("{m}", DateTime.Now.Minute.ToString());
            path = path.Replace("{s}", DateTime.Now.Second.ToString());
            return path;
        }

        private async void GetFlvInfo()
        {
            await Task.Run(() =>
            {
                try
                {
                    var mi = new MediaInfo();
                    mi.Open(_compiledPath);
                    var durationStr = mi.Get(StreamKind.General, 0, "Duration");
                    var bitrateStr = mi.Get(StreamKind.Video, 0, "BitRate");

                    _bitrate = int.Parse(bitrateStr);
                    bitrateStr = mi.Get(StreamKind.Audio, 0, "BitRate");
                    _bitrate += int.Parse(bitrateStr);
                    mi.Close();
                    _duration = int.Parse(durationStr);
                }
                catch
                {
                    //忽略此处的错误。
                }
            });
        }
    }
}
