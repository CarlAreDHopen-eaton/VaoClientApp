using System;
using System.Drawing;
using System.Windows.Forms;

namespace Vao.Sample
{
   public class CollapsibleDarkSectionPanel : DarkUI.Controls.DarkSectionPanel
   {
      private Rectangle mMinButtonRect;
      private bool mbIsCollapsed = false;
      private int miStoredHeight;

      protected override void OnResize(EventArgs e)
      {
         base.OnResize(e);
         mMinButtonRect = new Rectangle(Width - 20, 5, 14, 14);
      }

      protected override void OnPaint(PaintEventArgs e)
      {
         base.OnPaint(e);

         // Draw minimize/maximize symbol
         string symbol = mbIsCollapsed ? "+" : "-";
         TextRenderer.DrawText(e.Graphics, symbol, Font, mMinButtonRect, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
      }

      protected override void OnMouseClick(MouseEventArgs e)
      {
         base.OnMouseClick(e);

         if (e.Button == MouseButtons.Left && mMinButtonRect.Contains(e.Location))
         {
            ToggleCollapsed();
         }
      }

      private void ToggleCollapsed()
      {
         if (mbIsCollapsed)
         {
            Height = miStoredHeight;
         }
         else
         {
            miStoredHeight = Height;
            Height = Padding.Top + 1;
         }

         mbIsCollapsed = !mbIsCollapsed;

         CollapseStateChanged?.Invoke(this, EventArgs.Empty);

         Invalidate();
      }


      public event EventHandler CollapseStateChanged;
   }
}
