using Avalonia.Controls;
using Avalonia.Interactivity;
using Vao.Client;
using Vao.Client.Components;
using Vao.Client.Enum;

namespace Vao.Sample
{
   public partial class AlarmActionWindow : Window
   {
      private readonly FlexRApiClient mFlexRApiClient;
      private readonly Alarm mCurrentAlarm;
      private readonly User mCurrentLoggedInUser;

      public AlarmActionWindow()
      {
         InitializeComponent();
      }

      public AlarmActionWindow(FlexRApiClient client, Alarm alarm, User user)
      {
         InitializeComponent();
         mFlexRApiClient = client;
         mCurrentAlarm = alarm;
         mCurrentLoggedInUser = user;
         UpdateEnabled();
         FillAlarmInfo();
      }

      private void UpdateEnabled()
      {
         if (mCurrentLoggedInUser != null && mCurrentLoggedInUser.HasAccessLevel(UserPrivilege.Supervisor) && mCurrentAlarm?.Priority <= mCurrentLoggedInUser.Priority)
         {
            btnAlarmActivate.IsEnabled = mCurrentAlarm.Status == AlarmGeneralStatus.Inactive || mCurrentAlarm.Status == AlarmGeneralStatus.Acknowledged;
            btnAlarmAcknowledge.IsEnabled = mCurrentAlarm.Status == AlarmGeneralStatus.Active;
            btnAlarmEnable.IsEnabled = mCurrentAlarm.Status == AlarmGeneralStatus.Disabled;
            btnAlarmDisable.IsEnabled = mCurrentAlarm.Status != AlarmGeneralStatus.Disabled;
            btnAlarmExpediate.IsEnabled = mCurrentAlarm.Status == AlarmGeneralStatus.Active;
         }
         else
         {
            btnAlarmActivate.IsEnabled = false;
            btnAlarmAcknowledge.IsEnabled = false;
            btnAlarmEnable.IsEnabled = false;
            btnAlarmDisable.IsEnabled = false;
            btnAlarmExpediate.IsEnabled = false;
         }
      }

      private void FillAlarmInfo()
      {
         if (mCurrentAlarm != null)
         {
            txtAlarmName.Text = mCurrentAlarm.Name;
            txtAlarmStatus.Text = mCurrentAlarm.Status.ToString();
            txtAlarmExtendedStatus.Text = mCurrentAlarm.ExtendedStatus;
            txtAlarmPriority.Text = mCurrentAlarm.Priority.ToString();
         }
      }

      private void btnCancel_Click(object sender, RoutedEventArgs e) => Close();
      private void btnAlarmActivate_Click(object sender, RoutedEventArgs e) => mFlexRApiClient.SendAlarmCommand(mCurrentAlarm.ComponentNumber, nameof(AlarmCommands.Activate));
      private void btnAlarmAcknowledge_Click(object sender, RoutedEventArgs e) => mFlexRApiClient.SendAlarmCommand(mCurrentAlarm.ComponentNumber, nameof(AlarmCommands.Acknowledge));
      private void btnAlarmEnable_Click(object sender, RoutedEventArgs e) => mFlexRApiClient.SendAlarmCommand(mCurrentAlarm.ComponentNumber, nameof(AlarmCommands.Enable));
      private void btnAlarmDisable_Click(object sender, RoutedEventArgs e) => mFlexRApiClient.SendAlarmCommand(mCurrentAlarm.ComponentNumber, nameof(AlarmCommands.Disable));
      private void btnAlarmExpediate_Click(object sender, RoutedEventArgs e) => mFlexRApiClient.SendAlarmCommand(mCurrentAlarm.ComponentNumber, nameof(AlarmCommands.Expediate));
   }
}
