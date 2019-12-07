using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BiliRoku.Bililivelib;
using System.Threading.Tasks;
using System.Net;

namespace BiliRoku
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow
    {
        private RoomList _roomlist;

        public MainWindow()
        {
            ServicePointManager.ServerCertificateValidationCallback = (sender, cert, chain, errors) => true;
            _roomlist = new RoomList();
            InitializeComponent();
        }

        public void AppendLogln(string source, string level, string logText)
        {
            Dispatcher.Invoke(() =>
            {
                infoBlock.AppendText("[" + source + "] [" + level + " " + DateTime.Now.ToString("HH:mm:ss") + "] " + logText + "\n");
            });
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _roomlist.RestoreRooms();
            roomListView.ItemsSource = _roomlist;

            var _config = Config.Instance;
            //显示更新说明。
            if (_config.Version != Ver.VER)
            {
                MessageBox.Show("BiliRoku已经更新到 " + Ver.VER + "\n\n更新说明：\n" + Ver.DESC);
                _config.Version = Ver.VER;
            }
            AppendLogln("Core", "INFO", "启动成功。");
            var checkUpdate = new CheckUpdate();
            checkUpdate.OnResult += CheckUpdate_OnResult;
            InfoLogger.OnInfo += InfoLogger_OnInfo;
        }

        private void InfoLogger_OnInfo(InfoArgs info)
        {
            AppendLogln(info.source, info.level, info.info);
        }

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            throw new NotImplementedException();
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

        private void aboutLinkLabel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var about = new About {Owner = this};
            about.ShowDialog();
        }

        private void infoBlock_TextChanged(object sender, TextChangedEventArgs e)
        {
            infoBlock.ScrollToEnd();
        }

        private void AddRoomButton_Click(object sender, RoutedEventArgs e)
        {
            var addRoomDialog = new AddRoom { Owner = this };
            if(addRoomDialog.ShowDialog() == true)
            {
                var roomidString = addRoomDialog.roomid.Text;
                if(long.TryParse(roomidString, out long roomid))
                {
                    _roomlist.AddRoom(roomid.ToString());
                }
                else
                {
                    MessageBox.Show("房间号必须为一个整数。", "输入错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                
            }
        }

        private void DeleteRoom_Click(object sender, RoutedEventArgs e)
        {
            var source = (RoomTask)((Button)e.Source).DataContext;
            source.Destroy();
        }

        private void OpenSettingButton_Click(object sender, RoutedEventArgs e)
        {
            var settingDialog = new SavePathSetting { Owner = this };
            settingDialog.ShowDialog();
        }

        private void RefreshAllButton_Click(object sender, RoutedEventArgs e)
        {
            _roomlist.RefreshInfo();
        }

        private void RoomTaskMainButton_Click(object sender, RoutedEventArgs e)
        {
            var source = (RoomTask)((Button)e.Source).DataContext;
            source.StartButton();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            AppendLogln("Core", "INFO", "准备关闭，等待所有活动录像结束后退出。");
            _roomlist.DestroyAll();
        }
    }
}
