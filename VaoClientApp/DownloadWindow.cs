using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using Vao.Client;
using Vao.Client.Components;
using FluentFTP;
using FluentFTP.Helpers;
using DarkUI.Controls;
using System.IO;
using Vao.Sample.Properties;
using LibVLCSharp.Shared;
using Vao.Client.Utility;

namespace Vao.Sample
{
   public partial class DownloadWindow : Form
   {
      private bool mIsDownloadPathSet = false;
      private bool mIsFtpConnected = false;
      private readonly VaoClient mVaoClient = null;
      private FtpClient mFTPClient = null;
      private Camera mCurrentCamera;

      public Camera CurrentCamera
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
      public VaoClient VaoClient
      { 
         get { return mVaoClient; } 
      }

      public bool IsDownloadPathSet
      {
         get { return mIsDownloadPathSet; }
         set
         {
            mIsDownloadPathSet = value;
            UpdateEnabled();
            UpdateEnabledDownloadButton();
         }
      }

      public bool IsFtpConnected
      {
         get { return mIsFtpConnected; }
         set
         {
            mIsFtpConnected = value;
            UpdateEnabled();
         }
      }
      public DownloadWindow(VaoClient client)
      {
         InitializeComponent();
         mVaoClient = client;
         mVaoClient.OnMessage += VaoClient_OnMessage;
         FillStreamSelectionList();
         FillDurationSelectionList();
         FillCameraSelectionList();
         LoadSettings();
         UpdateEnabled();
         UpdateEnabledDownloadButton();
         // Updates the title bar to dark.
         DarkModeHelper.EnableImmersiveDarkMode(Handle);
      }

      public DownloadWindow()
      {
         InitializeComponent();
      }

      public void UpdateEnabled()
      {
         grpDownload.Enabled = IsDownloadPathSet;
         grpSelectRecording.Enabled = IsDownloadPathSet;
         selRecording.Enabled = IsDownloadPathSet;
         selCamera.Enabled = IsDownloadPathSet;
         selStreamNumber.Enabled = IsDownloadPathSet;
         startTimePicker.Enabled = IsDownloadPathSet;
         selDuration.Enabled = IsDownloadPathSet;
         txtRecorderAddress.Enabled = IsDownloadPathSet;
      }

      /// <summary>
      /// Loads the configuration settings into the UI.
      /// </summary>
      private void LoadSettings()
      {
         if (!string.IsNullOrEmpty(Settings.Default.FTPUser))
            txtFTPUser.Text = Settings.Default.FTPUser;

         if (!string.IsNullOrEmpty(Settings.Default.FTPPassword))
            txtFTPPassword.Text = Settings.Default.FTPPassword;

         if (!string.IsNullOrEmpty(Settings.Default.DownloadPath))
            txtDownloadPath.Text = Settings.Default.DownloadPath;
            IsDownloadPathSet = true;
      }

      private void SaveSettings()
      {
         Settings.Default.FTPUser = txtFTPUser.Text;
         Settings.Default.FTPPassword = txtFTPPassword.Text;
         Settings.Default.DownloadPath = txtDownloadPath.Text;
         Settings.Default.Save();
         IsDownloadPathSet = true;
      }

      private void WriteMessageLog(string strSource, string strMessage, LogLevel level)
      {
         if (InvokeRequired)
         {
            BeginInvoke(new MethodInvoker(() => WriteMessageLog(strSource, strMessage, level)));
         }
         else
         {
            var dlvi = new DarkListItem();
            var strTime = DateTime.Now.ToString(CultureInfo.InvariantCulture);
            var strLevel = level.ToString();
            var strMsg = $"{strTime} [{strLevel}] - {strSource} - {strMessage}";
            dlvi.Text = strMsg;

            if (strMessage != "drawable Warning: unsupported control query 3")
            {
               lstDownloadMessages.Items.Add(dlvi);
               lstDownloadMessages.SelectItem(lstDownloadMessages.Items.Count - 1);
               lstDownloadMessages.EnsureVisible();
            }
         }
      }

      private void VaoClient_OnMessage(object sender, MessageEventArgs e)
      {
         if (InvokeRequired)
         {
            BeginInvoke(new MethodInvoker(() => VaoClient_OnMessage(sender, e)));
         }
         else
         {
            if (e.IsStatusMessage)
            {
                if (e.StatusMessage.DownloadId != Guid.Empty)
                {
                    DarkListItem downloadItem = lstPendingDownloads.Items
                        .FirstOrDefault(x => (x as DarkListItemWithId).DownloadId == e.StatusMessage.DownloadId);

                    if (downloadItem != null)
                    {
                        switch (e.StatusMessage.Type)
                        {
                            case Client.Enum.MessageType.ExtractedVideoReadyForDownload:
                                downloadItem.Text = $"{e.StatusMessage.DownloadId} - Starting Download  - {DateTime.Now:HH:mm:ss}";
                                DownloadFileFromFtpServer(e.StatusMessage.DownloadRecorderAddress, e.StatusMessage.DownloadName, e.StatusMessage.DownloadUrl, downloadItem, e);
                                break;

                            case Client.Enum.MessageType.HvrWithRecordingIsOffline:
                                downloadItem.Text = $"{e.StatusMessage.DownloadId} - Hvr is offline  - {DateTime.Now:HH:mm:ss}";
                                break;

                            case Client.Enum.MessageType.NoVideoInRequestedDownload:
                                 downloadItem.Text = $"{e.StatusMessage.DownloadId} - No video in requested download  - {DateTime.Now:HH:mm:ss}";
                                 break;
                        }
                    }
                }
            }
            else
            {
                WriteMessageLog("VaoAPI", e.Message, LogLevel.Notice);
            }

         }
      }

      private void DownloadFileFromFtpServer(string recorderAddress, string downloadName, string downloadUrl, DarkListItem downloadItem, MessageEventArgs e)
      {
         FtpStatus ftpStatus;
         string fileId = downloadUrl.GetFileId();
         if (fileId == null)
         {
            WriteMessageLog("FTP", "No download url specified", LogLevel.Error);
            return;
         }

         string fileType = downloadUrl.GetFileType();
         if (!IsFtpConnected)
         {
            ConnectToFtpServer(recorderAddress, downloadItem, e);
         }

         else if (IsFtpConnected && recorderAddress != mFTPClient.Host)
         {
            DisconnectFromFtpServer();
            ConnectToFtpServer(recorderAddress, downloadItem, e);
         }

         if (!IsFtpConnected)
         {
            // Was unable to connect to ftp server.
            return;
         }

         if (File.Exists($@"{txtDownloadPath.Text}\{downloadName}.{fileType}"))
         {
            for (int i = 1; ; ++i)
            {
               string tmpDownloadName = $@"{downloadName} - Copy ({i})";

               if (!File.Exists($@"{txtDownloadPath.Text}\{tmpDownloadName}.{fileType}"))
               {
                  downloadName = tmpDownloadName;
                  break;
               }
            }
         }

         try
         {
            WriteMessageLog("FTP", $"Downloading {fileId} from FTP server {recorderAddress}", LogLevel.Notice);
            ftpStatus = mFTPClient.DownloadFile($@"{txtDownloadPath.Text}\{downloadName}.{fileType}", $"{fileId}");
            if (ftpStatus.IsSuccess())
            {
               if (downloadItem != null)
               {
                  downloadItem.Text = $"{downloadName}.{fileType}";
                  lstFinishedDownloads.Items.Add(downloadItem);
                  lstFinishedDownloads.SelectItem(lstFinishedDownloads.Items.Count - 1);
                  lstFinishedDownloads.EnsureVisible();

                  lstPendingDownloads.Items.Remove(downloadItem);
               }
            }
            if (ftpStatus.IsFailure())
            {
               downloadItem.Text = $"{e.StatusMessage.DownloadId} - Download failed - {DateTime.Now:HH:mm:ss}";
            }

         }
         catch (FluentFTP.Exceptions.FtpException ex)
         {
            if (ex.InnerException != null)
               WriteMessageLog("FTP", ex.InnerException.Message, LogLevel.Error);
            else
               WriteMessageLog("FTP", ex.Message, LogLevel.Error);
         }
      }

      private void FillStreamSelectionList()
      {
         selStreamNumber.Items.Add("Main Channel");
         selStreamNumber.Items.Add("Sub Channel");
         selStreamNumber.SelectedIndex = 0;
      }

      private void FillDurationSelectionList()
      {
         Dictionary<string, TimeSpan> durationDict = new Dictionary<string, TimeSpan>
        {
            { "1 minute", TimeSpan.FromMinutes(1) },
            { "5 minutes" , TimeSpan.FromMinutes(5)},
            { "10 minutes" , TimeSpan.FromMinutes(10)},
            { "20 minutes" , TimeSpan.FromMinutes(20)},
            { "30 minutes" , TimeSpan.FromMinutes(30)},
            { "40 minutes" , TimeSpan.FromMinutes(40)},
            { "50 minutes" , TimeSpan.FromMinutes(50)},
            { "1 hour" , TimeSpan.FromHours(1)}
        };

         BindingSource source = new BindingSource(durationDict, null);

         selDuration.DataSource = source;
         selDuration.ValueMember = "Value";
         selDuration.DisplayMember = "Key";
         selDuration.SelectedIndex = 0;
      }

      private void FillRecordingSelectionList(Camera camera)
      {
         Guid guid = Guid.NewGuid();
         List<PlaybackInfo> playbackInfoList = camera.GetPlaybackInfoList(guid);

         if (playbackInfoList?.Count > 0)
         {
            selRecording.DataSource = playbackInfoList;
            selRecording.DisplayMember = nameof(PlaybackInfo.RecorderAddress);
            selRecording.SelectedIndex = 0;
         }
         else
         {
            selRecording.DataSource = null;
            selRecording.Items.Clear();
            selRecording.Items.Add("No Recorders");
            selRecording.SelectedIndex = 0;
         }
      }

      private void FillCameraSelectionList()
      {
         List<Camera> cameraList = VaoClient.GetCameraList();
         if (cameraList?.Count > 0)
         {
            selCamera.DataSource = cameraList;
            selCamera.DisplayMember = nameof(Camera.Name);
            selCamera.SelectedIndex = 0;
         }
         else
         {
            selCamera.DataSource = null;
            selCamera.Items.Clear();
            selCamera.Items.Add("No Cameras");
            selCamera.SelectedIndex = 0;
         }
      }

      private bool ConnectToFtpServer(string recorderAddress, DarkListItem downloadItem, MessageEventArgs e)
      {
         FtpConfig ftpConfig = new FtpConfig
         {
            EncryptionMode = FtpEncryptionMode.Explicit,
            ValidateAnyCertificate = true
         };

         mFTPClient = new FtpClient(recorderAddress, txtFTPUser.Text, txtFTPPassword.Text, 0, ftpConfig);

         try
         {
            WriteMessageLog("FTP", $"Connecting to FTP server {recorderAddress}", LogLevel.Notice);
            mFTPClient.Connect();
            IsFtpConnected = true;
            return true;
         }
         catch (FluentFTP.Exceptions.FtpAuthenticationException ex)
         {
            downloadItem.Text = $"{e.StatusMessage.DownloadId} - Connecting to Ftp server failed - {DateTime.Now:HH:mm:ss}";
            
            if (ex.InnerException != null)
               WriteMessageLog("FTP", ex.InnerException.Message, LogLevel.Error);
            else
               WriteMessageLog("FTP", ex.Message, LogLevel.Error);

            return false;
         }
      }

      private void DisconnectFromFtpServer()
      {
         try
         {
            mFTPClient.Disconnect();
            IsFtpConnected = false;
         }
         catch (FluentFTP.Exceptions.FtpException ex)
         {
            if (ex.InnerException != null)
               WriteMessageLog("FTP", ex.InnerException.Message, LogLevel.Error);
            else
               WriteMessageLog("FTP", ex.Message, LogLevel.Error);

         }
      }

      private void selCamera_SelectedIndexChanged(object sender, EventArgs e)
      {
         if (selCamera.SelectedItem is Camera camera && camera != null)
         {
            FillRecordingSelectionList(camera);
            CurrentCamera = camera;
         }
      }

      private void selRecording_SelectedIndexChanged(object sender, EventArgs e)
      {
         if (selRecording.SelectedItem is PlaybackInfo recording && recording != null)
         {
            try
            {
               TimeSpan timeSpan = XmlConvert.ToTimeSpan(recording.RecordingLimit);
               startTimePicker.MinDate = DateTime.UtcNow.Add(-timeSpan);
               startTimePicker.MaxDate = DateTime.UtcNow;

               startTimePicker.Invalidate();
               startTimePicker.Update();

               txtRecorderAddress.Text = recording.RecorderAddress;
               
               if (recording.Stream == 1)
               {
                  selStreamNumber.Text = "Main Channel";
               }

               if (recording.Stream == 2)
               {
                  selStreamNumber.Text = "Sub Channel";
               }

            }
            catch (Exception)
            {
            }
         }
      }

      private void btnDownloadRequest_Click(object sender, EventArgs e)
      {
         SaveSettings();
         Camera currentCamera = CurrentCamera;
         int streamNumber;
         string durationIsoString = "";

         if (selStreamNumber.Text == "Main Channel")
            streamNumber = 1;
         else
            streamNumber = 2;
         
         if (currentCamera != null)
         {
            string downloadId;
            if (selDuration.SelectedItem is KeyValuePair<string, TimeSpan> selectedDuration && selectedDuration.Value != null)
            {
               TimeSpan duration = selectedDuration.Value;
               try
               {
                  durationIsoString = XmlConvert.ToString(duration);
               }
               catch 
               {
                  WriteMessageLog("FTP", "No duration specified", LogLevel.Error);
                  return;
               }
            }

            DownloadInfo downloadInfo = mVaoClient.GetDownloadInfo(currentCamera, txtRecorderAddress.Text, streamNumber, startTimePicker.Text, durationIsoString);
               
            if (downloadInfo != null)
            {
               downloadId = downloadInfo.DownloadId.ToString();
               DarkListItemWithId darklistItem = new DarkListItemWithId($"{downloadId} - Extracting videofile  - {DateTime.Now:HH:mm:ss}", downloadInfo.DownloadId);
               lstPendingDownloads.Items.Add(darklistItem);
            }
         }   
      }

      private void btnBrowseDownloadPath_Click(object sender, EventArgs e)
      {
         using (var folderDialog = new FolderBrowserDialog())
         {
            folderDialog.SelectedPath = @"C:\";

            DialogResult result = folderDialog.ShowDialog();

            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(folderDialog.SelectedPath))
            {
               txtDownloadPath.Text = folderDialog.SelectedPath;
               SaveSettings();
            }
         }
      }

      private void btnClearPendingDownloads_Click(object sender, EventArgs e)
      {
         lstPendingDownloads.Items.Clear();
      }

      private void btnClearFinishedDownloads_Click(object sender, EventArgs e)
      {
         lstFinishedDownloads.Items.Clear();
      }

      private void btnClearDownloadMessages_Click(object sender, EventArgs e)
      {
         lstDownloadMessages.Items.Clear();  
      }


      private void txtFTPUser_TextChanged(object sender, EventArgs e)
      {
         UpdateEnabledDownloadButton();
      }
      private void txtFTPPassword_TextChanged(object sender, EventArgs e)
      {
          UpdateEnabledDownloadButton();  
      }

      private void UpdateEnabledDownloadButton()
     {
         if (string.IsNullOrEmpty(txtFTPPassword.Text) || string.IsNullOrEmpty(txtFTPUser.Text))
         {
            btnDownloadRequest.Enabled = false;
         }
         else
         {
            btnDownloadRequest.Enabled = IsDownloadPathSet;
         }
      }
   }
}
