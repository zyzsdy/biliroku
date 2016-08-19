namespace BiliRoku.Bililivelib
{
    public delegate void DownloadInfoEvt(object sender, DownloadInfoArgs e);

    public class DownloadInfoArgs
    {
        public long Bytes;
        public string Duration;
        public int Bitrate;
    }
}
