using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Media.Imaging;

namespace Vao.Sample
{
   public static class IconHelper
   {
      private static readonly Dictionary<string, Bitmap> _cache = new();

      public static Bitmap Load(string avaresPath, bool invert = false)
      {
         string key = avaresPath + (invert ? "_inv" : "");
         if (_cache.TryGetValue(key, out var cached))
            return cached;

         var uri = new Uri(avaresPath);
         using var stream = Avalonia.Platform.AssetLoader.Open(uri);
         var original = new Bitmap(stream);

         if (!invert)
         {
            _cache[key] = original;
            return original;
         }

         var inverted = InvertBitmap(original);
         _cache[key] = inverted;
         return inverted;
      }

      private static Bitmap InvertBitmap(Bitmap source)
      {
         int w = source.PixelSize.Width;
         int h = source.PixelSize.Height;
         int stride = w * 4;
         int bufSize = h * stride;
         var pixelBuf = new byte[bufSize];

         var handle = GCHandle.Alloc(pixelBuf, GCHandleType.Pinned);
         try
         {
            source.CopyPixels(new PixelRect(0, 0, w, h), handle.AddrOfPinnedObject(), bufSize, stride);
         }
         finally
         {
            handle.Free();
         }

         for (int i = 0; i < pixelBuf.Length; i += 4)
         {
            pixelBuf[i + 0] = (byte)(255 - pixelBuf[i + 0]); // B
            pixelBuf[i + 1] = (byte)(255 - pixelBuf[i + 1]); // G
            pixelBuf[i + 2] = (byte)(255 - pixelBuf[i + 2]); // R
            // alpha unchanged
         }

         var wb = new WriteableBitmap(source.PixelSize, source.Dpi, Avalonia.Platform.PixelFormat.Bgra8888, Avalonia.Platform.AlphaFormat.Premul);
         using (var fb = wb.Lock())
         {
            Marshal.Copy(pixelBuf, 0, fb.Address, pixelBuf.Length);
         }

         using var ms = new MemoryStream();
         wb.Save(ms);
         ms.Position = 0;
         return new Bitmap(ms);
      }
   }
}
