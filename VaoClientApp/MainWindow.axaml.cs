using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using LibVLCSharp.Avalonia;
using LibVLCSharp.Shared;
using Vao.Sample.Controls;

namespace Vao.Sample
{
   public partial class MainWindow : Window
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

      private App mApp;

      public MainWindow()
      {
         InitializeComponent();

         StartInitializeVlc();

         mainView.RtspStreamRequested    += OnRtspStreamRequested;
         mainView.SlotRtspStreamRequested += OnSlotRtspStreamRequested;
         mainView.AnyOverlayStateChanged += OnAnyOverlayStateChanged;
         mainView.VideoLayoutChanging    += OnVideoLayoutChanging;
         mainView.VideoLayoutChanged     += OnVideoLayoutChanged;
         mainView.ConnectionStateChanged += (_, _) => UpdateWindowTitle();

         Opened += MainWindow_Opened;
         AddHandler(KeyDownEvent, MainWindow_KeyDown, handledEventsToo: true);
         SizeChanged += MainWindow_SizeChanged;

         mApp = (App)Application.Current;
         if (mApp != null)
            mApp.ThemeApplied += OnThemeApplied;

         Closed += MainWindow_Closed;

         UpdateWindowTitle();
      }

      // ── Window events ───────────────────────────────────────────────────────

      private void OnThemeApplied(ThemeDefinition theme)
      {
         UpdateWindowTitle();
      }

      private void MainWindow_Opened(object sender, EventArgs e)
      {
         Opened -= MainWindow_Opened;
         mainView.OnWindowOpened(Bounds.Width);
      }

      private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
      {
         mainView.HandleWindowResize(e.NewSize.Width);
      }

      protected override void OnClosing(WindowClosingEventArgs e)
      {
         mainView.SaveCurrentSettings();
         mainView.StopClient();
         base.OnClosing(e);
      }

      private void MainWindow_Closed(object sender, EventArgs e)
      {
         if (mApp != null)
         {
            mApp.ThemeApplied -= OnThemeApplied;
            mApp = null;
         }
      }

      // ── Key bindings ────────────────────────────────────────────────────────

      private void MainWindow_KeyDown(object sender, KeyEventArgs e)
      {
         if (e.Key == Key.F2)
         {
            mainView.HandleRenameCameraKey();
            e.Handled = true;
         }
         else if (e.Key == Key.F9)
         {
            mainView.ToggleSidebar();
            e.Handled = true;
         }
         else if (e.Key == Key.F10 && !mainView.IsTextInputFocused())
         {
            mainView.CycleThemeFromKeyboard();
            e.Handled = true;
         }
         else if (e.Key == Key.F11)
         {
            mainView.OpenMessageLogFromKeyboard();
            e.Handled = true;
         }
         else if (e.Key == Key.F12)
         {
            mainView.OpenSettingsFromKeyboard();
            e.Handled = true;
         }
         else if (e.Key == Key.Escape)
         {
            mainView.HandleEscapeKey();
            e.Handled = true;
         }

         var isShiftCtrl = (e.KeyModifiers & KeyModifiers.Shift) != 0 &&
                           (e.KeyModifiers & KeyModifiers.Control) != 0;

         if (isShiftCtrl)
         {
            var keySymbol = e.KeySymbol ?? string.Empty;
            if (keySymbol == "," || keySymbol == "<" || e.PhysicalKey == PhysicalKey.Comma)
            {
               mainView.NavigatePreviousCamera();
               e.Handled = true;
            }
            else if (keySymbol == "." || keySymbol == ">" || e.PhysicalKey == PhysicalKey.Period)
            {
               mainView.NavigateNextCamera();
               e.Handled = true;
            }
            else
            {
               var slot = HotkeySlotFromPhysicalKey(e.PhysicalKey);
               if (slot >= 0 && mainView.IsStarted)
               {
                  mainView.AssignCameraHotkey(slot);
                  e.Handled = true;
               }
            }
         }
         else if ((e.KeyModifiers & KeyModifiers.Control) != 0 && (e.KeyModifiers & KeyModifiers.Shift) == 0)
         {
            var slot = HotkeySlotFromPhysicalKey(e.PhysicalKey);
            if (slot >= 0 && mainView.IsStarted)
            {
               var hotkeys = AppSettings.Default.CameraHotkeys;
               if (hotkeys.TryGetValue(slot, out int cameraNo) && cameraNo != 0)
               {
                  mainView.SelectCameraHotkey(cameraNo);
                  e.Handled = true;
               }
            }
         }
      }

      private static int HotkeySlotFromPhysicalKey(PhysicalKey key) => key switch
      {
         PhysicalKey.Digit0 => 0,
         PhysicalKey.Digit1 => 1,
         PhysicalKey.Digit2 => 2,
         PhysicalKey.Digit3 => 3,
         PhysicalKey.Digit4 => 4,
         PhysicalKey.Digit5 => 5,
         PhysicalKey.Digit6 => 6,
         PhysicalKey.Digit7 => 7,
         PhysicalKey.Digit8 => 8,
         PhysicalKey.Digit9 => 9,
         _ => -1
      };

      // ── Stream events ───────────────────────────────────────────────────────

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

      // ── VLC ─────────────────────────────────────────────────────────────────

      private void StartInitializeVlc()
      {
         Core.Initialize();
         mainView.WriteMessageLog(MessageSource.LibVlc, "Loading VLC", LogLevel.Notice);
         var options = new[] { "-vv", "--rtsp-timeout=300", "--network-caching=300" };
         mLibVlc = new LibVLC(true, options);
         mLibVlc.Log += LibVlc_Log;
      }

      private void LibVlc_Log(object sender, LogEventArgs e)
      {
         if (e.Level == LogLevel.Debug) return;
         if (e.Message.ToLower() == "unsupported control query 3") return;
         mainView.WriteMessageLog(MessageSource.LibVlc, e.Message, e.Level);
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
            mMediaPlayer.EnableKeyInput = false;
            mMediaPlayer.EnableMouseInput = false;
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
         var pnlVideo = mainView.VideoSlot;
         if (mVideoControl == null)
         {
            mVideoControl = new VideoView();
            mVideoControl.Focusable = false;
         }
         if (!pnlVideo.Children.Contains(mVideoControl))
            pnlVideo.Children.Add(mVideoControl);
         mIsVideoTemporarilyDetached = false;
      }

      private void StopRtspStream()
      {
         if (mMediaPlayer != null)
         {
            var mp = mMediaPlayer;
            mMediaPlayer = null;
            var pnlVideo = mainView.VideoSlot;
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

      private static void DisposeMediaPlayerAsync(MediaPlayer mp)
      {
         if (mp == null) return;
         Task.Run(() => { if (mp.IsPlaying) mp.Stop(); mp.Dispose(); });
      }

      private void DetachVideoSurfaceForOverlay()
      {
         // Detach single-view
         var pnlVideo = mainView.VideoSlot;
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

      private void ReattachVideoSurfaceAfterOverlay()
      {
         // Reattach single-view
         var pnlVideo = mainView.VideoSlot;
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
                        if (mIsVideoStarted) StartRtspStream(mActiveRtspUrl);
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
            var slotPanel = mainView.GetVideoSlot(i);
            if (slotPanel == null) continue;

            // Recreate the VideoView for the slot (LibVLC requires fresh surface)
            var newView = new VideoView { Focusable = false };
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

      private void MediaPlayer_EncounteredError(object sender, EventArgs e)
         => mainView.WriteMessageLog(MessageSource.LibVlc, "LibVLC error encountered.", LogLevel.Error);

      private void MediaPlayer_Opening(object sender, EventArgs e)
         => mainView.WriteMessageLog(MessageSource.LibVlc, $"LibVLC opening {mMediaPlayer?.Media?.Mrl ?? ""}", LogLevel.Notice);

      // ── Quad mode VLC ──────────────────────────────────────────────────────

      private void OnSlotRtspStreamRequested(object sender, VideoSlotStreamEventArgs e)
      {
         if (string.IsNullOrEmpty(e.Url))
            StopSlotStream(e.SlotIndex);
         else
            StartSlotStream(e.SlotIndex, e.Url);
      }

      /// <summary>Before layout rebuild: detach surviving VideoViews from their current parents so they can be reparented.</summary>
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

      /// <summary>After layout rebuild: reattach surviving VideoViews to new panels, stop removed slots.</summary>
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
            var pnlVideo = mainView.GetVideoSlot(idx);
            if (pnlVideo != null && !pnlVideo.Children.Contains(videoView))
               pnlVideo.Children.Add(videoView);
         }
      }

      private void StartSlotStream(int slotIndex, string rtspUrl)
      {
         mSlotRtspUrls[slotIndex] = rtspUrl;
         if (mSlotIsStarted[slotIndex]) StopSlotStream(slotIndex);

         var pnlVideo = mainView.GetVideoSlot(slotIndex);
         if (pnlVideo == null) return;

         var videoView = new VideoView { Focusable = false };
         mSlotVideoControls[slotIndex] = videoView;
         if (!pnlVideo.Children.Contains(videoView))
            pnlVideo.Children.Add(videoView);

         var uri = new Uri(rtspUrl);
         var media = new Media(mLibVlc, uri);
         if (AppSettings.Default.UseTcp) media.AddOption(":rtsp-tcp");

         var mp = new MediaPlayer(media)
         {
            EnableKeyInput = false,
            EnableMouseInput = false
         };
         mp.EncounteredError += (_, _) =>
            mainView.WriteMessageLog(MessageSource.LibVlc, $"LibVLC error on slot {slotIndex}.", LogLevel.Error);
         mp.Opening += (_, _) =>
            mainView.WriteMessageLog(MessageSource.LibVlc, $"LibVLC opening slot {slotIndex}: {mp.Media?.Mrl ?? ""}", LogLevel.Notice);

         mSlotMediaPlayers[slotIndex] = mp;
         videoView.MediaPlayer = mp;
         mp.Play();
         mSlotIsStarted[slotIndex] = true;
      }

      private void StopSlotStream(int slotIndex)
      {
         var mp = mSlotMediaPlayers[slotIndex];
         if (mp == null) return;

         mSlotMediaPlayers[slotIndex] = null;
         var pnlVideo = mainView.GetVideoSlot(slotIndex);
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

      // ── Window title ────────────────────────────────────────────────────────

      private void UpdateWindowTitle()
      {
         var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
         string versionString = version != null ? $" v{version.Major}.{version.Minor}.{version.Build}" : "";
         string baseTitle = $"HERNIS FLEX VAO API Demo{versionString}";
         Title = mainView.IsStarted ? baseTitle : $"{baseTitle} (Not connected)";
      }
   }
}
