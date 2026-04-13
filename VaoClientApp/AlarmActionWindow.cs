using System;
using System.Windows.Forms;
using Vao.Client;
using Vao.Client.Components;
using Vao.Client.Enum;

namespace Vao.Sample
{
   public partial class AlarmActionWindow : Form
   {
      private readonly VaoClient mVaoClient = null;
      private Alarm mCurrentAlarm;
      private User mCurrentLoggedInUser;

      public Alarm CurrentAlarm
      {
         get
         {
            return mCurrentAlarm;
         }
         set
         {
            mCurrentAlarm = value;
            UpdateEnabled();
         }
      }

      public User CurrentLoggedInUser
      {
         get
         {
            return mCurrentLoggedInUser;
         }
         set
         {
            mCurrentLoggedInUser = value;
         }
      }
      public VaoClient VaoClient
      { 
         get { return mVaoClient; } 
      }

      public AlarmActionWindow(VaoClient client, Alarm alarm, User user)
      {
         InitializeComponent();
         mVaoClient = client;
         CurrentLoggedInUser = user;
         CurrentAlarm = alarm;

         UpdateEnabled();
         // Updates the title bar to dark.
         DarkModeHelper.EnableImmersiveDarkMode(Handle);
         FillAlarmInfo();
      }

      public AlarmActionWindow()
      {
         InitializeComponent();
      }

      public void UpdateEnabled()
      {
         // Supervisor-level access or above is required to change alarm state
         if (CurrentLoggedInUser != null && CurrentLoggedInUser.HasAccessLevel(UserPrivilege.Supervisor) && CurrentAlarm?.Priority <= CurrentLoggedInUser.Priority)
         {
            btnAlarmActivate.Enabled = CurrentAlarm.Status == AlarmGeneralStatus.Inactive || CurrentAlarm.Status == AlarmGeneralStatus.Acknowledged;
            btnAlarmAcknowledge.Enabled = CurrentAlarm.Status == AlarmGeneralStatus.Active;
            btnAlarmEnable.Enabled = CurrentAlarm.Status == AlarmGeneralStatus.Disabled;
            btnAlarmDisable.Enabled = CurrentAlarm.Status != AlarmGeneralStatus.Disabled;
            btnAlarmExpediate.Enabled = CurrentAlarm.Status == AlarmGeneralStatus.Active;
         }
         else
         {
            btnAlarmActivate.Enabled = false;
            btnAlarmAcknowledge.Enabled = false;
            btnAlarmEnable.Enabled = false;
            btnAlarmDisable.Enabled = false;
            btnAlarmExpediate.Enabled = false;
         }
      }

      private void FillAlarmInfo()
      {
         if (CurrentAlarm != null)
         {
            txtAlarmName.Text = CurrentAlarm.Name;
            txtAlarmStatus.Text = CurrentAlarm.Status.ToString();
            txtAlarmExtendedStatus.Text = CurrentAlarm.ExtendedStatus;
            txtAlarmPriority.Text = CurrentAlarm.Priority.ToString();
         }
      }

      private void btnCancel_Click(object sender, EventArgs e)
      {
         Close();
      }

      private void btnAlarmActivate_Click(object sender, EventArgs e)
      {
         VaoClient.SendAlarmCommand(CurrentAlarm.ComponentNumber, nameof(AlarmCommands.Activate));
      }

      private void btnAlarmAcknowledge_Click(object sender, EventArgs e)
      {
         VaoClient.SendAlarmCommand(CurrentAlarm.ComponentNumber, nameof(AlarmCommands.Acknowledge));
      }

      private void btnAlarmEnable_Click(object sender, EventArgs e)
      {
         VaoClient.SendAlarmCommand(CurrentAlarm.ComponentNumber, nameof(AlarmCommands.Enable));
      }

      private void btnAlarmDisable_Click(object sender, EventArgs e)
      {
         VaoClient.SendAlarmCommand(CurrentAlarm.ComponentNumber, nameof(AlarmCommands.Disable));
      }

      private void btnAlarmExpediate_Click(object sender, EventArgs e)
      {
         VaoClient.SendAlarmCommand(CurrentAlarm.ComponentNumber, nameof(AlarmCommands.Expediate));
      }
   }
}
