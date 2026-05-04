using System;
using System.Threading.Tasks;
using Android.App;
using Android.Widget;
using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;
using LibVLCSharp.Shared;
using Vao.Client.Enum;

namespace Vao.Sample;

/// <summary>
/// Owns the LibVLC / MediaPlayer lifecycle for the Android single-view target.
/// Uses LibVLCSharp.Platforms.Android.VideoView (a native SurfaceView overlay)
/// added to the Activity's content frame, positioned over pnlVideo.
///
/// Design: MediaPlayer and VideoView are created eagerly in the constructor so
/// the Android surface lifecycle (surfaceCreated → SetAndroidContext) completes
/// long before the first playback request arrives, avoiding the native SIGSEGV
/// that occurs when Play() is called before VLC has received a valid surface.
/// </summary>
internal sealed class AndroidVideoController
{
   private readonly MainView mMainView;
   private readonly Activity mActivity;
   private LibVLC mLibVlc;
   private MediaPlayer mMediaPlayer;
   private LibVLCSharp.Platforms.Android.VideoView mAndroidVideoView;
   private string mActiveRtspUrl;
   private bool mIsVideoStarted;
   private bool mIsVideoTemporarilyDetached;

   public AndroidVideoController(MainView mainView, Activity activity)
   {
      mMainView = mainView;
      mActivity = activity;
      mMainView.RtspStreamRequested    += OnRtspStreamRequested;
      mMainView.AnyOverlayStateChanged += OnAnyOverlayStateChanged;
      mMainView.VideoSlot.LayoutUpdated += OnVideoSlotLayoutUpdated;

      // VLC and VideoView are initialised immediately (OnCreate is on the main
      // thread) so the surface can be created by the layout system well before
      // the first StartRtspStream call.
      InitializeVlc();
      InitializeVideoView();
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
      mLibVlc      = new LibVLC(enableDebugLogs: false, options);

      mMediaPlayer                    = new MediaPlayer(mLibVlc);
      mMediaPlayer.EnableKeyInput     = false;
      mMediaPlayer.EnableMouseInput   = false;
      mMediaPlayer.EncounteredError  += MediaPlayer_EncounteredError;
      mMediaPlayer.Opening           += MediaPlayer_Opening;
   }

   /// <summary>
   /// Creates the VideoView and wires the MediaPlayer once.  Called from the
   /// constructor (which runs on the Android main thread via OnCreate), so
   /// RunOnUiThread executes synchronously here.
   /// </summary>
   private void InitializeVideoView()
   {
      mActivity.RunOnUiThread(() =>
      {
         mAndroidVideoView = new LibVLCSharp.Platforms.Android.VideoView(mActivity);
         mAndroidVideoView.Visibility = Android.Views.ViewStates.Gone;

         var contentFrame = mActivity.FindViewById<FrameLayout>(Android.Resource.Id.Content);
         var lp = new FrameLayout.LayoutParams(
            FrameLayout.LayoutParams.MatchParent,
            FrameLayout.LayoutParams.MatchParent);
         contentFrame.AddView(mAndroidVideoView, lp);

         // Wire MediaPlayer now.  LibVLCSharp will call SetAndroidContext when
         // surfaceCreated fires (first layout pass), well before Play() is called.
         mAndroidVideoView.MediaPlayer = mMediaPlayer;
      });
   }

   // ── VLC log ─────────────────────────────────────────────────────────────

   private void LibVlc_Log(object sender, LogEventArgs e)
   {
      if (e.Level == LogLevel.Debug) return;
      if (e.Message?.ToLower() == "unsupported control query 3") return;
      mMainView.WriteMessageLog(MessageSource.LibVlc, e.Message, e.Level);
   }

   // ── Event handlers ──────────────────────────────────────────────────────

   private void OnRtspStreamRequested(object sender, string url)
   {
      if (string.IsNullOrEmpty(url))
         StopRtspStream();
      else
         StartRtspStream(url);
   }

   private void OnAnyOverlayStateChanged(object sender, bool isOverlayVisible)
   {
      if (isOverlayVisible)
         DetachVideoSurfaceForOverlay();
      else
         ReattachVideoSurfaceAfterOverlay();
   }

   private void OnVideoSlotLayoutUpdated(object sender, EventArgs e)
   {
      if (mIsVideoStarted)
         UpdateVideoViewPosition();
   }

   // ── Playback ────────────────────────────────────────────────────────────

   private void StartRtspStream(string rtspUrl)
   {
      mActiveRtspUrl = rtspUrl;

      mActivity.RunOnUiThread(() =>
      {
         ShowVideoView();
         UpdateVideoViewPositionInternal();

         var uri   = new Uri(rtspUrl);
         var media = new Media(mLibVlc, uri);
         if (AppSettings.Default.UseTcp) media.AddOption(":rtsp-tcp");

         // MediaPlayer is already wired to the VideoView; surface context was
         // established when the layout system called surfaceCreated.
         mMediaPlayer.Play(media);
         media.Dispose();
         mIsVideoStarted = true;
      });
   }

   private void StopRtspStream()
   {
      if (!mIsVideoStarted) return;
      mIsVideoStarted            = false;
      mIsVideoTemporarilyDetached = false;
      mActiveRtspUrl             = null;

      mActivity.RunOnUiThread(HideVideoView);
      Task.Run(() => mMediaPlayer.Stop());
   }

   // ── VideoView visibility helpers ─────────────────────────────────────────

   private void ShowVideoView()
   {
      if (mAndroidVideoView != null)
         mAndroidVideoView.Visibility = Android.Views.ViewStates.Visible;
   }

   private void HideVideoView()
   {
      if (mAndroidVideoView != null)
         mAndroidVideoView.Visibility = Android.Views.ViewStates.Gone;
   }

   // ── Overlay surface management ──────────────────────────────────────────

   private void DetachVideoSurfaceForOverlay()
   {
      if (!mIsVideoStarted) return;
      mIsVideoTemporarilyDetached = true;
      mActivity.RunOnUiThread(HideVideoView);
   }

   private void ReattachVideoSurfaceAfterOverlay()
   {
      if (!mIsVideoTemporarilyDetached) return;
      mIsVideoTemporarilyDetached = false;
      // Showing the VideoView recreates its surface; LibVLCSharp re-calls
      // SetAndroidContext automatically via the SurfaceHolder callback.
      mActivity.RunOnUiThread(ShowVideoView);
   }

   // ── Position management ─────────────────────────────────────────────────

   private void UpdateVideoViewPosition()
   {
      mActivity.RunOnUiThread(UpdateVideoViewPositionInternal);
   }

   private void UpdateVideoViewPositionInternal()
   {
      var (x, y, w, h) = GetVideoSlotPixelBounds();
      if (w <= 0 || h <= 0) return;

      if (mAndroidVideoView?.Parent is FrameLayout)
      {
         var lp = new FrameLayout.LayoutParams(w, h);
         lp.LeftMargin = x;
         lp.TopMargin  = y;
         mAndroidVideoView.LayoutParameters = lp;
         mAndroidVideoView.RequestLayout();
      }
   }

   private (int x, int y, int w, int h) GetVideoSlotPixelBounds()
   {
      var panel = mMainView.VideoSlot;
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

   // ── MediaPlayer callbacks ───────────────────────────────────────────────

   private void MediaPlayer_EncounteredError(object sender, EventArgs e)
      => mMainView.WriteMessageLog(MessageSource.LibVlc, "LibVLC error encountered.", LogLevel.Error);

   private void MediaPlayer_Opening(object sender, EventArgs e)
      => mMainView.WriteMessageLog(MessageSource.LibVlc, $"LibVLC opening {mMediaPlayer?.Media?.Mrl ?? ""}", LogLevel.Notice);
}
