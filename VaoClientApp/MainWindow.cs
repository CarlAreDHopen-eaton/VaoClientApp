using System;
using System.Globalization;
using System.IO;
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

      }

      private void StartInitializeVlc()
      {
         WriteMessageLog("LibVLC", "Loading VLC");
         var options = new[] { "-vv", "--rtsp-timeout=300", "--network-caching=300" };
         mLibVlc = new LibVLC(true, options);
         mLibVlc.Log += LibVlc_Log;

         //TODO Check if not needed anymore.
         //DirectoryInfo vlcLibDirectory = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "libvlc", IntPtr.Size == 4 ? "win-x86" : "win-x64"));
         //libVlc.VlcLibDirectory = vlcLibDirectory;
      }

      private void LibVlc_Log(object sender, LogEventArgs e)
      {
         // Ignore debug log.
         if (e.Level == LogLevel.Debug)
            return;

         WriteMessageLog("LibVLC", e.FormattedLog);
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
         moVaoClient.StartClient();

         var status = moVaoClient.GetVaoStatus();
         if (status != null)
         {
            WriteMessageLog("VaoAPI", status);
            FillSelectCameraButtonList();
            moVaoClient.StartStatusThread();
         }
         else
         {
            WriteMessageLog("VaoAPI", "Unable to start.");
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
         WriteMessageLog("VaoAPI", e.Message);
      }

      private void WriteMessageLog(string strSource, string strMessage)
      {
         if (InvokeRequired)
         {
            BeginInvoke(new MethodInvoker(() => WriteMessageLog(strSource, strMessage)));
         }
         else
         {
            var lvi = new ListViewItem(DateTime.Now.ToString(CultureInfo.InvariantCulture));
            lvi.SubItems.Add($"{strSource} - {strMessage}");
            lstMessages.Items.Add(lvi);
         }
      }

      private void btnStop_Click(object sender, EventArgs e)
      {
         IsStared = false;
         if (moVaoClient != null)
         {
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
            mCurrentCamera = value;
            if (mCurrentCamera != null)
               lblCurrentCamera.Text = $"Camera : {mCurrentCamera.Name}";
            else
               lblCurrentCamera.Text = $"Camera : (No camera selected)";
            UpdateEnabled();
         }
      }

      private void SelectCamera(int cameraNo, int streamNo)
      {
         Camera camera = moVaoClient.GetVaoCamera(cameraNo);
         CurrentCamera = camera;
         string url = camera.GetCameraLiveStreamUrl(streamNo);
         if (!string.IsNullOrEmpty(url))
         {
            txtCurrentRtspUrl.Text = url;

            StartRtspStream(url);
         }
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
         WriteMessageLog("LibVLC", "LibVLC error encountered.");
      }

      private void MediaPlayer_Opening(object sender, EventArgs e)
      {
         string mrl = mVideoControl?.MediaPlayer?.Media?.Mrl ?? "";
         WriteMessageLog("LibVLC", $"LibVLC opening {mrl}");
      }

      private void OnSelectCameraClicked(object sender, EventArgs e)
      {
         if (sender is Button button)
         {
            if (button.Tag is Camera camera)
            {
               SelectCamera(camera.CameraNumber, chkPreferSubChannel.Checked ? 2 : 1);
            }
         }
      }

      private void FillSelectCameraButtonList()
      {
         var cameraList = moVaoClient.GetVaoCameras();
         if (cameraList != null)
         {
            pnlCameraSelectFlowPanel.Controls.Clear();
            foreach (var camera in cameraList)
            {
               Button oButton = new Button()
               {
                  // ReSharper disable once LocalizableElement
                  Text = "Cam " + camera.CameraNumber,
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
            if (sender == btnPanRight)
               currentCamera.PanRight(80);
            if (sender == btnTiltUp)
               currentCamera.TiltUp(80);
            if (sender == btnTiltDown)
               currentCamera.TiltDown(80);
            if (sender == btnZoomIn)
               currentCamera.ZoomIn(80);
            if (sender == btnZoomOut)
               currentCamera.ZoomOut(80);

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
               SelectCamera(CurrentCamera.CameraNumber, chkPreferSubChannel.Checked ? 2 : 1);
            }
         }
      }
   }
}
