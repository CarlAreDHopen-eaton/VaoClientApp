using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Media;
using Newtonsoft.Json;

namespace Vao.Sample
{
   public sealed class ThemeCatalog
   {
      private static readonly string ThemesDirectoryPath = Path.Combine(AppContext.BaseDirectory, "Themes");

      private static ThemeCatalog _default;
      public static ThemeCatalog Default => _default ??= Load();

      public List<ThemeDefinition> Themes { get; set; } = new();

      public static ThemeCatalog Reload()
      {
         _default = Load();
         return _default;
      }

      public ThemeDefinition GetTheme(string themeKey)
      {
         var normalizedKey = NormalizeKey(themeKey);
         return Themes.FirstOrDefault(theme => string.Equals(theme.Key, normalizedKey, StringComparison.OrdinalIgnoreCase));
      }

      public ThemeDefinition GetThemeOrDefault(string themeKey, bool fallbackIsDark, bool fallbackIsTablet)
      {
         var theme = GetTheme(themeKey);
         if (theme != null)
            return theme;

         return GetTheme("dark-tablet") ?? Themes.First();
      }

      public ThemeDefinition GetOppositeBrightnessTheme(string themeKey)
      {
         var currentTheme = GetTheme(themeKey) ?? Themes.First();
         var targetKey = BuildThemeKey(!currentTheme.IsDark, currentTheme.IsTablet);
         return GetTheme(targetKey) ?? currentTheme;
      }

      public static string BuildThemeKey(bool isDark, bool isTablet)
      {
         return $"{(isDark ? "dark" : "light")}-{(isTablet ? "tablet" : "desktop")}";
      }

      public IReadOnlyList<ThemeOption> GetThemeOptions()
      {
         return Themes
            .OrderBy(theme => theme.DisplayName)
            .Select(theme => new ThemeOption(theme.Key, theme.DisplayName))
            .ToList();
      }

      private static ThemeCatalog Load()
      {
         var catalog = new ThemeCatalog();

         if (Directory.Exists(ThemesDirectoryPath))
         {
            foreach (var filePath in Directory.GetFiles(ThemesDirectoryPath, "*.json", SearchOption.TopDirectoryOnly).OrderBy(path => path))
            {
               try
               {
                  var theme = JsonConvert.DeserializeObject<ThemeDefinition>(File.ReadAllText(filePath));
                  if (theme == null)
                     continue;

                  theme.Key = NormalizeKey(string.IsNullOrWhiteSpace(theme.Key) ? Path.GetFileNameWithoutExtension(filePath) : theme.Key);
                  theme.DisplayName = string.IsNullOrWhiteSpace(theme.DisplayName) ? theme.Key : theme.DisplayName;
                  theme.Layout ??= ThemeLayoutProfile.CreateDefaults(theme.IsDark, theme.IsTablet);
                  theme.Colors ??= ThemeColorProfile.CreateDefaults(theme.IsDark);
                  catalog.Themes.Add(theme);
               }
               catch
               {
               }
            }
         }

         if (catalog.Themes.Count == 0)
         {
            catalog.Themes.Add(ThemeDefinition.CreateDefault("dark-desktop", "Dark Desktop", true, false));
            catalog.Themes.Add(ThemeDefinition.CreateDefault("dark-tablet", "Dark Tablet", true, true));
            catalog.Themes.Add(ThemeDefinition.CreateDefault("light-desktop", "Light Desktop", false, false));
            catalog.Themes.Add(ThemeDefinition.CreateDefault("light-tablet", "Light Tablet", false, true));
         }

         return catalog;
      }

      private static string NormalizeKey(string themeKey)
      {
         return string.IsNullOrWhiteSpace(themeKey) ? string.Empty : themeKey.Trim().ToLowerInvariant();
      }
   }

   public sealed class ThemeDefinition
   {
      public string Key { get; set; } = string.Empty;
      public string DisplayName { get; set; } = string.Empty;
      public string Variant { get; set; } = "Dark";
      public string FormFactor { get; set; } = "Desktop";
      public ThemeLayoutProfile Layout { get; set; } = new();
      public ThemeColorProfile Colors { get; set; } = new();

      [JsonIgnore]
      public bool IsDark => string.Equals(Variant, "Dark", StringComparison.OrdinalIgnoreCase);

      [JsonIgnore]
      public bool IsTablet => string.Equals(FormFactor, "Tablet", StringComparison.OrdinalIgnoreCase);

      public static ThemeDefinition CreateDefault(string key, string displayName, bool isDark, bool isTablet)
      {
         return new ThemeDefinition
         {
            Key = key,
            DisplayName = displayName,
            Variant = isDark ? "Dark" : "Light",
            FormFactor = isTablet ? "Tablet" : "Desktop",
            Layout = ThemeLayoutProfile.CreateDefaults(isDark, isTablet),
            Colors = ThemeColorProfile.CreateDefaults(isDark)
         };
      }
   }

   public sealed class ThemeLayoutProfile
   {
      public double AppBarHeight { get; set; }
      public double SidebarWidth { get; set; }
      public double ButtonMinHeight { get; set; }
      public double InputMinHeight { get; set; }
      public double ListItemMinHeight { get; set; }
      public double AvatarSize { get; set; }
      public double AvatarCornerRadius { get; set; }
      public double IconSize { get; set; }
      public double IconSizeSmall { get; set; }
      public double SidebarSectionHeaderFontSize { get; set; }
      public double SectionHeaderFontSize { get; set; }
      public double PageHeaderTitleFontSize { get; set; }
      public double PageHeaderSubtitleFontSize { get; set; }
      public double ButtonPaddingHorizontal { get; set; }
      public double ButtonPaddingVertical { get; set; }
      public double InputPaddingHorizontal { get; set; }
      public double InputPaddingVertical { get; set; }
      public double PageMarginHorizontal { get; set; }
      public double PageMarginVertical { get; set; }

      public Thickness GetButtonPadding() => new(ButtonPaddingHorizontal, ButtonPaddingVertical);

      public Thickness GetInputPadding() => new(InputPaddingHorizontal, InputPaddingVertical);

      public Thickness GetPageMargin() => new(PageMarginHorizontal, PageMarginVertical);

      public Thickness GetPageMarginNoBottom() => new(PageMarginHorizontal, PageMarginVertical, PageMarginHorizontal, 0d);

      public static ThemeLayoutProfile CreateDefaults(bool isDark, bool isTablet)
      {
         _ = isDark;

         if (isTablet)
         {
            return new ThemeLayoutProfile
            {
               AppBarHeight = 64d,
               SidebarWidth = 280d,
               ButtonMinHeight = 44d,
               InputMinHeight = 44d,
               ListItemMinHeight = 48d,
               AvatarSize = 44d,
               AvatarCornerRadius = 22d,
               IconSize = 24d,
               IconSizeSmall = 20d,
               SidebarSectionHeaderFontSize = 14d,
               SectionHeaderFontSize = 16d,
               PageHeaderTitleFontSize = 20d,
               PageHeaderSubtitleFontSize = 12d,
               ButtonPaddingHorizontal = 16d,
               ButtonPaddingVertical = 8d,
               InputPaddingHorizontal = 12d,
               InputPaddingVertical = 8d,
               PageMarginHorizontal = 24d,
               PageMarginVertical = 24d
            };
         }

         return new ThemeLayoutProfile
         {
            AppBarHeight = 56d,
            SidebarWidth = 248d,
            ButtonMinHeight = 34d,
            InputMinHeight = 34d,
            ListItemMinHeight = 38d,
            AvatarSize = 36d,
            AvatarCornerRadius = 18d,
            IconSize = 20d,
            IconSizeSmall = 16d,
            SidebarSectionHeaderFontSize = 13d,
            SectionHeaderFontSize = 14d,
            PageHeaderTitleFontSize = 17d,
            PageHeaderSubtitleFontSize = 11d,
            ButtonPaddingHorizontal = 12d,
            ButtonPaddingVertical = 6d,
            InputPaddingHorizontal = 10d,
            InputPaddingVertical = 6d,
            PageMarginHorizontal = 20d,
            PageMarginVertical = 20d
         };
      }
   }

   public sealed class ThemeColorProfile
   {
      public string AppBg { get; set; } = string.Empty;
      public string SectionBg { get; set; } = string.Empty;
      public string SidebarBg { get; set; } = string.Empty;
      public string SidebarCardBg { get; set; } = string.Empty;
      public string VideoBg { get; set; } = string.Empty;
      public string VideoHeaderBg { get; set; } = string.Empty;
      public string Surface1 { get; set; } = string.Empty;
      public string Surface2 { get; set; } = string.Empty;
      public string Surface3 { get; set; } = string.Empty;
      public string Divider { get; set; } = string.Empty;
      public string SidebarBorder { get; set; } = string.Empty;
      public string SidebarHeaderFg { get; set; } = string.Empty;
      public string LabelFg { get; set; } = string.Empty;
      public string TextSecondary { get; set; } = string.Empty;
      public string Primary { get; set; } = string.Empty;
      public string PrimaryLight { get; set; } = string.Empty;
      public string PrimaryDark { get; set; } = string.Empty;
      public string Success { get; set; } = string.Empty;
      public string Warning { get; set; } = string.Empty;
      public string Error { get; set; } = string.Empty;
      public string Info { get; set; } = string.Empty;
      public string MessageErrorForeground { get; set; } = string.Empty;
      public string MessageWarningForeground { get; set; } = string.Empty;
      public string MessageDebugForeground { get; set; } = string.Empty;
      public string MessageDefaultForeground { get; set; } = string.Empty;
      public string MessageErrorBackground { get; set; } = string.Empty;
      public string MessageWarningBackground { get; set; } = string.Empty;
      public string MessageDebugBackground { get; set; } = string.Empty;
      public string MessageDefaultBackground { get; set; } = string.Empty;
      public string VideoHeaderNeutral { get; set; } = string.Empty;
      public string VideoHeaderLive { get; set; } = string.Empty;
      public string VideoHeaderPlayback { get; set; } = string.Empty;
      public string UserProfileConnectedBackground { get; set; } = string.Empty;
      public string UserProfileDisconnectedBackground { get; set; } = string.Empty;
      public string CameraLockAlarmForeground { get; set; } = string.Empty;
      public string CameraLockManualForeground { get; set; } = string.Empty;
      public string AlarmStatusActive { get; set; } = string.Empty;
      public string AlarmStatusInactive { get; set; } = string.Empty;
      public string AlarmStatusAcknowledged { get; set; } = string.Empty;
      public string AlarmStatusTampered { get; set; } = string.Empty;
      public string AlarmStatusDefault { get; set; } = string.Empty;

      public IEnumerable<KeyValuePair<string, IBrush>> ToBrushResources()
      {
         yield return CreateBrush(nameof(AppBg), AppBg);
         yield return CreateBrush(nameof(SectionBg), SectionBg);
         yield return CreateBrush(nameof(SidebarBg), SidebarBg);
         yield return CreateBrush(nameof(SidebarCardBg), SidebarCardBg);
         yield return CreateBrush(nameof(VideoBg), VideoBg);
         yield return CreateBrush(nameof(VideoHeaderBg), VideoHeaderBg);
         yield return CreateBrush(nameof(Surface1), Surface1);
         yield return CreateBrush(nameof(Surface2), Surface2);
         yield return CreateBrush(nameof(Surface3), Surface3);
         yield return CreateBrush(nameof(Divider), Divider);
         yield return CreateBrush(nameof(SidebarBorder), SidebarBorder);
         yield return CreateBrush(nameof(SidebarHeaderFg), SidebarHeaderFg);
         yield return CreateBrush(nameof(LabelFg), LabelFg);
         yield return CreateBrush(nameof(TextSecondary), TextSecondary);
         yield return CreateBrush(nameof(Primary), Primary);
         yield return CreateBrush(nameof(PrimaryLight), PrimaryLight);
         yield return CreateBrush(nameof(PrimaryDark), PrimaryDark);
         yield return CreateBrush(nameof(Success), Success);
         yield return CreateBrush(nameof(Warning), Warning);
         yield return CreateBrush(nameof(Error), Error);
         yield return CreateBrush(nameof(Info), Info);
         yield return CreateBrush(nameof(MessageErrorForeground), MessageErrorForeground);
         yield return CreateBrush(nameof(MessageWarningForeground), MessageWarningForeground);
         yield return CreateBrush(nameof(MessageDebugForeground), MessageDebugForeground);
         yield return CreateBrush(nameof(MessageDefaultForeground), MessageDefaultForeground);
         yield return CreateBrush(nameof(MessageErrorBackground), MessageErrorBackground);
         yield return CreateBrush(nameof(MessageWarningBackground), MessageWarningBackground);
         yield return CreateBrush(nameof(MessageDebugBackground), MessageDebugBackground);
         yield return CreateBrush(nameof(MessageDefaultBackground), MessageDefaultBackground);
         yield return CreateBrush(nameof(VideoHeaderNeutral), VideoHeaderNeutral);
         yield return CreateBrush(nameof(VideoHeaderLive), VideoHeaderLive);
         yield return CreateBrush(nameof(VideoHeaderPlayback), VideoHeaderPlayback);
         yield return CreateBrush(nameof(UserProfileConnectedBackground), UserProfileConnectedBackground);
         yield return CreateBrush(nameof(UserProfileDisconnectedBackground), UserProfileDisconnectedBackground);
         yield return CreateBrush(nameof(CameraLockAlarmForeground), CameraLockAlarmForeground);
         yield return CreateBrush(nameof(CameraLockManualForeground), CameraLockManualForeground);
         yield return CreateBrush(nameof(AlarmStatusActive), AlarmStatusActive);
         yield return CreateBrush(nameof(AlarmStatusInactive), AlarmStatusInactive);
         yield return CreateBrush(nameof(AlarmStatusAcknowledged), AlarmStatusAcknowledged);
         yield return CreateBrush(nameof(AlarmStatusTampered), AlarmStatusTampered);
         yield return CreateBrush(nameof(AlarmStatusDefault), AlarmStatusDefault);
      }

      private static KeyValuePair<string, IBrush> CreateBrush(string resourceKey, string colorValue)
      {
         return new KeyValuePair<string, IBrush>(resourceKey, new SolidColorBrush(Color.Parse(colorValue)));
      }

      public static ThemeColorProfile CreateDefaults(bool isDark)
      {
         return isDark
            ? new ThemeColorProfile
            {
               AppBg = "#121212",
               SectionBg = "#1E1E1E",
               SidebarBg = "#121212",
               SidebarCardBg = "#1E1E1E",
               VideoBg = "#000000",
               VideoHeaderBg = "#1E1E1E",
               Surface1 = "#1E1E1E",
               Surface2 = "#2C2C2C",
               Surface3 = "#3D3D3D",
               Divider = "#1FFFFFFF",
               SidebarBorder = "#1FFFFFFF",
               SidebarHeaderFg = "#DEFFFFFF",
               LabelFg = "#99FFFFFF",
               TextSecondary = "#61FFFFFF",
               Primary = "#007BC1",
               PrimaryLight = "#0088D0",
               PrimaryDark = "#006BA1",
               Success = "#39B620",
               Warning = "#F0AA1F",
               Error = "#CA3C3D",
               Info = "#007BC1",
               MessageErrorForeground = "#FFFF0000",
               MessageWarningForeground = "#FFFFA500",
               MessageDebugForeground = "#FFADD8E6",
               MessageDefaultForeground = "#FFFFFFFF",
               MessageErrorBackground = "#00FFFFFF",
               MessageWarningBackground = "#00FFFFFF",
               MessageDebugBackground = "#00FFFFFF",
               MessageDefaultBackground = "#00FFFFFF",
               VideoHeaderNeutral = "#1D3A4A",
               VideoHeaderLive = "#39B620",
               VideoHeaderPlayback = "#CA3C3D",
               UserProfileConnectedBackground = "#006BA1",
               UserProfileDisconnectedBackground = "#FF808080",
               CameraLockAlarmForeground = "#FFCA3C3D",
               CameraLockManualForeground = "#FFF0AA1F",
               AlarmStatusActive = "#FFFF0000",
               AlarmStatusInactive = "#FF808080",
               AlarmStatusAcknowledged = "#FFFFA500",
               AlarmStatusTampered = "#FFFF8C00",
               AlarmStatusDefault = "#78808080"
            }
            : new ThemeColorProfile
            {
               AppBg = "#EEEEEE",
               SectionBg = "#FAFAFA",
               SidebarBg = "#F6F6F6",
               SidebarCardBg = "#F3F3F3",
               VideoBg = "#E0E0E0",
               VideoHeaderBg = "#007BC1",
               Surface1 = "#FAFAFA",
               Surface2 = "#EFEFEF",
               Surface3 = "#E3E3E3",
               Divider = "#1F000000",
               SidebarBorder = "#D4D4D4",
               SidebarHeaderFg = "#DE000000",
               LabelFg = "#99000000",
               TextSecondary = "#61000000",
               Primary = "#007BC1",
               PrimaryLight = "#0088D0",
               PrimaryDark = "#006BA1",
               Success = "#39B620",
               Warning = "#F0AA1F",
               Error = "#CA3C3D",
               Info = "#007BC1",
               MessageErrorForeground = "#FFFF0000",
               MessageWarningForeground = "#FF783F00",
               MessageDebugForeground = "#FF00437A",
               MessageDefaultForeground = "#FF000000",
               MessageErrorBackground = "#FFFFEEEE",
               MessageWarningBackground = "#FFFFF5E6",
               MessageDebugBackground = "#FFECF5FC",
               MessageDefaultBackground = "#00FFFFFF",
               VideoHeaderNeutral = "#007BC1",
               VideoHeaderLive = "#FF2E7D32",
               VideoHeaderPlayback = "#FFB71C1C",
               UserProfileConnectedBackground = "#006BA1",
               UserProfileDisconnectedBackground = "#FF808080",
               CameraLockAlarmForeground = "#FFCA3C3D",
               CameraLockManualForeground = "#FFF0AA1F",
               AlarmStatusActive = "#FFFF0000",
               AlarmStatusInactive = "#FF808080",
               AlarmStatusAcknowledged = "#FFFFA500",
               AlarmStatusTampered = "#FFFF8C00",
               AlarmStatusDefault = "#78808080"
            };
      }
   }

   public sealed record ThemeOption(string Key, string DisplayName)
   {
      public override string ToString() => DisplayName;
   }
}