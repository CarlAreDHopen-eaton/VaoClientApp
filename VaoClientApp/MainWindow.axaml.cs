using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
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
      private ContextMenu mVideoContextMenu;
      private LibVLC mLibVlc;
      private MediaPlayer mMediaPlayer;
      private bool mIsVideoStarted = false;
      private bool mIsLoadingSettings;
      private bool mIsMessagesCollapsed = false;
      private GridLength mMessagesExpandedRowHeight = new GridLength(1, GridUnitType.Star);

      private ObservableCollection<MessageItem> mMessages = new();
      private ObservableCollection<MessageItem> mFilteredMessages = new();
      private Dictionary<MessageSource, bool> mSourceFilters = new()
      {
         { MessageSource.FlexApi, true },
         { MessageSource.LibVlc, true },
         { MessageSource.Config, true }
      };

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

         lstMessages.ItemsSource = mFilteredMessages;

         StartInitializeVlc();
         ClearPresetDropdown();
         ClearRecordingDropdown();

         mIsLoadingSettings = true;
         LoadSettings();
         mIsLoadingSettings = false;

         UpdateEnabled();
         ApplyIcons();
         UpdateUserInitial();

         Opened += MainWindow_Opened;
         KeyDown += MainWindow_KeyDown;
      }

      private void MainWindow_KeyDown(object sender, KeyEventArgs e)
      {
         if (e.Key == Key.F9)
         {
            btnToggleSidebar_Click(null, null);
            e.Handled = true;
         }
         else if (e.Key == Key.F10)
         {
            var app = (App)Avalonia.Application.Current;
            app.SetTheme(!IsDarkMode);
            RefreshMessageColors();
            RefreshVideoHeaderState();
            SaveSettings();
            e.Handled = true;
         }
      }

      private void MainWindow_Opened(object sender, EventArgs e)
      {
         Opened -= MainWindow_Opened;

         var s = AppSettings.Default;
         expCameraControl.IsExpanded = s.IsCameraControlExpanded;
         expCameraSelection.IsExpanded = s.IsCameraSelectionExpanded;
         expPresetSelection.IsExpanded = s.IsPresetSelectionExpanded;
         expAlarms.IsExpanded = s.IsAlarmsExpanded;
         expPlaybackSelection.IsExpanded = s.IsPlaybackSelectionExpanded;
         expDownloadRecording.IsExpanded = s.IsDownloadRecordingExpanded;

         // Apply saved sidebar state
         SetSidebarCollapsed(s.IsSidebarCollapsed);

         // Restore messages split ratio and collapsed state
         if (brdMessages.Parent is Grid mg && mg.RowDefinitions.Count > 2)
         {
            mg.RowDefinitions[0].Height = new GridLength(s.MessagesSplitVideoStars, GridUnitType.Star);
            mg.RowDefinitions[2].Height = new GridLength(s.MessagesSplitMessagesStars, GridUnitType.Star);
         }
         if (s.IsMessagesCollapsed)
         {
            mIsMessagesCollapsed = false; // will be toggled to true
            MessagesHeader_PointerPressed(null, null);
         }

         // Auto-connect if enabled
         if (s.AutoConnectOnStartup)
            btnConnect_Click(null, null);
      }

      private void btnToggleSidebar_Click(object sender, RoutedEventArgs e)
      {
         var isCurrentlyCollapsed = sidebarGrid.Width == 56;
         SetSidebarCollapsed(!isCurrentlyCollapsed);
         AppSettings.Default.IsSidebarCollapsed = !isCurrentlyCollapsed;
         SaveSettings();
      }

      private void SetSidebarCollapsed(bool collapsed)
      {
         if (collapsed)
         {
            sidebarGrid.Width = 56;
            narrowSidebar.IsVisible = true;
            fullSidebar.IsVisible = false;
         }
         else
         {
            sidebarGrid.Width = 280;
            narrowSidebar.IsVisible = false;
            fullSidebar.IsVisible = true;
         }
      }

      private void CollapseAllExpandersExcept(Expander target)
      {
         var allExpanders = new[] { expCameraControl, expCameraSelection, expPresetSelection, expAlarms, expPlaybackSelection, expDownloadRecording };
         foreach (var exp in allExpanders)
         {
            if (exp != target)
               exp.IsExpanded = false;
         }
         target.IsExpanded = true;
      }

      // Icon button handlers for narrow sidebar
      private void btnExpandCameraControl_Click(object sender, RoutedEventArgs e)
      {
         SetSidebarCollapsed(false);
         CollapseAllExpandersExcept(expCameraControl);
         AppSettings.Default.IsSidebarCollapsed = false;
         SaveSettings();
      }

      private void btnExpandCameraSelection_Click(object sender, RoutedEventArgs e)
      {
         SetSidebarCollapsed(false);
         CollapseAllExpandersExcept(expCameraSelection);
         AppSettings.Default.IsSidebarCollapsed = false;
         SaveSettings();
      }

      private void btnExpandPresetSelection_Click(object sender, RoutedEventArgs e)
      {
         SetSidebarCollapsed(false);
         CollapseAllExpandersExcept(expPresetSelection);
         AppSettings.Default.IsSidebarCollapsed = false;
         SaveSettings();
      }

      private void btnExpandAlarms_Click(object sender, RoutedEventArgs e)
      {
         SetSidebarCollapsed(false);
         CollapseAllExpandersExcept(expAlarms);
         AppSettings.Default.IsSidebarCollapsed = false;
         SaveSettings();
      }

      private void btnExpandPlayback_Click(object sender, RoutedEventArgs e)
      {
         SetSidebarCollapsed(false);
         CollapseAllExpandersExcept(expPlaybackSelection);
         AppSettings.Default.IsSidebarCollapsed = false;
         SaveSettings();
      }

      private void btnExpandDownload_Click(object sender, RoutedEventArgs e)
      {
         SetSidebarCollapsed(false);
         CollapseAllExpandersExcept(expDownloadRecording);
         AppSettings.Default.IsSidebarCollapsed = false;
         SaveSettings();
      }

      private void narrowSidebar_PointerEntered(object sender, Avalonia.Input.PointerEventArgs e)
      {
         // Auto-expand on hover
         if (sidebarGrid.Width == 56)
         {
            SetSidebarCollapsed(false);
            // Don't save the state - this is just a temporary hover expand
         }
      }

      private void fullSidebar_PointerExited(object sender, Avalonia.Input.PointerEventArgs e)
      {
         // Auto-collapse on mouse leave if the sidebar was opened via hover (not manually toggled)
         if (AppSettings.Default.IsSidebarCollapsed && sidebarGrid.Width == 280)
         {
            SetSidebarCollapsed(true);
         }
      }

      private bool IsDarkMode => AppSettings.Default.IsDarkMode;

      private void ApplyIcons()
      {
         // Icons are now rendered via Material Symbols font in AXAML.
         // No bitmap loading needed. Camera lock icon state is managed in Camera_LockStatusChanged.
      }

      private void UpdateUserInitial()
      {
         string username = AppSettings.Default.User?.Trim() ?? "";
         var txtMenuUsername = this.FindControl<TextBlock>("txtMenuUsername");
         var menuItemLogin = this.FindControl<MenuItem>("menuItemLogin");
         var menuItemLogout = this.FindControl<MenuItem>("menuItemLogout");

         if (string.IsNullOrEmpty(username))
         {
            txtUserInitial.Text = "U";
            if (txtMenuUsername != null) txtMenuUsername.Text = "Not logged in";
         }
         else
         {
            txtUserInitial.Text = username.Substring(0, 1).ToUpper();
            if (txtMenuUsername != null) txtMenuUsername.Text = username;
         }

         // Update Login/Logout enabled state based on connection
         if (menuItemLogin != null) menuItemLogin.IsEnabled = !IsStarted;
         if (menuItemLogout != null) menuItemLogout.IsEnabled = IsStarted;

         // Update tooltip with user, connection status, and server
          var status = IsStarted ? "Connected" : "Disconnected";
          var host = AppSettings.Default.Host1?.Trim() ?? "";
          var tip = string.IsNullOrEmpty(username) ? "Not logged in" : username;
          tip += $"\n{status}";
          if (IsStarted && !string.IsNullOrEmpty(host))
             tip += $"\nServer: {host}";
          ToolTip.SetTip(btnUserProfile, tip);

          // Gray circle when disconnected, themed color when connected
          if (IsStarted)
          {
             if (this.TryFindResource("PrimaryDark", out var res) && res is IBrush brush)
                btnUserProfile.Background = brush;
             else
                btnUserProfile.Background = new SolidColorBrush(Color.FromRgb(0, 90, 143));
          }
          else
          {
             btnUserProfile.Background = new SolidColorBrush(Color.FromRgb(128, 128, 128));
          }
       }

      private void btnUserProfile_Click(object sender, RoutedEventArgs e)
      {
         // The flyout opens automatically when the button is clicked
      }

      private async void menuItemLogin_Click(object sender, RoutedEventArgs e)
      {
         var s = AppSettings.Default;
         // Check if credentials are configured
         if (string.IsNullOrWhiteSpace(s.Host1) || string.IsNullOrWhiteSpace(s.User) || string.IsNullOrWhiteSpace(s.Password))
         {
            // Open settings first if not configured
            var settingsWindow = new SettingsWindow();
            await settingsWindow.ShowDialog(this);

            if (settingsWindow.WereSettingsSaved())
            {
               LoadSettings();
               UpdateUserInitial();
               // After settings saved, attempt to connect
               btnConnect_Click(sender, e);
            }
         }
         else
         {
            // Credentials configured, just connect
            btnConnect_Click(sender, e);
         }
      }

      private async void menuItemSettings_Click(object sender, RoutedEventArgs e)
      {
         var settingsWindow = new SettingsWindow();
         await settingsWindow.ShowDialog(this);

         if (settingsWindow.WereSettingsSaved())
         {
            // Reload settings into main window
            LoadSettings();
            UpdateUserInitial();
            RefreshMessageColors();
            RefreshVideoHeaderState();
         }
      }

      private void menuItemLogout_Click(object sender, RoutedEventArgs e)
      {
         if (IsStarted)
         {
            btnDisconnect_Click(sender, e);
         }
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
         tglSubChannel.IsChecked = s.PreferSubChannel;
         UpdateUserInitial();
      }

      private void SaveSettings()
      {
         var s = AppSettings.Default;
         if (mCurrentCamera != null) s.CurrentCamera = mCurrentCamera.ComponentNumber;
         s.IsCameraControlExpanded = expCameraControl.IsExpanded;
         s.IsCameraSelectionExpanded = expCameraSelection.IsExpanded;
         s.IsPresetSelectionExpanded = expPresetSelection.IsExpanded;
         s.IsAlarmsExpanded = expAlarms.IsExpanded;
         s.IsPlaybackSelectionExpanded = expPlaybackSelection.IsExpanded;
         s.IsDownloadRecordingExpanded = expDownloadRecording.IsExpanded;
         s.IsMessagesCollapsed = mIsMessagesCollapsed;
         if (brdMessages.Parent is Grid mg && mg.RowDefinitions.Count > 2 && !mIsMessagesCollapsed)
         {
            s.MessagesSplitVideoStars = mg.RowDefinitions[0].Height.Value;
            s.MessagesSplitMessagesStars = mg.RowDefinitions[2].Height.Value;
         }
         s.Save();
      }

      protected override void OnClosing(WindowClosingEventArgs e)
      {
         SaveSettings();
         base.OnClosing(e);
      }

      public void UpdateEnabled()
      {
         var menuItemLogin = this.FindControl<MenuItem>("menuItemLogin");
         var menuItemLogout = this.FindControl<MenuItem>("menuItemLogout");
         if (menuItemLogin != null) menuItemLogin.IsEnabled = !IsStarted;
         if (menuItemLogout != null) menuItemLogout.IsEnabled = IsStarted;
         pnlCameraSelectFlowPanel.IsEnabled = IsStarted;
         tglSubChannel.IsEnabled = IsStarted && !IsPlayback && mCurrentCamera != null && !string.IsNullOrEmpty(mCurrentCamera?.Stream2Resolution);
         grpSelectPreset.IsEnabled = IsStarted;
         grpSelectPlayback.IsEnabled = IsStarted && ApiSupportsPlayback;
         grpCameraControl.IsEnabled = IsStarted && mCurrentCamera != null;

         btnStopPlayback.IsEnabled = IsStarted && IsPlayback && ApiSupportsPlayback;
         btnPlayPlayback.IsEnabled = IsStarted && IsCameraSelected && !IsPlaybackStarted && ApiSupportsPlayback;
         btnGotoTime.IsEnabled = IsStarted && IsCameraSelected && ApiSupportsPlayback;
         btnDownload.IsEnabled = IsStarted && ApiSupportsPlayback;

         UpdateCameraControl();
      }

      private void btnConnect_Click(object sender, RoutedEventArgs e)
      {
         SaveSettings();
          if (!ValidateCanConnect()) return;

         var s = AppSettings.Default;
         IsStarted = true;
         moFlexRApiClient = new FlexRApiClient
         {
            Host = s.Host1,
            Port = s.ApiPort,
            Password = s.Password,
            User = s.User,
            UseHttps = s.UseHttps,
            IgnoreCertificateErrors = true
         };
         moFlexRApiClient.OnMessage += OnFlexRApiClientMessage;
         txtVideoHeader.Text = "No Camera Selected";
         brdVideoHeader.Background = GetNeutralHeaderBrush();

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
             UpdateUserInitial();

             if (AppSettings.Default.CurrentCamera != 0)
                SelectCamera(AppSettings.Default.CurrentCamera, AppSettings.Default.PreferSubChannel ? 2 : 1);
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
         var s = AppSettings.Default;
         if (string.IsNullOrWhiteSpace(s.Host1))
         { WriteMessageLog(MessageSource.Config, "Missing host name", LogLevel.Error); return false; }
         if (string.IsNullOrWhiteSpace(s.Password))
         { WriteMessageLog(MessageSource.Config, "Missing host password", LogLevel.Error); return false; }
         if (string.IsNullOrWhiteSpace(s.User))
         { WriteMessageLog(MessageSource.Config, "Missing user name", LogLevel.Error); return false; }
         if (string.IsNullOrWhiteSpace(s.ApiPort))
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

         IBrush color = GetColorForLogLevel(level);

         if (strMessage != "drawable Warning: unsupported control query 3")
          {
             var item = new MessageItem
             {
                Text = strMsg,
                Color = color,
                Background = GetBackgroundForLogLevel(level),
                Source = source,
                Level = level
             };
             mMessages.Add(item);
             if (mSourceFilters.TryGetValue(source, out bool visible) && visible)
             {
                bool wasAtEnd = mFilteredMessages.Count == 0 || IsScrolledToEnd();
                mFilteredMessages.Add(item);
                if (wasAtEnd)
                   lstMessages.ScrollIntoView(mFilteredMessages.Count - 1);
             }
          }
      }

      private IBrush GetColorForLogLevel(LogLevel level)
      {
         return level switch
         {
            LogLevel.Error => Brushes.Red,
            LogLevel.Warning => new SolidColorBrush(IsDarkMode ? Colors.Orange : Color.FromRgb(120, 63, 0)),
            LogLevel.Debug => new SolidColorBrush(IsDarkMode ? Colors.LightBlue : Color.FromRgb(0, 67, 122)),
            _ => new SolidColorBrush(IsDarkMode ? Colors.White : Colors.Black),
         };
      }

      private IBrush GetBackgroundForLogLevel(LogLevel level)
      {
         if (IsDarkMode)
            return Brushes.Transparent;

         return level switch
         {
            LogLevel.Error => new SolidColorBrush(Color.FromRgb(255, 238, 238)),
            LogLevel.Warning => new SolidColorBrush(Color.FromRgb(255, 245, 230)),
            LogLevel.Debug => new SolidColorBrush(Color.FromRgb(236, 245, 252)),
            _ => Brushes.Transparent,
         };
      }

      private void RefreshVideoHeaderState()
      {
         if (!IsStarted || mCurrentCamera == null)
         {
            txtVideoHeader.Text = "No Camera Selected";
            brdVideoHeader.Background = GetNeutralHeaderBrush();
            return;
         }

         var cameraNo = mCurrentCamera.ComponentNumber;
         if (IsPlayback || IsPlaybackStarted)
         {
            txtVideoHeader.Text = $"PLAYBACK - Camera {cameraNo}";
            brdVideoHeader.Background = GetPlaybackHeaderBrush();
         }
         else
         {
            txtVideoHeader.Text = $"LIVE - Camera {cameraNo}";
            brdVideoHeader.Background = GetLiveHeaderBrush();
         }
      }

      private IBrush GetNeutralHeaderBrush()
      {
         if (this.TryFindResource("VideoHeaderBg", this.ActualThemeVariant, out var brush) && brush is IBrush b)
            return b;
         return new SolidColorBrush(Color.FromRgb(29, 58, 74));
      }

      private IBrush GetLiveHeaderBrush()
      {
         if (!IsDarkMode)
            return new SolidColorBrush(Color.FromRgb(46, 125, 50));
         if (this.TryFindResource("Success", this.ActualThemeVariant, out var brush) && brush is IBrush b)
            return b;
         return new SolidColorBrush(Color.FromRgb(57, 182, 32));
      }

      private IBrush GetPlaybackHeaderBrush()
      {
         if (!IsDarkMode)
            return new SolidColorBrush(Color.FromRgb(183, 28, 28));
         if (this.TryFindResource("Error", this.ActualThemeVariant, out var brush) && brush is IBrush b)
            return b;
         return new SolidColorBrush(Color.FromRgb(202, 60, 61));
      }

      private void RefreshMessageColors()
      {
         foreach (var item in mMessages)
         {
            item.Color = GetColorForLogLevel(item.Level);
            item.Background = GetBackgroundForLogLevel(item.Level);
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
         brdVideoHeader.Background = GetNeutralHeaderBrush();
         IsCameraSelected = false;
         ClearPresetDropdown();
             ClearRecordingDropdown();
                 ClearCameraSelection();
                 ClearAlarmSelection();
                 UpdateUserInitial();
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
               mCurrentCameraButton.Classes.Remove("camera-selected");

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
               mCurrentCameraButton.Classes.Add("camera-selected");

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
             var iconCameraLock = this.FindControl<TextBlock>("iconCameraLock");
             if (mCurrentCamera != null && mCurrentCamera.IsLocked && mCurrentCamera.LockOwner == "Alarm")
             {
                if (iconCameraLock != null)
                {
                   iconCameraLock.Text = "\uE899"; // lock
                   iconCameraLock.Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#CA3C3D"));
                }
                btnCameraLock.IsEnabled = true;
             }
             else if (mCurrentCamera != null && mCurrentCamera.IsLocked)
             {
                if (iconCameraLock != null)
                {
                   iconCameraLock.Text = "\uE899"; // lock
                   iconCameraLock.Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#F0AA1F"));
                }
                btnCameraLock.IsEnabled = true;
             }
             else if (mCurrentCamera != null && !mCurrentCamera.IsLocked)
             {
                if (iconCameraLock != null)
                {
                   iconCameraLock.Text = "\uE898"; // lock_open
                   if (this.TryFindResource("SidebarHeaderFg", this.ActualThemeVariant, out var brush) && brush is Avalonia.Media.IBrush b)
                      iconCameraLock.Foreground = b;
                }
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
            bool hasSubChannel = !string.IsNullOrEmpty(camera.Stream2Resolution);
            // If sub requested but not available, fall back to main
            if (streamNo == 2 && !hasSubChannel)
               streamNo = 1;
            string url = camera.GetCameraLiveStreamUrl(streamNo);
            if (!string.IsNullOrEmpty(url))
            {
               txtCurrentRtspUrl.Text = GetMaskedUrl(url);
               txtVideoHeader.Text = $"LIVE - Camera {cameraNo}";
               brdVideoHeader.Background = GetLiveHeaderBrush();
               StartRtspStream(url);
            }
            // Update toggle to reflect actual stream without triggering save
            mIsLoadingSettings = true;
            tglSubChannel.IsChecked = (streamNo == 2);
            tglSubChannel.IsEnabled = hasSubChannel && !IsPlayback;
            mIsLoadingSettings = false;
         }
      }

      private void SelectAlarm(int alarmNo)
      {
         Alarm alarm = moFlexRApiClient.GetSingleAlarm(alarmNo);
         if (alarm != null) CurrentAlarm = alarm;
      }

      private const int MaxCamerasPerSubmenu = 25;

      private void videoContextMenu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
      {
         mVideoContextMenu.Items.Clear();

         if (moFlexRApiClient == null)
         {
            var noConnection = new MenuItem { Header = "Not connected", IsEnabled = false };
            mVideoContextMenu.Items.Add(noConnection);
            return;
         }

         List<Camera> cameraList = moFlexRApiClient.GetCameraList();
         if (cameraList == null || cameraList.Count == 0)
         {
            var noCameras = new MenuItem { Header = "No cameras available", IsEnabled = false };
            mVideoContextMenu.Items.Add(noCameras);
            return;
         }

         var selectCameraMenu = new MenuItem { Header = "Select Camera" };

         if (cameraList.Count <= MaxCamerasPerSubmenu)
         {
            foreach (var camera in cameraList)
            {
               var item = CreateCameraMenuItem(camera);
               selectCameraMenu.Items.Add(item);
            }
         }
         else
         {
            // Split into sub-menus of MaxCamerasPerSubmenu each
            for (int i = 0; i < cameraList.Count; i += MaxCamerasPerSubmenu)
            {
               int end = Math.Min(i + MaxCamerasPerSubmenu, cameraList.Count);
               var batch = cameraList.GetRange(i, end - i);
               var first = batch.First();
               var last = batch.Last();
               var rangeMenu = new MenuItem { Header = $"{first.Name} – {last.Name}" };

               foreach (var camera in batch)
               {
                  var item = CreateCameraMenuItem(camera);
                  rangeMenu.Items.Add(item);
               }

               selectCameraMenu.Items.Add(rangeMenu);
            }
         }

         mVideoContextMenu.Items.Add(selectCameraMenu);
      }

      private MenuItem CreateCameraMenuItem(Camera camera)
      {
         var item = new MenuItem
         {
            Header = camera.Name,
            Tag = camera
         };
         if (mCurrentCamera == camera)
         {
            item.Icon = new CheckBox { IsChecked = true, IsHitTestVisible = false };
         }
         item.Click += (s, e) =>
         {
            int streamNo = tglSubChannel.IsChecked == true ? 2 : 1;
            SelectCamera(camera.ComponentNumber, streamNo);
         };
         return item;
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
             if (AppSettings.Default.UseTcp) media.AddOption(":rtsp-tcp");
             if (mVideoControl != null) mVideoControl.MediaPlayer = mMediaPlayer;
             mMediaPlayer.Play();
             mIsVideoStarted = true;
          }
          else
          {
             var media = new Media(mLibVlc, uri);
             if (AppSettings.Default.UseTcp) media.AddOption(":rtsp-tcp");
            mMediaPlayer.Play(media);
            mIsVideoStarted = true;
         }
      }

      private void InitVideoControl()
      {
         if (mVideoContextMenu == null)
         {
            mVideoContextMenu = new ContextMenu();
            mVideoContextMenu.Opening += videoContextMenu_Opening;
            brdVideoHeader.ContextMenu = mVideoContextMenu;
         }
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
            SelectCamera(camera.ComponentNumber, AppSettings.Default.PreferSubChannel ? 2 : 1);
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
                  Content = camera.Name,
                  Height = 28,
                  Tag = camera,
                  Margin = new Thickness(2),
                  Padding = new Thickness(4, 2),
                  FontSize = 11,
                  HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                  HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center
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

      private void tglSubChannel_CheckedChanged(object sender, RoutedEventArgs e)
      {
         if (mIsLoadingSettings) return;
         if (IsStarted)
         {
            if (mIsPlaybackStarted) btnPlayPlayback_Click(null, null);
            else if (mCurrentCamera != null)
               SelectCamera(mCurrentCamera.ComponentNumber, tglSubChannel.IsChecked == true ? 2 : 1);
         }
      }

      private void MessagesHeader_PointerPressed(object sender, PointerPressedEventArgs e)
      {
         mIsMessagesCollapsed = !mIsMessagesCollapsed;
         var messagesGrid = brdMessages.Parent as Grid;
         if (messagesGrid == null) return;
         var messagesRow = messagesGrid.RowDefinitions[2];

         if (mIsMessagesCollapsed)
         {
            mMessagesExpandedRowHeight = messagesRow.Height;
            messagesRow.Height = GridLength.Auto;
            brdMessages.MaxHeight = 36;
            lstMessages.IsVisible = false;
            messagesSplitter.IsEnabled = false;
             txtMessagesToggle.Text = "▶";
         }
         else
         {
            brdMessages.MaxHeight = double.PositiveInfinity;
            messagesRow.Height = mMessagesExpandedRowHeight;
            lstMessages.IsVisible = true;
            messagesSplitter.IsEnabled = true;
             txtMessagesToggle.Text = "▼";
         }
         SaveSettings();
      }

      private bool IsScrolledToEnd()
      {
         var scrollViewer = FindScrollViewer(lstMessages);
         if (scrollViewer != null)
            return scrollViewer.Offset.Y >= scrollViewer.Extent.Height - scrollViewer.Viewport.Height - 20;
         return true;
      }

      private Avalonia.Controls.ScrollViewer FindScrollViewer(Avalonia.Controls.Control parent)
      {
         if (parent is Avalonia.Controls.ScrollViewer sv) return sv;
         foreach (var child in Avalonia.VisualTree.VisualExtensions.GetVisualChildren(parent))
         {
            if (child is Avalonia.Controls.Control c)
            {
               var result = FindScrollViewer(c);
               if (result != null) return result;
            }
         }
         return null;
      }

      private void ApplyMessageFilter()
      {
         mFilteredMessages.Clear();
         foreach (var msg in mMessages)
         {
            if (mSourceFilters.TryGetValue(msg.Source, out bool visible) && visible)
               mFilteredMessages.Add(msg);
         }
         if (mFilteredMessages.Count > 0)
            lstMessages.ScrollIntoView(mFilteredMessages.Count - 1);
      }

      private void OnSourceFilterChanged(MessageSource source, bool isChecked)
      {
         mSourceFilters[source] = isChecked;
         ApplyMessageFilter();
      }

      private void btnClearMessages_Click(object sender, RoutedEventArgs e) { mMessages.Clear(); mFilteredMessages.Clear(); }

      private async void menuCopyMessages_Click(object sender, RoutedEventArgs e)
      {
         var text = string.Join(Environment.NewLine, mFilteredMessages.Select(m => m.Text));
         if (Clipboard is { } clipboard)
            await clipboard.SetTextAsync(text);
      }

      private async void menuCopySelectedMessages_Click(object sender, RoutedEventArgs e)
      {
         await CopySelectedMessagesAsync();
      }

      private async void lstMessages_KeyDown(object sender, KeyEventArgs e)
      {
         if (e.Key == Key.C && (e.KeyModifiers & KeyModifiers.Control) == KeyModifiers.Control)
         {
            await CopySelectedMessagesAsync();
            e.Handled = true;
         }
      }

      private async Task CopySelectedMessagesAsync()
      {
         var selectedText = string.Join(Environment.NewLine,
            lstMessages.SelectedItems.Cast<MessageItem>().Select(m => m.Text));
         if (!string.IsNullOrEmpty(selectedText) && Clipboard is { } clipboard)
            await clipboard.SetTextAsync(selectedText);
      }

      private void menuScrollToEnd_Click(object sender, RoutedEventArgs e)
      {
         if (mFilteredMessages.Count > 0)
            lstMessages.ScrollIntoView(mFilteredMessages.Count - 1);
      }

      private void menuFilterFlexApi_Click(object sender, RoutedEventArgs e)
      {
         ToggleSourceFilter(sender, MessageSource.FlexApi);
      }

      private void menuFilterLibVlc_Click(object sender, RoutedEventArgs e)
      {
         ToggleSourceFilter(sender, MessageSource.LibVlc);
      }

      private void menuFilterConfig_Click(object sender, RoutedEventArgs e)
      {
         ToggleSourceFilter(sender, MessageSource.Config);
      }

      private void ToggleSourceFilter(object sender, MessageSource source)
      {
         if (sender is MenuItem menuItem && menuItem.Icon is CheckBox cb)
         {
            cb.IsChecked = !(cb.IsChecked ?? false);
            OnSourceFilterChanged(source, cb.IsChecked ?? false);
         }
      }

      private void MessagesSplitter_DragCompleted(object sender, Avalonia.Input.VectorEventArgs e)
      {
         SaveSettings();
      }

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
               brdVideoHeader.Background = GetPlaybackHeaderBrush();
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
                  brdVideoHeader.Background = GetPlaybackHeaderBrush();
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
         SelectCamera(mCurrentCamera.ComponentNumber, tglSubChannel.IsChecked == true ? 2 : 1);
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
               : new DateTimeOffset(DateTime.Now),
            HorizontalAlignment = HorizontalAlignment.Stretch
         };

         var dialog = CreatePickerDialog("Select Date", "Choose a date for playback", picker, result =>
         {
            if (picker.SelectedDate.HasValue)
               txtDatePlayback.Text = picker.SelectedDate.Value.ToString("yyyy-MM-dd");
         });

         await dialog.ShowDialog(this);
      }

      private async void btnPickTime_Click(object sender, RoutedEventArgs e)
      {
         var picker = new TimePicker
         {
            ClockIdentifier = "24HourClock",
            SelectedTime = TimeSpan.TryParse(txtTimePlayback.Text, out TimeSpan currentTime)
               ? currentTime
               : DateTime.Now.TimeOfDay,
            HorizontalAlignment = HorizontalAlignment.Stretch
         };

         var dialog = CreatePickerDialog("Select Time", "Choose a time for playback", picker, result =>
         {
            if (picker.SelectedTime.HasValue)
               txtTimePlayback.Text = picker.SelectedTime.Value.ToString(@"hh\:mm\:ss");
         });

         await dialog.ShowDialog(this);
      }

      private Window CreatePickerDialog(string title, string subtitle, Control pickerContent, Action<bool> onOk)
      {
         Window dialog = null;

         // Header (64px, Primary background)
         var primaryBrush = this.FindResource("Primary") is IBrush b
            ? b : new SolidColorBrush(Color.Parse("#007BC1"));
         var header = new Border
         {
            Height = 64,
            Background = primaryBrush,
            Child = new StackPanel
            {
               VerticalAlignment = VerticalAlignment.Center,
               Margin = new Thickness(24, 0, 0, 0),
               Children =
               {
                  new TextBlock
                  {
                     Text = title,
                     FontSize = 20,
                     FontWeight = FontWeight.Medium,
                     Foreground = Brushes.White
                  },
                  new TextBlock
                  {
                     Text = subtitle,
                     FontSize = 12,
                     Foreground = Brushes.White,
                     Opacity = 0.7
                  }
               }
            }
         };

         // Content area
         var content = new Border
         {
            Padding = new Thickness(24),
            Child = pickerContent
         };

         // Footer with Cancel + OK buttons
         var cancelButton = new Button
         {
            Content = "Cancel",
            MinWidth = 100,
            Classes = { "outlined" }
         };

         var okButton = new Button
         {
            Content = "OK",
            MinWidth = 100,
            Classes = { "primary" }
         };

         var footer = new Border
         {
            Padding = new Thickness(24, 16),
            Child = new StackPanel
            {
               Orientation = Avalonia.Layout.Orientation.Horizontal,
               HorizontalAlignment = HorizontalAlignment.Right,
               Spacing = 12,
               Children = { cancelButton, okButton }
            }
         };
         footer.Bind(Border.BackgroundProperty, footer.GetResourceObservable("Surface1"));
         footer.Bind(Border.BorderBrushProperty, footer.GetResourceObservable("Divider"));
         footer.BorderThickness = new Thickness(0, 1, 0, 0);

         dialog = new Window
         {
            Title = title,
            MinWidth = 320,
            SizeToContent = SizeToContent.WidthAndHeight,
            CanResize = false,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Content = new StackPanel
            {
               Children = { header, content, footer }
            }
         };

         okButton.Click += (s, args) =>
         {
            onOk(true);
            dialog.Close();
         };

         cancelButton.Click += (s, args) =>
         {
            dialog.Close();
         };

         return dialog;
      }
   }

   public enum MessageSource { FlexApi, LibVlc, Config }

   public class MessageItem : INotifyPropertyChanged
   {
      private string mText;
      private IBrush mColor;
      private IBrush mBackground;

      public string Text
      {
         get => mText;
         set
         {
            if (mText == value) return;
            mText = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text)));
         }
      }

      public IBrush Color
      {
         get => mColor;
         set
         {
            if (mColor == value) return;
            mColor = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Color)));
         }
      }

      public IBrush Background
      {
         get => mBackground;
         set
         {
            if (mBackground == value) return;
            mBackground = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Background)));
         }
      }

      public MessageSource Source { get; set; }
      public LogLevel Level { get; set; }

      public event PropertyChangedEventHandler PropertyChanged;
   }
}
