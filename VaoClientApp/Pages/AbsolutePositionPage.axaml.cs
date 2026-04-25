using Avalonia.Controls;
using Avalonia.Interactivity;
using Vao.Client;
using Vao.Client.Components;
using Vao.Sample.Navigation;

namespace Vao.Sample.Pages
{
    public partial class AbsolutePositionPage : NavigableViewBase
    {
        private readonly FlexRApiClient _flexRApiClient;
        private readonly Camera _currentCamera;

        public AbsolutePositionPage()
        {
            InitializeComponent();
        }

        public AbsolutePositionPage(FlexRApiClient client, Camera camera) : this()
        {
            _flexRApiClient = client;
            _currentCamera = camera;
        }

        private void btnSendAbsolutePosition_Click(object sender, RoutedEventArgs e)
        {
            if (_flexRApiClient == null || _currentCamera == null) return;

            var txtAbsolutePan = this.FindControl<TextBox>("txtAbsolutePan");
            var txtAbsoluteTilt = this.FindControl<TextBox>("txtAbsoluteTilt");
            var txtAbsoluteZoom = this.FindControl<TextBox>("txtAbsoluteZoom");

            float? pan = TryParseOrNull(txtAbsolutePan?.Text);
            float? tilt = TryParseOrNull(txtAbsoluteTilt?.Text);
            float? zoom = TryParseOrNull(txtAbsoluteZoom?.Text);

            _flexRApiClient.SendAbsolutePosition(_currentCamera.ComponentNumber, pan, tilt, zoom);
            GoBack();
        }

        private static float? TryParseOrNull(string text)
        {
            return float.TryParse(text, out float value) ? value : null;
        }

        private static bool IsValidFloat(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return false;
            return float.TryParse(text, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out _);
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) => GoBack();

        private void txtAbsoluteValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            var txtAbsolutePan = this.FindControl<TextBox>("txtAbsolutePan");
            var txtAbsoluteTilt = this.FindControl<TextBox>("txtAbsoluteTilt");
            var txtAbsoluteZoom = this.FindControl<TextBox>("txtAbsoluteZoom");
            var btnSend = this.FindControl<Button>("btnSendAbsolutePosition");

            if (btnSend != null)
                btnSend.IsEnabled = IsValidFloat(txtAbsolutePan?.Text) || IsValidFloat(txtAbsoluteTilt?.Text) || IsValidFloat(txtAbsoluteZoom?.Text);
        }

        private void btnBack_Click(object sender, RoutedEventArgs e) => GoBack();

        public override string GetPageTitle() => "Absolute Position";
    }
}
