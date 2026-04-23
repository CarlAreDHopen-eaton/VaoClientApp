using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;

namespace Vao.Sample
{
    public partial class SettingsWindow : Window
    {
        private bool _settingsSaved = false;

        public SettingsWindow()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void LoadSettings()
        {
            var s = AppSettings.Default;
            var txtHost = this.FindControl<TextBox>("txtHost");
            var txtPort = this.FindControl<TextBox>("txtPort");
            var txtUser = this.FindControl<TextBox>("txtUser");
            var txtPassword = this.FindControl<TextBox>("txtPassword");
            var chkSecure = this.FindControl<CheckBox>("chkSecure");
            var chkUseTcp = this.FindControl<CheckBox>("chkUseTcp");
            var chkPreferSubChannel = this.FindControl<CheckBox>("chkPreferSubChannel");
            var chkDarkMode = this.FindControl<CheckBox>("chkDarkMode");
            var chkAutoConnect = this.FindControl<CheckBox>("chkAutoConnect");

            txtHost.Text = s.Host1;
            txtPort.Text = s.ApiPort;
            txtUser.Text = s.User;
            txtPassword.Text = s.Password;
            chkSecure.IsChecked = s.UseHttps;
            chkUseTcp.IsChecked = s.UseTcp;
            chkPreferSubChannel.IsChecked = s.PreferSubChannel;
            chkDarkMode.IsChecked = s.IsDarkMode;
            chkAutoConnect.IsChecked = s.AutoConnectOnStartup;
        }

        private void SaveSettings()
        {
            var s = AppSettings.Default;
            var txtHost = this.FindControl<TextBox>("txtHost");
            var txtPort = this.FindControl<TextBox>("txtPort");
            var txtUser = this.FindControl<TextBox>("txtUser");
            var txtPassword = this.FindControl<TextBox>("txtPassword");
            var chkSecure = this.FindControl<CheckBox>("chkSecure");
            var chkUseTcp = this.FindControl<CheckBox>("chkUseTcp");
            var chkPreferSubChannel = this.FindControl<CheckBox>("chkPreferSubChannel");
            var chkDarkMode = this.FindControl<CheckBox>("chkDarkMode");
            var chkAutoConnect = this.FindControl<CheckBox>("chkAutoConnect");

            s.Host1 = txtHost.Text ?? "";
            s.ApiPort = txtPort.Text ?? "";
            s.User = txtUser.Text ?? "";
            s.Password = txtPassword.Text ?? "";
            s.UseHttps = chkSecure.IsChecked == true;
            s.UseTcp = chkUseTcp.IsChecked == true;
            s.PreferSubChannel = chkPreferSubChannel.IsChecked == true;
            s.IsDarkMode = chkDarkMode.IsChecked == true;
            s.AutoConnectOnStartup = chkAutoConnect.IsChecked == true;
            s.Save();
            _settingsSaved = true;
        }

        private void chkDarkMode_Changed(object sender, RoutedEventArgs e)
        {
            var chkDarkMode = this.FindControl<CheckBox>("chkDarkMode");
            bool isDark = chkDarkMode.IsChecked == true;
            ((App)Application.Current).SetTheme(isDark);
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            SaveSettings();
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            // Restore original dark mode setting if changed
            var s = AppSettings.Default;
            ((App)Application.Current).SetTheme(s.IsDarkMode);
            Close();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            btnCancel_Click(sender, e);
        }

        public bool WereSettingsSaved()
        {
            return _settingsSaved;
        }
    }
}
