﻿using System;
using System.IO;
using System.Windows.Forms;
using VaoClient;
using VaoClient.Components;
using Vlc.DotNet.Core;

namespace VaoClientApp
{
   public partial class MainWindow : Form
   {
      private bool mIsStared = false;
      private VaoClient.VaoClient moVaoClient;
      private Vlc.DotNet.Forms.VlcControl mVideoControl;
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

      private void btnStart_Click(object sender, System.EventArgs e)
      {
         IsStared = true;
         moVaoClient = new VaoClient.VaoClient
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
            var lvi = new ListViewItem(DateTime.Now.ToString());
            lvi.SubItems.Add(strMessage);
            lstMessages.Items.Add(lvi);
         }
      }

      private void btnStop_Click(object sender, System.EventArgs e)
      {
         IsStared = false;
         moVaoClient.StopClient();
         moVaoClient = null;
      }

      private void SelectCamera(int cameraNo, int streamNo = 1)
      {
         Camera camera = moVaoClient.GetVaoCamera(cameraNo);
         string url = camera.GetCameraLiveStreamUrl(streamNo);
         if (!string.IsNullOrEmpty(url))
         {
            textBox1.Text = url;

            if (mVideoControl == null)
            {
               var libVlc = new Vlc.DotNet.Forms.VlcControl();

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

      private void button1_Click(object sender, EventArgs e)
      {
         SelectCamera(1);
      }
      private void button2_Click(object sender, EventArgs e)
      {
         SelectCamera(2);
      }

      private void button3_Click(object sender, EventArgs e)
      {
         SelectCamera(3);

      }

      private void button6_Click(object sender, EventArgs e)
      {
         SelectCamera(4);
      }

      private void button5_Click(object sender, EventArgs e)
      {
         SelectCamera(5);
      }

      private void button4_Click(object sender, EventArgs e)
      {
         SelectCamera(6);
      }

      private void button9_Click(object sender, EventArgs e)
      {
         SelectCamera(7);
      }

      private void button8_Click(object sender, EventArgs e)
      {
         SelectCamera(8);
      }

      private void button7_Click(object sender, EventArgs e)
      {
         SelectCamera(9);
      }

      private void label3_Click(object sender, EventArgs e)
      {

      }
   }
}
