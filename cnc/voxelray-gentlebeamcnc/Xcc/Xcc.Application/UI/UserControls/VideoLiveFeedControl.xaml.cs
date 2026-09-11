using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.Web.WebView2.Wpf;
using Microsoft.Web.WebView2.Core;
using Prism.Commands;
using System.Globalization;

using Xcc.Application.Helpers;
using Xcc.Core.Constants;

namespace Xcc.Application.UI.UserControls
{
    public enum CameraType
    {
        Room,
        Head
    }

    /// <summary>
    /// Interaction logic for VideoLiveFeedControl
    /// </summary>
    public partial class VideoLiveFeedControl : UserControl
    {
        private bool _isInitializing = true;
        private bool _roomCameraLoaded = false;
        private bool _headCameraLoaded = false;
        private bool _isShuttingDown = false;

        #region Constructor
        public VideoLiveFeedControl()
        {
            InitializeComponent();
            
            // Register handlers for Room camera WebView2
            WebViewRoomCamera.NavigationCompleted += WebViewControl_NavigationCompleted;
            WebViewRoomCamera.CoreWebView2InitializationCompleted += WebViewControl_CoreWebView2InitializationCompleted;
            
            // Register handlers for Head camera WebView2
            WebViewHeadCamera.NavigationCompleted += WebViewControl_NavigationCompleted;
            WebViewHeadCamera.CoreWebView2InitializationCompleted += WebViewControl_CoreWebView2InitializationCompleted;
            
            UpdateButtonStates();  // Initialize button states at startup
        }
        #endregion Constructor


        #region Events Handlers
        private void WebViewControl_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            // Suppress this handler during shutdown (navigating to about:blank)
            if (_isShuttingDown)
                return;

            // Track which camera successfully loaded
            if (sender == WebViewRoomCamera)
            {
                _roomCameraLoaded = e.IsSuccess;
            }
            else if (sender == WebViewHeadCamera)
            {
                _headCameraLoaded = e.IsSuccess;
            }

            // During initialization, don't show error yet - let initialization complete
            if (_isInitializing)
                return;

            // After initialization, only show error if BOTH cameras failed
            if (!_roomCameraLoaded && !_headCameraLoaded)
            {
                ErrorPanel.Visibility = Visibility.Visible;
            }
            else
            {
                ErrorPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void WebViewControl_CoreWebView2InitializationCompleted(object sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            if (!e.IsSuccess)
                return;

            if (sender is not WebView2 webView)
                return;

            // Disable browser accelerator keys (fullscreen, refresh, F12, etc.)
            webView.CoreWebView2.Settings.AreBrowserAcceleratorKeysEnabled = false;
            
            // Disable context menu on right-click / long-press
            webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;

            // Handle microphone permission requests cleanly
            // Unsubscribe first to prevent duplicate subscriptions if core re-initializes
            webView.CoreWebView2.PermissionRequested -= OnPermissionRequested;
            webView.CoreWebView2.PermissionRequested += OnPermissionRequested;
        }

        private void OnPermissionRequested(object sender, CoreWebView2PermissionRequestedEventArgs args)
        {
            if (args.PermissionKind == CoreWebView2PermissionKind.Microphone)
            {
                args.State = CoreWebView2PermissionState.Allow;
                args.Handled = true;
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Clear shutdown flag - we're loading
            _isShuttingDown = false;
            
            // Initialize cameras (on first load) or re-navigate to them (on return after unload)
            // When returning after muting, cameras stay loaded in memory but muted
            UpdateButtonStates();
            InitializeAllCameras();
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            // Set shutdown flag to suppress NavigationCompleted handler
            _isShuttingDown = true;
            
            // Mute audio/microphone when leaving
            // Mute audio (camera to PC)
            IsAudioMuted = true;
            
            // Mute microphone (PC to camera)
            IsMicrophoneMuted = true;
            
            // Kill audio/microphone tracks via JavaScript
            string killAudioScript = @"(function() {
                try {
                    // Disable audio receiver track
                    if (window.audioReceiverTrack) {
                        window.audioReceiverTrack.enabled = false;
                    }
                    
                    // Disable microphone track
                    if (window.micTrack) {
                        window.micTrack.enabled = false;
                    }
                    
                    // Mute all media elements
                    document.querySelectorAll('audio, video').forEach(media => {
                        media.muted = true;
                    });
                } catch (err) { }
            })();";

            // Mute audio on Room camera
            if (WebViewRoomCamera?.CoreWebView2 != null)
            {
                try
                {
                    _ = WebViewRoomCamera.CoreWebView2.ExecuteScriptAsync(killAudioScript);
                }
                catch { /* Ignore errors */ }
            }

            // Mute audio on Head camera
            if (WebViewHeadCamera?.CoreWebView2 != null)
            {
                try
                {
                    _ = WebViewHeadCamera.CoreWebView2.ExecuteScriptAsync(killAudioScript);
                }
                catch { /* Ignore errors */ }
            }
        }

        /// <summary>
        /// Initializes camera feeds based on configuration
        /// LoadBothCameras=true (default): Load Room and Head in parallel (external system)
        /// LoadBothCameras=false: Load only the InitialCamera (internal system optimization)
        /// </summary>
        private async void InitializeAllCameras()
        {
            try
            {
                Task initTask;
                if (LoadBothCameras)
                {
                    // External systems: Load both cameras in parallel for technician flexibility
                    initTask = Task.WhenAll(StartRoomCameraAsync(), StartHeadCameraAsync());
                }
                else
                {
                    // Internal system: Load only the initial camera for efficiency
                    initTask = (InitialCamera == CameraType.Room) 
                        ? StartRoomCameraAsync() 
                        : StartHeadCameraAsync();
                }

                // Await the initialization task
                // SynchronizationContext automatically marshals back to UI thread
                await initTask;

                // Mark initialization complete and show the initial camera
                _isInitializing = false;
                ErrorPanel.Visibility = Visibility.Collapsed;
                SwitchCamera(InitialCamera);
                
                CurrentTask = new ObservableTask(Task.CompletedTask, StringConstants.Camera.CameraUnavailableUiErrorMessage);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Camera initialization error: {ex.Message}");
                _isInitializing = false;
                CurrentTask = new ObservableTask(Task.FromException(ex), StringConstants.Camera.CameraUnavailableUiErrorMessage);
            }
        }

        private async void ScreenshotButton_Click(object sender, RoutedEventArgs e)
        {
            // Screenshot functionality can be implemented later using WebView2 capture
            // For now, this is a placeholder
        }

        private void RoomCameraButton_Click(object sender, RoutedEventArgs e)
        {
            SwitchCamera(CameraType.Room);
        }

        private void HeadCameraButton_Click(object sender, RoutedEventArgs e)
        {
            SwitchCamera(CameraType.Head);
        }

        private async void ToggleAudioButton_Click(object sender, RoutedEventArgs e)
        {
            await ToggleAudio();
        }

        private async void ToggleMicrophoneButton_Click(object sender, RoutedEventArgs e)
        {
            await ToggleMicrophone();
        }
        #endregion Events Handlers


        #region Private methods

        private void SwitchCamera(CameraType cameraType)
        {
            CurrentCameraType = cameraType;
            UpdateButtonStates();
            
            // Toggle visibility between cameras (both are always loaded in memory)
            WebViewRoomCamera.Visibility = (cameraType == CameraType.Room) ? Visibility.Visible : Visibility.Collapsed;
            WebViewHeadCamera.Visibility = (cameraType == CameraType.Head) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void UpdateButtonStates()
        {
            IsRoomCameraButtonEnabled = CurrentCameraType != CameraType.Room;
            IsHeadCameraButtonEnabled = CurrentCameraType != CameraType.Head;
            IsRoomCameraActive = CurrentCameraType == CameraType.Room;
        }

        private void StartLiveFeed()
        {
            // Deprecated - no longer used in dual-camera architecture
            // Use InitializeAllCameras() instead
        }

        /// <summary>
        /// Initializes Room camera stream with microphone audio
        /// </summary>
        private async Task StartRoomCameraAsync()
        {
            try
            {
                // Configure WebView2 to allow insecure origins (required for http:// go2rtc URLs with microphone access)
                var options = new Microsoft.Web.WebView2.Core.CoreWebView2EnvironmentOptions();
                options.AdditionalBrowserArguments = "--unsafely-treat-insecure-origin-as-secure=http://172.31.1.222:1984";
                var environment = await Microsoft.Web.WebView2.Core.CoreWebView2Environment.CreateAsync(null, null, options);
                
                await WebViewRoomCamera.EnsureCoreWebView2Async(environment);

                // Inject script BEFORE navigation so it intercepts RTCPeerConnection track creation
                // This ensures microphone starts muted and we can control it later
                await WebViewRoomCamera.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(
                    @"(function() {
                        // Intercept addTransceiver to capture microphone track (go2rtc uses sendonly direction for mic)
                        const originalAddTransceiver = RTCPeerConnection.prototype.addTransceiver;
                        RTCPeerConnection.prototype.addTransceiver = function(trackOrKind, init) {
                            const result = originalAddTransceiver.apply(this, [trackOrKind, init]);
                            
                            // Capture MICROPHONE track (sendonly audio)
                            if (init && init.direction === 'sendonly' && result.sender && result.sender.track && result.sender.track.kind === 'audio') {
                                window.micTrack = result.sender.track;
                                // Start muted by default
                                window.micTrack.enabled = false;
                            }
                            
                            // Capture INCOMING AUDIO receiver (recvonly audio)
                            if (init && init.direction === 'recvonly' && result.receiver && result.receiver.track && result.receiver.track.kind === 'audio') {
                                window.audioReceiverTrack = result.receiver.track;
                                // Start with audio disabled by default
                                window.audioReceiverTrack.enabled = false;
                            }
                            
                            return result;
                        };
                    })();");

                WebViewRoomCamera.CoreWebView2.Navigate(CameraUrlRoom);
                
                // Inject JavaScript to hide media controls and scrollbars
                // Wait for go2rtc page to fully load and render
                await Task.Delay(1000);
                try
                {
                    await InjectWebViewScripts(WebViewRoomCamera);
                }
                catch { /* Ignore script execution errors */ }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Room camera initialization failed: {ex.Message}");
                // Don't throw - allow initialization to continue even if one camera fails
            }
        }

        /// <summary>
        /// Initializes Head camera stream (audio disabled for safety)
        /// </summary>
        private async Task StartHeadCameraAsync()
        {
            try
            {
                // Configure WebView2 to allow insecure origins (required for http:// go2rtc URLs with audio access)
                var options = new Microsoft.Web.WebView2.Core.CoreWebView2EnvironmentOptions();
                options.AdditionalBrowserArguments = "--unsafely-treat-insecure-origin-as-secure=http://172.31.1.222:1984";
                var environment = await Microsoft.Web.WebView2.Core.CoreWebView2Environment.CreateAsync(null, null, options);
                
                await WebViewHeadCamera.EnsureCoreWebView2Async(environment);

                // Inject script BEFORE navigation to disable audio on head camera for safety
                await WebViewHeadCamera.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(
                    @"(function() {
                        // Intercept RTCPeerConnection to disable all audio on head camera stream
                        const originalAddTransceiver = RTCPeerConnection.prototype.addTransceiver;
                        RTCPeerConnection.prototype.addTransceiver = function(trackOrKind, ...args) {
                            const result = originalAddTransceiver.apply(this, [trackOrKind, ...args]);
                            
                            // Disable all audio tracks (senders and receivers)
                            if (result.sender && result.sender.track && result.sender.track.kind === 'audio') {
                                result.sender.track.enabled = false;
                            }
                            if (result.receiver && result.receiver.track && result.receiver.track.kind === 'audio') {
                                result.receiver.track.enabled = false;
                            }
                            
                            return result;
                        };
                    })();");

                WebViewHeadCamera.CoreWebView2.Navigate(CameraUrlTreatmentHead);
                
                // Inject JavaScript to hide media controls and scrollbars
                // Wait for go2rtc page to fully load and render
                await Task.Delay(1000);
                try
                {
                    await InjectWebViewScripts(WebViewHeadCamera);
                }
                catch { /* Ignore script execution errors */ }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Head camera initialization failed: {ex.Message}");
                // Don't throw - allow initialization to continue even if one camera fails
            }
        }

        /// <summary>
        /// Injects global styles and hides media controls in WebView2
        /// </summary>
        private async Task InjectWebViewScripts(WebView2 webView)
        {
            await webView.CoreWebView2.ExecuteScriptAsync(
                $@"(function() {{
                    // Inject global styles
                    if (!document.getElementById('gentlebeam-webview-styles')) {{
                        const style = document.createElement('style');
                        style.id = 'gentlebeam-webview-styles';
                        style.textContent = `
                            html, body {{
                                overflow: hidden !important;
                            }}
                            ::-webkit-scrollbar {{
                                display: none !important;
                            }}
                        `;
                        document.head.appendChild(style);
                    }}

                    // Hide video controls and set initial audio state
                    const videos = document.querySelectorAll('video');
                    videos.forEach(video => {{
                        video.controls = false;
                        video.style.pointerEvents = 'none';
                        // Set initial muted state based on IsAudioMuted property
                        video.muted = {IsAudioMuted.ToString().ToLower()};
                    }});
                }})();");
        }

        /// <summary>
        /// Disables all audio on head camera stream for safety
        /// </summary>
        private async Task DisableHeadCameraAudio()
        {
            await WebViewHeadCamera.CoreWebView2.ExecuteScriptAsync(
                @"(function() {
                    // Disable all audio tracks on head camera stream
                    // 1. Primary: RTCRtpSender from active WebRTC connection
                    if (window.pc) {
                        const senders = window.pc.getSenders();
                        senders.forEach(sender => {
                            if (sender.track && sender.track.kind === 'audio') {
                                sender.track.enabled = false;
                            }
                        });
                    }

                    // 2. Fallback: go2rtc internal stream references
                    if (window.stream) {
                        window.stream.getAudioTracks().forEach(t => t.enabled = false);
                    }
                    if (window.localStream) {
                        window.localStream.getAudioTracks().forEach(t => t.enabled = false);
                    }
                })();");
        }



        /// <summary>
        /// Mutes the microphone via JavaScript on page load.
        /// Ensures PC microphone is disabled by default.
        /// </summary>
        private async Task ApplyMicrophoneMuteState()
        {
            try
            {
                IsMicrophoneMuted = true;

                await WebViewRoomCamera.CoreWebView2.ExecuteScriptAsync(
                    @"(function() {
                        // Wait for WebRTC objects to be initialized before muting
                        const waitForPeerConnection = async () => {
                            let attempts = 0;
                            while (!window.pc && attempts < 50) {
                                await new Promise(resolve => setTimeout(resolve, 100));
                                attempts++;
                            }
                            return window.pc !== undefined;
                        };

                        // Execute muting after waiting for peer connection
                        waitForPeerConnection().then(() => {
                            if (window.pc && window.pc.getSenders) {
                                try {
                                    const senders = window.pc.getSenders();
                                    senders.forEach(sender => {
                                        if (sender.track && sender.track.kind === 'audio') {
                                            sender.track.enabled = false;
                                        }
                                    });
                                } catch(e) { console.error('Error muting microphone:', e); }
                            }
                        });
                    })();");
            }
            catch { /* Ignore script execution errors */ }
        }

        private async Task<bool> StopLiveFeed()
        {
            return true;
        }

        private async Task<bool> CloseLiveFeed()
        {
            return await StopLiveFeed();
        }

        private async Task ToggleAudio()
        {
            // Use property to disable while toggling (doesn't break binding like direct assignment)
            IsAudioToggling = true;
            try
            {
                // Toggle audio by enabling/disabling the RECEIVED audio track via JavaScript
                IsAudioMuted = !IsAudioMuted;
                await WebViewRoomCamera.CoreWebView2.ExecuteScriptAsync(
                    $@"(function() {{
                        const shouldPlayAudio = {(!IsAudioMuted).ToString().ToLower()};

                        // 1. Control all media elements (video/audio tags) - set muted AND play if enabling audio
                        const mediaElements = document.querySelectorAll('audio, video');
                        mediaElements.forEach(el => {{
                            el.muted = !shouldPlayAudio;
                            if (shouldPlayAudio) {{
                                // Try to play audio
                                el.play().catch(e => console.error('Audio play error:', e));
                            }}
                        }});

                        // 2. Toggle the stored incoming audio receiver track
                        if (window.audioReceiverTrack) {{
                            window.audioReceiverTrack.enabled = shouldPlayAudio;
                        }}
                        // 3. Fallback: Toggle via WebRTC receivers
                        else if (window.pc && window.pc.getReceivers) {{
                            try {{
                                const receivers = window.pc.getReceivers();
                                receivers.forEach(receiver => {{
                                    if (receiver.track && receiver.track.kind === 'audio') {{
                                        receiver.track.enabled = shouldPlayAudio;
                                    }}
                                }});
                            }} catch(e) {{ console.error('Error toggling audio via receivers:', e); }}
                        }}
                    }})();");
            }
            catch { /* Ignore script execution errors */ }
            finally
            {
                IsAudioToggling = false;
            }
        }

        private async Task ToggleMicrophone()
        {
            // Use property to disable while toggling (doesn't break binding like direct assignment)
            IsMicrophoneToggling = true;
            try
            {
                // Toggle local microphone input (mute/unmute PC microphone)
                IsMicrophoneMuted = !IsMicrophoneMuted;
                
                await WebViewRoomCamera.CoreWebView2.ExecuteScriptAsync(
                    $@"(function() {{
                        const shouldEnable = {(!IsMicrophoneMuted).ToString().ToLower()};

                        // 1. Primary: Use stored microphone track reference (intercepted at creation time)
                        if (window.micTrack) {{
                            window.micTrack.enabled = shouldEnable;
                        }}
                        // 2. Fallback: Iterate over active senders from peer connection
                        else if (window.pc && window.pc.getSenders) {{
                            try {{
                                const senders = window.pc.getSenders();
                                senders.forEach(sender => {{
                                    if (sender.track && sender.track.kind === 'audio') {{
                                        sender.track.enabled = shouldEnable;
                                    }}
                                }});
                            }} catch(e) {{ console.error('Error accessing PC senders:', e); }}
                        }}
                    }})();");
            }
            catch { /* Ignore script execution errors */ }
            finally
            {
                IsMicrophoneToggling = false;
            }
        }
        #endregion Private methods


        #region Dependency Properties

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

        public string CameraUrlRoom { get => (string)GetValue(CameraUrlRoomProperty); set => SetValue(CameraUrlRoomProperty, value); }

        public static readonly DependencyProperty CameraUrlRoomProperty =
            DependencyProperty.Register(
                nameof(CameraUrlRoom),
                typeof(string),
                typeof(VideoLiveFeedControl),
                new PropertyMetadata("http://172.31.1.222:1984/webrtc.html?src=room&media=video+audio+microphone"));

        public string CameraUrlTreatmentHead { get => (string)GetValue(CameraUrlTreatmentHeadProperty); set => SetValue(CameraUrlTreatmentHeadProperty, value); }

        public static readonly DependencyProperty CameraUrlTreatmentHeadProperty =
            DependencyProperty.Register(
                nameof(CameraUrlTreatmentHead),
                typeof(string),
                typeof(VideoLiveFeedControl),
                new PropertyMetadata("http://172.31.1.222:1984/webrtc.html?src=head&media=video+audio"));

        public CameraType InitialCamera { get => (CameraType)GetValue(InitialCameraProperty); set => SetValue(InitialCameraProperty, value); }

        public static readonly DependencyProperty InitialCameraProperty =
            DependencyProperty.Register(
                nameof(InitialCamera),
                typeof(CameraType),
                typeof(VideoLiveFeedControl),
                new PropertyMetadata(CameraType.Room));

        public bool LoadBothCameras { get => (bool)GetValue(LoadBothCamerasProperty); set => SetValue(LoadBothCamerasProperty, value); }

        public static readonly DependencyProperty LoadBothCamerasProperty =
            DependencyProperty.Register(
                nameof(LoadBothCameras),
                typeof(bool),
                typeof(VideoLiveFeedControl),
                new PropertyMetadata(true));

        public CameraType CurrentCameraType { get; set; }

        public bool IsRoomCameraButtonEnabled { get => (bool)GetValue(IsRoomCameraButtonEnabledProperty); set => SetValue(IsRoomCameraButtonEnabledProperty, value); }

        public static readonly DependencyProperty IsRoomCameraButtonEnabledProperty =
            DependencyProperty.Register(
                nameof(IsRoomCameraButtonEnabled),
                typeof(bool),
                typeof(VideoLiveFeedControl),
                new PropertyMetadata(true));

        public bool IsHeadCameraButtonEnabled { get => (bool)GetValue(IsHeadCameraButtonEnabledProperty); set => SetValue(IsHeadCameraButtonEnabledProperty, value); }

        public static readonly DependencyProperty IsHeadCameraButtonEnabledProperty =
            DependencyProperty.Register(
                nameof(IsHeadCameraButtonEnabled),
                typeof(bool),
                typeof(VideoLiveFeedControl),
                new PropertyMetadata(true));

        public bool IsRoomCameraActive { get => (bool)GetValue(IsRoomCameraActiveProperty); set => SetValue(IsRoomCameraActiveProperty, value); }

        public static readonly DependencyProperty IsRoomCameraActiveProperty =
            DependencyProperty.Register(
                nameof(IsRoomCameraActive),
                typeof(bool),
                typeof(VideoLiveFeedControl),
                new PropertyMetadata(true));

        public bool IsAudioToggling { get => (bool)GetValue(IsAudioTogglingProperty); set => SetValue(IsAudioTogglingProperty, value); }

        public static readonly DependencyProperty IsAudioTogglingProperty =
            DependencyProperty.Register(
                nameof(IsAudioToggling),
                typeof(bool),
                typeof(VideoLiveFeedControl),
                new PropertyMetadata(false));

        public bool IsMicrophoneToggling { get => (bool)GetValue(IsMicrophoneTogglingProperty); set => SetValue(IsMicrophoneTogglingProperty, value); }

        public static readonly DependencyProperty IsMicrophoneTogglingProperty =
            DependencyProperty.Register(
                nameof(IsMicrophoneToggling),
                typeof(bool),
                typeof(VideoLiveFeedControl),
                new PropertyMetadata(false));

        public Visibility RoomCameraButtonVisibility { get => (Visibility)GetValue(RoomCameraButtonVisibilityProperty); set => SetValue(RoomCameraButtonVisibilityProperty, value); }

        public static readonly DependencyProperty RoomCameraButtonVisibilityProperty =
            DependencyProperty.Register(
                nameof(RoomCameraButtonVisibility),
                typeof(Visibility),
                typeof(VideoLiveFeedControl),
                new PropertyMetadata(Visibility.Visible));

        public Visibility HeadCameraButtonVisibility { get => (Visibility)GetValue(HeadCameraButtonVisibilityProperty); set => SetValue(HeadCameraButtonVisibilityProperty, value); }

        public static readonly DependencyProperty HeadCameraButtonVisibilityProperty =
            DependencyProperty.Register(
                nameof(HeadCameraButtonVisibility),
                typeof(Visibility),
                typeof(VideoLiveFeedControl),
                new PropertyMetadata(Visibility.Visible));

        public bool IsAudioMuted { get => (bool)GetValue(IsAudioMutedProperty); set => SetValue(IsAudioMutedProperty, value); }

        public static readonly DependencyProperty IsAudioMutedProperty =
            DependencyProperty.Register(
                nameof(IsAudioMuted),
                typeof(bool),
                typeof(VideoLiveFeedControl),
                new PropertyMetadata(true));

        public bool IsMicrophoneMuted { get => (bool)GetValue(IsMicrophoneMutedProperty); set => SetValue(IsMicrophoneMutedProperty, value); }

        public static readonly DependencyProperty IsMicrophoneMutedProperty =
            DependencyProperty.Register(
                nameof(IsMicrophoneMuted),
                typeof(bool),
                typeof(VideoLiveFeedControl),
                new PropertyMetadata(true));
        #endregion Dependency Properties

    }

    /// <summary>
    /// Converter: bool to mute status text (Muted/Unmuted)
    /// </summary>
    public class BoolToMuteStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isMuted)
            {
                return isMuted ? "Muted" : "Unmuted";
            }
            return "Unknown";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converter: camera type to visibility (show only for Room camera)
    /// </summary>
    public class CameraTypeToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is CameraType cameraType && parameter is string cameraParam)
            {
                return (cameraType.ToString() == cameraParam) ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converter: bool to speaker symbol (🔊 or 🔇)
    /// </summary>
    public class BoolToSpeakerSymbolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isMuted)
            {
                // 🔇 = muted (U+1F507), 🔊 = unmuted (U+1F50A)
                return isMuted ? "🔇" : "🔊";
            }
            return "🔊";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converter: bool to microphone symbol (🚫 when muted, 🎙️ when unmuted)
    /// </summary>
    public class BoolToMicSymbolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isMuted)
            {
                // 🚫 = prohibited (muted), 🎙️ = microphone (unmuted)
                return isMuted ? "🎙️🚫" : "🎙️";
            }
            return "🚫";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converter: inverts a boolean value for reverse bindings
    /// </summary>
    public class NegateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }
            return false;
        }
    }

    /// <summary>
    /// Converter: combines multiple booleans with AND logic (all must be true)
    /// </summary>
    public class CombineBooleansConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            foreach (object value in values)
            {
                if (!(value is bool boolValue && boolValue))
                {
                    return false;
                }
            }
            return true;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
