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
using Vao.Sample.Navigation;
using Vao.Sample.Pages;

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
      private string mActiveRtspUrl;
      private bool mIsVideoStarted = false;
      private bool mIsLoadingSettings;
      private bool mIsMessagesCollapsed = false;
      private GridLength mMessagesExpandedRowHeight = new GridLength(1, GridUnitType.Star);
      private bool mIsVideoTemporarilyDetached = false;
      private bool mPendingConnectAfterSettings = false;
      private bool mIsNavigationOverlayVisible = false;
      private bool mIsPickerOverlayVisible = false;
      private bool mIsConnecting = false;

      private const double SidebarAutoCollapseBreakpoint = 1100;

      private enum PickerOverlayMode
      {
         None,
         Date,
         Time
      }

      private PickerOverlayMode mPickerOverlayMode = PickerOverlayMode.None;

      // Navigation service for tablet/mobile compatibility
      private NavigationService mNavigationService;

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
         EnsureVideoContextMenu();

         lstMessages.ItemsSource = mFilteredMessages;

         StartInitializeVlc();
         ClearPresetDropdown();
         ClearRecordingDropdown();

         // Initialize navigation service for pages/dialogs
         InitializeNavigationService();

         mIsLoadingSettings = true;
         LoadSettings();
         mIsLoadingSettings = false;

         UpdateEnabled();
         ApplyIcons();
         UpdateUserInitial();

         Opened += MainWindow_Opened;
            AddHandler(KeyDownEvent, MainWindow_KeyDown, handledEventsToo: true);
            SizeChanged += MainWindow_SizeChanged;
      }

      private void InitializeNavigationService()
      {
         mNavigationService = new NavigationService();
         var navigationHost = this.FindControl<ContentControl>("navigationHost");
         if (navigationHost != null)
         {
            mNavigationService.Initialize(navigationHost);
            mNavigationService.NavigationChanged += NavigationService_NavigationChanged;
         }
      }

      private void NavigationService_NavigationChanged(object sender, NavigationChangedEventArgs e)
      {
         SetNavigationOverlayState(e.IsOverlayVisible);

         if (!e.IsOverlayVisible)
         {
            LoadSettings();
            UpdateUserInitial();
            RefreshMessageColors();
            RefreshVideoHeaderState();

            if (mPendingConnectAfterSettings)
            {
               mPendingConnectAfterSettings = false;
               var s = AppSettings.Default;
               if (!IsStarted && !string.IsNullOrWhiteSpace(s.Host1) && !string.IsNullOrWhiteSpace(s.User) && !string.IsNullOrWhiteSpace(s.Password))
               {
                  btnConnect_Click(null, null);
               }
            }
         }
      }

      private void SetNavigationOverlayState(bool isOverlayVisible)
      {
         mIsNavigationOverlayVisible = isOverlayVisible;
         SetMainChromeVisible(!isOverlayVisible);
         UpdateVideoSurfaceForOverlayState();
      }

      private void UpdateVideoSurfaceForOverlayState()
      {
         if (mIsNavigationOverlayVisible || mIsPickerOverlayVisible)
         {
            DetachVideoSurfaceForOverlay();
         }
         else
         {
            ReattachVideoSurfaceAfterOverlay();
         }
      }

      private void SetMainChromeVisible(bool isVisible)
      {
         if (mainTopBar != null)
            mainTopBar.IsVisible = isVisible;

         if (rootLayoutGrid != null && rootLayoutGrid.RowDefinitions.Count > 0)
            rootLayoutGrid.RowDefinitions[0].Height = isVisible ? new GridLength(64) : new GridLength(0);
      }

      private void DetachVideoSurfaceForOverlay()
      {
         if (mVideoControl == null || !pnlVideo.Children.Contains(mVideoControl))
            return;

         if (mMediaPlayer != null)
            mVideoControl.MediaPlayer = null;

         pnlVideo.Children.Remove(mVideoControl);
         mIsVideoTemporarilyDetached = true;
      }

      private void ReattachVideoSurfaceAfterOverlay()
      {
         bool shouldAttach = mIsVideoTemporarilyDetached ||
                             (mIsVideoStarted && mMediaPlayer != null && (mVideoControl == null || !pnlVideo.Children.Contains(mVideoControl)));
         if (!shouldAttach)
            return;

         if (mVideoControl != null && pnlVideo.Children.Contains(mVideoControl))
            pnlVideo.Children.Remove(mVideoControl);

         // Recreate the native video surface to avoid stale handles after overlay detach.
         mVideoControl = new VideoView();
         pnlVideo.Children.Add(mVideoControl);

         if (mMediaPlayer != null)
         {
            mVideoControl.MediaPlayer = null;
            mVideoControl.MediaPlayer = mMediaPlayer;

            // Emulate manual camera-switch recovery when native surface fails to redraw.
            if (mIsVideoStarted && !string.IsNullOrWhiteSpace(mActiveRtspUrl))
            {
               Dispatcher.UIThread.Post(() =>
               {
                  if (!mIsNavigationOverlayVisible && !mIsPickerOverlayVisible && mIsVideoStarted)
                  {
                     StartRtspStream(mActiveRtspUrl);
                  }
               }, DispatcherPriority.Background);
            }
         }

         mIsVideoTemporarilyDetached = false;
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
            UpdateThemeMenuLabel();
            e.Handled = true;
         }
         else if (e.Key == Key.F12)
         {
            OpenSettingsPage();
            e.Handled = true;
         }
         else if (e.Key == Key.Escape)
         {
            HandleEscapeNavigation();
            e.Handled = true;
         }
         
         // Check for Shift+Ctrl+, (previous camera) or Shift+Ctrl+. (next camera)
         var isShiftCtrl = (e.KeyModifiers & KeyModifiers.Shift) != 0 && 
                           (e.KeyModifiers & KeyModifiers.Control) != 0;
         
         if (isShiftCtrl)
         {
            var keySymbol = e.KeySymbol ?? string.Empty;

            if (keySymbol == "," || keySymbol == "<" || e.PhysicalKey == PhysicalKey.Comma)
            {
               NavigateToPreviousCamera();
               e.Handled = true;
            }
            else if (keySymbol == "." || keySymbol == ">" || e.PhysicalKey == PhysicalKey.Period)
            {
               NavigateToNextCamera();
               e.Handled = true;
            }
         }
      }

      private void MainWindow_Opened(object sender, EventArgs e)
      {
         Opened -= MainWindow_Opened;

         UpdateThemeMenuLabel();
         var s = AppSettings.Default;
         expCameraControl.IsExpanded = s.IsCameraControlExpanded;
         expCameraSelection.IsExpanded = s.IsCameraSelectionExpanded;
         expPresetSelection.IsExpanded = s.IsPresetSelectionExpanded;
         expAlarms.IsExpanded = s.IsAlarmsExpanded;
         expPlaybackSelection.IsExpanded = s.IsPlaybackSelectionExpanded;
         expDownloadRecording.IsExpanded = s.IsDownloadRecordingExpanded;

         // Apply saved sidebar state
             SetSidebarCollapsed(s.IsSidebarCollapsed, persistSetting: false);
             ApplyResponsiveSidebarLayout(Bounds.Width);

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
         SetSidebarCollapsed(!isCurrentlyCollapsed, persistSetting: true);
      }

      private void SetSidebarCollapsed(bool collapsed, bool persistSetting = false)
      {
         // Keep navigation controls anchored to the left rail; no floating overlay trigger.
         btnOpenSidebar.IsVisible = false;

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

         if (persistSetting)
         {
            AppSettings.Default.IsSidebarCollapsed = collapsed;
            SaveSettings();
         }
      }

      private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
      {
         ApplyResponsiveSidebarLayout(e.NewSize.Width);
      }

      private void ApplyResponsiveSidebarLayout(double width)
      {
         if (width < SidebarAutoCollapseBreakpoint)
         {
            SetSidebarCollapsed(true, persistSetting: false);
            return;
         }

         SetSidebarCollapsed(AppSettings.Default.IsSidebarCollapsed, persistSetting: false);
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
         SetSidebarCollapsed(false, persistSetting: true);
         CollapseAllExpandersExcept(expCameraControl);
      }

      private void btnExpandCameraSelection_Click(object sender, RoutedEventArgs e)
      {
         SetSidebarCollapsed(false, persistSetting: true);
         CollapseAllExpandersExcept(expCameraSelection);
      }

      private void btnExpandPresetSelection_Click(object sender, RoutedEventArgs e)
      {
         SetSidebarCollapsed(false, persistSetting: true);
         CollapseAllExpandersExcept(expPresetSelection);
      }

      private void btnExpandAlarms_Click(object sender, RoutedEventArgs e)
      {
         SetSidebarCollapsed(false, persistSetting: true);
         CollapseAllExpandersExcept(expAlarms);
      }

      private void btnExpandPlayback_Click(object sender, RoutedEventArgs e)
      {
         SetSidebarCollapsed(false, persistSetting: true);
         CollapseAllExpandersExcept(expPlaybackSelection);
      }

      private void btnExpandDownload_Click(object sender, RoutedEventArgs e)
      {
         SetSidebarCollapsed(false, persistSetting: true);
         CollapseAllExpandersExcept(expDownloadRecording);
      }

      private void narrowSidebar_PointerEntered(object sender, Avalonia.Input.PointerEventArgs e)
      {
         // Auto-expand on hover
         if (sidebarGrid.Width == 56)
         {
            SetSidebarCollapsed(false, persistSetting: false);
            // Don't save the state - this is just a temporary hover expand
         }
      }

      private void fullSidebar_PointerExited(object sender, Avalonia.Input.PointerEventArgs e)
      {
         // Auto-collapse on mouse leave if the sidebar was opened via hover (not manually toggled)
         if (AppSettings.Default.IsSidebarCollapsed && sidebarGrid.Width == 280)
         {
            SetSidebarCollapsed(true, persistSetting: false);
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
         if (menuItemLogin != null) menuItemLogin.IsEnabled = !IsStarted && !mIsConnecting;
         if (menuItemLogout != null) menuItemLogout.IsEnabled = IsStarted && !mIsConnecting;

         // Update tooltip with user, connection status, and server
          var status = mIsConnecting ? "Connecting" : (IsStarted ? "Connected" : "Disconnected");
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

      private void menuItemLogin_Click(object sender, RoutedEventArgs e)
      {
         var s = AppSettings.Default;
         // Check if credentials are configured
         if (string.IsNullOrWhiteSpace(s.Host1) || string.IsNullOrWhiteSpace(s.User) || string.IsNullOrWhiteSpace(s.Password))
         {
            // Open settings page first if not configured and connect after save.
            mPendingConnectAfterSettings = true;
            OpenSettingsPage();
         }
         else
         {
            // Credentials configured, just connect
            btnConnect_Click(sender, e);
         }
      }

      private void menuItemToggleTheme_Click(object sender, RoutedEventArgs e)
      {
         var app = (App)Avalonia.Application.Current;
         app.SetTheme(!IsDarkMode);
         RefreshMessageColors();
         RefreshVideoHeaderState();
         SaveSettings();
         UpdateThemeMenuLabel();
      }

      private void UpdateThemeMenuLabel()
      {
         if (txtToggleThemeLabel != null)
            txtToggleThemeLabel.Text = IsDarkMode ? "Switch to Light Mode" : "Switch to Dark Mode";
         if (txtToggleThemeIcon != null)
            txtToggleThemeIcon.Text = IsDarkMode ? "\uE51C" : "\uE518";
      }

      private void menuItemSettings_Click(object sender, RoutedEventArgs e)
      {
         OpenSettingsPage();
      }

      private void OpenSettingsPage()
      {
         var currentOverlay = navigationHost?.Content;
         if (currentOverlay is SettingsPage)
            return;

         var settingsPage = new SettingsPage
         {
            NavigationService = mNavigationService
         };
         mNavigationService.NavigateTo(settingsPage);
      }

      private void HandleEscapeNavigation()
      {
         var currentOverlay = navigationHost?.Content;

         if (currentOverlay is SettingsPage settingsPage)
         {
            settingsPage.CancelAndGoBack();
            return;
         }

         if (navigationHost?.IsVisible == true)
            mNavigationService.GoBack();
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
         if (menuItemLogin != null) menuItemLogin.IsEnabled = !IsStarted && !mIsConnecting;
         if (menuItemLogout != null) menuItemLogout.IsEnabled = IsStarted && !mIsConnecting;

         bool canUseConnectedFeatures = IsStarted && !mIsConnecting;
         pnlCameraSelectFlowPanel.IsEnabled = canUseConnectedFeatures;
         tglSubChannel.IsEnabled = canUseConnectedFeatures && !IsPlayback && mCurrentCamera != null && !string.IsNullOrEmpty(mCurrentCamera?.Stream2Resolution);
         grpSelectPreset.IsEnabled = canUseConnectedFeatures;
         grpSelectPlayback.IsEnabled = canUseConnectedFeatures && ApiSupportsPlayback;
         grpCameraControl.IsEnabled = canUseConnectedFeatures && mCurrentCamera != null;

         btnStopPlayback.IsEnabled = canUseConnectedFeatures && IsPlayback && ApiSupportsPlayback;
         btnPlayPlayback.IsEnabled = canUseConnectedFeatures && IsCameraSelected && !IsPlaybackStarted && ApiSupportsPlayback;
         btnGotoTime.IsEnabled = canUseConnectedFeatures && IsCameraSelected && ApiSupportsPlayback;
         btnDownload.IsEnabled = canUseConnectedFeatures && ApiSupportsPlayback;

         UpdateCameraControl();
      }

      private async void btnConnect_Click(object sender, RoutedEventArgs e)
      {
         if (mIsConnecting || IsStarted)
            return;

         SaveSettings();
         if (!ValidateCanConnect()) return;

         var s = AppSettings.Default;
         var connectClient = new FlexRApiClient
         {
            Host = s.Host1,
            Port = s.ApiPort,
            Password = s.Password,
            User = s.User,
            UseHttps = s.UseHttps,
            IgnoreCertificateErrors = true,
            ConnectionTimeoutMs = 8000
         };
         connectClient.OnMessage += OnFlexRApiClientMessage;
         txtVideoHeader.Text = "No Camera Selected";
         brdVideoHeader.Background = GetNeutralHeaderBrush();
         mIsConnecting = true;
         UpdateEnabled();
         UpdateUserInitial();
         WriteMessageLog(MessageSource.FlexApi, "Connecting to FLEX API...", LogLevel.Notice);

         bool started = false;
         User loggedInUser = null;
         List<Camera> cameraList = null;
         List<Alarm> alarmList = null;
         ApiVersion apiVersion = null;

         try
         {
            await Task.Run(() =>
            {
               started = connectClient.StartClient();
               if (!started)
                  return;

               loggedInUser = connectClient.GetLoggedInUserInfo();
               cameraList = connectClient.GetCameraList();
               alarmList = connectClient.GetAlarmList();
               apiVersion = connectClient.GetApiVersion();
            });

            if (started)
            {
               moFlexRApiClient = connectClient;
               IsStarted = true;
               WriteMessageLog(MessageSource.FlexApi, "Client started.", LogLevel.Notice);
               CurrentLoggedInUser = loggedInUser;
               FillSelectCameraButtonList(cameraList);
               FillSelectAlarmButtonList(alarmList);
               CheckApiVersion(apiVersion);
               ClearRecordingDropdown();
               ClearPresetDropdown();

               if (AppSettings.Default.CurrentCamera != 0)
                  SelectCamera(AppSettings.Default.CurrentCamera, AppSettings.Default.PreferSubChannel ? 2 : 1);
            }
            else
            {
               WriteMessageLog(MessageSource.FlexApi, "Unable to start, no response.", LogLevel.Error);
               connectClient.OnMessage -= OnFlexRApiClientMessage;
               connectClient.StopClient();
               ClearPresetDropdown();
            }
         }
         catch (Exception ex)
         {
            WriteMessageLog(MessageSource.FlexApi, $"Connection failed: {ex.Message}", LogLevel.Error);
            connectClient.OnMessage -= OnFlexRApiClientMessage;
            connectClient.StopClient();
            ClearPresetDropdown();
         }
         finally
         {
            mIsConnecting = false;
            UpdateEnabled();
            UpdateUserInitial();
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
         if (mIsConnecting)
            return;

         IsStarted = false;
         if (moFlexRApiClient != null)
         {
            moFlexRApiClient.OnMessage -= OnFlexRApiClientMessage;
            moFlexRApiClient.StopClient();
            moFlexRApiClient = null;
         }
         StopRtspStream();
         mActiveRtspUrl = null;
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
                  if (pnlVideo.Children.Contains(mVideoControl))
                     pnlVideo.Children.Remove(mVideoControl);
               mVideoControl = null;
            }
            mIsVideoStarted = false;
               mIsVideoTemporarilyDetached = false;
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

         bool hasPanTiltOrLens = (mCurrentCamera?.HasPanTiltControl ?? false) || (mCurrentCamera?.HasLensControl ?? false);
         btnAbsolutePosition.IsEnabled = hasPanTiltOrLens;
         btnGotoPreset.IsEnabled = hasPanTiltOrLens;
      }

      private void UpdateVideoHeaderTooltip(Camera camera, string rtspUrl, int streamNo)
      {
         if (camera == null)
         {
            ToolTip.SetTip(txtVideoHeader, null);
            return;
         }

         var capabilities = new List<string>();
         if (camera.HasPanTiltControl) capabilities.Add("Pan/Tilt");
         if (camera.HasLensControl) capabilities.Add("Lens (Zoom/Focus/Iris)");
         if (camera.HasWipeWashControl) capabilities.Add("Wipe/Wash");
         string capabilitiesText = capabilities.Count > 0 ? string.Join(", ", capabilities) : "None";

         string lockStatus = camera.IsLocked
            ? $"Locked by {camera.LockOwner ?? "Unknown"}"
            : "Unlocked";

         string videoHost = "N/A";
         try
         {
            var uri = new Uri(rtspUrl);
            videoHost = uri.Port > 0 ? $"{uri.Host}:{uri.Port}" : uri.Host;
         }
         catch { }

         string resolution = streamNo == 2
            ? (camera.Stream2Resolution ?? "N/A")
            : (camera.Stream1Resolution ?? "N/A");
         string channelName = streamNo == 2 ? "Sub" : "Main";

         string tooltip = $"Camera: {camera.Name} (#{camera.ComponentNumber})\n"
            + $"Resolution: {resolution} ({channelName})\n"
            + $"Capabilities: {capabilitiesText}\n"
            + $"Priority: {camera.Priority}\n"
            + $"Lock: {lockStatus}\n"
            + $"Video Server: {videoHost}";

         ToolTip.SetTip(txtVideoHeader, tooltip);
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
            // Evaluate sub channel availability after GetCameraLiveStreamUrl which refreshes camera data
            bool hasSubChannel = !string.IsNullOrEmpty(camera.Stream2Resolution);
            // If sub requested but not available, fall back to main
            if (streamNo == 2 && !hasSubChannel)
               streamNo = 1;
            if (!string.IsNullOrEmpty(url))
            {
               txtCurrentRtspUrl.Text = GetMaskedUrl(url);
                   txtVideoHeader.Text = $"LIVE - Camera {cameraNo}";
                   brdVideoHeader.Background = GetLiveHeaderBrush();
                   UpdateVideoHeaderTooltip(camera, url, streamNo);
                   StartRtspStream(url);
            }
            // Update toggle to reflect actual stream without triggering save
            mIsLoadingSettings = true;
            tglSubChannel.IsChecked = (streamNo == 2);
            tglSubChannel.IsEnabled = hasSubChannel && !IsPlayback;
            mIsLoadingSettings = false;
         }
      }

      private void NavigateToPreviousCamera()
      {
         if (!IsStarted || mCurrentCamera == null) return;

         List<Camera> cameraList = moFlexRApiClient.GetCameraList();
         if (cameraList == null || cameraList.Count == 0) return;

         int currentIndex = cameraList.FindIndex(c => c.ComponentNumber == mCurrentCamera.ComponentNumber);
         if (currentIndex < 0) return;

         int newIndex = currentIndex > 0 ? currentIndex - 1 : cameraList.Count - 1;
         int streamNo = tglSubChannel.IsChecked == true ? 2 : 1;
         SelectCamera(cameraList[newIndex].ComponentNumber, streamNo);
      }

      private void NavigateToNextCamera()
      {
         if (!IsStarted || mCurrentCamera == null) return;

         List<Camera> cameraList = moFlexRApiClient.GetCameraList();
         if (cameraList == null || cameraList.Count == 0) return;

         int currentIndex = cameraList.FindIndex(c => c.ComponentNumber == mCurrentCamera.ComponentNumber);
         if (currentIndex < 0) return;

         int newIndex = currentIndex < cameraList.Count - 1 ? currentIndex + 1 : 0;
         int streamNo = tglSubChannel.IsChecked == true ? 2 : 1;
         SelectCamera(cameraList[newIndex].ComponentNumber, streamNo);
      }

      private void SelectAlarm(int alarmNo)
      {
         Alarm alarm = moFlexRApiClient.GetSingleAlarm(alarmNo);
         if (alarm != null) CurrentAlarm = alarm;
      }

      private const int MaxCamerasPerSubmenu = 25;

      private List<object> BuildVideoContextMenuItems()
      {
         var rootItems = new List<object>();

         if (moFlexRApiClient == null)
         {
            rootItems.Add(new MenuItem { Header = "Not connected", IsEnabled = false });
            return rootItems;
         }

         List<Camera> cameraList = GetAvailableCamerasForMenu();
         if (cameraList == null || cameraList.Count == 0)
         {
            rootItems.Add(new MenuItem { Header = "No cameras available", IsEnabled = false });
            return rootItems;
         }

         if (cameraList.Count <= MaxCamerasPerSubmenu)
         {
            foreach (var camera in cameraList)
            {
               rootItems.Add(CreateCameraMenuItem(camera));
            }
         }
         else
         {
            for (int i = 0; i < cameraList.Count; i += MaxCamerasPerSubmenu)
            {
               int end = Math.Min(i + MaxCamerasPerSubmenu, cameraList.Count);
               var batch = cameraList.GetRange(i, end - i);
               var first = batch.First();
               var last = batch.Last();
               var rangeItems = new List<object>();
               var rangeMenu = new MenuItem { Header = $"{first.Name} – {last.Name}", ItemsSource = rangeItems };

               foreach (var camera in batch)
               {
                  rangeItems.Add(CreateCameraMenuItem(camera));
               }

               rootItems.Add(rangeMenu);
            }
         }
         return rootItems;
      }

      private List<Camera> GetAvailableCamerasForMenu()
      {
         var cameraList = moFlexRApiClient?.GetCameraList();
         if (cameraList != null && cameraList.Count > 0)
            return cameraList;

         // Fallback: use cameras already loaded in the sidebar selection panel.
         var fallback = pnlCameraSelectFlowPanel.Children
            .OfType<Button>()
            .Select(b => b.Tag as Camera)
            .Where(c => c != null)
            .GroupBy(c => c.ComponentNumber)
            .Select(g => g.First())
            .OrderBy(c => c.ComponentNumber)
            .ToList();

         return fallback;
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

      private void CheckApiVersion(ApiVersion apiversion = null)
      {
         apiversion ??= moFlexRApiClient.GetApiVersion();
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
         mActiveRtspUrl = rtspUrl;

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
         EnsureVideoContextMenu();
         if (mVideoControl == null)
         {
            mVideoControl = new VideoView();
         }

         if (!pnlVideo.Children.Contains(mVideoControl))
         {
            pnlVideo.Children.Add(mVideoControl);
         }

         mIsVideoTemporarilyDetached = false;
      }

      private void EnsureVideoContextMenu()
      {
         if (mVideoContextMenu != null)
            return;

         mVideoContextMenu = new ContextMenu();
      }

      private void VideoMenuButton_Click(object sender, RoutedEventArgs e)
      {
         try
         {
            EnsureVideoContextMenu();
            mVideoContextMenu.ItemsSource = BuildVideoContextMenuItems();
            var target = sender as Control ?? brdVideoHeader;
            target.ContextMenu = mVideoContextMenu;
            mVideoContextMenu.PlacementTarget = target;
            target.ContextMenu.Open();
         }
         catch (Exception ex)
         {
            WriteMessageLog(MessageSource.Config, $"Failed to open video menu: {ex.Message}", LogLevel.Error);
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
            var alarmPage = new AlarmActionPage(FlexRApiClient, CurrentAlarm, mCurrentLoggedInUser);
            alarmPage.NavigationService = mNavigationService;
            mNavigationService.NavigateTo(alarmPage);
         }
      }

          private void FillSelectCameraButtonList(List<Camera> cameraList = null)
      {
         ClearCameraSelection();
             cameraList ??= moFlexRApiClient.GetCameraList();
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

      private void FillSelectAlarmButtonList(List<Alarm> alarmList = null)
      {
         ClearAlarmSelection();
         alarmList ??= moFlexRApiClient.GetAlarmList();
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

      private void btnOpenDownloadWindow_Click(object sender, RoutedEventArgs e)
      {
         var downloadPage = new DownloadPage(FlexRApiClient);
         downloadPage.NavigationService = mNavigationService;
         mNavigationService.NavigateTo(downloadPage);
      }

      private void btnOpenAbsolutePositionWindow_Click(object sender, RoutedEventArgs e)
      {
         var positionPage = new AbsolutePositionPage(FlexRApiClient, mCurrentCamera);
         positionPage.NavigationService = mNavigationService;
         mNavigationService.NavigateTo(positionPage);
      }

      private void btnCameraLock_Click(object sender, RoutedEventArgs e)
      {
         if (!mCurrentCamera.IsLocked)
         {
            var lockPage = new CameraLockPage(FlexRApiClient, mCurrentCamera);
            lockPage.NavigationService = mNavigationService;
            mNavigationService.NavigateTo(lockPage);
         }
         else
         {
            FlexRApiClient.SendUnlockCamera(mCurrentCamera.ComponentNumber);
         }
      }

      private void btnPickDate_Click(object sender, RoutedEventArgs e)
      {
         mPickerOverlayMode = PickerOverlayMode.Date;
         txtPickerOverlayTitle.Text = "Select Date";
         txtPickerOverlaySubtitle.Text = "Choose a date for playback";

         overlayDatePicker.IsVisible = true;
         overlayTimePicker.IsVisible = false;
         overlayDatePicker.SelectedDate = DateTime.TryParse(txtDatePlayback.Text, out DateTime currentDate)
            ? new DateTimeOffset(currentDate)
            : new DateTimeOffset(DateTime.Now);

         ShowPickerOverlay();
      }

      private void btnPickTime_Click(object sender, RoutedEventArgs e)
      {
         mPickerOverlayMode = PickerOverlayMode.Time;
         txtPickerOverlayTitle.Text = "Select Time";
         txtPickerOverlaySubtitle.Text = "Choose a time for playback";

         overlayDatePicker.IsVisible = false;
         overlayTimePicker.IsVisible = true;
         overlayTimePicker.SelectedTime = TimeSpan.TryParse(txtTimePlayback.Text, out TimeSpan currentTime)
            ? currentTime
            : DateTime.Now.TimeOfDay;

         ShowPickerOverlay();
      }

      private void btnPickerOverlayCancel_Click(object sender, RoutedEventArgs e)
      {
         mPickerOverlayMode = PickerOverlayMode.None;
         HidePickerOverlay();
      }

      private void btnPickerOverlayApply_Click(object sender, RoutedEventArgs e)
      {
         if (mPickerOverlayMode == PickerOverlayMode.Date && overlayDatePicker.SelectedDate.HasValue)
         {
            txtDatePlayback.Text = overlayDatePicker.SelectedDate.Value.ToString("yyyy-MM-dd");
         }
         else if (mPickerOverlayMode == PickerOverlayMode.Time && overlayTimePicker.SelectedTime.HasValue)
         {
            txtTimePlayback.Text = overlayTimePicker.SelectedTime.Value.ToString(@"hh\:mm\:ss");
         }

         mPickerOverlayMode = PickerOverlayMode.None;
         HidePickerOverlay();
      }

      private void ShowPickerOverlay()
      {
         mIsPickerOverlayVisible = true;
         pickerOverlay.IsVisible = true;
         UpdateVideoSurfaceForOverlayState();
      }

      private void HidePickerOverlay()
      {
         mIsPickerOverlayVisible = false;
         pickerOverlay.IsVisible = false;
         overlayDatePicker.IsVisible = false;
         overlayTimePicker.IsVisible = false;
         UpdateVideoSurfaceForOverlayState();

         // Native video surfaces can require one extra layout tick to reliably
         // reattach after overlay close on some platforms.
         if (!mIsNavigationOverlayVisible)
         {
            Dispatcher.UIThread.Post(() =>
            {
               if (!mIsNavigationOverlayVisible && !mIsPickerOverlayVisible)
               {
                  ReattachVideoSurfaceAfterOverlay();
               }
            }, DispatcherPriority.Background);
         }
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
