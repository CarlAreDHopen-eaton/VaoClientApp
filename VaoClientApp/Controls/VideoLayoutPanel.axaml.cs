using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Threading;
using LibVLCSharp.Avalonia;
using LibVLCSharp.Shared;
using Vao.Client.Components;
using Vao.Sample.Layouts;
using Vao.Sample.Utility;

namespace Vao.Sample.Controls
{
   public partial class VideoLayoutPanel : UserControl
   {
      private const int C_MAX_SLOT_COUNT = 4;

      private LibVLC mLibVlc;

      // ── Multi-slot VLC state ───────────────────────────────────────────────
      private readonly MediaPlayer[] mSlotMediaPlayers = new MediaPlayer[C_MAX_SLOT_COUNT];
      private readonly Grid[] mSlotVideoContainers = new Grid[C_MAX_SLOT_COUNT];
      private readonly VideoView[] mSlotVideoControls = new VideoView[C_MAX_SLOT_COUNT];
      private readonly SlotReconnectState[] mSlotReconnectStates = new SlotReconnectState[C_MAX_SLOT_COUNT];
      private readonly string[] mSlotRtspUrls = new string[C_MAX_SLOT_COUNT];
      private readonly bool[] mSlotIsStarted = new bool[C_MAX_SLOT_COUNT];
      private readonly DateTime[] mSlotLastFrameProgressUtc = new DateTime[C_MAX_SLOT_COUNT];
      private readonly DateTime[] mSlotStableSinceUtc = new DateTime[C_MAX_SLOT_COUNT];
      private readonly DateTime[] mSlotStartedUtc = new DateTime[C_MAX_SLOT_COUNT];
      private readonly long[] mSlotLastDisplayedPictures = new long[C_MAX_SLOT_COUNT];
      private readonly TextBlock[] mSlotReconnectTextBlocks = new TextBlock[C_MAX_SLOT_COUNT];
      private readonly TextBlock[] mSlotStatsTextBlocks = new TextBlock[C_MAX_SLOT_COUNT];
      private readonly DispatcherTimer mStatsTimer;
      private bool mIsStatsForNerdsVisible;
      private VideoDisplayMode mVideoDisplayMode = VideoDisplayMode.Fit;

      // ── Events (bubbled from inner VideoPanel) ─────────────────────────────

      /// <summary>Fired when the sub-channel toggle changes.</summary>
      public event EventHandler<bool> SubChannelChanged;

      /// <summary>Fired when the user selects a camera from the video context menu.</summary>
      public event EventHandler<int> CameraSelectedFromMenu;

      /// <summary>Fired when the active slot changes.</summary>
      public event EventHandler<int> ActiveSlotChanged;

      /// <summary>Fired when the layout changes (after internal handling is complete).</summary>
      public event EventHandler<LayoutChangeEventArgs> LayoutChanged;

      /// <summary>Fired when any overlay should cause detach/reattach of native surfaces.</summary>
      public event EventHandler<bool> AnyOverlayStateChanged;

      /// <summary>Fired when a VLC log message is generated.</summary>
      public event EventHandler<VlcLogEventArgs> VlcLogGenerated;

      /// <summary>Fired when the global video display mode changes.</summary>
      public event EventHandler<VideoDisplayMode> VideoDisplayModeChanged;

      public VideoLayoutPanel()
      {
         InitializeComponent();

         for (int i = 0; i < C_MAX_SLOT_COUNT; i++)
         {
            mSlotReconnectStates[i] = new SlotReconnectState();
            ResetSlotFrameTracking(i);
         }

         StartInitializeVlc();
         WireVideoPanel();

         mStatsTimer = new DispatcherTimer
         {
            Interval = TimeSpan.FromSeconds(1)
         };
         mStatsTimer.Tick += (_, _) => UpdateAllStatsOverlays();
         mStatsTimer.Start();
      }

      // ── Public API ─────────────────────────────────────────────────────────

      public VideoPanel InnerVideoPanel
      {
         get { return videoPanel; }
      }

      public Panel VideoSlot
      {
         get { return videoPanel.VideoSlot; }
      }

      public Panel GetVideoSlot(int slotIndex) => videoPanel.GetVideoSlot(slotIndex);

      public IReadOnlyList<Panel> GetAllActiveVideoSlots() => videoPanel.GetAllActiveVideoSlots();

      public string CurrentLayoutKey
      {
         get { return videoPanel.CurrentLayoutKey; }
      }

      public VideoLayoutDefinition CurrentLayout
      {
         get { return videoPanel.CurrentLayout; }
      }

      public bool IsSingleView
      {
         get { return videoPanel.IsSingleView; }
      }

      public int ActiveSlotIndex
      {
         get { return videoPanel.ActiveSlotIndex; }
      }

      public bool IsStatsForNerdsVisible
      {
         get { return mIsStatsForNerdsVisible; }
      }

      public VideoDisplayMode VideoDisplayMode
      {
         get { return mVideoDisplayMode; }
      }

      public void SetStatsForNerdsVisible(bool visible)
      {
         videoPanel.SetStatsForNerdsVisible(visible);
      }

      public void SetVideoDisplayMode(VideoDisplayMode displayMode)
      {
         videoPanel.SetVideoDisplayMode(displayMode);
      }

      public bool IsPlayback
      {
         get { return videoPanel.IsPlayback; }
      }

      public void SetLayout(string layoutKey) => videoPanel.SetLayout(layoutKey);

      public void SetActiveSlot(int slotIndex) => videoPanel.SetActiveSlot(slotIndex);

      public int GetStreamNo() => videoPanel.GetStreamNo();

      public Camera GetActiveCamera() => videoPanel.GetActiveCamera();

      public void HandleSlotDrop(int slotIndex, DragEventArgs e) => videoPanel.HandleSlotDrop(slotIndex, e);

      public void SetCameraListProvider(
         Func<List<Camera>> getCameraList,
         Func<IEnumerable<CameraSelectionItem>> getCameraSelectionItems)
         => videoPanel.SetCameraListProvider(getCameraList, getCameraSelectionItems);

      public void ShowLiveStream(Camera camera, int streamNo) => videoPanel.ShowLiveStream(camera, streamNo);

      public void ShowPlaybackStream(string url, int cameraNo) => videoPanel.ShowPlaybackStream(url, cameraNo);

      public void ClearStream() => videoPanel.ClearStream();

      public void ResetHeader() => videoPanel.ResetHeader();

      public void UpdateSubChannelEnabled(bool enabled) => videoPanel.UpdateSubChannelEnabled(enabled);

      public void SetSubChannelChecked(bool isChecked, bool suppressEvent = false)
         => videoPanel.SetSubChannelChecked(isChecked, suppressEvent);

      public Dictionary<int, int> GetSlotCameraMap() => videoPanel.GetSlotCameraMap();

      public void RestoreSlotStream(int slotIndex, Camera camera, int streamNo)
         => videoPanel.RestoreSlotStream(slotIndex, camera, streamNo);

      public void RefreshHeaderState(bool isStarted, Camera camera, bool isPlayback, bool isPlaybackStarted)
         => videoPanel.RefreshHeaderState(isStarted, camera, isPlayback, isPlaybackStarted);

      // ── Stream control ─────────────────────────────────────────────────────

      public void StartSlotStream(int slotIndex, string rtspUrl)
      {
         StartSlotStreamInternal(slotIndex, rtspUrl, false);
      }

      public void StopSlotStream(int slotIndex)
      {
         StopSlotStreamInternal(slotIndex, true, true);
      }

      private void StartSlotStreamInternal(int slotIndex, string rtspUrl, bool isRetryRestart)
      {
         mSlotRtspUrls[slotIndex] = rtspUrl;

         if (!isRetryRestart)
            ResetSlotReconnectState(slotIndex, true, true);

         if (mSlotIsStarted[slotIndex])
            StopSlotStreamInternal(slotIndex, !isRetryRestart, !isRetryRestart);

         var pnlVideo = videoPanel.GetVideoSlot(slotIndex);
         if (pnlVideo == null) return;

         var slotContainer = CreateSlotVideoContainer(slotIndex);
         var videoView = mSlotVideoControls[slotIndex];

         if (!pnlVideo.Children.Contains(slotContainer))
            pnlVideo.Children.Add(slotContainer);

         var uri = new Uri(rtspUrl);
         var media = new Media(mLibVlc, uri);
         if (ConfigurationManager.Instance.UseTcp) media.AddOption(":rtsp-tcp");

         var mp = new MediaPlayer(media)
         {
            EnableKeyInput = false,
            EnableMouseInput = false
         };
         mp.EncounteredError += (_, _) =>
            OnVlcLog($"LibVLC error on slot {slotIndex}.", LogLevel.Error);
         mp.Opening += (_, _) =>
            OnVlcLog($"LibVLC opening slot {slotIndex}: {mp.Media?.Mrl ?? ""}", LogLevel.Notice);
         mp.Playing += (_, _) => Dispatcher.UIThread.Post(() =>
         {
            ApplyVideoDisplayMode(slotIndex, mp);
            mSlotStartedUtc[slotIndex] = DateTime.UtcNow;
            ResetSlotFrameTracking(slotIndex);
            UpdateSlotStatsOverlay(slotIndex);
         });
         mp.Stopped += (_, _) => Dispatcher.UIThread.Post(() =>
         {
            ClearSlotStatsOverlay(slotIndex);
            if (IsSlotLiveStream(slotIndex) && mSlotIsStarted[slotIndex])
               EnsureReconnectActive(slotIndex, "Stream stopped unexpectedly.");
         });

         ApplyVideoDisplayMode(slotIndex, mp);

         mSlotMediaPlayers[slotIndex] = mp;
         videoView.MediaPlayer = mp;
         mp.Play();
         mSlotIsStarted[slotIndex] = true;
         mSlotStartedUtc[slotIndex] = DateTime.UtcNow;
         ResetSlotFrameTracking(slotIndex);
      }

      private void StopSlotStreamInternal(int slotIndex, bool clearUrl, bool clearReconnectState)
      {
         var mp = mSlotMediaPlayers[slotIndex];
         if (mp == null) return;

         mSlotMediaPlayers[slotIndex] = null;
         var pnlVideo = videoPanel.GetVideoSlot(slotIndex);
         var videoView = mSlotVideoControls[slotIndex];
         var slotContainer = mSlotVideoContainers[slotIndex];
         if (videoView != null)
         {
            videoView.MediaPlayer = null;
            if (pnlVideo != null && slotContainer != null && pnlVideo.Children.Contains(slotContainer))
               pnlVideo.Children.Remove(slotContainer);
            mSlotVideoContainers[slotIndex] = null;
            mSlotVideoControls[slotIndex] = null;
         }
         mSlotIsStarted[slotIndex] = false;
         if (clearUrl)
            mSlotRtspUrls[slotIndex] = null;
         if (clearReconnectState)
            ResetSlotReconnectState(slotIndex, true, true);
         ResetSlotFrameTracking(slotIndex);
         ClearSlotStatsOverlay(slotIndex);
         DisposeMediaPlayerAsync(mp);
      }

      /// <summary>Detach all video surfaces (call before showing overlays).</summary>
      public void DetachVideoSurfaces()
      {
         for (int i = 0; i < C_MAX_SLOT_COUNT; i++)
         {
            var slotView = mSlotVideoControls[i];
            var slotContainer = mSlotVideoContainers[i];
            if (slotView == null || slotContainer == null) continue;
            if (mSlotMediaPlayers[i] != null) slotView.MediaPlayer = null;
            var parent = slotContainer.Parent as Panel;
            parent?.Children.Remove(slotContainer);
         }
      }

      /// <summary>Reattach all video surfaces (call after hiding overlays).</summary>
      public void ReattachVideoSurfaces()
      {
         for (int i = 0; i < C_MAX_SLOT_COUNT; i++)
         {
            var slotView = mSlotVideoControls[i];
            var slotContainer = mSlotVideoContainers[i];
            if (slotView == null || slotContainer == null || !mSlotIsStarted[i]) continue;
            var slotPanel = videoPanel.GetVideoSlot(i);
            if (slotPanel == null) continue;

            if (!slotPanel.Children.Contains(slotContainer))
               slotPanel.Children.Add(slotContainer);

            slotView.MediaPlayer = mSlotMediaPlayers[i];
            ApplyVideoDisplayMode(i, mSlotMediaPlayers[i]);
         }
      }

      public void Dispose()
      {
         mStatsTimer.Stop();

         for (int i = 0; i < C_MAX_SLOT_COUNT; i++)
         {
            ResetSlotReconnectState(i, true, true);
            StopSlotStream(i);
         }

         if (mLibVlc != null)
         {
            mLibVlc.Log -= LibVlc_Log;
            mLibVlc.Dispose();
            mLibVlc = null;
         }
      }

      // ── Private ────────────────────────────────────────────────────────────

      private void StartInitializeVlc()
      {
         Core.Initialize();
         OnVlcLog("Loading VLC", LogLevel.Notice);
         var options = new[] { "-vv", "--rtsp-timeout=300", "--network-caching=300" };
         mLibVlc = new LibVLC(true, options);
         mLibVlc.Log += LibVlc_Log;
      }

      private void LibVlc_Log(object sender, LogEventArgs e)
      {
         if (e.Level == LogLevel.Debug) return;
         if (e.Message.ToLower() == "unsupported control query 3") return;
         OnVlcLog(e.Message, e.Level);
      }

      private void OnVlcLog(string message, LogLevel level)
      {
         VlcLogGenerated?.Invoke(this, new VlcLogEventArgs(message, level));
      }

      private Grid CreateSlotVideoContainer(int slotIndex)
      {
         var videoView = new VideoView { Focusable = false };
         mSlotVideoControls[slotIndex] = videoView;
         int capturedSlotIndex = slotIndex;
         var overlay = new Avalonia.Controls.Border
         {
            Background = Avalonia.Media.Brushes.Transparent,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
            IsHitTestVisible = true
         };
         overlay.PointerPressed += (_, e) =>
         {
            videoPanel.SetActiveSlot(capturedSlotIndex);
         };

         DragDrop.SetAllowDrop(overlay, true);
         overlay.AddHandler(DragDrop.DragOverEvent, (object s, DragEventArgs e) =>
         {
            e.DragEffects = DragDropEffects.Copy;
         });
         overlay.AddHandler(DragDrop.DropEvent, (object s, DragEventArgs e) =>
         {
            videoPanel.SetActiveSlot(capturedSlotIndex);
            videoPanel.HandleSlotDrop(capturedSlotIndex, e);
         });
         overlay.SizeChanged += (_, _) =>
         {
            if (mVideoDisplayMode == VideoDisplayMode.StretchToFill)
            {
               MediaPlayer mediaPlayer = mSlotMediaPlayers[capturedSlotIndex];
               ApplyVideoDisplayMode(capturedSlotIndex, mediaPlayer);
            }
         };

         var statsText = new TextBlock
         {
            FontSize = 11,
            Foreground = Avalonia.Media.Brushes.White,
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            Text = ""
         };
         var statsBorder = new Avalonia.Controls.Border
         {
            Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromArgb(176, 0, 0, 0)),
            CornerRadius = new Avalonia.CornerRadius(4),
            Padding = new Avalonia.Thickness(6, 4),
            Margin = new Avalonia.Thickness(8),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,
            IsHitTestVisible = false,
            IsVisible = mIsStatsForNerdsVisible,
            Child = statsText
         };

         var reconnectText = new TextBlock
         {
            FontSize = 14,
            FontWeight = Avalonia.Media.FontWeight.SemiBold,
            Foreground = Avalonia.Media.Brushes.White,
            TextAlignment = Avalonia.Media.TextAlignment.Center,
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            Text = string.Empty
         };
         var reconnectBorder = new Avalonia.Controls.Border
         {
            Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromArgb(216, 0, 0, 0)),
            CornerRadius = new Avalonia.CornerRadius(8),
            Padding = new Avalonia.Thickness(14, 10),
            MaxWidth = 260,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            IsHitTestVisible = false,
            IsVisible = false,
            Child = reconnectText
         };

         mSlotStatsTextBlocks[slotIndex] = statsText;
         mSlotReconnectTextBlocks[slotIndex] = reconnectText;
         mSlotReconnectStates[slotIndex].OverlayBorder = reconnectBorder;

         var contentGrid = new Avalonia.Controls.Grid
         {
            ClipToBounds = true
         };
         contentGrid.Children.Add(videoView);
         contentGrid.Children.Add(overlay);
         contentGrid.Children.Add(reconnectBorder);
         contentGrid.Children.Add(statsBorder);

         mSlotVideoContainers[slotIndex] = contentGrid;
         return contentGrid;
      }

      private void WireVideoPanel()
      {
         videoPanel.SlotRtspStreamRequested += (_, e) =>
         {
            if (string.IsNullOrEmpty(e.Url))
               StopSlotStream(e.SlotIndex);
            else
               StartSlotStream(e.SlotIndex, e.Url);
         };
         videoPanel.SubChannelChanged += (_, isSubChannel) => SubChannelChanged?.Invoke(this, isSubChannel);
         videoPanel.CameraSelectedFromMenu += (_, cameraNo) => CameraSelectedFromMenu?.Invoke(this, cameraNo);
         videoPanel.ActiveSlotChanged += (_, slotIndex) => ActiveSlotChanged?.Invoke(this, slotIndex);
         videoPanel.StatsForNerdsVisibilityChanged += (_, isVisible) =>
         {
            mIsStatsForNerdsVisible = isVisible;
            ApplyStatsOverlayVisibility();
            if (isVisible)
               UpdateAllStatsOverlays();
         };
         videoPanel.VideoDisplayModeChanged += (_, displayMode) =>
         {
            mVideoDisplayMode = displayMode;
            ApplyVideoDisplayModeToAllPlayers();
            VideoDisplayModeChanged?.Invoke(this, displayMode);
         };
         videoPanel.LayoutChanging += OnVideoLayoutChanging;
         videoPanel.LayoutChanged += OnVideoLayoutChanged;
      }

      private void OnVideoLayoutChanging(object sender, LayoutChangeEventArgs e)
      {
         // Detach surviving slot VideoViews from old panels (without disposing)
         foreach (int idx in e.SurvivingSlotIndices)
         {
            var slotContainer = mSlotVideoContainers[idx];
            if (slotContainer == null) continue;
            var parent = slotContainer.Parent as Panel;
            parent?.Children.Remove(slotContainer);
         }
      }

      private void OnVideoLayoutChanged(object sender, LayoutChangeEventArgs e)
      {
         // Stop streams for removed slots
         foreach (int idx in e.RemovedSlotIndices)
         {
            ResetSlotReconnectState(idx, true, true);
            StopSlotStream(idx);
         }

         // Reattach surviving slot VideoViews to their new panels
         foreach (int idx in e.SurvivingSlotIndices)
         {
            var slotContainer = mSlotVideoContainers[idx];
            if (slotContainer == null) continue;
            var pnlVideo = videoPanel.GetVideoSlot(idx);
            if (pnlVideo != null && !pnlVideo.Children.Contains(slotContainer))
               pnlVideo.Children.Add(slotContainer);
         }

         // Reconnect slots that have a remembered camera but no active stream
         int slotCount = e.NewLayout?.SlotCount ?? 0;
         for (int i = 0; i < slotCount; i++)
         {
            int idx = e.NewLayout.Slots[i].Index;
            if (e.SurvivingSlotIndices.Contains(idx)) continue;
            var camera = videoPanel.GetSlotCamera(idx);
            if (camera != null && !mSlotIsStarted[idx])
            {
               string url = camera.GetCameraLiveStreamUrl(1);
               if (!string.IsNullOrEmpty(url))
               {
                  videoPanel.RestoreSlotStream(idx, camera, 1);
               }
            }
         }

         LayoutChanged?.Invoke(this, e);
      }

      private void ApplyStatsOverlayVisibility()
      {
         for (int i = 0; i < C_MAX_SLOT_COUNT; i++)
         {
            TextBlock statsText = mSlotStatsTextBlocks[i];
            if (statsText?.Parent is Avalonia.Controls.Border statsBorder)
            {
               statsBorder.IsVisible = mIsStatsForNerdsVisible;
            }
         }
      }

      private void ClearSlotStatsOverlay(int slotIndex)
      {
         TextBlock statsText = mSlotStatsTextBlocks[slotIndex];
         if (statsText != null)
            statsText.Text = string.Empty;
      }

      private void UpdateAllStatsOverlays()
      {
         for (int i = 0; i < C_MAX_SLOT_COUNT; i++)
         {
            EvaluateSlotRuntime(i);
            UpdateReconnectOverlay(i);
            if (mIsStatsForNerdsVisible)
               UpdateSlotStatsOverlay(i);
            else
               ClearSlotStatsOverlay(i);
         }
      }

      private void EvaluateSlotRuntime(int slotIndex)
      {
         if (!mSlotIsStarted[slotIndex] || !IsSlotLiveStream(slotIndex))
         {
            mSlotStableSinceUtc[slotIndex] = DateTime.MinValue;
            if (!mSlotIsStarted[slotIndex])
               ResetSlotFrameTracking(slotIndex);
            return;
         }

         MediaPlayer mediaPlayer = mSlotMediaPlayers[slotIndex];
         Media media = mediaPlayer?.Media;
         MediaStats? stats = media?.Statistics;
         if (!stats.HasValue)
            return;

         DateTime now = DateTime.UtcNow;
         long displayedPictures = stats.Value.DisplayedPictures;
         if (displayedPictures > mSlotLastDisplayedPictures[slotIndex])
         {
            mSlotLastDisplayedPictures[slotIndex] = displayedPictures;
            mSlotLastFrameProgressUtc[slotIndex] = now;

            if (mSlotStableSinceUtc[slotIndex] == DateTime.MinValue)
               mSlotStableSinceUtc[slotIndex] = now;

            SlotReconnectState reconnectState = mSlotReconnectStates[slotIndex];
            if (reconnectState.IsActive)
               reconnectState.IsActive = false;

            int stableResetWindow = Math.Max(1, ConfigurationManager.Instance.VideoReconnectStableResetSeconds);
            if (reconnectState.AttemptCount > 0 &&
                (now - mSlotStableSinceUtc[slotIndex]).TotalSeconds >= stableResetWindow)
            {
               reconnectState.AttemptCount = 0;
               OnVlcLog($"Slot {slotIndex}: stable stream for {stableResetWindow}s, reset reconnect counters.", LogLevel.Notice);
            }
            return;
         }

         mSlotStableSinceUtc[slotIndex] = DateTime.MinValue;

         int stallThreshold = Math.Max(1, ConfigurationManager.Instance.VideoStallThresholdSeconds);
         DateTime stallSince = mSlotLastFrameProgressUtc[slotIndex] > DateTime.MinValue
            ? mSlotLastFrameProgressUtc[slotIndex]
            : mSlotStartedUtc[slotIndex];

         if (stallSince == DateTime.MinValue)
            stallSince = now;

         if ((now - stallSince).TotalSeconds >= stallThreshold)
            EnsureReconnectActive(slotIndex, "No new video frames detected.");

         ProcessReconnectTimer(slotIndex, now);
      }

      private void EnsureReconnectActive(int slotIndex, string reason)
      {
         if (!ConfigurationManager.Instance.VideoReconnectEnabled)
            return;
         if (!mSlotIsStarted[slotIndex] || !IsSlotLiveStream(slotIndex))
            return;

         SlotReconnectState reconnectState = mSlotReconnectStates[slotIndex];
         if (reconnectState.IsActive)
            return;

         reconnectState.CancellationTokenSource?.Cancel();
         reconnectState.CancellationTokenSource?.Dispose();
         reconnectState.CancellationTokenSource = new CancellationTokenSource();
         reconnectState.IsActive = true;
         reconnectState.IsAttemptRunning = false;
         reconnectState.Reason = reason ?? string.Empty;
         reconnectState.NextRetryUtc = DateTime.UtcNow.AddSeconds(GetRetryIntervalSeconds(slotIndex, reconnectState.AttemptCount + 1));
      }

      private void ProcessReconnectTimer(int slotIndex, DateTime now)
      {
         SlotReconnectState reconnectState = mSlotReconnectStates[slotIndex];
         if (!reconnectState.IsActive || reconnectState.IsAttemptRunning)
            return;

         if (now < reconnectState.NextRetryUtc)
            return;

         CancellationToken cancellationToken = reconnectState.CancellationTokenSource?.Token ?? CancellationToken.None;
         _ = ExecuteReconnectAttemptAsync(slotIndex, cancellationToken);
      }

      private async Task ExecuteReconnectAttemptAsync(int slotIndex, CancellationToken cancellationToken)
      {
         SlotReconnectState reconnectState = mSlotReconnectStates[slotIndex];
         if (!reconnectState.IsActive || reconnectState.IsAttemptRunning)
            return;

         string url = mSlotRtspUrls[slotIndex];
         if (string.IsNullOrWhiteSpace(url) || !mSlotIsStarted[slotIndex])
         {
            reconnectState.IsActive = false;
            return;
         }

         reconnectState.IsAttemptRunning = true;
         reconnectState.AttemptCount++;
         int retryInterval = GetRetryIntervalSeconds(slotIndex, reconnectState.AttemptCount + 1);
         bool isSteadyMode = reconnectState.AttemptCount > Math.Max(1, ConfigurationManager.Instance.VideoReconnectQuickAttempts);

         if (!isSteadyMode || (DateTime.UtcNow - reconnectState.LastSteadyLogUtc).TotalSeconds >= 30)
         {
            OnVlcLog($"Slot {slotIndex}: reconnect attempt {reconnectState.AttemptCount}, next retry interval {retryInterval}s.", LogLevel.Warning);
            if (isSteadyMode)
               reconnectState.LastSteadyLogUtc = DateTime.UtcNow;
         }

         try
         {
            await Dispatcher.UIThread.InvokeAsync(() => StopSlotStreamInternal(slotIndex, false, false));

            int restartDelayMs = Math.Max(0, ConfigurationManager.Instance.VideoReconnectRestartDelayMs);
            if (restartDelayMs > 0)
               await Task.Delay(restartDelayMs, cancellationToken);

            if (cancellationToken.IsCancellationRequested)
               return;

            await Dispatcher.UIThread.InvokeAsync(() => StartSlotStreamInternal(slotIndex, url, true));
         }
         catch (OperationCanceledException)
         {
         }
         finally
         {
            reconnectState.IsAttemptRunning = false;
            if (reconnectState.IsActive)
               reconnectState.NextRetryUtc = DateTime.UtcNow.AddSeconds(GetRetryIntervalSeconds(slotIndex, reconnectState.AttemptCount + 1));
         }
      }

      private void UpdateReconnectOverlay(int slotIndex)
      {
         SlotReconnectState reconnectState = mSlotReconnectStates[slotIndex];
         TextBlock reconnectText = mSlotReconnectTextBlocks[slotIndex];
         Avalonia.Controls.Border reconnectBorder = reconnectState.OverlayBorder;
         if (reconnectText == null || reconnectBorder == null)
            return;

         if (!reconnectState.IsActive)
         {
            reconnectBorder.IsVisible = false;
            reconnectText.Text = string.Empty;
            return;
         }

         DateTime now = DateTime.UtcNow;
         int secondsRemaining = (int)Math.Ceiling((reconnectState.NextRetryUtc - now).TotalSeconds);
         if (secondsRemaining < 0)
            secondsRemaining = 0;

         int nextAttemptNumber = reconnectState.IsAttemptRunning
            ? reconnectState.AttemptCount
            : reconnectState.AttemptCount + 1;

         reconnectText.Text = reconnectState.IsAttemptRunning
            ? $"Reconnecting camera...{Environment.NewLine}Try {reconnectState.AttemptCount}{Environment.NewLine}Restarting now"
            : $"Reconnecting camera...{Environment.NewLine}Try {nextAttemptNumber}{Environment.NewLine}Next attempt in {secondsRemaining}s";
         reconnectBorder.IsVisible = true;
      }

      private int GetRetryIntervalSeconds(int slotIndex, int attemptNumber)
      {
         int quickAttempts = Math.Max(1, ConfigurationManager.Instance.VideoReconnectQuickAttempts);
         int quickInterval = Math.Max(1, ConfigurationManager.Instance.VideoReconnectQuickIntervalSeconds);
         int steadyInterval = Math.Max(1, ConfigurationManager.Instance.VideoReconnectSteadyIntervalSeconds);
         if (attemptNumber <= quickAttempts)
            return quickInterval;
         return steadyInterval;
      }

      private void ResetSlotFrameTracking(int slotIndex)
      {
         DateTime now = DateTime.UtcNow;
         mSlotLastDisplayedPictures[slotIndex] = -1;
         mSlotLastFrameProgressUtc[slotIndex] = now;
         mSlotStableSinceUtc[slotIndex] = DateTime.MinValue;
         if (mSlotStartedUtc[slotIndex] == DateTime.MinValue)
            mSlotStartedUtc[slotIndex] = now;
      }

      private void ResetSlotReconnectState(int slotIndex, bool resetCounters, bool hideOverlay)
      {
         SlotReconnectState reconnectState = mSlotReconnectStates[slotIndex];
         reconnectState.IsActive = false;
         reconnectState.IsAttemptRunning = false;
         reconnectState.Reason = string.Empty;
         reconnectState.NextRetryUtc = DateTime.MinValue;
         reconnectState.CancellationTokenSource?.Cancel();
         reconnectState.CancellationTokenSource?.Dispose();
         reconnectState.CancellationTokenSource = null;

         if (resetCounters)
         {
            reconnectState.AttemptCount = 0;
            reconnectState.LastSteadyLogUtc = DateTime.MinValue;
         }

         if (hideOverlay)
         {
            TextBlock reconnectText = mSlotReconnectTextBlocks[slotIndex];
            Avalonia.Controls.Border reconnectBorder = reconnectState.OverlayBorder;
            if (reconnectText != null)
               reconnectText.Text = string.Empty;
            if (reconnectBorder != null)
               reconnectBorder.IsVisible = false;
         }
      }

      private bool IsSlotLiveStream(int slotIndex)
      {
         string rtspUrl = mSlotRtspUrls[slotIndex];
         if (string.IsNullOrWhiteSpace(rtspUrl))
            return false;
         return !IsPlaybackUrl(rtspUrl);
      }

      private static bool IsPlaybackUrl(string rtspUrl)
      {
         return rtspUrl != null && rtspUrl.IndexOf("playback", StringComparison.OrdinalIgnoreCase) >= 0;
      }

      private void UpdateSlotStatsOverlay(int slotIndex)
      {
         TextBlock statsText = mSlotStatsTextBlocks[slotIndex];
         MediaPlayer mediaPlayer = mSlotMediaPlayers[slotIndex];

         if (statsText == null || mediaPlayer?.Media == null || !mSlotIsStarted[slotIndex])
         {
            ClearSlotStatsOverlay(slotIndex);
            return;
         }

         Media media = mediaPlayer.Media;
         MediaStats? stats = media.Statistics;
         MediaTrack[] tracks = media.Tracks;

         string videoCodec = "unknown";
         string resolution = "-";
         string mediaFps = "-";
         ulong videoBitrate = 0;

         if (tracks != null)
         {
            foreach (MediaTrack track in tracks)
            {
               if (track.TrackType != TrackType.Video)
                  continue;

               string codecDescription = media.CodecDescription(track.TrackType, track.Codec);
               videoCodec = string.IsNullOrWhiteSpace(codecDescription) ? $"0x{track.Codec:X}" : codecDescription;
               videoBitrate = track.Bitrate;

               VideoTrack videoData = track.Data.Video;
               resolution = $"{videoData.Width}x{videoData.Height}";
               if (videoData.FrameRateDen > 0)
               {
                  double fpsFromTrack = (double)videoData.FrameRateNum / videoData.FrameRateDen;
                  mediaFps = fpsFromTrack.ToString("0.00");
               }
               break;
            }
         }

         string playerFps = mediaPlayer.Fps.ToString("0.00");

         StringBuilder builder = new StringBuilder();
         builder.AppendLine($"Slot: {slotIndex + 1}");
         builder.AppendLine($"Codec: {videoCodec}");
         builder.AppendLine($"Resolution: {resolution}");
         builder.AppendLine($"FPS: {playerFps} (track {mediaFps})");
         builder.AppendLine($"Track bitrate: {FormatBitsPerSecond(videoBitrate)}");

         if (stats.HasValue)
         {
            MediaStats statValue = stats.Value;
            builder.AppendLine($"Input bitrate: {statValue.InputBitrate:0.00} kb/s");
            builder.AppendLine($"Demux bitrate: {statValue.DemuxBitrate:0.00} kb/s");
            builder.AppendLine($"Read bytes: {statValue.ReadBytes:N0}");
            builder.AppendLine($"Decoded video: {statValue.DecodedVideo:N0}");
            builder.AppendLine($"Displayed frames: {statValue.DisplayedPictures:N0}");
            builder.AppendLine($"Lost frames: {statValue.LostPictures:N0}");
         }

         statsText.Text = builder.ToString().TrimEnd();
      }

      private static string FormatBitsPerSecond(ulong bitsPerSecond)
      {
         if (bitsPerSecond == 0) return "0 bps";
         if (bitsPerSecond >= 1_000_000) return $"{bitsPerSecond / 1_000_000d:0.00} Mbps";
         if (bitsPerSecond >= 1_000) return $"{bitsPerSecond / 1_000d:0.00} kbps";
         return $"{bitsPerSecond} bps";
      }

      private void ApplyVideoDisplayMode(int slotIndex, MediaPlayer mediaPlayer)
      {
         if (mediaPlayer == null) return;

         mediaPlayer.AspectRatio = null;
         mediaPlayer.CropGeometry = string.Empty;
         ApplyVideoViewLayout(slotIndex);

         switch (mVideoDisplayMode)
         {
            case VideoDisplayMode.StretchToFill:
               mediaPlayer.Scale = 0.0f;
               mediaPlayer.AspectRatio = BuildStretchAspectRatio(slotIndex);
               break;
            default:
               mediaPlayer.Scale = 0.0f;
               break;
         }
      }

      private void ApplyVideoDisplayModeToAllPlayers()
      {
         for (int i = 0; i < C_MAX_SLOT_COUNT; i++)
            ApplyVideoDisplayMode(i, mSlotMediaPlayers[i]);
      }

      private void ApplyVideoViewLayout(int slotIndex)
      {
         VideoView videoView = mSlotVideoControls[slotIndex];
         if (videoView == null) return;

         videoView.Width = double.NaN;
         videoView.Height = double.NaN;
         videoView.Margin = new Avalonia.Thickness(0);
         videoView.HorizontalAlignment = HorizontalAlignment.Stretch;
         videoView.VerticalAlignment = VerticalAlignment.Stretch;
      }

      private string BuildStretchAspectRatio(int slotIndex)
      {
         Panel slotPanel = videoPanel.GetVideoSlot(slotIndex);
         if (slotPanel == null) return "16:9";

         double width = slotPanel.Bounds.Width;
         double height = slotPanel.Bounds.Height;
         if (width <= 1 || height <= 1) return "16:9";

         int aspectWidth = Math.Max(1, (int)Math.Round(width));
         int aspectHeight = Math.Max(1, (int)Math.Round(height));
         return $"{aspectWidth}:{aspectHeight}";
      }

      private static void DisposeMediaPlayerAsync(MediaPlayer mp)
      {
         if (mp == null) return;
         Task.Run(() => { if (mp.IsPlaying) mp.Stop(); mp.Dispose(); });
      }

      private sealed class SlotReconnectState
      {
         public int AttemptCount;
         public bool IsActive;
         public bool IsAttemptRunning;
         public DateTime LastSteadyLogUtc;
         public DateTime NextRetryUtc;
         public string Reason = string.Empty;
         public Avalonia.Controls.Border OverlayBorder;
         public CancellationTokenSource CancellationTokenSource;
      }
   }

   // ── Supporting types ──────────────────────────────────────────────────────

   public class VlcLogEventArgs : EventArgs
   {
      public string Message { get; }
      public LogLevel Level { get; }

      public VlcLogEventArgs(string message, LogLevel level)
      {
         Message = message;
         Level = level;
      }
   }

}
