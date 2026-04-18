using Avalonia.Controls;
using Avalonia.Interactivity;
using Vao.Client;
using Vao.Client.Components;

namespace Vao.Sample
{
   public partial class AbsolutePositionWindow : Window
   {
      private readonly FlexRApiClient mFlexRApiClient;
      private readonly Camera mCurrentCamera;

      public AbsolutePositionWindow()
      {
         InitializeComponent();
      }

      public AbsolutePositionWindow(FlexRApiClient client, Camera camera)
      {
         InitializeComponent();
         mFlexRApiClient = client;
         mCurrentCamera = camera;
      }

      private void btnSendAbsolutePosition_Click(object sender, RoutedEventArgs e)
      {
         float? pan = TryParseOrNull(txtAbsolutePan.Text);
         float? tilt = TryParseOrNull(txtAbsoluteTilt.Text);
         float? zoom = TryParseOrNull(txtAbsoluteZoom.Text);
         mFlexRApiClient.SendAbsolutePosition(mCurrentCamera.ComponentNumber, pan, tilt, zoom);
      }

      private static float? TryParseOrNull(string text)
      {
         return float.TryParse(text, out float value) ? value : null;
      }

      private static bool IsValidFloat(string text)
      {
         if (string.IsNullOrWhiteSpace(text)) return false;
         return float.TryParse(text, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out _);
      }

      private void btnCancel_Click(object sender, RoutedEventArgs e) => Close();

      private void txtAbsoluteValue_TextChanged(object sender, TextChangedEventArgs e)
      {
         btnSendAbsolutePosition.IsEnabled = IsValidFloat(txtAbsolutePan.Text) || IsValidFloat(txtAbsoluteTilt.Text) || IsValidFloat(txtAbsoluteZoom.Text);
      }
   }
}
