using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Views;
using Android.Widget;
using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;
using LibVLCSharp.Shared;
using Vao.Client.Enum;
using Vao.Sample.Controls;

namespace Vao.Sample;

/// <summary>
/// Owns the LibVLC / MediaPlayer lifecycle for the Android multi-slot target.
/// Uses one LibVLCSharp.Platforms.Android.VideoView (a native SurfaceView overlay)
/// per active video slot, each added to the Activity's content frame and
/// positioned over its corresponding Avalonia slot panel.
///
/// Design: A single LibVLC instance is shared. Per-slot MediaPlayer + VideoView
/// pairs are created eagerly when a layout is activated, so the Android surface
/// lifecycle (surfaceCreated → SetAndroidContext) completes long before the
/// first playback request arrives — avoiding the native SIGSEGV that occurs
/// when Play() is called before VLC has received a valid surface.
/// </summary>
internal sealed class AndroidVideoController
{
   private readonly MainView mMainView;
   private readonly Activity mActivity;
   private LibVLC mLibVlc;

   // Per-slot state keyed by slot index
   private readonly Dictionary<int, SlotVideoState> mSlotStates = new();

   // Panels whose LayoutUpdated event we've subscribed to, keyed by slot index
   private readonly Dictionary<int, Avalonia.Controls.Panel> mTrackedPanels = new();

   // Stream requests that arrived before the slot was initialized; replayed once ready
   private readonly Dictionary<int, string> mPendingStreams = new();

   private sealed class SlotVideoState
   {
      public MediaPlayer MediaPlayer;
      public LibVLCSharp.Platforms.Android.VideoView VideoView;
      public string ActiveRtspUrl;
      public bool IsVideoStarted;
      public bool IsVideoTemporarilyDetached;
   }

   public AndroidVideoController(MainView mainView, Activity activity)
   {
      mMainView = mainView;
      mActivity = activity;

      mMainView.RtspStreamRequested     += OnRtspStreamRequested;
      mMainView.SlotRtspStreamRequested += OnSlotRtspStreamRequested;
      mMainView.AnyOverlayStateChanged  += OnAnyOverlayStateChanged;
      mMainView.VideoLayoutChanging     += OnVideoLayoutChanging;
      mMainView.VideoLayoutChanged      += OnVideoLayoutChanged;

      // VLC is initialised immediately (OnCreate is on the Android main thread)
      // so the surface can be created by the layout system well before the first
      // StartRtspStream call.
      InitializeVlc();

      // Create slot 0 for the default (single-view) layout.
      InitializeSlot(0);
      TrackSlotPanel(0);
   }

   // ── Initialisation ──────────────────────────────────────────────────────

   private void InitializeVlc()
   {
      Core.Initialize();
      mMainView.WriteMessageLog(MessageSource.LibVlc, "Loading VLC", LogLevel.Notice);
      // VLC 3.0.7 (Android x86_64) has a null-pointer bug in its vsprintf/log
      // formatting path: any log message can pass a null %s to vsprintf, causing
      // a SIGSEGV (strlen on 0x30).  "--verbose=-1" completely disables VLC's
      // internal logging so vsprintf is never reached.
      var options = new[] { "--verbose=-1", "--rtsp-timeout=300", "--network-caching=300" };
      mLibVlc = new LibVLC(enableDebugLogs: false, options);
   }

   /// <summary>
   /// Creates and adds a VideoView + MediaPlayer for the given slot index.
   /// Safe to call from any thread; the VideoView is created on the Android
   /// UI thread so its surface is established before the first Play() call.
   /// </summary>
   private void InitializeSlot(int slotIndex)
   {
      if (mSlotStates.ContainsKey(slotIndex)) return;

      mMainView.WriteMessageLog(MessageSource.LibVlc,
         $"InitializeSlot({slotIndex}) called — pending=[{string.Join(",", mPendingStreams.Keys)}]",
         LogLevel.Debug);

      var state = new SlotVideoState();
      state.MediaPlayer               = new MediaPlayer(mLibVlc);
      state.MediaPlayer.EnableKeyInput  = false;
      state.MediaPlayer.EnableMouseInput = false;

      // Capture slotIndex in closures.
      int capturedIndex = slotIndex;
      state.MediaPlayer.EncounteredError += (s, e) =>
      {
         mMainView.WriteMessageLog(MessageSource.LibVlc,
            $"LibVLC error on slot {capturedIndex} ({state.ActiveRtspUrl ?? "no url"}) — " +
            $"if other slots play fine this slot may have exceeded the hardware decoder limit.",
            LogLevel.Error);
         ScheduleReconnect(capturedIndex, state);
      };
      state.MediaPlayer.EndReached += (s, e) =>
      {
         mMainView.WriteMessageLog(MessageSource.LibVlc,
            $"LibVLC stream ended on slot {capturedIndex} — reconnecting",
            LogLevel.Notice);
         ScheduleReconnect(capturedIndex, state);
      };
      state.MediaPlayer.Opening += (s, e) =>
         mMainView.WriteMessageLog(MessageSource.LibVlc,
            $"LibVLC opening {state.MediaPlayer?.Media?.Mrl ?? ""} (slot {capturedIndex})", LogLevel.Notice);

      mSlotStates[slotIndex] = state;

      mActivity.RunOnUiThread(() =>
      {
         var videoView = new LibVLCSharp.Platforms.Android.VideoView(mActivity);
         videoView.Visibility = ViewStates.Gone;

         var contentFrame = mActivity.FindViewById<FrameLayout>(Android.Resource.Id.Content);
         var lp = new FrameLayout.LayoutParams(
            FrameLayout.LayoutParams.MatchParent,
            FrameLayout.LayoutParams.MatchParent);
         contentFrame.AddView(videoView, lp);

         // Wire MediaPlayer now. LibVLCSharp calls SetAndroidContext when
         // surfaceCreated fires (first layout pass), well before Play().
         videoView.MediaPlayer = state.MediaPlayer;
         state.VideoView = videoView;

         // Replay any stream request that arrived before we were ready.
         if (mPendingStreams.TryGetValue(capturedIndex, out var pendingUrl))
         {
            mPendingStreams.Remove(capturedIndex);
            mMainView.WriteMessageLog(MessageSource.LibVlc,
               $"Replaying pending stream for slot {capturedIndex}: {pendingUrl}",
               LogLevel.Notice);
            StartRtspStream(capturedIndex, pendingUrl);
         }
         else
         {
            mMainView.WriteMessageLog(MessageSource.LibVlc,
               $"InitializeSlot({capturedIndex}) UI complete — no pending stream (pending=[{string.Join(",", mPendingStreams.Keys)}])",
               LogLevel.Debug);
         }
      });
   }

   /// <summary>
   /// Stops playback, removes the VideoView from the hierarchy and disposes
   /// the MediaPlayer for the given slot.
   /// </summary>
   private void DestroySlot(int slotIndex)
   {
      if (!mSlotStates.TryGetValue(slotIndex, out var state)) return;

      mPendingStreams.Remove(slotIndex);
      state.IsVideoStarted = false;
      state.IsVideoTemporarilyDetached = false;

      var mp = state.MediaPlayer;
      mActivity.RunOnUiThread(() =>
      {
         if (state.VideoView?.Parent is ViewGroup parent)
            parent.RemoveView(state.VideoView);
         state.VideoView = null;
      });

      Task.Run(() =>
      {
         mp?.Stop();
         mp?.Dispose();
      });

      mSlotStates.Remove(slotIndex);
   }

   // ── Slot-panel LayoutUpdated tracking ────────────────────────────────────

   private void TrackSlotPanel(int slotIndex)
   {
      var panel = mMainView.GetVideoSlot(slotIndex);
      if (panel == null) return;

      if (mTrackedPanels.TryGetValue(slotIndex, out var old))
      {
         if (old == panel) return;
         old.LayoutUpdated -= OnVideoSlotLayoutUpdated;
      }

      panel.LayoutUpdated += OnVideoSlotLayoutUpdated;
      mTrackedPanels[slotIndex] = panel;
   }

   private void UntrackSlotPanel(int slotIndex)
   {
      if (mTrackedPanels.TryGetValue(slotIndex, out var panel))
      {
         panel.LayoutUpdated -= OnVideoSlotLayoutUpdated;
         mTrackedPanels.Remove(slotIndex);
      }
   }

   // ── Event handlers ──────────────────────────────────────────────────────

   private void OnRtspStreamRequested(object sender, string url)
   {
      // Single-view path — always slot 0.
      if (string.IsNullOrEmpty(url))
         StopRtspStream(0);
      else
         StartRtspStream(0, url);
   }

   private void OnSlotRtspStreamRequested(object sender, VideoSlotStreamEventArgs e)
   {
      if (string.IsNullOrEmpty(e.Url))
         StopRtspStream(e.SlotIndex);
      else
         StartRtspStream(e.SlotIndex, e.Url);
   }

   private void OnAnyOverlayStateChanged(object sender, bool isOverlayVisible)
   {
      if (isOverlayVisible)
         DetachAllVideoSurfacesForOverlay();
      else
         ReattachAllVideoSurfacesAfterOverlay();
   }

   private void OnVideoLayoutChanging(object sender, LayoutChangeEventArgs e)
   {
      // Untrack + destroy slots that won't survive into the new layout.
      foreach (int idx in e.RemovedSlotIndices)
      {
         UntrackSlotPanel(idx);
         DestroySlot(idx);
      }

      // When transitioning from single-view → multi-view the slot-0 panel
      // reference will change (pnlVideo → quadGrid cell), so untrack it now;
      // OnVideoLayoutChanged will re-track the new panel.
      if (e.PreviousLayout != null && e.PreviousLayout.IsSingleView && !e.NewLayout.IsSingleView)
         UntrackSlotPanel(0);
   }

   private void OnVideoLayoutChanged(object sender, LayoutChangeEventArgs e)
   {
      mMainView.WriteMessageLog(MessageSource.LibVlc,
         $"OnVideoLayoutChanged: isSingleView={e.NewLayout.IsSingleView} slots=[{string.Join(",", e.NewLayout.Slots?.Select(s => s.Index) ?? Enumerable.Empty<int>())}] knownSlots=[{string.Join(",", mSlotStates.Keys)}] pending=[{string.Join(",", mPendingStreams.Keys)}]",
         LogLevel.Debug);

      if (e.NewLayout.IsSingleView)
      {
         if (!mSlotStates.ContainsKey(0))
            InitializeSlot(0);
         TrackSlotPanel(0);
      }
      else
      {
         foreach (var slotDef in e.NewLayout.Slots)
         {
            if (!mSlotStates.ContainsKey(slotDef.Index))
               InitializeSlot(slotDef.Index);
            TrackSlotPanel(slotDef.Index);
         }
      }
   }

   private void OnVideoSlotLayoutUpdated(object sender, EventArgs e)
   {
      // Reposition all active VideoViews on any slot-panel layout pass.
      mActivity.RunOnUiThread(() =>
      {
         foreach (var kvp in mSlotStates)
         {
            if (kvp.Value.IsVideoStarted)
               UpdateVideoViewPositionInternal(kvp.Key, kvp.Value);
         }
      });
   }

   // ── Playback ────────────────────────────────────────────────────────────

   private void StartRtspStream(int slotIndex, string rtspUrl)
   {
      mMainView.WriteMessageLog(MessageSource.LibVlc,
         $"StartRtspStream called: slot={slotIndex} knownSlots=[{string.Join(",", mSlotStates.Keys)}]",
         LogLevel.Debug);

      if (!mSlotStates.TryGetValue(slotIndex, out var state))
      {
         mMainView.WriteMessageLog(MessageSource.LibVlc,
            $"StartRtspStream: slot {slotIndex} not in mSlotStates — initializing on-demand and queuing for replay",
            LogLevel.Warning);
         mPendingStreams[slotIndex] = rtspUrl;
         InitializeSlot(slotIndex);
         TrackSlotPanel(slotIndex);
         return;
      }

      state.ActiveRtspUrl = rtspUrl;

      mActivity.RunOnUiThread(() =>
      {
         mMainView.WriteMessageLog(MessageSource.LibVlc,
            $"StartRtspStream UI: slot={slotIndex} videoView={(state.VideoView == null ? "null" : "ok")}",
            LogLevel.Debug);
         ShowVideoView(state);
         UpdateVideoViewPositionInternal(slotIndex, state);

         var uri   = new Uri(rtspUrl);
         var media = new Media(mLibVlc, uri);
         if (ConfigurationManager.Instance.UseTcp) media.AddOption(":rtsp-tcp");

         // Most Android devices support only 1–4 simultaneous hardware decode
         // sessions.  Slot 0 keeps hardware decode (lowest latency); additional
         // slots force software decode so they are not silently blocked by the
         // HW decoder limit.
         if (slotIndex > 0)
            media.AddOption(":avcodec-hw=none");

         // Limit FFmpeg decode threads per stream.  Without this, FFmpeg
         // auto-selects a thread count based on available CPUs (e.g. 4 on an
         // 8-core device), so two simultaneous streams consume all 8 cores and
         // starve each other.  Two threads per stream leaves headroom.
         media.AddOption(":avcodec-threads=2");

         // Defer Play() to the next layout frame so that the SurfaceView's
         // surfaceCreated callback fires first and LibVLCSharp has a chance to
         // call SetAndroidContext before VLC tries to render.  Without this
         // delay the surface is not yet valid when Play() is called immediately
         // after ShowVideoView(), causing silent playback failure on slot 1+.
         var videoView = state.VideoView;
         if (videoView == null)
         {
            mMainView.WriteMessageLog(MessageSource.LibVlc,
               $"StartRtspStream: VideoView is null for slot {slotIndex} — Play() skipped",
               LogLevel.Error);
            media.Dispose();
         }
         else
         {
            videoView.PostDelayed(() =>
            {
               mMainView.WriteMessageLog(MessageSource.LibVlc,
                  $"StartRtspStream: calling Play() for slot {slotIndex}",
                  LogLevel.Debug);
               state.MediaPlayer.Play(media);
               media.Dispose();
            }, 150);
         }

         state.IsVideoStarted = true;
      });
   }

   private void StopRtspStream(int slotIndex)
   {
      if (!mSlotStates.TryGetValue(slotIndex, out var state)) return;
      if (!state.IsVideoStarted) return;

      state.IsVideoStarted             = false;
      state.IsVideoTemporarilyDetached = false;
      state.ActiveRtspUrl              = null;

      mActivity.RunOnUiThread(() => HideVideoView(state));
      var mp = state.MediaPlayer;
      Task.Run(() => mp?.Stop());
   }

   // ── VideoView visibility helpers ─────────────────────────────────────────

   private static void ShowVideoView(SlotVideoState state)
   {
      if (state.VideoView != null)
         state.VideoView.Visibility = ViewStates.Visible;
   }

   private static void HideVideoView(SlotVideoState state)
   {
      if (state.VideoView != null)
         state.VideoView.Visibility = ViewStates.Gone;
   }

   // ── Overlay surface management ──────────────────────────────────────────

   private void DetachAllVideoSurfacesForOverlay()
   {
      mActivity.RunOnUiThread(() =>
      {
         foreach (var state in mSlotStates.Values)
         {
            if (!state.IsVideoStarted) continue;
            state.IsVideoTemporarilyDetached = true;
            HideVideoView(state);
         }
      });
   }

   private void ReattachAllVideoSurfacesAfterOverlay()
   {
      // Showing the VideoView recreates its surface; LibVLCSharp re-calls
      // SetAndroidContext automatically via the SurfaceHolder callback.
      mActivity.RunOnUiThread(() =>
      {
         foreach (var state in mSlotStates.Values)
         {
            if (!state.IsVideoTemporarilyDetached) continue;
            state.IsVideoTemporarilyDetached = false;
            ShowVideoView(state);
         }
      });
   }

   // ── Reconnect ────────────────────────────────────────────────────────────

   /// <summary>
   /// Schedules a stream restart after a short delay.  Only fires if playback
   /// was not intentionally stopped (IsVideoStarted still true).
   /// </summary>
   private void ScheduleReconnect(int slotIndex, SlotVideoState state)
   {
      Task.Delay(2000).ContinueWith(_ =>
      {
         if (!state.IsVideoStarted || state.ActiveRtspUrl == null) return;
         var url = state.ActiveRtspUrl;
         mMainView.WriteMessageLog(MessageSource.LibVlc,
            $"Reconnecting slot {slotIndex}: {url}",
            LogLevel.Notice);
         StartRtspStream(slotIndex, url);
      });
   }

   // ── Position management ─────────────────────────────────────────────────

   private void UpdateVideoViewPositionInternal(int slotIndex, SlotVideoState state)
   {
      var (x, y, w, h) = GetVideoSlotPixelBounds(slotIndex);
      if (w <= 0 || h <= 0) return;

      if (state.VideoView?.Parent is FrameLayout)
      {
         var lp = new FrameLayout.LayoutParams(w, h);
         lp.LeftMargin = x;
         lp.TopMargin  = y;
         state.VideoView.LayoutParameters = lp;
         state.VideoView.RequestLayout();
      }
   }

   private (int x, int y, int w, int h) GetVideoSlotPixelBounds(int slotIndex)
   {
      var panel = mMainView.GetVideoSlot(slotIndex);
      if (panel == null) return (0, 0, 0, 0);

      var root = panel.GetVisualRoot();
      if (root == null) return (0, 0, 0, 0);

      var transform = panel.TransformToVisual((Visual)root);
      if (transform == null) return (0, 0, 0, 0);

      var topLeft = transform.Value.Transform(new Point(0, 0));
      var bounds  = panel.Bounds;
      double scale = root is TopLevel tl ? tl.RenderScaling : 1.0;

      return ((int)(topLeft.X * scale),
              (int)(topLeft.Y * scale),
              (int)(bounds.Width  * scale),
              (int)(bounds.Height * scale));
   }
}
