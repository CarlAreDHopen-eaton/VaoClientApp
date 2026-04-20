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
      }

      public void SetTheme(bool isDark)
      {
         RequestedThemeVariant = isDark ? ThemeVariant.Dark : ThemeVariant.Light;
         AppSettings.Default.IsDarkMode = isDark;
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
