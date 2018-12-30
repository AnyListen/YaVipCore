using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FishMusic.Helper;

namespace FishMusic.Download
{
    public class XunleiClient : IDisposable
    {
        private IntPtr _taskIntPtr;
        private readonly string _fileName;
        private Xl.DownTaskParam _stParam;
//        public delegate void ProgressChangedHandle(object sender, ProgressChangedEventArgs e);
//        public event ProgressChangedHandle XunleiProgressChanged;
        //private int _progress;
        public DownloadInfo DownloadInfo  { get; set; }


        public XunleiClient(DownloadInfo info)
        {
            DownloadInfo = info;
            _fileName = info.FilePath;
            _stParam = new Xl.DownTaskParam
            {
                szTaskUrl = info.DownLink,
                szSavePath = Path.Combine(Environment.CurrentDirectory, "ErrorSongs"),
                szFilename = DateTime.Now.Ticks + CommonHelper.GetFormat(info.FilePath)
            };
            _taskIntPtr = Xl.XL_CreateTask(_stParam);
            Xl.XL_StartTask(_taskIntPtr);
        }

        public Task DownloadFileTaskAsync()
        {
            var tcs = new TaskCompletionSource<bool>();
            var downTaskInfo = new Xl.DownTaskInfo();
            new Timer(self =>
            {
                if (DownloadInfo.Status == DownStatus.PAUSE)
                {
                    Xl.XL_StopTask(_taskIntPtr);
                }
                if (DownloadInfo.Status == DownStatus.CANCLE)
                {
                    Xl.XL_StopTask(_taskIntPtr);
                    Xl.XL_DeleteTask(_taskIntPtr);
                    ((IDisposable)self).Dispose();
                    tcs.TrySetResult(true);
                    return;
                }
                Xl.XL_QueryTaskInfoEx(_taskIntPtr, downTaskInfo);
                //var isError = false;
                DownloadInfo.TotalBytesToDownload = downTaskInfo.nTotalSize;
                DownloadInfo.MegaBytesDownloaded = downTaskInfo.nTotalDownload;
                DownloadInfo.DownloadSpeed = downTaskInfo.nSpeed;
                switch (downTaskInfo.stat)
                {
                    case Xl.DownTaskStatus.TscDownload:
                        DownloadInfo.Progress = Convert.ToInt32((int) (downTaskInfo.fPercent * 100));
                        if (DownloadInfo.Status != DownStatus.PAUSE)
                        {
                            DownloadInfo.Status = DownStatus.RUNNING;
                        }
                        break;
                    case Xl.DownTaskStatus.TscComplete:
                        ((IDisposable)self).Dispose();
                        DownloadInfo.Progress = 100;
                        var oriPath = Path.Combine(Environment.CurrentDirectory, "ErrorSongs", _stParam.szFilename);
                        if (File.Exists(_fileName))
                        {
                            var newFileName = _fileName.Replace(CommonHelper.GetFormat(_fileName),
                                new Random(DateTime.Now.Millisecond).Next(0, 1000) + CommonHelper.GetFormat(_fileName));
                            File.Move(_fileName, newFileName);
                        }
                        FileInfo fileInfo = new FileInfo(_fileName);
                        if (!Directory.Exists(fileInfo.DirectoryName))
                        {
                            Directory.CreateDirectory(fileInfo.DirectoryName);
                        }
                        File.Move(oriPath, _fileName);
                        tcs.TrySetResult(true);
                        DownloadInfo.Status = DownStatus.SUCCESS;
                        DownloadInfo.Progress = 100;
                        break;
                    case Xl.DownTaskStatus.TscStartpending:
                        DownloadInfo.Progress = 0;
                        if (DownloadInfo.Status != DownStatus.PAUSE)
                        {
                            DownloadInfo.Status = DownStatus.RUNNING;
                        }
                        break;
                    case Xl.DownTaskStatus.TscPause:
                        if (DownloadInfo.Status == DownStatus.RUNNING)
                        {
                            Xl.XL_StartTask(_taskIntPtr);
                        }
                        break;
                    default:
                        DownloadInfo.Status = DownStatus.ERROR;
                        //isError = true;
                        ((IDisposable)self).Dispose();
                        tcs.TrySetResult(true);
                        break;
                }
                //XunleiProgressChanged?.Invoke(null, new ProgressChangedEventArgs(_progress, isError));
            }).Change(0,500);
            return tcs.Task;
        }

        public void Dispose()
        {
            _taskIntPtr = IntPtr.Zero;
            _stParam = null;
        }
    }
}