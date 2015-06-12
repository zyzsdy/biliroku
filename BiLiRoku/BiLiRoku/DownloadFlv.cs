using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BiLiRoku
{
    class DownloadFlv
    {
        string useragent = Version.UA;
        RichTextBox infoBox = null;
        Label nowByteLabel = null;
        string flvurl;
        string destPath;
        WebClient wc;

        public bool Start(string flvurl, string destPath, RichTextBox infoBox = null, Label nowByteLabel = null)
        {
            this.flvurl = flvurl;
            this.infoBox = infoBox;
            this.destPath = destPath;
            this.nowByteLabel = nowByteLabel;

            return NewDownload();
        }

        private bool NewDownload()
        {   
            wc = new WebClient();
            wc.Headers.Add("Accept: */*");
            wc.Headers.Add("User-Agent: " + useragent);
            wc.Headers.Add("Accept-Language: zh-CN,zh;q=0.8,en;q=0.6,ja;q=0.4");
            wc.DownloadFileCompleted += stopDownload;
            wc.DownloadProgressChanged += showProgress;
            try
            {
                wc.DownloadFileAsync(new Uri(flvurl), destPath);
            }
            catch (Exception e)
            {
                if (infoBox != null)
                {
                    infoBox.AppendText("[ERROR " + DateTime.Now.ToString("HH:mm:ss") + "] 视频流下载失败：" + e.Message + "\n");
                }
                return false;
            }
            infoBox.AppendText("[INFO " + DateTime.Now.ToString("HH:mm:ss") + "] 开始录制...\n");
            return true;
        }

        private void showProgress(object sender, DownloadProgressChangedEventArgs e)
        {
            if(nowByteLabel != null)
            {
                nowByteLabel.Text = "已接收：" + e.BytesReceived + " B";
            }
        }

        private void stopDownload(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            infoBox.AppendText("[INFO " + DateTime.Now.ToString("HH:mm:ss") + "] 录制停止。\n");
        }

        public void Stop()
        {
            infoBox.AppendText("[INFO " + DateTime.Now.ToString("HH:mm:ss") + "] 中断录制...\n");
            wc.CancelAsync();
            wc.Dispose();
        }
    }
}
