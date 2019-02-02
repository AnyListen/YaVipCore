using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using AnyListen.Models;
using CommonServiceLocator;
using FishMusic.Download;
using FishMusic.Helper;
using FishMusic.Model.Setting;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MahApps.Metro.IconPacks;
using Un4seen.Bass;
using Un4seen.Bass.Misc;
using Color = System.Drawing.Color;

namespace FishMusic.ViewModel
{
    public class PlayViewModel : ViewModelBase, ICleanup
    {
        #region 初始化

        private SYNCPROC _endTrackSyncProc;

        private int _activeStreamHandle;
        public int ActiveStreamHandle
        {
            get => _activeStreamHandle;
            protected set
            {
                if (_activeStreamHandle == value) return;
                _activeStreamHandle = value;
                RaisePropertyChanged("ActiveStream");
            }
        }

        private BASSTimer _updateTimer;

        private int _playerVolume;
        public int PlayerVolume
        {
            get => _playerVolume;
            set
            {
                if (value < 0 || value > 100 || _playerVolume == value) return;
                _playerVolume = value;
                RaisePropertyChanged("PlayerVolume");
                VolumeKind = _playerVolume > 0 ? PackIconFontAwesomeKind.VolumeDownSolid : PackIconFontAwesomeKind.VolumeOffSolid;
                try
                {
                    Bass.BASS_ChannelSetAttribute(ActiveStreamHandle, BASSAttribute.BASS_ATTRIB_VOL, PlayerVolume / 100f);
                }
                catch (Exception ex)
                {
                    AnyListen.Helper.CommonHelper.AddLog(ex.ToString());
                }
            }
        }

        private bool _showTime;

        public bool ShowTime
        {
            get => _showTime;
            set
            {
                if (_showTime == value)
                {
                    return;
                }
                _showTime = value;
                RaisePropertyChanged("ShowTime");
            }
        }


        public RelayCommand UnloadedCmd { get; set; }
        public RelayCommand ImageResizeCmd { get; set; }
        public RelayCommand<object> ImageMouseDownCmd { get; set; }
        public RelayCommand<object> ImageMouseMoveCmd { get; set; }
        public RelayCommand ImageMouseLeaveCmd { get; set; }
        public RelayCommand<object> InitControlCmd { get; set; }

        private void InitBass()
        {
            try
            {
                var mainWindow = Application.Current.MainWindow;
                if (mainWindow == null)
                {
                    return;
                }
                var interopHelper = new WindowInteropHelper(mainWindow);
                BassNet.Registration("shelher@163.com", "2X2831371512622");
                if (Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, interopHelper.Handle))
                {
                    var info = new BASS_INFO();
                    Bass.BASS_GetInfo(info);
                }
                else
                {
                    MessageBox.Show("Bass_Init error!--请检查电脑的音频输出设备是否正常工作。", "初始化失败", MessageBoxButton.OK);
                }

                Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_NET_TIMEOUT, 15000);

                if (_updateTimer != null) return;
                _updateTimer = new BASSTimer(200);
                _updateTimer.Tick += timerUpdate_Tick;

                _endTrackSyncProc = (handle, channel, data, user) => Stop();

                //_waveformGenerateWorker.DoWork += waveformGenerateWorker_DoWork;
                //_waveformGenerateWorker.RunWorkerCompleted += waveformGenerateWorker_RunWorkerCompleted;
                //_waveformGenerateWorker.WorkerSupportsCancellation = true;
            }
            catch (Exception ex)
            {
                AnyListen.Helper.CommonHelper.AddLog(ex.ToString());
            }
        }
        
        private int _processWidth;

        public int ProcessWidth
        {
            get => _processWidth;
            set
            {
                if (_processWidth == value)
                {
                    return;
                }
                _processWidth = value;
                RaisePropertyChanged("ProcessWidth");
            }
        }

        private double _currentPosition;

        public double CurrentPosition
        {
            get => _currentPosition;
            set
            {
                _currentPosition = value;
                RaisePropertyChanged("CurrentPosition");
            }
        }


        private double _trackLength;

        public double TrackLength
        {
            get => _trackLength;
            set
            {
                if (Math.Abs(_trackLength - value) < 0.5)
                {
                    return;
                }
                _trackLength = value;
                RaisePropertyChanged("TrackLength");
            }
        }

        private string _popUpText;

        public string PopUpText
        {
            get => _popUpText;
            set
            {
                if (_popUpText == value)
                {
                    return;
                }
                _popUpText = value;
                RaisePropertyChanged("PopUpText");
            }
        }

        private SongResult _playingSong;

        public SongResult PlayingSong
        {
            get => _playingSong;
            set
            {
                _playingSong = value;
                RaisePropertyChanged("PlayingSong");
            }
        }

        private void timerUpdate_Tick(object sender, EventArgs e)
        {
            switch (Bass.BASS_ChannelIsActive(ActiveStreamHandle))
            {
                case BASSActive.BASS_ACTIVE_PAUSED:
                    IsPlaying = false;
                    break;
                case BASSActive.BASS_ACTIVE_STOPPED:
                    IsPlaying = false;
                    break;
                case BASSActive.BASS_ACTIVE_STALLED:
                case BASSActive.BASS_ACTIVE_PLAYING:
                    IsPlaying = true;
                    var pos = Bass.BASS_ChannelGetPosition(ActiveStreamHandle);
                    CurrentPosition = Bass.BASS_ChannelBytes2Seconds(ActiveStreamHandle, pos);
                    ProcessWidth = (int)Math.Ceiling(CurrentPosition * _gridControl.RenderSize.Width / TrackLength);
                    break;
            }
        }

        public override void Cleanup()
        {
            MessengerInstance.Unregister<SongResult>(this, "PlaySong");
        }

        #endregion

        private SoftSetting _softSetting;

        public PlayViewModel()
        {
            Xl.XL_Init();
            PlayingSong = new SongResult();
            InitControlCmd = new RelayCommand<object>(InitControl);
            ImageResizeCmd = new RelayCommand(DrawWave);
            ImageMouseDownCmd = new RelayCommand<object>(GridMouseDown);
            ImageMouseMoveCmd = new RelayCommand<object>(GridMouseMove);
            ImageMouseLeaveCmd = new RelayCommand(()=> { ShowTime = false; });
            PlayCmd = new RelayCommand(Play);
            PlayClickCmd = new RelayCommand(PlayClick);
            PauseCmd = new RelayCommand(Pause);
            StopCmd = new RelayCommand(Stop);
            RepeatClickCmd = new RelayCommand(()=> { IsRepeat = !IsRepeat; });
            RandomClickCmd = new RelayCommand(()=> { IsRandom = !IsRandom; });
            VolumeClickCmd = new RelayCommand(() =>
            {
                var temp = PlayerVolume;
                PlayerVolume = _lastVolume;
                _lastVolume = temp;
            });
            FullScreenCmd = new RelayCommand((() =>
            {
                MessengerInstance.Send<bool>(true, "FullScreen");
            }));

            BeforeCmd = new RelayCommand(PlayBefore);
            NextCmd = new RelayCommand(PlayNext);

            PlayerVolume = 90;
            InitBass();
            UnloadedCmd = new RelayCommand(() =>
            {
                _updateTimer.Stop();
                _updateTimer.Dispose();
                Stop();
                ActiveStreamHandle = 0;
                Xl.XL_UnInit();
            });

            MessengerInstance.Register(this, "PlaySong", new Action<SongResult>(s =>
            {
                if (s == null)
                {
                    return;
                }
                PlayingSong = s;
                var songUrl = AnyListen.AnyListen.GetRealUrl(CommonHelper.GetDownloadUrl(s, 0, _softSetting.DownSetting.LossType, false));
                PlaySong(songUrl);
            }));
            _softSetting = ServiceLocator.Current.GetInstance<SettingViewModel>().SoftSetting;
        }

        private void GridMouseMove(object obj)
        {
            if (ActiveStreamHandle == 0)
            {
                return;
            }
            if (!(obj is MouseEventArgs eventArgs)) return;
            ShowTime = false;
            var mouseDownPoint = eventArgs.GetPosition(_gridControl);

            var pos = mouseDownPoint.X / _gridControl.RenderSize.Width * TrackLength;
            PopUpText = CommonHelper.SecondsToTime((int) pos) + " / " + CommonHelper.SecondsToTime((int)TrackLength);
            ShowTime = true;
        }

        private void GridMouseDown(object obj)
        {
            if (_wf == null)
            {
                return;
            }
            if (!(obj is MouseButtonEventArgs eventArgs)) return;
            var mouseDownPoint = eventArgs.GetPosition(_gridControl);
            var pos = _wf.GetBytePositionFromX((int)mouseDownPoint.X, (int)_gridControl.RenderSize.Width, -1, -1);
            Bass.BASS_ChannelSetPosition(ActiveStreamHandle, pos);
        }

        private Grid _gridControl;
        private void InitControl(object obj)
        {
            if (!(obj is Grid grid))
            {
                return;
            }
            _gridControl = grid;
        }

        #region 播放控制
        public RelayCommand PlayClickCmd { get; set; }
        public RelayCommand PlayCmd { get; set; }
        public RelayCommand PauseCmd { get; set; }
        public RelayCommand StopCmd { get; set; }
        public RelayCommand BeforeCmd { get; set; }
        public RelayCommand NextCmd { get; set; }
        public RelayCommand RepeatClickCmd { get; set; }
        public RelayCommand RandomClickCmd { get; set; }
        public RelayCommand VolumeClickCmd { get; set; }
        public RelayCommand FullScreenCmd { get; set; }
        private bool _isPlaying;
        

        public bool IsPlaying
        {
            get => _isPlaying;
            set
            {
                if (_isPlaying == value) return;
                _isPlaying = value;
                RaisePropertyChanged("IsPlaying");
            }
        }

        private bool _isRepeat;
        public bool IsRepeat
        {
            get => _isRepeat;
            set
            {
                if (_isRepeat == value) return;
                _isRepeat = value;
                RaisePropertyChanged("IsRepeat");
            }
        }

        private bool _isRandom;
        public bool IsRandom
        {
            get => _isRandom;
            set
            {
                if (_isRandom == value) return;
                _isRandom = value;
                RaisePropertyChanged("IsRandom");
            }
        }

        private int _lastVolume;
        private PackIconFontAwesomeKind _volumeKind;
        public PackIconFontAwesomeKind VolumeKind
        {
            get => _volumeKind;
            set
            {
                if (_volumeKind == value) return;
                _volumeKind = value;
                RaisePropertyChanged("VolumeKind");
            }
        }

        private void PlayClick()
        {
            if (IsPlaying)
            {
                Pause();
            }
            else
            {
                if (ActiveStreamHandle != 0)
                {
                    Play();
                }
                else
                {
                    PlaySong(@"https://luooqi.com/music/cloud/xm_320_1770824323.mp3?sign=1943de79f63ea6fb1ad91775a1d6167a");
                }
            }
        }

        private void Play()
        {
            if (ActiveStreamHandle != 0)
            {
                Bass.BASS_ChannelPlay(ActiveStreamHandle, false);
            }
            _updateTimer.Start();
        }

        private void Pause()
        {
            if (!IsPlaying)
            {
                return;
            }
            Bass.BASS_ChannelPause(ActiveStreamHandle);
            IsPlaying = false;
            _updateTimer.Stop();
        }

        private void Stop()
        {
            if (ActiveStreamHandle != 0)
            {
                Bass.BASS_ChannelStop(ActiveStreamHandle);
            }
            IsPlaying = false;
            _updateTimer.Stop();
        }

        private void PlayBefore()
        {
            
        }

        private void PlayNext()
        {

        }


        //private bool _canPlay;
        //public bool CanPlay
        //{
        //    get => _canPlay;
        //    protected set
        //    {
        //        if (_canPlay == value) return;
        //        _canPlay = value;
        //        RaisePropertyChanged("CanPlay");
        //    }
        //}

        //private bool _canPause;
        //public bool CanPause
        //{
        //    get => _canPause;
        //    protected set
        //    {
        //        if (_canPause == value) return;
        //        _canPause = value;
        //        RaisePropertyChanged("CanPause");
        //    }
        //}

        //private bool _canStop;
        //public bool CanStop
        //{
        //    get => _canStop;
        //    protected set
        //    {
        //        if (_canStop == value) return;
        //        _canStop = value;
        //        RaisePropertyChanged("CanStop");
        //    }
        //}

        private Task _playTask;
        private CancellationTokenSource _cancellationToken;
        private void PlaySong(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return;
            }
            try
            {
                if (_playTask != null)
                {
                    _cancellationToken.Cancel();
                    _playTask.Wait();
                    _playTask.Dispose();
                }
            }
            catch (Exception e)
            {
                AnyListen.Helper.CommonHelper.AddLog(e.ToString());
            }
            _cancellationToken = new CancellationTokenSource();
            if (ActiveStreamHandle != 0)
            {
                Bass.BASS_StreamFree(ActiveStreamHandle);
            }
            _playTask = Task.Factory.StartNew(() =>
            {
                if (path.ToLower().Contains("http"))
                {
                    _myDownloadProc = MyCacheSync;
                    _fs?.Close();
                    _fs = null;
                    ActiveStreamHandle =
                        Bass.BASS_StreamCreateURL(path, 0, BASSFlag.BASS_DEFAULT, _myDownloadProc, IntPtr.Zero);
                }
                else
                {
                    ActiveStreamHandle = Bass.BASS_StreamCreateFile(path, 0, 0, BASSFlag.BASS_DEFAULT);
                }
                if (Bass.BASS_ChannelIsActive(ActiveStreamHandle) != BASSActive.BASS_ACTIVE_PLAYING)
                {
                    Bass.BASS_Start();
                }
                if (ActiveStreamHandle != 0 && Bass.BASS_ChannelPlay(ActiveStreamHandle, true))
                {
                    WaveBackImage = null;
                    var stream = Application
                        .GetResourceStream(new Uri("/Resources/Images/wave_demo.png", UriKind.RelativeOrAbsolute))
                        ?.Stream;
                    if (stream != null)
                    {
                        var bitMap = new System.Drawing.Bitmap(System.Drawing.Image.FromStream(stream));
                        WaveProcessImage = bitMap;
                        stream.Close();
                    }
                    TrackLength = Bass.BASS_ChannelBytes2Seconds(ActiveStreamHandle,
                        Bass.BASS_ChannelGetLength(ActiveStreamHandle));
                    Bass.BASS_ChannelSetSync(ActiveStreamHandle, BASSSync.BASS_SYNC_END, 0, _endTrackSyncProc,
                        IntPtr.Zero);
                    _updateTimer.Start();
                    try
                    {
                        Bass.BASS_ChannelSetAttribute(ActiveStreamHandle, BASSAttribute.BASS_ATTRIB_VOL,
                            PlayerVolume / 100f);
                    }
                    catch (Exception ex)
                    {
                        AnyListen.Helper.CommonHelper.AddLog(ex.ToString());
                    }
                    return;
                }
                else
                {
                    ActiveStreamHandle = 0;
                }
                AnyListen.Helper.CommonHelper.AddLog($"Error={Bass.BASS_ErrorGetCode()}");
                Bass.BASS_Stop();
            }, _cancellationToken.Token);
        }
        #endregion

        #region 自定义缓存

        private FileStream _fs;
        private DOWNLOADPROC _myDownloadProc;
        private byte[] _data;
        private WaveForm _wf;
        private WaveForm _wf1;

        private System.Drawing.Bitmap _waveBackImage;

        public System.Drawing.Bitmap WaveBackImage
        {
            get => _waveBackImage;
            set
            {
                _waveBackImage = value;
                RaisePropertyChanged("WaveBackImage");
            }
        }

        private System.Drawing.Bitmap _waveProcessImage;

        public System.Drawing.Bitmap WaveProcessImage
        {
            get => _waveProcessImage;
            set
            {
                _waveProcessImage = value;
                RaisePropertyChanged("WaveProcessImage");
            }
        }

        private void MyCacheSync(IntPtr buffer, int length, IntPtr user)
        {
            if (_fs == null)
            {
                var path = Path.Combine(Path.GetTempPath(), "dc0d9925971d790596704f8aa0360b93.mp3");
                if (File.Exists(path))
                {
                    try
                    {
                        File.Delete(path);
                    }
                    catch (Exception)
                    {
                        Thread.Sleep(1000);
                        File.Delete(path);
                    }
                }
                _fs = File.Create(path);
            }
            if (buffer == IntPtr.Zero)
            {
                _fs.Flush();
                _fs.Close();
                Thread.Sleep(1000);
                var path = Path.Combine(Path.GetTempPath(), "dc0d9925971d790596704f8aa0360b93.mp3");
                _wf = new WaveForm(path)
                {
                    FrameResolution = 0.02f,
                    CallbackFrequency = 2000,
                    DrawWaveForm = WaveForm.WAVEFORMDRAWTYPE.HalfMono,
                    NotifyHandler = DrawSync,
                    ColorBackground = Color.Transparent,
                    ColorLeft = Color.FromArgb(170, 128, 186, 69),
                    ColorLeftEnvelope = Color.Transparent,
                    DrawEnvelope = false,
                    DrawCenterLine = false,
                    DrawGradient = false
                };
                _wf1 = new WaveForm(path)
                {
                    FrameResolution = 0.02f,
                    CallbackFrequency = 2000,
                    DrawWaveForm = WaveForm.WAVEFORMDRAWTYPE.HalfMono,
                    ColorBackground = Color.Transparent,
                    ColorLeft = Color.FromArgb(255, 128, 186, 69),
                    ColorLeftEnvelope = Color.Transparent,
                    DrawEnvelope = false,
                    DrawCenterLine = false,
                    DrawGradient = false
                };
                _wf.RenderStart(true, BASSFlag.BASS_DEFAULT);
                _wf1.RenderStart(true, BASSFlag.BASS_DEFAULT);
            }
            else
            {
                if (_data == null || _data.Length < length)
                    _data = new byte[length];
                Marshal.Copy(buffer, _data, 0, length);
                _fs.Write(_data, 0, length);
            }
        }

        private void DrawSync(int framesDone, int framesTotal, TimeSpan elapsedTime, bool finished)
        {
            DrawWave();
            if (!finished) return;
            _wf.SyncPlayback(ActiveStreamHandle);
            _wf1.SyncPlayback(ActiveStreamHandle);
        }

        private void DrawWave()
        {
            WaveBackImage = _wf?.CreateBitmap((int)_gridControl.RenderSize.Width, (int)_gridControl.RenderSize.Height, -1, -1, true);
            WaveProcessImage = _wf1?.CreateBitmap((int)_gridControl.RenderSize.Width, (int)_gridControl.RenderSize.Height, -1, -1, true);
        }

        #endregion

        //#region 可视化
        //private readonly int _fftDataSize = (int)FFTDataSize.FFT2048;
        //private readonly int _maxFft = (int)(BASSData.BASS_DATA_AVAILABLE | BASSData.BASS_DATA_FFT2048);
        //private int _sampleFrequency = 44100;

        //public RelayCommand<object> RegistCmd { get; set; }

        //private void Regist(object obj)
        //{
        //    if (!(obj is WaveformTimeline control))
        //    {
        //        return;
        //    }
        //    control.RegisterSoundPlayer(this);
        //}

        //public bool GetFFTData(float[] fftDataBuffer)
        //{
        //    return (Bass.BASS_ChannelGetData(_activeStreamHandle, fftDataBuffer, _maxFft)) > 0;
        //}

        //public int GetFFTFrequencyIndex(int frequency)
        //{
        //    return Utils.FFTFrequency2Index(frequency, _fftDataSize, _sampleFrequency);
        //}

        //#endregion

        //#region 可视化-频谱
        //private bool _inChannelSet;             //是否正在设置ChannelPosition
        //private bool _inChannelTimerUpdate;     //是否正在更新TimerUpdate-->position

        //private double _channelPosition;

        //public double ChannelPosition
        //{
        //    get => _channelPosition;
        //    set
        //    {
        //        if (_inChannelSet) return;
        //        _inChannelSet = true; // Avoid recursion
        //        var oldValue = _channelPosition;
        //        var position = Math.Max(0, Math.Min(value, ChannelLength));
        //        if (!_inChannelTimerUpdate)
        //        {
        //            Bass.BASS_ChannelSetPosition(ActiveStreamHandle, Bass.BASS_ChannelSeconds2Bytes(ActiveStreamHandle, position));
        //        }
        //        _channelPosition = position;
        //        if (Math.Abs(oldValue - _channelPosition) > 0.0001)
        //        {
        //            RaisePropertyChanged("ChannelPosition");
        //        }
        //        _inChannelSet = false;
        //    }
        //}

        //private double _channelLength;

        //public double ChannelLength
        //{
        //    get => _channelLength;
        //    protected set
        //    {
        //        if (!(Math.Abs(_channelLength - value) > 0.0001)) return;
        //        _channelLength = value;
        //        RaisePropertyChanged("ChannelLength");
        //    }
        //}

        //private float[] _waveformData;

        //public float[] WaveformData
        //{
        //    get => _waveformData;
        //    protected set
        //    {
        //        if (_waveformData == value)
        //        {
        //            return;
        //        }
        //        _waveformData = value;
        //        RaisePropertyChanged("WaveformData");
        //    }
        //}

        //private TimeSpan _selectionBegin;

        //public TimeSpan SelectionBegin
        //{
        //    get => _selectionBegin;
        //    set
        //    {
        //        if (_selectionBegin == value)
        //        {
        //            return;
        //        }
        //        _selectionBegin = value;
        //        RaisePropertyChanged("SelectionBegin");
        //    }
        //}

        //private TimeSpan _selectionEnd;

        //public TimeSpan SelectionEnd
        //{
        //    get => _selectionEnd;
        //    set
        //    {
        //        if (_selectionEnd == value)
        //        {
        //            return;
        //        }
        //        _selectionEnd = value;
        //        RaisePropertyChanged("SelectionEnd");
        //    }
        //}

        //#endregion

        //#region Waveform Generation
        //private readonly BackgroundWorker _waveformGenerateWorker = new BackgroundWorker();
        //private string _pendingWaveformPath;
        //private const int WaveformCompressedPointCount = 2000;

        //private class WaveformGenerationParams
        //{
        //    public WaveformGenerationParams(int points, string path)
        //    {
        //        Points = points;
        //        Path = path;
        //    }
        //    public int Points { get; }
        //    public string Path { get; }
        //}

        //private void GenerateWaveformData(string path)
        //{
        //    if (_waveformGenerateWorker.IsBusy)
        //    {
        //        _pendingWaveformPath = path;
        //        _waveformGenerateWorker.CancelAsync();
        //        return;
        //    }

        //    if (!_waveformGenerateWorker.IsBusy)
        //        _waveformGenerateWorker.RunWorkerAsync(new WaveformGenerationParams(WaveformCompressedPointCount, path));
        //}

        //private void waveformGenerateWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        //{
        //    if (!e.Cancelled) return;
        //    if (!_waveformGenerateWorker.IsBusy)
        //        _waveformGenerateWorker.RunWorkerAsync(new WaveformGenerationParams(WaveformCompressedPointCount, _pendingWaveformPath));
        //}

        //private void waveformGenerateWorker_DoWork(object sender, DoWorkEventArgs e)
        //{
        //    if (!(e.Argument is WaveformGenerationParams waveformParams))
        //    {
        //        return;
        //    }
        //    int stream;
        //    if (waveformParams.Path.Contains("http"))
        //    {
        //        stream = Bass.BASS_StreamCreateURL(waveformParams.Path, 0,
        //            BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_STREAM_PRESCAN, null,
        //            IntPtr.Zero);
        //    }
        //    else
        //    {
        //        stream = Bass.BASS_StreamCreateFile(waveformParams.Path, 0, 0, BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_STREAM_PRESCAN);
        //    }
        //    UpdateWave(stream, waveformParams, e);
        //    Bass.BASS_StreamFree(stream);
        //}

        //private void UpdateWave(int stream, WaveformGenerationParams waveformParams, DoWorkEventArgs e)
        //{
        //    var frameLength = (int)Bass.BASS_ChannelSeconds2Bytes(stream, 0.02);
        //    var streamLength = Bass.BASS_ChannelGetLength(stream, 0);
        //    var frameCount = (int)(streamLength / (double)frameLength);
        //    var waveformLength = frameCount * 2;
        //    var waveformData = new float[waveformLength];
        //    var levels = new float[2];

        //    var actualPoints = Math.Min(waveformParams.Points, frameCount);

        //    var compressedPointCount = actualPoints * 2;
        //    var waveformCompressedPoints = new float[compressedPointCount];
        //    var waveMaxPointIndexes = new List<int>();
        //    for (var i = 1; i <= actualPoints; i++)
        //    {
        //        waveMaxPointIndexes.Add((int)Math.Round(waveformLength * (i / (double)actualPoints), 0));
        //    }

        //    var maxLeftPointLevel = float.MinValue;
        //    var maxRightPointLevel = float.MinValue;
        //    var currentPointIndex = 0;
        //    for (var i = 0; i < waveformLength; i += 2)
        //    {
        //        Bass.BASS_ChannelGetLevel(stream, levels, 0.02f, BASSLevel.BASS_LEVEL_STEREO);
        //        //levels = Bass.BASS_ChannelGetLevels(stream);
        //        waveformData[i] = levels[0];
        //        waveformData[i + 1] = levels[1];

        //        if (levels[0] > maxLeftPointLevel)
        //            maxLeftPointLevel = levels[0];
        //        if (levels[1] > maxRightPointLevel)
        //            maxRightPointLevel = levels[1];

        //        if (i > waveMaxPointIndexes[currentPointIndex])
        //        {
        //            waveformCompressedPoints[(currentPointIndex * 2)] = maxLeftPointLevel;
        //            waveformCompressedPoints[(currentPointIndex * 2) + 1] = maxRightPointLevel;
        //            maxLeftPointLevel = float.MinValue;
        //            maxRightPointLevel = float.MinValue;
        //            currentPointIndex++;
        //        }
        //        if (i % 3000 == 0)
        //        {
        //            var clonedData = (float[])waveformCompressedPoints.Clone();
        //            Application.Current.Dispatcher.Invoke(new Action(() =>
        //            {
        //                WaveformData = clonedData;
        //            }));
        //        }
        //        if (!_waveformGenerateWorker.CancellationPending) continue;
        //        e.Cancel = true;
        //        break;
        //    }
        //    var finalClonedData = (float[])waveformCompressedPoints.Clone();
        //    Application.Current.Dispatcher.Invoke(new Action(() =>
        //    {
        //        WaveformData = finalClonedData;
        //    }));
        //}
        //#endregion
    }
}