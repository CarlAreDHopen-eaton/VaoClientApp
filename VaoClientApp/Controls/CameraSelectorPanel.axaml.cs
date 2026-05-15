using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Vao.Client.Components;
using Vao.Client.Enum;

namespace Vao.Sample.Controls
{
   public partial class CameraSelectorPanel : UserControl
   {
      private readonly ObservableCollection<CameraSelectionItem> mCameraSelectionItems = new();
      private readonly ObservableCollection<CameraSelectionItem> mFilteredCameraSelectionItems = new();
      private bool mIsUpdatingSelection;
      private Camera mCurrentCamera;

      private bool mIsResizing;
      private double mResizeStartHeight;
      private Point mResizeStartPoint;

      /// <summary>Fired when the user selects a camera. EventArgs contains the component number.</summary>
      public event EventHandler<int> CameraSelected;

      /// <summary>Fired when resize or other layout change occurs so settings can be saved.</summary>
      public event EventHandler LayoutChanged;

      public CameraSelectorPanel()
      {
         InitializeComponent();
         lstCameraSelection.ItemsSource = mFilteredCameraSelectionItems;
      }

      // ── Public API ─────────────────────────────────────────────────────────

      public ListBox CameraList
      {
         get { return lstCameraSelection; }
      }

      public void Fill(List<Camera> cameraList)
      {
         Clear();
         if (cameraList == null) return;
         foreach (var camera in cameraList.OrderBy(c => c.ComponentNumber))
            mCameraSelectionItems.Add(new CameraSelectionItem(camera));
         ApplySearchFilter();
      }

      public void Clear()
      {
         mCameraSelectionItems.Clear();
         mFilteredCameraSelectionItems.Clear();
         mIsUpdatingSelection = true;
         if (lstCameraSelection != null) lstCameraSelection.SelectedItem = null;
         mIsUpdatingSelection = false;
         if (txtCameraSearch != null) txtCameraSearch.Text = string.Empty;
      }

      public void SyncSelection(Camera currentCamera)
      {
         mCurrentCamera = currentCamera;
         mIsUpdatingSelection = true;
         if (lstCameraSelection != null)
         {
            lstCameraSelection.SelectedItem = mCurrentCamera == null
               ? null
               : mFilteredCameraSelectionItems.FirstOrDefault(item => item.Camera?.ComponentNumber == mCurrentCamera.ComponentNumber);
            if (lstCameraSelection.SelectedItem != null)
               lstCameraSelection.ScrollIntoView(lstCameraSelection.SelectedItem);
         }
         mIsUpdatingSelection = false;
      }

      public void RefreshFilter() => ApplySearchFilter();

      public IEnumerable<CameraSelectionItem> Items
      {
         get { return mCameraSelectionItems; }
      }

      public void InitializeHeight(double height, double minHeight, double maxHeight)
      {
         if (lstCameraSelection == null) return;
         lstCameraSelection.MinHeight = minHeight;
         lstCameraSelection.MaxHeight = maxHeight;
         lstCameraSelection.Height = height;
      }

      public double ListHeight
      {
         get { return lstCameraSelection?.Height ?? 0; }
      }

      public void HandleRename(UserPrivilege privilege)
      {
         var selectedItem = lstCameraSelection?.SelectedItem as CameraSelectionItem;
         if (selectedItem?.Camera == null) return;

         var currentUser = selectedItem.Camera.FlexApiClient.CurrentUser;
         if (currentUser == null || currentUser.Privilege < privilege) return;

         var container = lstCameraSelection.ContainerFromItem(selectedItem) as Control;
         if (container == null) return;

         var textBlock = container.GetVisualDescendants().OfType<TextBlock>().FirstOrDefault(tb => tb.Name == "txtCameraItemName");
         var textBox   = container.GetVisualDescendants().OfType<TextBox>().FirstOrDefault(tb => tb.Name == "txtCameraItemEdit");
         if (textBlock == null || textBox == null) return;

         var camera = selectedItem.Camera;
         textBox.Text = camera.Name;
         textBlock.IsVisible = false;
         textBox.IsVisible = true;
         textBox.Focus();
         textBox.SelectAll();

         const string C_ALLOWED_CHARS = "QWERTYUIOPASDFGHJKLZXCVBNMqwertyuiopasdfghjklzxcvbnm1234567890,.()?!-/_ ";

         void CommitEdit()
         {
            string newName = textBox.Text?.Trim();
            textBlock.IsVisible = true;
            textBox.IsVisible = false;
            if (!string.IsNullOrEmpty(newName) && newName != camera.Name)
            {
               bool success = camera.SetName(newName);
               if (success) { camera.UpdateCameraData(); ApplySearchFilter(); }
            }
         }

         void CancelEdit() { textBlock.IsVisible = true; textBox.IsVisible = false; }

         void OnTextInput(object s, TextInputEventArgs args)
         {
            if (args.Text != null && args.Text.Any(c => !C_ALLOWED_CHARS.Contains(c)))
               args.Handled = true;
         }

         void OnKeyDown(object s, KeyEventArgs args)
         {
            if (args.Key == Key.Enter) { CommitEdit(); args.Handled = true; }
            else if (args.Key == Key.Escape) { CancelEdit(); args.Handled = true; }
         }

         void OnLostFocus(object s, RoutedEventArgs args)
         {
            CommitEdit();
            textBox.RemoveHandler(InputElement.TextInputEvent, OnTextInput);
            textBox.KeyDown -= OnKeyDown;
            textBox.LostFocus -= OnLostFocus;
         }

         textBox.AddHandler(InputElement.TextInputEvent, OnTextInput, RoutingStrategies.Tunnel);
         textBox.KeyDown += OnKeyDown;
         textBox.LostFocus += OnLostFocus;
      }

      // ── Event handlers ─────────────────────────────────────────────────────

      private void txtCameraSearch_TextChanged(object sender, TextChangedEventArgs e) => ApplySearchFilter();

      private void lstCameraSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
      {
         if (mIsUpdatingSelection) return;
         if (mDragStartPoint != null) return; // Suppress selection while a drag gesture may be starting
         if (lstCameraSelection?.SelectedItem is not CameraSelectionItem selected || selected.Camera == null) return;
         CameraSelected?.Invoke(this, selected.Camera.ComponentNumber);
      }

      // ── Drag support ──────────────────────────────────────────────────────

      private Point? mDragStartPoint;
      private bool mIsDragging;
      private object mSelectionBeforeDrag;
      private const double C_DRAG_THRESHOLD = 8;

      public void InitializeDragSupport()
      {
         lstCameraSelection.AddHandler(InputElement.PointerPressedEvent, CameraDrag_PointerPressed, RoutingStrategies.Tunnel);
         lstCameraSelection.AddHandler(InputElement.PointerMovedEvent, CameraDrag_PointerMoved, RoutingStrategies.Tunnel);
         lstCameraSelection.AddHandler(InputElement.PointerReleasedEvent, CameraDrag_PointerReleased, RoutingStrategies.Tunnel);
      }

      private void CameraDrag_PointerPressed(object sender, PointerPressedEventArgs e)
      {
         if (e.GetCurrentPoint(lstCameraSelection).Properties.IsLeftButtonPressed)
         {
            mSelectionBeforeDrag = lstCameraSelection.SelectedItem;
            mDragStartPoint = e.GetPosition(lstCameraSelection);
            mIsDragging = false;
         }
      }

      private async void CameraDrag_PointerMoved(object sender, PointerEventArgs e)
      {
         if (mDragStartPoint == null || mIsDragging) return;

         var currentPos = e.GetPosition(lstCameraSelection);
         var delta = currentPos - mDragStartPoint.Value;
         if (Math.Abs(delta.X) < C_DRAG_THRESHOLD && Math.Abs(delta.Y) < C_DRAG_THRESHOLD) return;

         // Find which CameraSelectionItem is under the start point
         var item = GetCameraItemAtPoint(mDragStartPoint.Value);
         if (item?.Camera == null) { mDragStartPoint = null; return; }

         mIsDragging = true;

         // Restore the selection that was active before the pointer press
         mIsUpdatingSelection = true;
         lstCameraSelection.SelectedItem = mSelectionBeforeDrag;
         mIsUpdatingSelection = false;

         var data = new DataObject();
         data.Set("CameraComponentNumber", item.Camera.ComponentNumber);
         await DragDrop.DoDragDrop(e, data, DragDropEffects.Copy);
         mDragStartPoint = null;
         mIsDragging = false;
      }

      private void CameraDrag_PointerReleased(object sender, PointerReleasedEventArgs e)
      {
         if (mDragStartPoint != null && !mIsDragging)
         {
            // No drag occurred — commit the selection that the ListBox made on pointer press
            mDragStartPoint = null;
            if (lstCameraSelection?.SelectedItem is CameraSelectionItem selected && selected.Camera != null)
               CameraSelected?.Invoke(this, selected.Camera.ComponentNumber);
         }
         mDragStartPoint = null;
         mIsDragging = false;
      }

      private CameraSelectionItem GetCameraItemAtPoint(Point point)
      {
         foreach (var item in mFilteredCameraSelectionItems)
         {
            var container = lstCameraSelection.ContainerFromItem(item) as Control;
            if (container == null) continue;
            var bounds = container.Bounds;
            if (bounds.Contains(point))
               return item;
         }
         return null;
      }

      private void ApplySearchFilter()
      {
         string query = txtCameraSearch?.Text?.Trim() ?? string.Empty;
         var filtered = string.IsNullOrWhiteSpace(query)
            ? mCameraSelectionItems
            : mCameraSelectionItems.Where(item => item.Matches(query));

         mFilteredCameraSelectionItems.Clear();
         foreach (var item in filtered)
            mFilteredCameraSelectionItems.Add(item);

         SyncSelection(mCurrentCamera);
      }

      // ── Resize ─────────────────────────────────────────────────────────────

      private void ResizeHandle_PointerPressed(object sender, PointerPressedEventArgs e)
      {
         if (lstCameraSelection == null) return;
         mIsResizing = true;
         mResizeStartPoint = e.GetPosition(this);
         mResizeStartHeight = lstCameraSelection.Height > 0 ? lstCameraSelection.Height : lstCameraSelection.Bounds.Height;
         if (sender is InputElement ie) e.Pointer.Capture(ie);
      }

      private void ResizeHandle_PointerMoved(object sender, PointerEventArgs e)
      {
         if (!mIsResizing || lstCameraSelection == null) return;
         var minH = AppConstants.Default.ResizableSidebarMenuMinHeight;
         var maxH = AppConstants.Default.ResizableSidebarMenuMaxHeight;
         var delta = e.GetPosition(this).Y - mResizeStartPoint.Y;
         lstCameraSelection.Height = Math.Clamp(mResizeStartHeight + delta, minH, maxH);
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
