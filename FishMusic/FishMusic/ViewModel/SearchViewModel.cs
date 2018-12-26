using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using AnyListen.Models;
using FishMusic.Model;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace FishMusic.ViewModel
{
    public class SearchViewModel : ViewModelBase
    {
        private ObservableCollection<SongResult> _searchResultCollection;

        public ObservableCollection<SongResult> SearchResultCollection
        {
            get => _searchResultCollection;
            set
            {
                _searchResultCollection = value;
                RaisePropertyChanged("SearchResultCollection");
            }
        }

        public List<SearchEngine> EngineList { get; set; }

        public List<string> HotWords { get; set; }

        private string _selectEngine;
        private int _size = 100;
        private int _page = 1;

        public string SelectEngine
        {
            get => _selectEngine;
            set
            {
                _selectEngine = value;
                RaisePropertyChanged("SelectEngine");
            }
        }

        private string _searchText;

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                RaisePropertyChanged("SearchText");
            }
        }

        private string _currentPage;

        public string CurrentPage
        {
            get => _currentPage;
            set
            {
                _currentPage = value;
                RaisePropertyChanged("CurrentPage");
            }
        }

        private bool _canSearch;

        public bool CanSearch
        {
            get => _canSearch;
            set
            {
                _canSearch = value;
                if (!_canSearch)
                {
                    CurrentPage = "searching";
                }
                else if (SearchResultCollection == null)
                {
                    CurrentPage = "hotsearch";
                }
                else if (SearchResultCollection.Count == 0)
                {
                    CurrentPage = "noresult";
                }
                else
                {
                    CurrentPage = "search";
                }

                RaisePropertyChanged("CanSearch");
            }
        }

        private SongResult _selectSong;

        public SongResult SelectSong
        {
            get => _selectSong;
            set
            {
                _selectSong = value;
                RaisePropertyChanged("SelectSong");
            }
        }

        public RelayCommand SearchCmd { get; set; }
        public RelayCommand DoubleClickCmd { get; set; }
        public RelayCommand<object> HotWordsClickCmd { get; set; }

        public SearchViewModel()
        {
            EngineList = new List<SearchEngine>()
            {
                new SearchEngine() {Key = "wy", Name = "网易音乐"},
                new SearchEngine() {Key = "xm", Name = "虾米音乐"},
                new SearchEngine() {Key = "qq", Name = "腾讯音乐"},
                new SearchEngine() {Key = "bd", Name = "百度音乐"},
                new SearchEngine() {Key = "sn", Name = "索尼音乐"},
                new SearchEngine() {Key = "kg", Name = "酷狗音乐"},
                new SearchEngine() {Key = "kw", Name = "酷我音乐"},
            };

            HotWords = new List<string>() {"周杰伦", "丢火车", "α·Pav", "甜梅号", "田馥甄", "华晨宇", "林俊杰"};

            SelectEngine = "xm";
            CurrentPage = "search";
            SearchResultCollection = null;
            SelectSong = new SongResult();
            SearchCmd = new RelayCommand(SearchSong);
            DoubleClickCmd = new RelayCommand(PlaySong);
            HotWordsClickCmd = new RelayCommand<object>((s) =>
            {
                SearchText = s.ToString();
                SearchSong();
            });
            CanSearch = true;
        }

        private void PlaySong()
        {
            if (SelectSong == null)
            {
                return;
            }
            MessengerInstance.Send(SelectSong, "PlaySong");
        }

        private void SearchSong()
        {

            if (string.IsNullOrEmpty(SearchText?.Trim()))
            {
                return;
            }

            if (!CanSearch)
            {
                return;
            }

            CanSearch = false;
            Task.Factory.StartNew(() =>
            {
                try
                {
                    var list = AnyListen.AnyListen.GetMusic(_selectEngine).SongSearch(_searchText, _page, _size);
                    SearchResultCollection = new ObservableCollection<SongResult>(list);
                }
                finally
                {
                    CanSearch = true;
                }
            });
        }
    }
}