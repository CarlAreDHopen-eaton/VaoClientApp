using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using Vao.Sample.Navigation;
using System;
using System.Linq;

namespace Vao.Sample.Pages
{
    public partial class SettingsPage : NavigableViewBase
    {
        private bool _settingsSaved = false;
        private string _originalThemeKey = string.Empty;

        public SettingsPage()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnNavigatedTo()
        {
            _originalThemeKey = AppSettings.Default.GetPreferredThemeKey();
            LoadSettings();
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
            var cmbTheme = this.FindControl<ComboBox>("cmbTheme");
            var chkAutoConnect = this.FindControl<CheckBox>("chkAutoConnect");
            var txtFTPUser = this.FindControl<TextBox>("txtFTPUser");
            var txtFTPPassword = this.FindControl<TextBox>("txtFTPPassword");
            var txtDownloadPath = this.FindControl<TextBox>("txtDownloadPath");
            var app = (App)Application.Current;
            var themeOptions = app.GetThemeOptions();
            var selectedThemeKey = s.GetPreferredThemeKey();

            if (txtHost != null) txtHost.Text = s.Host1;
            if (txtPort != null) txtPort.Text = s.ApiPort;
            if (txtUser != null) txtUser.Text = s.User;
            if (txtPassword != null) txtPassword.Text = s.Password;
            if (chkSecure != null) chkSecure.IsChecked = s.UseHttps;
            if (chkUseTcp != null) chkUseTcp.IsChecked = s.UseTcp;
            if (chkPreferSubChannel != null) chkPreferSubChannel.IsChecked = s.PreferSubChannel;
            if (chkAutoConnect != null) chkAutoConnect.IsChecked = s.AutoConnectOnStartup;
            if (txtFTPUser != null) txtFTPUser.Text = s.FTPUser;
            if (txtFTPPassword != null) txtFTPPassword.Text = s.FTPPassword;
            if (txtDownloadPath != null) txtDownloadPath.Text = s.DownloadPath;
            if (cmbTheme != null)
            {
                cmbTheme.ItemsSource = themeOptions;
                cmbTheme.SelectedItem = themeOptions.FirstOrDefault(option => option.Key == selectedThemeKey);
            }
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
            var cmbTheme = this.FindControl<ComboBox>("cmbTheme");
            var chkAutoConnect = this.FindControl<CheckBox>("chkAutoConnect");
            var txtFTPUser = this.FindControl<TextBox>("txtFTPUser");
            var txtFTPPassword = this.FindControl<TextBox>("txtFTPPassword");
            var txtDownloadPath = this.FindControl<TextBox>("txtDownloadPath");
            var selectedTheme = cmbTheme?.SelectedItem as ThemeOption;

            s.Host1 = txtHost?.Text ?? "";
            s.ApiPort = txtPort?.Text ?? "";
            s.User = txtUser?.Text ?? "";
            s.Password = txtPassword?.Text ?? "";
            s.UseHttps = chkSecure?.IsChecked == true;
            s.UseTcp = chkUseTcp?.IsChecked == true;
            s.PreferSubChannel = chkPreferSubChannel?.IsChecked == true;
            s.SelectedTheme = selectedTheme?.Key ?? s.GetPreferredThemeKey();
            s.AutoConnectOnStartup = chkAutoConnect?.IsChecked == true;
            s.FTPUser = txtFTPUser?.Text ?? "";
            s.FTPPassword = txtFTPPassword?.Text ?? "";
            s.DownloadPath = txtDownloadPath?.Text ?? "";
            s.Save();
            _originalThemeKey = s.SelectedTheme;
            _settingsSaved = true;
        }

        private void cmbTheme_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedTheme = this.FindControl<ComboBox>("cmbTheme")?.SelectedItem as ThemeOption;
            if (selectedTheme != null)
                ((App)Application.Current).ApplyTheme(selectedTheme.Key, persistSelection: false);
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            SaveSettings();
            GoBack();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            CancelAndGoBack();
        }

        private async void btnBrowseDownloadPath_Click(object sender, RoutedEventArgs e)
        {
            var topLevel = TopLevel.GetTopLevel(this);
            var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Select download path",
                AllowMultiple = false
            });
            if (folders.Count > 0)
            {
                var txtDownloadPath = this.FindControl<TextBox>("txtDownloadPath");
                if (txtDownloadPath != null)
                    txtDownloadPath.Text = folders[0].Path.LocalPath;
            }
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            CancelAndGoBack();
        }

        public void CancelAndGoBack()
        {
            // Restore original appearance settings if changed and go back.
            ((App)Application.Current).ApplyTheme(_originalThemeKey, persistSelection: false);
            GoBack();
        }

        public override string GetPageTitle() => "Settings";

        public bool WereSettingsSaved() => _settingsSaved;
    }
}
