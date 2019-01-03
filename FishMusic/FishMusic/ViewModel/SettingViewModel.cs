using System.Linq;
using FishMusic.Helper;
using FishMusic.Model;
using GalaSoft.MvvmLight;

namespace FishMusic.ViewModel
{
    public class SettingViewModel : ViewModelBase
    {
        private int _selectIndex;

        public int SelectIndex
        {
            get => _selectIndex;
            set
            {
                _selectIndex = value;
                RaisePropertyChanged("SelectIndex");
            }
        }

        private SoftSetting _softSetting;
        public SoftSetting SoftSetting
        {
            get => _softSetting;
            set
            {
                _softSetting = value;
                RaisePropertyChanged("SoftSetting");
            }
        }

        public SettingViewModel()
        {
            SelectIndex = 3;
            using (var db = DbHelper.GetDatabase())
            {
                var coll = db.GetCollection<SoftSetting>();
                var settingList = coll.FindAll().ToList();
                if (settingList.Any())
                {
                    SoftSetting = settingList.First();
                }
                else
                {
                    SoftSetting = new SoftSetting();
                    coll.Upsert(SoftSetting);
                }
            }
            SoftSetting.DownSetting.PropertyChanged += DownSetting_PropertyChanged;
        }

        private void DownSetting_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            using (var db = DbHelper.GetDatabase())
            {
                var coll = db.GetCollection<SoftSetting>();
                coll.Upsert(SoftSetting);
            }
        }
    }
}