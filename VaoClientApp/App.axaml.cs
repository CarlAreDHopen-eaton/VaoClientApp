using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;

namespace Vao.Sample
{
   public class App : Application
   {
      private ThemeDefinition mCurrentTheme;
#if ANDROID
      public MainView AndroidMainView { get; private set; }
#endif

      public ThemeDefinition CurrentTheme => mCurrentTheme;

      public event Action<ThemeDefinition> ThemeApplied;

      public override void Initialize()
      {
         AvaloniaXamlLoader.Load(this);
         ApplyTheme(ConfigurationManager.Instance.GetPreferredThemeKey(), persistSelection: false);
      }

      public void ApplyTheme(string themeKey, bool persistSelection = true)
      {
         if (Resources == null)
            return;

         var theme = ThemeCatalog.Reload().GetThemeOrDefault(themeKey, false, true);

         mCurrentTheme = theme;
         RequestedThemeVariant = theme.IsDark ? ThemeVariant.Dark : ThemeVariant.Light;

         if (persistSelection)
            ConfigurationManager.Instance.SelectedTheme = theme.Key;

         ApplyColorResources(theme);
         ApplyLayoutResources(theme);
         RefreshAllStyles();
         if (Dispatcher.UIThread.CheckAccess())
            ThemeApplied?.Invoke(theme);
         else
            Dispatcher.UIThread.Post(() => ThemeApplied?.Invoke(theme), DispatcherPriority.Send);
      }

      private static readonly System.Reflection.MethodInfo mInvalidateStylesMethod =
         typeof(StyledElement).GetMethod("InvalidateStyles",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

      private void RefreshAllStyles()
      {
         if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
         {
            foreach (var window in desktop.Windows)
               mInvalidateStylesMethod?.Invoke(window, [true]);
         }
      }

      private void ApplyLayoutResources(ThemeDefinition theme)
      {
         theme.ApplyLayoutResources(Resources);
      }

      private void ApplyColorResources(ThemeDefinition theme)
      {
         foreach (var entry in theme.ToBrushResources())
            Resources[entry.Key] = entry.Value;

         ApplyFluentAccentResources(theme);
      }

      private void ApplyFluentAccentResources(ThemeDefinition theme)
      {
         // Fluent controls (including CheckBox) consume these accent resource keys.
         // Keeping them in sync with theme accent ensures consistent control coloring.
         var accentDefault = ParseColor(theme.Colors.Accent.Default, Colors.DeepSkyBlue);
         var accentHover = ParseColor(theme.Colors.Accent.Hover, Lighten(accentDefault, 0.12));
         var accentPressed = ParseColor(theme.Colors.Accent.Pressed, Darken(accentDefault, 0.12));

         var light2 = Lighten(accentDefault, 0.25);
         var light3 = Lighten(accentDefault, 0.38);
         var dark2 = Darken(accentDefault, 0.22);
         var dark3 = Darken(accentDefault, 0.34);

         SetAccentResource("SystemAccentColor", accentDefault);
         SetAccentResource("SystemAccentColorLight1", accentHover);
         SetAccentResource("SystemAccentColorLight2", light2);
         SetAccentResource("SystemAccentColorLight3", light3);
         SetAccentResource("SystemAccentColorDark1", accentPressed);
         SetAccentResource("SystemAccentColorDark2", dark2);
         SetAccentResource("SystemAccentColorDark3", dark3);
      }

      private void SetAccentResource(string baseKey, Color color)
      {
         Resources[baseKey] = color;
         Resources[$"{baseKey}Brush"] = new SolidColorBrush(color);
      }

      private static Color ParseColor(string value, Color fallback)
      {
         return Color.TryParse(value, out var parsed) ? parsed : fallback;
      }

      private static Color Lighten(Color color, double amount)
      {
         byte mix(byte channel) => (byte)(channel + ((255 - channel) * amount));
         return Color.FromArgb(color.A, mix(color.R), mix(color.G), mix(color.B));
      }

      private static Color Darken(Color color, double amount)
      {
         byte mix(byte channel) => (byte)(channel * (1.0 - amount));
         return Color.FromArgb(color.A, mix(color.R), mix(color.G), mix(color.B));
      }

      public ThemeDefinition CycleTheme()
      {
         var catalog = ThemeCatalog.Reload();
         var themes = catalog.Themes;

         if (themes == null || themes.Count == 0)
         {
            ApplyTheme("dark-tablet");
            return CurrentTheme;
         }

         var currentThemeKey = AppSettings.Default.GetPreferredThemeKey();
         var currentIndex = themes.FindIndex(theme => string.Equals(theme.Key, currentThemeKey, StringComparison.OrdinalIgnoreCase));
         var nextIndex = currentIndex < 0 ? 0 : (currentIndex + 1) % themes.Count;
         var nextTheme = themes[nextIndex];

         ApplyTheme(nextTheme.Key);
         return nextTheme;
      }

      public ThemeDefinition GetNextThemeInCycle()
      {
         var catalog = ThemeCatalog.Reload();
         var themes = catalog.Themes;

         if (themes == null || themes.Count == 0)
            return CurrentTheme;

         var currentThemeKey = AppSettings.Default.GetPreferredThemeKey();
         var currentIndex = themes.FindIndex(theme => string.Equals(theme.Key, currentThemeKey, StringComparison.OrdinalIgnoreCase));
         var nextIndex = currentIndex < 0 ? 0 : (currentIndex + 1) % themes.Count;
         return themes[nextIndex];
      }

      public IReadOnlyList<ThemeOption> GetThemeOptions()
      {
         return ThemeCatalog.Reload().GetThemeOptions();
      }

      public override void OnFrameworkInitializationCompleted()
      {
#if !ANDROID
         if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
         {
            desktop.MainWindow = new MainWindow();
         }
         else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
         {
            singleViewPlatform.MainView = new MainWindow();
         }
#else
         if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
         {
            var mainView = new MainView();
#if ANDROID
            AndroidMainView = mainView;
#endif
            singleViewPlatform.MainView = mainView;
         }
#endif
         base.OnFrameworkInitializationCompleted();
      }
   }
}
