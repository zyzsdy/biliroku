using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using BiliRoku.Commentlib;

namespace BiliRoku.Bililivelib
{
    internal class CommentBuilder
    {
        private bool IsOpen { get; set; }

        private readonly string _xmlPath;
        private readonly long _nowTime;
        private readonly CommentProvider _cmtProvider;
        private readonly ConcurrentQueue<CommentModel> _cmtQueue;

        private const string XmlHeader = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><i><chatserver>chat.bilibili.com</chatserver><chatid>0</chatid><mission>0</mission><maxlimit>0</maxlimit><source>k-v</source>";
        private const string XmlFooter = "</i>";

        public CommentBuilder(string xmlPath, long nowTime, CommentProvider cmtProvider)
        {
            _xmlPath = xmlPath;
            _nowTime = nowTime;
            _cmtProvider = cmtProvider;
            _cmtQueue = new ConcurrentQueue<CommentModel>();
        }

        public void Start()
        {
            IsOpen = true;
            StartWrite();
            
            _cmtProvider.OnReceivedComment += _cmtProvider_OnReceivedComment;
        }

        private void _cmtProvider_OnReceivedComment(object sender, ReceivedCommentArgs e)
        {
            _cmtQueue.Enqueue(e.Comment);
        }

        public void Stop()
        {
            IsOpen = false;
        }
        public void QuickStop()
        {
            IsOpen = false;
            try
            {
                _sw.Close();
                _fs.Close();
            }
            catch
            {
                ;
            }
        }


        private FileStream _fs;
        private StreamWriter _sw;
        private async void StartWrite()
        {
            try
            {
                _fs = new FileStream(_xmlPath, FileMode.Create);
                _sw = new StreamWriter(_fs, Encoding.GetEncoding("UTF-8"));
                _sw.Write(XmlHeader);
            }
            catch
            {
                ;
            }
            
            StartFlush();
            while (IsOpen)
            {
                try
                {
                    var cmt = await Dequeue();
                    if (cmt == null || cmt.MsgType != MsgTypeEnum.Comment) continue;
                    _sw.WriteLine(cmt.ToString(_nowTime));
                }
                catch
                {
                    continue;
                }
            }
            try
            {
                _sw.Write(XmlFooter);
                _sw.Flush();
                _sw.Close();
                _fs.Close();
            }
            catch
            {
                ;
            }
        }

        private async Task<CommentModel> Dequeue()
        {
            CommentModel result = null;
            if (_cmtQueue.IsEmpty)
            {
                await Task.Delay(100);
            }
            else
            {
                while (!_cmtQueue.TryDequeue(out result))
                {
                }
            }
            return result;
        }

        private async void StartFlush()
        {
            try
            {
                while (IsOpen)
                {
                    if (_sw == null) continue;
                    await _sw.FlushAsync();
                    await Task.Delay(30000);
                }
            }catch
            {

            }
            
        }
    }
}
