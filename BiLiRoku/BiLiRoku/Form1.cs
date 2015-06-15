using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BiLiRoku
{
    public partial class MainForm : Form
    {
        bool isRec = false; //是否正在录制
        BiliNamaPathFind bnpf; //获得真实地址
        DownloadFlv downloadFlv; //下载FLV

        public MainForm()
        {
            InitializeComponent();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("BiliRoku ver " + Version.VER + "\n\nBy zyzsdy\n\n主页：http://zyzsdy.com/biliroku", "关于 BiliRoku");
        }

        private void openSaveBtn_Click(object sender, EventArgs e)
        {
            saveFileDialog1.ShowDialog();
            savepathTxtBox.Text = saveFileDialog1.FileName;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            //进行一些初始化工作。
            isRec = false;
            bnpf = null;
            infoTxtBox.AppendText("[INFO " + DateTime.Now.ToString("HH:mm:ss") + "]:启动成功。\n");
            nowBytesLabel.Text = "";
            recTimeLabel.Text = "";
            nowTimeLabel.Text = "";
        }

        private void startBtn_Click(object sender, EventArgs e)
        {
            if (isRec)
            {
                startBtn.Text = "停止中...";
                startBtn.Enabled = false;

                downloadFlv.Stop();

                isRec = false;
                infoTxtBox.AppendText("[INFO " + DateTime.Now.ToString("HH:mm:ss") + "]:已停止。\n");
                startBtn.Text = "开始";
                startBtn.Enabled = true;
            }
            else
            {
                startBtn.Text = "启动中...";
                startBtn.Enabled = false;
                //检查必填字段
                string roomid = roomidTxtBox.Text;
                if (roomid == "")
                {
                    MessageBox.Show("请输入房间号。", "错误");
                    startBtn.Text = "开始";
                    startBtn.Enabled = true;
                    return;
                }
                string savepath = saveFileDialog1.FileName;
                if (savepath == "")
                {
                    MessageBox.Show("请选择保存路径。", "错误");
                    startBtn.Text = "开始";
                    startBtn.Enabled = true;
                    return;
                }
                infoTxtBox.AppendText("[INFO " + DateTime.Now.ToString("HH:mm:ss") + "]:开始解析...\n");

                //开始解析url
                bnpf = new BiliNamaPathFind();
                if(bnpf.Init(roomid, infoTxtBox))
                {
                    downloadFlv = new DownloadFlv();
                    downloadFlv.SetInfos(infoTxtBox, nowBytesLabel, recTimeLabel, nowTimeLabel);
                    if(downloadFlv.Start(bnpf.trueURL, savepath))
                    {
                        isRec = true;
                        startBtn.Text = "停止";
                        startBtn.Enabled = true;
                    }else
                    {
                        infoTxtBox.AppendText("[ERROR " + DateTime.Now.ToString("HH:mm:ss") + "]:已停止。\n");
                        startBtn.Text = "开始";
                        startBtn.Enabled = true;
                    }

                }
                else
                {
                    infoTxtBox.AppendText("[ERROR " + DateTime.Now.ToString("HH:mm:ss") + "]:已停止。\n");
                    startBtn.Text = "开始";
                    startBtn.Enabled = true;
                }

            }
            
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string filename = saveFileDialog1.FileName;
            if (filename == "")
            {
                MessageBox.Show("你还没选文件呢！！！！！", "Error?");
            }
            else
            {
                string path = System.IO.Path.GetDirectoryName(filename);
                System.Diagnostics.Process.Start("explorer.exe", path);
            }
        }
    }
}
