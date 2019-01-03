using System;
using FishMusic.Download;
using GalaSoft.MvvmLight;

namespace FishMusic.Model
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

        public string Id { get; set; }
        public DateTime UpdateTime { get; set; }

        public SoftSetting()
        {
            Id = "luooqi";
            UpdateTime = DateTime.Now;
            DownSetting = new DownloadSettings();
        }
    }
}