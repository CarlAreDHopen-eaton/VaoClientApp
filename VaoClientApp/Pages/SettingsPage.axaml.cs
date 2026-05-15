using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using Vao.Sample.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Vao.Sample.Pages
{
    public partial class SettingsPage : NavigableViewBase
    {
        private bool mSettingsSaved = false;
        private string mOriginalThemeKey = string.Empty;
        private List<ConnectionAlternative> mConnectionAlternatives = new List<ConnectionAlternative>();

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
            mOriginalThemeKey = ConfigurationManager.Instance.GetPreferredThemeKey();
            LoadSettings();
        }

        private void LoadSettings()
        {
            var cfg = ConfigurationManager.Instance;
            var app = (App)Application.Current;
            var themeOptions = app.GetThemeOptions();
            var selectedThemeKey = cfg.GetPreferredThemeKey();

            mConnectionAlternatives = cfg.GetConnectionAlternatives()
                .Select(c => new ConnectionAlternative { Host = c.Host, Port = c.Port })
                .ToList();

            ConfigurationManager.SetTextBoxValue(this, "txtSystemName", cfg.SystemName);
            ConfigurationManager.SetTextBoxValue(this, "txtUser", cfg.User);
            ConfigurationManager.SetTextBoxValue(this, "txtPassword", cfg.Password);
            ConfigurationManager.SetTextBoxValue(this, "txtFTPUser", cfg.FTPUser);
            ConfigurationManager.SetTextBoxValue(this, "txtFTPPassword", cfg.FTPPassword);
            ConfigurationManager.SetTextBoxValue(this, "txtDownloadPath", cfg.DownloadPath);
            ConfigurationManager.SetCheckBoxValue(this, "chkSecure", cfg.UseHttps);
            ConfigurationManager.SetCheckBoxValue(this, "chkUseTcp", cfg.UseTcp);
            ConfigurationManager.SetCheckBoxValue(this, "chkPreferSubChannel", cfg.PreferSubChannel);
            ConfigurationManager.SetCheckBoxValue(this, "chkAutoConnect", cfg.AutoConnectOnStartup);
            ConfigurationManager.SetCheckBoxValue(this, "chkShowActiveAlarms", cfg.ShowActiveAlarms);
            ConfigurationManager.SetCheckBoxValue(this, "chkShowTamperedAlarms", cfg.ShowTamperedAlarms);
            ConfigurationManager.SetCheckBoxValue(this, "chkShowAcknowledgedAlarms", cfg.ShowAcknowledgedAlarms);
            ConfigurationManager.SetCheckBoxValue(this, "chkShowPassiveAlarms", cfg.ShowPassiveAlarms);
            ConfigurationManager.SetCheckBoxValue(this, "chkShowDisabledAlarms", cfg.ShowDisabledAlarms);

            var cmbTheme = this.FindControl<ComboBox>("cmbTheme");
            if (cmbTheme != null)
            {
                cmbTheme.ItemsSource = themeOptions;
                cmbTheme.SelectedItem = themeOptions.FirstOrDefault(option => option.Key == selectedThemeKey);
            }

            RefreshConnectionList(cfg.SelectedConnectionIndex);
        }

        private void SaveSettings()
        {
            var cfg = ConfigurationManager.Instance;

            cfg.SystemName = ConfigurationManager.GetTextBoxValue(this, "txtSystemName");
            cfg.User = ConfigurationManager.GetTextBoxValue(this, "txtUser");
            cfg.Password = ConfigurationManager.GetTextBoxValue(this, "txtPassword");
            cfg.FTPUser = ConfigurationManager.GetTextBoxValue(this, "txtFTPUser");
            cfg.FTPPassword = ConfigurationManager.GetTextBoxValue(this, "txtFTPPassword");
            cfg.DownloadPath = ConfigurationManager.GetTextBoxValue(this, "txtDownloadPath");
            cfg.UseHttps = ConfigurationManager.GetCheckBoxValue(this, "chkSecure");
            cfg.UseTcp = ConfigurationManager.GetCheckBoxValue(this, "chkUseTcp");
            cfg.PreferSubChannel = ConfigurationManager.GetCheckBoxValue(this, "chkPreferSubChannel");
            cfg.AutoConnectOnStartup = ConfigurationManager.GetCheckBoxValue(this, "chkAutoConnect");
            cfg.ShowActiveAlarms = ConfigurationManager.GetCheckBoxValue(this, "chkShowActiveAlarms");
            cfg.ShowTamperedAlarms = ConfigurationManager.GetCheckBoxValue(this, "chkShowTamperedAlarms");
            cfg.ShowAcknowledgedAlarms = ConfigurationManager.GetCheckBoxValue(this, "chkShowAcknowledgedAlarms");
            cfg.ShowPassiveAlarms = ConfigurationManager.GetCheckBoxValue(this, "chkShowPassiveAlarms");
            cfg.ShowDisabledAlarms = ConfigurationManager.GetCheckBoxValue(this, "chkShowDisabledAlarms");

            var selectedTheme = this.FindControl<ComboBox>("cmbTheme")?.SelectedItem as ThemeOption;
            cfg.SelectedTheme = selectedTheme?.Key ?? cfg.GetPreferredThemeKey();
            cfg.SetConnectionAlternatives(mConnectionAlternatives, ConfigurationManager.GetListBoxSelectedIndex(this, "lstConnections"));

            cfg.Flush();
            mOriginalThemeKey = cfg.SelectedTheme;

            // Apply the theme to ensure it's fully applied with persistence
            ((App)Application.Current).ApplyTheme(cfg.SelectedTheme, persistSelection: true);
            mSettingsSaved = true;
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

        private void btnAddConnection_Click(object sender, RoutedEventArgs e)
        {
            var txtConnectionHost = this.FindControl<TextBox>("txtConnectionHost");
            var txtConnectionPort = this.FindControl<TextBox>("txtConnectionPort");
            var host = txtConnectionHost?.Text?.Trim() ?? string.Empty;
            var port = txtConnectionPort?.Text?.Trim() ?? "444";

            if (string.IsNullOrWhiteSpace(host))
                return;

            mConnectionAlternatives.Add(new ConnectionAlternative { Host = host, Port = string.IsNullOrWhiteSpace(port) ? "444" : port });
            RefreshConnectionList(mConnectionAlternatives.Count - 1);
            ClearConnectionEntryFields();
        }

        private void btnUpdateConnection_Click(object sender, RoutedEventArgs e)
        {
            var txtConnectionHost = this.FindControl<TextBox>("txtConnectionHost");
            var txtConnectionPort = this.FindControl<TextBox>("txtConnectionPort");
            var lstConnections = this.FindControl<ListBox>("lstConnections");

            if (lstConnections == null || lstConnections.SelectedIndex < 0 || lstConnections.SelectedIndex >= mConnectionAlternatives.Count)
                return;

            var host = txtConnectionHost?.Text?.Trim() ?? string.Empty;
            var port = txtConnectionPort?.Text?.Trim() ?? "444";
            if (string.IsNullOrWhiteSpace(host))
                return;

            mConnectionAlternatives[lstConnections.SelectedIndex] = new ConnectionAlternative
            {
                Host = host,
                Port = string.IsNullOrWhiteSpace(port) ? "444" : port
            };

            RefreshConnectionList(lstConnections.SelectedIndex);
        }

        private void btnRemoveConnection_Click(object sender, RoutedEventArgs e)
        {
            var lstConnections = this.FindControl<ListBox>("lstConnections");
            if (lstConnections == null || lstConnections.SelectedIndex < 0 || lstConnections.SelectedIndex >= mConnectionAlternatives.Count)
                return;

            var removedIndex = lstConnections.SelectedIndex;
            mConnectionAlternatives.RemoveAt(removedIndex);

            var nextIndex = Math.Min(removedIndex, mConnectionAlternatives.Count - 1);
            RefreshConnectionList(nextIndex);
            ClearConnectionEntryFields();
        }

        private void lstConnections_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var lstConnections = this.FindControl<ListBox>("lstConnections");
            var txtConnectionHost = this.FindControl<TextBox>("txtConnectionHost");
            var txtConnectionPort = this.FindControl<TextBox>("txtConnectionPort");

            if (lstConnections == null || txtConnectionHost == null || txtConnectionPort == null)
                return;

            if (lstConnections.SelectedIndex < 0 || lstConnections.SelectedIndex >= mConnectionAlternatives.Count)
            {
                ClearConnectionEntryFields();
                return;
            }

            var selected = mConnectionAlternatives[lstConnections.SelectedIndex];
            txtConnectionHost.Text = selected.Host;
            txtConnectionPort.Text = selected.Port;
        }

        private void RefreshConnectionList(int selectedIndex)
        {
            var lstConnections = this.FindControl<ListBox>("lstConnections");
            if (lstConnections == null)
                return;

            lstConnections.ItemsSource = mConnectionAlternatives
                .Select((entry, index) => $"{index + 1}. {entry.DisplayName}")
                .ToList();

            if (mConnectionAlternatives.Count == 0)
            {
                lstConnections.SelectedIndex = -1;
                return;
            }

            lstConnections.SelectedIndex = Math.Clamp(selectedIndex, 0, mConnectionAlternatives.Count - 1);
        }

        private void ClearConnectionEntryFields()
        {
            var txtConnectionHost = this.FindControl<TextBox>("txtConnectionHost");
            var txtConnectionPort = this.FindControl<TextBox>("txtConnectionPort");
            if (txtConnectionHost != null)
                txtConnectionHost.Text = string.Empty;
            if (txtConnectionPort != null)
                txtConnectionPort.Text = "444";
        }

        public void CancelAndGoBack()
        {
            // Restore original appearance settings if changed and go back.
            ((App)Application.Current).ApplyTheme(mOriginalThemeKey, persistSelection: false);
            GoBack();
        }

        public override string GetPageTitle() => "Settings";

        public bool WereSettingsSaved() => mSettingsSaved;
    }
}
