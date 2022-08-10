using System;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using Vao.Client;
using Vao.Client.Components;
using Vlc.DotNet.Core;
using Vlc.DotNet.Forms;

namespace Vao.Sample
{
   public partial class MainWindow : Form
   {
      private bool mIsStared = false;
      private VaoClient moVaoClient;
      private VlcControl mVideoControl;
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
         
         UpdateEnabled();
      }

      public void UpdateEnabled()
      {
         btnStop.Enabled = IsStared;
         grpCameraSelection.Enabled = IsStared;

         btnStart.Enabled = !IsStared;
         txtHost.Enabled = !IsStared;
         txtPassword.Enabled = !IsStared;
         txtPort.Enabled = !IsStared;
         txtUser.Enabled = !IsStared;
         chkSecure.Enabled = !IsStared; 
      }

      private void btnStart_Click(object sender, EventArgs e)
      {
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
         }
         else
         {
            AddMessage("Some error.");
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
         moVaoClient.StopClient();
         moVaoClient = null;
      }

      private void SelectCamera(int cameraNo, int streamNo)
      {
         Camera camera = moVaoClient.GetVaoCamera(cameraNo);
         string url = camera.GetCameraLiveStreamUrl(streamNo);
         if (!string.IsNullOrEmpty(url))
         {
            textBox1.Text = url;

            if (mVideoControl == null)
            {
               var libVlc = new VlcControl();

               libVlc.BeginInit();
               {
                  DirectoryInfo vlcLibDirectory = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "libvlc", IntPtr.Size == 4 ? "win-x86" : "win-x64"));
                  libVlc.VlcLibDirectory = vlcLibDirectory;
                  libVlc.VlcMediaplayerOptions = new[] { "-vv" };
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
            mVideoControl.Play(uri);
         }
      }

      private void LibVlcOnEncounteredError(object sender, VlcMediaPlayerEncounteredErrorEventArgs e)
      {
         AddMessage(e.ToString());
      }

      private void LibVlcOnOpening(object sender, VlcMediaPlayerOpeningEventArgs e)
      {
         AddMessage(e.ToString());
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
            flowLayoutPanel1.Controls.Clear();
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
               flowLayoutPanel1.Controls.Add(oButton);
            }
         }
      }
   }
}
