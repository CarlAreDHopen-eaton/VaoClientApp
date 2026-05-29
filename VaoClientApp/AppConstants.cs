using System;
using System.IO;
using Newtonsoft.Json;

namespace Vao.Sample
{
   public sealed class AppConstants
   {
#if !ANDROID
      private static readonly string ConstantsPath = Path.Combine(AppContext.BaseDirectory, "AppConstants.json");
#endif

      private static AppConstants mDefault;
      public static AppConstants Default
      {
         get { return mDefault ??= Load(); }
      }

      public double ResizableSidebarMenuMinHeight { get; set; } = 32.0;
      public double ResizableSidebarMenuDefaultHeight { get; set; } = 360.0;
      public double ResizableSidebarMenuMaxHeight { get; set; } = 800.0;

      private static AppConstants Load()
      {
         try
         {
#if ANDROID
            var assets = Android.App.Application.Context.Assets;
            if (assets != null)
            {
               using var stream = assets.Open("AppConstants.json");
               using var reader = new StreamReader(stream);
               var constants = JsonConvert.DeserializeObject<AppConstants>(reader.ReadToEnd());
               if (constants != null)
                  return constants;
            }
#else
            if (File.Exists(ConstantsPath))
            {
               var constants = JsonConvert.DeserializeObject<AppConstants>(File.ReadAllText(ConstantsPath));
               if (constants != null)
                  return constants;
            }
#endif
         }
         catch { }
         return new AppConstants();
      }
   }
}
