using System.Collections.Generic;
using Newtonsoft.Json;

namespace Vao.Sample.Layouts
{
   public class VideoLayoutDefinition
   {
      public string Key { get; set; } = "single";
      public string DisplayName { get; set; } = "1 - Single";
      public int Rows { get; set; } = 1;
      public int Columns { get; set; } = 1;
      public List<VideoSlotDefinition> Slots { get; set; } = new();

      [JsonIgnore]
      public int SlotCount => Slots?.Count ?? 0;

      [JsonIgnore]
      public bool IsSingleView => SlotCount <= 1;
   }

   public class VideoSlotDefinition
   {
      public int Index { get; set; }
      public int Row { get; set; }
      public int Column { get; set; }
      public int RowSpan { get; set; } = 1;
      public int ColumnSpan { get; set; } = 1;
   }
}
