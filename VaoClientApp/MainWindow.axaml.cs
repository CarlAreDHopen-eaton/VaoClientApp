using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using LibVLCSharp.Avalonia;
using LibVLCSharp.Shared;
using Vao.Client;
using Vao.Client.Components;
using Vao.Client.Enum;

namespace Vao.Sample
{
   public partial class MainWindow : Window
   {
      private bool mIsStarted = false;
      private bool mIsCameraSelected = false;
      private bool mIsPlaybackStarted = false;
      private bool mApiSupportsPlayback = false;

      private FlexRApiClient moFlexRApiClient;
      private VideoView mVideoControl;
      private Guid mViewerID = Guid.NewGuid();
      private Camera mCurrentCamera;
      private Alarm mCurrentAlarm;
      private User mCurrentLoggedInUser;
      private Button mCurrentCameraButton;
      private LibVLC mLibVlc;
      private MediaPlayer mMediaPlayer;
      private bool mIsVideoStarted = false;
      private bool mIsLoadingSettings;

      private ObservableCollection<MessageItem> mMessages = new();

      public FlexRApiClient FlexRApiClient => moFlexRApiClient;

      public bool IsStarted
      {
         get => mIsStarted;
         set { mIsStarted = value; UpdateEnabled(); }
      }

      public bool ApiSupportsPlayback
      {
         get => mApiSupportsPlayback;
         set { mApiSupportsPlayback = value; UpdateEnabled(); }
      }

      public bool IsPlaybackStarted
      {
         get => mIsPlaybackStarted;
         set { mIsPlaybackStarted = value; UpdateEnabled(); }
      }

      public bool IsPlayback => IsUriPlaybackUri(txtCurrentRtspUrl.Text);

      public bool IsCameraSelected
      {
         get => mIsCameraSelected;
         set { mIsCameraSelected = value; UpdateEnabled(); }
      }

      private static bool IsUriPlaybackUri(string text)
      {
         return text != null && text.Contains("playback");
      }


      public MainWindow()
      {
         InitializeComponent();

         lstMessages.ItemsSource = mMessages;

         StartInitializeVlc();
         ClearPresetDropdown();
         ClearRecordingDropdown();

         mIsLoadingSettings = true;
         LoadSettings();
         mIsLoadingSettings = false;

         UpdateEnabled();
         ApplyIcons();

         Opened += MainWindow_Opened;
      }

      private void MainWindow_Opened(object sender, EventArgs e)
      {
         Opened -= MainWindow_Opened;

         var s = AppSettings.Default;
         expConnection.IsExpanded = s.IsConnectionExpanded;
         expCameraControl.IsExpanded = s.IsCameraControlExpanded;
         expCameraSelection.IsExpanded = s.IsCameraSelectionExpanded;
         expPresetSelection.IsExpanded = s.IsPresetSelectionExpanded;
         expAlarms.IsExpanded = s.IsAlarmsExpanded;
         expPlaybackSelection.IsExpanded = s.IsPlaybackSelectionExpanded;
         expDownloadRecording.IsExpanded = s.IsDownloadRecordingExpanded;
         expSettings.IsExpanded = s.IsSettingsExpanded;
      }

      private static readonly string ResBase = "avares://VaoClientApp/Resources/";

      private bool IsDarkMode => AppSettings.Default.IsDarkMode;

      private void ApplyIcons()
      {
         bool invert = IsDarkMode;
         imgFocusNear.Source = IconHelper.Load(ResBase + "flip_to_front_black_24dp.png", invert);
         imgTiltUp.Source = IconHelper.Load(ResBase + "arrow_upward_black_24dp.png", invert);
         imgZoomIn.Source = IconHelper.Load(ResBase + "zoom_in_black_24dp.png", invert);
         imgPanLeft.Source = IconHelper.Load(ResBase + "arrow_back_black_24dp.png", invert);
         imgCenterCamera.Source = IconHelper.Load(ResBase + "control_camera_black_24dp.png", invert);
         imgPanRight.Source = IconHelper.Load(ResBase + "arrow_forward_black_24dp.png", invert);
         imgFocusFar.Source = IconHelper.Load(ResBase + "flip_to_back_black_24dp.png", invert);
         imgTiltDown.Source = IconHelper.Load(ResBase + "arrow_downward_black_24dp.png", invert);
         imgZoomOut.Source = IconHelper.Load(ResBase + "zoom_out_black_24dp.png", invert);
         imgAbsolutePosition.Source = IconHelper.Load(ResBase + "absoluteposition_black_24dp.png", invert);
         imgCameraLock.Source = IconHelper.Load(ResBase + "cameraunlocked_black_24dp.png", invert);
      }

      private void chkDarkMode_Changed(object sender, RoutedEventArgs e)
      {
         if (mIsLoadingSettings) return;
         bool isDark = chkDarkMode.IsChecked == true;
         ((App)Application.Current).SetTheme(isDark);
         ApplyIcons();
         SaveSettings();
      }

      private void StartInitializeVlc()
      {
         WriteMessageLog(MessageSource.LibVlc, "Loading VLC", LogLevel.Notice);
         var options = new[] { "-vv", "--rtsp-timeout=300", "--network-caching=300" };
         mLibVlc = new LibVLC(true, options);
         mLibVlc.Log += LibVlc_Log;
      }

      private void LibVlc_Log(object sender, LogEventArgs e)
      {
         if (e.Level == LogLevel.Debug) return;
         if (e.Message.ToLower() == "unsupported control query 3") return;
         WriteMessageLog(MessageSource.LibVlc, e.Message, e.Level);
      }

      private void LoadSettings()
      {
         var s = AppSettings.Default;
         if (!string.IsNullOrEmpty(s.Host1)) txtHost.Text = s.Host1;
         if (!string.IsNullOrEmpty(s.User)) txtUser.Text = s.User;
         if (!string.IsNullOrEmpty(s.ApiPort)) txtPort.Text = s.ApiPort;
         if (!string.IsNullOrEmpty(s.Password)) txtPassword.Text = s.Password;
         chkUseTcp.IsChecked = s.UseTcp;
         chkPreferSubChannel.IsChecked = s.PreferSubChannel;
         chkSecure.IsChecked = s.UseHttps;
         chkDarkMode.IsChecked = s.IsDarkMode;
      }

      private void SaveSettings()
      {
         var s = AppSettings.Default;
         s.Host1 = txtHost.Text ?? "";
         s.User = txtUser.Text ?? "";
         s.ApiPort = txtPort.Text ?? "";
         s.Password = txtPassword.Text ?? "";
         if (mCurrentCamera != null) s.CurrentCamera = mCurrentCamera.ComponentNumber;
         s.UseTcp = chkUseTcp.IsChecked == true;
         s.PreferSubChannel = chkPreferSubChannel.IsChecked == true;
         s.UseHttps = chkSecure.IsChecked == true;
         s.IsConnectionExpanded = expConnection.IsExpanded;
         s.IsCameraControlExpanded = expCameraControl.IsExpanded;
         s.IsCameraSelectionExpanded = expCameraSelection.IsExpanded;
         s.IsPresetSelectionExpanded = expPresetSelection.IsExpanded;
         s.IsAlarmsExpanded = expAlarms.IsExpanded;
         s.IsPlaybackSelectionExpanded = expPlaybackSelection.IsExpanded;
         s.IsDownloadRecordingExpanded = expDownloadRecording.IsExpanded;
         s.IsSettingsExpanded = expSettings.IsExpanded;
         s.Save();
      }

      protected override void OnClosing(WindowClosingEventArgs e)
      {
         SaveSettings();
         base.OnClosing(e);
      }

      public void UpdateEnabled()
      {
         btnDisconnect.IsEnabled = IsStarted;
         pnlCameraSelectFlowPanel.IsEnabled = IsStarted;
         chkPreferSubChannel.IsEnabled = IsStarted && !IsPlayback;
         grpSelectPreset.IsEnabled = IsStarted;
         grpSelectPlayback.IsEnabled = IsStarted && ApiSupportsPlayback;
         grpCameraControl.IsEnabled = IsStarted && mCurrentCamera != null;

         btnStopPlayback.IsEnabled = IsStarted && IsPlayback && ApiSupportsPlayback;
         btnPlayPlayback.IsEnabled = IsStarted && IsCameraSelected && !IsPlaybackStarted && ApiSupportsPlayback;
         btnGotoTime.IsEnabled = IsStarted && IsCameraSelected && ApiSupportsPlayback;
         btnConnect.IsEnabled = !IsStarted;
         txtHost.IsEnabled = !IsStarted;
         txtPassword.IsEnabled = !IsStarted;
         txtPort.IsEnabled = !IsStarted;
         txtUser.IsEnabled = !IsStarted;
         chkSecure.IsEnabled = !IsStarted;
         btnDownload.IsEnabled = IsStarted && ApiSupportsPlayback;

         UpdateCameraControl();
      }

      private void btnConnect_Click(object sender, RoutedEventArgs e)
      {
         SaveSettings();
         if (!ValidateCanConnect()) return;

         IsStarted = true;
         moFlexRApiClient = new FlexRApiClient
         {
            Host = txtHost.Text,
            Port = txtPort.Text,
            Password = txtPassword.Text,
            User = txtUser.Text,
            UseHttps = chkSecure.IsChecked == true,
            IgnoreCertificateErrors = true
         };
         moFlexRApiClient.OnMessage += OnFlexRApiClientMessage;
         txtVideoHeader.Text = "No Camera Selected";
         brdVideoHeader.Background = new SolidColorBrush(Color.FromRgb(66, 77, 95));

         if (moFlexRApiClient.StartClient())
         {
            WriteMessageLog(MessageSource.FlexApi, "Client started.", LogLevel.Notice);
            SetCurrentLoggedInUser();
            FillSelectCameraButtonList();
            FillSelectAlarmButtonList();
            CheckApiVersion();
            ClearRecordingDropdown();
            ClearPresetDropdown();
            UpdateEnabled();

            if (AppSettings.Default.CurrentCamera != 0)
               SelectCamera(AppSettings.Default.CurrentCamera, chkPreferSubChannel.IsChecked == true ? 2 : 1);
         }
         else
         {
            WriteMessageLog(MessageSource.FlexApi, "Unable to start, no response.", LogLevel.Error);
            btnDisconnect_Click(sender, e);
            ClearPresetDropdown();
            UpdateEnabled();
         }
      }

      private bool ValidateCanConnect()
      {
         if (string.IsNullOrWhiteSpace(txtHost.Text))
         { WriteMessageLog(MessageSource.Config, "Missing host name", LogLevel.Error); return false; }
         if (string.IsNullOrWhiteSpace(txtPassword.Text))
         { WriteMessageLog(MessageSource.Config, "Missing host password", LogLevel.Error); return false; }
         if (string.IsNullOrWhiteSpace(txtUser.Text))
         { WriteMessageLog(MessageSource.Config, "Missing user name", LogLevel.Error); return false; }
         if (string.IsNullOrWhiteSpace(txtPort.Text))
         { WriteMessageLog(MessageSource.Config, "Missing port", LogLevel.Error); return false; }
         return true;
      }

      private void OnFlexRApiClientMessage(object sender, MessageEventArgs e)
      {
         if (e.StatusMessage is StatusMessage statusMessage)
         {
            string logText = $"{statusMessage.Timestamp} : [{statusMessage.Type}] {statusMessage.Message}";
            switch (statusMessage.Level)
            {
               case MessageLevel.Debug: WriteMessageLog(MessageSource.FlexApi, logText, LogLevel.Debug); break;
               case MessageLevel.Info: WriteMessageLog(MessageSource.FlexApi, logText, LogLevel.Notice); break;
               case MessageLevel.Warning: WriteMessageLog(MessageSource.FlexApi, logText, LogLevel.Warning); break;
               case MessageLevel.Error: WriteMessageLog(MessageSource.FlexApi, logText, LogLevel.Error); break;
            }
         }
      }

      private enum MessageSource { FlexApi, LibVlc, Config }

      private void WriteMessageLog(MessageSource source, string strMessage, LogLevel level)
      {
         if (!Dispatcher.UIThread.CheckAccess())
         {
            Dispatcher.UIThread.Post(() => WriteMessageLog(source, strMessage, level));
            return;
         }

         string strSource = source.ToString().PadRight(7);
         var strTime = DateTime.Now.ToString(CultureInfo.InvariantCulture);
         var strLevel = level.ToString().PadRight(7);
         var strMsg = $"{strTime} [{strLevel}][{strSource}] - {strMessage}";

         IBrush color = level switch
         {
            LogLevel.Error => Brushes.Red,
            LogLevel.Warning => new SolidColorBrush(IsDarkMode ? Colors.Orange : Color.FromRgb(200, 120, 0)),
            LogLevel.Debug => new SolidColorBrush(IsDarkMode ? Colors.LightBlue : Color.FromRgb(0, 100, 180)),
            _ => new SolidColorBrush(IsDarkMode ? Colors.White : Colors.Black),
         };

         if (strMessage != "drawable Warning: unsupported control query 3")
         {
            mMessages.Add(new MessageItem { Text = strMsg, Color = color });
            lstMessages.ScrollIntoView(mMessages.Count - 1);
         }
      }

      private void btnDisconnect_Click(object sender, RoutedEventArgs e)
      {
         IsStarted = false;
         if (moFlexRApiClient != null)
         {
            moFlexRApiClient.OnMessage -= OnFlexRApiClientMessage;
            moFlexRApiClient.StopClient();
            moFlexRApiClient = null;
         }
         StopRtspStream();
         CurrentCamera = null;
         CurrentAlarm = null;
         txtCurrentRtspUrl.Text = string.Empty;
         txtVideoHeader.Text = "No Camera Selected";
         brdVideoHeader.Background = new SolidColorBrush(Color.FromRgb(66, 77, 95));
         IsCameraSelected = false;
         ClearPresetDropdown();
         ClearRecordingDropdown();
         ClearCameraSelection();
         ClearAlarmSelection();
      }

      private void StopRtspStream()
      {
         if (mMediaPlayer != null)
         {
            var mp = mMediaPlayer;
            mMediaPlayer = null;
            if (mVideoControl != null)
            {
               mVideoControl.MediaPlayer = null;
               pnlVideo.Children.Remove(mVideoControl);
               mVideoControl = null;
            }
            mIsVideoStarted = false;
            DisposeMediaPlayerAsync(mp);
         }
      }

      private static void DisposeMediaPlayerAsync(MediaPlayer mp)
      {
         if (mp == null) return;
         Task.Run(() =>
         {
            if (mp.IsPlaying) mp.Stop();
            mp.Dispose();
         });
      }

      private Camera CurrentCamera
      {
         get => mCurrentCamera;
         set
         {
            if (mCurrentCameraButton != null)
               mCurrentCameraButton.Background = null;

            if (mCurrentCamera != null)
            {
               mCurrentCamera.PropertyChanged -= Camera_PropertyChanged;
               mCurrentCamera.LockStatusChanged -= Camera_LockStatusChanged;
            }

            mCurrentCamera = value;

            if (mCurrentCamera != null)
            {
               mCurrentCamera.PropertyChanged += Camera_PropertyChanged;
               mCurrentCamera.LockStatusChanged += Camera_LockStatusChanged;
            }

            mCurrentCameraButton = GetCameraButton(mCurrentCamera);
            if (mCurrentCameraButton != null)
               mCurrentCameraButton.Background = Brushes.Goldenrod;

            if (mCurrentCamera != null)
            {
               lblCurrentCamera.Text = $"Camera : {mCurrentCamera.Name}";
               IsCameraSelected = true;
               FillSelectPresetList();
               if (ApiSupportsPlayback)
                  FillPlaybackSelectionList();
               AppSettings.Default.CurrentCamera = mCurrentCamera.ComponentNumber;
            }
            else
            {
               ClearPresetDropdown();
               ClearRecordingDropdown();
            }
            UpdateEnabled();
         }
      }

      private Alarm CurrentAlarm { get => mCurrentAlarm; set { mCurrentAlarm = value; UpdateEnabled(); } }
      private User CurrentLoggedInUser { get => mCurrentLoggedInUser; set { mCurrentLoggedInUser = value; UpdateEnabled(); } }

      private void ClearRecordingDropdown()
      {
         selPlayback.ItemsSource = null;
         var items = new List<object>();
         items.Add(!ApiSupportsPlayback && IsStarted ? "Api Version does not support playback" : "No camera selected");
         selPlayback.ItemsSource = items;
         selPlayback.SelectedIndex = 0;
      }

      private void ClearPresetDropdown()
      {
         lblCurrentCamera.Text = "Camera : (No camera selected)";
         selPreset.ItemsSource = null;
         selPreset.ItemsSource = new List<string> { "No camera selected" };
         selPreset.SelectedIndex = 0;
      }

      private void ClearCameraSelection() { pnlCameraSelectFlowPanel.Children.Clear(); }
      private void ClearAlarmSelection() { pnlAlarms.Children.Clear(); }

      private void Camera_PropertyChanged(object sender, PropertyChangedEventArgs e)
      {
         if (e.PropertyName == nameof(Camera.IsLocked) || e.PropertyName == nameof(Camera.LockOwner) || e.PropertyName == nameof(Camera.CanUnlock))
            return;
         Dispatcher.UIThread.Post(UpdateCameraControl);
      }

      private void Camera_LockStatusChanged(object sender, EventArgs e)
      {
         Dispatcher.UIThread.Post(() =>
         {
            bool invert = IsDarkMode;
            if (mCurrentCamera != null && mCurrentCamera.IsLocked && mCurrentCamera.LockOwner == "Alarm")
            {
               imgCameraLock.Source = IconHelper.Load(ResBase + "cameralocked_red_24dp.png", invert);
               btnCameraLock.IsEnabled = true;
            }
            else if (mCurrentCamera != null && mCurrentCamera.IsLocked)
            {
               imgCameraLock.Source = IconHelper.Load(ResBase + "cameralocked_yellow_24dp.png", invert);
               btnCameraLock.IsEnabled = true;
            }
            else if (mCurrentCamera != null && !mCurrentCamera.IsLocked)
            {
               imgCameraLock.Source = IconHelper.Load(ResBase + "cameraunlocked_black_24dp.png", invert);
               btnCameraLock.IsEnabled = true;
            }

            if (mCurrentCamera != null && mCurrentCamera.CanUnlock == false)
            {
               btnCameraLock.IsEnabled = false;
            }
         });
      }

      private void UpdateCameraControl()
      {
         btnPanLeft.IsEnabled = mCurrentCamera?.HasPanTiltControl ?? false;
         btnPanRight.IsEnabled = mCurrentCamera?.HasPanTiltControl ?? false;
         btnTiltDown.IsEnabled = mCurrentCamera?.HasPanTiltControl ?? false;
         btnTiltUp.IsEnabled = mCurrentCamera?.HasPanTiltControl ?? false;
         btnZoomIn.IsEnabled = mCurrentCamera?.HasLensControl ?? false;
         btnZoomOut.IsEnabled = mCurrentCamera?.HasLensControl ?? false;
         btnFocusFar.IsEnabled = mCurrentCamera?.HasLensControl ?? false;
         btnFocusNear.IsEnabled = mCurrentCamera?.HasLensControl ?? false;
      }

      private void FillSelectPresetList()
      {
         List<Preset> presets = mCurrentCamera?.PresetList;
         if (presets != null && presets.Count > 0)
         {
            selPreset.ItemsSource = presets;
            selPreset.SelectedIndex = presets.Count > 1 ? 1 : 0;
         }
         else
         {
            selPreset.ItemsSource = new List<string> { "No Presets" };
            selPreset.SelectedIndex = 0;
         }
      }

      private void FillPlaybackSelectionList()
      {
         List<PlaybackInfo> playbackInfoList = mCurrentCamera?.GetPlaybackInfoList(mViewerID);
         if (playbackInfoList != null && playbackInfoList.Count > 0)
         {
            selPlayback.ItemsSource = playbackInfoList;
         }
         else
         {
            selPlayback.ItemsSource = new List<string> { "No Recorders" };
            selPlayback.SelectedIndex = 0;
         }
      }

      private Button GetCameraButton(Camera camera)
      {
         foreach (var child in pnlCameraSelectFlowPanel.Children)
         {
            if (child is Button button && button.Tag == camera)
               return button;
         }
         return null;
      }

      private void SelectCamera(int cameraNo, int streamNo)
      {
         Camera camera = moFlexRApiClient.GetCamera(cameraNo);
         if (camera != null)
         {
            CurrentCamera = camera;
            string url = camera.GetCameraLiveStreamUrl(streamNo);
            if (!string.IsNullOrEmpty(url))
            {
               txtCurrentRtspUrl.Text = GetMaskedUrl(url);
               txtVideoHeader.Text = $"LIVE - Camera {cameraNo}";
               brdVideoHeader.Background = new SolidColorBrush(Color.FromRgb(65, 142, 62));
               StartRtspStream(url);
            }
         }
      }

      private void SelectAlarm(int alarmNo)
      {
         Alarm alarm = moFlexRApiClient.GetSingleAlarm(alarmNo);
         if (alarm != null) CurrentAlarm = alarm;
      }

      private void CheckApiVersion()
      {
         ApiVersion apiversion = moFlexRApiClient.GetApiVersion();
         if (apiversion != null)
         {
            Version version = new Version(apiversion.MajorVersion, apiversion.MinorVersion);
            if (version >= new Version(1, 1))
               ApiSupportsPlayback = true;
         }
         UpdateEnabled();
      }

      private static string GetMaskedUrl(string url)
      {
         var maskedUri = new UriBuilder(url) { Password = "******", UserName = "******" };
         return maskedUri.ToString();
      }

      private void StartRtspStream(string rtspUrl)
      {
         if (mIsVideoStarted) StopRtspStream();

         InitVideoControl();
         var uri = new Uri(rtspUrl);
         if (mMediaPlayer == null)
         {
            var media = new Media(mLibVlc, uri);
            mMediaPlayer = new MediaPlayer(media);
            mMediaPlayer.EncounteredError += MediaPlayer_EncounteredError;
            mMediaPlayer.Opening += MediaPlayer_Opening;
            if (chkUseTcp.IsChecked == true) media.AddOption(":rtsp-tcp");
            if (mVideoControl != null) mVideoControl.MediaPlayer = mMediaPlayer;
            mMediaPlayer.Play();
            mIsVideoStarted = true;
         }
         else
         {
            var media = new Media(mLibVlc, uri);
            if (chkUseTcp.IsChecked == true) media.AddOption(":rtsp-tcp");
            mMediaPlayer.Play(media);
            mIsVideoStarted = true;
         }
      }

      private void InitVideoControl()
      {
         if (mVideoControl == null)
         {
            mVideoControl = new VideoView();
            pnlVideo.Children.Add(mVideoControl);
         }
      }

      private void MediaPlayer_EncounteredError(object sender, EventArgs e)
      {
         WriteMessageLog(MessageSource.LibVlc, "LibVLC error encountered.", LogLevel.Error);
      }

      private void MediaPlayer_Opening(object sender, EventArgs e)
      {
         string mrl = mMediaPlayer?.Media?.Mrl ?? "";
         WriteMessageLog(MessageSource.LibVlc, $"LibVLC opening {GetMaskedUrl(mrl)}", LogLevel.Notice);
      }

      private void OnSelectCameraClicked(object sender, RoutedEventArgs e)
      {
         if (sender is Button button && button.Tag is Camera camera)
            SelectCamera(camera.ComponentNumber, chkPreferSubChannel.IsChecked == true ? 2 : 1);
      }

      private void OnSelectAlarmClicked(object sender, RoutedEventArgs e)
      {
         if (sender is Button button && button.Tag is Alarm alarm)
         {
            SelectAlarm(alarm.ComponentNumber);
            var window = new AlarmActionWindow(FlexRApiClient, CurrentAlarm, mCurrentLoggedInUser);
            window.ShowDialog(this);
         }
      }

      private void FillSelectCameraButtonList()
      {
         ClearCameraSelection();
         List<Camera> cameraList = moFlexRApiClient.GetCameraList();
         if (cameraList != null)
         {
            foreach (var camera in cameraList)
            {
               var button = new Button
               {
                  Content = "Cam " + camera.ComponentNumber,
                  Height = 30,
                  Width = 60,
                  Tag = camera,
                  Margin = new Thickness(2)
               };
               ToolTip.SetTip(button, camera.Name);
               button.Click += OnSelectCameraClicked;
               pnlCameraSelectFlowPanel.Children.Add(button);
            }
         }
      }

      private void FillSelectAlarmButtonList()
      {
         ClearAlarmSelection();
         List<Alarm> alarmList = moFlexRApiClient.GetAlarmList();
         if (alarmList == null) return;

         foreach (Alarm alarm in alarmList)
         {
            var border = new Border
            {
               Width = 64, Height = 24,
               BorderThickness = new Thickness(2),
               BorderBrush = GetBrushForStatus(alarm.Status),
               Margin = new Thickness(2)
            };

            var button = new Button
            {
               Content = "Alarm " + alarm.ComponentNumber,
               FontSize = 10,
               Tag = alarm,
               HorizontalAlignment = HorizontalAlignment.Center,
               VerticalAlignment = VerticalAlignment.Center,
            };
            ToolTip.SetTip(button, alarm.Name);
            button.Click += OnSelectAlarmClicked;
            border.Child = button;
            border.Tag = alarm;

            pnlAlarms.Children.Add(border);

            alarm.PropertyChanged += (s, ev) =>
            {
               if (ev.PropertyName == "Status")
                  Dispatcher.UIThread.Post(() => border.BorderBrush = GetBrushForStatus(alarm.Status));
            };
         }
      }

      private static IBrush GetBrushForStatus(AlarmGeneralStatus status)
      {
         return status switch
         {
            AlarmGeneralStatus.Active => Brushes.Red,
            AlarmGeneralStatus.Inactive => Brushes.Gray,
            AlarmGeneralStatus.Acknowledged => Brushes.Orange,
            AlarmGeneralStatus.Tampered => Brushes.DarkOrange,
            _ => new SolidColorBrush(Color.FromArgb(120, 128, 128, 128)),
         };
      }

      private void SetCurrentLoggedInUser() { CurrentLoggedInUser = FlexRApiClient.GetLoggedInUserInfo(); }

      private void OnControlCameraPointerPressed(object sender, PointerPressedEventArgs e)
      {
         var camera = mCurrentCamera;
         if (camera == null) return;
         if (sender == btnPanLeft) camera.PanLeft(80);
         else if (sender == btnPanRight) camera.PanRight(80);
         else if (sender == btnTiltUp) camera.TiltUp(80);
         else if (sender == btnTiltDown) camera.TiltDown(80);
         else if (sender == btnZoomIn) camera.ZoomIn(80);
         else if (sender == btnZoomOut) camera.ZoomOut(80);
         else if (sender == btnFocusFar) camera.FocusFar();
         else if (sender == btnFocusNear) camera.FocusNear();
      }

      private void OnControlCameraPointerReleased(object sender, PointerReleasedEventArgs e)
      {
         var camera = mCurrentCamera;
         if (camera != null && sender is Button)
            camera.PanTiltZoomStop();
      }

      private void chkPreferSubChannel_CheckedChanged(object sender, RoutedEventArgs e)
      {
         if (mIsLoadingSettings) return;
         if (IsStarted)
         {
            if (mIsPlaybackStarted) btnPlayPlayback_Click(null, null);
            else if (mCurrentCamera != null)
               SelectCamera(mCurrentCamera.ComponentNumber, chkPreferSubChannel.IsChecked == true ? 2 : 1);
         }
         SaveSettings();
      }

      private void btnClearMessages_Click(object sender, RoutedEventArgs e) { mMessages.Clear(); }

      private void btnGotoPreset_Click(object sender, RoutedEventArgs e)
      {
         if (selPreset.SelectedItem is Preset preset) preset.GotoPreset();
      }

      private void btnPlayPlayback_Click(object sender, RoutedEventArgs e)
      {
         if (selPlayback.SelectedItem is PlaybackInfo recording)
         {
            string url = recording.PlaybackUrl;
            if (!string.IsNullOrEmpty(url))
            {
               txtCurrentRtspUrl.Text = GetMaskedUrl(url);
               var cameraNo = mCurrentCamera?.ComponentNumber ?? 0;
               txtVideoHeader.Text = $"PLAYBACK - Camera {cameraNo}";
               brdVideoHeader.Background = new SolidColorBrush(Color.FromRgb(142, 62, 62));
               StartRtspStream(url);
               IsPlaybackStarted = true;
               UpdateEnabled();
            }
         }
      }

      private void btnGotoTime_Click(object sender, RoutedEventArgs e)
      {
         if (selPlayback.SelectedItem is PlaybackInfo recording)
         {
            try
            {
               // Parse date from TextBox (format: YYYY-MM-DD)
               DateTime date = DateTime.Now.Date;
               if (!string.IsNullOrWhiteSpace(txtDatePlayback.Text) && DateTime.TryParse(txtDatePlayback.Text, out DateTime parsedDate))
               {
                  date = parsedDate.Date;
               }

               // Parse time from TextBox (format: HH:MM:SS or HH:MM)
               TimeSpan time = TimeSpan.Zero;
               if (!string.IsNullOrWhiteSpace(txtTimePlayback.Text) && TimeSpan.TryParse(txtTimePlayback.Text, out TimeSpan parsedTime))
               {
                  time = parsedTime;
               }

               var dateTimeObject = date + time;
               string startTimeParameter = $"?start={dateTimeObject:yyyyMMddHHmmss}";
               string url = recording.PlaybackUrl;
               if (!string.IsNullOrEmpty(url))
               {
                  string urlWithStartTime = url + startTimeParameter;
                  txtCurrentRtspUrl.Text = GetMaskedUrl(url);
                  var cameraNo = mCurrentCamera?.ComponentNumber ?? 0;
                  txtVideoHeader.Text = $"PLAYBACK - Camera {cameraNo}";
                  brdVideoHeader.Background = new SolidColorBrush(Color.FromRgb(142, 62, 62));
                  StartRtspStream(urlWithStartTime);
                  IsPlaybackStarted = true;
                  UpdateEnabled();
               }
            }
            catch { }
         }
      }

      private void btnStopPlayback_Click(object sender, RoutedEventArgs e)
      {
         IsStarted = false;
         StopRtspStream();
         SelectCamera(mCurrentCamera.ComponentNumber, chkPreferSubChannel.IsChecked == true ? 2 : 1);
         IsStarted = true;
         IsPlaybackStarted = false;
         UpdateEnabled();
      }

      private void selPlayback_SelectedIndexChanged(object sender, SelectionChangedEventArgs e)
      {
         if (selPlayback.SelectedItem is PlaybackInfo recording)
         {
            try
            {
               // Avalonia DatePicker doesn't support display date range constraints
            }
            catch { }
         }
      }

      private async void btnOpenDownloadWindow_Click(object sender, RoutedEventArgs e)
      {
         var downloadWindow = new DownloadWindow(FlexRApiClient);
         await downloadWindow.ShowDialog(this);
      }

      private void chkUseTcp_CheckedChanged(object sender, RoutedEventArgs e)
      {
         if (mIsLoadingSettings) return;
         if (IsStarted)
         {
            if (mIsPlaybackStarted) btnPlayPlayback_Click(null, null);
            else if (mCurrentCamera != null)
               SelectCamera(mCurrentCamera.ComponentNumber, chkPreferSubChannel.IsChecked == true ? 2 : 1);
         }
         SaveSettings();
      }

      private async void btnOpenAbsolutePositionWindow_Click(object sender, RoutedEventArgs e)
      {
         var window = new AbsolutePositionWindow(FlexRApiClient, mCurrentCamera);
         await window.ShowDialog(this);
      }

      private async void btnCameraLock_Click(object sender, RoutedEventArgs e)
      {
         if (!mCurrentCamera.IsLocked)
         {
            var window = new CameraLockWindow(FlexRApiClient, mCurrentCamera);
            await window.ShowDialog(this);
         }
         else
         {
            FlexRApiClient.SendUnlockCamera(mCurrentCamera.ComponentNumber);
         }
      }

      private async void btnPickDate_Click(object sender, RoutedEventArgs e)
      {
         var picker = new DatePicker
         {
            SelectedDate = DateTime.TryParse(txtDatePlayback.Text, out DateTime currentDate) 
               ? new DateTimeOffset(currentDate) 
               : new DateTimeOffset(DateTime.Now)
         };

         var okButton = new Button
         {
            Content = "OK",
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 10, 0, 0)
         };

         var dialog = new Window
         {
            Title = "Select Date",
            Width = 320,
            Height = 380,
            Content = new StackPanel
            {
               Margin = new Thickness(10),
               Children = { picker, okButton }
            },
            WindowStartupLocation = WindowStartupLocation.CenterOwner
         };

         okButton.Click += (s, args) =>
         {
            if (picker.SelectedDate.HasValue)
            {
               txtDatePlayback.Text = picker.SelectedDate.Value.ToString("yyyy-MM-dd");
            }
            dialog.Close();
         };

         await dialog.ShowDialog(this);
      }

      private async void btnPickTime_Click(object sender, RoutedEventArgs e)
      {
         var picker = new TimePicker
         {
            ClockIdentifier = "24HourClock",
            SelectedTime = TimeSpan.TryParse(txtTimePlayback.Text, out TimeSpan currentTime)
               ? currentTime
               : DateTime.Now.TimeOfDay
         };

         var okButton = new Button
         {
            Content = "OK",
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 10, 0, 0)
         };

         var dialog = new Window
         {
            Title = "Select Time",
            Width = 320,
            Height = 380,
            Content = new StackPanel
            {
               Margin = new Thickness(10),
               Children = { picker, okButton }
            },
            WindowStartupLocation = WindowStartupLocation.CenterOwner
         };

         okButton.Click += (s, args) =>
         {
            if (picker.SelectedTime.HasValue)
            {
               txtTimePlayback.Text = picker.SelectedTime.Value.ToString(@"hh\:mm\:ss");
            }
            dialog.Close();
         };

         await dialog.ShowDialog(this);
      }
   }

   public class MessageItem
   {
      public string Text { get; set; }
      public IBrush Color { get; set; }
   }
}
