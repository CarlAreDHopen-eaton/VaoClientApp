using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.VisualTree;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using LibVLCSharp.Shared;
using Vao.Client;
using Vao.Client.Components;
using Vao.Client.Enum;
using Vao.Sample.Navigation;
using Vao.Sample.Pages;

namespace Vao.Sample
{
   public partial class MainView : UserControl
   {
      // ── Connection state ───────────────────────────────────────────────────

      private bool mIsStarted;
      private bool mIsCameraSelected;
      private bool mIsPlaybackStarted;
      private bool mApiSupportsPlayback;
      private ApiVersion mApiVersion;
      private ImplementationVersion mImplementationVersion;
      private FlexApiClient mFlexApiClient;
      private Guid mViewerID = Guid.NewGuid();
      private Camera mCurrentCamera;
      private Alarm mCurrentAlarm;
      private User mCurrentLoggedInUser;

      // ── UI state ───────────────────────────────────────────────────────────

      private bool mIsUpdatingCameraSelection;
      private bool mIsUpdatingAlarmSelection;
      private bool mIsResizingSidebarMenu;
      private bool mIsLoadingSettings;
      private bool mIsMessagesCollapsed;
      private bool mIsNavigationOverlayVisible;
      private bool mIsPickerOverlayVisible;
      private bool mIsConnecting;
      private bool mPendingConnectAfterSettings;
      private string mConnectedEndpointDisplay = string.Empty;
      private ListBox mActiveResizableList;
      private double mResizeStartHeight;
      private Point mResizeStartPoint;
      private GridLength mMessagesExpandedRowHeight = new GridLength(1, GridUnitType.Star);
      private ContextMenu mVideoContextMenu;
      private string mActiveRtspUrl;

      private App mApp;

      private const double C_SIDEBAR_AUTO_COLLAPSE_BREAKPOINT = 1100;
      private const double C_COLLAPSED_SIDEBAR_WIDTH = 56;

      private enum PickerOverlayMode { None, Date, Time }
      private PickerOverlayMode mPickerOverlayMode = PickerOverlayMode.None;

      // ── Collections ────────────────────────────────────────────────────────

      private ObservableCollection<MessageItem> mMessages = new();
      private ObservableCollection<MessageItem> mFilteredMessages = new();
      private readonly ObservableCollection<CameraSelectionItem> mCameraSelectionItems = new();
      private readonly ObservableCollection<CameraSelectionItem> mFilteredCameraSelectionItems = new();
      private readonly ObservableCollection<AlarmSelectionItem> mAlarmSelectionItems = new();
      private readonly ObservableCollection<AlarmSelectionItem> mFilteredAlarmSelectionItems = new();

      private Dictionary<MessageSource, bool> mSourceFilters = new()
      {
         { MessageSource.FlexApi, true },
         { MessageSource.LibVlc, true },
         { MessageSource.Config, true }
      };

      private Dictionary<AlarmGeneralStatus, bool> mAlarmStatusFilters = new()
      {
         { AlarmGeneralStatus.Active, true },
         { AlarmGeneralStatus.Inactive, true },
         { AlarmGeneralStatus.Acknowledged, true },
         { AlarmGeneralStatus.Tampered, true },
         { AlarmGeneralStatus.Disabled, true },
         { AlarmGeneralStatus.Unknown, true }
      };

      // ── Public events (desktop subscribes for VLC management) ──────────────

      /// <summary>Fired when an RTSP stream should start (non-null URL) or stop (null).</summary>
      public event EventHandler<string> RtspStreamRequested;

      /// <summary>Fired when any overlay becomes visible (true) or all overlays hide (false).
      /// Desktop uses this to detach/reattach the native VideoView.</summary>
      public event EventHandler<bool> AnyOverlayStateChanged;

      /// <summary>Fired when the connection state changes (connected or disconnected).</summary>
      public event EventHandler ConnectionStateChanged;

      // ── Public properties ──────────────────────────────────────────────────

      public Panel VideoSlot => pnlVideo;

      public bool IsStarted
      {
         get => mIsStarted;
         set { mIsStarted = value; UpdateEnabled(); ConnectionStateChanged?.Invoke(this, EventArgs.Empty); }
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

      public bool IsCameraSelected
      {
         get => mIsCameraSelected;
         set { mIsCameraSelected = value; UpdateEnabled(); }
      }

      public bool IsPlayback => IsActiveRtspPlayback(mActiveRtspUrl);

      // ── Public methods (called by MainWindow thin shell) ───────────────────

      public void ToggleSidebar() => btnToggleSidebar_Click(null, null);

      public void HandleWindowResize(double width) => ApplyResponsiveSidebarLayout(width);

      public void SaveCurrentSettings() => SaveSettings();

      public void CycleThemeFromKeyboard()
      {
         if (mApp != null)
            CycleThemeAndRefreshUi();
      }

      public void OpenSettingsFromKeyboard() => OpenSettingsPage();

      public void HandleEscapeKey() => HandleEscapeNavigation();

      public void HandleRenameCameraKey() => HandleRenameCameraAsync();

      public void NavigatePreviousCamera() => NavigateToPreviousCamera();

      public void NavigateNextCamera() => NavigateToNextCamera();

      public void SelectCameraHotkey(int cameraNo)
      {
         if (!IsStarted || mFlexApiClient == null) return;
         int streamNo = tglSubChannel?.IsChecked == true ? 2 : 1;
         SelectCamera(cameraNo, streamNo);
      }

      public void AssignCameraHotkey(int slot)
      {
         if (mCurrentCamera == null) return;
         AppSettings.Default.CameraHotkeys[slot] = mCurrentCamera.ComponentNumber;
         SaveSettings();
         WriteMessageLog(MessageSource.Config, $"Camera {mCurrentCamera.ComponentNumber} assigned to slot {slot} (Ctrl+{slot})", LogLevel.Notice);
      }

      public int GetSubChannelStreamNo() => tglSubChannel?.IsChecked == true ? 2 : 1;

      /// <summary>Called by MainWindow once the window is open and sized to restore all saved UI state.</summary>
      public void OnWindowOpened(double windowWidth)
      {
         UpdateThemeMenus();
         var s = AppSettings.Default;
         expCameraControl.IsExpanded = s.IsCameraControlExpanded;
         expCameraSelection.IsExpanded = s.IsCameraSelectionExpanded;
         expPresetSelection.IsExpanded = s.IsPresetSelectionExpanded;
         expAlarms.IsExpanded = s.IsAlarmsExpanded;
         expPlaybackSelection.IsExpanded = s.IsPlaybackSelectionExpanded;
         expDownloadRecording.IsExpanded = s.IsDownloadRecordingExpanded;

         SetSidebarCollapsed(s.IsSidebarCollapsed, persistSetting: false);
         ApplyResponsiveSidebarLayout(windowWidth);

         if (brdMessages?.Parent is Grid mg && mg.RowDefinitions.Count > 2)
         {
            mg.RowDefinitions[0].Height = new GridLength(s.MessagesSplitVideoStars, GridUnitType.Star);
            mg.RowDefinitions[2].Height = new GridLength(s.MessagesSplitMessagesStars, GridUnitType.Star);
         }
         if (s.IsMessagesCollapsed)
         {
            mIsMessagesCollapsed = false;
            MessagesHeader_PointerPressed(null, null);
         }

         if (s.AutoConnectOnStartup)
            btnConnect_Click(null, null);
      }

      // ── Constructor ────────────────────────────────────────────────────────

      public MainView()
      {
         InitializeComponent();
         EnsureVideoContextMenu();

         lstMessages.ItemsSource = mFilteredMessages;
         lstCameraSelection.ItemsSource = mFilteredCameraSelectionItems;
         lstAlarmSelection.ItemsSource = mFilteredAlarmSelectionItems;

         InitializeAlarmStatusFilterMenu();
         InitializeSidebarMenuHeights(useSavedHeights: true);
         RegisterPtzButtonHandlers();
         ClearPresetDropdown();
         ClearRecordingDropdown();

         InitializeNavigationService();

         mIsLoadingSettings = true;
         LoadSettings();
         mIsLoadingSettings = false;

         UpdateEnabled();
         UpdateUserInitial();

         mApp = (App)Application.Current;
         if (mApp != null)
            mApp.ThemeApplied += OnThemeApplied;
      }

      // ── Theme applied ──────────────────────────────────────────────────────

      private void OnThemeApplied(ThemeDefinition theme)
      {
         var sidebarWasCollapsed = IsSidebarCurrentlyCollapsed();

         if (mainTopBar != null)
         {
            mainTopBar.Height = theme.Components.AppBar.Height;
            mainTopBar.MinHeight = theme.Components.AppBar.Height;
            mainTopBar.MaxHeight = theme.Components.AppBar.Height;
         }

         var appBarToggleButton = this.FindControl<Button>("btnToggleSidebarAppBar");
         if (appBarToggleButton != null)
         {
            appBarToggleButton.MinHeight = theme.Sizing.Button.MinHeight;
            appBarToggleButton.MinWidth = theme.Sizing.Button.MinHeight;
         }

         var appTitle = this.FindControl<TextBlock>("txtAppTitle");
         if (appTitle != null)
            appTitle.FontSize = theme.Sizing.Typography.PageTitle;

         var appSubtitle = this.FindControl<TextBlock>("txtAppSubtitle");
         if (appSubtitle != null)
            appSubtitle.FontSize = theme.Sizing.Typography.PageSubtitle;

         SetSidebarCollapsed(sidebarWasCollapsed, persistSetting: false);
         ApplyResponsiveSidebarLayout(Bounds.Width);
         SetMainChromeVisible(!mIsNavigationOverlayVisible);

         if (btnUserProfile != null)
         {
            btnUserProfile.Width = theme.Sizing.Avatar.Size;
            btnUserProfile.Height = theme.Sizing.Avatar.Size;
            btnUserProfile.CornerRadius = new CornerRadius(theme.Sizing.Avatar.CornerRadius);
         }

         foreach (var btn in this.GetVisualDescendants().OfType<Button>())
            btn.MinHeight = theme.Sizing.Button.MinHeight;

         var menuHeaderPadding = new Thickness(16, theme.Components.Sidebar.SidebarMenuHeaderPaddingVertical);
         foreach (var expander in this.GetVisualDescendants().OfType<Expander>())
         {
            var toggle = expander.GetVisualDescendants()
                                 .OfType<ToggleButton>()
                                 .FirstOrDefault(t => t.Name == "PART_toggle");
            if (toggle == null) continue;
            toggle.MinHeight = theme.Components.Sidebar.SidebarMenuHeaderMinHeight;
            toggle.Padding = menuHeaderPadding;
            toggle.FontSize = theme.Components.Sidebar.SidebarMenuHeaderFontSize;
         }

         InitializeSidebarMenuHeights(useSavedHeights: true);
      }

      private void InitializeSidebarMenuHeights(bool useSavedHeights)
      {
         var defaultHeight = AppConstants.Default.ResizableSidebarMenuDefaultHeight;
         var minHeight = AppConstants.Default.ResizableSidebarMenuMinHeight;
         var maxHeight = AppConstants.Default.ResizableSidebarMenuMaxHeight;
         var settings = AppSettings.Default;

         double ResolveTargetHeight(double savedHeight, double currentHeight)
         {
            var preferred = useSavedHeights ? savedHeight : currentHeight;
            if (preferred <= 0) preferred = defaultHeight;
            return Math.Clamp(preferred, minHeight, maxHeight);
         }

         var cameraTarget = ResolveTargetHeight(settings.CameraSidebarMenuHeight, lstCameraSelection?.Height ?? 0);
         var alarmTarget = ResolveTargetHeight(settings.AlarmSidebarMenuHeight, lstAlarmSelection?.Height ?? 0);

         if (lstCameraSelection != null)
         {
            lstCameraSelection.MinHeight = minHeight;
            lstCameraSelection.MaxHeight = maxHeight;
            lstCameraSelection.Height = cameraTarget;
         }

         if (lstAlarmSelection != null)
         {
            lstAlarmSelection.MinHeight = minHeight;
            lstAlarmSelection.MaxHeight = maxHeight;
            lstAlarmSelection.Height = alarmTarget;
         }
      }

      // ── Navigation service ─────────────────────────────────────────────────

      private NavigationService mNavigationService;

      private void InitializeNavigationService()
      {
         mNavigationService = new NavigationService();
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
               if (!IsStarted && s.HasAnyConnectionAlternative() && !string.IsNullOrWhiteSpace(s.User) && !string.IsNullOrWhiteSpace(s.Password))
                  btnConnect_Click(null, null);
            }
         }
      }

      private void SetNavigationOverlayState(bool isOverlayVisible)
      {
         mIsNavigationOverlayVisible = isOverlayVisible;
         SetMainChromeVisible(!isOverlayVisible);
         UpdateVideoSurfaceForOverlayState();
      }

      private void SetMainChromeVisible(bool isVisible)
      {
         if (mainTopBar != null)
            mainTopBar.IsVisible = isVisible;

         if (rootLayoutGrid != null && rootLayoutGrid.RowDefinitions.Count > 0)
         {
            var appBarHeight = mApp?.CurrentTheme?.Components?.AppBar?.Height ?? 64d;
            rootLayoutGrid.RowDefinitions[0].Height = isVisible
               ? new GridLength(appBarHeight)
               : new GridLength(0);
         }
      }

      private void UpdateVideoSurfaceForOverlayState()
      {
         bool anyOverlay = mIsNavigationOverlayVisible || mIsPickerOverlayVisible;
         AnyOverlayStateChanged?.Invoke(this, anyOverlay);
      }

      // ── Connect / Disconnect ───────────────────────────────────────────────

      private async void btnConnect_Click(object sender, RoutedEventArgs e)
      {
         if (mIsConnecting || IsStarted) return;

         SaveSettings();
         if (!ValidateCanConnect()) return;

         var s = AppSettings.Default;
         var endpoints = s.GetConnectionAlternatives()
            .Where(c => !string.IsNullOrWhiteSpace(c.Host) && !string.IsNullOrWhiteSpace(c.Port))
            .ToList();

         if (endpoints.Count == 0)
         {
            WriteMessageLog(MessageSource.Config, "No valid host/port alternatives configured", LogLevel.Error);
            return;
         }

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
         FlexApiClient connectClient = null;
         ConnectionAlternative connectedEndpoint = null;

         try
         {
            foreach (var endpoint in endpoints)
            {
               var candidateClient = new FlexApiClient
               {
                  Host = endpoint.Host,
                  Port = endpoint.Port,
                  Password = s.Password,
                  User = s.User,
                  UseHttps = s.UseHttps,
                  IgnoreCertificateErrors = true,
                  ConnectionTimeoutMs = 8000
               };
               candidateClient.OnMessage += OnFlexApiClientMessage;

               WriteMessageLog(MessageSource.FlexApi, $"Trying {endpoint.Host}:{endpoint.Port}...", LogLevel.Notice);

               try
               {
                  await Task.Run(() =>
                  {
                     started = candidateClient.StartClient();
                     if (!started) return;
                     loggedInUser = candidateClient.GetLoggedInUserInfo();
                     cameraList = candidateClient.GetCameraList();
                     alarmList = candidateClient.GetAlarmList();
                     apiVersion = candidateClient.GetApiVersion();
                  });

                  if (started)
                  {
                     connectClient = candidateClient;
                     connectedEndpoint = endpoint;
                     break;
                  }
               }
               catch (Exception ex)
               {
                  WriteMessageLog(MessageSource.FlexApi, $"Connection attempt failed for {endpoint.Host}:{endpoint.Port}: {ex.Message}", LogLevel.Warning);
               }

               candidateClient.OnMessage -= OnFlexApiClientMessage;
               candidateClient.StopClient();
            }

            if (started)
            {
               mFlexApiClient = connectClient;
               mConnectedEndpointDisplay = $"{connectedEndpoint?.Host}:{connectedEndpoint?.Port}";
               if (connectedEndpoint != null && s.PromoteConnectionAlternativeToTop(connectedEndpoint.Host, connectedEndpoint.Port))
                  s.Save();

               IsStarted = true;
               WriteMessageLog(MessageSource.FlexApi, $"Client started on {mConnectedEndpointDisplay}.", LogLevel.Notice);
               CurrentLoggedInUser = loggedInUser;
               FillSelectCameraButtonList(cameraList);
               FillSelectAlarmButtonList(alarmList);
               CheckApiVersion(apiVersion);
               ClearRecordingDropdown();
               ClearPresetDropdown();
               UpdateConnectionStatus(connectedEndpoint?.Host, connectedEndpoint?.Port);

               if (AppSettings.Default.CurrentCamera != 0)
                  SelectCamera(AppSettings.Default.CurrentCamera, AppSettings.Default.PreferSubChannel ? 2 : 1);
            }
            else
            {
               WriteMessageLog(MessageSource.FlexApi, "Unable to start, no response from any configured host.", LogLevel.Error);
               ClearPresetDropdown();
            }
         }
         catch (Exception ex)
         {
            WriteMessageLog(MessageSource.FlexApi, $"Connection failed: {ex.Message}", LogLevel.Error);
            if (connectClient != null)
            {
               connectClient.OnMessage -= OnFlexApiClientMessage;
               connectClient.StopClient();
            }
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
         if (!s.HasAnyConnectionAlternative())
         { WriteMessageLog(MessageSource.Config, "Missing host name", LogLevel.Error); return false; }
         if (string.IsNullOrWhiteSpace(s.Password))
         { WriteMessageLog(MessageSource.Config, "Missing host password", LogLevel.Error); return false; }
         if (string.IsNullOrWhiteSpace(s.User))
         { WriteMessageLog(MessageSource.Config, "Missing user name", LogLevel.Error); return false; }
         return true;
      }

      private void btnDisconnect_Click(object sender, RoutedEventArgs e)
      {
         if (mIsConnecting) return;

         IsStarted = false;
         if (mFlexApiClient != null)
         {
            mFlexApiClient.OnMessage -= OnFlexApiClientMessage;
            mFlexApiClient.StopClient();
            mFlexApiClient = null;
         }

         mConnectedEndpointDisplay = string.Empty;
         mApiVersion = null;
         mImplementationVersion = null;

         RtspStreamRequested?.Invoke(this, null);
         mActiveRtspUrl = null;
         txtCurrentRtspUrl.Text = string.Empty;
         txtVideoHeader.Text = "No Camera Selected";
         brdVideoHeader.Background = GetNeutralHeaderBrush();
         if (txtAppSubtitle != null) txtAppSubtitle.Text = "Not connected";

         IsCameraSelected = false;
         CurrentCamera = null;
         CurrentAlarm = null;
         ClearPresetDropdown();
         ClearRecordingDropdown();
         ClearCameraSelection();
         ClearAlarmSelection();
         UpdateUserInitial();
      }

      private void OnFlexApiClientMessage(object sender, MessageEventArgs e)
      {
         if (e.StatusMessage is StatusMessage statusMessage)
         {
            string logText = $"{statusMessage.Timestamp} : [{statusMessage.Type}] {statusMessage.Message}";
            switch (statusMessage.Level)
            {
               case MessageLevel.Debug:   WriteMessageLog(MessageSource.FlexApi, logText, LogLevel.Debug); break;
               case MessageLevel.Info:    WriteMessageLog(MessageSource.FlexApi, logText, LogLevel.Notice); break;
               case MessageLevel.Warning: WriteMessageLog(MessageSource.FlexApi, logText, LogLevel.Warning); break;
               case MessageLevel.Error:   WriteMessageLog(MessageSource.FlexApi, logText, LogLevel.Error); break;
            }
         }
      }

      private void UpdateConnectionStatus(string host, string port)
      {
         if (txtAppSubtitle != null)
            txtAppSubtitle.Text = IsStarted ? $"Connected · {host}:{port}" : "Not connected";
      }

      // ── UpdateEnabled ──────────────────────────────────────────────────────

      public void UpdateEnabled()
      {
         var menuItemLogin = this.FindControl<MenuItem>("menuItemLogin");
         var menuItemLogout = this.FindControl<MenuItem>("menuItemLogout");
         if (menuItemLogin != null) menuItemLogin.IsEnabled = !IsStarted && !mIsConnecting;
         if (menuItemLogout != null) menuItemLogout.IsEnabled = IsStarted && !mIsConnecting;

         bool canUseConnectedFeatures = IsStarted && !mIsConnecting;
         if (pnlCameraSelection != null) pnlCameraSelection.IsEnabled = canUseConnectedFeatures;
         if (pnlAlarmsSelection != null) pnlAlarmsSelection.IsEnabled = canUseConnectedFeatures;
         if (tglSubChannel != null) tglSubChannel.IsEnabled = canUseConnectedFeatures && !IsPlayback && mCurrentCamera != null && !string.IsNullOrEmpty(mCurrentCamera?.Stream2Resolution);
         if (grpSelectPreset != null) grpSelectPreset.IsEnabled = canUseConnectedFeatures;
         if (grpSelectPlayback != null) grpSelectPlayback.IsEnabled = canUseConnectedFeatures && ApiSupportsPlayback;
         if (grpCameraControl != null) grpCameraControl.IsEnabled = canUseConnectedFeatures && mCurrentCamera != null;

         if (btnStopPlayback != null) btnStopPlayback.IsEnabled = canUseConnectedFeatures && IsPlayback && ApiSupportsPlayback;
         if (btnPlayPlayback != null) btnPlayPlayback.IsEnabled = canUseConnectedFeatures && IsCameraSelected && !IsPlaybackStarted && ApiSupportsPlayback;
         if (btnGotoTime != null) btnGotoTime.IsEnabled = canUseConnectedFeatures && IsCameraSelected && ApiSupportsPlayback;
         if (btnDownload != null) btnDownload.IsEnabled = canUseConnectedFeatures && ApiSupportsPlayback;

         UpdateCameraControl();
      }

      // ── Camera ─────────────────────────────────────────────────────────────

      private Camera CurrentCamera
      {
         get => mCurrentCamera;
         set
         {
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

            SyncCameraSelectionWithCurrent();

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
            var iconLock = this.FindControl<TextBlock>("iconCameraLock");
            if (mCurrentCamera != null && mCurrentCamera.IsLocked && mCurrentCamera.LockOwner == "Alarm")
            {
               if (iconLock != null) { iconLock.Text = "\uE899"; iconLock.Foreground = GetBrushResource("CameraLockAlarmForeground", "#CA3C3D"); }
               if (btnCameraLock != null) btnCameraLock.IsEnabled = true;
            }
            else if (mCurrentCamera != null && mCurrentCamera.IsLocked)
            {
               if (iconLock != null) { iconLock.Text = "\uE899"; iconLock.Foreground = GetBrushResource("CameraLockManualForeground", "#F0AA1F"); }
               if (btnCameraLock != null) btnCameraLock.IsEnabled = true;
            }
            else if (mCurrentCamera != null && !mCurrentCamera.IsLocked)
            {
               if (iconLock != null)
               {
                  iconLock.Text = "\uE898";
                  if (this.TryFindResource("SidebarHeaderFg", this.ActualThemeVariant, out var brush) && brush is IBrush b)
                     iconLock.Foreground = b;
               }
               if (btnCameraLock != null) btnCameraLock.IsEnabled = true;
            }

            if (mCurrentCamera != null && mCurrentCamera.CanUnlock == false)
               if (btnCameraLock != null) btnCameraLock.IsEnabled = false;
         });
      }

      private void UpdateCameraControl()
      {
         if (btnPanLeft != null) btnPanLeft.IsEnabled = mCurrentCamera?.HasPanTiltControl ?? false;
         if (btnPanRight != null) btnPanRight.IsEnabled = mCurrentCamera?.HasPanTiltControl ?? false;
         if (btnTiltDown != null) btnTiltDown.IsEnabled = mCurrentCamera?.HasPanTiltControl ?? false;
         if (btnTiltUp != null) btnTiltUp.IsEnabled = mCurrentCamera?.HasPanTiltControl ?? false;
         if (btnZoomIn != null) btnZoomIn.IsEnabled = mCurrentCamera?.HasLensControl ?? false;
         if (btnZoomOut != null) btnZoomOut.IsEnabled = mCurrentCamera?.HasLensControl ?? false;
         if (btnFocusFar != null) btnFocusFar.IsEnabled = mCurrentCamera?.HasLensControl ?? false;
         if (btnFocusNear != null) btnFocusNear.IsEnabled = mCurrentCamera?.HasLensControl ?? false;

         bool hasPanTiltOrLens = (mCurrentCamera?.HasPanTiltControl ?? false) || (mCurrentCamera?.HasLensControl ?? false);
         if (btnAbsolutePosition != null) btnAbsolutePosition.IsEnabled = hasPanTiltOrLens;
         if (btnGotoPreset != null) btnGotoPreset.IsEnabled = hasPanTiltOrLens;
      }

      private void FillSelectCameraButtonList(List<Camera> cameraList = null)
      {
         ClearCameraSelection();
         cameraList ??= mFlexApiClient?.GetCameraList();
         if (cameraList == null) return;
         foreach (var camera in cameraList.OrderBy(c => c.ComponentNumber))
            mCameraSelectionItems.Add(new CameraSelectionItem(camera));
         ApplyCameraSearchFilter();
      }

      private void ClearCameraSelection()
      {
         mCameraSelectionItems.Clear();
         mFilteredCameraSelectionItems.Clear();
         mIsUpdatingCameraSelection = true;
         if (lstCameraSelection != null) lstCameraSelection.SelectedItem = null;
         mIsUpdatingCameraSelection = false;
         if (txtCameraSearch != null) txtCameraSearch.Text = string.Empty;
      }

      private void txtCameraSearch_TextChanged(object sender, TextChangedEventArgs e) => ApplyCameraSearchFilter();

      private void ApplyCameraSearchFilter()
      {
         string query = txtCameraSearch?.Text?.Trim() ?? string.Empty;
         var filtered = string.IsNullOrWhiteSpace(query)
            ? mCameraSelectionItems
            : mCameraSelectionItems.Where(item => item.Matches(query));

         mFilteredCameraSelectionItems.Clear();
         foreach (var item in filtered)
            mFilteredCameraSelectionItems.Add(item);

         SyncCameraSelectionWithCurrent();
      }

      private void SyncCameraSelectionWithCurrent()
      {
         mIsUpdatingCameraSelection = true;
         if (lstCameraSelection != null)
         {
            lstCameraSelection.SelectedItem = mCurrentCamera == null
               ? null
               : mFilteredCameraSelectionItems.FirstOrDefault(item => item.Camera?.ComponentNumber == mCurrentCamera.ComponentNumber);
            if (lstCameraSelection.SelectedItem != null)
               lstCameraSelection.ScrollIntoView(lstCameraSelection.SelectedItem);
         }
         mIsUpdatingCameraSelection = false;
      }

      private void lstCameraSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
      {
         if (mIsUpdatingCameraSelection) return;
         if (lstCameraSelection?.SelectedItem is not CameraSelectionItem selected || selected.Camera == null) return;
         int streamNo = tglSubChannel?.IsChecked == true ? 2 : 1;
         SelectCamera(selected.Camera.ComponentNumber, streamNo);
      }

      private void SelectCamera(int cameraNo, int streamNo)
      {
         if (mFlexApiClient == null) return;
         var camera = mFlexApiClient.GetCamera(cameraNo);
         if (camera == null) return;

         CurrentCamera = camera;

         string url = camera.GetCameraLiveStreamUrl(streamNo);
         bool hasSubChannel = !string.IsNullOrEmpty(camera.Stream2Resolution);
         if (streamNo == 2 && !hasSubChannel) streamNo = 1;

         if (!string.IsNullOrEmpty(url))
         {
            mActiveRtspUrl = url;
            if (txtCurrentRtspUrl != null) txtCurrentRtspUrl.Text = GetMaskedUrl(url);
            if (txtVideoHeader != null) txtVideoHeader.Text = $"LIVE - Camera {cameraNo}";
            if (brdVideoHeader != null) brdVideoHeader.Background = GetLiveHeaderBrush();
            RtspStreamRequested?.Invoke(this, url);
         }

         mIsLoadingSettings = true;
         if (tglSubChannel != null)
         {
            tglSubChannel.IsChecked = (streamNo == 2);
            tglSubChannel.IsEnabled = hasSubChannel && !IsPlayback;
         }
         mIsLoadingSettings = false;
      }

      private void SelectAlarm(int alarmNo)
      {
         if (mFlexApiClient == null) return;
         var alarm = mFlexApiClient.GetSingleAlarm(alarmNo);
         if (alarm != null) CurrentAlarm = alarm;
      }

      private void NavigateToPreviousCamera()
      {
         if (!IsStarted || mCurrentCamera == null || mFlexApiClient == null) return;
         var cameraList = mFlexApiClient.GetCameraList();
         if (cameraList == null || cameraList.Count == 0) return;
         int idx = cameraList.FindIndex(c => c.ComponentNumber == mCurrentCamera.ComponentNumber);
         if (idx < 0) return;
         int newIdx = idx > 0 ? idx - 1 : cameraList.Count - 1;
         SelectCamera(cameraList[newIdx].ComponentNumber, GetSubChannelStreamNo());
      }

      private void NavigateToNextCamera()
      {
         if (!IsStarted || mCurrentCamera == null || mFlexApiClient == null) return;
         var cameraList = mFlexApiClient.GetCameraList();
         if (cameraList == null || cameraList.Count == 0) return;
         int idx = cameraList.FindIndex(c => c.ComponentNumber == mCurrentCamera.ComponentNumber);
         if (idx < 0) return;
         int newIdx = idx < cameraList.Count - 1 ? idx + 1 : 0;
         SelectCamera(cameraList[newIdx].ComponentNumber, GetSubChannelStreamNo());
      }

      // ── Preset ─────────────────────────────────────────────────────────────

      private void FillSelectPresetList()
      {
         var presets = mCurrentCamera?.PresetList;
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

      private void ClearPresetDropdown()
      {
         if (lblCurrentCamera != null) lblCurrentCamera.Text = "Camera : (No camera selected)";
         if (selPreset != null)
         {
            selPreset.ItemsSource = null;
            selPreset.ItemsSource = new List<string> { "No camera selected" };
            selPreset.SelectedIndex = 0;
         }
      }

      private void btnGotoPreset_Click(object sender, RoutedEventArgs e)
      {
         if (selPreset?.SelectedItem is Preset preset) preset.GotoPreset();
      }

      // ── Alarms ─────────────────────────────────────────────────────────────

      private void FillSelectAlarmButtonList(List<Alarm> alarmList = null)
      {
         ClearAlarmSelection();
         alarmList ??= mFlexApiClient?.GetAlarmList();
         if (alarmList == null) return;

         foreach (var alarm in alarmList.OrderBy(a => a.ComponentNumber))
         {
            var item = new AlarmSelectionItem(alarm, GetBrushForStatus(alarm.Status), GetTextForStatus(alarm.Status))
            {
               AlarmIcon = GetIconForAlarmStatus(alarm.Status)
            };
            mAlarmSelectionItems.Add(item);
            alarm.PropertyChanged += (s, ev) =>
            {
               if (ev.PropertyName == nameof(Alarm.Status))
               {
                  Dispatcher.UIThread.Post(() =>
                  {
                     item.StatusBrush = GetBrushForStatus(alarm.Status);
                     item.StatusText = GetTextForStatus(alarm.Status);
                     item.AlarmIcon = GetIconForAlarmStatus(alarm.Status);
                     UpdateAlarmSidebarIcon();
                  });
               }
            };
         }

         ApplyAlarmSearchFilter();
         UpdateAlarmSidebarIcon();
      }

      private void ClearAlarmSelection()
      {
         mAlarmSelectionItems.Clear();
         mFilteredAlarmSelectionItems.Clear();
         mIsUpdatingAlarmSelection = true;
         if (lstAlarmSelection != null) lstAlarmSelection.SelectedItem = null;
         mIsUpdatingAlarmSelection = false;
         if (txtAlarmSearch != null) txtAlarmSearch.Text = string.Empty;
      }

      private void txtAlarmSearch_TextChanged(object sender, TextChangedEventArgs e) => ApplyAlarmSearchFilter();

      private void lstAlarmSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
      {
         if (mIsUpdatingAlarmSelection) return;
         if (lstAlarmSelection?.SelectedItem is not AlarmSelectionItem selected || selected.Alarm == null) return;
         SelectAlarm(selected.Alarm.ComponentNumber);
      }

      private void btnAlarmEdit_Click(object sender, RoutedEventArgs e)
      {
         if (sender is not Button btn) return;
         var item = btn.DataContext as AlarmSelectionItem;
         if (item?.Alarm == null) return;
         SelectAlarm(item.Alarm.ComponentNumber);
         var alarmPage = new AlarmActionPage(CurrentAlarm)
         {
            NavigationService = mNavigationService
         };
         mNavigationService.NavigateTo(alarmPage);
      }

      private void ApplyAlarmSearchFilter()
      {
         string query = txtAlarmSearch?.Text?.Trim() ?? string.Empty;
         var filtered = string.IsNullOrWhiteSpace(query)
            ? mAlarmSelectionItems.AsEnumerable()
            : mAlarmSelectionItems.Where(item => item.Matches(query));

         filtered = filtered.Where(item => item.Alarm != null && mAlarmStatusFilters.GetValueOrDefault(item.Alarm.Status, true));

         mFilteredAlarmSelectionItems.Clear();
         foreach (var item in filtered)
            mFilteredAlarmSelectionItems.Add(item);

         mIsUpdatingAlarmSelection = true;
         if (lstAlarmSelection != null)
         {
            lstAlarmSelection.SelectedItem = mCurrentAlarm == null
               ? null
               : mFilteredAlarmSelectionItems.FirstOrDefault(item => item.Alarm?.ComponentNumber == mCurrentAlarm.ComponentNumber);
         }
         mIsUpdatingAlarmSelection = false;
      }

      private void AlarmStatusFilter_Click(object sender, RoutedEventArgs e)
      {
         if (sender is not MenuItem menuItem || menuItem.Tag is not string tagValue) return;
         if (!Enum.TryParse<AlarmGeneralStatus>(tagValue, ignoreCase: true, out var status)) return;
         mAlarmStatusFilters[status] = !mAlarmStatusFilters[status];
         menuItem.Icon = mAlarmStatusFilters[status] ? new TextBlock { Text = "\u2713" } : null;
         ApplyAlarmSearchFilter();
         SaveSettings();
      }

      private void InitializeAlarmStatusFilterMenu()
      {
         if (ctxAlarmStatusFilter?.Items == null) return;
         foreach (var item in ctxAlarmStatusFilter.Items.OfType<MenuItem>())
         {
            if (item.Tag is string tagValue && Enum.TryParse<AlarmGeneralStatus>(tagValue, ignoreCase: true, out var status))
               item.Icon = mAlarmStatusFilters.GetValueOrDefault(status, true) ? new TextBlock { Text = "\u2713" } : null;
         }
      }

      private void UpdateAlarmSidebarIcon()
      {
         bool hasActiveAlarm = mAlarmSelectionItems.Any(item => item.Alarm?.Status == AlarmGeneralStatus.Active);
         if (iconSidebarAlarm != null)
         {
            if (hasActiveAlarm) iconSidebarAlarm.Foreground = GetBrushForStatus(AlarmGeneralStatus.Active);
            else iconSidebarAlarm.ClearValue(TextBlock.ForegroundProperty);
         }
         var alarmHeader = this.FindControl<TextBlock>("iconAlarmHeader");
         if (alarmHeader != null)
         {
            if (hasActiveAlarm) alarmHeader.Foreground = GetBrushForStatus(AlarmGeneralStatus.Active);
            else alarmHeader.ClearValue(TextBlock.ForegroundProperty);
         }
      }

      private IBrush GetBrushForStatus(AlarmGeneralStatus status) => status switch
      {
         AlarmGeneralStatus.Active       => GetBrushResource("AlarmStatusActive", "#FF0000"),
         AlarmGeneralStatus.Inactive     => GetBrushResource("AlarmStatusInactive", "#808080"),
         AlarmGeneralStatus.Acknowledged => GetBrushResource("AlarmStatusAcknowledged", "#FFA500"),
         AlarmGeneralStatus.Tampered     => GetBrushResource("AlarmStatusTampered", "#FF8C00"),
         _                               => GetBrushResource("AlarmStatusDefault", "#78808080"),
      };

      private static string GetTextForStatus(AlarmGeneralStatus status) => status switch
      {
         AlarmGeneralStatus.Active       => "(Active)",
         AlarmGeneralStatus.Inactive     => "(Inactive)",
         AlarmGeneralStatus.Acknowledged => "(Acknowledged)",
         AlarmGeneralStatus.Tampered     => "(Tampered)",
         AlarmGeneralStatus.Disabled     => "(Disabled)",
         _                               => "(Unknown)",
      };

      private static string GetIconForAlarmStatus(AlarmGeneralStatus status) => status switch
      {
         AlarmGeneralStatus.Active   => "\uE7F7",
         AlarmGeneralStatus.Tampered => "\uE004",
         AlarmGeneralStatus.Inactive => "\uE7F4",
         AlarmGeneralStatus.Disabled => "\uE7F6",
         _                           => "\uE7F4",
      };

      // ── PTZ ────────────────────────────────────────────────────────────────

      private void RegisterPtzButtonHandlers()
      {
         var ptzButtons = new[] { btnPanLeft, btnPanRight, btnTiltUp, btnTiltDown, btnZoomIn, btnZoomOut, btnFocusFar, btnFocusNear };
         foreach (var btn in ptzButtons)
         {
            if (btn == null) continue;
            btn.AddHandler(PointerPressedEvent, OnControlCameraPointerPressed, RoutingStrategies.Bubble, handledEventsToo: true);
            btn.AddHandler(PointerReleasedEvent, OnControlCameraPointerReleased, RoutingStrategies.Bubble, handledEventsToo: true);
         }
      }

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
         mCurrentCamera?.PanTiltZoomStop();
      }

      // ── Absolute Position / Camera Lock ────────────────────────────────────

      private void btnOpenAbsolutePositionWindow_Click(object sender, RoutedEventArgs e)
      {
         if (mFlexApiClient == null || mCurrentCamera == null) return;
         var page = new AbsolutePositionPage(mCurrentCamera) { NavigationService = mNavigationService };
         mNavigationService.NavigateTo(page);
      }

      private void btnCameraLock_Click(object sender, RoutedEventArgs e)
      {
         if (mFlexApiClient == null || mCurrentCamera == null) return;
         if (!mCurrentCamera.IsLocked)
         {
            var page = new CameraLockPage(mCurrentCamera) { NavigationService = mNavigationService };
            mNavigationService.NavigateTo(page);
         }
         else
         {
            mCurrentCamera.Unlock();
         }
      }

      // ── Playback ───────────────────────────────────────────────────────────

      private void FillPlaybackSelectionList()
      {
         var list = mCurrentCamera?.GetPlaybackInfoList(mViewerID);
         if (list != null && list.Count > 0)
            selPlayback.ItemsSource = list;
         else
            ClearRecordingDropdown();
      }

      private void ClearRecordingDropdown()
      {
         if (selPlayback != null)
         {
            selPlayback.ItemsSource = null;
            var items = new List<object>();
            items.Add(!ApiSupportsPlayback && IsStarted ? "Api Version does not support playback" : "No camera selected");
            selPlayback.ItemsSource = items;
            selPlayback.SelectedIndex = 0;
         }
      }

      private void btnPlayPlayback_Click(object sender, RoutedEventArgs e)
      {
         if (selPlayback?.SelectedItem is PlaybackInfo recording)
         {
            string url = recording.PlaybackUrl;
            if (!string.IsNullOrEmpty(url))
            {
               mActiveRtspUrl = url;
               if (txtCurrentRtspUrl != null) txtCurrentRtspUrl.Text = GetMaskedUrl(url);
               var cameraNo = mCurrentCamera?.ComponentNumber ?? 0;
               if (txtVideoHeader != null) txtVideoHeader.Text = $"PLAYBACK - Camera {cameraNo}";
               if (brdVideoHeader != null) brdVideoHeader.Background = GetPlaybackHeaderBrush();
               RtspStreamRequested?.Invoke(this, url);
               IsPlaybackStarted = true;
               UpdateEnabled();
            }
         }
      }

      private void btnGotoTime_Click(object sender, RoutedEventArgs e)
      {
         if (selPlayback?.SelectedItem is not PlaybackInfo recording) return;
         try
         {
            DateTime date = DateTime.Now.Date;
            if (!string.IsNullOrWhiteSpace(txtDatePlayback?.Text) && DateTime.TryParse(txtDatePlayback.Text, out DateTime parsedDate))
               date = parsedDate.Date;

            TimeSpan time = TimeSpan.Zero;
            if (!string.IsNullOrWhiteSpace(txtTimePlayback?.Text) && TimeSpan.TryParse(txtTimePlayback.Text, out TimeSpan parsedTime))
               time = parsedTime;

            var dt = date + time;
            string url = recording.PlaybackUrl + $"?start={dt:yyyyMMddHHmmss}";
            mActiveRtspUrl = url;
            if (txtCurrentRtspUrl != null) txtCurrentRtspUrl.Text = GetMaskedUrl(recording.PlaybackUrl);
            var cameraNo = mCurrentCamera?.ComponentNumber ?? 0;
            if (txtVideoHeader != null) txtVideoHeader.Text = $"PLAYBACK - Camera {cameraNo}";
            if (brdVideoHeader != null) brdVideoHeader.Background = GetPlaybackHeaderBrush();
            RtspStreamRequested?.Invoke(this, url);
            IsPlaybackStarted = true;
            UpdateEnabled();
         }
         catch { }
      }

      private void btnStopPlayback_Click(object sender, RoutedEventArgs e)
      {
         IsPlaybackStarted = false;
         if (mCurrentCamera != null)
            SelectCamera(mCurrentCamera.ComponentNumber, GetSubChannelStreamNo());
         UpdateEnabled();
      }

      private void selPlayback_SelectedIndexChanged(object sender, SelectionChangedEventArgs e) { }

      private void tglSubChannel_CheckedChanged(object sender, RoutedEventArgs e)
      {
         if (mIsLoadingSettings) return;
         if (IsStarted && mCurrentCamera != null)
            SelectCamera(mCurrentCamera.ComponentNumber, tglSubChannel.IsChecked == true ? 2 : 1);
      }

      // ── API Version / Title ────────────────────────────────────────────────

      private void CheckApiVersion(ApiVersion apiVersion = null)
      {
         apiVersion ??= mFlexApiClient?.GetApiVersion();
         mApiVersion = apiVersion;
         if (apiVersion != null)
         {
            var version = new Version(apiVersion.MajorVersion, apiVersion.MinorVersion);
            if (version >= new Version(1, 1))
               ApiSupportsPlayback = true;
         }

         try { mImplementationVersion = mFlexApiClient?.GetImplementationVersion(); }
         catch { mImplementationVersion = null; }

         UpdateEnabled();
      }

      // ── Date/Time picker overlay ───────────────────────────────────────────

      private void btnPickDate_Click(object sender, RoutedEventArgs e)
      {
         mPickerOverlayMode = PickerOverlayMode.Date;
         if (txtPickerOverlayTitle != null) txtPickerOverlayTitle.Text = "Select Date";
         if (txtPickerOverlaySubtitle != null) txtPickerOverlaySubtitle.Text = "Choose a date for playback";
         if (overlayDatePicker != null) overlayDatePicker.IsVisible = true;
         if (overlayTimePicker != null) overlayTimePicker.IsVisible = false;
         if (overlayDatePicker != null)
            overlayDatePicker.SelectedDate = DateTime.TryParse(txtDatePlayback?.Text, out DateTime d) ? new DateTimeOffset(d) : new DateTimeOffset(DateTime.Now);
         ShowPickerOverlay();
      }

      private void btnPickTime_Click(object sender, RoutedEventArgs e)
      {
         mPickerOverlayMode = PickerOverlayMode.Time;
         if (txtPickerOverlayTitle != null) txtPickerOverlayTitle.Text = "Select Time";
         if (txtPickerOverlaySubtitle != null) txtPickerOverlaySubtitle.Text = "Choose a time for playback";
         if (overlayDatePicker != null) overlayDatePicker.IsVisible = false;
         if (overlayTimePicker != null)
         {
            overlayTimePicker.IsVisible = true;
            overlayTimePicker.SelectedTime = TimeSpan.TryParse(txtTimePlayback?.Text, out TimeSpan t) ? t : DateTime.Now.TimeOfDay;
         }
         ShowPickerOverlay();
      }

      private void btnPickerOverlayApply_Click(object sender, RoutedEventArgs e)
      {
         if (mPickerOverlayMode == PickerOverlayMode.Date && overlayDatePicker?.SelectedDate.HasValue == true)
            if (txtDatePlayback != null) txtDatePlayback.Text = overlayDatePicker.SelectedDate.Value.ToString("yyyy-MM-dd");
         else if (mPickerOverlayMode == PickerOverlayMode.Time && overlayTimePicker?.SelectedTime.HasValue == true)
            if (txtTimePlayback != null) txtTimePlayback.Text = overlayTimePicker.SelectedTime.Value.ToString(@"hh\:mm\:ss");

         mPickerOverlayMode = PickerOverlayMode.None;
         HidePickerOverlay();
      }

      private void btnPickerOverlayCancel_Click(object sender, RoutedEventArgs e)
      {
         mPickerOverlayMode = PickerOverlayMode.None;
         HidePickerOverlay();
      }

      private void ShowPickerOverlay()
      {
         mIsPickerOverlayVisible = true;
         if (pickerOverlay != null) pickerOverlay.IsVisible = true;
         UpdateVideoSurfaceForOverlayState();
      }

      private void HidePickerOverlay()
      {
         mIsPickerOverlayVisible = false;
         if (pickerOverlay != null) pickerOverlay.IsVisible = false;
         if (overlayDatePicker != null) overlayDatePicker.IsVisible = false;
         if (overlayTimePicker != null) overlayTimePicker.IsVisible = false;
         UpdateVideoSurfaceForOverlayState();

         if (!mIsNavigationOverlayVisible)
         {
            Dispatcher.UIThread.Post(() =>
            {
               if (!mIsNavigationOverlayVisible && !mIsPickerOverlayVisible)
                  UpdateVideoSurfaceForOverlayState();
            }, DispatcherPriority.Background);
         }
      }

      // ── Download ───────────────────────────────────────────────────────────

      private void btnOpenDownloadWindow_Click(object sender, RoutedEventArgs e)
      {
         if (mFlexApiClient == null) return;
         var page = new DownloadPage(mFlexApiClient) { NavigationService = mNavigationService };
         mNavigationService.NavigateTo(page);
      }

      // ── Video menu ─────────────────────────────────────────────────────────

      private void EnsureVideoContextMenu()
      {
         mVideoContextMenu ??= new ContextMenu();
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

      private List<object> BuildVideoContextMenuItems()
      {
         var rootItems = new List<object>();
         if (mFlexApiClient == null) { rootItems.Add(new MenuItem { Header = "Not connected", IsEnabled = false }); return rootItems; }

         var cameraList = GetAvailableCamerasForMenu();
         if (cameraList == null || cameraList.Count == 0) { rootItems.Add(new MenuItem { Header = "No cameras available", IsEnabled = false }); return rootItems; }

         const int C_MAX_CAMERAS_PER_SUBMENU = 25;
         if (cameraList.Count <= C_MAX_CAMERAS_PER_SUBMENU)
         {
            foreach (var camera in cameraList) rootItems.Add(CreateCameraMenuItem(camera));
         }
         else
         {
            for (int i = 0; i < cameraList.Count; i += C_MAX_CAMERAS_PER_SUBMENU)
            {
               int end = Math.Min(i + C_MAX_CAMERAS_PER_SUBMENU, cameraList.Count);
               var batch = cameraList.GetRange(i, end - i);
               var rangeItems = new List<object>();
               var rangeMenu = new MenuItem { Header = $"{batch.First().Name} – {batch.Last().Name}", ItemsSource = rangeItems };
               foreach (var camera in batch) rangeItems.Add(CreateCameraMenuItem(camera));
               rootItems.Add(rangeMenu);
            }
         }

         return rootItems;
      }

      private List<Camera> GetAvailableCamerasForMenu()
      {
         var list = mFlexApiClient?.GetCameraList();
         if (list != null && list.Count > 0) return list;
         return mCameraSelectionItems.Select(i => i.Camera).Where(c => c != null)
            .GroupBy(c => c.ComponentNumber).Select(g => g.First()).OrderBy(c => c.ComponentNumber).ToList();
      }

      private MenuItem CreateCameraMenuItem(Camera camera)
      {
         int hotkeySlot = GetCameraHotkeySlot(camera.ComponentNumber);
         object header;
         string labelText = $"#{camera.ComponentNumber:D4} [{(camera.HasPanTiltControl ? "PTZ" : "Fixed")}] {camera.Name}";
         if (hotkeySlot >= 0)
         {
            var panel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
            panel.Children.Add(new TextBlock { Text = labelText });
            panel.Children.Add(new TextBlock { Text = $"Ctrl+{hotkeySlot}", Opacity = 0.5, FontSize = 11, VerticalAlignment = VerticalAlignment.Center });
            header = panel;
         }
         else
         {
            header = labelText;
         }

         var item = new MenuItem { Header = header, Tag = camera };
         if (mCurrentCamera == camera)
            item.Icon = new CheckBox { IsChecked = true, IsHitTestVisible = false };
         item.Click += (s, e) => SelectCamera(camera.ComponentNumber, GetSubChannelStreamNo());
         return item;
      }

      private static int GetCameraHotkeySlot(int cameraNumber)
      {
         var hotkeys = AppSettings.Default.CameraHotkeys;
         if (hotkeys == null) return -1;
         foreach (var kvp in hotkeys)
            if (kvp.Value == cameraNumber) return kvp.Key;
         return -1;
      }

      // ── Sidebar ────────────────────────────────────────────────────────────

      private void btnToggleSidebar_Click(object sender, RoutedEventArgs e)
      {
         SetSidebarCollapsed(!IsSidebarCurrentlyCollapsed(), persistSetting: true);
      }

      private void SetSidebarCollapsed(bool collapsed, bool persistSetting = false)
      {
         if (btnOpenSidebar != null) btnOpenSidebar.IsVisible = false;

         if (collapsed)
         {
            if (sidebarGrid != null) sidebarGrid.Width = C_COLLAPSED_SIDEBAR_WIDTH;
            if (narrowSidebar != null) narrowSidebar.IsVisible = true;
            if (fullSidebar != null) fullSidebar.IsVisible = false;
         }
         else
         {
            if (sidebarGrid != null) sidebarGrid.Width = GetExpandedSidebarWidth();
            if (narrowSidebar != null) narrowSidebar.IsVisible = false;
            if (fullSidebar != null) fullSidebar.IsVisible = true;
         }

         if (persistSetting)
         {
            AppSettings.Default.IsSidebarCollapsed = collapsed;
            SaveSettings();
         }
      }

      private double GetExpandedSidebarWidth() => mApp?.CurrentTheme?.Components?.Sidebar?.Width ?? 280d;

      private bool IsSidebarCurrentlyCollapsed()
      {
         if (narrowSidebar?.IsVisible == true) return true;
         return Math.Abs(sidebarGrid?.Width ?? 0 - C_COLLAPSED_SIDEBAR_WIDTH) < 0.5;
      }

      private void ApplyResponsiveSidebarLayout(double width)
      {
         if (width < C_SIDEBAR_AUTO_COLLAPSE_BREAKPOINT)
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
            if (exp != target) exp.IsExpanded = false;
         target.IsExpanded = true;
      }

      private void btnExpandCameraControl_Click(object sender, RoutedEventArgs e)   { SetSidebarCollapsed(false, persistSetting: true); CollapseAllExpandersExcept(expCameraControl); }
      private void btnExpandCameraSelection_Click(object sender, RoutedEventArgs e)  { SetSidebarCollapsed(false, persistSetting: true); CollapseAllExpandersExcept(expCameraSelection); }
      private void btnExpandPresetSelection_Click(object sender, RoutedEventArgs e)  { SetSidebarCollapsed(false, persistSetting: true); CollapseAllExpandersExcept(expPresetSelection); }
      private void btnExpandAlarms_Click(object sender, RoutedEventArgs e)           { SetSidebarCollapsed(false, persistSetting: true); CollapseAllExpandersExcept(expAlarms); }
      private void btnExpandPlayback_Click(object sender, RoutedEventArgs e)         { SetSidebarCollapsed(false, persistSetting: true); CollapseAllExpandersExcept(expPlaybackSelection); }
      private void btnExpandDownload_Click(object sender, RoutedEventArgs e)         { SetSidebarCollapsed(false, persistSetting: true); CollapseAllExpandersExcept(expDownloadRecording); }

      // ── Sidebar resize handles ─────────────────────────────────────────────

      private ListBox ResolveResizableList(object sender)
      {
         if (sender is not Control control) return null;
         return control.Tag?.ToString() switch
         {
            "camera" => lstCameraSelection,
            "alarm"  => lstAlarmSelection,
            _        => null
         };
      }

      private void SidebarMenuResizeHandle_PointerPressed(object sender, PointerPressedEventArgs e)
      {
         var list = ResolveResizableList(sender);
         if (list == null) return;
         mActiveResizableList = list;
         mIsResizingSidebarMenu = true;
         mResizeStartPoint = e.GetPosition(this);
         mResizeStartHeight = list.Height > 0 ? list.Height : list.Bounds.Height;
         if (sender is InputElement inputElement) e.Pointer.Capture(inputElement);
      }

      private void SidebarMenuResizeHandle_PointerMoved(object sender, PointerEventArgs e)
      {
         if (!mIsResizingSidebarMenu || mActiveResizableList == null) return;
         var minH = AppConstants.Default.ResizableSidebarMenuMinHeight;
         var maxH = AppConstants.Default.ResizableSidebarMenuMaxHeight;
         var delta = e.GetPosition(this).Y - mResizeStartPoint.Y;
         mActiveResizableList.Height = Math.Clamp(mResizeStartHeight + delta, minH, maxH);
      }

      private void SidebarMenuResizeHandle_PointerReleased(object sender, PointerReleasedEventArgs e) => EndSidebarMenuResize(e.Pointer);
      private void SidebarMenuResizeHandle_PointerCaptureLost(object sender, PointerCaptureLostEventArgs e) => EndSidebarMenuResize(null);

      private void EndSidebarMenuResize(IPointer pointer)
      {
         if (!mIsResizingSidebarMenu) return;
         mIsResizingSidebarMenu = false;
         mActiveResizableList = null;
         pointer?.Capture(null);
         SaveSettings();
      }

      // ── Messages ───────────────────────────────────────────────────────────

      public void WriteMessageLog(MessageSource source, string strMessage, LogLevel level)
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
            if (wasAtEnd && lstMessages != null)
               lstMessages.ScrollIntoView(mFilteredMessages.Count - 1);
         }
      }

      private IBrush GetColorForLogLevel(LogLevel level) => level switch
      {
         LogLevel.Error   => GetBrushResource("MessageErrorForeground", "#FF0000"),
         LogLevel.Warning => GetBrushResource("MessageWarningForeground", "#F0AA1F"),
         LogLevel.Debug   => GetBrushResource("MessageDebugForeground", "#ADD8E6"),
         _                => GetBrushResource("MessageDefaultForeground", "#FFFFFF"),
      };

      private IBrush GetBackgroundForLogLevel(LogLevel level) => GetBrushResource("MessageLogBackground", "#00000000");

      private void RefreshMessageColors()
      {
         foreach (var item in mMessages)
         {
            item.Color = GetColorForLogLevel(item.Level);
            item.Background = GetBackgroundForLogLevel(item.Level);
         }
      }

      private void RefreshVideoHeaderState()
      {
         if (!IsStarted || mCurrentCamera == null)
         {
            if (txtVideoHeader != null) txtVideoHeader.Text = "No Camera Selected";
            if (brdVideoHeader != null) brdVideoHeader.Background = GetNeutralHeaderBrush();
            return;
         }

         var cameraNo = mCurrentCamera.ComponentNumber;
         if (IsPlayback || IsPlaybackStarted)
         {
            if (txtVideoHeader != null) txtVideoHeader.Text = $"PLAYBACK - Camera {cameraNo}";
            if (brdVideoHeader != null) brdVideoHeader.Background = GetPlaybackHeaderBrush();
         }
         else
         {
            if (txtVideoHeader != null) txtVideoHeader.Text = $"LIVE - Camera {cameraNo}";
            if (brdVideoHeader != null) brdVideoHeader.Background = GetLiveHeaderBrush();
         }
      }

      private void MessagesHeader_PointerPressed(object sender, PointerPressedEventArgs e)
      {
         mIsMessagesCollapsed = !mIsMessagesCollapsed;
         var messagesGrid = brdMessages?.Parent as Grid;
         if (messagesGrid == null) return;
         var messagesRow = messagesGrid.RowDefinitions[2];

         if (mIsMessagesCollapsed)
         {
            mMessagesExpandedRowHeight = messagesRow.Height;
            messagesRow.Height = GridLength.Auto;
            if (brdMessages != null) brdMessages.MaxHeight = 36;
            if (lstMessages != null) lstMessages.IsVisible = false;
            if (messagesSplitter != null) messagesSplitter.IsEnabled = false;
            if (txtMessagesToggle != null) txtMessagesToggle.Text = "▶";
         }
         else
         {
            if (brdMessages != null) brdMessages.MaxHeight = double.PositiveInfinity;
            messagesRow.Height = mMessagesExpandedRowHeight;
            if (lstMessages != null) lstMessages.IsVisible = true;
            if (messagesSplitter != null) messagesSplitter.IsEnabled = true;
            if (txtMessagesToggle != null) txtMessagesToggle.Text = "▼";
         }

         SaveSettings();
      }

      private void MessagesSplitter_DragCompleted(object sender, Avalonia.Input.VectorEventArgs e) => SaveSettings();

      private bool IsScrolledToEnd()
      {
         var scrollViewer = FindScrollViewer(lstMessages);
         if (scrollViewer != null)
            return scrollViewer.Offset.Y >= scrollViewer.Extent.Height - scrollViewer.Viewport.Height - 20;
         return true;
      }

      private ScrollViewer FindScrollViewer(Control parent)
      {
         if (parent is ScrollViewer sv) return sv;
         foreach (var child in Avalonia.VisualTree.VisualExtensions.GetVisualChildren(parent))
            if (child is Control c) { var result = FindScrollViewer(c); if (result != null) return result; }
         return null;
      }

      private void ApplyMessageFilter()
      {
         mFilteredMessages.Clear();
         foreach (var msg in mMessages)
            if (mSourceFilters.TryGetValue(msg.Source, out bool visible) && visible)
               mFilteredMessages.Add(msg);
         if (mFilteredMessages.Count > 0 && lstMessages != null)
            lstMessages.ScrollIntoView(mFilteredMessages.Count - 1);
      }

      private void btnClearMessages_Click(object sender, RoutedEventArgs e) { mMessages.Clear(); mFilteredMessages.Clear(); }

      private async void menuCopyMessages_Click(object sender, RoutedEventArgs e)
      {
         var text = string.Join(Environment.NewLine, mFilteredMessages.Select(m => m.Text));
         var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
         if (clipboard is { } cb) await cb.SetTextAsync(text);
      }

      private async void menuCopySelectedMessages_Click(object sender, RoutedEventArgs e) => await CopySelectedMessagesAsync();

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
         var selectedText = string.Join(Environment.NewLine, lstMessages.SelectedItems.Cast<MessageItem>().Select(m => m.Text));
         var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
         if (!string.IsNullOrEmpty(selectedText) && clipboard is { } cb)
            await cb.SetTextAsync(selectedText);
      }

      private void menuScrollToEnd_Click(object sender, RoutedEventArgs e)
      {
         if (mFilteredMessages.Count > 0 && lstMessages != null)
            lstMessages.ScrollIntoView(mFilteredMessages.Count - 1);
      }

      private void menuFilterFlexApi_Click(object sender, RoutedEventArgs e) => ToggleSourceFilter(sender, MessageSource.FlexApi);
      private void menuFilterLibVlc_Click(object sender, RoutedEventArgs e)  => ToggleSourceFilter(sender, MessageSource.LibVlc);
      private void menuFilterConfig_Click(object sender, RoutedEventArgs e)  => ToggleSourceFilter(sender, MessageSource.Config);

      private void ToggleSourceFilter(object sender, MessageSource source)
      {
         if (sender is MenuItem menuItem && menuItem.Icon is CheckBox cb)
         {
            cb.IsChecked = !(cb.IsChecked ?? false);
            mSourceFilters[source] = cb.IsChecked ?? false;
            ApplyMessageFilter();
         }
      }

      // ── Camera rename (F2) ─────────────────────────────────────────────────

      private void HandleRenameCameraAsync()
      {
         var selectedItem = lstCameraSelection?.SelectedItem as CameraSelectionItem;
         if (selectedItem?.Camera == null) return;

         var currentUser = selectedItem.Camera.FlexApiClient.CurrentUser;
         if (currentUser == null || currentUser.Privilege < UserPrivilege.Supervisor) return;

         var container = lstCameraSelection.ContainerFromItem(selectedItem) as Control;
         if (container == null) return;

         var textBlock = container.GetVisualDescendants().OfType<TextBlock>().FirstOrDefault(tb => tb.Name == "txtCameraItemName");
         var textBox   = container.GetVisualDescendants().OfType<TextBox>().FirstOrDefault(tb => tb.Name == "txtCameraItemEdit");
         if (textBlock == null || textBox == null) return;

         var camera = selectedItem.Camera;
         textBox.Text = camera.Name;
         textBlock.IsVisible = false;
         textBox.IsVisible = true;
         textBox.Focus();
         textBox.SelectAll();

         const string C_ALLOWED_CHARS = "QWERTYUIOPASDFGHJKLZXCVBNMqwertyuiopasdfghjklzxcvbnm1234567890,.()?!-/_ ";

         void CommitEdit()
         {
            string newName = textBox.Text?.Trim();
            textBlock.IsVisible = true;
            textBox.IsVisible = false;
            if (!string.IsNullOrEmpty(newName) && newName != camera.Name)
            {
               bool success = camera.SetName(newName);
               if (success) { camera.UpdateCameraData(); ApplyCameraSearchFilter(); }
            }
         }

         void CancelEdit() { textBlock.IsVisible = true; textBox.IsVisible = false; }

         void OnTextInput(object s, TextInputEventArgs args)
         {
            if (args.Text != null && args.Text.Any(c => !C_ALLOWED_CHARS.Contains(c)))
               args.Handled = true;
         }

         void OnKeyDown(object s, KeyEventArgs args)
         {
            if (args.Key == Key.Enter) { CommitEdit(); args.Handled = true; }
            else if (args.Key == Key.Escape) { CancelEdit(); args.Handled = true; }
         }

         void OnLostFocus(object s, RoutedEventArgs args)
         {
            CommitEdit();
            textBox.RemoveHandler(InputElement.TextInputEvent, OnTextInput);
            textBox.KeyDown -= OnKeyDown;
            textBox.LostFocus -= OnLostFocus;
         }

         textBox.AddHandler(InputElement.TextInputEvent, OnTextInput, RoutingStrategies.Tunnel);
         textBox.KeyDown += OnKeyDown;
         textBox.LostFocus += OnLostFocus;
      }

      public bool IsTextInputFocused()
      {
         var focusedElement = TopLevel.GetTopLevel(this)?.FocusManager?.GetFocusedElement();
         if (focusedElement is TextBox) return true;
         if (focusedElement is Visual visual && visual.GetVisualAncestors().OfType<TextBox>().Any()) return true;
         return false;
      }

      // ── Settings ───────────────────────────────────────────────────────────

      private void LoadSettings()
      {
         var s = AppSettings.Default;
         if (tglSubChannel != null) tglSubChannel.IsChecked = s.PreferSubChannel;
         UpdateUserInitial();

         mAlarmStatusFilters[AlarmGeneralStatus.Active]       = s.ShowActiveAlarms;
         mAlarmStatusFilters[AlarmGeneralStatus.Tampered]     = s.ShowTamperedAlarms;
         mAlarmStatusFilters[AlarmGeneralStatus.Acknowledged] = s.ShowAcknowledgedAlarms;
         mAlarmStatusFilters[AlarmGeneralStatus.Inactive]     = s.ShowPassiveAlarms;
         mAlarmStatusFilters[AlarmGeneralStatus.Disabled]     = s.ShowDisabledAlarms;
         InitializeAlarmStatusFilterMenu();
      }

      private void SaveSettings()
      {
         var s = AppSettings.Default;
         if (mCurrentCamera != null) s.CurrentCamera = mCurrentCamera.ComponentNumber;
         if (expCameraControl != null)   s.IsCameraControlExpanded = expCameraControl.IsExpanded;
         if (expCameraSelection != null) s.IsCameraSelectionExpanded = expCameraSelection.IsExpanded;
         if (expPresetSelection != null) s.IsPresetSelectionExpanded = expPresetSelection.IsExpanded;
         if (expAlarms != null)          s.IsAlarmsExpanded = expAlarms.IsExpanded;
         if (expPlaybackSelection != null) s.IsPlaybackSelectionExpanded = expPlaybackSelection.IsExpanded;
         if (expDownloadRecording != null) s.IsDownloadRecordingExpanded = expDownloadRecording.IsExpanded;
         s.IsMessagesCollapsed = mIsMessagesCollapsed;

         if (brdMessages?.Parent is Grid mg && mg.RowDefinitions.Count > 2 && !mIsMessagesCollapsed)
         {
            s.MessagesSplitVideoStars   = mg.RowDefinitions[0].Height.Value;
            s.MessagesSplitMessagesStars = mg.RowDefinitions[2].Height.Value;
         }

         if (lstCameraSelection?.Height > 0) s.CameraSidebarMenuHeight = lstCameraSelection.Height;
         if (lstAlarmSelection?.Height > 0)  s.AlarmSidebarMenuHeight  = lstAlarmSelection.Height;

         s.ShowActiveAlarms      = mAlarmStatusFilters.GetValueOrDefault(AlarmGeneralStatus.Active, true);
         s.ShowTamperedAlarms    = mAlarmStatusFilters.GetValueOrDefault(AlarmGeneralStatus.Tampered, true);
         s.ShowAcknowledgedAlarms = mAlarmStatusFilters.GetValueOrDefault(AlarmGeneralStatus.Acknowledged, true);
         s.ShowPassiveAlarms     = mAlarmStatusFilters.GetValueOrDefault(AlarmGeneralStatus.Inactive, true);
         s.ShowDisabledAlarms    = mAlarmStatusFilters.GetValueOrDefault(AlarmGeneralStatus.Disabled, true);
         s.Save();
      }

      // ── User Profile / Theme ───────────────────────────────────────────────

      private bool IsDarkMode => mApp?.CurrentTheme?.IsDark ?? true;

      private void UpdateUserInitial()
      {
         string username = AppSettings.Default.User?.Trim() ?? "";
         var txtMenuUsernameCtrl = this.FindControl<TextBlock>("txtMenuUsername");
         var menuItemLoginCtrl   = this.FindControl<MenuItem>("menuItemLogin");
         var menuItemLogoutCtrl  = this.FindControl<MenuItem>("menuItemLogout");

         if (txtUserInitial != null) txtUserInitial.Text = string.IsNullOrEmpty(username) ? "U" : username.Substring(0, 1).ToUpper();
         if (txtMenuUsernameCtrl != null) txtMenuUsernameCtrl.Text = string.IsNullOrEmpty(username) ? "Not logged in" : username;
         if (menuItemLoginCtrl  != null) menuItemLoginCtrl.IsEnabled  = !IsStarted && !mIsConnecting;
         if (menuItemLogoutCtrl != null) menuItemLogoutCtrl.IsEnabled = IsStarted && !mIsConnecting;

         if (btnUserProfile != null)
         {
            var status = mIsConnecting ? "Connecting" : (IsStarted ? "Connected" : "Disconnected");
            var tip = string.IsNullOrEmpty(username) ? "Not logged in" : username;
            tip += $"\n{status}";
            if (IsStarted && !string.IsNullOrEmpty(mConnectedEndpointDisplay)) tip += $"\nServer: {mConnectedEndpointDisplay}";
            if (IsStarted && mApiVersion != null) tip += $"\nAPI: v{mApiVersion.MajorVersion}.{mApiVersion.MinorVersion}";
            if (IsStarted && mImplementationVersion != null) tip += $"\n{mImplementationVersion.Name}: {mImplementationVersion.Version}";
            ToolTip.SetTip(btnUserProfile, tip);
            btnUserProfile.Background = IsStarted
               ? GetBrushResource("UserProfileConnectedBackground", "#006BA1")
               : GetBrushResource("UserProfileDisconnectedBackground", "#808080");
         }
      }

      private void btnUserProfile_Click(object sender, RoutedEventArgs e) => PopulateThemeSelectionMenuItems();

      private void menuItemLogin_Click(object sender, RoutedEventArgs e)
      {
         var s = AppSettings.Default;
         if (!s.HasAnyConnectionAlternative() || string.IsNullOrWhiteSpace(s.User) || string.IsNullOrWhiteSpace(s.Password))
         {
            mPendingConnectAfterSettings = true;
            OpenSettingsPage();
         }
         else
         {
            btnConnect_Click(sender, e);
         }
      }

      private void menuItemLogout_Click(object sender, RoutedEventArgs e)
      {
         if (IsStarted) btnDisconnect_Click(sender, e);
      }

      private void menuItemSettings_Click(object sender, RoutedEventArgs e) => OpenSettingsPage();

      private void OpenSettingsPage()
      {
         if (navigationHost?.Content is SettingsPage) return;
         var settingsPage = new SettingsPage { NavigationService = mNavigationService };
         mNavigationService.NavigateTo(settingsPage);
      }

      private void HandleEscapeNavigation()
      {
         if (navigationHost?.Content is SettingsPage sp) { sp.CancelAndGoBack(); return; }
         if (navigationHost?.IsVisible == true) mNavigationService.GoBack();
      }

      private void UpdateThemeMenus() => PopulateThemeSelectionMenuItems();

      private void PopulateThemeSelectionMenuItems()
      {
         var menuItemThemeSelect = this.FindControl<MenuItem>("menuItemThemeSelect");
         if (menuItemThemeSelect == null || mApp == null) return;

         var items = new List<object>();
         var targetTheme = mApp.GetNextThemeInCycle();
         var toggleItem = new MenuItem
         {
            Icon = new TextBlock { Text = IsDarkMode ? "\uE51C" : "\uE518", Classes = { "ms-icon" }, FontSize = 18 },
            Header = new StackPanel
            {
               Orientation = Orientation.Horizontal, Spacing = 8,
               Children =
               {
                  new TextBlock { Text = $"Cycle to {targetTheme.DisplayName}" },
                  new TextBlock { Text = "F10", Opacity = 0.5, FontSize = 11, VerticalAlignment = VerticalAlignment.Center }
               }
            }
         };
         toggleItem.Click += menuItemToggleTheme_Click;
         items.Add(toggleItem);
         items.Add(new Separator());
         items.AddRange(BuildThemeSelectionMenuItems());
         menuItemThemeSelect.ItemsSource = items;
      }

      private List<object> BuildThemeSelectionMenuItems()
      {
         if (mApp == null) return new List<object>();
         var selectedKey = AppSettings.Default.GetPreferredThemeKey();
         var items = new List<object>();
         foreach (var option in mApp.GetThemeOptions())
         {
            var item = new MenuItem { Header = option.DisplayName, Tag = option.Key };
            if (string.Equals(option.Key, selectedKey, StringComparison.OrdinalIgnoreCase))
               item.Icon = new CheckBox { IsChecked = true, IsHitTestVisible = false };
            item.Click += ThemeSelectionMenuItem_Click;
            items.Add(item);
         }
         return items;
      }

      private void menuItemToggleTheme_Click(object sender, RoutedEventArgs e)
      {
         CycleThemeAndRefreshUi();
         CloseProfileMenuFlyout();
      }

      private void ThemeSelectionMenuItem_Click(object sender, RoutedEventArgs e)
      {
         if (sender is not MenuItem menuItem || menuItem.Tag is not string themeKey || string.IsNullOrWhiteSpace(themeKey)) return;
         mApp?.ApplyTheme(themeKey, persistSelection: true);
         ApplySharedThemeRefresh();
         Dispatcher.UIThread.Post(CloseProfileMenuFlyout, DispatcherPriority.Background);
      }

      private void CycleThemeAndRefreshUi()
      {
         mApp?.CycleTheme();
         ApplySharedThemeRefresh();
      }

      private void ApplySharedThemeRefresh()
      {
         RefreshMessageColors();
         RefreshVideoHeaderState();
         SaveSettings();
         UpdateThemeMenus();
      }

      private void CloseProfileMenuFlyout()
      {
         if (btnUserProfile?.Flyout is MenuFlyout flyout) flyout.Hide();
      }

      // ── Helpers ────────────────────────────────────────────────────────────

      private IBrush GetBrushResource(string key, string fallback)
      {
         if (this.TryFindResource(key, this.ActualThemeVariant, out var resource) && resource is IBrush b)
            return b;
         return new SolidColorBrush(Color.Parse(fallback));
      }

      private IBrush GetNeutralHeaderBrush()  => GetBrushResource("VideoHeaderNeutral", "#1D3A4A");
      private IBrush GetLiveHeaderBrush()     => GetBrushResource("VideoHeaderLive", "#39B620");
      private IBrush GetPlaybackHeaderBrush() => GetBrushResource("VideoHeaderPlayback", "#CA3C3D");

      private static bool IsActiveRtspPlayback(string url) => url != null && url.Contains("playback");

      private static string GetMaskedUrl(string url)
      {
         try { return new UriBuilder(url) { Password = "******", UserName = "******" }.ToString(); }
         catch { return url ?? string.Empty; }
      }
   }

   // ── Support types ──────────────────────────────────────────────────────────

   public class CameraSelectionItem
   {
      public CameraSelectionItem(Camera camera) { Camera = camera; }
      public Camera Camera { get; }
      public string CameraName       => Camera?.Name ?? string.Empty;
      public string CameraNumberText => Camera == null ? string.Empty : $"#{Camera.ComponentNumber:D4}";
      public string CameraTypeIcon   => Camera?.HasPanTiltControl == true ? "\uF113" : "\uF299";

      public bool Matches(string query)
      {
         if (Camera == null || string.IsNullOrWhiteSpace(query)) return Camera != null;
         return Camera.Name?.Contains(query, StringComparison.OrdinalIgnoreCase) == true
            || Camera.ComponentNumber.ToString(CultureInfo.InvariantCulture).Contains(query, StringComparison.OrdinalIgnoreCase);
      }
   }

   public class AlarmSelectionItem : INotifyPropertyChanged
   {
      private IBrush mStatusBrush;
      private string mStatusText;
      private string mAlarmIcon = "\uE7F4";

      public AlarmSelectionItem(Alarm alarm, IBrush statusBrush, string statusText)
      {
         Alarm = alarm;
         mStatusBrush = statusBrush;
         mStatusText = statusText;
      }

      public Alarm Alarm { get; }
      public string AlarmName       => Alarm?.Name ?? string.Empty;
      public string AlarmNumberText => Alarm == null ? string.Empty : $"#{Alarm.ComponentNumber:D4}";

      public IBrush StatusBrush
      {
         get => mStatusBrush;
         set { if (ReferenceEquals(mStatusBrush, value)) return; mStatusBrush = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StatusBrush))); }
      }

      public string StatusText
      {
         get => mStatusText;
         set { if (mStatusText == value) return; mStatusText = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StatusText))); PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TooltipText))); }
      }

      public string TooltipText => $"{AlarmNumberText} {AlarmName} {StatusText}".Trim();

      public string AlarmIcon
      {
         get => mAlarmIcon;
         set { if (mAlarmIcon == value) return; mAlarmIcon = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AlarmIcon))); }
      }

      public bool Matches(string query)
      {
         if (Alarm == null || string.IsNullOrWhiteSpace(query)) return Alarm != null;
         return Alarm.Name?.Contains(query, StringComparison.OrdinalIgnoreCase) == true
            || Alarm.ComponentNumber.ToString(CultureInfo.InvariantCulture).Contains(query, StringComparison.OrdinalIgnoreCase);
      }

      public event PropertyChangedEventHandler PropertyChanged;
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
         set { if (mText == value) return; mText = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text))); }
      }

      public IBrush Color
      {
         get => mColor;
         set { if (ReferenceEquals(mColor, value)) return; mColor = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Color))); }
      }

      public IBrush Background
      {
         get => mBackground;
         set { if (ReferenceEquals(mBackground, value)) return; mBackground = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Background))); }
      }

      public MessageSource Source { get; set; }
      public LogLevel Level { get; set; }

      public event PropertyChangedEventHandler PropertyChanged;
   }
}
