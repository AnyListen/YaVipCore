using System;
using AnyListen.Models;
using GalaSoft.MvvmLight;

namespace FishMusic.Download
{
    public class DownloadInfo : ViewModelBase
    {
        public SongResult SongInfo { get; set; }

        private int _progress;

        public int Progress
        {
            get => _progress;
            set
            {
                _progress = value;
                RaisePropertyChanged("Progress");
            }
        }

        public string DownLink { get; set; }
        public string FilePath { get; set; }
        
        public string Id { get; set; }

        private DownStatus _status;

        public DownStatus Status
        {
            get => _status;
            set
            {
                _status = value;
                RaisePropertyChanged("Status");
            }
        }

        private long _megaBytesDownloaded;

        public long MegaBytesDownloaded
        {
            get => _megaBytesDownloaded;
            set
            {
                _megaBytesDownloaded = value;
                RaisePropertyChanged("MegaBytesDownloaded");
            }
        }

        private long _totalBytesToDownload;

        public long TotalBytesToDownload
        {
            get => _totalBytesToDownload;
            set
            {
                _totalBytesToDownload = value;
                RaisePropertyChanged("TotalBytesToDownload");
            }
        }

        private long _downloadSpeed;

        public long DownloadSpeed
        {
            get => _downloadSpeed;
            set
            {
                _downloadSpeed = value;
                RaisePropertyChanged("DownloadSpeed");
            }
        }

        public DateTime SuccessTime { get; set; }
        public DateTime CreateTime { get; set; }
    }

    public enum DownStatus
    {
        WAITING,
        RUNNING,
        PAUSE,
        CANCLE,
        ERROR,
        SUCCESS,
        UNKNOWN
    }
}