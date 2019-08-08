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
                    FilenameBox.Text = FilenameBox.Text + ".flv";
                }
                else
                {
                    return;
                }
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
            MessageBox.Show(this, @"说明：

{roomid}--房间号 {title}--房间名 {username}--主播用户名
{Y}--年(四位) {M}--月 {d}--日
{H}--时 {m}--分 {s}--秒 (不自动补0,一位数时占一位)
{YY}(两位年份){MM}{dd}{HH}{mm}{ss}分别对应上方补0(保持占两位数)
{YYYY} 四位年份
注：若文件名中不含时间变量，则为固定文件名，固定文件名可能会被覆盖。
文件名中也可出现“\”字符，这时会建立子目录。", "保存文件名变量说明", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
