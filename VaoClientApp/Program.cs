using Avalonia;
using System;

namespace Vao.Sample
{
   static class Program
   {
      [STAThread]
      static void Main(string[] args)
      {
         BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
      }

      public static AppBuilder BuildAvaloniaApp()
         => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();
   }
}
