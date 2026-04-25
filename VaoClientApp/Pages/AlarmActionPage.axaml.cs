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
        private readonly FlexRApiClient _flexRApiClient;
        private readonly Alarm _currentAlarm;
        private readonly User _currentLoggedInUser;

        public AlarmActionPage()
        {
            InitializeComponent();
        }

        public AlarmActionPage(FlexRApiClient client, Alarm alarm, User user) : this()
        {
            _flexRApiClient = client;
            _currentAlarm = alarm;
            _currentLoggedInUser = user;
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

            bool hasAccess = _currentLoggedInUser != null && 
                           _currentLoggedInUser.HasAccessLevel(UserPrivilege.Supervisor) && 
                           _currentAlarm?.Priority <= _currentLoggedInUser.Priority;

            if (hasAccess)
            {
                if (btnActivate != null) btnActivate.IsEnabled = _currentAlarm.Status == AlarmGeneralStatus.Inactive || _currentAlarm.Status == AlarmGeneralStatus.Acknowledged;
                if (btnAcknowledge != null) btnAcknowledge.IsEnabled = _currentAlarm.Status == AlarmGeneralStatus.Active;
                if (btnEnable != null) btnEnable.IsEnabled = _currentAlarm.Status == AlarmGeneralStatus.Disabled;
                if (btnDisable != null) btnDisable.IsEnabled = _currentAlarm.Status != AlarmGeneralStatus.Disabled;
                if (btnExpediate != null) btnExpediate.IsEnabled = _currentAlarm.Status == AlarmGeneralStatus.Active;
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

            if (_currentAlarm != null)
            {
                if (txtName != null) txtName.Text = _currentAlarm.Name;
                if (txtStatus != null) txtStatus.Text = _currentAlarm.Status.ToString();
                if (txtExtended != null) txtExtended.Text = _currentAlarm.ExtendedStatus;
                if (txtPriority != null) txtPriority.Text = _currentAlarm.Priority.ToString();
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) => GoBack();
        private void btnBack_Click(object sender, RoutedEventArgs e) => GoBack();
        
        private void btnAlarmActivate_Click(object sender, RoutedEventArgs e) 
        {
            _flexRApiClient?.SendAlarmCommand(_currentAlarm.ComponentNumber, nameof(AlarmCommands.Activate));
            GoBack();
        }

        private void btnAlarmAcknowledge_Click(object sender, RoutedEventArgs e) 
        {
            _flexRApiClient?.SendAlarmCommand(_currentAlarm.ComponentNumber, nameof(AlarmCommands.Acknowledge));
            GoBack();
        }

        private void btnAlarmEnable_Click(object sender, RoutedEventArgs e) 
        {
            _flexRApiClient?.SendAlarmCommand(_currentAlarm.ComponentNumber, nameof(AlarmCommands.Enable));
            GoBack();
        }

        private void btnAlarmDisable_Click(object sender, RoutedEventArgs e) 
        {
            _flexRApiClient?.SendAlarmCommand(_currentAlarm.ComponentNumber, nameof(AlarmCommands.Disable));
            GoBack();
        }

        private void btnAlarmExpediate_Click(object sender, RoutedEventArgs e) 
        {
            _flexRApiClient?.SendAlarmCommand(_currentAlarm.ComponentNumber, nameof(AlarmCommands.Expediate));
            GoBack();
        }

        public override string GetPageTitle() => "Alarm Action";
    }
}
