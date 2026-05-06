using System.Text;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Vao.Client;
using Vao.Client.Components;
using Vao.Sample.Navigation;

namespace Vao.Sample.Pages
{
   public partial class CameraLockPage : NavigableViewBase
   {
      private readonly Camera mCurrentCamera;

      public CameraLockPage()
      {
         InitializeComponent();
      }

      public CameraLockPage(Camera camera) : this()
      {
         mCurrentCamera = camera;
      }

      private void btnSend_Click(object sender, RoutedEventArgs e)
      {
         if (mCurrentCamera == null) return;

         var numHour = this.FindControl<NumericUpDown>("numUpDownHour");
         var numMin = this.FindControl<NumericUpDown>("numUpDownMin");
         var numSec = this.FindControl<NumericUpDown>("numUpDownSec");

         int hours = (int)(numHour?.Value ?? 0);
         int minutes = (int)(numMin?.Value ?? 0);
         int seconds = (int)(numSec?.Value ?? 0);

         if (hours == 0 && minutes == 0 && seconds == 0)
         {
            mCurrentCamera.Lock(null);
            GoBack();
            return;
         }

         var duration = new StringBuilder("PT");
         if (hours > 0) duration.Append($"{hours}H");
         if (minutes > 0) duration.Append($"{minutes}M");
         if (seconds > 0) duration.Append($"{seconds}S");

         mCurrentCamera.Lock(duration.ToString());
         GoBack();
      }

      private void numUpDown_ValueChanged(object sender, NumericUpDownValueChangedEventArgs e)
      {
         var numHour = this.FindControl<NumericUpDown>("numUpDownHour");
         var numMin = this.FindControl<NumericUpDown>("numUpDownMin");
         var numSec = this.FindControl<NumericUpDown>("numUpDownSec");

         if (numHour == null || numMin == null || numSec == null) return;

         bool canBe24 = (numMin.Value ?? 0) == 0 && (numSec.Value ?? 0) == 0;
         numHour.Maximum = canBe24 ? 24 : 23;
      }

      private void btnCancel_Click(object sender, RoutedEventArgs e) => GoBack();

      private void btnBack_Click(object sender, RoutedEventArgs e) => GoBack();

      public override string GetPageTitle() => "Camera Lock";
   }
}
