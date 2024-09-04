using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using LibVLCSharp.Shared;
using Vao.Client;
using Vao.Client.Components;
using Vao.Sample.Properties;

namespace Vao.Sample
{
   public partial class MainWindow : Form
   {
      private bool mIsStared = false;
      private bool mIsCameraSelected = false;
      private bool mIsPlaybackStarted = false;
      private bool mApiSupportsPlayback = false;

      private VaoClient moVaoClient;
      private VideoViewWithViewerId mVideoControl;
      private Camera mCurrentCamera;
      private Button mCurrentCameraButton;
      private LibVLC mLibVlc;
      private ToolTip moToolTip;

      public VaoClient VaoClient
      {  get { return moVaoClient; } }

      public bool IsStared
      {
         get { return mIsStared; }
         set
         {
            mIsStared = value;
            UpdateEnabled();
         }
      }

      public bool ApiSupportsPlayback
      {
         get { return mApiSupportsPlayback; }
         set
         {
            mApiSupportsPlayback = value;
            UpdateEnabled();
         }
      }
      public bool IsPlaybackStarted
      {
         get { return mIsPlaybackStarted; }
         set
         {
            mIsPlaybackStarted = value;
            UpdateEnabled();
         }
      }

      public bool IsPlayback
      {
         get 
         {
            return IsUriPlaybackUri(txtCurrentRtspUrl.Text);
         }
      }

      public bool IsCameraSelected
      {
         get { return mIsCameraSelected; }
         set
         {
            mIsCameraSelected = value;
            UpdateEnabled();
         }
      }

      private bool IsUriPlaybackUri(string text)
      {
         if (text == null) return false;
         return text.Contains("playback");
      }

      public MainWindow()
      {
         InitializeComponent();
         
         // Updates the title bar to dark.
         DarkModeHelper.EnableImmersiveDarkMode(Handle);

         StartInitializeVlc();

         ClearPresetDropdown();
         ClearRecordingDropdown();
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

         InitVideoControl();
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

         if (!string.IsNullOrEmpty(Settings.Default.Password))
            txtPassword.Text = Settings.Default.Password;

         // Select password input control after loading settings.
         txtPassword.Select();
      }

      private void SaveSettings()
      {
         Settings.Default.Host1 = txtHost.Text;
         Settings.Default.User = txtUser.Text;
         Settings.Default.ApiPort = txtPort.Text;
         Settings.Default.Password = txtPassword.Text;
         if (CurrentCamera != null)
            Settings.Default.CurrentCamera = CurrentCamera.ComponentNumber;
         Settings.Default.Save();
      }

      public void UpdateEnabled()
      {
         btnDisconnect.Enabled = IsStared;
         grpCameraSelection.Enabled = IsStared;
         chkPreferSubChannel.Enabled = IsStared && !IsPlayback;
         grpSelectPreset.Enabled = IsStared;
         grpSelectPlayback.Enabled = IsStared && ApiSupportsPlayback;

         grpCameraControl.Enabled = IsStared && CurrentCamera != null;

         btnStopPlayback.Enabled = IsStared && IsPlayback && ApiSupportsPlayback;
         
         btnPlayPlayback.Enabled = IsStared && IsCameraSelected && !IsPlaybackStarted && ApiSupportsPlayback;
         btnGotoTime.Enabled = IsStared && IsCameraSelected && ApiSupportsPlayback;
         grpSelectPlayback.Enabled = IsStared && IsCameraSelected && ApiSupportsPlayback;
         btnConnect.Enabled = !IsStared;
         txtHost.Enabled = !IsStared;
         txtPassword.Enabled = !IsStared;
         txtPort.Enabled = !IsStared;
         txtUser.Enabled = !IsStared;
         chkSecure.Enabled = !IsStared;
         btnDownload.Enabled = IsStared && ApiSupportsPlayback;

         UpdateCameraControl();
      }

      private void btnConnect_Click(object sender, EventArgs e)
      {
         SaveSettings();

         if (!ValidateCanConnect())
            return;

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
         txtVideoHeader.Text = $"No Camera Selected";
         grpVideoControl.BackColor = Color.FromArgb(66, 77, 95);
         if (moVaoClient.StartClient())
         {
            WriteMessageLog("VaoAPI", "Client started.", LogLevel.Notice);
            FillSelectCameraButtonList();
            CheckApiVersion();
            ClearRecordingDropdown();
            ClearPresetDropdown();
            UpdateEnabled();

            if (Settings.Default.CurrentCamera != 0)
               SelectCamera(Settings.Default.CurrentCamera, chkPreferSubChannel.Checked ? 2 : 1);
         }
         else
         {
            WriteMessageLog("VaoAPI", "Unable to start, no response.", LogLevel.Error);
            btnDisconnect_Click(sender, e);
            ClearPresetDropdown();
            UpdateEnabled();
         }
      }

      /// <summary>
      /// Checks if it will be possible to try to connect.
      /// </summary>
      /// <returns></returns>
      private bool ValidateCanConnect()
      {
         if (string.IsNullOrWhiteSpace(txtHost.Text))
         {
            WriteMessageLog("Application", "Missing host name", LogLevel.Error);
            return false;
         }

         if (string.IsNullOrWhiteSpace(txtPassword.Text))
         {
            WriteMessageLog("Application", "Missing host password", LogLevel.Error);
            return false; 
         }

         if (string.IsNullOrWhiteSpace(txtUser.Text))
         {
            WriteMessageLog("Application", "Missing user name", LogLevel.Error);
            return false;
         }

         if (string.IsNullOrWhiteSpace(txtPort.Text))
         {
            WriteMessageLog("Application", "Missing port", LogLevel.Error);
            return false;
         }

         return true;
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
            // Since this is async we might get here after the message contron is disposed. (When application is closing)
            if (lstMessages.IsDisposed == true)
               return;

            var dlvi = new DarkUI.Controls.DarkListItem();
            var strTime = DateTime.Now.ToString(CultureInfo.InvariantCulture);
            var strLevel = level.ToString();
            var strMsg = $"{strTime} [{strLevel}] - {strSource} - {strMessage}";
            dlvi.Text = strMsg;

            if (strMessage != "drawable Warning: unsupported control query 3")
            {
               lstMessages.Items.Add(dlvi);
            }       
         }
      }

      private void btnDisconnect_Click(object sender, EventArgs e)
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
         txtCurrentRtspUrl.Text = string.Empty;
         txtVideoHeader.Text = "No Camera Selected";
         grpVideoControl.BackColor = Color.FromArgb(66, 77, 95);
         IsCameraSelected = false;
         ClearPresetDropdown();
         ClearRecordingDropdown();

      }

      private void StopRtspStream()
      {
         if (mVideoControl?.MediaPlayer != null)
         {
            var mp = mVideoControl.MediaPlayer;
            mVideoControl.MediaPlayer = null;
            mIsVideoStarted = false;
            DisposeMediaPlayerAsync(mp);
         }
      }

      private static void DisposeMediaPlayerAsync(MediaPlayer mp)
      {
         if (mp == null)
            return;

         Task.Run(() =>
         {
            if (mp.IsPlaying)
               mp.Stop();
            mp.Dispose();
         });
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
               IsCameraSelected = true;
               FillSelectPresetList();
               if (ApiSupportsPlayback)
               {
                  FillPlaybackSelectionList();
               }

               Settings.Default.CurrentCamera = mCurrentCamera.ComponentNumber;
            }
            else
            {
               ClearPresetDropdown();
               ClearRecordingDropdown();
            }

            UpdateEnabled();
         }
      }

      private void ClearRecordingDropdown()
      {
         selPlayback.DataSource = null;
         selPlayback.Items.Clear();
         if (!ApiSupportsPlayback && IsStared)
         {
            selPlayback.Items.Add("Api Version does not support playback");
         }
         else
         {
            selPlayback.Items.Add("No camera selected");
         }
         selPlayback.SelectedIndex = 0;
      }

      private void ClearPresetDropdown()
      {
         lblCurrentCamera.Text = $"Camera : (No camera selected)";
         selPreset.DataSource = null;
         selPreset.Items.Clear();
         selPreset.Items.Add("No camera selected");
         selPreset.SelectedIndex = 0;
      }
      protected override void OnVisibleChanged(EventArgs e)
      {
         base.OnVisibleChanged(e);
         if (Visible)
         {
            // Bugfix due to DarkUI libary not invalidating the controls when the windows appears on screen.
            selPreset.Invalidate();
            selPlayback.Invalidate();
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
         if (presets != null)
         {
            if (presets.Count > 0)
            {
               selPreset.DisplayMember = nameof(Preset.Name);
               selPreset.DataSource = presets;               
               selPreset.SelectedIndex = 1;
            }
            else
            {
               selPreset.DataSource = null;
               selPreset.Items.Clear();
               selPreset.Items.Add("No Presets");
               selPreset.SelectedIndex = 0;
            }
         }
         else
         {
            selPreset.DataSource = null;
            selPreset.Items.Clear();
            selPreset.Items.Add("No Presets");
            selPreset.SelectedIndex = 0;
         }
      }

      private void FillPlaybackSelectionList()
      {
         List<PlaybackInfo> playbackInfoList = CurrentCamera?.GetPlaybackInfoList(mVideoControl.ViewerID);

         if (playbackInfoList != null)
         {
            if (playbackInfoList.Count > 0)
            {
               selPlayback.DataSource = playbackInfoList;
               selPlayback.DisplayMember = nameof(PlaybackInfo.RecorderAddress);
            }
            else
            {
               selPlayback.DataSource = null;
               selPlayback.Items.Clear();
               selPlayback.Items.Add("No Recorders");
               selPlayback.SelectedIndex = 0;
            }
         }
         else
         {
            selPlayback.DataSource = null;
            selPlayback.Items.Clear();
            selPlayback.Items.Add("No Recorders");
            selPlayback.SelectedIndex = 0;
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
               txtVideoHeader.Text = $"LIVE - Camera {cameraNo}";
               grpVideoControl.BackColor = Color.FromArgb(65, 142, 62);
               StartRtspStream(url);
            }
         }
      }

      private void CheckApiVersion()
      {
         ApiVersion apiversion = moVaoClient.GetApiVersion();
         if (apiversion != null)
         {
            Version version = new Version(apiversion.MajorVersion, apiversion.MinorVersion);
            if (version >= new Version(1,1))
            {
               ApiSupportsPlayback = true;
            }
         }
         UpdateEnabled();
      }

      private static string GetMaskedUrl(string url)
      {
         UriBuilder maskedUri = new UriBuilder(url);
         maskedUri.Password = "******";
         maskedUri.UserName = "******";
         maskedUri.ToString();
         return maskedUri.ToString();         
      }

      bool mIsVideoStarted = false;

      private void StartRtspStream(string rtspUrl)
      {
         if (mIsVideoStarted)
         {
            // Stop the stream if active.
            StopRtspStream();
         }

         // Start the stream.
         var uri = new Uri(rtspUrl);
         if (mVideoControl.MediaPlayer == null)
         {
            var media = new Media(mLibVlc, uri);
            mVideoControl.MediaPlayer = new MediaPlayer(media);
            mVideoControl.MediaPlayer.EncounteredError += MediaPlayer_EncounteredError;
            mVideoControl.MediaPlayer.Opening += MediaPlayer_Opening;
            
            mVideoControl.MediaPlayer.Play();
            mIsVideoStarted = true;
         }
         else
         {
            var media = new Media(mLibVlc, uri);
            mVideoControl.MediaPlayer.Play(media);
            mIsVideoStarted = true;
         }


      }

      private void InitVideoControl()
      {
         // Add the control to the WinForm.
         if (mVideoControl == null)
         {
            var libVlc = new VideoViewWithViewerId();
            pnlVideo.Controls.Add(libVlc);
            libVlc.Dock = DockStyle.Fill;
            libVlc.BringToFront();
            mVideoControl = libVlc;
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
               Button oButton = new DarkUI.Controls.DarkButton()
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
            if ((sender == btnPanLeft) || (sender == btnPanRight) || 
                (sender == btnTiltUp) || (sender == btnTiltDown) || 
                (sender == btnZoomIn) || (sender == btnZoomOut) ||
                (sender == btnFocusFar) || (sender == btnFocusNear))

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
      private void btnPlayPlayback_Click(object sender, EventArgs e)
      {
         if (selPlayback.SelectedItem is PlaybackInfo recording && recording != null)
         {
            string url = recording.PlaybackUrl;
            if (!string.IsNullOrEmpty(url))
            {
               txtCurrentRtspUrl.Text = GetMaskedUrl(url);

               var cameraNo = CurrentCamera?.ComponentNumber ?? 0;
               txtVideoHeader.Text = $"PLAYBACK - Camera {cameraNo}";
               grpVideoControl.BackColor = Color.FromArgb(142, 62, 62);

               StartRtspStream(url);
               IsPlaybackStarted = true;
               UpdateEnabled();
            }
         }
      }
      private void btnGotoTime_Click(object sender, EventArgs e)
      {
         if (selPlayback.SelectedItem is PlaybackInfo recording && recording != null)
         {
            if (dateTimePicker1.Text != null)
            {
               try 
               {
                  DateTime dateTimeObject = DateTime.ParseExact(dateTimePicker1.Text, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                  string startTimeParameter = $"?start={dateTimeObject.ToString("yyyyMMddHHmmss")}";
                  string url = recording.PlaybackUrl;

                  if (!string.IsNullOrEmpty(url))
                  {
                     string urlWithStartTime = url + startTimeParameter;
                     txtCurrentRtspUrl.Text = GetMaskedUrl(url);
                     var cameraNo = CurrentCamera?.ComponentNumber ?? 0;
                     txtVideoHeader.Text = $"PLAYBACK - Camera {cameraNo}";
                     grpVideoControl.BackColor = Color.FromArgb(142, 62, 62);
                     StartRtspStream(urlWithStartTime);
                     IsPlaybackStarted = true;
                     UpdateEnabled();
                  }
               }
               catch (Exception)
               {
               }
            }
         }
      }

      private void btnStopPlayback_Click(object sender, EventArgs e)
      {
         IsStared = false;
         StopRtspStream();
         SelectCamera(CurrentCamera.ComponentNumber, chkPreferSubChannel.Checked ? 2 : 1);
         IsStared = true;
         IsPlaybackStarted = false;
         UpdateEnabled();
      }

      private void selPlayback_SelectedIndexChanged(object sender, EventArgs e)
      {
         if (selPlayback.SelectedItem is PlaybackInfo recording && recording != null)
         {
            try
            {
               TimeSpan timeSpan = XmlConvert.ToTimeSpan(recording.RecordingLimit);
               dateTimePicker1.MinDate = DateTime.UtcNow.Add(-timeSpan);
               dateTimePicker1.MaxDate = DateTime.UtcNow;

               dateTimePicker1.Invalidate();
               dateTimePicker1.Update();
            }
            catch (Exception)
            {
            }
         }
      }

      private void btnOpenDownloadWindow_Click(object sender, EventArgs e)
      {
         using (DownloadWindow downloadWindow = new DownloadWindow(VaoClient))
         {
            downloadWindow.StartPosition = FormStartPosition.CenterParent;
            downloadWindow.Icon = this.Icon;
            downloadWindow.ShowDialog(this);
         }
      }
   }
}
