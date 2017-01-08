using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BiliRoku.Bililivelib;

namespace BiliRoku
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow
    {
        private Downloader _downloader;
        private Config _config;

        public MainWindow()
        {
            InitializeComponent();
        }

        public void AppendLogln(string level, string logText)
        {
            Dispatcher.Invoke(()=> {
                infoBlock.AppendText("[" + level + " " + DateTime.Now.ToString("HH:mm:ss") + "] " + logText + "\n");
            });
        }

        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            if (_downloader == null)
            {
                AppendLogln("ERROR", "初始化未成功，请尝试重启。");
                return;
            }

            if (_downloader.IsRunning)
            {
                _downloader.Stop();
            }
            else
            {
                _downloader.Start();
            }
        }

        public void SetProcessingBtn()
        {
            startButton.IsEnabled = false;
            startButton.Content = "处理中...";
        }
        public void SetStopBtn()
        {
            Dispatcher.Invoke(() =>
            {
                startButton.IsEnabled = true;
                roomIdBox.IsEnabled = true;
                saveCommentCheckBox.IsEnabled = true;
                waitForStreamCheckBox.IsEnabled = true;
                openSavepathConfigDialogButton.IsEnabled = true;
                RecordStatusGroupBox.Visibility = Visibility.Hidden;
                LiveStatus.Content = "检测中";
                RecordTimeStatus.Content = "00:00:00";
                BitrateStatus.Content = "0 Kbps";
                SizeStatus.Content = "0 B";
                startButton.Content = "开始";
            });
        }
        public void SetStartBtn()
        {
            Dispatcher.Invoke(() =>
            {
                startButton.IsEnabled = true;
                roomIdBox.IsEnabled = false;
                saveCommentCheckBox.IsEnabled = false;
                waitForStreamCheckBox.IsEnabled = false;
                openSavepathConfigDialogButton.IsEnabled = false;
                RecordStatusGroupBox.Visibility = Visibility.Visible;
                startButton.Content = "停止";
            });
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _downloader = new Downloader(this);
            _config = new Config();

            //显示更新说明。
            if (_config.Version != Ver.VER)
            {
                MessageBox.Show("BiliRoku已经更新到 " + Ver.VER + "\n\n更新说明：\n" + Ver.DESC);
                _config.Version = Ver.VER;
            }
            //读取配置并填入文本框
            if (_config.RoomId != null)
            {
                roomIdBox.Text = _config.RoomId;
            }
            if (_config.SaveLocation != null)
            {
                savepathBox.Text = _config.SaveLocation;
            }
            saveCommentCheckBox.IsChecked = _config.IsDownloadComment;
            waitForStreamCheckBox.IsChecked = _config.IsWaitStreaming;
            AppendLogln("INFO", "启动成功。");
            var checkUpdate = new CheckUpdate();
            checkUpdate.OnInfo += CheckUpdate_OnInfo;
            checkUpdate.OnResult += CheckUpdate_OnResult;
        }

        private void CheckUpdate_OnResult(object sender, UpdateResultArgs result)
        {
            Dispatcher.Invoke(() =>
            {
                aboutLinkLabel.Content = "发现新版本：" + result.version;
                aboutLinkLabel.MouseLeftButtonUp -= aboutLinkLabel_MouseLeftButtonUp;
                aboutLinkLabel.MouseLeftButtonUp += (s, e) =>
                {
                    System.Diagnostics.Process.Start("explorer.exe", result.url);
                };
            });
        }

        private void CheckUpdate_OnInfo(object sender, string info)
        {
            AppendLogln("AutoUpdate", info);
        }

        private void aboutLinkLabel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var about = new About {Owner = this};
            about.ShowDialog();
        }

        private void infoBlock_TextChanged(object sender, TextChangedEventArgs e)
        {
            infoBlock.ScrollToEnd();
        }

        private void roomIdBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_config != null)
            {
                _config.RoomId = roomIdBox.Text;
            }
        }

        private void savepathBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_config != null)
            {
                _config.SaveLocation = savepathBox.Text;
            }
        }

        private void saveCommentCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (_config != null)
            {
                _config.IsDownloadComment = saveCommentCheckBox.IsChecked ?? true;
            }
        }

        private void waitForStreamCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (_config != null)
            {
                _config.IsWaitStreaming = waitForStreamCheckBox.IsChecked ?? true;
            }
        }

        private void openSavepathConfigDialogButton_Click(object sender, RoutedEventArgs e)
        {
            var savePathSetting = new SavePathSetting {Owner = this};
            if (savePathSetting.ShowDialog() == true)
            {
                var savepath = savePathSetting.SavePath;
                if(savepath[savepath.Length - 1] == '\\')
                {
                    savepath = savepath.Substring(0, savepath.Length - 1);
                }
                var filename = savePathSetting.Filename;
                savepathBox.Text = savepath + "\\" + filename;
            }
        }

        private void savepathTextLabel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var filename = FlvDownloader.CompilePath(savepathBox.Text, roomIdBox.Text);
            if (filename == "")
            {
                MessageBox.Show("你还没选文件呢！！！！！", "Error?");
            }
            else
            {
                var path = System.IO.Path.GetDirectoryName(filename);
                System.Diagnostics.Process.Start("explorer.exe", path);
            }
        }
    }
}
