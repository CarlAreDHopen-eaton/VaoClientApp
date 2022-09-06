using System;
using System.Runtime.InteropServices;

namespace Vao.Sample
{
   public static class DarkModeHelper
   {
      private const int C_DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20_H1 = 19;
      private const int C_DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

      /// <summary>
      /// Sets the value of Desktop Window Manager (DWM) non-client rendering attributes for a window.
      /// </summary>
      /// <param name="handle">The handle to the window for which the attribute value is to be set</param>
      /// <param name="dwAttribute">A flag describing which value to set</param>
      /// <param name="pvAttribute">A pointer to an object containing the attribute value to set</param>
      /// <param name="attrSize">The size, in bytes, of the attribute value being set via the pvAttribute parameter</param>
      /// <returns></returns>
      [DllImport("dwmapi.dll")]
      private static extern int DwmSetWindowAttribute(IntPtr handle, int dwAttribute, ref int pvAttribute, int attrSize);

      /// <summary>
      /// Enables black titlebar.
      /// </summary>
      /// <param name="handle"></param>
      /// <returns></returns>
      public static bool EnableImmersiveDarkMode(IntPtr handle)
      {
         if (IsWindows10OrGreater(17763))
         {
            var attribute = C_DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20_H1;
            if (IsWindows10OrGreater(18985))
            {
               attribute = C_DWMWA_USE_IMMERSIVE_DARK_MODE;
            }

            int useImmersiveDarkMode = 1;
            return DwmSetWindowAttribute(handle, attribute, ref useImmersiveDarkMode, sizeof(int)) == 0;
         }

         return false;
      }

      /// <summary>
      /// Verify if Windows version supports dark mode.
      /// </summary>
      /// <param name="build"></param>
      /// <returns></returns>
      private static bool IsWindows10OrGreater(int build = -1)
      {
         return Environment.OSVersion.Version.Major >= 10 && Environment.OSVersion.Version.Build >= build;
      }


   }
}
