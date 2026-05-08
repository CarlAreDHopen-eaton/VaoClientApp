using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Vao.Client.Components;

namespace Vao.Sample.Controls
{
   public partial class VideoPanel : UserControl
   {
      private Camera mCurrentCamera;
      private string mActiveRtspUrl;
      private bool mIsLoadingSettings;
      private ContextMenu mVideoContextMenu;
      private Func<List<Camera>> mGetCameraList;
      private Func<IEnumerable<CameraSelectionItem>> mGetCameraSelectionItems;

      /// <summary>Fired when an RTSP stream should start (non-null URL) or stop (null).</summary>
      public event EventHandler<string> RtspStreamRequested;

      /// <summary>Fired when the sub-channel toggle changes.</summary>
      public event EventHandler<bool> SubChannelChanged;

      /// <summary>Fired when the user selects a camera from the video context menu.</summary>
      public event EventHandler<int> CameraSelectedFromMenu;

      public VideoPanel()
      {
         InitializeComponent();
         EnsureVideoContextMenu();
      }

      // ── Public API ─────────────────────────────────────────────────────────

      public Panel VideoSlot => pnlVideo;

      public void SetCameraListProvider(Func<List<Camera>> getCameraList, Func<IEnumerable<CameraSelectionItem>> getCameraSelectionItems)
      {
         mGetCameraList = getCameraList;
         mGetCameraSelectionItems = getCameraSelectionItems;
      }

      public void ShowLiveStream(Camera camera, int streamNo)
      {
         mCurrentCamera = camera;
         if (camera == null) return;

         string url = camera.GetCameraLiveStreamUrl(streamNo);
         bool hasSubChannel = !string.IsNullOrEmpty(camera.Stream2Resolution);
         if (streamNo == 2 && !hasSubChannel) streamNo = 1;

         if (!string.IsNullOrEmpty(url))
         {
            mActiveRtspUrl = url;
            if (txtCurrentRtspUrl != null) txtCurrentRtspUrl.Text = GetMaskedUrl(url);
            if (txtVideoHeader != null) txtVideoHeader.Text = $"LIVE - Camera {camera.ComponentNumber}";
            if (brdVideoHeader != null) brdVideoHeader.Background = GetLiveHeaderBrush();
            RtspStreamRequested?.Invoke(this, url);
         }

         mIsLoadingSettings = true;
         if (tglSubChannel != null)
         {
            tglSubChannel.IsChecked = (streamNo == 2);
            tglSubChannel.IsEnabled = hasSubChannel && !IsActiveRtspPlayback(mActiveRtspUrl);
         }
         mIsLoadingSettings = false;
      }

      public void ShowPlaybackStream(string url, int cameraNo)
      {
         if (string.IsNullOrEmpty(url)) return;
         mActiveRtspUrl = url;
         if (txtCurrentRtspUrl != null) txtCurrentRtspUrl.Text = GetMaskedUrl(url);
         if (txtVideoHeader != null) txtVideoHeader.Text = $"PLAYBACK - Camera {cameraNo}";
         if (brdVideoHeader != null) brdVideoHeader.Background = GetPlaybackHeaderBrush();
         RtspStreamRequested?.Invoke(this, url);
      }

      public void ClearStream()
      {
         RtspStreamRequested?.Invoke(this, null);
         mActiveRtspUrl = null;
         if (txtCurrentRtspUrl != null) txtCurrentRtspUrl.Text = string.Empty;
         if (txtVideoHeader != null) txtVideoHeader.Text = "No Camera Selected";
         if (brdVideoHeader != null) brdVideoHeader.Background = GetNeutralHeaderBrush();
      }

      public void ResetHeader()
      {
         if (txtVideoHeader != null) txtVideoHeader.Text = "No Camera Selected";
         if (brdVideoHeader != null) brdVideoHeader.Background = GetNeutralHeaderBrush();
      }

      public void UpdateSubChannelEnabled(bool enabled)
      {
         if (tglSubChannel != null) tglSubChannel.IsEnabled = enabled;
      }

      public bool IsSubChannel => tglSubChannel?.IsChecked == true;
      public int GetStreamNo() => tglSubChannel?.IsChecked == true ? 2 : 1;

      public void SetSubChannelChecked(bool isChecked, bool suppressEvent = false)
      {
         if (suppressEvent) mIsLoadingSettings = true;
         if (tglSubChannel != null) tglSubChannel.IsChecked = isChecked;
         if (suppressEvent) mIsLoadingSettings = false;
      }

      public string ActiveRtspUrl => mActiveRtspUrl;
      public bool IsPlayback => IsActiveRtspPlayback(mActiveRtspUrl);

      public void RefreshHeaderState(bool isStarted, Camera camera, bool isPlayback, bool isPlaybackStarted)
      {
         if (!isStarted || camera == null)
         {
            if (txtVideoHeader != null) txtVideoHeader.Text = "No Camera Selected";
            if (brdVideoHeader != null) brdVideoHeader.Background = GetNeutralHeaderBrush();
            return;
         }

         var cameraNo = camera.ComponentNumber;
         if (isPlayback || isPlaybackStarted)
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

      // ── Event handlers ─────────────────────────────────────────────────────

      private void tglSubChannel_CheckedChanged(object sender, RoutedEventArgs e)
      {
         if (mIsLoadingSettings) return;
         SubChannelChanged?.Invoke(this, tglSubChannel.IsChecked == true);
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
         catch { }
      }

      // ── Video context menu ─────────────────────────────────────────────────

      private void EnsureVideoContextMenu()
      {
         mVideoContextMenu ??= new ContextMenu();
      }

      private List<object> BuildVideoContextMenuItems()
      {
         var rootItems = new List<object>();
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
         var list = mGetCameraList?.Invoke();
         if (list != null && list.Count > 0) return list;
         var items = mGetCameraSelectionItems?.Invoke();
         if (items == null) return new List<Camera>();
         return items.Select(i => i.Camera).Where(c => c != null)
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
         item.Click += (s, e) => CameraSelectedFromMenu?.Invoke(this, camera.ComponentNumber);
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

      // ── Helpers ────────────────────────────────────────────────────────────

      private static bool IsActiveRtspPlayback(string url) => url != null && url.Contains("playback");

      private static string GetMaskedUrl(string url)
      {
         try { return new UriBuilder(url) { Password = "******", UserName = "******" }.ToString(); }
         catch { return url ?? string.Empty; }
      }

      private IBrush GetBrushResource(string key, string fallback)
      {
         if (this.TryFindResource(key, this.ActualThemeVariant, out var resource) && resource is IBrush b)
            return b;
         return new SolidColorBrush(Color.Parse(fallback));
      }

      private IBrush GetNeutralHeaderBrush()  => GetBrushResource("VideoHeaderNeutral", "#1D3A4A");
      private IBrush GetLiveHeaderBrush()     => GetBrushResource("VideoHeaderLive", "#39B620");
      private IBrush GetPlaybackHeaderBrush() => GetBrushResource("VideoHeaderPlayback", "#CA3C3D");
   }
}
