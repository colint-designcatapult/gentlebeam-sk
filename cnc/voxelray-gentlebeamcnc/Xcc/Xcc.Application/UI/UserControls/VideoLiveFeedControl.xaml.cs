using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using Unosquare.FFME.Common;

using Xcc.Application.Helpers;
using Xcc.Core.Constants;

namespace Xcc.Application.UI.UserControls
{
    /// <summary>
    /// Interaction logic for VideoLiveFeedControl
    /// </summary>
    public partial class VideoLiveFeedControl : UserControl
    {
        private Task<bool>? CloseLiveFeedTask { set; get; }

       
        #region Constructor
        public VideoLiveFeedControl()
        {
            InitializeComponent();

            FFMEVideoPlayer.MediaInitializing += FFMEVideoPlayer_MediaInitializing;
            FFMEVideoPlayer.MediaOpening += FFMEVideoPlayer_MediaOpening;
        }
        #endregion Constructor


        #region Events Handlers
        private void FFMEVideoPlayer_MediaOpening(object? sender, MediaOpeningEventArgs e)
        {
            e.Options.IsAudioDisabled = true;
            e.Options.IsSubtitleDisabled = true;
            e.Options.IsTimeSyncDisabled = true;
            e.Options.MinimumPlaybackBufferPercent = 0; //0.5 recommended
            e.Options.UseParallelDecoding = true;
            //e.Options.UseParallelRendering = true;
            e.Options.VideoForcedFps = 25 / 2.0;
        }

        private void FFMEVideoPlayer_MediaInitializing(object? sender, MediaInitializingEventArgs e)
        {
            e.Configuration.PrivateOptions["rtsp_transport"] = "tcp";
            e.Configuration.PrivateOptions["flags"] = "low_delay";
            e.Configuration.GlobalOptions.FlagNoBuffer = true;
            e.Configuration.GlobalOptions.MaxAnalyzeDuration = TimeSpan.Zero;
        }

        private void FFMEVideoPlayer_Unloaded(object sender, RoutedEventArgs e)
        {
            return;
            //CloseLiveFeedTask = CloseLiveFeed(); //save the close task
            //await CloseLiveFeedTask;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            StartLiveFeed();
        }

        private void FFMEVideoPlayer_MediaFailed(object sender, MediaFailedEventArgs e)
        {
            Debug.WriteLine($"{this.GetHashCode()} media failed");
        }

        private void FFMEVideoPlayer_MediaClosed(object sender, EventArgs e)
        {
            Debug.WriteLine($"{this.GetHashCode()} media closed");
        }

        private async void ScreenshotButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var bitmap = await FFMEVideoPlayer.CaptureBitmapAsync();
                var position = FFMEVideoPlayer.FramePosition;

                var positionString = DateTime.Now.ToString("yyyy'-'MM'-'dd'_'HH'_'mm'_'ss'.'fff");
                var pathToScreenshot = Path.Combine(PathToDatabase, $"Screenshot_{positionString}.jpeg");

                pathToScreenshot = Path.GetFullPath(pathToScreenshot);

                var directoryName = Path.GetFullPath(PathToDatabase);
                if (Directory.Exists(directoryName) == false)
                {
                    Directory.CreateDirectory(directoryName);
                }

                bitmap?.Save(pathToScreenshot, ImageFormat.Jpeg);

                if (File.Exists(pathToScreenshot) == false)
                    ScreenshotCommand?.Execute(null);
                else
                    ScreenshotCommand?.Execute(pathToScreenshot);
            }
            catch(Exception ex)
            {
                throw new Exception("Failed to capture or save image from camera.", ex);
            }
        }

        public async Task<Bitmap> GetScreenshotAsync()
        {
            string failedCaptureImageMessage = "Failed to capture image from camera.";

            try
            {
                var capturedBitmap = await FFMEVideoPlayer.CaptureBitmapAsync();

                if (capturedBitmap is null)
                    throw new Exception(failedCaptureImageMessage);

                return capturedBitmap;
            }
            catch (Exception ex)
            {
                throw new Exception(failedCaptureImageMessage, ex);
            }
        }
        #endregion Events Handlers


        #region Private methods

        private void StartLiveFeed()
        {
            if (CurrentTask is null || CurrentTask.IsFaulted)
            {
                CurrentTask = new ObservableTask(StartLiveFeedAsync(), StringConstants.Camera.CameraUnavailableUiErrorMessage);
            }
        }

        private async Task StartLiveFeedAsync()
        {
            if (UriSource is null)
            {
                throw new ArgumentException(StringConstants.Camera.NoUriUiErrorMessage);
            }
            else if (await FFMEVideoPlayer.Open(UriSource) == false)
            {
                throw new Exception(StringConstants.Camera.NoConnectionUiErrorMessage);
            }
        }

        private async Task<bool> StopLiveFeed()
        {
            return FFMEVideoPlayer.IsPlaying == false || await FFMEVideoPlayer.Stop();
        }


        private async Task<bool> CloseLiveFeed() => await FFMEVideoPlayer.Close();
        #endregion Private methods


        #region Dependency Properties
        public Uri UriSource { get => (Uri)GetValue(UriSourceProperty); set => SetValue(UriSourceProperty, value); }

        public static readonly DependencyProperty UriSourceProperty =
            DependencyProperty.Register(
                "UriSource",
                typeof(Uri),
                typeof(VideoLiveFeedControl),
                new FrameworkPropertyMetadata(
                    default,
                    (obj, args) =>
                    {
                        if (obj is VideoLiveFeedControl control)
                        {
                            if (control.IsVisible == false) // update feed only if UriSource changed while control presented in the Visual tree.
                                return;

                            //control.StartLiveFeed();
                        }
                    }));

        public ICommand ScreenshotCommand { get => (ICommand)GetValue(ScreenshotCommandProperty); set => SetValue(ScreenshotCommandProperty, value); }

        public static readonly DependencyProperty ScreenshotCommandProperty =
            DependencyProperty.Register(
                "ScreenshotCommand",
                typeof(ICommand),
                typeof(VideoLiveFeedControl));

        public ICommand ExitCommand { get => (ICommand)GetValue(ExitCommandProperty); set => SetValue(ExitCommandProperty, value); }

        public static readonly DependencyProperty ExitCommandProperty =
            DependencyProperty.Register(
                "ExitCommand",
                typeof(ICommand),
                typeof(VideoLiveFeedControl));

        public string PathToDatabase { get => (string)GetValue(PathToDatabaseProperty); set => SetValue(PathToDatabaseProperty, value); }

        public static readonly DependencyProperty PathToDatabaseProperty =
            DependencyProperty.Register(
                "PathToDatabase",
                typeof(string),
                typeof(VideoLiveFeedControl));

        
        public Visibility ControlPanelVisibility { get => (Visibility)GetValue(ControlPanelVisibilityProperty); set => SetValue(ControlPanelVisibilityProperty, value); }

        public static readonly DependencyProperty ControlPanelVisibilityProperty =
            DependencyProperty.Register(
                "ControlPanelVisibility",
                typeof(Visibility),
                typeof(VideoLiveFeedControl));


        public ObservableTask CurrentTask { get => (ObservableTask)GetValue(CurrentTaskProperty); set => SetValue(CurrentTaskProperty, value); }

        public static readonly DependencyProperty CurrentTaskProperty =
            DependencyProperty.Register(
                nameof(CurrentTask),
                typeof(ObservableTask),
                typeof(VideoLiveFeedControl));
        #endregion Dependency Properties


    }
}
