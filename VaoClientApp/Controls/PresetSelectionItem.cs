using System;
using System.Globalization;
using Vao.Client.Components;

namespace Vao.Sample.Controls
{
   public class PresetSelectionItem
   {
      public PresetSelectionItem(Preset preset) { Preset = preset; }
      public Preset Preset { get; }

      public string PresetName
      {
         get { return Preset?.Name ?? string.Empty; }
      }

      public string PresetNumberText
      {
         get { return Preset == null ? string.Empty : $"#{Preset.ComponentNumber:D3}"; }
      }

      public bool Matches(string query)
      {
         if (Preset == null || string.IsNullOrWhiteSpace(query)) return Preset != null;
         return Preset.Name?.Contains(query, StringComparison.OrdinalIgnoreCase) == true
            || Preset.ComponentNumber.ToString(CultureInfo.InvariantCulture).Contains(query, StringComparison.OrdinalIgnoreCase);
      }
   }
}
