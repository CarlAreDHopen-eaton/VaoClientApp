using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace Vao.Sample
{
   public class App : Application
   {
      public override void Initialize()
      {
         AvaloniaXamlLoader.Load(this);
         var isDark = AppSettings.Default.IsDarkMode;
         RequestedThemeVariant = isDark ? ThemeVariant.Dark : ThemeVariant.Light;
         SetUiMode(AppSettings.Default.IsTabletMode);
      }

      public void SetTheme(bool isDark)
      {
         RequestedThemeVariant = isDark ? ThemeVariant.Dark : ThemeVariant.Light;
         AppSettings.Default.IsDarkMode = isDark;
      }

      public void SetUiMode(bool isTabletMode)
      {
         AppSettings.Default.IsTabletMode = isTabletMode;

         if (Resources == null)
            return;

         Resources["ButtonMinHeight"] = isTabletMode ? 44d : 34d;
         Resources["InputMinHeight"] = isTabletMode ? 44d : 34d;
         Resources["ListItemMinHeight"] = isTabletMode ? 48d : 38d;
         Resources["AvatarSize"] = isTabletMode ? 44d : 36d;
         Resources["AvatarCornerRadius"] = isTabletMode ? new CornerRadius(22d) : new CornerRadius(18d);
         Resources["IconSize"] = isTabletMode ? 24d : 20d;
         Resources["IconSizeSmall"] = isTabletMode ? 20d : 16d;
         Resources["SidebarSectionHeaderFontSize"] = isTabletMode ? 14d : 13d;
         Resources["SectionHeaderFontSize"] = isTabletMode ? 16d : 14d;
         Resources["PageHeaderTitleFontSize"] = isTabletMode ? 20d : 17d;
         Resources["PageHeaderSubtitleFontSize"] = isTabletMode ? 12d : 11d;
         Resources["ButtonPadding"] = isTabletMode ? new Thickness(16d, 8d) : new Thickness(12d, 6d);
         Resources["InputPadding"] = isTabletMode ? new Thickness(12d, 8d) : new Thickness(10d, 6d);
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
