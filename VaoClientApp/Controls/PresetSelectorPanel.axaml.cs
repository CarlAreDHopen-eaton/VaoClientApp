using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Vao.Client.Components;
using Vao.Sample;

namespace Vao.Sample.Controls
{
   public partial class PresetSelectorPanel : UserControl
   {
      private readonly ObservableCollection<PresetSelectionItem> mPresetSelectionItems = new();
      private readonly ObservableCollection<PresetSelectionItem> mFilteredPresetSelectionItems = new();
      private bool mIsUpdatingSelection;
      private Preset mCurrentPreset;

      private bool mIsResizing;
      private double mResizeStartHeight;
      private Point mResizeStartPoint;

      /// <summary>Fired when the user selects a preset. EventArgs contains the selected Preset.</summary>
      public event EventHandler<Preset> PresetSelected;

      /// <summary>Fired when the user double-clicks a preset to go to it.</summary>
      public event EventHandler<Preset> PresetActivated;

      /// <summary>Fired when resize or other layout change occurs so settings can be saved.</summary>
      public event EventHandler LayoutChanged;

      public PresetSelectorPanel()
      {
         InitializeComponent();
         lstPresetSelection.ItemsSource = mFilteredPresetSelectionItems;
      }

      // ── Public API ─────────────────────────────────────────────────────────

      public void Fill(List<Preset> presetList)
      {
         Clear();
         if (presetList == null) return;
         foreach (var preset in presetList.OrderBy(p => p.ComponentNumber))
            mPresetSelectionItems.Add(new PresetSelectionItem(preset));
         ApplySearchFilter();
      }

      public void Clear()
      {
         mPresetSelectionItems.Clear();
         mFilteredPresetSelectionItems.Clear();
         mIsUpdatingSelection = true;
         if (lstPresetSelection != null) lstPresetSelection.SelectedItem = null;
         mIsUpdatingSelection = false;
         if (txtPresetSearch != null) txtPresetSearch.Text = string.Empty;
      }

      public void SyncSelection(Preset currentPreset)
      {
         mCurrentPreset = currentPreset;
         mIsUpdatingSelection = true;
         if (lstPresetSelection != null)
         {
            lstPresetSelection.SelectedItem = mCurrentPreset == null
               ? null
               : mFilteredPresetSelectionItems.FirstOrDefault(item => item.Preset?.ComponentNumber == mCurrentPreset.ComponentNumber);
            if (lstPresetSelection.SelectedItem != null)
               lstPresetSelection.ScrollIntoView(lstPresetSelection.SelectedItem);
         }
         mIsUpdatingSelection = false;
      }

      public void InitializeHeight(double height, double minHeight, double maxHeight)
      {
         if (lstPresetSelection == null) return;
         lstPresetSelection.MinHeight = minHeight;
         lstPresetSelection.MaxHeight = maxHeight;
         lstPresetSelection.Height = height;
      }

      public double ListHeight
      {
         get { return lstPresetSelection?.Height ?? 0; }
      }

      // ── Event handlers ─────────────────────────────────────────────────────

      private void txtPresetSearch_TextChanged(object sender, TextChangedEventArgs e) => ApplySearchFilter();

      private void lstPresetSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
      {
         if (mIsUpdatingSelection) return;
         if (lstPresetSelection?.SelectedItem is not PresetSelectionItem selected || selected.Preset == null) return;
         PresetSelected?.Invoke(this, selected.Preset);
      }

      private void lstPresetSelection_DoubleTapped(object sender, TappedEventArgs e)
      {
         if (lstPresetSelection?.SelectedItem is not PresetSelectionItem selected || selected.Preset == null) return;
         PresetActivated?.Invoke(this, selected.Preset);
      }

      // ── Filter ─────────────────────────────────────────────────────────────

      private void ApplySearchFilter()
      {
         string query = txtPresetSearch?.Text?.Trim() ?? string.Empty;
         var filtered = string.IsNullOrWhiteSpace(query)
            ? mPresetSelectionItems
            : mPresetSelectionItems.Where(item => item.Matches(query));

         mFilteredPresetSelectionItems.Clear();
         foreach (var item in filtered)
            mFilteredPresetSelectionItems.Add(item);

         SyncSelection(mCurrentPreset);
      }

      // ── Resize ─────────────────────────────────────────────────────────────

      private void ResizeHandle_PointerPressed(object sender, PointerPressedEventArgs e)
      {
         if (lstPresetSelection == null) return;
         mIsResizing = true;
         mResizeStartPoint = e.GetPosition(this);
         mResizeStartHeight = lstPresetSelection.Height > 0 ? lstPresetSelection.Height : lstPresetSelection.Bounds.Height;
         if (sender is InputElement ie) e.Pointer.Capture(ie);
      }

      private void ResizeHandle_PointerMoved(object sender, PointerEventArgs e)
      {
         if (!mIsResizing || lstPresetSelection == null) return;
         var minH = AppConstants.Default.ResizableSidebarMenuMinHeight;
         var maxH = AppConstants.Default.ResizableSidebarMenuMaxHeight;
         var delta = e.GetPosition(this).Y - mResizeStartPoint.Y;
         lstPresetSelection.Height = Math.Clamp(mResizeStartHeight + delta, minH, maxH);
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
}
