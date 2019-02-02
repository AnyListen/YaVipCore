using System;
using System.IO;
using GalaSoft.MvvmLight;

namespace FishMusic.Model.Setting
{
    public class DownloadSettings : ViewModelBase
    {
        private int _bitRate;
        private int _lossType;
        private string _downPath;
        private string _userFolder;
        private string _userName;
        private int _nameSelect;
        private int _folderSelect;
        private bool _downPic;
        private bool _downLrc;
        private bool _enableUserSetting;

        public int BitRate
        {
            get => _bitRate;
            set
            {
                _bitRate = value;
                RaisePropertyChanged("BitRate");
            }
        }

        public int LossType
        {
            get => _lossType;
            set
            {
                _lossType = value;
                RaisePropertyChanged("LossType");
            }
        }

        public string DownPath
        {
            get => _downPath;
            set
            {
                _downPath = value;
                RaisePropertyChanged("DownPath");
            }
        }

        public string UserFolder
        {
            get => _userFolder;
            set
            {
                _userFolder = value;
                RaisePropertyChanged("UserFolder");
            }
        }

        public string UserName
        {
            get => _userName;
            set
            {
                _userName = value;
                RaisePropertyChanged("UserName");
            }
        }

        public int NameSelect
        {
            get => _nameSelect;
            set
            {
                _nameSelect = value;
                RaisePropertyChanged("NameSelect");
            }
        }

        public int FolderSelect
        {
            get => _folderSelect;
            set
            {
                _folderSelect = value;
                RaisePropertyChanged("FolderSelect");
            }
        }

        public bool DownPic
        {
            get => _downPic;
            set
            {
                _downPic = value;
                RaisePropertyChanged("DownPic");
            }
        }

        public bool DownLrc
        {
            get => _downLrc;
            set
            {
                _downLrc = value;
                RaisePropertyChanged("DownLrc");
            }
        }

        public bool EnableUserSetting
        {
            get => _enableUserSetting;
            set
            {
                _enableUserSetting = value;
                RaisePropertyChanged("EnableUserSetting");
            }
        }

        public DownloadSettings()
        {
            BitRate = 1;
            LossType = 0;
            DownPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Music");
            DownLrc = false;
            DownPic = false;
            EnableUserSetting = false;
            NameSelect = 1;
            FolderSelect = 0;
            UserName = "%ARTIST% - %SONG%";
            UserFolder = "";
        }
    }
}