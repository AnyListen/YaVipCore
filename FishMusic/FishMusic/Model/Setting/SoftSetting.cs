using System;
using GalaSoft.MvvmLight;

namespace FishMusic.Model.Setting
{
    public class SoftSetting : ViewModelBase
    {
        private DownloadSettings _downSetting;

        public DownloadSettings DownSetting
        {
            get => _downSetting;
            set
            {
                _downSetting = value;
                RaisePropertyChanged("DownSetting");
            }
        }

        private PlaySettings _playSetting;

        public PlaySettings PlaySetting
        {
            get => _playSetting;
            set
            {
                _playSetting = value;
                RaisePropertyChanged("PlaySetting");
            }
        }

        public string Id { get; set; }
        public DateTime UpdateTime { get; set; }

        public SoftSetting()
        {
            Id = "luooqi";
            UpdateTime = DateTime.Now;
            DownSetting = new DownloadSettings();
            PlaySetting = new PlaySettings();
        }
    }
}