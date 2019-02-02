using GalaSoft.MvvmLight;

namespace FishMusic.Model.Setting
{
    public class PlaySettings : ViewModelBase
    {
        private int _playQuality;

        public int PlayQuality
        {
            get => _playQuality;
            set
            {
                _playQuality = value;
                RaisePropertyChanged("PlayQuality");
            }
        }

        public PlaySettings()
        {
            PlayQuality = 1;
        }
    }
}