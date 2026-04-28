using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using System;
using System.Linq;

namespace Vao.Sample
{
    public partial class SettingsWindow : Window
    {
        private bool _settingsSaved = false;
        private string _originalThemeKey = string.Empty;

        public SettingsWindow()
        {
            InitializeComponent();
            _originalThemeKey = AppSettings.Default.GetPreferredThemeKey();
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
            var cmbTheme = this.FindControl<ComboBox>("cmbTheme");
            var chkAutoConnect = this.FindControl<CheckBox>("chkAutoConnect");
            var txtFTPUser = this.FindControl<TextBox>("txtFTPUser");
            var txtFTPPassword = this.FindControl<TextBox>("txtFTPPassword");
            var txtDownloadPath = this.FindControl<TextBox>("txtDownloadPath");
            var themeOptions = ((App)Application.Current).GetThemeOptions();

            txtHost.Text = s.Host1;
            txtPort.Text = s.ApiPort;
            txtUser.Text = s.User;
            txtPassword.Text = s.Password;
            chkSecure.IsChecked = s.UseHttps;
            chkUseTcp.IsChecked = s.UseTcp;
            chkPreferSubChannel.IsChecked = s.PreferSubChannel;
            chkAutoConnect.IsChecked = s.AutoConnectOnStartup;
            txtFTPUser.Text = s.FTPUser;
            txtFTPPassword.Text = s.FTPPassword;
            txtDownloadPath.Text = s.DownloadPath;
            cmbTheme.ItemsSource = themeOptions;
            cmbTheme.SelectedItem = themeOptions.FirstOrDefault(option => option.Key == s.GetPreferredThemeKey());
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
            var selectedTheme = cmbTheme.SelectedItem as ThemeOption;

            s.Host1 = txtHost.Text ?? "";
            s.ApiPort = txtPort.Text ?? "";
            s.User = txtUser.Text ?? "";
            s.Password = txtPassword.Text ?? "";
            s.UseHttps = chkSecure.IsChecked == true;
            s.UseTcp = chkUseTcp.IsChecked == true;
            s.PreferSubChannel = chkPreferSubChannel.IsChecked == true;
            s.SelectedTheme = selectedTheme?.Key ?? s.GetPreferredThemeKey();
            s.AutoConnectOnStartup = chkAutoConnect.IsChecked == true;
            s.FTPUser = txtFTPUser.Text ?? "";
            s.FTPPassword = txtFTPPassword.Text ?? "";
            s.DownloadPath = txtDownloadPath.Text ?? "";
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
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            ((App)Application.Current).ApplyTheme(_originalThemeKey, persistSelection: false);
            Close();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            btnCancel_Click(sender, e);
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
                txtDownloadPath.Text = folders[0].Path.LocalPath;
            }
        }

        public bool WereSettingsSaved()
        {
            return _settingsSaved;
        }
    }
}
