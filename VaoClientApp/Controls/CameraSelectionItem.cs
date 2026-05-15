using System;
using System.Globalization;
using Vao.Client.Components;

namespace Vao.Sample.Controls
{
   // ── Support types ──────────────────────────────────────────────────────────

   public class CameraSelectionItem
   {
      public CameraSelectionItem(Camera camera) { Camera = camera; }
      public Camera Camera { get; }
      public string CameraName
      {
         get { return Camera?.Name ?? string.Empty; }
      }

      public string CameraNumberText
      {
         get { return Camera == null ? string.Empty : $"#{Camera.ComponentNumber:D4}"; }
      }

      public string CameraTypeIcon
      {
         get { return Camera?.HasPanTiltControl == true ? "\uF113" : "\uF299"; }
      }

      public bool Matches(string query)
      {
         if (Camera == null || string.IsNullOrWhiteSpace(query)) return Camera != null;
         return Camera.Name?.Contains(query, StringComparison.OrdinalIgnoreCase) == true
            || Camera.ComponentNumber.ToString(CultureInfo.InvariantCulture).Contains(query, StringComparison.OrdinalIgnoreCase);
      }
   }
}
