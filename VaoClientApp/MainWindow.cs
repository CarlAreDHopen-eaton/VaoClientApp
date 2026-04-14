using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using DarkUI.Controls;
using LibVLCSharp.Shared;
using Vao.Client;
using Vao.Client.Components;
using Vao.Client.Enum;
using Vao.Sample.Properties;

namespace Vao.Sample
{
   public partial class MainWindow : Form
   {
      private bool mIsStared = false;
      private bool mIsCameraSelected = false;
      private bool mIsPlaybackStarted = false;
      private bool mApiSupportsPlayback = false;

      private FlexRApiClient moFlexRApiClient;
      private VideoViewWithViewerId mVideoControl;
      private Camera mCurrentCamera;
      private Alarm mCurrentAlarm;
      private User mCurrentLoggedInUser;
      private Button mCurrentCameraButton;
      private LibVLC mLibVlc;
      private ToolTip moToolTip;

      public FlexRApiClient FlexRApiClient
      {  get { return moFlexRApiClient; } }

      public bool IsStarted
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

         foreach (CollapsibleDarkSectionPanel panel in splitMainVerticalSplit.Panel1.Controls
                     .OfType<CollapsibleDarkSectionPanel>())
         {
            panel.CollapseStateChanged += Panel_CollapseStateChanged;
         }
         RelayoutPanels();

         Text = $"{Text} v{Application.ProductVersion}";
         
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

         chkUseTcp.Checked = Settings.Default.UseTcp;
         chkPreferSubChannel.Checked = Settings.Default.PreferSubChannel;
         chkSecure.Checked = Settings.Default.UseHttps;

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
         Settings.Default.UseTcp = chkUseTcp.Checked;
         Settings.Default.PreferSubChannel = chkPreferSubChannel.Checked;
         Settings.Default.UseHttps = chkSecure.Checked;

         Settings.Default.Save();
      }

      public void UpdateEnabled()
      {
         btnDisconnect.Enabled = IsStarted;
         grpCameraSelection.Enabled = IsStarted;
         chkPreferSubChannel.Enabled = IsStarted && !IsPlayback;
         grpSelectPreset.Enabled = IsStarted;
         grpSelectPlayback.Enabled = IsStarted && ApiSupportsPlayback;

         grpCameraControl.Enabled = IsStarted && CurrentCamera != null;

         btnStopPlayback.Enabled = IsStarted && IsPlayback && ApiSupportsPlayback;
         
         btnPlayPlayback.Enabled = IsStarted && IsCameraSelected && !IsPlaybackStarted && ApiSupportsPlayback;
         btnGotoTime.Enabled = IsStarted && IsCameraSelected && ApiSupportsPlayback;
         grpSelectPlayback.Enabled = IsStarted && IsCameraSelected && ApiSupportsPlayback;
         btnConnect.Enabled = !IsStarted;
         txtHost.Enabled = !IsStarted;
         txtPassword.Enabled = !IsStarted;
         txtPort.Enabled = !IsStarted;
         txtUser.Enabled = !IsStarted;
         chkSecure.Enabled = !IsStarted;
         btnDownload.Enabled = IsStarted && ApiSupportsPlayback;

         UpdateCameraControl();
      }

      private void btnConnect_Click(object sender, EventArgs e)
      {
         SaveSettings();

         if (!ValidateCanConnect())
            return;

         IsStarted = true;
         moFlexRApiClient = new FlexRApiClient
         {
            Host = txtHost.Text,
            Port = txtPort.Text,
            Password = txtPassword.Text,
            User = txtUser.Text,
            UseHttps = chkSecure.Checked,
            IgnoreCertificateErrors = true
         };
         moFlexRApiClient.OnMessage += OnFlexRApiClientMessage;
         txtVideoHeader.Text = $"No Camera Selected";
         grpVideoControl.BackColor = Color.FromArgb(66, 77, 95);
         if (moFlexRApiClient.StartClient())
         {
            WriteMessageLog("VaoAPI", "Client started.", LogLevel.Notice);
            SetCurrentLoggedInUser();
            FillSelectCameraButtonList();
            FillSelectAlarmButtonList();
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
      private void OnFlexRApiClientMessage(object sender, MessageEventArgs e)
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
         IsStarted = false;
         if (moFlexRApiClient != null)
         {
            moFlexRApiClient.OnMessage -= OnFlexRApiClientMessage;
            moFlexRApiClient.StopClient();
            moFlexRApiClient = null;
         }
         StopRtspStream();
         CurrentCamera = null;
         CurrentAlarm = null;
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
            {
               mCurrentCamera.PropertyChanged -= Camera_PropertyChanged;
               mCurrentCamera.LockStatusChanged -= Camera_LockStatusChanged;
            }

            mCurrentCamera = value;

            if (mCurrentCamera != null)
            {
               mCurrentCamera.PropertyChanged += Camera_PropertyChanged;
               mCurrentCamera.LockStatusChanged += Camera_LockStatusChanged;
            }
               

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

      private Alarm CurrentAlarm
      {
         get
         {
            return mCurrentAlarm;
         }
         set
         {
            mCurrentAlarm = value;
            UpdateEnabled();
         }
      }

      private User CurrentLoggedInUser
      {
         get
         {
            return mCurrentLoggedInUser;
         }
         set
         {
            mCurrentLoggedInUser = value;
            UpdateEnabled();
         }
      }

      private void ClearRecordingDropdown()
      {
         selPlayback.DataSource = null;
         selPlayback.Items.Clear();
         if (!ApiSupportsPlayback && IsStarted)
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
            // Bugfix due to DarkUI library not invalidating the controls when the windows appear on screen.
            selPreset.Invalidate();
            selPlayback.Invalidate();
         }
      }
      private void Camera_PropertyChanged(object sender, PropertyChangedEventArgs e)
      {
         if (e.PropertyName == nameof(Camera.IsLocked) || e.PropertyName == nameof(Camera.LockOwner) || e.PropertyName == nameof(Camera.CanUnlock))
            return;

         UpdateCameraControl();
      }

      private void Camera_LockStatusChanged(object sender, EventArgs e)
      {
         if (InvokeRequired)
         {
            Invoke(new MethodInvoker(() => Camera_LockStatusChanged(sender, e)));
            return;
         }

         Image buttonImage = btnCameraLock.Image;

         if (mCurrentCamera != null && mCurrentCamera.IsLocked && mCurrentCamera.LockOwner == "Alarm")
         {
            buttonImage = Resources.cameralocked_red_24dp;
            btnCameraLock.Enabled = true;
         }

         else if (mCurrentCamera != null && mCurrentCamera.IsLocked)
         {
            buttonImage = Resources.cameralocked_yellow_24dp;
            btnCameraLock.Enabled = true;
         }

         else if (mCurrentCamera != null && !mCurrentCamera.IsLocked)
         {
            buttonImage = Resources.cameraunlocked_black_24dp;
            btnCameraLock.Enabled = true;
         }

         if (mCurrentCamera != null && mCurrentCamera.CanUnlock == false)
         {
            buttonImage = MakeGrayscale(buttonImage, true);
            btnCameraLock.Enabled = false;
         }

         btnCameraLock.Image = buttonImage;
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
      /// <summary>
      /// Converts an image into a grayscale image
      /// </summary>
      /// <param name="original">The original image.</param>
      /// <param name="bSelected">Indicates whether the image is selected.</param>
      /// <returns></returns>
      private static Image MakeGrayscale(Image original, bool bSelected)
      {
         if (original != null)
         {
            Bitmap newBitmap = new Bitmap(original.Width, original.Height);
            Graphics g = Graphics.FromImage(newBitmap);

            ColorMatrix colorMatrix = new ColorMatrix(
               new[]
               {
                  new[] {.1f, .1f, .1f, 0, 0},
                  new[] {.99f, .99f, .99f, 0, 0},
                  new[] {.41f, .41f, .41f, 0, 0},
                  new float[] {0, 0, 0, 1, 0},
                  new float[] {0, 0, 0, 0, 1}
               });

            if (bSelected)
               colorMatrix = new ColorMatrix(
                  new[]
                  {
                     new[] {1f, 0f, 0f, 0, 0},
                     new[] {0f, 1f, 0.4f, 0, 0},
                     new[] {0f, 0f, 1f, 0f, 0f},
                     new float[] {0, 0, 0, 1, 0},
                     new float[] {0, 0, 0, 0, 1}
                  });

            ImageAttributes attributes = new ImageAttributes();
            attributes.SetColorMatrix(colorMatrix);

            g.DrawImage(original, new Rectangle(0, 0, original.Width, original.Height),
               0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attributes);

            g.Dispose();
            return newBitmap;
         }
         return null;
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
         Camera camera = moFlexRApiClient.GetCamera(cameraNo);
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

      private void SelectAlarm(int alarmNo)
      {
         Alarm alarm = moFlexRApiClient.GetSingleAlarm(alarmNo);
         if (alarm != null)
         {
            CurrentAlarm = alarm;
         }
      }

      private void CheckApiVersion()
      {
         ApiVersion apiversion = moFlexRApiClient.GetApiVersion();
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
            if (chkUseTcp.Checked)
            {
               media.AddOption(":rtsp-tcp");
            }

            mVideoControl.MediaPlayer.Play();
            mIsVideoStarted = true;
         }
         else
         {
            var media = new Media(mLibVlc, uri);
            if (chkUseTcp.Checked)
            {
               media.AddOption(":rtsp-tcp");
            }
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

      private void OnSelectAlarmClicked(object sender, EventArgs e)
      {
         if (sender is Button button && button.Tag is Alarm alarm)
         {
            SelectAlarm(alarm.ComponentNumber);

            using (AlarmActionWindow window = new AlarmActionWindow(FlexRApiClient, CurrentAlarm, CurrentLoggedInUser))
            {
               window.StartPosition = FormStartPosition.Manual;
               window.Icon = Icon;
               window.Load += AlarmActionWindow_Load;
               window.ShowDialog(this);
            }
         }
      }

      private void AlarmActionWindow_Load(object sender, EventArgs e)
      {
         if (sender is Form window)
         {
            Point panelTopRight = new Point(pnlAlarms.Width, 0);
            Point screenLocation = pnlAlarms.PointToScreen(panelTopRight);

            window.Location = screenLocation;
         }
      }

      private void FillSelectCameraButtonList()
      {
         var cameraList = moFlexRApiClient.GetCameraList();
         if (cameraList != null)
         {
            foreach (Control control in pnlCameraSelectFlowPanel.Controls)
            {
               if (control is Button button)
               {
                  moToolTip.SetToolTip(button, null);
                  button.Click -= OnSelectCameraClicked;
               }
            } 
            pnlCameraSelectFlowPanel.Controls.Clear();

            foreach (var camera in cameraList)
            {
               Button oButton = new DarkButton()
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

      private void FillSelectAlarmButtonList()
      {
         List<Alarm> alarmList = moFlexRApiClient.GetAlarmList();
         if (alarmList == null)
            return;

         foreach (Control control in pnlAlarms.Controls)
         {
            if (control.Controls.Count > 0 && control.Controls[0] is Button button)
            {
               moToolTip.SetToolTip(button, null);
               button.Click -= OnSelectAlarmClicked;
            }
         }
         pnlAlarms.Controls.Clear();

         foreach (Alarm alarm in alarmList)
         {
            Panel frameBehindButton = new Panel();
            frameBehindButton.Size = new Size(64, 24);
            frameBehindButton.Tag = alarm;
            frameBehindButton.BackColor = GetColorForStatus(alarm.Status);
            frameBehindButton.Paint += FrameBehindAlarmButton_Paint;

            DarkButton oButton = new DarkButton();
            oButton.Text = "Alarm " + alarm.ComponentNumber;
            oButton.Size = new Size(60, 20);
            oButton.Tag = alarm;

            oButton.Location = new Point((frameBehindButton.Width - oButton.Width) / 2, (frameBehindButton.Height - oButton.Height) / 2);

            frameBehindButton.Controls.Add(oButton);

            oButton.Click += OnSelectAlarmClicked;
            moToolTip.SetToolTip(oButton, alarm.Name);

            pnlAlarms.Controls.Add(frameBehindButton);
            frameBehindButton.Invalidate();

            alarm.PropertyChanged += delegate (object sender, PropertyChangedEventArgs e)
            {
               if (e.PropertyName == "Status")
               {
                  UpdateAlarmButtonFrameColor(frameBehindButton, alarm);
               }
            };
         }
      }

      private void FrameBehindAlarmButton_Paint(object sender, PaintEventArgs e)
      {
         Panel frame = (Panel)sender;
         if (!(frame.Tag is Alarm alarm))
            return;

         Pen pen;

         if (alarm.Status == AlarmGeneralStatus.Disabled)
         {
            pen = new Pen(Color.DarkRed, 3f);
            pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
         }
         else
         {
            pen = new Pen(frame.BackColor, 3f); // solid border
            pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
         }

         try
         {
            Rectangle r = frame.ClientRectangle;
            r.Width--;
            r.Height--;

            e.Graphics.DrawRectangle(pen, r);
         }
         finally
         {
            pen.Dispose();
         }
      }

      private void UpdateAlarmButtonFrameColor(Panel frame, Alarm alarm)
      {
         if (frame.InvokeRequired)
         {
            frame.BeginInvoke(new Action(() => UpdateAlarmButtonFrameColor(frame, alarm)));
            return;
         }

         frame.BackColor = GetColorForStatus(alarm.Status);
         frame.Invalidate();
      }

      private static Color GetColorForStatus(AlarmGeneralStatus status)
      {
         switch(status)
         {
            case AlarmGeneralStatus.Active:
            return Color.Red;

            case AlarmGeneralStatus.Inactive:
            return Color.Gray;

            case AlarmGeneralStatus.Acknowledged:
            return Color.Orange;

            case AlarmGeneralStatus.Tampered:
            return Color.DarkOrange;

            default:
            return Color.FromArgb(120, Color.Gray);
         }
      }

      private void SetCurrentLoggedInUser()
      {
         CurrentLoggedInUser = FlexRApiClient.GetLoggedInUserInfo();
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
         if (IsStarted)
         {
            if (mIsPlaybackStarted)
            {
               btnPlayPlayback_Click(null, EventArgs.Empty);
            }
            else
            {
               if (CurrentCamera != null)
               {
                  SelectCamera(CurrentCamera.ComponentNumber, chkPreferSubChannel.Checked ? 2 : 1);
               }
            }
         }
         SaveSettings();
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
         IsStarted = false;
         StopRtspStream();
         SelectCamera(CurrentCamera.ComponentNumber, chkPreferSubChannel.Checked ? 2 : 1);
         IsStarted = true;
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
         using (DownloadWindow downloadWindow = new DownloadWindow(FlexRApiClient))
         {
            downloadWindow.StartPosition = FormStartPosition.CenterParent;
            downloadWindow.Icon = this.Icon;
            downloadWindow.ShowDialog(this);
         }
      }

      private void chkUseTcp_CheckedChanged(object sender, EventArgs e)
      {
         if (IsStarted)
         {
            if (mIsPlaybackStarted)
            {
               btnPlayPlayback_Click(null, EventArgs.Empty);
            }
            else
            {
               if (CurrentCamera != null)
               {
                  SelectCamera(CurrentCamera.ComponentNumber, chkPreferSubChannel.Checked ? 2 : 1);
               }
            }
         }
         SaveSettings();
      }

      private void btnOpenAbsolutePositionWindow_Click(object sender, EventArgs e)
      {
         using (AbsolutePositionWindow absolutePositionWindow = new AbsolutePositionWindow(FlexRApiClient, CurrentCamera))
         {
            absolutePositionWindow.StartPosition = FormStartPosition.Manual;
            absolutePositionWindow.Icon = Icon;

            if (sender is Button button)
            {
               Point screenPoint = button.PointToScreen(new Point(button.Width, 0));
               absolutePositionWindow.Location = screenPoint;
            }


            absolutePositionWindow.ShowDialog(this);
         }
      }

      private void btnCameraLock_Click(object sender, EventArgs e)
      { 
         if (!CurrentCamera.IsLocked)
         {
            using (CameraLockWindow cameraLockWindow = new CameraLockWindow(FlexRApiClient, CurrentCamera))
            {
               cameraLockWindow.StartPosition = FormStartPosition.Manual;
               cameraLockWindow.Icon = Icon;

               if (sender is Button button)
               {
                  Point screenPoint = button.PointToScreen(new Point(button.Width, 0));
                  cameraLockWindow.Location = screenPoint;
               }
               cameraLockWindow.ShowDialog(this);
            }
         }
         else
         {
            FlexRApiClient.SendUnlockCamera(CurrentCamera.ComponentNumber);
         }
      }


      private void Panel_CollapseStateChanged(object sender, EventArgs e)
      {
         RelayoutPanels();
      }

      private static bool IsStackable(Control c)
      {
         return c is CollapsibleDarkSectionPanel || c is DarkSectionPanel || c is Panel;
      }

      private void RelayoutPanels()
      {
         int y = 0;

         List<Control> panels = new List<Control>();
         foreach (Control c in splitMainVerticalSplit.Panel1.Controls)
         {
            if (IsStackable(c))
            {
               panels.Add(c);
            }
         }
         panels.Sort((a, b) => a.TabIndex.CompareTo(b.TabIndex));

         foreach (Control p in panels)
         {
            p.Top = y;
            y += p.Height - 1;
         }
      }
   }
}
