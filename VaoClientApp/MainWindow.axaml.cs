using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Vao.Sample.Controls;
using Vao.Sample.Themes;
using Vao.Sample.Utility;

namespace Vao.Sample
{
   public partial class MainWindow : Window
   {
      private App mApp;

      public MainWindow()
      {
         InitializeComponent();

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
               var hotkeys = ConfigurationManager.Instance.CameraHotkeys;
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
