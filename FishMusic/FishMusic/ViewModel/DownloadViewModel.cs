using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Threading;
using FishMusic.Download;
using FishMusic.Helper;
using GalaSoft.MvvmLight;
using TagLib;
using TagLib.Id3v2;
using CommonHelper = AnyListen.Helper.CommonHelper;
using File = TagLib.File;
using Tag = TagLib.Id3v2.Tag;

namespace FishMusic.ViewModel
{
    public class DownloadViewModel : ViewModelBase, ICleanup
    {
        private int _itemSelIndex;

        public int ItemSelIndex
        {
            get => _itemSelIndex;
            set
            {
                _itemSelIndex = value;
                RaisePropertyChanged("ItemSelIndex");
            }
        }


        private ObservableCollection<DownloadInfo> _downloadingCollection = new ObservableCollection<DownloadInfo>();
        private ObservableCollection<DownloadInfo> _downloadedCollection = new ObservableCollection<DownloadInfo>();

        public ObservableCollection<DownloadInfo> DownloadingCollection
        {
            get => _downloadingCollection;
            set
            {
                _downloadingCollection = value;
                RaisePropertyChanged("DownloadingCollection");
            }
        }
        public ObservableCollection<DownloadInfo> DownloadedCollection
        {
            get => _downloadedCollection;
            set
            {
                _downloadedCollection = value;
                RaisePropertyChanged("DownloadedCollection");
            }
        }


        private bool _taskStop;

        public bool TaskStop
        {
            get => _taskStop;
            set
            {
                _taskStop = value;
                RaisePropertyChanged("TaskStop");
                var download = DownloadingCollection.SingleOrDefault(t => t.Status == DownStatus.RUNNING);
                if (download == null)
                {
                    return;
                }
                download.Status = _taskStop ? DownStatus.PAUSE : DownStatus.RUNNING;
            }
        }

        public DownloadViewModel()
        {
            InitDownCollection();
            MessengerInstance.Register(this, "DownloadSong", new Action<DownloadInfo>(down =>
            {
                if (DownloadingCollection.Count(d=>d.Id == down.Id) > 0 || DownloadedCollection.Count(d => d.Id == down.Id) > 0)
                {
                    return;
                }
                DownloadingCollection.Add(down);
                using (var db = DbHelper.GetDatabase())
                {
                    var col = db.GetCollection<DownloadInfo>();
                    col.Upsert(down);
                }
            }));
            Task.Factory.StartNew(DownloadSong);
        }

        private void InitDownCollection()
        {
            using (var db = DbHelper.GetDatabase())
            {
                var col = db.GetCollection<DownloadInfo>();
                var list = col.FindAll().ToList();
                if (list.Any())
                {
                    DownloadingCollection = new ObservableCollection<DownloadInfo>(list.Where(t=>t.Status != DownStatus.SUCCESS));
                    DownloadedCollection = new ObservableCollection<DownloadInfo>(list.Where(t=>t.Status == DownStatus.SUCCESS));
                }
            }
        }

        private void DownloadSong()
        {
            while (true)
            {
                var singleTask = DownloadingCollection.FirstOrDefault(t => t.Status == DownStatus.WAITING);
                if (singleTask == null)
                {
                    Thread.Sleep(1000);
                    continue;
                }
                while (_taskStop)
                {
                    Thread.Sleep(1000);
                }
                using (var client = new XunleiClient(singleTask))
                {
                    var task = client.DownloadFileTaskAsync();
                    task.Wait();
                }
                if (singleTask.Status == DownStatus.SUCCESS)
                {
                    ChangeID3(singleTask);
                    singleTask.SuccessTime = DateTime.Now;
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                    {
                        DownloadingCollection.Remove(singleTask);
                        DownloadedCollection.Add(singleTask);
                    }));
                }
                using (var db = DbHelper.GetDatabase())
                {
                    var col = db.GetCollection<DownloadInfo>();
                    col.Upsert(singleTask);
                }
            }
        }

        private void ChangeID3(DownloadInfo downloadInfo)
        {
            var filePath = new FileInfo(downloadInfo.FilePath);
            if (!filePath.Exists) return;
            try
            {
                var songResult = downloadInfo.SongInfo;
                using (var file = File.Create(filePath.FullName))
                {
                    Tag.DefaultVersion = 3;
                    Tag.ForceDefaultVersion = true;
                    Tag.DefaultEncoding = StringType.UTF8;
                    if (file == null)
                    {
                        return;
                    }
                    if (filePath.FullName.Contains("ogg") || filePath.FullName.Contains("wav"))
                    {
                        var id3V1 = file.GetTag(TagTypes.Id3v1, true);
                        if (!string.IsNullOrEmpty(songResult.SongName))
                        {
                            id3V1.Title = songResult.SongName;
                        }
                        if (!string.IsNullOrEmpty(songResult.ArtistName))
                        {
                            id3V1.Performers = songResult.ArtistName.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                        }
                        if (!string.IsNullOrEmpty(songResult.AlbumName))
                        {
                            id3V1.Album = songResult.AlbumName;
                        }
                        if (songResult.TrackNum != 0)
                        {
                            id3V1.Track = Convert.ToUInt32(songResult.TrackNum);
                        }
                        id3V1.Disc = Convert.ToUInt32(songResult.Disc);
                        if (!string.IsNullOrEmpty(songResult.Year))
                        {
                            id3V1.Year = Convert.ToUInt32(songResult.Year.Substring(0, 4));
                        }
                        id3V1.Comment = "鱼声音乐";
                    }
                    else
                    {
                        TagLib.Tag tags;
                        if (filePath.FullName.Contains("ape"))
                        {
                            tags = file.GetTag(TagTypes.Ape, true);
                        }
                        else if (filePath.FullName.Contains("flac"))
                        {
                            tags = file.GetTag(TagTypes.FlacMetadata, true);
                        }
                        else
                        {
                            tags = file.GetTag(TagTypes.Id3v2, true);
                        }
                        if (!string.IsNullOrEmpty(songResult.SongName))
                        {
                            tags.Title = songResult.SongName;
                        }
                        if (!string.IsNullOrEmpty(songResult.ArtistName))
                        {
                            tags.Performers = songResult.ArtistName.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                        }
                        if (!string.IsNullOrEmpty(songResult.AlbumName))
                        {
                            tags.Album = songResult.AlbumName;
                        }
                        if (!string.IsNullOrEmpty(songResult.AlbumArtist))
                        {
                            tags.AlbumArtists = songResult.AlbumArtist.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                        }
                        if (songResult.TrackNum != 0)
                        {
                            tags.Track = Convert.ToUInt32(songResult.TrackNum);
                        }
                        tags.Disc = Convert.ToUInt32(songResult.Disc);
                        tags.Copyright = "鱼声音乐";
                        if (!string.IsNullOrEmpty(songResult.Year))
                        {
                            tags.Year = Convert.ToUInt32(songResult.Year.Substring(0, 4));
                            if (tags.TagTypes == TagTypes.Id3v2)
                            {
                                var dat = TextInformationFrame.Get((Tag)tags, "TDAT", true);
                                dat.Text = new[] { songResult.Year };
                            }
                        }
                        if (tags.TagTypes == TagTypes.Id3v2)
                        {
                            if (!string.IsNullOrEmpty(songResult.Company))
                            {
                                var cmp = TextInformationFrame.Get((Tag)tags, "TPUB", true);
                                cmp.Text = new[] { songResult.Company };
                            }
                            if (!string.IsNullOrEmpty(songResult.Language))
                            {
                                var cmp = TextInformationFrame.Get((Tag)tags, "TLAN", true);
                                cmp.Text = new[] { songResult.Language };
                            }
                            if (songResult.Length != 0)
                            {
                                var cmp = TextInformationFrame.Get((Tag)tags, "TLEN", true);
                                cmp.Text = new[] { songResult.Length.ToString() };
                            }
                            if (!string.IsNullOrEmpty(songResult.SongSubName))
                            {
                                var title = TextInformationFrame.Get((Tag)tags, "TIT3", true);
                                title.Text = new[] { songResult.SongSubName };
                            }
                        }
                        if (!string.IsNullOrEmpty(songResult.LrcUrl))
                        {
                            try
                            {
                                var html = new WebClient { Encoding = Encoding.UTF8 }.DownloadString(AnyListen.AnyListen.GetRealUrl(songResult.LrcUrl));
                                if (!string.IsNullOrEmpty(html))
                                {
                                    html = HttpUtility.HtmlDecode(html);
                                    html = HttpUtility.HtmlDecode(html);
                                    tags.Lyrics = html;
                                    // ReSharper disable once StringLastIndexOfIsCultureSpecific.1
                                    //var lrcPath = filePath.FullName.Substring(0, filePath.FullName.LastIndexOf(".")) +
                                    //              ".lrc";
                                    //System.IO.File.WriteAllText(lrcPath, html);
                                }
                            }
                            catch (Exception)
                            {
                                //
                            }
                        }

                        try
                        {
                            if (!string.IsNullOrEmpty(songResult.PicUrl))
                            {
                                var picArr = new WebClient().DownloadData(AnyListen.AnyListen.GetRealUrl(songResult.PicUrl));
                                var picture = new Picture
                                {
                                    Description = "luooqi",
                                    MimeType = MediaTypeNames.Image.Jpeg,
                                    Type = PictureType.FrontCover,
                                    Data = new ByteVector(picArr, picArr.Length)
                                };
                                tags.Pictures = new IPicture[] { picture };
                            }
                        }
                        catch (Exception ex)
                        {
                            CommonHelper.AddLog(ex.ToString());
                        }
                        file.Save();
                    }
                }
            }
            catch (Exception ex)
            {
                CommonHelper.AddLog(ex.ToString());
            }
        }

        public override void Cleanup()
        {
            MessengerInstance.Unregister<bool>(this, "DownloadSong");
        }
    }
}