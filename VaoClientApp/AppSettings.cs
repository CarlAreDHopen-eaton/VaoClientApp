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
      public bool PreferSubChannel { get; set; } = true;
      public bool IsDarkMode { get; set; } = true;

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
               return JsonConvert.DeserializeObject<AppSettings>(File.ReadAllText(SettingsPath)) ?? new AppSettings();
         }
         catch { }
         return new AppSettings();
      }
   }
}
