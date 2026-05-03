using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using LibVLCSharp.Shared;
using Vao.Client;
using Vao.Client.Components;
using Vao.Client.Enum;
using Vao.Sample.Navigation;
using Vao.Sample.Pages;

namespace Vao.Sample;

public partial class AndroidMainView : UserControl
{
   private FlexRApiClient mApiClient;
   private bool mIsConnecting;
   private bool mIsConnected;
   private bool mIsApiSupportsPlayback;
   private bool mIsPlaybackStarted;
   private bool mIsUpdatingCameraSelection;
   private bool mIsUpdatingAlarmSelection;
   private bool mIsMessagesCollapsed;
   private Camera mCurrentCamera;
   private Alarm mCurrentAlarm;
   private User mCurrentLoggedInUser;
   private NavigationService mNavigationService;

   private enum PickerOverlayMode { None, Date, Time }
   private PickerOverlayMode mPickerOverlayMode;

   private readonly ObservableCollection<MessageItem> mMessages = new();
   private readonly ObservableCollection<MessageItem> mFilteredMessages = new();
   private readonly ObservableCollection<CameraSelectionItem> mCameraSelectionItems = new();
   private readonly ObservableCollection<CameraSelectionItem> mFilteredCameraSelectionItems = new();
   private readonly ObservableCollection<AlarmSelectionItem> mAlarmSelectionItems = new();
   private readonly ObservableCollection<AlarmSelectionItem> mFilteredAlarmSelectionItems = new();

   public AndroidMainView()
   {
      InitializeComponent();
      LoadSettingsIntoForm();
      InitializeNavigationService();

      lstMessages.ItemsSource = mFilteredMessages;
      lstCameraSelection.ItemsSource = mFilteredCameraSelectionItems;
      lstAlarmSelection.ItemsSource = mFilteredAlarmSelectionItems;

      RegisterPtzButtonHandlers();
      ClearPresetDropdown();
      ClearRecordingDropdown();
      UpdateEnabled();
   }

   // ── Settings ──────────────────────────────────────────────────────────────

   private void LoadSettingsIntoForm()
   {
      var s = AppSettings.Default;
      var conn = s.GetSelectedConnectionAlternative();
      if (txtHost != null) txtHost.Text = conn.Host ?? "";
      if (txtPort != null) txtPort.Text = conn.Port ?? "444";
      if (txtUser != null) txtUser.Text = s.User ?? "";
      if (txtPassword != null) txtPassword.Text = s.Password ?? "";
      if (chkUseHttps != null) chkUseHttps.IsChecked = s.UseHttps;
   }

   // ── Navigation Service ────────────────────────────────────────────────────

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
      if (!e.IsOverlayVisible)
         UpdateEnabled();
   }

   // ── Connect / Disconnect ──────────────────────────────────────────────────

   private async void btnConnect_Click(object sender, RoutedEventArgs e)
   {
      if (mIsConnecting || mIsConnected)
         return;

      var host = txtHost?.Text?.Trim() ?? "";
      var port = string.IsNullOrWhiteSpace(txtPort?.Text) ? "444" : txtPort.Text.Trim();
      var user = txtUser?.Text?.Trim() ?? "";
      var password = txtPassword?.Text ?? "";
      var useHttps = chkUseHttps?.IsChecked == true;

      if (string.IsNullOrWhiteSpace(host))
      {
         ShowConnectStatus("Host is required.", isError: true);
         return;
      }

      if (string.IsNullOrWhiteSpace(user))
      {
         ShowConnectStatus("Username is required.", isError: true);
         return;
      }

      mIsConnecting = true;
      if (btnConnect != null) btnConnect.IsEnabled = false;
      ShowConnectStatus($"Connecting to {host}:{port}...", isError: false);

      var s = AppSettings.Default;
      s.User = user;
      s.Password = password;
      s.UseHttps = useHttps;
      s.SetConnectionAlternatives(new[] { new ConnectionAlternative { Host = host, Port = port } });
      s.Save();

      bool started = false;
      List<Camera> cameraList = null;
      List<Alarm> alarmList = null;
      User loggedInUser = null;
      FlexRApiClient candidate = null;

      try
      {
         candidate = new FlexRApiClient
         {
            Host = host,
            Port = port,
            User = user,
            Password = password,
            UseHttps = useHttps,
            IgnoreCertificateErrors = true,
            ConnectionTimeoutMs = 10000
         };
         candidate.OnMessage += OnApiClientMessage;

         await Task.Run(() =>
         {
            started = candidate.StartClient();
            if (started)
            {
               loggedInUser = candidate.GetLoggedInUserInfo();
               cameraList = candidate.GetCameraList();
               alarmList = candidate.GetAlarmList();
               var apiVersion = candidate.GetApiVersion();
               if (apiVersion != null)
               {
                  var v = new Version(apiVersion.MajorVersion, apiVersion.MinorVersion);
                  if (v >= new Version(1, 1))
                     mIsApiSupportsPlayback = true;
               }
            }
         });
      }
      catch (Exception ex)
      {
         ShowConnectStatus($"Connection failed: {ex.Message}", isError: true);
         candidate?.StopClient();
         mIsConnecting = false;
         if (btnConnect != null) btnConnect.IsEnabled = true;
         return;
      }

      if (!started)
      {
         ShowConnectStatus("Could not connect. Check host, port, and credentials.", isError: true);
         candidate?.StopClient();
         mIsConnecting = false;
         if (btnConnect != null) btnConnect.IsEnabled = true;
         return;
      }

      mApiClient = candidate;
      mIsConnected = true;
      mIsConnecting = false;
      mCurrentLoggedInUser = loggedInUser;

      FillCameraList(cameraList);
      FillAlarmList(alarmList);
      ClearPresetDropdown();
      ClearRecordingDropdown();
      TransitionToConnectedState(host, port);
      UpdateEnabled();

      if (AppSettings.Default.CurrentCamera != 0)
         SelectCamera(AppSettings.Default.CurrentCamera);

      WriteMessageLog("Connected to " + host + ":" + port, LogLevel.Notice);
   }

   private void btnDisconnect_Click(object sender, RoutedEventArgs e)
   {
      if (mApiClient != null)
      {
         mApiClient.OnMessage -= OnApiClientMessage;
         mApiClient.StopClient();
         mApiClient = null;
      }
      mIsConnected = false;
      mIsApiSupportsPlayback = false;
      mIsPlaybackStarted = false;
      mCurrentCamera = null;
      mCurrentAlarm = null;

      ClearCameraSelection();
      ClearAlarmSelection();
      ClearPresetDropdown();
      ClearRecordingDropdown();
      TransitionToDisconnectedState();
      UpdateEnabled();
   }

   private void OnApiClientMessage(object sender, Vao.Client.MessageEventArgs e)
   {
      if (e.StatusMessage is Vao.Client.Components.StatusMessage sm)
      {
         var lvl = sm.Level switch
         {
            Vao.Client.Enum.MessageLevel.Error   => LogLevel.Error,
            Vao.Client.Enum.MessageLevel.Warning => LogLevel.Warning,
            Vao.Client.Enum.MessageLevel.Debug   => LogLevel.Debug,
            _                                    => LogLevel.Notice
         };
         WriteMessageLog($"{sm.Timestamp} [{sm.Type}] {sm.Message}", lvl);
      }
   }

   // ── UpdateEnabled ─────────────────────────────────────────────────────────

   private void UpdateEnabled()
   {
      bool connected = mIsConnected && !mIsConnecting;

      if (pnlCameraSelection != null) pnlCameraSelection.IsEnabled = connected;
      if (pnlAlarmsSelection != null) pnlAlarmsSelection.IsEnabled = connected;
      if (grpSelectPreset != null) grpSelectPreset.IsEnabled = connected;
      if (grpSelectPlayback != null) grpSelectPlayback.IsEnabled = connected && mIsApiSupportsPlayback;
      if (grpCameraControl != null) grpCameraControl.IsEnabled = connected && mCurrentCamera != null;
      if (btnDownload != null) btnDownload.IsEnabled = connected && mIsApiSupportsPlayback;

      if (btnStopPlayback != null) btnStopPlayback.IsEnabled = connected && mIsPlaybackStarted && mIsApiSupportsPlayback;
      if (btnPlayPlayback != null) btnPlayPlayback.IsEnabled = connected && mCurrentCamera != null && !mIsPlaybackStarted && mIsApiSupportsPlayback;
      if (btnGotoTime != null) btnGotoTime.IsEnabled = connected && mCurrentCamera != null && mIsApiSupportsPlayback;

      UpdateCameraControlButtons();
   }

   private void UpdateCameraControlButtons()
   {
      if (btnPanLeft != null) btnPanLeft.IsEnabled = mCurrentCamera?.HasPanTiltControl ?? false;
      if (btnPanRight != null) btnPanRight.IsEnabled = mCurrentCamera?.HasPanTiltControl ?? false;
      if (btnTiltUp != null) btnTiltUp.IsEnabled = mCurrentCamera?.HasPanTiltControl ?? false;
      if (btnTiltDown != null) btnTiltDown.IsEnabled = mCurrentCamera?.HasPanTiltControl ?? false;
      if (btnZoomIn != null) btnZoomIn.IsEnabled = mCurrentCamera?.HasLensControl ?? false;
      if (btnZoomOut != null) btnZoomOut.IsEnabled = mCurrentCamera?.HasLensControl ?? false;
      if (btnFocusFar != null) btnFocusFar.IsEnabled = mCurrentCamera?.HasLensControl ?? false;
      if (btnFocusNear != null) btnFocusNear.IsEnabled = mCurrentCamera?.HasLensControl ?? false;

      bool hasPtzOrLens = (mCurrentCamera?.HasPanTiltControl ?? false) || (mCurrentCamera?.HasLensControl ?? false);
      if (btnAbsolutePosition != null) btnAbsolutePosition.IsEnabled = hasPtzOrLens;
      if (btnGotoPreset != null) btnGotoPreset.IsEnabled = hasPtzOrLens;
   }

   // ── Camera Selection ──────────────────────────────────────────────────────

   private void FillCameraList(List<Camera> cameras)
   {
      ClearCameraSelection();
      if (cameras == null) return;
      foreach (var c in cameras.OrderBy(c => c.ComponentNumber))
         mCameraSelectionItems.Add(new CameraSelectionItem(c));
      ApplyCameraSearchFilter();
   }

   private void ClearCameraSelection()
   {
      mCameraSelectionItems.Clear();
      mFilteredCameraSelectionItems.Clear();
      mIsUpdatingCameraSelection = true;
      lstCameraSelection.SelectedItem = null;
      mIsUpdatingCameraSelection = false;
      if (txtCameraSearch != null) txtCameraSearch.Text = string.Empty;
   }

   private void txtCameraSearch_TextChanged(object sender, TextChangedEventArgs e)
   {
      ApplyCameraSearchFilter();
   }

   private void ApplyCameraSearchFilter()
   {
      string query = txtCameraSearch?.Text?.Trim() ?? string.Empty;
      var filtered = string.IsNullOrWhiteSpace(query)
         ? mCameraSelectionItems.AsEnumerable()
         : mCameraSelectionItems.Where(i => i.Matches(query));

      mFilteredCameraSelectionItems.Clear();
      foreach (var item in filtered)
         mFilteredCameraSelectionItems.Add(item);

      SyncCameraSelectionWithCurrent();
   }

   private void SyncCameraSelectionWithCurrent()
   {
      mIsUpdatingCameraSelection = true;
      lstCameraSelection.SelectedItem = mCurrentCamera == null
         ? null
         : mFilteredCameraSelectionItems.FirstOrDefault(i => i.Camera?.ComponentNumber == mCurrentCamera.ComponentNumber);
      if (lstCameraSelection.SelectedItem != null)
         lstCameraSelection.ScrollIntoView(lstCameraSelection.SelectedItem);
      mIsUpdatingCameraSelection = false;
   }

   private void lstCameraSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
   {
      if (mIsUpdatingCameraSelection) return;
      if (lstCameraSelection.SelectedItem is not CameraSelectionItem selected || selected.Camera == null) return;
      SelectCamera(selected.Camera.ComponentNumber);
   }

   private void SelectCamera(int cameraNo)
   {
      if (mApiClient == null) return;
      var camera = mApiClient.GetCamera(cameraNo);
      if (camera == null) return;

      if (mCurrentCamera != null)
         mCurrentCamera.PropertyChanged -= Camera_PropertyChanged;

      mCurrentCamera = camera;
      mCurrentCamera.PropertyChanged += Camera_PropertyChanged;

      SyncCameraSelectionWithCurrent();

      if (lblCurrentCamera != null)
         lblCurrentCamera.Text = $"Camera: {camera.Name}";

      if (txtVideoHeader != null)
         txtVideoHeader.Text = $"LIVE - Camera {cameraNo}";
      if (txtCameraSubtitle != null)
         txtCameraSubtitle.Text = camera.Name;
      if (txtVideoPlaceholder != null)
         txtVideoPlaceholder.Text = $"\"{camera.Name}\" — video not available on this platform";

      var headerBrush = GetBrushResource("VideoHeaderLive", "#39B620");
      if (brdVideoHeader != null) brdVideoHeader.Background = headerBrush;

      FillPresetList();
      if (mIsApiSupportsPlayback) FillPlaybackList();

      AppSettings.Default.CurrentCamera = cameraNo;
      UpdateEnabled();
   }

   private void Camera_PropertyChanged(object sender, PropertyChangedEventArgs e)
   {
      Dispatcher.UIThread.Post(UpdateCameraControlButtons);
   }

   // ── Preset ────────────────────────────────────────────────────────────────

   private void FillPresetList()
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
      if (lblCurrentCamera != null) lblCurrentCamera.Text = "Camera: (No camera selected)";
      selPreset.ItemsSource = new List<string> { "No camera selected" };
      selPreset.SelectedIndex = 0;
   }

   private void btnGotoPreset_Click(object sender, RoutedEventArgs e)
   {
      if (selPreset.SelectedItem is Preset preset) preset.GotoPreset();
   }

   // ── Alarms ────────────────────────────────────────────────────────────────

   private void FillAlarmList(List<Alarm> alarms)
   {
      ClearAlarmSelection();
      if (alarms == null) return;

      foreach (var alarm in alarms.OrderBy(a => a.ComponentNumber))
      {
         var item = new AlarmSelectionItem(alarm, GetBrushForAlarmStatus(alarm.Status), GetTextForAlarmStatus(alarm.Status))
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
                  item.StatusBrush = GetBrushForAlarmStatus(alarm.Status);
                  item.StatusText = GetTextForAlarmStatus(alarm.Status);
                  item.AlarmIcon = GetIconForAlarmStatus(alarm.Status);
                  UpdateAlarmHeaderIcon();
               });
            }
         };
      }

      ApplyAlarmSearchFilter();
      UpdateAlarmHeaderIcon();
   }

   private void ClearAlarmSelection()
   {
      mAlarmSelectionItems.Clear();
      mFilteredAlarmSelectionItems.Clear();
      mIsUpdatingAlarmSelection = true;
      lstAlarmSelection.SelectedItem = null;
      mIsUpdatingAlarmSelection = false;
      if (txtAlarmSearch != null) txtAlarmSearch.Text = string.Empty;
   }

   private void txtAlarmSearch_TextChanged(object sender, TextChangedEventArgs e)
   {
      ApplyAlarmSearchFilter();
   }

   private void ApplyAlarmSearchFilter()
   {
      string query = txtAlarmSearch?.Text?.Trim() ?? string.Empty;
      var filtered = string.IsNullOrWhiteSpace(query)
         ? mAlarmSelectionItems.AsEnumerable()
         : mAlarmSelectionItems.Where(i => i.Matches(query));

      mFilteredAlarmSelectionItems.Clear();
      foreach (var item in filtered)
         mFilteredAlarmSelectionItems.Add(item);

      mIsUpdatingAlarmSelection = true;
      lstAlarmSelection.SelectedItem = mCurrentAlarm == null
         ? null
         : mFilteredAlarmSelectionItems.FirstOrDefault(i => i.Alarm?.ComponentNumber == mCurrentAlarm.ComponentNumber);
      mIsUpdatingAlarmSelection = false;
   }

   private void lstAlarmSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
   {
      if (mIsUpdatingAlarmSelection) return;
      if (lstAlarmSelection.SelectedItem is not AlarmSelectionItem selected || selected.Alarm == null) return;
      mCurrentAlarm = mApiClient?.GetSingleAlarm(selected.Alarm.ComponentNumber);
      UpdateEnabled();
   }

   private void btnAlarmEdit_Click(object sender, RoutedEventArgs e)
   {
      if (sender is not Button btn) return;
      var item = btn.DataContext as AlarmSelectionItem;
      if (item?.Alarm == null) return;
      mCurrentAlarm = mApiClient?.GetSingleAlarm(item.Alarm.ComponentNumber);
      var page = new AlarmActionPage(mApiClient, mCurrentAlarm, mCurrentLoggedInUser)
      {
         NavigationService = mNavigationService
      };
      mNavigationService.NavigateTo(page);
   }

   private void UpdateAlarmHeaderIcon()
   {
      bool hasActive = mAlarmSelectionItems.Any(i => i.Alarm?.Status == AlarmGeneralStatus.Active);
      if (iconAlarmHeader != null)
      {
         if (hasActive)
            iconAlarmHeader.Foreground = GetBrushForAlarmStatus(AlarmGeneralStatus.Active);
         else
            iconAlarmHeader.ClearValue(TextBlock.ForegroundProperty);
      }
   }

   private IBrush GetBrushForAlarmStatus(AlarmGeneralStatus status) => status switch
   {
      AlarmGeneralStatus.Active       => GetBrushResource("AlarmStatusActive", "#FF0000"),
      AlarmGeneralStatus.Inactive     => GetBrushResource("AlarmStatusInactive", "#808080"),
      AlarmGeneralStatus.Acknowledged => GetBrushResource("AlarmStatusAcknowledged", "#FFA500"),
      AlarmGeneralStatus.Tampered     => GetBrushResource("AlarmStatusTampered", "#FF8C00"),
      _                               => GetBrushResource("AlarmStatusDefault", "#78808080"),
   };

   private static string GetTextForAlarmStatus(AlarmGeneralStatus status) => status switch
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

   // ── PTZ ───────────────────────────────────────────────────────────────────

   private void RegisterPtzButtonHandlers()
   {
      var ptzButtons = new[] { btnPanLeft, btnPanRight, btnTiltUp, btnTiltDown, btnZoomIn, btnZoomOut, btnFocusFar, btnFocusNear };
      foreach (var btn in ptzButtons)
      {
         if (btn == null) continue;
         btn.AddHandler(PointerPressedEvent, OnPtzPointerPressed, RoutingStrategies.Bubble, handledEventsToo: true);
         btn.AddHandler(PointerReleasedEvent, OnPtzPointerReleased, RoutingStrategies.Bubble, handledEventsToo: true);
      }
   }

   private void OnPtzPointerPressed(object sender, PointerPressedEventArgs e)
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

   private void OnPtzPointerReleased(object sender, PointerReleasedEventArgs e)
   {
      mCurrentCamera?.PanTiltZoomStop();
   }

   // ── Absolute Position / Camera Lock ───────────────────────────────────────

   private void btnAbsolutePosition_Click(object sender, RoutedEventArgs e)
   {
      if (mApiClient == null || mCurrentCamera == null) return;
      var page = new AbsolutePositionPage(mApiClient, mCurrentCamera) { NavigationService = mNavigationService };
      mNavigationService.NavigateTo(page);
   }

   private void btnCameraLock_Click(object sender, RoutedEventArgs e)
   {
      if (mApiClient == null || mCurrentCamera == null) return;
      if (!mCurrentCamera.IsLocked)
      {
         var page = new CameraLockPage(mApiClient, mCurrentCamera) { NavigationService = mNavigationService };
         mNavigationService.NavigateTo(page);
      }
      else
      {
         mApiClient.SendUnlockCamera(mCurrentCamera.ComponentNumber);
      }
   }

   // ── Playback ──────────────────────────────────────────────────────────────

   private void FillPlaybackList()
   {
      var list = mCurrentCamera?.GetPlaybackInfoList(Guid.NewGuid());
      if (list != null && list.Count > 0)
      {
         selPlayback.ItemsSource = list;
         selPlayback.SelectedIndex = 0;
      }
      else
      {
         ClearRecordingDropdown();
      }
   }

   private void ClearRecordingDropdown()
   {
      selPlayback.ItemsSource = new List<string> { mIsApiSupportsPlayback && mIsConnected ? "No recorders" : "No camera selected" };
      selPlayback.SelectedIndex = 0;
   }

   private void btnPlayPlayback_Click(object sender, RoutedEventArgs e)
   {
      if (selPlayback.SelectedItem is PlaybackInfo recording)
      {
         WriteMessageLog($"Playback requested: {recording.PlaybackUrl}", LogLevel.Notice);
         if (txtVideoHeader != null) txtVideoHeader.Text = $"PLAYBACK - Camera {mCurrentCamera?.ComponentNumber}";
         if (brdVideoHeader != null) brdVideoHeader.Background = GetBrushResource("VideoHeaderPlayback", "#CA3C3D");
         mIsPlaybackStarted = true;
         UpdateEnabled();
      }
   }

   private void btnStopPlayback_Click(object sender, RoutedEventArgs e)
   {
      mIsPlaybackStarted = false;
      if (mCurrentCamera != null)
      {
         if (txtVideoHeader != null) txtVideoHeader.Text = $"LIVE - Camera {mCurrentCamera.ComponentNumber}";
         if (brdVideoHeader != null) brdVideoHeader.Background = GetBrushResource("VideoHeaderLive", "#39B620");
      }
      UpdateEnabled();
   }

   private void btnGotoTime_Click(object sender, RoutedEventArgs e)
   {
      if (selPlayback.SelectedItem is not PlaybackInfo recording) return;
      try
      {
         DateTime date = string.IsNullOrWhiteSpace(txtDatePlayback?.Text) || !DateTime.TryParse(txtDatePlayback.Text, out var d)
            ? DateTime.Now.Date : d.Date;
         TimeSpan time = string.IsNullOrWhiteSpace(txtTimePlayback?.Text) || !TimeSpan.TryParse(txtTimePlayback.Text, out var t)
            ? TimeSpan.Zero : t;
         var dt = date + time;
         WriteMessageLog($"Playback requested: {recording.PlaybackUrl}?start={dt:yyyyMMddHHmmss}", LogLevel.Notice);
         if (txtVideoHeader != null) txtVideoHeader.Text = $"PLAYBACK - Camera {mCurrentCamera?.ComponentNumber}";
         if (brdVideoHeader != null) brdVideoHeader.Background = GetBrushResource("VideoHeaderPlayback", "#CA3C3D");
         mIsPlaybackStarted = true;
         UpdateEnabled();
      }
      catch { }
   }

   // ── Date/Time picker overlay ──────────────────────────────────────────────

   private void btnPickDate_Click(object sender, RoutedEventArgs e)
   {
      mPickerOverlayMode = PickerOverlayMode.Date;
      if (txtPickerOverlayTitle != null) txtPickerOverlayTitle.Text = "Select Date";
      if (txtPickerOverlaySubtitle != null) txtPickerOverlaySubtitle.Text = "Choose a date for playback";
      if (overlayDatePicker != null) overlayDatePicker.IsVisible = true;
      if (overlayTimePicker != null) overlayTimePicker.IsVisible = false;
      if (pickerOverlay != null) pickerOverlay.IsVisible = true;
   }

   private void btnPickTime_Click(object sender, RoutedEventArgs e)
   {
      mPickerOverlayMode = PickerOverlayMode.Time;
      if (txtPickerOverlayTitle != null) txtPickerOverlayTitle.Text = "Select Time";
      if (txtPickerOverlaySubtitle != null) txtPickerOverlaySubtitle.Text = "Choose a time for playback";
      if (overlayDatePicker != null) overlayDatePicker.IsVisible = false;
      if (overlayTimePicker != null) overlayTimePicker.IsVisible = true;
      if (pickerOverlay != null) pickerOverlay.IsVisible = true;
   }

   private void btnPickerOverlayApply_Click(object sender, RoutedEventArgs e)
   {
      if (mPickerOverlayMode == PickerOverlayMode.Date && overlayDatePicker?.SelectedDate != null)
      {
         if (txtDatePlayback != null)
            txtDatePlayback.Text = overlayDatePicker.SelectedDate.Value.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
      }
      else if (mPickerOverlayMode == PickerOverlayMode.Time && overlayTimePicker?.SelectedTime != null)
      {
         if (txtTimePlayback != null)
            txtTimePlayback.Text = overlayTimePicker.SelectedTime.Value.ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture);
      }
      ClosePickerOverlay();
   }

   private void btnPickerOverlayCancel_Click(object sender, RoutedEventArgs e)
   {
      ClosePickerOverlay();
   }

   private void ClosePickerOverlay()
   {
      mPickerOverlayMode = PickerOverlayMode.None;
      if (pickerOverlay != null) pickerOverlay.IsVisible = false;
   }

   // ── Download ──────────────────────────────────────────────────────────────

   private void btnOpenDownloadWindow_Click(object sender, RoutedEventArgs e)
   {
      if (mApiClient == null) return;
      var page = new DownloadPage(mApiClient) { NavigationService = mNavigationService };
      mNavigationService.NavigateTo(page);
   }

   // ── Messages ──────────────────────────────────────────────────────────────

   private void WriteMessageLog(string message, LogLevel level)
   {
      if (!Dispatcher.UIThread.CheckAccess())
      {
         Dispatcher.UIThread.Post(() => WriteMessageLog(message, level));
         return;
      }

      var color = level switch
      {
         LogLevel.Error   => GetBrushResource("MessageErrorForeground", "#FF0000"),
         LogLevel.Warning => GetBrushResource("MessageWarningForeground", "#F0AA1F"),
         LogLevel.Debug   => GetBrushResource("MessageDebugForeground", "#ADD8E6"),
         _                => GetBrushResource("MessageDefaultForeground", "#FFFFFF"),
      };

      var strTime = DateTime.Now.ToString(CultureInfo.InvariantCulture);
      var item = new MessageItem
      {
         Text = $"{strTime} [{level,-7}] {message}",
         Color = color,
         Background = GetBrushResource("MessageLogBackground", "#00000000"),
         Source = MessageSource.FlexApi,
         Level = level
      };

      mMessages.Add(item);
      mFilteredMessages.Add(item);
      if (lstMessages != null && mFilteredMessages.Count > 0)
         lstMessages.ScrollIntoView(mFilteredMessages.Count - 1);
   }

   private void MessagesHeader_PointerPressed(object sender, PointerPressedEventArgs e)
   {
      mIsMessagesCollapsed = !mIsMessagesCollapsed;
      if (brdMessages != null)
      {
         brdMessages.IsVisible = !mIsMessagesCollapsed;
      }
      if (txtMessagesToggle != null)
         txtMessagesToggle.Text = mIsMessagesCollapsed ? "▶" : "▼";
   }

   // ── State transitions ─────────────────────────────────────────────────────

   private void TransitionToConnectedState(string host, string port)
   {
      if (pnlDisconnect != null) pnlDisconnect.IsVisible = false;
      if (pnlConnected != null) pnlConnected.IsVisible = true;
      if (btnDisconnect != null) btnDisconnect.IsVisible = true;
      if (txtConnectionStatus != null) txtConnectionStatus.Text = $"Connected · {host}:{port}";
      if (ellConnectionDot != null) ellConnectionDot.Fill = new SolidColorBrush(Color.Parse("#39B620"));
   }

   private void TransitionToDisconnectedState()
   {
      if (pnlDisconnect != null) pnlDisconnect.IsVisible = true;
      if (pnlConnected != null) pnlConnected.IsVisible = false;
      if (btnDisconnect != null) btnDisconnect.IsVisible = false;
      if (btnConnect != null) btnConnect.IsEnabled = true;
      if (txtConnectionStatus != null) txtConnectionStatus.Text = "Not connected";
      if (ellConnectionDot != null) ellConnectionDot.Fill = new SolidColorBrush(Color.Parse("#CA3C3D"));
      if (txtVideoHeader != null) txtVideoHeader.Text = "No Camera Selected";
      if (brdVideoHeader != null) brdVideoHeader.Background = GetBrushResource("VideoHeaderNeutral", "#1D3A4A");
   }

   private void ShowConnectStatus(string message, bool isError)
   {
      if (txtConnectStatus == null) return;
      txtConnectStatus.Text = message;
      txtConnectStatus.IsVisible = !string.IsNullOrEmpty(message);
      txtConnectStatus.Foreground = isError
         ? new SolidColorBrush(Color.Parse("#CA3C3D"))
         : new SolidColorBrush(Color.Parse("#39B620"));
   }

   // ── Helpers ───────────────────────────────────────────────────────────────

   private IBrush GetBrushResource(string key, string fallback)
   {
      if (this.TryFindResource(key, this.ActualThemeVariant, out var resource) && resource is IBrush b)
         return b;
      return new SolidColorBrush(Color.Parse(fallback));
   }

   // ── Old simple camera-selection handler (kept for reference, not wired) ───
   // (removed: lstCameras_SelectionChanged – replaced by lstCameraSelection_SelectionChanged)
}
