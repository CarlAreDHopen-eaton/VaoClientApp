using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Newtonsoft.Json;

namespace Vao.Sample.Themes
{
   public sealed class ThemeCatalog
   {
#if !ANDROID
      private static readonly string ThemesDirectoryPath = Path.Combine(AppContext.BaseDirectory, "Themes");
#endif

      private static ThemeCatalog mDefault;
      public static ThemeCatalog Default
      {
         get { return mDefault ??= Load(); }
      }

      public List<ThemeDefinition> Themes { get; set; } = new();

      public static ThemeCatalog Reload()
      {
         mDefault = Load();
         return mDefault;
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

#if ANDROID
         try
         {
            var assets = Android.App.Application.Context.Assets;
            var files = assets?.List("Themes");
            if (files != null)
            {
               foreach (var file in files.Where(f => f.EndsWith(".json", StringComparison.OrdinalIgnoreCase)).OrderBy(f => f))
               {
                  try
                  {
                     using var stream = assets.Open($"Themes/{file}");
                     using var reader = new StreamReader(stream);
                     var theme = JsonConvert.DeserializeObject<ThemeDefinition>(reader.ReadToEnd());
                     if (theme == null)
                        continue;

                     theme.Key = NormalizeKey(string.IsNullOrWhiteSpace(theme.Key) ? Path.GetFileNameWithoutExtension(file) : theme.Key);
                     theme.DisplayName = string.IsNullOrWhiteSpace(theme.DisplayName) ? theme.Key : theme.DisplayName;
                     theme.Colors ??= ThemeColors.CreateDefaults(theme.IsDark);
                     theme.Sizing ??= ThemeSizing.CreateDefaults(theme.IsTablet);
                     theme.Components ??= ThemeComponents.CreateDefaults(theme.IsDark, theme.IsTablet);
                     catalog.Themes.Add(theme);
                  }
                  catch
                  {
                  }
               }
            }
         }
         catch
         {
         }
#else
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
                  theme.Colors ??= ThemeColors.CreateDefaults(theme.IsDark);
                  theme.Sizing ??= ThemeSizing.CreateDefaults(theme.IsTablet);
                  theme.Components ??= ThemeComponents.CreateDefaults(theme.IsDark, theme.IsTablet);
                  catalog.Themes.Add(theme);
               }
               catch
               {
               }
            }
         }
#endif

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
      public string FontFamily { get; set; }
      public ThemeColors Colors { get; set; } = new();
      public ThemeSizing Sizing { get; set; } = new();
      public ThemeComponents Components { get; set; } = new();

      [JsonIgnore]
      public bool IsDark
      {
         get { return string.Equals(Variant, "Dark", StringComparison.OrdinalIgnoreCase); }
      }

      [JsonIgnore]
      public bool IsTablet
      {
         get { return string.Equals(FormFactor, "Tablet", StringComparison.OrdinalIgnoreCase); }
      }

      public static ThemeDefinition CreateDefault(string key, string displayName, bool isDark, bool isTablet)
      {
         return new ThemeDefinition
         {
            Key = key,
            DisplayName = displayName,
            Variant = isDark ? "Dark" : "Light",
            FormFactor = isTablet ? "Tablet" : "Desktop",
            Colors = ThemeColors.CreateDefaults(isDark),
            Sizing = ThemeSizing.CreateDefaults(isTablet),
            Components = ThemeComponents.CreateDefaults(isDark, isTablet)
         };
      }

      /// <summary>
      /// Returns all color values as flat brush resources keyed for DynamicResource binding.
      /// </summary>
      public IEnumerable<KeyValuePair<string, IBrush>> ToBrushResources()
      {
         // Accent
         yield return Brush("Primary", Colors.Accent.Default);
         yield return Brush("PrimaryLight", Colors.Accent.Hover);
         yield return Brush("PrimaryDark", Colors.Accent.Pressed);
         yield return Brush("PrimaryHeaderFg", "#DEFFFFFF"); // Keep app bar content readable on primary background across themes

         // Background
         yield return Brush("AppBg", Colors.Background.App);
         yield return Brush("Surface1", Colors.Background.Raised);
         yield return Brush("Surface2", Colors.Background.Elevated);
         yield return Brush("Surface3", Colors.Background.Overlay);

         // Foreground
         yield return Brush("TextPrimary", Colors.Foreground.Primary);
         yield return Brush("LabelFg", Colors.Foreground.Secondary);
         yield return Brush("TextSecondary", Colors.Foreground.Hint);
         yield return Brush("ListItemIconFg", Colors.Foreground.ListItemIcon ?? Colors.Foreground.Secondary);
         yield return Brush("ListItemNumberFg", Colors.Foreground.ListItemNumber ?? Colors.Foreground.Secondary);

         // Border
         yield return Brush("Divider", Colors.Border.Default);

         // Status
         yield return Brush("Success", Colors.Status.Success);
         yield return Brush("Warning", Colors.Status.Warning);
         yield return Brush("Error", Colors.Status.Error);
         yield return Brush("Info", Colors.Status.Info);

         // Sidebar
         yield return Brush("SidebarBg", Components.Sidebar.Colors.Background);
         yield return Brush("SidebarCardBg", Components.Sidebar.Colors.CardBackground);
         yield return Brush("SidebarBorder", Components.Sidebar.Colors.Border);
         yield return Brush("SidebarHeaderFg", Components.Sidebar.Colors.HeaderForeground);

         // Video
         yield return Brush("VideoBg", Components.VideoPanel.Colors.Background);
         yield return Brush("VideoHeaderBg", Components.VideoPanel.Colors.HeaderBackground);
         yield return Brush("VideoHeaderNeutral", Components.VideoPanel.Colors.HeaderIdle);
         yield return Brush("VideoHeaderLive", Components.VideoPanel.Colors.HeaderLive);
         yield return Brush("VideoHeaderPlayback", Components.VideoPanel.Colors.HeaderPlayback);

         // Messages
         yield return Brush("MessageLogBackground", Components.MessageLog.Background);
         yield return Brush("MessageErrorForeground", Components.MessageLog.ErrorForeground);
         yield return Brush("MessageWarningForeground", Components.MessageLog.WarningForeground);
         yield return Brush("MessageDebugForeground", Components.MessageLog.DebugForeground);
         yield return Brush("MessageDefaultForeground", Components.MessageLog.DefaultForeground);

         // User profile
         yield return Brush("UserProfileConnectedBackground", Components.UserProfile.ConnectedBackground);
         yield return Brush("UserProfileDisconnectedBackground", Components.UserProfile.DisconnectedBackground);

         // Camera lock
         yield return Brush("CameraLockAlarmForeground", Components.CameraLock.AlarmLockedForeground);
         yield return Brush("CameraLockManualForeground", Components.CameraLock.UserLockedForeground);

         // Camera selection
         yield return Brush("CameraSelectedBackground", Components.CameraSelection.SelectedBackground);
         yield return Brush("CameraSelectedHoverBackground", Components.CameraSelection.SelectedHoverBackground);

         // Alarm status
         yield return Brush("AlarmStatusActive", Components.AlarmStatus.Active);
         yield return Brush("AlarmStatusInactive", Components.AlarmStatus.Inactive);
         yield return Brush("AlarmStatusAcknowledged", Components.AlarmStatus.Acknowledged);
         yield return Brush("AlarmStatusTampered", Components.AlarmStatus.Tampered);
         yield return Brush("AlarmStatusDefault", Components.AlarmStatus.Default);
      }

      /// <summary>
      /// Applies all sizing/layout values as Avalonia resources.
      /// </summary>
      public void ApplyLayoutResources(IResourceDictionary resources)
      {
         resources["AppBarHeight"] = Components.AppBar.Height;
         resources["SidebarWidth"] = Components.Sidebar.Width;
         resources["SidebarMenuHeaderFontSize"] = Components.Sidebar.SidebarMenuHeaderFontSize;
         resources["SidebarMenuHeaderMinHeight"] = Components.Sidebar.SidebarMenuHeaderMinHeight;
         resources["SidebarMenuHeaderPadding"] = new Thickness(16, Components.Sidebar.SidebarMenuHeaderPaddingVertical);

         resources["ButtonMinHeight"] = Sizing.Button.MinHeight;
         resources["InputMinHeight"] = Sizing.Input.MinHeight;
         resources["ListItemMinHeight"] = Sizing.ListItem.MinHeight;

         resources["AvatarSize"] = Sizing.Avatar.Size;
         resources["AvatarCornerRadius"] = new CornerRadius(Sizing.Avatar.CornerRadius);
         resources["SidebarMenuItemHeight"] = Components.Sidebar.SidebarMenuItemHeight;
         resources["SidebarMenuSearchHeight"] = Components.Sidebar.SidebarMenuSearchHeight;
         resources["ResizeHandleHeight"] = Components.Sidebar.ResizeHandleHeight;

         resources["IconSize"] = Sizing.Icon.Default;
         resources["IconSizeSmall"] = Sizing.Icon.Small;

         resources["SectionHeaderFontSize"] = Sizing.Typography.SectionHeader;
         resources["PageHeaderTitleFontSize"] = Sizing.Typography.PageTitle;
         resources["PageHeaderSubtitleFontSize"] = Sizing.Typography.PageSubtitle;

         resources["ButtonPadding"] = new Thickness(Sizing.Button.PaddingHorizontal, Sizing.Button.PaddingVertical);
         resources["InputPadding"] = new Thickness(Sizing.Input.PaddingHorizontal, Sizing.Input.PaddingVertical);
         resources["PageMargin"] = new Thickness(Sizing.Page.MarginHorizontal, Sizing.Page.MarginVertical);
         resources["PageMarginNoBottom"] = new Thickness(Sizing.Page.MarginHorizontal, Sizing.Page.MarginVertical, Sizing.Page.MarginHorizontal, 0d);
      }

      private static KeyValuePair<string, IBrush> Brush(string key, string color)
      {
         return new KeyValuePair<string, IBrush>(key, new SolidColorBrush(Color.Parse(color)));
      }
   }

   // ── Colors ───────────────────────────────────────────────────────────

   public sealed class ThemeColors
   {
      public ThemeAccentColors Accent { get; set; } = new();
      public ThemeStatusColors Status { get; set; } = new();
      public ThemeBackgroundColors Background { get; set; } = new();
      public ThemeForegroundColors Foreground { get; set; } = new();
      public ThemeBorderColors Border { get; set; } = new();

      public static ThemeColors CreateDefaults(bool isDark)
      {
         return isDark
            ? new ThemeColors
            {
               Accent = new ThemeAccentColors { Default = "#007BC1", Hover = "#0088D0", Pressed = "#006BA1" },
               Status = new ThemeStatusColors { Success = "#39B620", Warning = "#F0AA1F", Error = "#CA3C3D", Info = "#007BC1" },
               Background = new ThemeBackgroundColors { App = "#121212", Raised = "#1E1E1E", Elevated = "#2C2C2C", Overlay = "#3D3D3D" },
               Foreground = new ThemeForegroundColors { Primary = "#DEFFFFFF", Secondary = "#99FFFFFF", Hint = "#61FFFFFF" },
               Border = new ThemeBorderColors { Default = "#1FFFFFFF", Secondary = "#1FFFFFFF" }
            }
            : new ThemeColors
            {
               Accent = new ThemeAccentColors { Default = "#007BC1", Hover = "#0088D0", Pressed = "#006BA1" },
               Status = new ThemeStatusColors { Success = "#39B620", Warning = "#F0AA1F", Error = "#CA3C3D", Info = "#007BC1" },
               Background = new ThemeBackgroundColors { App = "#EEEEEE", Raised = "#FAFAFA", Elevated = "#EFEFEF", Overlay = "#E3E3E3" },
               Foreground = new ThemeForegroundColors { Primary = "#DE000000", Secondary = "#99000000", Hint = "#61000000" },
               Border = new ThemeBorderColors { Default = "#1F000000", Secondary = "#D4D4D4" }
            };
      }
   }

   public sealed class ThemeAccentColors
   {
      public string Default { get; set; } = "#007BC1";
      public string Hover { get; set; } = "#0088D0";
      public string Pressed { get; set; } = "#006BA1";
   }

   public sealed class ThemeStatusColors
   {
      public string Success { get; set; } = "#39B620";
      public string Warning { get; set; } = "#F0AA1F";
      public string Error { get; set; } = "#CA3C3D";
      public string Info { get; set; } = "#007BC1";
   }

   public sealed class ThemeBackgroundColors
   {
      public string App { get; set; } = "#121212";
      public string Raised { get; set; } = "#1E1E1E";
      public string Elevated { get; set; } = "#2C2C2C";
      public string Overlay { get; set; } = "#3D3D3D";
   }

   public sealed class ThemeForegroundColors
   {
      public string Primary { get; set; } = "#DEFFFFFF";
      public string Secondary { get; set; } = "#99FFFFFF";
      public string Hint { get; set; } = "#61FFFFFF";
      public string ListItemIcon { get; set; }
      public string ListItemNumber { get; set; }
   }

   public sealed class ThemeBorderColors
   {
      public string Default { get; set; } = "#1FFFFFFF";
      public string Secondary { get; set; } = "#1FFFFFFF";
   }

   // ── Sizing ───────────────────────────────────────────────────────────

   public sealed class ThemeSizing
   {
      public ThemePageSizing Page { get; set; } = new();
      public ThemeTypographySizing Typography { get; set; } = new();
      public ThemeIconSizing Icon { get; set; } = new();
      public ThemeButtonSizing Button { get; set; } = new();
      public ThemeInputSizing Input { get; set; } = new();
      public ThemeListItemSizing ListItem { get; set; } = new();
      public ThemeAvatarSizing Avatar { get; set; } = new();

      public static ThemeSizing CreateDefaults(bool isTablet)
      {
         return isTablet
            ? new ThemeSizing
            {
               Page = new ThemePageSizing { MarginHorizontal = 24, MarginVertical = 24 },
               Typography = new ThemeTypographySizing { PageTitle = 20, PageSubtitle = 12, SectionHeader = 16 },
               Icon = new ThemeIconSizing { Default = 24, Small = 20 },
               Button = new ThemeButtonSizing { MinHeight = 44, PaddingHorizontal = 16, PaddingVertical = 8 },
               Input = new ThemeInputSizing { MinHeight = 44, PaddingHorizontal = 12, PaddingVertical = 8 },
               ListItem = new ThemeListItemSizing { MinHeight = 48 },
               Avatar = new ThemeAvatarSizing { Size = 44, CornerRadius = 22 }
            }
            : new ThemeSizing
            {
               Page = new ThemePageSizing { MarginHorizontal = 20, MarginVertical = 20 },
               Typography = new ThemeTypographySizing { PageTitle = 17, PageSubtitle = 11, SectionHeader = 14 },
               Icon = new ThemeIconSizing { Default = 20, Small = 16 },
               Button = new ThemeButtonSizing { MinHeight = 34, PaddingHorizontal = 12, PaddingVertical = 6 },
               Input = new ThemeInputSizing { MinHeight = 34, PaddingHorizontal = 10, PaddingVertical = 6 },
               ListItem = new ThemeListItemSizing { MinHeight = 38 },
               Avatar = new ThemeAvatarSizing { Size = 36, CornerRadius = 18 }
            };
      }
   }

   public sealed class ThemePageSizing
   {
      public double MarginHorizontal { get; set; }
      public double MarginVertical { get; set; }
   }

   public sealed class ThemeTypographySizing
   {
      public double PageTitle { get; set; }
      public double PageSubtitle { get; set; }
      public double SectionHeader { get; set; }
   }

   public sealed class ThemeIconSizing
   {
      public double Default { get; set; }
      public double Small { get; set; }
   }

   public sealed class ThemeButtonSizing
   {
      public double MinHeight { get; set; }
      public double PaddingHorizontal { get; set; }
      public double PaddingVertical { get; set; }
   }

   public sealed class ThemeInputSizing
   {
      public double MinHeight { get; set; }
      public double PaddingHorizontal { get; set; }
      public double PaddingVertical { get; set; }
   }

   public sealed class ThemeListItemSizing
   {
      public double MinHeight { get; set; }
   }

   public sealed class ThemeAvatarSizing
   {
      public double Size { get; set; }
      public double CornerRadius { get; set; }
   }

   // ── Components ───────────────────────────────────────────────────────

   public sealed class ThemeComponents
   {
      public ThemeAppBarComponent AppBar { get; set; } = new();
      public ThemeSidebarComponent Sidebar { get; set; } = new();
      public ThemeVideoPanelComponent VideoPanel { get; set; } = new();
      public ThemeMessageLogComponent MessageLog { get; set; } = new();
      public ThemeUserProfileComponent UserProfile { get; set; } = new();
      public ThemeCameraLockComponent CameraLock { get; set; } = new();
      public ThemeCameraSelectionComponent CameraSelection { get; set; } = new();
      public ThemeAlarmStatusComponent AlarmStatus { get; set; } = new();

      public static ThemeComponents CreateDefaults(bool isDark, bool isTablet)
      {
         return new ThemeComponents
         {
            AppBar = new ThemeAppBarComponent { Height = isTablet ? 64 : 56 },
            Sidebar = new ThemeSidebarComponent
            {
               Width = isTablet ? 280 : 248,
               SidebarMenuHeaderFontSize = isTablet ? 14 : 13,
               SidebarMenuHeaderMinHeight = isTablet ? 52 : 48,
               SidebarMenuHeaderPaddingVertical = isTablet ? 14 : 12,
               SidebarMenuItemHeight = isTablet ? 44 : 28,
                  SidebarMenuSearchHeight = isTablet ? 44 : 28,
               Colors = isDark
                  ? new ThemeSidebarColors { Background = "#121212", CardBackground = "#1E1E1E", Border = "#1FFFFFFF", HeaderForeground = "#DEFFFFFF" }
                  : new ThemeSidebarColors { Background = "#F6F6F6", CardBackground = "#F3F3F3", Border = "#D4D4D4", HeaderForeground = "#DE000000" }
            },
            VideoPanel = new ThemeVideoPanelComponent
            {
               Colors = isDark
                  ? new ThemeVideoPanelColors { Background = "#000000", HeaderBackground = "#1E1E1E", HeaderIdle = "#FF1D3A4A", HeaderLive = "#FF39B620", HeaderPlayback = "#FFCA3C3D" }
                  : new ThemeVideoPanelColors { Background = "#E0E0E0", HeaderBackground = "#007BC1", HeaderIdle = "#FF007BC1", HeaderLive = "#FF2E7D32", HeaderPlayback = "#FFB71C1C" }
            },
            MessageLog = isDark
               ? new ThemeMessageLogComponent
               {
                  Background = "#00000000",
                  ErrorForeground = "#FFFF0000",
                  WarningForeground = "#FFFFA500",
                  DebugForeground = "#FFADD8E6",
                  DefaultForeground = "#FFFFFFFF"
               }
               : new ThemeMessageLogComponent
               {
                  Background = "#00000000",
                  ErrorForeground = "#FFFF0000",
                  WarningForeground = "#FF783F00",
                  DebugForeground = "#FF00437A",
                  DefaultForeground = "#FF000000"
               },
            UserProfile = new ThemeUserProfileComponent { ConnectedBackground = "#FF006BA1", DisconnectedBackground = "#FF808080" },
            CameraLock = new ThemeCameraLockComponent { AlarmLockedForeground = "#FFCA3C3D", UserLockedForeground = "#FFF0AA1F" },
            CameraSelection = new ThemeCameraSelectionComponent
            {
               SelectedBackground = isDark ? "#FFDAA520" : "#FFDAA520",
               SelectedHoverBackground = isDark ? "#FFB8860B" : "#FFB8860B"
            },
            AlarmStatus = new ThemeAlarmStatusComponent { Active = "#FFFF0000", Inactive = "#FF808080", Acknowledged = "#FFFFA500", Tampered = "#FFFF8C00", Default = "#78808080" }
         };
      }
   }

   public sealed class ThemeAppBarComponent
   {
      public double Height { get; set; }
   }

   public sealed class ThemeSidebarComponent
   {
      public double Width { get; set; }
      public double SidebarMenuHeaderFontSize { get; set; }
      public double SidebarMenuHeaderMinHeight { get; set; }
      public double SidebarMenuHeaderPaddingVertical { get; set; }
      public double SidebarMenuItemHeight { get; set; } = 34;
      public double SidebarMenuSearchHeight { get; set; } = 30;
      public double ResizeHandleHeight { get; set; } = 8;
      public ThemeSidebarColors Colors { get; set; } = new();
   }

   public sealed class ThemeSidebarColors
   {
      public string Background { get; set; } = string.Empty;
      public string CardBackground { get; set; } = string.Empty;
      public string Border { get; set; } = string.Empty;
      public string HeaderForeground { get; set; } = string.Empty;
   }

   public sealed class ThemeVideoPanelComponent
   {
      public ThemeVideoPanelColors Colors { get; set; } = new();
   }

   public sealed class ThemeVideoPanelColors
   {
      public string Background { get; set; } = string.Empty;
      public string HeaderBackground { get; set; } = string.Empty;
      public string HeaderIdle { get; set; } = string.Empty;
      public string HeaderLive { get; set; } = string.Empty;
      public string HeaderPlayback { get; set; } = string.Empty;
   }

   public sealed class ThemeMessageLogComponent
   {
      public string Background { get; set; } = "#00000000";
      public string ErrorForeground { get; set; } = "#FFFF0000";
      public string WarningForeground { get; set; } = "#FFFFA500";
      public string DebugForeground { get; set; } = "#FFADD8E6";
      public string DefaultForeground { get; set; } = "#FFFFFFFF";
   }

   public sealed class ThemeUserProfileComponent
   {
      public string ConnectedBackground { get; set; } = string.Empty;
      public string DisconnectedBackground { get; set; } = string.Empty;
   }

   public sealed class ThemeCameraLockComponent
   {
      public string AlarmLockedForeground { get; set; } = string.Empty;
      public string UserLockedForeground { get; set; } = string.Empty;
   }

   public sealed class ThemeCameraSelectionComponent
   {
      public string SelectedBackground { get; set; } = "#FFDAA520";
      public string SelectedHoverBackground { get; set; } = "#FFB8860B";
   }

   public sealed class ThemeAlarmStatusComponent
   {
      public string Active { get; set; } = string.Empty;
      public string Inactive { get; set; } = string.Empty;
      public string Acknowledged { get; set; } = string.Empty;
      public string Tampered { get; set; } = string.Empty;
      public string Default { get; set; } = string.Empty;
   }

   public sealed record ThemeOption(string Key, string DisplayName)
   {
      public override string ToString() => DisplayName;
   }
}