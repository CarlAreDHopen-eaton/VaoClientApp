using System;
using System.Collections.Generic;
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
using Vao.Sample.Controls;
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
      private FlexApiClient mFlexApiClient;
      private Guid mViewerID = Guid.NewGuid();
      private Camera mCurrentCamera;
      private Alarm mCurrentAlarm;
      private User mCurrentLoggedInUser;

      // ── UI state ───────────────────────────────────────────────────────────

      private bool mIsLoadingSettings;
      private bool mIsNavigationOverlayVisible;
      private bool mIsPickerOverlayVisible;
      private bool mIsConnecting;
      private bool mPendingConnectAfterSettings;
      private string mConnectedEndpointDisplay = string.Empty;

      private App mApp;

      private readonly Controls.MessageLogPanel messageLogPanel = new();

      private const double C_SIDEBAR_AUTO_COLLAPSE_BREAKPOINT = 1100;
      private const double C_COLLAPSED_SIDEBAR_WIDTH = 56;

      private enum PickerOverlayMode { None, Date, Time }
      private PickerOverlayMode mPickerOverlayMode = PickerOverlayMode.None;

      // ── Public events ─────────────────────────────────────────────────────

      /// <summary>Fired when the connection state changes (connected or disconnected).</summary>
      public event EventHandler ConnectionStateChanged;

      // ── Public properties ──────────────────────────────────────────────────

      /// <summary>The current video layout key.</summary>
      public string CurrentLayoutKey => videoLayoutPanel.CurrentLayoutKey;

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

      public bool IsPlayback => videoLayoutPanel.IsPlayback;

      // ── Public methods (called by MainWindow thin shell) ───────────────────

      public void ToggleSidebar() => btnToggleSidebar_Click(null, null);

      public void HandleWindowResize(double width) => ApplyResponsiveSidebarLayout(width);

      public void SaveCurrentSettings()
      {
         SaveSettings();
         ConfigurationManager.Instance.Flush();
      }

      public void StopClient()
      {
         if (mFlexApiClient != null)
         {
            mFlexApiClient.OnMessage -= OnFlexApiClientMessage;
            mFlexApiClient.StopClient();
            mFlexApiClient = null;
         }
      }

      public void CycleThemeFromKeyboard()
      {
         if (mApp != null)
            CycleThemeAndRefreshUi();
      }

      public void OpenSettingsFromKeyboard() => OpenSettingsPage();

      public void OpenMessageLogFromKeyboard() => OpenMessageLogPage();

      public void HandleEscapeKey() => HandleEscapeNavigation();

      public void HandleRenameCameraKey() => cameraSelectorPanel.HandleRename(UserPrivilege.Supervisor);

      public void NavigatePreviousCamera() => NavigateToPreviousCamera();

      public void NavigateNextCamera() => NavigateToNextCamera();

      public void SelectCameraHotkey(int cameraNo)
      {
         if (!IsStarted || mFlexApiClient == null) return;
         int streamNo = videoLayoutPanel.GetStreamNo();
         SelectCamera(cameraNo, streamNo);
      }

      public void AssignCameraHotkey(int slot)
      {
         if (mCurrentCamera == null) return;
         ConfigurationManager.Instance.SetCameraHotkey(slot, mCurrentCamera.ComponentNumber);
         WriteMessageLog(MessageSource.Config, $"Camera {mCurrentCamera.ComponentNumber} assigned to slot {slot} (Ctrl+{slot})", LogLevel.Notice);
      }

      public int GetSubChannelStreamNo() => videoLayoutPanel.GetStreamNo();

      public void SetActiveVideoSlot(int slotIndex) => videoLayoutPanel.SetActiveSlot(slotIndex);

      public void HandleSlotDrop(int slotIndex, DragEventArgs e) => videoLayoutPanel.HandleSlotDrop(slotIndex, e);

      /// <summary>Called by MainWindow once the window is open and sized to restore all saved UI state.</summary>
      public void OnWindowOpened(double windowWidth)
      {
         UpdateThemeMenus();
         var cfg = ConfigurationManager.Instance;
         expCameraControl.IsExpanded = cfg.IsCameraControlExpanded;
         expCameraSelection.IsExpanded = cfg.IsCameraSelectionExpanded;
         expPresetSelection.IsExpanded = cfg.IsPresetSelectionExpanded;
         expAlarms.IsExpanded = cfg.IsAlarmsExpanded;
         expPlaybackSelection.IsExpanded = cfg.IsPlaybackSelectionExpanded;
         expDownloadRecording.IsExpanded = cfg.IsDownloadRecordingExpanded;

         // Save settings whenever an expander is toggled so state persists even if the app is killed
         foreach (var exp in new[] { expCameraControl, expCameraSelection, expPresetSelection, expAlarms, expPlaybackSelection, expDownloadRecording })
         {
            exp.PropertyChanged += (_, e) =>
            {
               if (e.Property.Name == nameof(Expander.IsExpanded) && !mIsLoadingSettings)
                  SaveSettings();
            };
         }

         SetSidebarCollapsed(cfg.IsSidebarCollapsed, persistSetting: false);
         ApplyResponsiveSidebarLayout(windowWidth);

         if (cfg.AutoConnectOnStartup)
            btnConnect_Click(null, null);
      }

      // ── Constructor ────────────────────────────────────────────────────────

      public MainView()
      {
         InitializeComponent();

         // Wire child control events
         WireVideoPanel();
         WireCameraSelectorPanel();
         WireAlarmSelectorPanel();
         WirePtzControlPanel();
         WirePlaybackControlPanel();

         ClearPresetDropdown();
         playbackControlPanel.ClearRecordings(false);

         InitializeNavigationService();

         mIsLoadingSettings = true;
         LoadSettings();
         mIsLoadingSettings = false;

         InitializeSidebarMenuHeights(useSavedHeights: true);
         UpdateEnabled();
         UpdateUserInitial();

         mApp = (App)Application.Current;
         if (mApp != null)
            mApp.ThemeApplied += OnThemeApplied;
      }

      // ── Wire child controls ────────────────────────────────────────────────

      private void WireVideoPanel()
      {
         videoLayoutPanel.SubChannelChanged += (_, isSubChannel) =>
         {
            if (mIsLoadingSettings) return;
            if (IsStarted && mCurrentCamera != null)
               SelectCamera(mCurrentCamera.ComponentNumber, isSubChannel ? 2 : 1);
         };
         videoLayoutPanel.CameraSelectedFromMenu += (_, cameraNo) => SelectCamera(cameraNo, videoLayoutPanel.GetStreamNo());
         videoLayoutPanel.LayoutChanged += (_, args) =>
         {
            if (!mIsLoadingSettings) SaveSettings();
         };
         videoLayoutPanel.ActiveSlotChanged += (_, slotIndex) =>
         {
            var slotCamera = videoLayoutPanel.GetActiveCamera();
            if (slotCamera != null)
               CurrentCamera = slotCamera;
         };
         videoLayoutPanel.VlcLogGenerated += (_, e) => WriteMessageLog(MessageSource.LibVlc, e.Message, e.Level);
         videoLayoutPanel.SetCameraListProvider(
            () => mFlexApiClient?.GetCameraList(),
            () => cameraSelectorPanel.Items);
      }

       private void WireCameraSelectorPanel()
      {
         cameraSelectorPanel.CameraSelected += (_, cameraNo) =>
         {
            int streamNo = videoLayoutPanel.GetStreamNo();
            SelectCamera(cameraNo, streamNo);
         };
         cameraSelectorPanel.LayoutChanged += (_, _) => { if (!mIsLoadingSettings) SaveSettings(); };
         cameraSelectorPanel.InitializeDragSupport();
      }

      private void WireAlarmSelectorPanel()
      {
         alarmSelectorPanel.AlarmSelected += (_, alarmNo) => SelectAlarm(alarmNo);
         alarmSelectorPanel.AlarmEditRequested += (_, alarm) =>
         {
            SelectAlarm(alarm.ComponentNumber);
            var alarmPage = new AlarmActionPage(mCurrentAlarm) { NavigationService = mNavigationService };
            mNavigationService.NavigateTo(alarmPage);
         };
         alarmSelectorPanel.LayoutChanged += (_, _) => { if (!mIsLoadingSettings) SaveSettings(); };
         alarmSelectorPanel.ActiveAlarmStateChanged += (_, _) => UpdateAlarmSidebarIcon();
      }

      private void WirePtzControlPanel()
      {
         ptzControlPanel.AbsolutePositionRequested += (_, _) =>
         {
            if (mFlexApiClient == null || mCurrentCamera == null) return;
            var page = new AbsolutePositionPage(mCurrentCamera) { NavigationService = mNavigationService };
            mNavigationService.NavigateTo(page);
         };
         ptzControlPanel.CameraLockRequested += (_, _) =>
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
         };
      }

      private void WirePlaybackControlPanel()
      {
         playbackControlPanel.PlayRequested += (_, url) =>
         {
            if (string.IsNullOrEmpty(url)) return;
            var cameraNo = mCurrentCamera?.ComponentNumber ?? 0;
            videoLayoutPanel.ShowPlaybackStream(url, cameraNo);
            IsPlaybackStarted = true;
            UpdateEnabled();
         };
         playbackControlPanel.StopRequested += (_, _) =>
         {
            IsPlaybackStarted = false;
            if (mCurrentCamera != null)
               SelectCamera(mCurrentCamera.ComponentNumber, videoLayoutPanel.GetStreamNo());
            UpdateEnabled();
         };
         playbackControlPanel.GotoTimeRequested += (_, url) =>
         {
            if (string.IsNullOrEmpty(url)) return;
            var cameraNo = mCurrentCamera?.ComponentNumber ?? 0;
            videoLayoutPanel.ShowPlaybackStream(url, cameraNo);
            IsPlaybackStarted = true;
            UpdateEnabled();
         };
         playbackControlPanel.PickDateRequested += (_, _) => OpenDatePicker();
         playbackControlPanel.PickTimeRequested += (_, _) => OpenTimePicker();
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
         var settings = ConfigurationManager.Instance;

         double ResolveTargetHeight(double savedHeight, double currentHeight)
         {
            var preferred = useSavedHeights ? savedHeight : currentHeight;
            if (preferred <= 0) preferred = defaultHeight;
            return Math.Clamp(preferred, minHeight, maxHeight);
         }

         var cameraTarget = ResolveTargetHeight(settings.CameraSidebarMenuHeight, cameraSelectorPanel.ListHeight);
         var alarmTarget = ResolveTargetHeight(settings.AlarmSidebarMenuHeight, alarmSelectorPanel.ListHeight);

         cameraSelectorPanel.InitializeHeight(cameraTarget, minHeight, maxHeight);
         alarmSelectorPanel.InitializeHeight(alarmTarget, minHeight, maxHeight);
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
            messageLogPanel.RefreshColors();
            videoLayoutPanel.RefreshHeaderState(IsStarted, mCurrentCamera, IsPlayback, IsPlaybackStarted);

            if (mPendingConnectAfterSettings)
            {
               mPendingConnectAfterSettings = false;
               var cfg = ConfigurationManager.Instance;
               if (!IsStarted && cfg.HasAnyConnectionAlternative() && !string.IsNullOrWhiteSpace(cfg.User) && !string.IsNullOrWhiteSpace(cfg.Password))
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
         if (anyOverlay)
            videoLayoutPanel.DetachVideoSurfaces();
         else
            videoLayoutPanel.ReattachVideoSurfaces();
      }

      // ── Connect / Disconnect ───────────────────────────────────────────────

      private async void btnConnect_Click(object sender, RoutedEventArgs e)
      {
         if (mIsConnecting || IsStarted) return;

         SaveSettings();
         if (!ValidateCanConnect()) return;

         var cfg = ConfigurationManager.Instance;
         var endpoints = cfg.GetConnectionAlternatives()
            .Where(c => !string.IsNullOrWhiteSpace(c.Host) && !string.IsNullOrWhiteSpace(c.Port))
            .ToList();

         if (endpoints.Count == 0)
         {
            WriteMessageLog(MessageSource.Config, "No valid host/port alternatives configured", LogLevel.Error);
            return;
         }

         videoLayoutPanel.ResetHeader();
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
                   Password = cfg.Password,
                   User = cfg.User,
                   UseHttps = cfg.UseHttps,
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
               if (connectedEndpoint != null)
                     cfg.PromoteConnectionAlternativeToTop(connectedEndpoint.Host, connectedEndpoint.Port);

               IsStarted = true;
               WriteMessageLog(MessageSource.FlexApi, $"Client started on {mConnectedEndpointDisplay}.", LogLevel.Notice);
               CurrentLoggedInUser = loggedInUser;
               cameraSelectorPanel.Fill(cameraList);
               alarmSelectorPanel.Fill(alarmList, GetBrushForStatus);
               CheckApiVersion(apiVersion);
               playbackControlPanel.ClearRecordings(ApiSupportsPlayback);
               ClearPresetDropdown();
               UpdateConnectionStatus(connectedEndpoint?.Host, connectedEndpoint?.Port);

               var slotCameras = cfg.SlotCameras;
               int streamNo = cfg.PreferSubChannel ? 2 : 1;
               bool anyRestored = false;
               foreach (var kvp in slotCameras)
               {
                  if (kvp.Value == 0) continue;
                  var slotCamera = cameraList?.FirstOrDefault(c => c.ComponentNumber == kvp.Value);
                  if (slotCamera == null) continue;
                  anyRestored = true;
                  if (kvp.Key == 0)
                     SelectCamera(kvp.Value, streamNo);
                  else
                     videoLayoutPanel.RestoreSlotStream(kvp.Key, slotCamera, streamNo);
               }
               if (!anyRestored)
                  WriteMessageLog(MessageSource.Config, "No saved cameras to restore. Select cameras to persist them for next session.", LogLevel.Notice);
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
         var error = ConfigurationManager.Instance.ValidateConnectionSettings();
         if (error != null)
         { WriteMessageLog(MessageSource.Config, error, LogLevel.Error); return false; }
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

         videoLayoutPanel.ClearStream();
         if (txtAppSubtitle != null) txtAppSubtitle.Text = "Not connected";

         IsCameraSelected = false;
         CurrentCamera = null;
         CurrentAlarm = null;
         ClearPresetDropdown();
         playbackControlPanel.ClearRecordings(false);
         cameraSelectorPanel.Clear();
         alarmSelectorPanel.Clear();
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
         if (cameraSelectorPanel != null) cameraSelectorPanel.IsEnabled = canUseConnectedFeatures;
         if (alarmSelectorPanel != null) alarmSelectorPanel.IsEnabled = canUseConnectedFeatures;

         videoLayoutPanel.UpdateSubChannelEnabled(canUseConnectedFeatures && !IsPlayback && mCurrentCamera != null && !string.IsNullOrEmpty(mCurrentCamera?.Stream2Resolution));

         if (grpSelectPreset != null) grpSelectPreset.IsEnabled = canUseConnectedFeatures;
         if (playbackControlPanel != null) playbackControlPanel.IsEnabled = canUseConnectedFeatures && ApiSupportsPlayback;
         if (ptzControlPanel != null) ptzControlPanel.ControlGroup.IsEnabled = canUseConnectedFeatures && mCurrentCamera != null;

         playbackControlPanel.UpdateButtonStates(canUseConnectedFeatures, IsCameraSelected, IsPlaybackStarted, ApiSupportsPlayback, IsPlayback);
         if (btnDownload != null) btnDownload.IsEnabled = canUseConnectedFeatures && ApiSupportsPlayback;

         ptzControlPanel.UpdateButtonStates();
      }

      // ── Camera ─────────────────────────────────────────────────────────────

      private Camera CurrentCamera
      {
         get => mCurrentCamera;
         set
         {
            mCurrentCamera = value;
            ptzControlPanel.SetCamera(mCurrentCamera);
            cameraSelectorPanel.SyncSelection(mCurrentCamera);

            if (mCurrentCamera != null)
            {
               IsCameraSelected = true;
               FillSelectPresetList();
               if (ApiSupportsPlayback)
                  playbackControlPanel.FillRecordings(mCurrentCamera, mViewerID);
            }
            else
            {
               ClearPresetDropdown();
               playbackControlPanel.ClearRecordings(ApiSupportsPlayback);
            }

            UpdateEnabled();
         }
      }

      private Alarm CurrentAlarm { get => mCurrentAlarm; set { mCurrentAlarm = value; UpdateEnabled(); } }
      private User CurrentLoggedInUser { get => mCurrentLoggedInUser; set { mCurrentLoggedInUser = value; UpdateEnabled(); } }

      private void SelectCamera(int cameraNo, int streamNo)
      {
         if (mFlexApiClient == null) return;
         var camera = mFlexApiClient.GetCamera(cameraNo);
         if (camera == null) return;

         CurrentCamera = camera;
         videoLayoutPanel.ShowLiveStream(camera, streamNo);
         ConfigurationManager.Instance.SlotCameras = videoLayoutPanel.GetSlotCameraMap();
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
         ptzControlPanel.ClearLabel();
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

      // ── Alarm icon ─────────────────────────────────────────────────────────

      private void UpdateAlarmSidebarIcon()
      {
         bool hasActiveAlarm = alarmSelectorPanel.HasActiveAlarm;
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

      // ── API Version ────────────────────────────────────────────────────────

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

         UpdateEnabled();
      }

      // ── Date/Time picker overlay ───────────────────────────────────────────

      private void OpenDatePicker()
      {
         mPickerOverlayMode = PickerOverlayMode.Date;
         if (txtPickerOverlayTitle != null) txtPickerOverlayTitle.Text = "Select Date";
         if (txtPickerOverlaySubtitle != null) txtPickerOverlaySubtitle.Text = "Choose a date for playback";
         if (overlayDatePicker != null) overlayDatePicker.IsVisible = true;
         if (overlayTimePicker != null) overlayTimePicker.IsVisible = false;
         if (overlayDatePicker != null)
            overlayDatePicker.SelectedDate = DateTime.TryParse(playbackControlPanel.DateText, out DateTime d) ? new DateTimeOffset(d) : new DateTimeOffset(DateTime.Now);
         ShowPickerOverlay();
      }

      private void OpenTimePicker()
      {
         mPickerOverlayMode = PickerOverlayMode.Time;
         if (txtPickerOverlayTitle != null) txtPickerOverlayTitle.Text = "Select Time";
         if (txtPickerOverlaySubtitle != null) txtPickerOverlaySubtitle.Text = "Choose a time for playback";
         if (overlayDatePicker != null) overlayDatePicker.IsVisible = false;
         if (overlayTimePicker != null)
         {
            overlayTimePicker.IsVisible = true;
            overlayTimePicker.SelectedTime = TimeSpan.TryParse(playbackControlPanel.TimeText, out TimeSpan t) ? t : DateTime.Now.TimeOfDay;
         }
         ShowPickerOverlay();
      }

      private void btnPickerOverlayApply_Click(object sender, RoutedEventArgs e)
      {
         if (mPickerOverlayMode == PickerOverlayMode.Date && overlayDatePicker?.SelectedDate.HasValue == true)
            playbackControlPanel.DateText = overlayDatePicker.SelectedDate.Value.ToString("yyyy-MM-dd");
         else if (mPickerOverlayMode == PickerOverlayMode.Time && overlayTimePicker?.SelectedTime.HasValue == true)
            playbackControlPanel.TimeText = overlayTimePicker.SelectedTime.Value.ToString(@"hh\:mm\:ss");

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

      // ── Messages (delegate to MessageLogPanel) ─────────────────────────────

      public void WriteMessageLog(MessageSource source, string strMessage, LogLevel level)
      {
         messageLogPanel.WriteMessageLog(source, strMessage, level);
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
            ConfigurationManager.Instance.IsSidebarCollapsed = collapsed;
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
         SetSidebarCollapsed(ConfigurationManager.Instance.IsSidebarCollapsed, persistSetting: false);
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
         var cfg = ConfigurationManager.Instance;
         videoLayoutPanel.SetSubChannelChecked(cfg.PreferSubChannel, suppressEvent: true);
         UpdateUserInitial();

         var filters = new Dictionary<AlarmGeneralStatus, bool>
         {
            { AlarmGeneralStatus.Active, cfg.ShowActiveAlarms },
            { AlarmGeneralStatus.Tampered, cfg.ShowTamperedAlarms },
            { AlarmGeneralStatus.Acknowledged, cfg.ShowAcknowledgedAlarms },
            { AlarmGeneralStatus.Inactive, cfg.ShowPassiveAlarms },
            { AlarmGeneralStatus.Disabled, cfg.ShowDisabledAlarms }
         };
         alarmSelectorPanel.SetStatusFilters(filters);

         // Restore the saved layout
         if (!string.IsNullOrWhiteSpace(cfg.SelectedLayout))
            videoLayoutPanel.SetLayout(cfg.SelectedLayout);
      }

       private void SaveSettings()
      {
         var cfg = ConfigurationManager.Instance;
         if (expCameraControl != null)   cfg.IsCameraControlExpanded = expCameraControl.IsExpanded;
         if (expCameraSelection != null) cfg.IsCameraSelectionExpanded = expCameraSelection.IsExpanded;
         if (expPresetSelection != null) cfg.IsPresetSelectionExpanded = expPresetSelection.IsExpanded;
         if (expAlarms != null)          cfg.IsAlarmsExpanded = expAlarms.IsExpanded;
         if (expPlaybackSelection != null) cfg.IsPlaybackSelectionExpanded = expPlaybackSelection.IsExpanded;
         if (expDownloadRecording != null) cfg.IsDownloadRecordingExpanded = expDownloadRecording.IsExpanded;

         cfg.CameraSidebarMenuHeight = cameraSelectorPanel.ListHeight;
         cfg.AlarmSidebarMenuHeight = alarmSelectorPanel.ListHeight;

         var alarmFilters = alarmSelectorPanel.GetStatusFilters();
         cfg.ShowActiveAlarms      = alarmFilters.GetValueOrDefault(AlarmGeneralStatus.Active, true);
         cfg.ShowTamperedAlarms    = alarmFilters.GetValueOrDefault(AlarmGeneralStatus.Tampered, true);
         cfg.ShowAcknowledgedAlarms = alarmFilters.GetValueOrDefault(AlarmGeneralStatus.Acknowledged, true);
         cfg.ShowPassiveAlarms     = alarmFilters.GetValueOrDefault(AlarmGeneralStatus.Inactive, true);
         cfg.ShowDisabledAlarms    = alarmFilters.GetValueOrDefault(AlarmGeneralStatus.Disabled, true);

         cfg.SelectedLayout = videoLayoutPanel.CurrentLayoutKey;
         if (IsStarted)
            cfg.SlotCameras = videoLayoutPanel.GetSlotCameraMap();
      }

      // ── User Profile / Theme ───────────────────────────────────────────────

      private bool IsDarkMode => mApp?.CurrentTheme?.IsDark ?? true;

      private void UpdateUserInitial()
      {
         string username = ConfigurationManager.Instance.User?.Trim() ?? "";
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
            if (IsStarted) tip += $"\n{mFlexApiClient.SystemName}: {mFlexApiClient.SystemVersion}";
            ToolTip.SetTip(btnUserProfile, tip);
            btnUserProfile.Background = IsStarted
               ? GetBrushResource("UserProfileConnectedBackground", "#006BA1")
               : GetBrushResource("UserProfileDisconnectedBackground", "#808080");
         }
      }

      private void btnUserProfile_Click(object sender, RoutedEventArgs e) => PopulateThemeSelectionMenuItems();

      private void menuItemLogin_Click(object sender, RoutedEventArgs e)
      {
         var cfg = ConfigurationManager.Instance;
         if (!cfg.HasAnyConnectionAlternative() || string.IsNullOrWhiteSpace(cfg.User) || string.IsNullOrWhiteSpace(cfg.Password))
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

      private void menuItemMessageLog_Click(object sender, RoutedEventArgs e) => OpenMessageLogPage();

      private void OpenMessageLogPage()
      {
         if (navigationHost?.Content is MessageLogPage) return;
         var page = new MessageLogPage(messageLogPanel) { NavigationService = mNavigationService };
         mNavigationService.NavigateTo(page);
      }

      private void OpenSettingsPage()
      {
         if (navigationHost?.Content is SettingsPage) return;
         var settingsPage = new SettingsPage { NavigationService = mNavigationService };
         mNavigationService.NavigateTo(settingsPage);
      }

      private void HandleEscapeNavigation()
      {
         if (navigationHost?.Content is SettingsPage sp) { sp.CancelAndGoBack(); return; }
         if (navigationHost?.Content is MessageLogPage) { mNavigationService.GoBack(); return; }
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
         var selectedKey = ConfigurationManager.Instance.GetPreferredThemeKey();
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
         messageLogPanel.RefreshColors();
         videoLayoutPanel.RefreshHeaderState(IsStarted, mCurrentCamera, IsPlayback, IsPlaybackStarted);
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
   }

   public enum MessageSource { FlexApi, LibVlc, Config }
}
