using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Vao.Sample.Layouts;
using Vao.Sample.Themes;

namespace Vao.Sample.Controls;

/// <summary>Sidebar panel that displays available video split layouts with generated icons.</summary>
public partial class SplitSelectorPanel : UserControl
{
   #region Private Members

   private readonly ObservableCollection<SplitSelectionItem> mSplitItems = new();
   private readonly List<(Canvas Canvas, SplitSelectionItem Item)> mIconCanvases = new();
   private bool mIsUpdatingSelection;
   private bool mIsResizing;
   private double mResizeStartHeight;
   private Point mResizeStartPoint;

   #endregion

   #region Public Events

   /// <summary>Fired when the user selects a layout. EventArgs contains the layout key.</summary>
   public event EventHandler<string> LayoutSelected;

   /// <summary>Fired when the panel is resized by the user.</summary>
   public event EventHandler LayoutChanged;

   #endregion

   #region Constructors

   public SplitSelectorPanel()
   {
      InitializeComponent();
      lstSplitSelection.ItemsSource = mSplitItems;
   }

   #endregion

   #region Internal Methods

   /// <inheritdoc/>
   protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
   {
      base.OnAttachedToVisualTree(e);
      if (Application.Current is App app)
         app.ThemeApplied += OnThemeApplied;
   }

   /// <inheritdoc/>
   protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
   {
      base.OnDetachedFromVisualTree(e);
      if (Application.Current is App app)
         app.ThemeApplied -= OnThemeApplied;
   }

   #endregion

   #region Public Methods

   /// <summary>Populates the list with all available layouts from the catalog.</summary>
   public void Fill()
   {
      mSplitItems.Clear();
      var catalog = VideoLayoutCatalog.Default;
      foreach (VideoLayoutDefinition layout in catalog.GetAllLayouts())
      {
         mSplitItems.Add(new SplitSelectionItem(layout));
      }
   }

   /// <summary>Synchronizes the selected item to match the current layout key.</summary>
   public void SyncSelection(string layoutKey)
   {
      mIsUpdatingSelection = true;
      lstSplitSelection.SelectedItem = mSplitItems.FirstOrDefault(
         item => string.Equals(item.Key, layoutKey, StringComparison.OrdinalIgnoreCase));
      mIsUpdatingSelection = false;
   }

   #endregion

   #region Private Methods

   private void lstSplitSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
   {
      if (mIsUpdatingSelection) return;
      if (lstSplitSelection.SelectedItem is SplitSelectionItem item)
      {
         LayoutSelected?.Invoke(this, item.Key);
      }
   }

   private void OnThemeApplied(ThemeDefinition theme)
   {
      foreach ((Canvas canvas, SplitSelectionItem item) in mIconCanvases)
         DrawSplitIcon(canvas, item.Layout);
   }

   private void SplitIconCanvas_Loaded(object sender, RoutedEventArgs e)
   {
      if (sender is not Canvas canvas) return;
      if (canvas.DataContext is not SplitSelectionItem item) return;

      mIconCanvases.RemoveAll(entry => ReferenceEquals(entry.Item, item));
      mIconCanvases.Add((canvas, item));
      DrawSplitIcon(canvas, item.Layout);
   }

   /// <summary>Draws a mini grid representation of the layout into the canvas.</summary>
   private static void DrawSplitIcon(Canvas canvas, VideoLayoutDefinition layout)
   {
      canvas.Children.Clear();

      const double strokeThickness = 0.75;
      const double padding = 1.0;
      double canvasWidth = canvas.Width;
      double canvasHeight = canvas.Height;
      double drawWidth = canvasWidth - 2 * padding;
      double drawHeight = canvasHeight - 2 * padding;
      double gap = 2;
      int rows = Math.Max(1, layout.Rows);
      int columns = Math.Max(1, layout.Columns);

      double cellWidth = (drawWidth - gap * (columns - 1)) / columns;
      double cellHeight = (drawHeight - gap * (rows - 1)) / rows;

      IBrush fillBrush;
      IBrush strokeBrush;
      if (canvas.TryFindResource("ListItemIconFg", out var iconRes) && iconRes is SolidColorBrush iconBrush)
      {
         var c = iconBrush.Color;
         fillBrush = new SolidColorBrush(Color.FromArgb(80, c.R, c.G, c.B));
         strokeBrush = new SolidColorBrush(Color.FromArgb(220, c.R, c.G, c.B));
      }
      else if (canvas.TryFindResource("Primary", out var primaryRes) && primaryRes is SolidColorBrush primaryBrush)
      {
         var c = primaryBrush.Color;
         fillBrush = new SolidColorBrush(Color.FromArgb(80, c.R, c.G, c.B));
         strokeBrush = new SolidColorBrush(Color.FromArgb(220, c.R, c.G, c.B));
      }
      else
      {
         fillBrush = new SolidColorBrush(Color.FromArgb(80, 0, 123, 193));
         strokeBrush = new SolidColorBrush(Color.FromArgb(220, 0, 123, 193));
      }

      if (layout.Slots == null || layout.Slots.Count == 0)
      {
         // Fallback: draw a single rectangle
         var rect = new Rectangle
         {
            Width = drawWidth,
            Height = drawHeight,
            Fill = fillBrush,
            Stroke = strokeBrush,
            StrokeThickness = strokeThickness,
            RadiusX = 1,
            RadiusY = 1
         };
         Canvas.SetLeft(rect, padding);
         Canvas.SetTop(rect, padding);
         canvas.Children.Add(rect);
         return;
      }

      foreach (VideoSlotDefinition slot in layout.Slots)
      {
         double x = padding + slot.Column * (cellWidth + gap);
         double y = padding + slot.Row * (cellHeight + gap);
         double w = slot.ColumnSpan * cellWidth + (slot.ColumnSpan - 1) * gap;
         double h = slot.RowSpan * cellHeight + (slot.RowSpan - 1) * gap;

         var rect = new Rectangle
         {
            Width = Math.Max(1, w),
            Height = Math.Max(1, h),
            Fill = fillBrush,
            Stroke = strokeBrush,
            StrokeThickness = strokeThickness,
            RadiusX = 1,
            RadiusY = 1
         };
         Canvas.SetLeft(rect, x);
         Canvas.SetTop(rect, y);
         canvas.Children.Add(rect);
      }
   }

   #endregion

   // ── Resize ─────────────────────────────────────────────────────────────

   private void ResizeHandle_PointerPressed(object sender, PointerPressedEventArgs e)
   {
      if (lstSplitSelection == null) return;
      mIsResizing = true;
      mResizeStartPoint = e.GetPosition(this);
      mResizeStartHeight = lstSplitSelection.Height > 0 ? lstSplitSelection.Height : lstSplitSelection.Bounds.Height;
      if (sender is InputElement ie) e.Pointer.Capture(ie);
   }

   private void ResizeHandle_PointerMoved(object sender, PointerEventArgs e)
   {
      if (!mIsResizing || lstSplitSelection == null) return;
      double minH = AppConstants.Default.ResizableSidebarMenuMinHeight;
      double maxH = AppConstants.Default.ResizableSidebarMenuMaxHeight;
      double delta = e.GetPosition(this).Y - mResizeStartPoint.Y;
      lstSplitSelection.Height = Math.Clamp(mResizeStartHeight + delta, minH, maxH);
   }

   private void ResizeHandle_PointerReleased(object sender, PointerReleasedEventArgs e) => EndResize(e.Pointer);
   private void ResizeHandle_PointerCaptureLost(object sender, PointerCaptureLostEventArgs e) => EndResize(null);

   private void EndResize(IPointer pointer)
   {
      if (!mIsResizing) return;
      mIsResizing = false;
      pointer?.Capture(null);
      LayoutChanged?.Invoke(this, EventArgs.Empty);
   }
}
