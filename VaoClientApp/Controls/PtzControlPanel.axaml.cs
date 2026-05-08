using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Vao.Client.Components;

namespace Vao.Sample.Controls
{
   public partial class PtzControlPanel : UserControl
   {
      private Camera mCurrentCamera;

      /// <summary>Fired when the user clicks the Absolute Position button.</summary>
      public event EventHandler AbsolutePositionRequested;

      /// <summary>Fired when the user clicks the Camera Lock button.</summary>
      public event EventHandler CameraLockRequested;

      public PtzControlPanel()
      {
         InitializeComponent();
         RegisterPtzButtonHandlers();
      }

      // ── Public API ─────────────────────────────────────────────────────────

      public StackPanel ControlGroup => grpCameraControl;

      public void SetCamera(Camera camera)
      {
         if (mCurrentCamera != null)
         {
            mCurrentCamera.PropertyChanged -= Camera_PropertyChanged;
            mCurrentCamera.LockStatusChanged -= Camera_LockStatusChanged;
         }

         mCurrentCamera = camera;

         if (mCurrentCamera != null)
         {
            mCurrentCamera.PropertyChanged += Camera_PropertyChanged;
            mCurrentCamera.LockStatusChanged += Camera_LockStatusChanged;
            lblCurrentCamera.Text = $"Camera : {mCurrentCamera.Name}";
         }
         else
         {
            lblCurrentCamera.Text = "Camera : (No camera selected)";
         }

         UpdateButtonStates();
      }

      public void UpdateButtonStates()
      {
         if (btnPanLeft != null) btnPanLeft.IsEnabled = mCurrentCamera?.HasPanTiltControl ?? false;
         if (btnPanRight != null) btnPanRight.IsEnabled = mCurrentCamera?.HasPanTiltControl ?? false;
         if (btnTiltDown != null) btnTiltDown.IsEnabled = mCurrentCamera?.HasPanTiltControl ?? false;
         if (btnTiltUp != null) btnTiltUp.IsEnabled = mCurrentCamera?.HasPanTiltControl ?? false;
         if (btnZoomIn != null) btnZoomIn.IsEnabled = mCurrentCamera?.HasLensControl ?? false;
         if (btnZoomOut != null) btnZoomOut.IsEnabled = mCurrentCamera?.HasLensControl ?? false;
         if (btnFocusFar != null) btnFocusFar.IsEnabled = mCurrentCamera?.HasLensControl ?? false;
         if (btnFocusNear != null) btnFocusNear.IsEnabled = mCurrentCamera?.HasLensControl ?? false;

         bool hasPanTiltOrLens = (mCurrentCamera?.HasPanTiltControl ?? false) || (mCurrentCamera?.HasLensControl ?? false);
         if (btnAbsolutePosition != null) btnAbsolutePosition.IsEnabled = hasPanTiltOrLens;
      }

      public void ClearLabel()
      {
         if (lblCurrentCamera != null) lblCurrentCamera.Text = "Camera : (No camera selected)";
      }

      // ── Private ────────────────────────────────────────────────────────────

      private void Camera_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
      {
         if (e.PropertyName == nameof(Camera.IsLocked) || e.PropertyName == nameof(Camera.LockOwner) || e.PropertyName == nameof(Camera.CanUnlock))
            return;
         Dispatcher.UIThread.Post(UpdateButtonStates);
      }

      private void Camera_LockStatusChanged(object sender, EventArgs e)
      {
         Dispatcher.UIThread.Post(() =>
         {
            if (mCurrentCamera != null && mCurrentCamera.IsLocked && mCurrentCamera.LockOwner == "Alarm")
            {
               if (iconCameraLock != null) { iconCameraLock.Text = "\uE899"; iconCameraLock.Foreground = GetBrushResource("CameraLockAlarmForeground", "#CA3C3D"); }
               if (btnCameraLock != null) btnCameraLock.IsEnabled = true;
            }
            else if (mCurrentCamera != null && mCurrentCamera.IsLocked)
            {
               if (iconCameraLock != null) { iconCameraLock.Text = "\uE899"; iconCameraLock.Foreground = GetBrushResource("CameraLockManualForeground", "#F0AA1F"); }
               if (btnCameraLock != null) btnCameraLock.IsEnabled = true;
            }
            else if (mCurrentCamera != null && !mCurrentCamera.IsLocked)
            {
               if (iconCameraLock != null)
               {
                  iconCameraLock.Text = "\uE898";
                  if (this.TryFindResource("SidebarHeaderFg", this.ActualThemeVariant, out var brush) && brush is IBrush b)
                     iconCameraLock.Foreground = b;
               }
               if (btnCameraLock != null) btnCameraLock.IsEnabled = true;
            }

            if (mCurrentCamera != null && mCurrentCamera.CanUnlock == false)
               if (btnCameraLock != null) btnCameraLock.IsEnabled = false;
         });
      }

      private void RegisterPtzButtonHandlers()
      {
         var ptzButtons = new[] { btnPanLeft, btnPanRight, btnTiltUp, btnTiltDown, btnZoomIn, btnZoomOut, btnFocusFar, btnFocusNear };
         foreach (var btn in ptzButtons)
         {
            if (btn == null) continue;
            btn.AddHandler(PointerPressedEvent, OnControlCameraPointerPressed, RoutingStrategies.Bubble, handledEventsToo: true);
            btn.AddHandler(PointerReleasedEvent, OnControlCameraPointerReleased, RoutingStrategies.Bubble, handledEventsToo: true);
         }
      }

      private void OnControlCameraPointerPressed(object sender, PointerPressedEventArgs e)
      {
         var camera = mCurrentCamera;
         if (camera == null) return;
         if (sender == btnPanLeft) camera.PanLeft(80);
         else if (sender == btnPanRight) camera.PanRight(80);
         else if (sender == btnTiltUp) camera.TiltUp(80);
         else if (sender == btnTiltDown) camera.TiltDown(80);
         else if (sender == btnZoomIn) camera.ZoomIn(80);
         else if (sender == btnZoomOut) camera.ZoomOut(80);
         else if (sender == btnFocusFar) camera.FocusFar();
         else if (sender == btnFocusNear) camera.FocusNear();
      }

      private void OnControlCameraPointerReleased(object sender, PointerReleasedEventArgs e)
      {
         mCurrentCamera?.PanTiltZoomStop();
      }

      private void btnAbsolutePosition_Click(object sender, RoutedEventArgs e)
      {
         if (mCurrentCamera != null)
            AbsolutePositionRequested?.Invoke(this, EventArgs.Empty);
      }

      private void btnCameraLock_Click(object sender, RoutedEventArgs e)
      {
         if (mCurrentCamera != null)
            CameraLockRequested?.Invoke(this, EventArgs.Empty);
      }

      private IBrush GetBrushResource(string key, string fallback)
      {
         if (this.TryFindResource(key, this.ActualThemeVariant, out var resource) && resource is IBrush b)
            return b;
         return new SolidColorBrush(Color.Parse(fallback));
      }
   }
}
