using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace Vao.Sample
{
   public class App : Application
   {
      public override void Initialize()
      {
         AvaloniaXamlLoader.Load(this);
         RequestedThemeVariant = Avalonia.Styling.ThemeVariant.Dark;
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
