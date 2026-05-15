using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Vao.Sample.Themes;
using Vao.Sample.Utility;

namespace Vao.Sample;

/// <summary>
/// Main application window that manages theme updates, keyboard shortcuts, and window lifecycle.
/// </summary>
public partial class MainWindow : Window
{
   #region Private Members

   private App mApp;

   #endregion

   #region Constructors

   /// <summary>
   /// Initializes the main window, wires up event handlers, and sets the window title.
   /// </summary>
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

   #endregion

   #region Private Methods

   /// <summary>
   /// Maps a physical digit key to its corresponding hotkey slot number.
   /// </summary>
   /// <param name="key">The physical key to map.</param>
   /// <returns>The slot number (0–9), or -1 if the key is not a digit.</returns>
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

   /// <summary>
   /// Handles the window Closed event by unsubscribing from theme changes.
   /// </summary>
   private void MainWindow_Closed(object sender, EventArgs e)
   {
      if (mApp != null)
      {
         mApp.ThemeApplied -= OnThemeApplied;
         mApp = null;
      }
   }

   /// <summary>
   /// Handles global key-down events for keyboard shortcuts and camera hotkeys.
   /// </summary>
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

      bool isShiftCtrl = (e.KeyModifiers & KeyModifiers.Shift) != 0 &&
                         (e.KeyModifiers & KeyModifiers.Control) != 0;

      if (isShiftCtrl)
      {
         string keySymbol = e.KeySymbol ?? string.Empty;
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
            int slot = HotkeySlotFromPhysicalKey(e.PhysicalKey);
            if (slot >= 0 && mainView.IsStarted)
            {
               mainView.AssignCameraHotkey(slot);
               e.Handled = true;
            }
         }
      }
      else if ((e.KeyModifiers & KeyModifiers.Control) != 0 && (e.KeyModifiers & KeyModifiers.Shift) == 0)
      {
         int slot = HotkeySlotFromPhysicalKey(e.PhysicalKey);
         if (slot >= 0 && mainView.IsStarted)
         {
            Dictionary<int, int> hotkeys = ConfigurationManager.Instance.CameraHotkeys;
            if (hotkeys.TryGetValue(slot, out int cameraNo) && cameraNo != 0)
            {
               mainView.SelectCameraHotkey(cameraNo);
               e.Handled = true;
            }
         }
      }
   }

   /// <summary>
   /// Handles the window Opened event by notifying the main view of the initial width.
   /// </summary>
   private void MainWindow_Opened(object sender, EventArgs e)
   {
      Opened -= MainWindow_Opened;
      mainView.OnWindowOpened(Bounds.Width);
   }

   /// <summary>
   /// Handles the window SizeChanged event by forwarding the new width to the main view.
   /// </summary>
   private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
   {
      mainView.HandleWindowResize(e.NewSize.Width);
   }

   /// <summary>
   /// Handles the OnClosing event by saving settings and stopping the client.
   /// </summary>
   protected override void OnClosing(WindowClosingEventArgs e)
   {
      mainView.SaveCurrentSettings();
      mainView.StopClient();
      base.OnClosing(e);
   }

   /// <summary>
   /// Refreshes the window title when the applied theme changes.
   /// </summary>
   private void OnThemeApplied(ThemeDefinition theme)
   {
      UpdateWindowTitle();
   }

   /// <summary>
   /// Updates the window title to reflect the current version and connection state.
   /// </summary>
   private void UpdateWindowTitle()
   {
      Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
      string versionString = version != null ? $" v{version.Major}.{version.Minor}.{version.Build}" : "";
      string baseTitle = $"HERNIS FLEX VAO API Demo{versionString}";
      Title = mainView.IsStarted ? baseTitle : $"{baseTitle} (Not connected)";
   }

   #endregion
}
