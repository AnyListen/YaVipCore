using GalaSoft.MvvmLight;
using System;

namespace FishMusic.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private bool _fullScreen = false;

        public bool FullScreen
        {
            get => _fullScreen;
            set
            {
                _fullScreen = value;
                RaisePropertyChanged("FullScreen");
            }
        }

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            MessengerInstance.Register(this, "FullScreen", new Action<bool>(b =>
            {
                FullScreen = !FullScreen;
            }));
        }

        public override void Cleanup()
        {
            MessengerInstance.Unregister<bool>(this, "FullScreen");
        }
    }
}