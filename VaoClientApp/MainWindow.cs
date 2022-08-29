using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using LibVLCSharp.Shared;
using LibVLCSharp.WinForms;
using Vao.Client;
using Vao.Client.Components;
using Vao.Sample.Properties;

namespace Vao.Sample
{
   public partial class MainWindow : Form
   {
      private bool mIsStared = false;
      private VaoClient moVaoClient;
      private VideoView mVideoControl;
      private Camera mCurrentCamera;
      private Button mCurrentCameraButton;
      private LibVLC mLibVlc;
      private ToolTip moToolTip;

      public bool IsStared
      {
         get { return mIsStared; }
         set
         {
            mIsStared = value;
            UpdateEnabled();
         }
      }

      public MainWindow()
      {
         InitializeComponent();

         StartInitializeVlc();

         LoadSettings();
         UpdateEnabled();

         moToolTip = new ToolTip();
         moToolTip.SetToolTip(btnPanLeft, "Pan Left");
         moToolTip.SetToolTip(btnPanRight, "Pan Right");
         moToolTip.SetToolTip(btnTiltUp, "Tilt Up");
         moToolTip.SetToolTip(btnTiltDown, "Tilt Down");

         moToolTip.SetToolTip(btnZoomIn, "Zoom In");
         moToolTip.SetToolTip(btnZoomOut, "Zoom Out");

         moToolTip.SetToolTip(btnFocusFar, "Focus Far");
         moToolTip.SetToolTip(btnFocusNear, "Focus Near");
         
         moToolTip.SetToolTip(btnClearMessages, "Clear message log");
      }

      private void StartInitializeVlc()
      {
         WriteMessageLog("LibVLC", "Loading VLC", LogLevel.Notice);
         var options = new[] { "-vv", "--rtsp-timeout=300", "--network-caching=300" };
         mLibVlc = new LibVLC(true, options);
         mLibVlc.Log += LibVlc_Log;
      }

      private void LibVlc_Log(object sender, LogEventArgs e)
      {
         // Ignore debug log.
         if (e.Level == LogLevel.Debug)
            return;

         WriteMessageLog("LibVLC", e.FormattedLog, e.Level);
      }

      /// <summary>
      /// Loads the configuration settings into the UI.
      /// </summary>
      private void LoadSettings()
      {
         if (!string.IsNullOrEmpty(Settings.Default.Host1))
            txtHost.Text = Settings.Default.Host1;

         if (!string.IsNullOrEmpty(Settings.Default.User))
            txtUser.Text = Settings.Default.User;

         if (!string.IsNullOrEmpty(Settings.Default.ApiPort))
            txtPort.Text = Settings.Default.ApiPort;

         // Select password input control after loading settings.
         txtPassword.Select();
      }

      private void SaveSettings()
      {
         Settings.Default.Host1 = txtHost.Text;
         Settings.Default.User = txtUser.Text;
         Settings.Default.ApiPort = txtPort.Text;
         Settings.Default.Save();
      }

      public void UpdateEnabled()
      {
         btnStop.Enabled = IsStared;
         grpCameraSelection.Enabled = IsStared;
         chkPreferSubChannel.Enabled = IsStared;

         grpCameraControl.Enabled = IsStared && CurrentCamera != null;

         btnStart.Enabled = !IsStared;
         txtHost.Enabled = !IsStared;
         txtPassword.Enabled = !IsStared;
         txtPort.Enabled = !IsStared;
         txtUser.Enabled = !IsStared;
         chkSecure.Enabled = !IsStared;

         UpdateCameraControl();
      }

      private void btnStart_Click(object sender, EventArgs e)
      {
         SaveSettings();
         IsStared = true;
         moVaoClient = new VaoClient
         {
            Host = txtHost.Text,
            Port = txtPort.Text,
            Password = txtPassword.Text,
            User = txtUser.Text,
            UseHttps = chkSecure.Checked,
            IgnoreCertificateErrors = true
         };
         moVaoClient.OnMessage += OnVaoClientMessage;
         if (moVaoClient.StartClient())
         {
            WriteMessageLog("VaoAPI", "Client started.", LogLevel.Notice);
            FillSelectCameraButtonList();
         }
         else
         {
            WriteMessageLog("VaoAPI", "Unable to start, no response.", LogLevel.Error);
            btnStop_Click(sender, e);
         }
         UpdateEnabled();
      }

      /// <summary>
      /// Event handler for the VAO client.
      /// Receives a message from the REST API and updates the message log control.
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void OnVaoClientMessage(object sender, MessageEventArgs e)
      {
         WriteMessageLog("VaoAPI", e.Message, LogLevel.Notice);
      }

      private void WriteMessageLog(string strSource, string strMessage, LogLevel level)
      {
         if (InvokeRequired)
         {
            BeginInvoke(new MethodInvoker(() => WriteMessageLog(strSource, strMessage, level)));
         }
         else
         {
            var lvi = new ListViewItem(DateTime.Now.ToString(CultureInfo.InvariantCulture));
            lvi.SubItems.Add(level.ToString());
            lvi.SubItems.Add($"{strSource} - {strMessage}");
            lstMessages.Items.Add(lvi);
         }
      }

      private void btnStop_Click(object sender, EventArgs e)
      {
         IsStared = false;
         if (moVaoClient != null)
         {
            moVaoClient.OnMessage -= OnVaoClientMessage;
            moVaoClient.StopClient();
            moVaoClient = null;
         }
         StopRtspStream();
         CurrentCamera = null;
      }

      private void StopRtspStream()
      {
         if (mVideoControl?.MediaPlayer != null)
         {
            mVideoControl.MediaPlayer.Stop();
         }
      }

      private Camera CurrentCamera 
      {
         get
         {
            return mCurrentCamera;
         }
         set
         {
            if (mCurrentCameraButton != null)
               mCurrentCameraButton.BackColor = Color.White;

            if (mCurrentCamera != null)
               mCurrentCamera.PropertyChanged -= Camera_PropertyChanged;

            mCurrentCamera = value;
            
            if (mCurrentCamera != null)
               mCurrentCamera.PropertyChanged += Camera_PropertyChanged;

            mCurrentCameraButton = GetCameraButton(mCurrentCamera);
            if (mCurrentCameraButton != null)
               mCurrentCameraButton.BackColor = Color.Goldenrod;

            if (mCurrentCamera != null)
            {
               lblCurrentCamera.Text = $"Camera : {mCurrentCamera.Name}";
               FillSelectPresetList();
            }
            else
            {
               lblCurrentCamera.Text = $"Camera : (No camera selected)";
               selPreset.Items.Clear();
               selPreset.Items.Add("No Preset");
            }
            
            UpdateEnabled();
         }
      }

      private void Camera_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
      {
         UpdateCameraControl();
      }

      private void UpdateCameraControl()
      {
         btnPanLeft.Enabled = CurrentCamera?.HasPanTiltControl ?? false;
         btnPanRight.Enabled = CurrentCamera?.HasPanTiltControl ?? false;
         btnTiltDown.Enabled = CurrentCamera?.HasPanTiltControl ?? false;
         btnTiltUp.Enabled = CurrentCamera?.HasPanTiltControl ?? false;

         btnZoomIn.Enabled = CurrentCamera?.HasLensControl ?? false;
         btnZoomOut.Enabled = CurrentCamera?.HasLensControl ?? false;
         btnFocusFar.Enabled = CurrentCamera?.HasLensControl ?? false;
         btnFocusNear.Enabled = CurrentCamera?.HasLensControl ?? false;
      }

      private void FillSelectPresetList()
      {
         List<Preset> presets = CurrentCamera?.PresetList;
         if (presets != null && presets.Count > 0)
         {
            selPreset.DataSource = presets;
            selPreset.DisplayMember = nameof(Preset.Name);
         }
      }

      private Button GetCameraButton(Camera camera)
      {
         foreach (Button button in pnlCameraSelectFlowPanel.Controls)
         { 
            if (button.Tag == camera)
               return button;
         }
         return null;
      }

      private void SelectCamera(int cameraNo, int streamNo)
      {
         Camera camera = moVaoClient.GetCamera(cameraNo);
         if (camera != null)
         {
            CurrentCamera = camera;
            string url = camera.GetCameraLiveStreamUrl(streamNo);
            if (!string.IsNullOrEmpty(url))
            {
               txtCurrentRtspUrl.Text = GetMaskedUrl(url);
               StartRtspStream(url);
            }
         }
      }

      private static string GetMaskedUrl(string url)
      {
         UriBuilder maskedUri = new UriBuilder(url);
         maskedUri.Password = "******";
         maskedUri.UserName = "******";
         maskedUri.ToString();
         return maskedUri.ToString();         
      }

      private void StartRtspStream(string rtspUrl)
      {
         // Add the control to the WinForm.
         if (mVideoControl == null)
         {
            var libVlc = new VideoView();
            pnlVideo.Controls.Add(libVlc);
            libVlc.Dock = DockStyle.Fill;
            libVlc.BringToFront();
            mVideoControl = libVlc;
         }

         // Stop the stream if active.
         StopRtspStream();

         // Start the stream.
         var uri = new Uri(rtspUrl);
         if (mVideoControl.MediaPlayer == null)
         {
            var media = new Media(mLibVlc, uri);
            mVideoControl.MediaPlayer = new MediaPlayer(media);
            mVideoControl.MediaPlayer.EncounteredError += MediaPlayer_EncounteredError;
            mVideoControl.MediaPlayer.Opening += MediaPlayer_Opening;
            mVideoControl.MediaPlayer.Play();
         }
         else
         {
            var media = new Media(mLibVlc, uri);
            mVideoControl.MediaPlayer.Play(media);
         }
      }

      private void MediaPlayer_EncounteredError(object sender, EventArgs e)
      {
         WriteMessageLog("LibVLC", "LibVLC error encountered.", LogLevel.Error);
      }

      private void MediaPlayer_Opening(object sender, EventArgs e)
      {
         string mrl = mVideoControl?.MediaPlayer?.Media?.Mrl ?? "";
         WriteMessageLog("LibVLC", $"LibVLC opening {GetMaskedUrl(mrl)}", LogLevel.Notice);
      }

      private void OnSelectCameraClicked(object sender, EventArgs e)
      {
         if (sender is Button button)
         {
            if (button.Tag is Camera camera)
            {
               SelectCamera(camera.ComponentNumber, chkPreferSubChannel.Checked ? 2 : 1);
            }
         }
      }

      private void FillSelectCameraButtonList()
      {
         var cameraList = moVaoClient.GetCameraList();
         if (cameraList != null)
         {
            pnlCameraSelectFlowPanel.Controls.Clear();
            foreach (var camera in cameraList)
            {
               Button oButton = new Button()
               {
                  // ReSharper disable once LocalizableElement
                  Text = "Cam " + camera.ComponentNumber,
                  Height = 30,
                  Width = 60,
                  Tag = camera
               };
               oButton.Click += OnSelectCameraClicked;
               moToolTip.SetToolTip(oButton, camera.Name);
               pnlCameraSelectFlowPanel.Controls.Add(oButton);
            }
         }
      }

      private void OnControlCameraMouseDown(object sender, MouseEventArgs e)
      {
         var currentCamera = CurrentCamera;
         if (currentCamera != null)
         {
            if (sender == btnPanLeft)
               currentCamera.PanLeft(80);
            else if (sender == btnPanRight)
               currentCamera.PanRight(80);
            else if (sender == btnTiltUp)
               currentCamera.TiltUp(80);
            else if (sender == btnTiltDown)
               currentCamera.TiltDown(80);
            else if (sender == btnZoomIn)
               currentCamera.ZoomIn(80);
            else if (sender == btnZoomOut)
               currentCamera.ZoomOut(80);
            else if (sender == btnFocusFar)
               currentCamera.FocusFar();
            else if (sender == btnFocusNear)
               currentCamera.FocusNear();
         }
      }

      private void OnControlCameraMouseUp(object sender, MouseEventArgs e)
      {
         var currentCamera = CurrentCamera;
         if (currentCamera != null && sender is Button)
         {
            if ((sender == btnPanLeft) || (sender == btnPanRight) || (sender == btnTiltUp) || (sender == btnTiltDown) || (sender == btnZoomIn) || (sender == btnZoomOut))
                currentCamera.PanTiltZoomStop();
         }
      }

      private void chkPreferSubChannel_CheckedChanged(object sender, EventArgs e)
      {
         if (IsStared)
         {
            if (CurrentCamera != null)
            {
               SelectCamera(CurrentCamera.ComponentNumber, chkPreferSubChannel.Checked ? 2 : 1);
            }
         }
      }

      private void btnClearMessages_Click(object sender, EventArgs e)
      {
         lstMessages.Items.Clear();
      }

      private void btnGotoPreset_Click(object sender, EventArgs e)
      {
         if (selPreset.SelectedItem is Preset preset)
         {
            preset.GotoPreset();
         }
      }
   }
}
