using System;
using System.Windows.Forms;
using Vao.Client;
using Vao.Client.Components;

namespace Vao.Sample
{
   public partial class AbsolutePositionWindow : Form
   {
      private readonly FlexRApiClient mFlexRApiClient = null;
      private Camera mCurrentCamera;

      public Camera CurrentCamera
      {
         get
         {
            return mCurrentCamera;
         }
         set
         {
            mCurrentCamera = value;
         }
      }
      public FlexRApiClient FlexRApiClient
      { 
         get { return mFlexRApiClient; } 
      }

      public AbsolutePositionWindow(FlexRApiClient client, Camera camera)
      {
         InitializeComponent();
         mFlexRApiClient = client;
         CurrentCamera = camera;

         // Updates the title bar to dark.
         DarkModeHelper.EnableImmersiveDarkMode(Handle);
      }

      public AbsolutePositionWindow()
      {
         InitializeComponent();
      }

      private void btnSendAbsolutePosition_Click(object sender, EventArgs e)
      {
         float? pan = TryParseOrNull(txtAbsolutePan.Text);
         float? tilt = TryParseOrNull(txtAbsoluteTilt.Text);
         float? zoom = TryParseOrNull(txtAbsoluteZoom.Text);

         FlexRApiClient.SendAbsolutePosition(CurrentCamera.ComponentNumber, pan, tilt, zoom);
      }

      private static float? TryParseOrNull(string text)
      {
         return float.TryParse(text, out float value) ? value : (float?)null;
      }

      private void txtAbsolutePan_KeyPress(object sender, KeyPressEventArgs e)
      {
         ValidateFloatKeyPress(sender, e);
      }

      private void txtAbsoluteTilt_KeyPress(object sender, KeyPressEventArgs e)
      {
         ValidateFloatKeyPress(sender, e);
      }

      private void txtAbsoluteZoom_KeyPress(object sender, KeyPressEventArgs e)
      {
         ValidateFloatKeyPress(sender, e);
      }

      private static void ValidateFloatKeyPress(object sender, KeyPressEventArgs e)
      {
         TextBox tb = (TextBox)sender;

         // allow control keys like backspace
         if (char.IsControl(e.KeyChar))
            return;

         // allow digits
         if (char.IsDigit(e.KeyChar))
            return;

         // allow one decimal point
         if (e.KeyChar == '.' && !tb.Text.Contains("."))
            return;

         e.Handled = true;
      }

      private static bool IsValidFloat(string text)
      {
         if (string.IsNullOrWhiteSpace(text))
            return false;

         return float.TryParse(text, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out _);
      }

      private void btnCancel_Click(object sender, EventArgs e)
      {
         Close();
      }

      private void txtAbsolutePan_TextChanged(object sender, EventArgs e)
      {
         btnSendAbsolutePosition.Enabled = IsValidFloat(txtAbsolutePan.Text) || IsValidFloat(txtAbsoluteTilt.Text) || IsValidFloat(txtAbsoluteZoom.Text);
      }

      private void txtAbsoluteTilt_TextChanged(object sender, EventArgs e)
      {
         btnSendAbsolutePosition.Enabled = IsValidFloat(txtAbsolutePan.Text) || IsValidFloat(txtAbsoluteTilt.Text) || IsValidFloat(txtAbsoluteZoom.Text);
      }

      private void txtAbsoluteZoom_TextChanged(object sender, EventArgs e)
      {
         btnSendAbsolutePosition.Enabled = IsValidFloat(txtAbsolutePan.Text) || IsValidFloat(txtAbsoluteTilt.Text) || IsValidFloat(txtAbsoluteZoom.Text);
      }
   }
}
