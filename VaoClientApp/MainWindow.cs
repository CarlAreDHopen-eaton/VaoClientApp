using System;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using Vao.Client;
using Vao.Client.Components;
using Vao.Sample.Properties;
using Vlc.DotNet.Core;
using Vlc.DotNet.Forms;

namespace Vao.Sample
{
   public partial class MainWindow : Form
   {
      private bool mIsStared = false;
      private VaoClient moVaoClient;
      private VlcControl mVideoControl;
      private Camera mCurrentCamera;


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

         LoadSettings();
         UpdateEnabled();
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
         moVaoClient.OnError += OnVaoClientError;
         var status = moVaoClient.GetVaoStatus();
         if (status != null)
         {
            AddMessage(status);
            FillSelectCameraButtonList();
            //TODO Finish this.
            //moVaoClient.StartStatusThread();
         }
         else
         {
            AddMessage("Unable to start.");
            btnStop_Click(sender, e);
         }
      }

      private void OnVaoClientError(object sender, ConnectEventArgs e)
      {
         AddMessage(e.Message);
      }

      private void AddMessage(string strMessage)
      {
         if (InvokeRequired)
         {
            Invoke(new MethodInvoker(() => AddMessage(strMessage)));
         }
         else
         {
            var lvi = new ListViewItem(DateTime.Now.ToString(CultureInfo.InvariantCulture));
            lvi.SubItems.Add(strMessage);
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
         if (mVideoControl != null)
         {
            mVideoControl.Stop();
         }
         CurrentCamera = null;
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

            if (mVideoControl == null)
            {
               var libVlc = new VlcControl();

               libVlc.BeginInit();
               {
                  DirectoryInfo vlcLibDirectory = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "libvlc", IntPtr.Size == 4 ? "win-x86" : "win-x64"));
                  libVlc.VlcLibDirectory = vlcLibDirectory;
                  libVlc.VlcMediaplayerOptions = new[] { "-vv", "--rtsp-timeout=300", "--network-caching=300" };
                  libVlc.Opening += LibVlcOnOpening;
                  libVlc.EncounteredError += LibVlcOnEncounteredError;

                  pnlVideo.Controls.Add(libVlc);

                  libVlc.Dock = DockStyle.Fill;
                  libVlc.BringToFront();
               }
               libVlc.EndInit();
               mVideoControl = libVlc;
            }

            //mVideoControl.SetMedia(new Uri(url));
            var uri = new Uri(url);
            if (mVideoControl.IsPlaying)
               mVideoControl.Stop();
            mVideoControl.ResetMedia();
            mVideoControl.Play(uri);
         }
      }

      private void LibVlcOnEncounteredError(object sender, VlcMediaPlayerEncounteredErrorEventArgs e)
      {
         AddMessage("LibVLC error encountered.");
      }

      private void LibVlcOnOpening(object sender, VlcMediaPlayerOpeningEventArgs e)
      {
         AddMessage("LibVLC opening");
      }

      private void OnSelectCameraClicked(object sender, EventArgs e)
      {
         if (sender is Button button)
         {
            if (button.Tag is Camera camera)
            {
               SelectCamera(camera.CameraNumber, 1);
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
               currentCamera.PanLeft();
            if (sender == btnPanRight)
               currentCamera.PanRight();
            if (sender == btnTiltUp)
               currentCamera.TiltUp();
            if (sender == btnTiltDown)
               currentCamera.TiltDown();
         }
      }

      private void OnControlCameraMouseUp(object sender, MouseEventArgs e)
      {
         var currentCamera = CurrentCamera;
         if (currentCamera != null && sender is Button)
         {
            if ((sender == btnPanLeft) || (sender == btnPanRight) || (sender == btnTiltUp) || (sender == btnTiltDown))
                currentCamera.PanTiltStop();
         }
      }
   }
}
