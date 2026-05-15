using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using LibVLCSharp.Avalonia;
using LibVLCSharp.Shared;
using Vao.Client.Components;

namespace Vao.Sample.Controls
{
   public partial class VideoLayoutPanel : UserControl
   {
      private const int C_MAX_SLOT_COUNT = 4;

      private LibVLC mLibVlc;

      // ── Single mode VLC state ──────────────────────────────────────────────
      private MediaPlayer mMediaPlayer;
      private VideoView mVideoControl;
      private string mActiveRtspUrl;
      private bool mIsVideoStarted;
      private bool mIsVideoTemporarilyDetached;

      // ── Multi-slot VLC state ───────────────────────────────────────────────
      private readonly MediaPlayer[] mSlotMediaPlayers = new MediaPlayer[C_MAX_SLOT_COUNT];
      private readonly VideoView[] mSlotVideoControls = new VideoView[C_MAX_SLOT_COUNT];
      private readonly string[] mSlotRtspUrls = new string[C_MAX_SLOT_COUNT];
      private readonly bool[] mSlotIsStarted = new bool[C_MAX_SLOT_COUNT];

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
      }

      // ── Public API ─────────────────────────────────────────────────────────

      public VideoPanel InnerVideoPanel => videoPanel;

      public Panel VideoSlot => videoPanel.VideoSlot;

      public Panel GetVideoSlot(int slotIndex) => videoPanel.GetVideoSlot(slotIndex);

      public IReadOnlyList<Panel> GetAllActiveVideoSlots() => videoPanel.GetAllActiveVideoSlots();

      public string CurrentLayoutKey => videoPanel.CurrentLayoutKey;

      public VideoLayoutDefinition CurrentLayout => videoPanel.CurrentLayout;

      public bool IsSingleView => videoPanel.IsSingleView;

      public int ActiveSlotIndex => videoPanel.ActiveSlotIndex;

      public bool IsPlayback => videoPanel.IsPlayback;

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

      public void StartStream(string rtspUrl)
      {
         mActiveRtspUrl = rtspUrl;
         if (mIsVideoStarted) StopStream();

         InitVideoControl();
         var uri = new Uri(rtspUrl);
         if (mMediaPlayer == null)
         {
            var media = new Media(mLibVlc, uri);
            mMediaPlayer = new MediaPlayer(media);
            mMediaPlayer.EnableKeyInput = false;
            mMediaPlayer.EnableMouseInput = false;
            mMediaPlayer.EncounteredError += MediaPlayer_EncounteredError;
            mMediaPlayer.Opening += MediaPlayer_Opening;
            if (ConfigurationManager.Instance.UseTcp) media.AddOption(":rtsp-tcp");
            if (mVideoControl != null) mVideoControl.MediaPlayer = mMediaPlayer;
            mMediaPlayer.Play();
            mIsVideoStarted = true;
         }
         else
         {
            var media = new Media(mLibVlc, uri);
            if (ConfigurationManager.Instance.UseTcp) media.AddOption(":rtsp-tcp");
            mMediaPlayer.Play(media);
            mIsVideoStarted = true;
         }
      }

      public void StopStream()
      {
         if (mMediaPlayer != null)
         {
            var mp = mMediaPlayer;
            mMediaPlayer = null;
            var pnlVideo = videoPanel.VideoSlot;
            if (mVideoControl != null)
            {
               mVideoControl.MediaPlayer = null;
               if (pnlVideo != null && pnlVideo.Children.Contains(mVideoControl))
                  pnlVideo.Children.Remove(mVideoControl);
               mVideoControl = null;
            }
            mIsVideoStarted = false;
            mIsVideoTemporarilyDetached = false;
            DisposeMediaPlayerAsync(mp);
         }
      }

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
         DisposeMediaPlayerAsync(mp);
      }

      /// <summary>Detach all video surfaces (call before showing overlays).</summary>
      public void DetachVideoSurfaces()
      {
         // Detach single-view
         var pnlVideo = videoPanel.VideoSlot;
         if (pnlVideo != null && mVideoControl != null && pnlVideo.Children.Contains(mVideoControl))
         {
            if (mMediaPlayer != null) mVideoControl.MediaPlayer = null;
            pnlVideo.Children.Remove(mVideoControl);
            mIsVideoTemporarilyDetached = true;
         }

         // Detach slot views
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
         // Reattach single-view
         var pnlVideo = videoPanel.VideoSlot;
         if (pnlVideo != null)
         {
            bool shouldAttach = mIsVideoTemporarilyDetached ||
               (mIsVideoStarted && mMediaPlayer != null &&
                (mVideoControl == null || !pnlVideo.Children.Contains(mVideoControl)));
            if (shouldAttach)
            {
               if (mVideoControl != null && pnlVideo.Children.Contains(mVideoControl))
                  pnlVideo.Children.Remove(mVideoControl);

               mVideoControl = new VideoView();
               mVideoControl.Focusable = false;
               pnlVideo.Children.Add(mVideoControl);

               if (mMediaPlayer != null)
               {
                  mVideoControl.MediaPlayer = null;
                  mVideoControl.MediaPlayer = mMediaPlayer;

                  if (mIsVideoStarted && !string.IsNullOrWhiteSpace(mActiveRtspUrl))
                  {
                     Dispatcher.UIThread.Post(() =>
                     {
                        if (mIsVideoStarted) StartStream(mActiveRtspUrl);
                     }, DispatcherPriority.Background);
                  }
               }

               mIsVideoTemporarilyDetached = false;
            }
         }

         // Reattach slot views
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
         StopStream();
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

      private void InitVideoControl()
      {
         var pnlVideo = videoPanel.VideoSlot;
         if (mVideoControl == null)
         {
            mVideoControl = new VideoView();
            mVideoControl.Focusable = false;
         }
         if (!pnlVideo.Children.Contains(mVideoControl))
            pnlVideo.Children.Add(mVideoControl);
         mIsVideoTemporarilyDetached = false;
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

         videoView.Content = overlay;
         return videoView;
      }

      private void WireVideoPanel()
      {
         videoPanel.RtspStreamRequested += (_, url) =>
         {
            if (string.IsNullOrEmpty(url))
               StopStream();
            else
               StartStream(url);
         };
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

         LayoutChanged?.Invoke(this, e);
      }

      private void MediaPlayer_EncounteredError(object sender, EventArgs e)
         => OnVlcLog("LibVLC error encountered.", LogLevel.Error);

      private void MediaPlayer_Opening(object sender, EventArgs e)
         => OnVlcLog($"LibVLC opening {mMediaPlayer?.Media?.Mrl ?? ""}", LogLevel.Notice);

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
