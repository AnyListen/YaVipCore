using System.Linq;
using System.Windows.Forms;
using FishMusic.Helper;
using FishMusic.Model.Setting;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

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


        public RelayCommand<object> ChangeDownPathCmd { get; set; }

        public SettingViewModel()
        {
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
            SoftSetting.PlaySetting.PropertyChanged += DownSetting_PropertyChanged;
            ChangeDownPathCmd = new RelayCommand<object>(ChangeDownPath);
        }

        private void ChangeDownPath(object obj)
        {
            var dialog = new FolderBrowserDialog();
            if (DialogResult.OK == dialog.ShowDialog())
            {
                SoftSetting.DownSetting.DownPath = dialog.SelectedPath;
            }
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