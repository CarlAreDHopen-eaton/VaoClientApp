using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Avalonia.Controls;

namespace Vao.Sample.Utility
{
   /// <summary>
   /// Manages application configuration with automatic debounced persistence.
   /// Set properties directly; changes are batched and saved after a short delay.
   /// Call <see cref="Flush"/> on application shutdown to ensure pending changes are written immediately.
   /// </summary>
   public sealed class ConfigurationManager
   {
      private const int C_SAVE_DELAY_MS = 1000;
      private const string C_DEFAULT_PORT = "444";
      private const string C_DEFAULT_THEME = "dark-tablet";

      private static ConfigurationManager mInstance;
      public static ConfigurationManager Instance
      {
         get { return mInstance ??= new ConfigurationManager(); }
      }

      private readonly object mLock = new();
      private Timer mDebounceTimer;
      private bool mIsDirty;

      private ConfigurationManager()
      {
      }

      // ── Connection Settings ─────────────────────────────────────────────────

      public string SystemName
      {
         get { return Settings.SystemName; }
         set { Settings.SystemName = value?.Trim() ?? ""; MarkDirty(); }
      }

      public bool UseHttps
      {
         get { return Settings.UseHttps; }
         set { Settings.UseHttps = value; MarkDirty(); }
      }

      public bool UseTcp
      {
         get { return Settings.UseTcp; }
         set { Settings.UseTcp = value; MarkDirty(); }
      }

      public string User
      {
         get { return Settings.User; }
         set { Settings.User = value ?? ""; MarkDirty(); }
      }

      public string Password
      {
         get { return Settings.Password; }
         set { Settings.Password = value ?? ""; MarkDirty(); }
      }

      public bool AutoConnectOnStartup
      {
         get { return Settings.AutoConnectOnStartup; }
         set { Settings.AutoConnectOnStartup = value; MarkDirty(); }
      }

      public int SelectedConnectionIndex
      {
         get { return Settings.SelectedConnectionIndex; }
         set { Settings.SelectedConnectionIndex = value; MarkDirty(); }
      }

      // ── FTP / Download ──────────────────────────────────────────────────────

      public string FTPUser
      {
         get { return Settings.FTPUser; }
         set { Settings.FTPUser = value ?? ""; MarkDirty(); }
      }

      public string FTPPassword
      {
         get { return Settings.FTPPassword; }
         set { Settings.FTPPassword = value ?? ""; MarkDirty(); }
      }

      public string DownloadPath
      {
         get { return Settings.DownloadPath; }
         set { Settings.DownloadPath = value ?? ""; MarkDirty(); }
      }

      public bool IsDownloadPathSet
      {
         get { return !string.IsNullOrEmpty(Settings.DownloadPath); }
      }

      // ── Camera / Video ──────────────────────────────────────────────────────

      public bool PreferSubChannel
      {
         get { return Settings.PreferSubChannel; }
         set { Settings.PreferSubChannel = value; MarkDirty(); }
      }

      public string SelectedLayout
      {
         get { return Settings.SelectedLayout; }
         set { Settings.SelectedLayout = value ?? "single"; MarkDirty(); }
      }

      public Dictionary<int, int> SlotCameras
      {
         get { return Settings.SlotCameras; }
         set { Settings.SlotCameras = value ?? new Dictionary<int, int>(); MarkDirty(); }
      }

      public Dictionary<int, int> CameraHotkeys
      {
         get { return Settings.CameraHotkeys; }
         set { Settings.CameraHotkeys = value ?? new Dictionary<int, int>(); MarkDirty(); }
      }

      public void SetCameraHotkey(int slot, int cameraComponentNumber)
      {
         Settings.CameraHotkeys[slot] = cameraComponentNumber;
         MarkDirty();
      }

      // ── Theme ───────────────────────────────────────────────────────────────

      public string SelectedTheme
      {
         get { return Settings.SelectedTheme; }
         set { Settings.SelectedTheme = value ?? C_DEFAULT_THEME; MarkDirty(); }
      }

      public string GetPreferredThemeKey()
      {
         return string.IsNullOrWhiteSpace(Settings.SelectedTheme) ? C_DEFAULT_THEME : Settings.SelectedTheme;
      }

      // ── Sidebar / Expander State ────────────────────────────────────────────

      public bool IsSidebarCollapsed
      {
         get { return Settings.IsSidebarCollapsed; }
         set { Settings.IsSidebarCollapsed = value; MarkDirty(); }
      }

      public bool IsConnectionExpanded
      {
         get { return Settings.IsConnectionExpanded; }
         set { Settings.IsConnectionExpanded = value; MarkDirty(); }
      }

      public bool IsCameraControlExpanded
      {
         get { return Settings.IsCameraControlExpanded; }
         set { Settings.IsCameraControlExpanded = value; MarkDirty(); }
      }

      public bool IsCameraSelectionExpanded
      {
         get { return Settings.IsCameraSelectionExpanded; }
         set { Settings.IsCameraSelectionExpanded = value; MarkDirty(); }
      }

      public bool IsPresetSelectionExpanded
      {
         get { return Settings.IsPresetSelectionExpanded; }
         set { Settings.IsPresetSelectionExpanded = value; MarkDirty(); }
      }

      public bool IsAlarmsExpanded
      {
         get { return Settings.IsAlarmsExpanded; }
         set { Settings.IsAlarmsExpanded = value; MarkDirty(); }
      }

      public bool IsPlaybackSelectionExpanded
      {
         get { return Settings.IsPlaybackSelectionExpanded; }
         set { Settings.IsPlaybackSelectionExpanded = value; MarkDirty(); }
      }

      public bool IsDownloadRecordingExpanded
      {
         get { return Settings.IsDownloadRecordingExpanded; }
         set { Settings.IsDownloadRecordingExpanded = value; MarkDirty(); }
      }

      public bool IsSettingsExpanded
      {
         get { return Settings.IsSettingsExpanded; }
         set { Settings.IsSettingsExpanded = value; MarkDirty(); }
      }

      public bool IsMessagesCollapsed
      {
         get { return Settings.IsMessagesCollapsed; }
         set { Settings.IsMessagesCollapsed = value; MarkDirty(); }
      }

      public double MessagesSplitVideoStars
      {
         get { return Settings.MessagesSplitVideoStars; }
         set { Settings.MessagesSplitVideoStars = value; MarkDirty(); }
      }

      public double MessagesSplitMessagesStars
      {
         get { return Settings.MessagesSplitMessagesStars; }
         set { Settings.MessagesSplitMessagesStars = value; MarkDirty(); }
      }

      public double CameraSidebarMenuHeight
      {
         get { return Settings.CameraSidebarMenuHeight; }
         set
         {
            if (value > 0)
            {
               Settings.CameraSidebarMenuHeight = value;
               MarkDirty();
            }
         }
      }

      public double AlarmSidebarMenuHeight
      {
         get { return Settings.AlarmSidebarMenuHeight; }
         set
         {
            if (value > 0)
            {
               Settings.AlarmSidebarMenuHeight = value;
               MarkDirty();
            }
         }
      }

      public double PresetSidebarMenuHeight
      {
         get { return Settings.PresetSidebarMenuHeight; }
         set
         {
            if (value > 0)
            {
               Settings.PresetSidebarMenuHeight = value;
               MarkDirty();
            }
         }
      }

      // ── Alarm Filters ───────────────────────────────────────────────────────

      public bool ShowActiveAlarms
      {
         get { return Settings.ShowActiveAlarms; }
         set { Settings.ShowActiveAlarms = value; MarkDirty(); }
      }

      public bool ShowTamperedAlarms
      {
         get { return Settings.ShowTamperedAlarms; }
         set { Settings.ShowTamperedAlarms = value; MarkDirty(); }
      }

      public bool ShowAcknowledgedAlarms
      {
         get { return Settings.ShowAcknowledgedAlarms; }
         set { Settings.ShowAcknowledgedAlarms = value; MarkDirty(); }
      }

      public bool ShowPassiveAlarms
      {
         get { return Settings.ShowPassiveAlarms; }
         set { Settings.ShowPassiveAlarms = value; MarkDirty(); }
      }

      public bool ShowDisabledAlarms
      {
         get { return Settings.ShowDisabledAlarms; }
         set { Settings.ShowDisabledAlarms = value; MarkDirty(); }
      }

      // ── Connection Alternatives ─────────────────────────────────────────────

      public IReadOnlyList<ConnectionAlternative> GetConnectionAlternatives()
      {
         Settings.NormalizeConnections();
         return Settings.ConnectionAlternatives;
      }

      public bool HasAnyConnectionAlternative()
      {
         Settings.NormalizeConnections();
         return Settings.ConnectionAlternatives.Any(c => !string.IsNullOrWhiteSpace(c.Host) && !string.IsNullOrWhiteSpace(c.Port));
      }

      public ConnectionAlternative GetSelectedConnectionAlternative()
      {
         Settings.NormalizeConnections();
         if (Settings.ConnectionAlternatives.Count == 0)
            return new ConnectionAlternative();

         var selectedIndex = Math.Clamp(Settings.SelectedConnectionIndex, 0, Settings.ConnectionAlternatives.Count - 1);
         return Settings.ConnectionAlternatives[selectedIndex];
      }

      public void SetConnectionAlternatives(IEnumerable<ConnectionAlternative> alternatives, int selectedIndex = 0)
      {
         Settings.ConnectionAlternatives = (alternatives ?? Enumerable.Empty<ConnectionAlternative>())
            .Where(c => c != null)
            .Select(c => new ConnectionAlternative
            {
               Host = c.Host?.Trim() ?? "",
               Port = string.IsNullOrWhiteSpace(c.Port) ? C_DEFAULT_PORT : c.Port.Trim()
            })
            .Where(c => !string.IsNullOrWhiteSpace(c.Host))
            .ToList();

         if (Settings.ConnectionAlternatives.Count == 0)
         {
            Settings.Host1 = "";
            Settings.ApiPort = C_DEFAULT_PORT;
            Settings.SelectedConnectionIndex = 0;
            MarkDirty();
            return;
         }

         Settings.SelectedConnectionIndex = Math.Clamp(selectedIndex, 0, Settings.ConnectionAlternatives.Count - 1);
         var primary = Settings.ConnectionAlternatives[Settings.SelectedConnectionIndex];
         Settings.Host1 = primary.Host;
         Settings.ApiPort = primary.Port;
         MarkDirty();
      }

      public bool PromoteConnectionAlternativeToTop(string host, string port)
      {
         Settings.NormalizeConnections();

         var normalizedHost = host?.Trim() ?? "";
         var normalizedPort = string.IsNullOrWhiteSpace(port) ? C_DEFAULT_PORT : port.Trim();
         if (string.IsNullOrWhiteSpace(normalizedHost))
            return false;

         var matchIndex = Settings.ConnectionAlternatives.FindIndex(c =>
            string.Equals(c.Host, normalizedHost, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(c.Port, normalizedPort, StringComparison.Ordinal));

         if (matchIndex <= 0)
         {
            if (matchIndex == 0)
            {
               Settings.SelectedConnectionIndex = 0;
               Settings.Host1 = Settings.ConnectionAlternatives[0].Host;
               Settings.ApiPort = Settings.ConnectionAlternatives[0].Port;
            }
            return false;
         }

         var matched = Settings.ConnectionAlternatives[matchIndex];
         Settings.ConnectionAlternatives.RemoveAt(matchIndex);
         Settings.ConnectionAlternatives.Insert(0, matched);
         Settings.SelectedConnectionIndex = 0;
         Settings.Host1 = matched.Host;
         Settings.ApiPort = matched.Port;
         MarkDirty();
         return true;
      }

      // ── Validation ──────────────────────────────────────────────────────────

      /// <summary>Validates that required connection fields are present. Returns null if valid, or an error message.</summary>
      public string ValidateConnectionSettings()
      {
         if (!HasAnyConnectionAlternative())
            return "Missing host name";
         if (string.IsNullOrWhiteSpace(Settings.Password))
            return "Missing host password";
         if (string.IsNullOrWhiteSpace(Settings.User))
            return "Missing user name";
         return null;
      }

      public bool CanDownload()
      {
         return !string.IsNullOrEmpty(Settings.FTPPassword)
             && !string.IsNullOrEmpty(Settings.FTPUser)
             && IsDownloadPathSet;
      }

      // ── Persistence ─────────────────────────────────────────────────────────

      /// <summary>The underlying settings data for serialization.</summary>
      public AppSettings Settings
      {
         get { return AppSettings.Default; }
      }

      /// <summary>Marks the configuration as changed and schedules a debounced save.</summary>
      public void MarkDirty()
      {
         lock (mLock)
         {
            mIsDirty = true;
            mDebounceTimer?.Dispose();
            mDebounceTimer = new Timer(OnTimerElapsed, null, C_SAVE_DELAY_MS, Timeout.Infinite);
         }
      }

      /// <summary>Immediately persists any pending changes. Call on application shutdown.</summary>
      public void Flush()
      {
         lock (mLock)
         {
            mDebounceTimer?.Dispose();
            mDebounceTimer = null;

            if (mIsDirty)
            {
               mIsDirty = false;
               Settings.Save();
            }
         }
      }

      private void OnTimerElapsed(object state)
      {
         lock (mLock)
         {
            mDebounceTimer?.Dispose();
            mDebounceTimer = null;

            if (mIsDirty)
            {
               mIsDirty = false;
               Settings.Save();
            }
         }
      }

      // ── UI Helpers ──────────────────────────────────────────────────────────

      /// <summary>Reads text from a named TextBox control, returning fallback if not found or empty.</summary>
      public static string GetTextBoxValue(Control parent, string controlName, string fallback = "")
      {
         return parent.FindControl<TextBox>(controlName)?.Text ?? fallback;
      }

      /// <summary>Reads the checked state from a named CheckBox control.</summary>
      public static bool GetCheckBoxValue(Control parent, string controlName, bool fallback = false)
      {
         return parent.FindControl<CheckBox>(controlName)?.IsChecked == true;
      }

      /// <summary>Gets the selected index from a named ListBox control.</summary>
      public static int GetListBoxSelectedIndex(Control parent, string controlName, int fallback = 0)
      {
         var list = parent.FindControl<ListBox>(controlName);
         return list != null && list.SelectedIndex >= 0 ? list.SelectedIndex : fallback;
      }

      /// <summary>Sets text on a named TextBox control.</summary>
      public static void SetTextBoxValue(Control parent, string controlName, string value)
      {
         var control = parent.FindControl<TextBox>(controlName);
         if (control != null) control.Text = value;
      }

      /// <summary>Sets checked state on a named CheckBox control.</summary>
      public static void SetCheckBoxValue(Control parent, string controlName, bool value)
      {
         var control = parent.FindControl<CheckBox>(controlName);
         if (control != null) control.IsChecked = value;
      }
   }
}
