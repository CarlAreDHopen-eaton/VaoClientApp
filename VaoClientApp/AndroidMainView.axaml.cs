using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.Media;
using Vao.Client;
using Vao.Client.Components;

namespace Vao.Sample;

public partial class AndroidMainView : UserControl
{
   private FlexRApiClient mApiClient;
   private bool mIsConnecting;
   private bool mIsConnected;

   public AndroidMainView()
   {
      InitializeComponent();
      LoadSettingsIntoForm();
   }

   private void LoadSettingsIntoForm()
   {
      var s = AppSettings.Default;
      var conn = s.GetSelectedConnectionAlternative();

      var txtHost = this.FindControl<TextBox>("txtHost");
      var txtPort = this.FindControl<TextBox>("txtPort");
      var txtUser = this.FindControl<TextBox>("txtUser");
      var txtPassword = this.FindControl<TextBox>("txtPassword");
      var chkUseHttps = this.FindControl<CheckBox>("chkUseHttps");

      if (txtHost != null) txtHost.Text = conn.Host ?? "";
      if (txtPort != null) txtPort.Text = conn.Port ?? "444";
      if (txtUser != null) txtUser.Text = s.User ?? "";
      if (txtPassword != null) txtPassword.Text = s.Password ?? "";
      if (chkUseHttps != null) chkUseHttps.IsChecked = s.UseHttps;
   }

   private async void btnConnect_Click(object sender, RoutedEventArgs e)
   {
      if (mIsConnecting || mIsConnected)
         return;

      var txtHost = this.FindControl<TextBox>("txtHost");
      var txtPort = this.FindControl<TextBox>("txtPort");
      var txtUser = this.FindControl<TextBox>("txtUser");
      var txtPassword = this.FindControl<TextBox>("txtPassword");
      var chkUseHttps = this.FindControl<CheckBox>("chkUseHttps");
      var btnConnect = this.FindControl<Button>("btnConnect");
      var txtConnectStatus = this.FindControl<TextBlock>("txtConnectStatus");

      var host = txtHost?.Text?.Trim() ?? "";
      var port = string.IsNullOrWhiteSpace(txtPort?.Text) ? "444" : txtPort.Text.Trim();
      var user = txtUser?.Text?.Trim() ?? "";
      var password = txtPassword?.Text ?? "";
      var useHttps = chkUseHttps?.IsChecked == true;

      if (string.IsNullOrWhiteSpace(host))
      {
         SetStatus(txtConnectStatus, "Host is required.", isError: true);
         return;
      }

      if (string.IsNullOrWhiteSpace(user))
      {
         SetStatus(txtConnectStatus, "Username is required.", isError: true);
         return;
      }

      mIsConnecting = true;
      if (btnConnect != null) btnConnect.IsEnabled = false;
      SetStatus(txtConnectStatus, $"Connecting to {host}:{port}...", isError: false);

      var s = AppSettings.Default;
      s.User = user;
      s.Password = password;
      s.UseHttps = useHttps;
      s.SetConnectionAlternatives(new[] { new ConnectionAlternative { Host = host, Port = port } });
      s.Save();

      bool started = false;
      List<Camera> cameraList = null;
      FlexRApiClient candidate = null;

      try
      {
         candidate = new FlexRApiClient
         {
            Host = host,
            Port = port,
            User = user,
            Password = password,
            UseHttps = useHttps,
            IgnoreCertificateErrors = true,
            ConnectionTimeoutMs = 10000
         };
         candidate.OnMessage += (_, args) =>
            Trace.WriteLine($"[VaoConnect] OnMessage [{args.Level}]: {args.Message}");

         await Task.Run(() =>
         {
            Trace.WriteLine($"[VaoConnect] Attempting StartClient host={host} port={port} https={useHttps}");
            started = candidate.StartClient();
            Trace.WriteLine($"[VaoConnect] StartClient returned: {started}");
            if (started)
            {
               cameraList = candidate.GetCameraList();
               Trace.WriteLine($"[VaoConnect] GetCameraList returned {cameraList?.Count ?? -1} cameras");
            }
         });
      }
      catch (Exception ex)
      {
         Trace.WriteLine($"[VaoConnect] Exception: {ex}");
         SetStatus(txtConnectStatus, $"Connection failed: {ex.Message}", isError: true);
         candidate?.StopClient();
         mIsConnecting = false;
         if (btnConnect != null) btnConnect.IsEnabled = true;
         return;
      }

      if (!started)
      {
         SetStatus(txtConnectStatus, "Could not connect. Check the host, port, and credentials.", isError: true);
         candidate?.StopClient();
         mIsConnecting = false;
         if (btnConnect != null) btnConnect.IsEnabled = true;
         return;
      }

      mApiClient = candidate;
      mIsConnected = true;
      mIsConnecting = false;
      PopulateCameraList(cameraList);
      TransitionToConnectedState(host, port);
   }

   private void btnDisconnect_Click(object sender, RoutedEventArgs e)
   {
      mApiClient?.StopClient();
      mApiClient = null;
      mIsConnected = false;
      TransitionToDisconnectedState();
   }

   private void lstCameras_SelectionChanged(object sender, SelectionChangedEventArgs e)
   {
      if (sender is ListBox lb && lb.SelectedItem is Camera camera)
      {
         var txtCameraTitle = this.FindControl<TextBlock>("txtCameraTitle");
         var txtCameraSubtitle = this.FindControl<TextBlock>("txtCameraSubtitle");
         var txtVideoPlaceholder = this.FindControl<TextBlock>("txtVideoPlaceholder");

         if (txtCameraTitle != null)
            txtCameraTitle.Text = camera.Name;
         if (txtCameraSubtitle != null)
            txtCameraSubtitle.Text = $"Camera {camera.ComponentNumber}";
         if (txtVideoPlaceholder != null)
            txtVideoPlaceholder.Text = $"Video for \"{camera.Name}\" — Android renderer coming next";
      }
   }

   private void PopulateCameraList(List<Camera> cameras)
   {
      var lstCameras = this.FindControl<ListBox>("lstCameras");
      if (lstCameras != null)
         lstCameras.ItemsSource = cameras ?? new List<Camera>();
   }

   private void TransitionToConnectedState(string host, string port)
   {
      var pnlDisconnect = this.FindControl<ScrollViewer>("pnlDisconnect");
      var pnlConnected = this.FindControl<Grid>("pnlConnected");
      var btnDisconnect = this.FindControl<Button>("btnDisconnect");
      var txtConnectionStatus = this.FindControl<TextBlock>("txtConnectionStatus");
      var ellConnectionDot = this.FindControl<Ellipse>("ellConnectionDot");

      if (pnlDisconnect != null) pnlDisconnect.IsVisible = false;
      if (pnlConnected != null) pnlConnected.IsVisible = true;
      if (btnDisconnect != null) btnDisconnect.IsVisible = true;
      if (txtConnectionStatus != null) txtConnectionStatus.Text = $"Connected · {host}:{port}";
      if (ellConnectionDot != null) ellConnectionDot.Fill = new SolidColorBrush(Color.Parse("#39B620"));
   }

   private void TransitionToDisconnectedState()
   {
      var pnlDisconnect = this.FindControl<ScrollViewer>("pnlDisconnect");
      var pnlConnected = this.FindControl<Grid>("pnlConnected");
      var btnDisconnect = this.FindControl<Button>("btnDisconnect");
      var btnConnect = this.FindControl<Button>("btnConnect");
      var txtConnectionStatus = this.FindControl<TextBlock>("txtConnectionStatus");
      var ellConnectionDot = this.FindControl<Ellipse>("ellConnectionDot");

      if (pnlDisconnect != null) pnlDisconnect.IsVisible = true;
      if (pnlConnected != null) pnlConnected.IsVisible = false;
      if (btnDisconnect != null) btnDisconnect.IsVisible = false;
      if (btnConnect != null) btnConnect.IsEnabled = true;
      if (txtConnectionStatus != null) txtConnectionStatus.Text = "Not connected";
      if (ellConnectionDot != null) ellConnectionDot.Fill = new SolidColorBrush(Color.Parse("#CA3C3D"));
   }

   private static void SetStatus(TextBlock label, string message, bool isError)
   {
      if (label == null) return;
      label.Text = message;
      label.IsVisible = !string.IsNullOrEmpty(message);
      label.Foreground = isError
         ? new SolidColorBrush(Color.Parse("#CA3C3D"))
         : new SolidColorBrush(Color.Parse("#99FFFFFF"));
   }
}
