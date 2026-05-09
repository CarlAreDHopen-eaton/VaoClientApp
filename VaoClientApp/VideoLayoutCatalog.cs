using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace Vao.Sample
{
   public sealed class VideoLayoutCatalog
   {
#if !ANDROID
      private static readonly string LayoutsDirectoryPath = Path.Combine(AppContext.BaseDirectory, "Layouts");
#endif

      private static VideoLayoutCatalog mDefault;
      public static VideoLayoutCatalog Default => mDefault ??= Load();

      public List<VideoLayoutDefinition> Layouts { get; set; } = new();

      public static VideoLayoutCatalog Reload()
      {
         mDefault = Load();
         return mDefault;
      }

      public VideoLayoutDefinition GetLayout(string layoutKey)
      {
         if (string.IsNullOrWhiteSpace(layoutKey))
            return GetDefaultLayout();

         var normalized = layoutKey.Trim().ToLowerInvariant();
         return Layouts.FirstOrDefault(l => string.Equals(l.Key, normalized, StringComparison.OrdinalIgnoreCase))
                ?? GetDefaultLayout();
      }

      public VideoLayoutDefinition GetDefaultLayout()
      {
         return Layouts.FirstOrDefault(l => l.IsSingleView)
                ?? Layouts.FirstOrDefault()
                ?? CreateFallbackSingle();
      }

      public IReadOnlyList<VideoLayoutDefinition> GetAllLayouts() => Layouts;

      private static VideoLayoutCatalog Load()
      {
         var catalog = new VideoLayoutCatalog();

#if ANDROID
         try
         {
            var assets = Android.App.Application.Context.Assets;
            var files = assets?.List("Layouts");
            if (files != null)
            {
               foreach (var file in files.Where(f => f.EndsWith(".json", StringComparison.OrdinalIgnoreCase)).OrderBy(f => f))
               {
                  try
                  {
                     using var stream = assets.Open($"Layouts/{file}");
                     using var reader = new System.IO.StreamReader(stream);
                     var layout = JsonConvert.DeserializeObject<VideoLayoutDefinition>(reader.ReadToEnd());
                     if (layout == null || layout.Slots == null || layout.Slots.Count == 0)
                        continue;

                     layout.Key = string.IsNullOrWhiteSpace(layout.Key)
                        ? Path.GetFileNameWithoutExtension(file).ToLowerInvariant()
                        : layout.Key.Trim().ToLowerInvariant();
                     layout.DisplayName = string.IsNullOrWhiteSpace(layout.DisplayName) ? layout.Key : layout.DisplayName;

                     if (layout.Rows < 1) layout.Rows = 1;
                     if (layout.Columns < 1) layout.Columns = 1;

                     catalog.Layouts.Add(layout);
                  }
                  catch
                  {
                  }
               }
            }
         }
         catch
         {
         }
#else
         if (Directory.Exists(LayoutsDirectoryPath))
         {
            foreach (var filePath in Directory.GetFiles(LayoutsDirectoryPath, "*.json", SearchOption.TopDirectoryOnly).OrderBy(path => path))
            {
               try
               {
                  var layout = JsonConvert.DeserializeObject<VideoLayoutDefinition>(File.ReadAllText(filePath));
                  if (layout == null || layout.Slots == null || layout.Slots.Count == 0)
                     continue;

                  layout.Key = string.IsNullOrWhiteSpace(layout.Key)
                     ? Path.GetFileNameWithoutExtension(filePath).ToLowerInvariant()
                     : layout.Key.Trim().ToLowerInvariant();
                  layout.DisplayName = string.IsNullOrWhiteSpace(layout.DisplayName) ? layout.Key : layout.DisplayName;

                  if (layout.Rows < 1) layout.Rows = 1;
                  if (layout.Columns < 1) layout.Columns = 1;

                  catalog.Layouts.Add(layout);
               }
               catch
               {
               }
            }
         }
#endif

         if (catalog.Layouts.Count == 0)
            catalog.Layouts.Add(CreateFallbackSingle());

         return catalog;
      }

      private static VideoLayoutDefinition CreateFallbackSingle()
      {
         return new VideoLayoutDefinition
         {
            Key = "single",
            DisplayName = "1 - Single",
            Rows = 1,
            Columns = 1,
            Slots = new List<VideoSlotDefinition>
            {
               new() { Index = 0, Row = 0, Column = 0, RowSpan = 1, ColumnSpan = 1 }
            }
         };
      }
   }
}
