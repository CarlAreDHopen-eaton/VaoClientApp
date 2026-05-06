using Avalonia.Controls;
using Avalonia.Interactivity;
using Vao.Client;
using Vao.Client.Components;
using Vao.Client.Enum;
using Vao.Sample.Navigation;

namespace Vao.Sample.Pages
{
   public partial class AlarmActionPage : NavigableViewBase
   {
      private readonly Alarm mCurrentAlarm;

      public AlarmActionPage()
      {
         InitializeComponent();
      }

      public AlarmActionPage(Alarm alarm) : this()
      {
         mCurrentAlarm = alarm;
         UpdateEnabled();
         FillAlarmInfo();
      }

      private void UpdateEnabled()
      {
         var btnActivate = this.FindControl<Button>("btnAlarmActivate");
         var btnAcknowledge = this.FindControl<Button>("btnAlarmAcknowledge");
         var btnEnable = this.FindControl<Button>("btnAlarmEnable");
         var btnDisable = this.FindControl<Button>("btnAlarmDisable");
         var btnExpediate = this.FindControl<Button>("btnAlarmExpediate");

         var currentLoggedInUser = mCurrentAlarm.FlexApiClient.CurrentUser;
         bool hasAccess = currentLoggedInUser != null &&
                        currentLoggedInUser.HasAccessLevel(UserPrivilege.Supervisor) &&
                        mCurrentAlarm?.Priority <= currentLoggedInUser.Priority;

         if (hasAccess)
         {
            if (btnActivate != null) btnActivate.IsEnabled = mCurrentAlarm.Status == AlarmGeneralStatus.Inactive || mCurrentAlarm.Status == AlarmGeneralStatus.Acknowledged;
            if (btnAcknowledge != null) btnAcknowledge.IsEnabled = mCurrentAlarm.Status == AlarmGeneralStatus.Active;
            if (btnEnable != null) btnEnable.IsEnabled = mCurrentAlarm.Status == AlarmGeneralStatus.Disabled;
            if (btnDisable != null) btnDisable.IsEnabled = mCurrentAlarm.Status != AlarmGeneralStatus.Disabled;
            if (btnExpediate != null) btnExpediate.IsEnabled = mCurrentAlarm.Status == AlarmGeneralStatus.Active;
         }
         else
         {
            if (btnActivate != null) btnActivate.IsEnabled = false;
            if (btnAcknowledge != null) btnAcknowledge.IsEnabled = false;
            if (btnEnable != null) btnEnable.IsEnabled = false;
            if (btnDisable != null) btnDisable.IsEnabled = false;
            if (btnExpediate != null) btnExpediate.IsEnabled = false;
         }
      }

      private void FillAlarmInfo()
      {
         var txtName = this.FindControl<TextBox>("txtAlarmName");
         var txtStatus = this.FindControl<TextBox>("txtAlarmStatus");
         var txtExtended = this.FindControl<TextBox>("txtAlarmExtendedStatus");
         var txtPriority = this.FindControl<TextBox>("txtAlarmPriority");

         if (mCurrentAlarm != null)
         {
            if (txtName != null) txtName.Text = mCurrentAlarm.Name;
            if (txtStatus != null) txtStatus.Text = mCurrentAlarm.Status.ToString();
            if (txtExtended != null) txtExtended.Text = mCurrentAlarm.ExtendedStatus;
            if (txtPriority != null) txtPriority.Text = mCurrentAlarm.Priority.ToString();
            if (txtPageHeader != null) txtPageHeader.Text = $"Alarm Details - {mCurrentAlarm.Name}";
         }
      }

      private void btnCancel_Click(object sender, RoutedEventArgs e) => GoBack();
      private void btnBack_Click(object sender, RoutedEventArgs e) => GoBack();

      private void btnAlarmActivate_Click(object sender, RoutedEventArgs e)
      {
         mCurrentAlarm.FlexApiClient?.SendAlarmCommand(mCurrentAlarm.ComponentNumber, nameof(AlarmCommands.Activate));
         GoBack();
      }

      private void btnAlarmAcknowledge_Click(object sender, RoutedEventArgs e)
      {
         mCurrentAlarm.FlexApiClient?.SendAlarmCommand(mCurrentAlarm.ComponentNumber, nameof(AlarmCommands.Acknowledge));
         GoBack();
      }

      private void btnAlarmEnable_Click(object sender, RoutedEventArgs e)
      {
         mCurrentAlarm.FlexApiClient?.SendAlarmCommand(mCurrentAlarm.ComponentNumber, nameof(AlarmCommands.Enable));
         GoBack();
      }

      private void btnAlarmDisable_Click(object sender, RoutedEventArgs e)
      {
         mCurrentAlarm.FlexApiClient?.SendAlarmCommand(mCurrentAlarm.ComponentNumber, nameof(AlarmCommands.Disable));
         GoBack();
      }

      private void btnAlarmExpediate_Click(object sender, RoutedEventArgs e)
      {
         mCurrentAlarm.FlexApiClient?.SendAlarmCommand(mCurrentAlarm.ComponentNumber, nameof(AlarmCommands.Expediate));
         GoBack();
      }

      public override string GetPageTitle() => "Alarm Action";
   }
}
