using System.Text;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Vao.Client;
using Vao.Client.Components;

namespace Vao.Sample
{
   public partial class CameraLockWindow : Window
   {
      private readonly FlexApiClient mFlexApiClient;
      private readonly Camera mCurrentCamera;

      public CameraLockWindow()
      {
         InitializeComponent();
      }

      public CameraLockWindow(FlexApiClient client, Camera camera)
      {
         InitializeComponent();
         mFlexApiClient = client;
         mCurrentCamera = camera;
      }

      private void btnSend_Click(object sender, RoutedEventArgs e)
      {
         int hours = (int)(numUpDownHour.Value ?? 0);
         int minutes = (int)(numUpDownMin.Value ?? 0);
         int seconds = (int)(numUpDownSec.Value ?? 0);

         if (hours == 0 && minutes == 0 && seconds == 0)
         {
            mFlexApiClient.SendLockCamera(mCurrentCamera.ComponentNumber, null);
            return;
         }

         var duration = new StringBuilder("PT");
         if (hours > 0) duration.Append($"{hours}H");
         if (minutes > 0) duration.Append($"{minutes}M");
         if (seconds > 0) duration.Append($"{seconds}S");

         mFlexApiClient.SendLockCamera(mCurrentCamera.ComponentNumber, duration.ToString());
      }

      private void numUpDown_ValueChanged(object sender, NumericUpDownValueChangedEventArgs e)
      {
         if (numUpDownHour == null || numUpDownMin == null || numUpDownSec == null) return;
         bool canBe24 = (numUpDownMin.Value ?? 0) == 0 && (numUpDownSec.Value ?? 0) == 0;
         numUpDownHour.Maximum = canBe24 ? 24 : 23;
         if ((numUpDownHour.Value ?? 0) > numUpDownHour.Maximum)
            numUpDownHour.Value = numUpDownHour.Maximum;
      }

      private void btnCancel_Click(object sender, RoutedEventArgs e) => Close();
   }
}
