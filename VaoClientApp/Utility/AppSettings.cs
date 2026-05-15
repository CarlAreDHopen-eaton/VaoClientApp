using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace Vao.Sample.Utility
{
   public class ConnectionAlternative
   {
      public string Host { get; set; } = "";
      public string Port { get; set; } = "444";

      [JsonIgnore]
      public string DisplayName
      {
         get { return string.IsNullOrWhiteSpace(Host) ? "(empty host)" : $"{Host}:{Port}"; }
      }
   }

   public class AppSettings
   {
      private static readonly string SettingsPath = Path.Combine(
         Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
         "VaoClientApp", "settings.json");

      private static AppSettings mDefault;
      public static AppSettings Default
      {
         get { return mDefault ??= Load(); }
      }

      public string Host1 { get; set; } = "";
      public string Host2 { get; set; } = "";
      public string ApiPort { get; set; } = "444";
      public string SystemName { get; set; } = "";
      public bool UseHttps { get; set; } = true;
      public string User { get; set; } = "";
      public string Password { get; set; } = "";
      public List<ConnectionAlternative> ConnectionAlternatives { get; set; } = new List<ConnectionAlternative>();
      public int SelectedConnectionIndex { get; set; } = 0;
      public string DownloadPath { get; set; } = "";
      public string FTPUser { get; set; } = "";
      public string FTPPassword { get; set; } = "";
      public bool UseTcp { get; set; } = false;
      public bool AutoConnectOnStartup { get; set; } = false;
      public bool PreferSubChannel { get; set; } = true;
      public string SelectedTheme { get; set; } = "dark-tablet";
      public bool IsSidebarCollapsed { get; set; } = false;

      public bool IsConnectionExpanded { get; set; } = true;
      public bool IsCameraControlExpanded { get; set; } = true;
      public bool IsCameraSelectionExpanded { get; set; } = true;
      public bool IsPresetSelectionExpanded { get; set; } = true;
      public bool IsAlarmsExpanded { get; set; } = true;
      public bool IsPlaybackSelectionExpanded { get; set; } = true;
      public bool IsDownloadRecordingExpanded { get; set; } = true;
      public bool IsSettingsExpanded { get; set; } = false;
      public bool IsMessagesCollapsed { get; set; } = false;
      public double MessagesSplitVideoStars { get; set; } = 3.0;
      public double MessagesSplitMessagesStars { get; set; } = 1.0;
      public double CameraSidebarMenuHeight { get; set; } = 240.0;
      public double AlarmSidebarMenuHeight { get; set; } = 240.0;
      public double PresetSidebarMenuHeight { get; set; } = 180.0;

      /// <summary>Camera hotkey slots 0-9, value is camera component number (0 = unassigned).</summary>
      public Dictionary<int, int> CameraHotkeys { get; set; } = new Dictionary<int, int>();

      public bool ShowActiveAlarms { get; set; } = true;
      public bool ShowTamperedAlarms { get; set; } = true;
      public bool ShowAcknowledgedAlarms { get; set; } = true;
      public bool ShowPassiveAlarms { get; set; } = true;
      public bool ShowDisabledAlarms { get; set; } = true;

      /// <summary>Key of the currently selected video layout (from Layouts/*.json).</summary>
      public string SelectedLayout { get; set; } = "single";

      /// <summary>Last selected camera per video slot index (slot index → camera component number, 0 = none).</summary>
      public Dictionary<int, int> SlotCameras { get; set; } = new Dictionary<int, int>();

      public void Save()
      {
         NormalizeConnections();
         var dir = Path.GetDirectoryName(SettingsPath);
         if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir!);
         File.WriteAllText(SettingsPath, JsonConvert.SerializeObject(this, Formatting.Indented));
      }

      private static AppSettings Load()
      {
         try
         {
            if (File.Exists(SettingsPath))
            {
               var settings = JsonConvert.DeserializeObject<AppSettings>(File.ReadAllText(SettingsPath)) ?? new AppSettings();
               if (string.IsNullOrWhiteSpace(settings.SelectedTheme))
                  settings.SelectedTheme = "dark-tablet";
               settings.NormalizeConnections();
               return settings;
            }
         }
         catch { }
         var defaults = new AppSettings();
         defaults.NormalizeConnections();
         return defaults;
      }

      public string GetPreferredThemeKey()
      {
         return string.IsNullOrWhiteSpace(SelectedTheme) ? "dark-tablet" : SelectedTheme;
      }

      public IReadOnlyList<ConnectionAlternative> GetConnectionAlternatives()
      {
         NormalizeConnections();
         return ConnectionAlternatives;
      }

      public bool HasAnyConnectionAlternative()
      {
         NormalizeConnections();
         return ConnectionAlternatives.Any(c => !string.IsNullOrWhiteSpace(c.Host) && !string.IsNullOrWhiteSpace(c.Port));
      }

      public ConnectionAlternative GetSelectedConnectionAlternative()
      {
         NormalizeConnections();
         if (ConnectionAlternatives.Count == 0)
            return new ConnectionAlternative();

         var selectedIndex = Math.Clamp(SelectedConnectionIndex, 0, ConnectionAlternatives.Count - 1);
         return ConnectionAlternatives[selectedIndex];
      }

      public void SetConnectionAlternatives(IEnumerable<ConnectionAlternative> alternatives, int selectedIndex = 0)
      {
         ConnectionAlternatives = (alternatives ?? Enumerable.Empty<ConnectionAlternative>())
            .Where(c => c != null)
            .Select(c => new ConnectionAlternative
            {
               Host = c.Host?.Trim() ?? "",
               Port = string.IsNullOrWhiteSpace(c.Port) ? "444" : c.Port.Trim()
            })
            .Where(c => !string.IsNullOrWhiteSpace(c.Host))
            .ToList();

         if (ConnectionAlternatives.Count == 0)
         {
            Host1 = "";
            ApiPort = "444";
            SelectedConnectionIndex = 0;
            return;
         }

         SelectedConnectionIndex = Math.Clamp(selectedIndex, 0, ConnectionAlternatives.Count - 1);
         var primary = ConnectionAlternatives[SelectedConnectionIndex];
         Host1 = primary.Host;
         ApiPort = primary.Port;
      }

      public bool PromoteConnectionAlternativeToTop(string host, string port)
      {
         NormalizeConnections();

         var normalizedHost = host?.Trim() ?? "";
         var normalizedPort = string.IsNullOrWhiteSpace(port) ? "444" : port.Trim();
         if (string.IsNullOrWhiteSpace(normalizedHost))
            return false;

         var matchIndex = ConnectionAlternatives.FindIndex(c =>
            string.Equals(c.Host, normalizedHost, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(c.Port, normalizedPort, StringComparison.Ordinal));

         if (matchIndex <= 0)
         {
            if (matchIndex == 0)
            {
               SelectedConnectionIndex = 0;
               Host1 = ConnectionAlternatives[0].Host;
               ApiPort = ConnectionAlternatives[0].Port;
            }
            return false;
         }

         var matched = ConnectionAlternatives[matchIndex];
         ConnectionAlternatives.RemoveAt(matchIndex);
         ConnectionAlternatives.Insert(0, matched);
         SelectedConnectionIndex = 0;
         Host1 = matched.Host;
         ApiPort = matched.Port;
         return true;
      }

      public void NormalizeConnections()
      {
         if (ConnectionAlternatives == null)
            ConnectionAlternatives = new List<ConnectionAlternative>();

         ConnectionAlternatives = ConnectionAlternatives
            .Where(c => c != null)
            .Select(c => new ConnectionAlternative
            {
               Host = c.Host?.Trim() ?? "",
               Port = string.IsNullOrWhiteSpace(c.Port) ? "444" : c.Port.Trim()
            })
            .Where(c => !string.IsNullOrWhiteSpace(c.Host))
            .ToList();

         if (ConnectionAlternatives.Count == 0 && !string.IsNullOrWhiteSpace(Host1))
         {
            ConnectionAlternatives.Add(new ConnectionAlternative
            {
               Host = Host1.Trim(),
               Port = string.IsNullOrWhiteSpace(ApiPort) ? "444" : ApiPort.Trim()
            });
         }

         if (ConnectionAlternatives.Count <= 1 && !string.IsNullOrWhiteSpace(Host2))
         {
            ConnectionAlternatives.Add(new ConnectionAlternative
            {
               Host = Host2.Trim(),
               Port = string.IsNullOrWhiteSpace(ApiPort) ? "444" : ApiPort.Trim()
            });
         }

         if (ConnectionAlternatives.Count == 0)
         {
            Host1 = "";
            ApiPort = "444";
            SelectedConnectionIndex = 0;
            return;
         }

         SelectedConnectionIndex = Math.Clamp(SelectedConnectionIndex, 0, ConnectionAlternatives.Count - 1);
         var selected = ConnectionAlternatives[SelectedConnectionIndex];
         Host1 = selected.Host;
         ApiPort = selected.Port;
      }
   }
}
