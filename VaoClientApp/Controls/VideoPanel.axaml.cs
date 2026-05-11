using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
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
      private const int C_MAX_SLOT_COUNT = 16;

      private Camera mCurrentCamera;
      private string mActiveRtspUrl;
      private bool mIsLoadingSettings;
      private ContextMenu mVideoContextMenu;
      private Func<List<Camera>> mGetCameraList;
      private Func<IEnumerable<CameraSelectionItem>> mGetCameraSelectionItems;

      // ── Layout state ────────────────────────────────────────────────────────
      private VideoLayoutDefinition mCurrentLayout;
      private int mActiveSlotIndex;
      private readonly VideoSlotState[] mSlots = new VideoSlotState[C_MAX_SLOT_COUNT];
      private readonly Border[] mSlotBorders = new Border[C_MAX_SLOT_COUNT];

      /// <summary>Fired when an RTSP stream should start (non-null URL) or stop (null) on a specific slot index.</summary>
      public event EventHandler<VideoSlotStreamEventArgs> SlotRtspStreamRequested;

      /// <summary>Fired when an RTSP stream should start (non-null URL) or stop (null). Used for single-view backward compat.</summary>
      public event EventHandler<string> RtspStreamRequested;

      /// <summary>Fired when the sub-channel toggle changes.</summary>
      public event EventHandler<bool> SubChannelChanged;

      /// <summary>Fired when the user selects a camera from the video context menu.</summary>
      public event EventHandler<int> CameraSelectedFromMenu;

      /// <summary>Fired when the layout changes.</summary>
      public event EventHandler<LayoutChangeEventArgs> LayoutChanged;

      /// <summary>Fired before the layout grid is rebuilt. Subscribers should detach video surfaces.</summary>
      public event EventHandler<LayoutChangeEventArgs> LayoutChanging;

      /// <summary>Fired when the active slot changes.</summary>
      public event EventHandler<int> ActiveSlotChanged;

      public VideoPanel()
      {
         InitializeComponent();
         EnsureVideoContextMenu();
         for (int i = 0; i < C_MAX_SLOT_COUNT; i++)
            mSlots[i] = new VideoSlotState();
         mCurrentLayout = VideoLayoutCatalog.Default.GetDefaultLayout();

         // Enable drop on single-view video area
         DragDrop.SetAllowDrop(pnlVideo, true);
         pnlVideo.AddHandler(DragDrop.DropEvent, SingleView_Drop);
         pnlVideo.AddHandler(DragDrop.DragOverEvent, Video_DragOver);

         DragDrop.SetAllowDrop(grpVideoControl, true);
         grpVideoControl.AddHandler(DragDrop.DropEvent, SingleView_Drop);
         grpVideoControl.AddHandler(DragDrop.DragOverEvent, Video_DragOver);
      }

      // ── Public API ─────────────────────────────────────────────────────────

      public VideoLayoutDefinition CurrentLayout => mCurrentLayout;
      public string CurrentLayoutKey => mCurrentLayout?.Key ?? "single";
      public int ActiveSlotIndex => mActiveSlotIndex;

      public bool IsSingleView => mCurrentLayout?.IsSingleView ?? true;

      public Panel VideoSlot => IsSingleView ? pnlVideo : mSlots[mActiveSlotIndex].VideoPanel;

      /// <summary>Returns the video panel for a specific slot index.</summary>
      public Panel GetVideoSlot(int slotIndex)
      {
         if (slotIndex < 0 || slotIndex >= C_MAX_SLOT_COUNT) return pnlVideo;
         return IsSingleView ? pnlVideo : mSlots[slotIndex].VideoPanel;
      }

      /// <summary>Returns all active video slot panels based on the current layout.</summary>
      public IReadOnlyList<Panel> GetAllActiveVideoSlots()
      {
         if (IsSingleView)
            return new[] { pnlVideo };
         int count = mCurrentLayout.SlotCount;
         return mSlots.Take(count).Select(s => s.VideoPanel).Where(p => p != null).ToArray();
      }

      public void SetLayout(string layoutKey)
      {
         var layout = VideoLayoutCatalog.Default.GetLayout(layoutKey);
         SetLayout(layout);
      }

      public void SetLayout(VideoLayoutDefinition layout)
      {
         if (layout == null) return;
         if (mCurrentLayout != null && string.Equals(mCurrentLayout.Key, layout.Key, StringComparison.OrdinalIgnoreCase))
            return;

         var previousLayout = mCurrentLayout;

         // Determine which slot indices survive the transition (only multi↔multi preserves streams)
         var previousSlotIndices = new HashSet<int>();
         var newSlotIndices = new HashSet<int>();
         bool isMultiToMulti = previousLayout != null && !previousLayout.IsSingleView && !layout.IsSingleView;

         if (isMultiToMulti)
         {
            foreach (var s in previousLayout.Slots) previousSlotIndices.Add(s.Index);
            foreach (var s in layout.Slots) newSlotIndices.Add(s.Index);
         }
         else if (previousLayout != null && !previousLayout.IsSingleView)
         {
            foreach (var s in previousLayout.Slots) previousSlotIndices.Add(s.Index);
         }
         else if (!layout.IsSingleView)
         {
            foreach (var s in layout.Slots) newSlotIndices.Add(s.Index);
         }

         var survivingIndices = new HashSet<int>(previousSlotIndices);
         survivingIndices.IntersectWith(newSlotIndices);

         var removedIndices = new HashSet<int>(previousSlotIndices);
         removedIndices.ExceptWith(newSlotIndices);

         var changeArgs = new LayoutChangeEventArgs(previousLayout, layout, survivingIndices, removedIndices);

         // Notify subscribers to detach video surfaces before we rebuild
         LayoutChanging?.Invoke(this, changeArgs);

         // Tear down previous multi-slot UI (but do NOT stop surviving streams)
         if (previousLayout != null && !previousLayout.IsSingleView)
         {
            for (int i = 0; i < previousLayout.SlotCount; i++)
            {
               int idx = previousLayout.Slots[i].Index;
               if (removedIndices.Contains(idx))
                  mSlots[idx].ResetAll();
               else
                  mSlots[idx].ResetUi();
            }
            quadGrid.IsVisible = false;
            quadGrid.Children.Clear();
         }

         mCurrentLayout = layout;

         if (layout.IsSingleView)
         {
            // Transition to single: removed slots will be stopped by MainWindow
            foreach (int idx in removedIndices)
               mSlots[idx].ResetAll();

            grpVideoControl.IsVisible = true;
            mActiveSlotIndex = 0;

            var slot0Camera = mSlots[0].Camera;
            if (slot0Camera != null)
               mCurrentCamera = slot0Camera;
         }
         else
         {
            // Transition from single: always stop the single stream (different VLC player instance)
            if (previousLayout != null && previousLayout.IsSingleView && mActiveRtspUrl != null)
            {
               RtspStreamRequested?.Invoke(this, null);
               mActiveRtspUrl = null;
            }

            grpVideoControl.IsVisible = false;
            BuildLayoutGrid(layout);
            quadGrid.IsVisible = true;
            SetActiveSlot(0);
         }

         LayoutChanged?.Invoke(this, changeArgs);
      }

      public void SetCameraListProvider(Func<List<Camera>> getCameraList, Func<IEnumerable<CameraSelectionItem>> getCameraSelectionItems)
      {
         mGetCameraList = getCameraList;
         mGetCameraSelectionItems = getCameraSelectionItems;
      }

      public void ShowLiveStream(Camera camera, int streamNo)
      {
         if (!IsSingleView)
         {
            ShowLiveStreamOnSlot(mActiveSlotIndex, camera, streamNo);
            return;
         }

         mCurrentCamera = camera;
         mSlots[0].Camera = camera;
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

         if (!IsSingleView)
         {
            var slot = mSlots[mActiveSlotIndex];
            slot.ActiveRtspUrl = url;
            if (slot.HeaderText != null) slot.HeaderText.Text = $"PLAYBACK - Camera {cameraNo}";
            if (slot.HeaderBorder != null) slot.HeaderBorder.Background = GetPlaybackHeaderBrush();
            SlotRtspStreamRequested?.Invoke(this, new VideoSlotStreamEventArgs(mActiveSlotIndex, url));
            return;
         }

         mActiveRtspUrl = url;
         if (txtCurrentRtspUrl != null) txtCurrentRtspUrl.Text = GetMaskedUrl(url);
         if (txtVideoHeader != null) txtVideoHeader.Text = $"PLAYBACK - Camera {cameraNo}";
         if (brdVideoHeader != null) brdVideoHeader.Background = GetPlaybackHeaderBrush();
         RtspStreamRequested?.Invoke(this, url);
      }

      public void ClearStream()
      {
         if (!IsSingleView)
         {
            ClearSlotStream(mActiveSlotIndex);
            return;
         }

         RtspStreamRequested?.Invoke(this, null);
         mActiveRtspUrl = null;
         if (txtCurrentRtspUrl != null) txtCurrentRtspUrl.Text = string.Empty;
         if (txtVideoHeader != null) txtVideoHeader.Text = "No Camera Selected";
         if (brdVideoHeader != null) brdVideoHeader.Background = GetNeutralHeaderBrush();
      }

      public void ResetHeader()
      {
         if (!IsSingleView)
         {
            var slot = mSlots[mActiveSlotIndex];
            if (slot.HeaderText != null) slot.HeaderText.Text = "No Camera Selected";
            if (slot.HeaderBorder != null) slot.HeaderBorder.Background = GetNeutralHeaderBrush();
            return;
         }

         if (txtVideoHeader != null) txtVideoHeader.Text = "No Camera Selected";
         if (brdVideoHeader != null) brdVideoHeader.Background = GetNeutralHeaderBrush();
      }

      public void UpdateSubChannelEnabled(bool enabled)
       {
          if (tglSubChannel != null) tglSubChannel.IsEnabled = enabled;
          var slotToggle = mSlots[mActiveSlotIndex]?.SubChannelToggle;
          if (slotToggle != null) slotToggle.IsEnabled = enabled;
       }

       public bool IsSubChannel
       {
          get
          {
             if (!IsSingleView)
             {
                var slotToggle = mSlots[mActiveSlotIndex]?.SubChannelToggle;
                if (slotToggle != null) return slotToggle.IsChecked == true;
             }
             return tglSubChannel?.IsChecked == true;
          }
       }
       public int GetStreamNo() => IsSubChannel ? 2 : 1;

       public void SetSubChannelChecked(bool isChecked, bool suppressEvent = false)
       {
          if (suppressEvent) mIsLoadingSettings = true;
          if (tglSubChannel != null) tglSubChannel.IsChecked = isChecked;
          var slotToggle = mSlots[mActiveSlotIndex]?.SubChannelToggle;
          if (slotToggle != null) slotToggle.IsChecked = isChecked;
          if (suppressEvent) mIsLoadingSettings = false;
       }

      public string ActiveRtspUrl
      {
         get
         {
            if (!IsSingleView)
               return mSlots[mActiveSlotIndex].ActiveRtspUrl;
            return mActiveRtspUrl;
         }
      }

      public bool IsPlayback => IsActiveRtspPlayback(ActiveRtspUrl);

      /// <summary>Returns the camera assigned to a specific slot, or null.</summary>
      public Camera GetSlotCamera(int slotIndex)
      {
         if (slotIndex < 0 || slotIndex >= C_MAX_SLOT_COUNT) return null;
         return mSlots[slotIndex].Camera;
      }

      /// <summary>Returns the camera for the currently active slot.</summary>
      public Camera GetActiveCamera()
      {
         if (!IsSingleView)
            return mSlots[mActiveSlotIndex].Camera;
         return mCurrentCamera;
      }

      /// <summary>Returns a dictionary of slot index → camera component number for all slots that have a camera.</summary>
      public Dictionary<int, int> GetSlotCameraMap()
      {
         var map = new Dictionary<int, int>();
         for (int i = 0; i < C_MAX_SLOT_COUNT; i++)
         {
            if (mSlots[i].Camera != null)
               map[i] = mSlots[i].Camera.ComponentNumber;
         }
         return map;
      }

      /// <summary>Restores a camera to a slot (used when reloading settings). Only stores the reference; does not start a stream.</summary>
      public void SetSlotCamera(int slotIndex, Camera camera)
      {
         if (slotIndex < 0 || slotIndex >= C_MAX_SLOT_COUNT) return;
         mSlots[slotIndex].Camera = camera;
      }

      public void RefreshHeaderState(bool isStarted, Camera camera, bool isPlayback, bool isPlaybackStarted)
      {
         if (!IsSingleView)
         {
            var slot = mSlots[mActiveSlotIndex];
            var slotCamera = slot.Camera;
            if (!isStarted || slotCamera == null)
            {
               if (slot.HeaderText != null) slot.HeaderText.Text = "No Camera Selected";
               if (slot.HeaderBorder != null) slot.HeaderBorder.Background = GetNeutralHeaderBrush();
               return;
            }
            var cNo = slotCamera.ComponentNumber;
            bool slotPlayback = IsActiveRtspPlayback(slot.ActiveRtspUrl);
            if (slotPlayback || isPlaybackStarted)
            {
               if (slot.HeaderText != null) slot.HeaderText.Text = $"PLAYBACK - Camera {cNo}";
               if (slot.HeaderBorder != null) slot.HeaderBorder.Background = GetPlaybackHeaderBrush();
            }
            else
            {
               if (slot.HeaderText != null) slot.HeaderText.Text = $"LIVE - Camera {cNo}";
               if (slot.HeaderBorder != null) slot.HeaderBorder.Background = GetLiveHeaderBrush();
            }
            return;
         }

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

      // ── Slot operations ─────────────────────────────────────────────────────

      private void ShowLiveStreamOnSlot(int slotIndex, Camera camera, int streamNo)
      {
         var slot = mSlots[slotIndex];
         slot.Camera = camera;
         if (camera == null) return;

         string url = camera.GetCameraLiveStreamUrl(streamNo);
         bool hasSubChannel = !string.IsNullOrEmpty(camera.Stream2Resolution);
         if (streamNo == 2 && !hasSubChannel) streamNo = 1;

         if (!string.IsNullOrEmpty(url))
         {
            slot.ActiveRtspUrl = url;
            if (slot.HeaderText != null) slot.HeaderText.Text = $"LIVE - Camera {camera.ComponentNumber}";
            if (slot.HeaderBorder != null) slot.HeaderBorder.Background = GetLiveHeaderBrush();
            SlotRtspStreamRequested?.Invoke(this, new VideoSlotStreamEventArgs(slotIndex, url));
         }

         mIsLoadingSettings = true;
         if (slot.SubChannelToggle != null)
         {
            slot.SubChannelToggle.IsChecked = (streamNo == 2);
            slot.SubChannelToggle.IsEnabled = hasSubChannel && !IsActiveRtspPlayback(slot.ActiveRtspUrl);
         }
         if (slotIndex == mActiveSlotIndex && tglSubChannel != null)
         {
            tglSubChannel.IsChecked = (streamNo == 2);
            tglSubChannel.IsEnabled = hasSubChannel && !IsActiveRtspPlayback(slot.ActiveRtspUrl);
         }
         mIsLoadingSettings = false;
      }

      private void ClearSlotStream(int slotIndex)
      {
         var slot = mSlots[slotIndex];
         SlotRtspStreamRequested?.Invoke(this, new VideoSlotStreamEventArgs(slotIndex, null));
         slot.ActiveRtspUrl = null;
         slot.Camera = null;
         if (slot.HeaderText != null) slot.HeaderText.Text = "No Camera Selected";
         if (slot.HeaderBorder != null) slot.HeaderBorder.Background = GetNeutralHeaderBrush();
      }

      public void SetActiveSlot(int slotIndex)
      {
         int slotCount = mCurrentLayout?.SlotCount ?? 1;
         if (slotIndex < 0 || slotIndex >= slotCount) return;
         mActiveSlotIndex = slotIndex;
         UpdateSlotSelectionBorders();

         var slot = mSlots[slotIndex];
         mCurrentCamera = slot.Camera;

         ActiveSlotChanged?.Invoke(this, slotIndex);
      }

      private void UpdateSlotSelectionBorders()
      {
         int slotCount = mCurrentLayout?.SlotCount ?? 0;
         for (int i = 0; i < slotCount; i++)
         {
            if (mSlotBorders[i] != null)
            {
               mSlotBorders[i].BorderBrush = i == mActiveSlotIndex
                  ? GetBrushResource("Primary", "#0066CC")
                  : new SolidColorBrush(Color.Parse("#555555"));
               mSlotBorders[i].BorderThickness = new Thickness(2);
            }
         }
      }

      // ── Layout grid builder ─────────────────────────────────────────────────

      private void BuildLayoutGrid(VideoLayoutDefinition layout)
      {
         quadGrid.Children.Clear();
         quadGrid.RowDefinitions.Clear();
         quadGrid.ColumnDefinitions.Clear();

         for (int r = 0; r < layout.Rows; r++)
            quadGrid.RowDefinitions.Add(new RowDefinition(new GridLength(1, GridUnitType.Star)));
         for (int c = 0; c < layout.Columns; c++)
            quadGrid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(1, GridUnitType.Star)));

         for (int i = 0; i < layout.Slots.Count; i++)
         {
            var slotDef = layout.Slots[i];
            int slotIndex = slotDef.Index;

            var headerText = new TextBlock
            {
               Text = "No Camera Selected",
               FontWeight = FontWeight.SemiBold,
               FontSize = 12,
               Foreground = Brushes.White,
               Margin = new Thickness(8, 4),
               Opacity = 0.87,
               VerticalAlignment = VerticalAlignment.Center
            };

            var menuIcon = new TextBlock
            {
               Text = "\uE5D4",
               FontSize = 16,
               Foreground = Brushes.White,
               HorizontalAlignment = HorizontalAlignment.Center,
               VerticalAlignment = VerticalAlignment.Center
            };
            if (this.TryFindResource("MaterialSymbolsOutlined", out var fontRes) && fontRes is FontFamily msFam)
               menuIcon.FontFamily = msFam;
            else
               menuIcon.Classes.Add("ms-icon");

            var menuButton = new Button
            {
               Content = menuIcon,
               Width = 28,
               Height = 28,
               Padding = new Thickness(0),
               Background = Brushes.Transparent,
               BorderThickness = new Thickness(0),
               VerticalAlignment = VerticalAlignment.Center,
               Cursor = new Cursor(StandardCursorType.Hand)
            };
            int capturedSlotIndex = slotIndex;
            menuButton.Click += (s, e) =>
            {
               SetActiveSlot(capturedSlotIndex);
               VideoMenuButton_Click(s, e);
            };

            var slotToggle = new ToggleSwitch
            {
               OnContent = "Sub",
               OffContent = "Main",
               IsChecked = true,
               VerticalAlignment = VerticalAlignment.Center,
               VerticalContentAlignment = VerticalAlignment.Center,
               HorizontalContentAlignment = HorizontalAlignment.Center,
               Padding = new Thickness(0),
               Margin = new Thickness(0, 0, 4, 0),
               MinHeight = 0,
               Foreground = Brushes.White,
               IsEnabled = false
            };
            int toggleSlotIndex = slotIndex;
            slotToggle.IsCheckedChanged += (s, e) =>
            {
               if (mIsLoadingSettings) return;
               SetActiveSlot(toggleSlotIndex);
               SubChannelChanged?.Invoke(this, slotToggle.IsChecked == true);
            };

            var headerGrid = new Grid
            {
               ColumnDefinitions = { new ColumnDefinition(new GridLength(1, GridUnitType.Star)), new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Auto) }
            };
            Grid.SetColumn(headerText, 0);
            Grid.SetColumn(slotToggle, 1);
            Grid.SetColumn(menuButton, 2);
            headerGrid.Children.Add(headerText);
            headerGrid.Children.Add(slotToggle);
            headerGrid.Children.Add(menuButton);

            var headerBorder = new Border
            {
               Background = GetNeutralHeaderBrush(),
               CornerRadius = new CornerRadius(4, 4, 0, 0),
               Child = headerGrid
            };

            var videoPanel = new Panel { Background = GetBrushResource("VideoBg", "#000000") };

            var innerGrid = new Grid
            {
               RowDefinitions = { new RowDefinition(GridLength.Auto), new RowDefinition(new GridLength(1, GridUnitType.Star)) }
            };
            Grid.SetRow(headerBorder, 0);
            Grid.SetRow(videoPanel, 1);
            innerGrid.Children.Add(headerBorder);
            innerGrid.Children.Add(videoPanel);

            var slotBorder = new Border
            {
               Background = GetBrushResource("Surface1", "#1E1E1E"),
               CornerRadius = new CornerRadius(4),
               Margin = new Thickness(2),
               BorderBrush = new SolidColorBrush(Colors.Transparent),
               BorderThickness = new Thickness(0),
               Child = innerGrid
            };

            int capturedIdx = slotIndex;
            slotBorder.PointerPressed += (_, e) =>
            {
               SetActiveSlot(capturedIdx);
               e.Handled = true;
            };

            // Enable drag-drop on each slot
            DragDrop.SetAllowDrop(slotBorder, true);
            slotBorder.AddHandler(DragDrop.DragOverEvent, Video_DragOver);
            int dropSlotIndex = slotIndex;
            slotBorder.AddHandler(DragDrop.DropEvent, (object s, DragEventArgs e) => SlotBorder_Drop(s, e, dropSlotIndex));

            Grid.SetRow(slotBorder, slotDef.Row);
            Grid.SetColumn(slotBorder, slotDef.Column);
            if (slotDef.RowSpan > 1) Grid.SetRowSpan(slotBorder, slotDef.RowSpan);
            if (slotDef.ColumnSpan > 1) Grid.SetColumnSpan(slotBorder, slotDef.ColumnSpan);
            quadGrid.Children.Add(slotBorder);

            mSlots[slotIndex].HeaderText = headerText;
             mSlots[slotIndex].HeaderBorder = headerBorder;
             mSlots[slotIndex].VideoPanel = videoPanel;
             mSlots[slotIndex].SubChannelToggle = slotToggle;
             mSlotBorders[slotIndex] = slotBorder;

            // Restore header if this slot already has a camera remembered
            if (mSlots[slotIndex].Camera != null)
            {
               headerText.Text = $"Camera {mSlots[slotIndex].Camera.ComponentNumber}";
            }
         }

         UpdateSlotSelectionBorders();
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

         // Layout options from catalog
         var layoutMenu = new MenuItem { Header = "Layout" };
         var layoutItems = new List<object>();
         var allLayouts = VideoLayoutCatalog.Default.GetAllLayouts();
         foreach (var layout in allLayouts)
         {
            var layoutItem = new MenuItem { Header = layout.DisplayName };
            if (string.Equals(mCurrentLayout?.Key, layout.Key, StringComparison.OrdinalIgnoreCase))
               layoutItem.Icon = new CheckBox { IsChecked = true, IsHitTestVisible = false };
            var capturedLayout = layout;
            layoutItem.Click += (_, _) => SetLayout(capturedLayout);
            layoutItems.Add(layoutItem);
         }
         layoutMenu.ItemsSource = layoutItems;
         rootItems.Add(layoutMenu);
         rootItems.Add(new Separator());

         // Camera list
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

         var currentCam = !IsSingleView ? mSlots[mActiveSlotIndex].Camera : mCurrentCamera;
         var item = new MenuItem { Header = header, Tag = camera };
         if (currentCam == camera)
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

      private void Video_DragOver(object sender, DragEventArgs e)
      {
         if (e.Data.Contains("CameraComponentNumber"))
            e.DragEffects = DragDropEffects.Copy;
         else
            e.DragEffects = DragDropEffects.None;
      }

      private void SingleView_Drop(object sender, DragEventArgs e)
      {
         if (e.Data.Get("CameraComponentNumber") is int cameraNo)
            CameraSelectedFromMenu?.Invoke(this, cameraNo);
      }

      private void SlotBorder_Drop(object sender, DragEventArgs e, int slotIndex)
      {
         HandleSlotDrop(slotIndex, e);
      }

      public void HandleSlotDrop(int slotIndex, DragEventArgs e)
      {
         if (e.Data.Get("CameraComponentNumber") is int cameraNo)
         {
            SetActiveSlot(slotIndex);
            CameraSelectedFromMenu?.Invoke(this, cameraNo);
         }
      }

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

      // ── Nested types ────────────────────────────────────────────────────────

      private class VideoSlotState
      {
         public Camera Camera { get; set; }
         public string ActiveRtspUrl { get; set; }
         public TextBlock HeaderText { get; set; }
         public Border HeaderBorder { get; set; }
         public Panel VideoPanel { get; set; }
         public ToggleSwitch SubChannelToggle { get; set; }

         /// <summary>Resets UI elements but preserves the Camera reference so it can be remembered across layout switches.</summary>
         public void ResetUi()
         {
            ActiveRtspUrl = null;
            HeaderText = null;
            HeaderBorder = null;
            VideoPanel = null;
            SubChannelToggle = null;
         }

         public void ResetAll()
         {
            Camera = null;
            ResetUi();
         }
      }
   }

   public class VideoSlotStreamEventArgs : EventArgs
   {
      public int SlotIndex { get; }
      public string Url { get; }

      public VideoSlotStreamEventArgs(int slotIndex, string url)
      {
         SlotIndex = slotIndex;
         Url = url;
      }
   }

   public class LayoutChangeEventArgs : EventArgs
   {
      public VideoLayoutDefinition PreviousLayout { get; }
      public VideoLayoutDefinition NewLayout { get; }

      /// <summary>Slot indices that exist in both old and new layouts — their streams can be preserved.</summary>
      public IReadOnlySet<int> SurvivingSlotIndices { get; }

      /// <summary>Slot indices that existed in the old layout but not the new — their streams must be stopped.</summary>
      public IReadOnlySet<int> RemovedSlotIndices { get; }

      public LayoutChangeEventArgs(
         VideoLayoutDefinition previousLayout,
         VideoLayoutDefinition newLayout,
         HashSet<int> survivingSlotIndices,
         HashSet<int> removedSlotIndices)
      {
         PreviousLayout = previousLayout;
         NewLayout = newLayout;
         SurvivingSlotIndices = survivingSlotIndices;
         RemovedSlotIndices = removedSlotIndices;
      }
   }
}
