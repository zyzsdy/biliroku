using MediaInfoLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BiLiRoku
{
    class DownloadFlv
    {
        string useragent = Version.UA; //发往服务器的user-agent
        RichTextBox infoBox = null; //主界面日志消息输出
        Label nowByteLabel = null; //主界面已下载字节数标签。
        Label recTimeLabel = null; //主界面已录制时间标签。
        Label nowTimeLabel = null; //主界面录制到时间标签。
        string flvurl; //当前下载的FLV
        string destPath; //当前存储FLV的路径
        WebClient wc;

        DateTime startTime; //开始录制时间
        int duration; //已经持续的时间
        int bitrate; //比特数
        int nextCheck = 300000; //下次检查的字节数
        bool isShowInfo = false; //是否已经显示FLV信息

        public void SetInfos(RichTextBox infoBox = null, Label nowByteLabel = null, Label recTimeLabel = null, Label nowTimeLabel = null)
        {
            this.infoBox = infoBox;
            this.nowByteLabel = nowByteLabel;
            this.recTimeLabel = recTimeLabel;
            this.nowTimeLabel = nowTimeLabel;
        }

        public bool Start(string flvurl, string destPath)
        {
            this.flvurl = flvurl;
            this.destPath = destPath;
            //初始化
            startTime = DateTime.Now;
            duration = 0;
            bitrate = 0;
            nextCheck = 300000;

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
                startTime = DateTime.Now;
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
            if (e.BytesReceived >= nextCheck)
            {
                Thread nt = new Thread(GetFlvInfo);
                nt.Start();
                nextCheck += 300000;
            }
            if(nowByteLabel != null)
            {
                nowByteLabel.Text = "已接收：" + e.BytesReceived + " B";
            }
            if(recTimeLabel != null)
            {
                StringBuilder durationTimeText = new StringBuilder("已录制：");
                durationTimeText.Append((duration / (1000 * 60 * 60)).ToString("00")).Append(":")
                    .Append(((duration / (1000 * 60)) % 60).ToString("00")).Append(":")
                    .Append(((duration / 1000) % 60).ToString("00")).Append(".")
                    .Append((duration % 1000).ToString("000"));
                recTimeLabel.Text = durationTimeText.ToString(); 
            }
            if(nowTimeLabel != null)
            {
                nowTimeLabel.Text = "当前录到：" +
                startTime.AddMilliseconds(duration).ToString("HH:mm:ss.fff");
            }
            if (!isShowInfo)
            {
                if (bitrate > 0)
                {
                    infoBox.AppendText("[INFO " + DateTime.Now.ToString("HH:mm:ss") + "] Bitrate: " + bitrate / 1000.0f + "Kbps\n");
                    isShowInfo = true;
                }
            }
        }

        private void GetFlvInfo()
        {
            try {
                MediaInfo mi = new MediaInfo();
                mi.Open(destPath);
                string durationStr = mi.Get(StreamKind.General, 0, "Duration");
                if (!isShowInfo)
                {
                    string bitrateStr = mi.Get(StreamKind.Video, 0, "BitRate");
                    bitrate = int.Parse(bitrateStr);
                    bitrateStr = mi.Get(StreamKind.Audio, 0, "BitRate");
                    bitrate += int.Parse(bitrateStr);
                }
                mi.Close();
                duration = int.Parse(durationStr);
                
            }
            catch
            {
                //忽略此处的错误。
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
