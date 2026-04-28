using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using Avalonia.Threading;

namespace Vao.Sample
{
   public class App : Application
   {
      private ThemeDefinition _currentTheme;

      public ThemeDefinition CurrentTheme => _currentTheme;

      public event Action<ThemeDefinition> ThemeApplied;

      public override void Initialize()
      {
         AvaloniaXamlLoader.Load(this);
         ApplyTheme(AppSettings.Default.GetPreferredThemeKey(), persistSelection: false);
      }

      public void ApplyTheme(string themeKey, bool persistSelection = true)
      {
         if (Resources == null)
            return;

         var settings = AppSettings.Default;
         var theme = ThemeCatalog.Reload().GetThemeOrDefault(themeKey, false, true);

         _currentTheme = theme;
         RequestedThemeVariant = theme.IsDark ? ThemeVariant.Dark : ThemeVariant.Light;

         if (persistSelection)
            settings.SelectedTheme = theme.Key;

         ApplyColorResources(theme);
         ApplyLayoutResources(theme);
         RefreshAllStyles();
         if (Dispatcher.UIThread.CheckAccess())
            ThemeApplied?.Invoke(theme);
         else
            Dispatcher.UIThread.Post(() => ThemeApplied?.Invoke(theme), DispatcherPriority.Send);
      }

      private static readonly System.Reflection.MethodInfo _invalidateStylesMethod =
         typeof(StyledElement).GetMethod("InvalidateStyles",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

      private void RefreshAllStyles()
      {
         if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
         {
            foreach (var window in desktop.Windows)
               _invalidateStylesMethod?.Invoke(window, [true]);
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
      }

      public ThemeDefinition ToggleThemeBrightness()
      {
         var toggledTheme = ThemeCatalog.Reload().GetOppositeBrightnessTheme(AppSettings.Default.GetPreferredThemeKey());
         ApplyTheme(toggledTheme.Key);
         return toggledTheme;
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

      public ThemeDefinition GetToggleBrightnessTargetTheme()
      {
         return ThemeCatalog.Reload().GetOppositeBrightnessTheme(AppSettings.Default.GetPreferredThemeKey());
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
         if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
         {
            desktop.MainWindow = new MainWindow();
         }
         base.OnFrameworkInitializationCompleted();
      }
   }
}
