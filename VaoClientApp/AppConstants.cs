using System;
using System.IO;
using Newtonsoft.Json;

namespace Vao.Sample
{
   public sealed class AppConstants
   {
      private static readonly string ConstantsPath = Path.Combine(AppContext.BaseDirectory, "AppConstants.json");

      private static AppConstants mDefault;
      public static AppConstants Default => mDefault ??= Load();

      public double ResizableSidebarMenuMinHeight { get; set; } = 180.0;
      public double ResizableSidebarMenuDefaultHeight { get; set; } = 360.0;
      public double ResizableSidebarMenuMaxHeight { get; set; } = 800.0;

      private static AppConstants Load()
      {
         try
         {
            if (File.Exists(ConstantsPath))
            {
               var constants = JsonConvert.DeserializeObject<AppConstants>(File.ReadAllText(ConstantsPath));
               if (constants != null)
                  return constants;
            }
         }
         catch { }
         return new AppConstants();
      }
   }
}
