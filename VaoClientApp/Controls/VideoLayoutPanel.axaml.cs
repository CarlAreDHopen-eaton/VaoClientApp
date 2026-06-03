using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
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
      private readonly VideoView[] mSlotVideoControls = new VideoView[C_MAX_SLOT_COUNT];
      private readonly string[] mSlotRtspUrls = new string[C_MAX_SLOT_COUNT];
      private readonly bool[] mSlotIsStarted = new bool[C_MAX_SLOT_COUNT];
      private readonly TextBlock[] mSlotStatsTextBlocks = new TextBlock[C_MAX_SLOT_COUNT];
      private readonly DispatcherTimer mStatsTimer;
      private bool mIsStatsForNerdsVisible;

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

      public VideoLayoutPanel()
      {
         InitializeComponent();

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

      public void SetStatsForNerdsVisible(bool visible)
      {
         videoPanel.SetStatsForNerdsVisible(visible);
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
         mSlotRtspUrls[slotIndex] = rtspUrl;
         if (mSlotIsStarted[slotIndex]) StopSlotStream(slotIndex);

         var pnlVideo = videoPanel.GetVideoSlot(slotIndex);
         if (pnlVideo == null) return;

         var videoView = CreateSlotVideoView(slotIndex);
         mSlotVideoControls[slotIndex] = videoView;

         if (!pnlVideo.Children.Contains(videoView))
            pnlVideo.Children.Add(videoView);

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
         mp.Playing += (_, _) => Dispatcher.UIThread.Post(() => UpdateSlotStatsOverlay(slotIndex));
         mp.Stopped += (_, _) => Dispatcher.UIThread.Post(() => ClearSlotStatsOverlay(slotIndex));

         mSlotMediaPlayers[slotIndex] = mp;
         videoView.MediaPlayer = mp;
         mp.Play();
         mSlotIsStarted[slotIndex] = true;
      }

      public void StopSlotStream(int slotIndex)
      {
         var mp = mSlotMediaPlayers[slotIndex];
         if (mp == null) return;

         mSlotMediaPlayers[slotIndex] = null;
         var pnlVideo = videoPanel.GetVideoSlot(slotIndex);
         var videoView = mSlotVideoControls[slotIndex];
         if (videoView != null)
         {
            videoView.MediaPlayer = null;
            if (pnlVideo != null && pnlVideo.Children.Contains(videoView))
               pnlVideo.Children.Remove(videoView);
            mSlotVideoControls[slotIndex] = null;
         }
         mSlotIsStarted[slotIndex] = false;
         mSlotRtspUrls[slotIndex] = null;
         ClearSlotStatsOverlay(slotIndex);
         DisposeMediaPlayerAsync(mp);
      }

      /// <summary>Detach all video surfaces (call before showing overlays).</summary>
      public void DetachVideoSurfaces()
      {
         for (int i = 0; i < C_MAX_SLOT_COUNT; i++)
         {
            var slotView = mSlotVideoControls[i];
            if (slotView == null) continue;
            if (mSlotMediaPlayers[i] != null) slotView.MediaPlayer = null;
            var parent = slotView.Parent as Panel;
            parent?.Children.Remove(slotView);
         }
      }

      /// <summary>Reattach all video surfaces (call after hiding overlays).</summary>
      public void ReattachVideoSurfaces()
      {
         for (int i = 0; i < C_MAX_SLOT_COUNT; i++)
         {
            var slotView = mSlotVideoControls[i];
            if (slotView == null || !mSlotIsStarted[i]) continue;
            var slotPanel = videoPanel.GetVideoSlot(i);
            if (slotPanel == null) continue;

            var newView = CreateSlotVideoView(i);
            mSlotVideoControls[i] = newView;
            slotPanel.Children.Add(newView);
            newView.MediaPlayer = mSlotMediaPlayers[i];

            if (!string.IsNullOrWhiteSpace(mSlotRtspUrls[i]))
            {
               int idx = i;
               Dispatcher.UIThread.Post(() =>
               {
                  if (mSlotIsStarted[idx]) StartSlotStream(idx, mSlotRtspUrls[idx]);
               }, DispatcherPriority.Background);
            }
         }
      }

      public void Dispose()
      {
         mStatsTimer.Stop();

         for (int i = 0; i < C_MAX_SLOT_COUNT; i++)
            StopSlotStream(i);

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

      private VideoView CreateSlotVideoView(int slotIndex)
      {
         var videoView = new VideoView { Focusable = false };
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

         mSlotStatsTextBlocks[slotIndex] = statsText;

         var contentGrid = new Avalonia.Controls.Grid();
         contentGrid.Children.Add(overlay);
         contentGrid.Children.Add(statsBorder);

         videoView.Content = contentGrid;
         return videoView;
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
         videoPanel.LayoutChanging += OnVideoLayoutChanging;
         videoPanel.LayoutChanged += OnVideoLayoutChanged;
      }

      private void OnVideoLayoutChanging(object sender, LayoutChangeEventArgs e)
      {
         // Detach surviving slot VideoViews from old panels (without disposing)
         foreach (int idx in e.SurvivingSlotIndices)
         {
            var videoView = mSlotVideoControls[idx];
            if (videoView == null) continue;
            var parent = videoView.Parent as Panel;
            parent?.Children.Remove(videoView);
         }
      }

      private void OnVideoLayoutChanged(object sender, LayoutChangeEventArgs e)
      {
         // Stop streams for removed slots
         foreach (int idx in e.RemovedSlotIndices)
            StopSlotStream(idx);

         // Reattach surviving slot VideoViews to their new panels
         foreach (int idx in e.SurvivingSlotIndices)
         {
            var videoView = mSlotVideoControls[idx];
            if (videoView == null) continue;
            var pnlVideo = videoPanel.GetVideoSlot(idx);
            if (pnlVideo != null && !pnlVideo.Children.Contains(videoView))
               pnlVideo.Children.Add(videoView);
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
            VideoView slotView = mSlotVideoControls[i];
            if (slotView?.Content is Avalonia.Controls.Grid grid && grid.Children.Count > 1 && grid.Children[1] is Avalonia.Controls.Border statsBorder)
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
         if (!mIsStatsForNerdsVisible) return;

         for (int i = 0; i < C_MAX_SLOT_COUNT; i++)
            UpdateSlotStatsOverlay(i);
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

      private static void DisposeMediaPlayerAsync(MediaPlayer mp)
      {
         if (mp == null) return;
         Task.Run(() => { if (mp.IsPlaying) mp.Stop(); mp.Dispose(); });
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
