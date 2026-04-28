using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace Vao.Sample
{
   public class App : Application
   {
      private ThemeDefinition _currentTheme;

      public ThemeDefinition CurrentTheme => _currentTheme;

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

         ApplyColorResources(theme.Colors);
         ApplyLayoutResources(theme.Layout);
      }

      public ThemeDefinition ToggleThemeBrightness()
      {
         var toggledTheme = ThemeCatalog.Reload().GetOppositeBrightnessTheme(AppSettings.Default.GetPreferredThemeKey());
         ApplyTheme(toggledTheme.Key);
         return toggledTheme;
      }

      public ThemeDefinition GetToggleBrightnessTargetTheme()
      {
         return ThemeCatalog.Reload().GetOppositeBrightnessTheme(AppSettings.Default.GetPreferredThemeKey());
      }

      public IReadOnlyList<ThemeOption> GetThemeOptions()
      {
         return ThemeCatalog.Reload().GetThemeOptions();
      }

      private void ApplyLayoutResources(ThemeLayoutProfile profile)
      {
         Resources["AppBarHeight"] = profile.AppBarHeight;
         Resources["SidebarWidth"] = profile.SidebarWidth;
         Resources["ButtonMinHeight"] = profile.ButtonMinHeight;
         Resources["InputMinHeight"] = profile.InputMinHeight;
         Resources["ListItemMinHeight"] = profile.ListItemMinHeight;
         Resources["AvatarSize"] = profile.AvatarSize;
         Resources["AvatarCornerRadius"] = new CornerRadius(profile.AvatarCornerRadius);
         Resources["IconSize"] = profile.IconSize;
         Resources["IconSizeSmall"] = profile.IconSizeSmall;
         Resources["SidebarSectionHeaderFontSize"] = profile.SidebarSectionHeaderFontSize;
         Resources["SectionHeaderFontSize"] = profile.SectionHeaderFontSize;
         Resources["PageHeaderTitleFontSize"] = profile.PageHeaderTitleFontSize;
         Resources["PageHeaderSubtitleFontSize"] = profile.PageHeaderSubtitleFontSize;
         Resources["ButtonPadding"] = profile.GetButtonPadding();
         Resources["InputPadding"] = profile.GetInputPadding();
         Resources["PageMargin"] = profile.GetPageMargin();
         Resources["PageMarginNoBottom"] = profile.GetPageMarginNoBottom();
      }

      private void ApplyColorResources(ThemeColorProfile colors)
      {
         foreach (var entry in colors.ToBrushResources())
            Resources[entry.Key] = entry.Value;
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
