using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;

namespace BiliRoku
{
    /// <summary>
    /// SavePathSetting.xaml 的交互逻辑
    /// </summary>
    public partial class SavePathSetting
    {
        private Config _config;
        private SaveFileDialog _sfd;

        public SavePathSetting()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _config = Config.Instance;
            _sfd = new SaveFileDialog {FileName = "savefile.flv"};
            if (_config.SavePath != null)
            {
                SaveDirBox.Text = _config.SavePath;
                _sfd.InitialDirectory = _config.SavePath;
            }
            if (_config.Filename != null)
            {
                FilenameBox.Text = _config.Filename;
            }
            if (_config.RefreshTime != null)
            {
                refreshTimeBox.Text = _config.RefreshTime;
            }
            if (_config.Timeout != null)
            {
                timeoutBox.Text = _config.Timeout;
            }
            SaveCommetCheckBox.IsChecked = _config.IsDownloadComment;
            AutoRecordCheckBox.IsChecked = _config.IsWaitStreaming;
            AutoRetryCheckBox.IsChecked = _config.IsAutoRetry;
        }

        private void OpenSaveDialogButton_Click(object sender, RoutedEventArgs e)
        {
            if(_sfd.ShowDialog() == true)
            {
                SaveDirBox.Text = System.IO.Path.GetDirectoryName(_sfd.FileName);
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (SaveDirBox.Text == "")
            {
                MessageBox.Show("保存路径不能为空。", "BiliRoku");
                return;
            }
            if (!System.IO.Directory.Exists(SaveDirBox.Text))
            {
                if (MessageBoxResult.OK != MessageBox.Show("目录不存在，确认将创建此目录", "确认？", MessageBoxButton.OKCancel))
                {
                    return;
                }
            }
            if (FilenameBox.Text == "")
            {
                MessageBox.Show("文件名不能为空。", "BiliRoku");
                return;
            }
            if (!System.IO.Path.HasExtension(FilenameBox.Text))
            {
                if (MessageBoxResult.OK == MessageBox.Show("文件路径不含扩展名。确认将自动添加“.flv”的扩展名。", "确认？", MessageBoxButton.OKCancel))
                {
                    FilenameBox.Text += ".flv";
                }
                else
                {
                    return;
                }
            }
            if (!int.TryParse(refreshTimeBox.Text, out _))
            {
                MessageBox.Show("刷新间隔必须为整数。", "BiliRoku");
                return;
            }
            if (!int.TryParse(timeoutBox.Text, out _))
            {
                MessageBox.Show("超时时间必须为整数。", "BiliRoku");
                return;
            }
            if (_config != null)
            {
                _config.SavePath = SaveDirBox.Text;
                _config.Filename = FilenameBox.Text;
                _config.RefreshTime = refreshTimeBox.Text;
                _config.Timeout = timeoutBox.Text;
                _config.IsDownloadComment = SaveCommetCheckBox.IsChecked ?? false;
                _config.IsWaitStreaming = AutoRecordCheckBox.IsChecked ?? false;
                _config.IsAutoRetry = AutoRetryCheckBox.IsChecked ?? false;
            }
            DialogResult = true;
            Close();
        }

        private void SaveNameHelp_Click(object sender, RoutedEventArgs e)
        {
            var flnhlp = new FileNameHelp { Owner = this };
            flnhlp.ShowDialog();
        }
    }
}
