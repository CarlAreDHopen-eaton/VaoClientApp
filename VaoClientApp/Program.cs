using Avalonia;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
#if !ANDROID
using LibVLCSharp.Shared;
#endif

namespace Vao.Sample
{
   static class Program
   {
#if !ANDROID
      [STAThread]
      static void Main(string[] args)
      {
         EnsureLibVlcEnvironment();
         RegisterLinuxLibVlcResolver();
         Core.Initialize();
         BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
      }
#endif

#if !ANDROID
      private static void RegisterLinuxLibVlcResolver()
      {
         if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
         {
            return;
         }

         try
         {
            NativeLibrary.SetDllImportResolver(typeof(Core).Assembly, ResolveLibVlc);
         }
         catch (InvalidOperationException)
         {
            // Resolver already registered, nothing to do.
         }
      }

      private static IntPtr ResolveLibVlc(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
      {
         if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
         {
            return IntPtr.Zero;
         }

         if (libraryName != "libvlc" && libraryName != "libvlc.so" && libraryName != "liblibvlc")
         {
            return IntPtr.Zero;
         }

         string[] candidates =
         {
            "/usr/lib64/libvlc.so.5",
            "/lib64/libvlc.so.5",
            "/usr/lib/x86_64-linux-gnu/libvlc.so.5",
            "/usr/lib64/libvlc.so",
            "/lib64/libvlc.so",
            "/usr/lib/x86_64-linux-gnu/libvlc.so"
         };

         foreach (var candidate in candidates)
         {
            if (File.Exists(candidate) && NativeLibrary.TryLoad(candidate, out var handle))
            {
               return handle;
            }
         }

         return IntPtr.Zero;
      }

      private static void EnsureLibVlcEnvironment()
      {
         if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
         {
            return;
         }

         var pluginPath = Environment.GetEnvironmentVariable("LIBVLC_PLUGIN_PATH");
         if (!string.IsNullOrWhiteSpace(pluginPath))
         {
            return;
         }

         const string fedoraPluginPath = "/usr/lib64/vlc/plugins";
         if (Directory.Exists(fedoraPluginPath))
         {
            Environment.SetEnvironmentVariable("LIBVLC_PLUGIN_PATH", fedoraPluginPath);
         }
      }
#endif

      public static AppBuilder BuildAvaloniaApp()
      {
         var builder = AppBuilder.Configure<App>();

#if ANDROID
         return builder
            .UseAndroid()
            .LogToTrace();
#else
         return builder
            .UsePlatformDetect()
            .LogToTrace();
#endif
      }
   }
}
