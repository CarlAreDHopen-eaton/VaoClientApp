using System;
using System.Windows.Forms;
using Vao.Client;
using Vao.Client.Components;
using System.Text;

namespace Vao.Sample
{
   public partial class CameraLockWindow : Form
   {
      private readonly VaoClient mVaoClient = null;
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
      public VaoClient VaoClient
      { 
         get { return mVaoClient; } 
      }

      public CameraLockWindow(VaoClient client, Camera camera)
      {
         InitializeComponent();
         mVaoClient = client;
         CurrentCamera = camera;

         // Updates the title bar to dark.
         DarkModeHelper.EnableImmersiveDarkMode(Handle);
      }

      public CameraLockWindow()
      {
         InitializeComponent();
      }

      private void btnSendAbsolutePosition_Click(object sender, EventArgs e)
      {
         int hours = (int)numUpDownHour.Value;
         int minutes = (int)numUpDownMin.Value;
         int seconds = (int)numUpDownSec.Value;

         // If everything is zero, send null
         if (hours == 0 && minutes == 0 && seconds == 0)
         {
            VaoClient.SendLockCamera(CurrentCamera.ComponentNumber, null);
            return;
         }

         StringBuilder duration = new StringBuilder("PT");

         if (hours > 0)
            duration.Append($"{hours}H");

         if (minutes > 0)
            duration.Append($"{minutes}M");

         if (seconds > 0)
            duration.Append($"{seconds}S");

         VaoClient.SendLockCamera(CurrentCamera.ComponentNumber, duration.ToString());
      }


      private void UpdateHourConstraints()
      {
         bool canBe24 = numUpDownMin.Value == 0 && numUpDownSec.Value == 0;

         numUpDownHour.Maximum = canBe24 ? 24 : 23;

         if (numUpDownHour.Value > numUpDownHour.Maximum)
            numUpDownHour.Value = numUpDownHour.Maximum;
      }


      private void btnCancel_Click(object sender, EventArgs e)
      {
         Close();
      }

      private void numUpDownHour_ValueChanged(object sender, EventArgs e)
      {
         UpdateHourConstraints();
      }

      private void numUpDownMin_ValueChanged(object sender, EventArgs e)
      {
         UpdateHourConstraints();
      }

      private void numUpDownSec_ValueChanged(object sender, EventArgs e)
      {
         UpdateHourConstraints();
      }
   }
}
