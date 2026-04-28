using System;
using System.IO;
using Newtonsoft.Json;

namespace Vao.Sample
{
   public class AppSettings
   {
      private static readonly string SettingsPath = Path.Combine(
         Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
         "VaoClientApp", "settings.json");

      private static AppSettings _default;
      public static AppSettings Default => _default ??= Load();

      public string Host1 { get; set; } = "";
      public string Host2 { get; set; } = "";
      public string ApiPort { get; set; } = "444";
      public bool UseHttps { get; set; } = true;
      public string User { get; set; } = "";
      public string Password { get; set; } = "";
      public string DownloadPath { get; set; } = "";
      public string FTPUser { get; set; } = "";
      public string FTPPassword { get; set; } = "";
      public int CurrentCamera { get; set; } = 0;
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

      public void Save()
      {
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
               return settings;
            }
         }
         catch { }
         return new AppSettings();
      }

      public string GetPreferredThemeKey()
      {
         return string.IsNullOrWhiteSpace(SelectedTheme) ? "dark-tablet" : SelectedTheme;
      }
   }
}
