using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.Media;
using Vao.Sample.Layouts;

namespace Vao.Sample.Controls;

/// <summary>Sidebar panel that displays available video split layouts with generated icons.</summary>
public partial class SplitSelectorPanel : UserControl
{
   #region Private Members

   private readonly ObservableCollection<SplitSelectionItem> mSplitItems = new();
   private bool mIsUpdatingSelection;

   #endregion

   #region Public Events

   /// <summary>Fired when the user selects a layout. EventArgs contains the layout key.</summary>
   public event EventHandler<string> LayoutSelected;

   #endregion

   #region Constructors

   public SplitSelectorPanel()
   {
      InitializeComponent();
      lstSplitSelection.ItemsSource = mSplitItems;
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

   private void SplitIconCanvas_Loaded(object sender, RoutedEventArgs e)
   {
      if (sender is not Canvas canvas) return;

      // Walk up to find the DataContext (SplitSelectionItem)
      if (canvas.DataContext is not SplitSelectionItem item) return;

      DrawSplitIcon(canvas, item.Layout);
   }

   /// <summary>Draws a mini grid representation of the layout into the canvas.</summary>
   private static void DrawSplitIcon(Canvas canvas, VideoLayoutDefinition layout)
   {
      canvas.Children.Clear();

      double canvasWidth = 22;
      double canvasHeight = 22;
      double gap = 1;
      int rows = Math.Max(1, layout.Rows);
      int columns = Math.Max(1, layout.Columns);

      double cellWidth = (canvasWidth - gap * (columns - 1)) / columns;
      double cellHeight = (canvasHeight - gap * (rows - 1)) / rows;

      IBrush fillBrush = new SolidColorBrush(Color.FromArgb(180, 100, 149, 237)); // cornflower blue
      IBrush strokeBrush = new SolidColorBrush(Color.FromArgb(220, 70, 130, 180));

      if (layout.Slots == null || layout.Slots.Count == 0)
      {
         // Fallback: draw a single rectangle
         var rect = new Rectangle
         {
            Width = canvasWidth - 2,
            Height = canvasHeight - 2,
            Fill = fillBrush,
            Stroke = strokeBrush,
            StrokeThickness = 0.5,
            RadiusX = 1,
            RadiusY = 1
         };
         Canvas.SetLeft(rect, 1);
         Canvas.SetTop(rect, 1);
         canvas.Children.Add(rect);
         return;
      }

      foreach (VideoSlotDefinition slot in layout.Slots)
      {
         double x = slot.Column * (cellWidth + gap);
         double y = slot.Row * (cellHeight + gap);
         double w = slot.ColumnSpan * cellWidth + (slot.ColumnSpan - 1) * gap;
         double h = slot.RowSpan * cellHeight + (slot.RowSpan - 1) * gap;

         var rect = new Rectangle
         {
            Width = Math.Max(1, w),
            Height = Math.Max(1, h),
            Fill = fillBrush,
            Stroke = strokeBrush,
            StrokeThickness = 0.5,
            RadiusX = 1,
            RadiusY = 1
         };
         Canvas.SetLeft(rect, x);
         Canvas.SetTop(rect, y);
         canvas.Children.Add(rect);
      }
   }

   #endregion
}
