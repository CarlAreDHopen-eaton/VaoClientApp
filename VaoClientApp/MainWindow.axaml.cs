using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using LibVLCSharp.Avalonia;
using LibVLCSharp.Shared;

namespace Vao.Sample
{
   public partial class MainWindow : Window
   {
      private LibVLC mLibVlc;
      private MediaPlayer mMediaPlayer;
      private VideoView mVideoControl;
      private string mActiveRtspUrl;
      private bool mIsVideoStarted;
      private bool mIsVideoTemporarilyDetached;
      private App mApp;

      public MainWindow()
      {
         InitializeComponent();

         StartInitializeVlc();

         mainView.RtspStreamRequested    += OnRtspStreamRequested;
         mainView.AnyOverlayStateChanged += OnAnyOverlayStateChanged;
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
         Task.Run(() => { if (mp.IsPlaying) mp.Stop(); mp.Dispose(); });
      }

      private void DetachVideoSurfaceForOverlay()
      {
         var pnlVideo = mainView.VideoSlot;
         if (mVideoControl == null || !pnlVideo.Children.Contains(mVideoControl)) return;
         if (mMediaPlayer != null) mVideoControl.MediaPlayer = null;
         pnlVideo.Children.Remove(mVideoControl);
         mIsVideoTemporarilyDetached = true;
      }

      private void ReattachVideoSurfaceAfterOverlay()
      {
         var pnlVideo = mainView.VideoSlot;
         bool shouldAttach = mIsVideoTemporarilyDetached ||
            (mIsVideoStarted && mMediaPlayer != null &&
             (mVideoControl == null || !pnlVideo.Children.Contains(mVideoControl)));
         if (!shouldAttach) return;

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

      private void MediaPlayer_EncounteredError(object sender, EventArgs e)
         => mainView.WriteMessageLog(MessageSource.LibVlc, "LibVLC error encountered.", LogLevel.Error);

      private void MediaPlayer_Opening(object sender, EventArgs e)
         => mainView.WriteMessageLog(MessageSource.LibVlc, $"LibVLC opening {mMediaPlayer?.Media?.Mrl ?? ""}", LogLevel.Notice);

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
