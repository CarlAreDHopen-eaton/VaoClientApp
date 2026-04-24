using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using FluentFTP;
using FluentFTP.Helpers;
using LibVLCSharp.Shared;
using Vao.Client;
using Vao.Client.Components;
using Vao.Client.Utility;

namespace Vao.Sample
{
   public partial class DownloadWindow : Window
   {
      private bool mIsFtpConnected = false;
      private readonly FlexRApiClient mFlexRApiClient;
      private FtpClient mFTPClient;
      private Camera mCurrentCamera;

      private ObservableCollection<DownloadItem> mPendingDownloads = new();
      private ObservableCollection<string> mFinishedDownloads = new();
      private ObservableCollection<string> mDownloadMessages = new();

      public DownloadWindow()
      {
         InitializeComponent();
      }

      public DownloadWindow(FlexRApiClient client)
      {
         InitializeComponent();
         mFlexRApiClient = client;
         mFlexRApiClient.OnMessage += FlexRApiClientOnMessage;

         lstPendingDownloads.ItemsSource = mPendingDownloads;
         lstFinishedDownloads.ItemsSource = mFinishedDownloads;
         lstDownloadMessages.ItemsSource = mDownloadMessages;

         FillStreamSelectionList();
         FillDurationSelectionList();
         FillCameraSelectionList();
         UpdateEnabled();
         UpdateEnabledDownloadButton();
      }

      private bool IsDownloadPathSet => !string.IsNullOrEmpty(AppSettings.Default.DownloadPath);

      private bool IsFtpConnected
      {
         get => mIsFtpConnected;
         set { mIsFtpConnected = value; UpdateEnabled(); }
      }

      private void UpdateEnabled()
      {
         grpDownload.IsEnabled = IsDownloadPathSet;
         grpSelectRecording.IsEnabled = IsDownloadPathSet;
         selRecording.IsEnabled = IsDownloadPathSet;
         selCamera.IsEnabled = IsDownloadPathSet;
         selStreamNumber.IsEnabled = IsDownloadPathSet;
         selDuration.IsEnabled = IsDownloadPathSet;
         txtRecorderAddress.IsEnabled = IsDownloadPathSet;
      }

      private void WriteMessageLog(string source, string message, LogLevel level)
      {
         if (!Dispatcher.UIThread.CheckAccess())
         {
            Dispatcher.UIThread.Post(() => WriteMessageLog(source, message, level));
            return;
         }
         var strTime = DateTime.Now.ToString(CultureInfo.InvariantCulture);
         mDownloadMessages.Add($"{strTime} [{level}] - {source} - {message}");
      }

      private void FlexRApiClientOnMessage(object sender, MessageEventArgs e)
      {
         if (!Dispatcher.UIThread.CheckAccess())
         {
            Dispatcher.UIThread.Post(() => FlexRApiClientOnMessage(sender, e));
            return;
         }

         if (e.IsFromSystem)
         {
            if (e.StatusMessage.DownloadId != Guid.Empty)
            {
               var downloadItem = mPendingDownloads.FirstOrDefault(x => x.DownloadId == e.StatusMessage.DownloadId);
               if (downloadItem != null)
               {
                  switch (e.StatusMessage.Type)
                  {
                     case Client.Enum.MessageType.ExtractedVideoReadyForDownload:
                        downloadItem.Text = $"{e.StatusMessage.DownloadId} - Starting Download - {DateTime.Now:HH:mm:ss}";
                        DownloadFileFromFtpServer(e.StatusMessage.DownloadRecorderAddress, e.StatusMessage.DownloadName, e.StatusMessage.DownloadUrl, downloadItem, e);
                        break;
                     case Client.Enum.MessageType.HvrWithRecordingIsOffline:
                        downloadItem.Text = $"{e.StatusMessage.DownloadId} - Hvr is offline - {DateTime.Now:HH:mm:ss}";
                        break;
                     case Client.Enum.MessageType.NoVideoInRequestedDownload:
                        downloadItem.Text = $"{e.StatusMessage.DownloadId} - No video in requested download - {DateTime.Now:HH:mm:ss}";
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

      private void DownloadFileFromFtpServer(string recorderAddress, string downloadName, string downloadUrl, DownloadItem downloadItem, MessageEventArgs e)
      {
         string fileId = downloadUrl.GetFileId();
         if (fileId == null) { WriteMessageLog("FTP", "No download url specified", LogLevel.Error); return; }

         string fileType = downloadUrl.GetFileType();
         if (!IsFtpConnected)
            ConnectToFtpServer(recorderAddress);
         else if (IsFtpConnected && recorderAddress != mFTPClient.Host)
         {
            DisconnectFromFtpServer();
            ConnectToFtpServer(recorderAddress);
         }

         if (!IsFtpConnected) return;

         string downloadPath = AppSettings.Default.DownloadPath;
         if (string.IsNullOrEmpty(downloadPath)) downloadPath = "C:\\";
         if (File.Exists($@"{downloadPath}\{downloadName}.{fileType}"))
         {
            for (int i = 1; ; ++i)
            {
               string tmp = $@"{downloadName} - Copy ({i})";
               if (!File.Exists($@"{downloadPath}\{tmp}.{fileType}")) { downloadName = tmp; break; }
            }
         }

         try
         {
            WriteMessageLog("FTP", $"Downloading {fileId} from FTP server {recorderAddress}", LogLevel.Notice);
            var ftpStatus = mFTPClient.DownloadFile($@"{downloadPath}\{downloadName}.{fileType}", $"{fileId}");
            if (ftpStatus.IsSuccess())
            {
               mFinishedDownloads.Add($"{downloadName}.{fileType}");
               mPendingDownloads.Remove(downloadItem);
            }
            if (ftpStatus.IsFailure())
               downloadItem.Text = $"{e.StatusMessage.DownloadId} - Download failed - {DateTime.Now:HH:mm:ss}";
         }
         catch (FluentFTP.Exceptions.FtpException ex)
         {
            WriteMessageLog("FTP", ex.InnerException?.Message ?? ex.Message, LogLevel.Error);
         }
      }

      private void FillStreamSelectionList()
      {
         selStreamNumber.ItemsSource = new List<string> { "Main Channel", "Sub Channel" };
         selStreamNumber.SelectedIndex = 0;
      }

      private void FillDurationSelectionList()
      {
         var items = new List<DurationItem>
         {
            new("1 minute", TimeSpan.FromMinutes(1)),
            new("5 minutes", TimeSpan.FromMinutes(5)),
            new("10 minutes", TimeSpan.FromMinutes(10)),
            new("20 minutes", TimeSpan.FromMinutes(20)),
            new("30 minutes", TimeSpan.FromMinutes(30)),
            new("40 minutes", TimeSpan.FromMinutes(40)),
            new("50 minutes", TimeSpan.FromMinutes(50)),
            new("1 hour", TimeSpan.FromHours(1)),
         };
         selDuration.ItemsSource = items;
         selDuration.SelectedIndex = 0;
      }

      private void FillRecordingSelectionList(Camera camera)
      {
         Guid guid = Guid.NewGuid();
         List<PlaybackInfo> playbackInfoList = camera.GetPlaybackInfoList(guid);
         if (playbackInfoList?.Count > 0)
         {
            selRecording.ItemsSource = playbackInfoList;
            selRecording.SelectedIndex = 0;
         }
         else
         {
            selRecording.ItemsSource = new List<string> { "No Recorders" };
            selRecording.SelectedIndex = 0;
         }
      }

      private void FillCameraSelectionList()
      {
         List<Camera> cameraList = mFlexRApiClient.GetCameraList();
         if (cameraList?.Count > 0)
         {
            selCamera.ItemsSource = cameraList;
            selCamera.SelectedIndex = 0;
         }
         else
         {
            selCamera.ItemsSource = new List<string> { "No Cameras" };
            selCamera.SelectedIndex = 0;
         }
      }

      private bool ConnectToFtpServer(string recorderAddress)
      {
         var s = AppSettings.Default;
         var ftpConfig = new FtpConfig { EncryptionMode = FtpEncryptionMode.Explicit, ValidateAnyCertificate = true };
         mFTPClient = new FtpClient(recorderAddress, s.FTPUser ?? "", s.FTPPassword ?? "", 0, ftpConfig);
         try
         {
            WriteMessageLog("FTP", $"Connecting to FTP server {recorderAddress}", LogLevel.Notice);
            mFTPClient.Connect();
            IsFtpConnected = true;
            return true;
         }
         catch (FluentFTP.Exceptions.FtpAuthenticationException ex)
         {
            WriteMessageLog("FTP", ex.InnerException?.Message ?? ex.Message, LogLevel.Error);
            return false;
         }
      }

      private void DisconnectFromFtpServer()
      {
         try { mFTPClient.Disconnect(); IsFtpConnected = false; }
         catch (FluentFTP.Exceptions.FtpException ex)
         {
            WriteMessageLog("FTP", ex.InnerException?.Message ?? ex.Message, LogLevel.Error);
         }
      }

      private void selCamera_SelectedIndexChanged(object sender, SelectionChangedEventArgs e)
      {
         if (selCamera.SelectedItem is Camera camera)
         {
            FillRecordingSelectionList(camera);
            mCurrentCamera = camera;
            UpdateEnabled();
            UpdateEnabledDownloadButton();
         }
      }

      private void selRecording_SelectedIndexChanged(object sender, SelectionChangedEventArgs e)
      {
         if (selRecording.SelectedItem is PlaybackInfo recording)
         {
            try
            {
               // Avalonia DatePicker doesn't support display date range constraints
               txtRecorderAddress.Text = recording.RecorderAddress;

               if (recording.Stream == 1) selStreamNumber.SelectedIndex = 0;
               if (recording.Stream == 2) selStreamNumber.SelectedIndex = 1;
            }
            catch { }
         }
      }

      private void btnDownloadRequest_Click(object sender, RoutedEventArgs e)
      {
         if (mCurrentCamera == null) return;

         int streamNumber = (selStreamNumber.SelectedItem as string) == "Main Channel" ? 1 : 2;
         string durationIsoString = "";

         if (selDuration.SelectedItem is DurationItem selectedDuration)
         {
            try { durationIsoString = XmlConvert.ToString(selectedDuration.Duration); }
            catch { WriteMessageLog("FTP", "No duration specified", LogLevel.Error); return; }
         }

         var date = startDatePicker.SelectedDate?.DateTime ?? DateTime.Now;
         var time = startTimePicker.SelectedTime ?? TimeSpan.Zero;
         var startTimeStr = (date.Date + time).ToString("yyyy-MM-dd HH:mm:ss");

         DownloadInfo downloadInfo = mFlexRApiClient.GetDownloadInfo(mCurrentCamera, txtRecorderAddress.Text ?? "", streamNumber, startTimeStr, durationIsoString);
         if (downloadInfo != null)
         {
            mPendingDownloads.Add(new DownloadItem
            {
               DownloadId = downloadInfo.DownloadId,
               Text = $"{downloadInfo.DownloadId} - Extracting videofile - {DateTime.Now:HH:mm:ss}"
            });
         }
      }

      private void btnClearPendingDownloads_Click(object sender, RoutedEventArgs e) => mPendingDownloads.Clear();
      private void btnClearFinishedDownloads_Click(object sender, RoutedEventArgs e) => mFinishedDownloads.Clear();
      private void btnClearDownloadMessages_Click(object sender, RoutedEventArgs e) => mDownloadMessages.Clear();

      private void UpdateEnabledDownloadButton()
      {
         if (btnDownloadRequest == null) return;
         var s = AppSettings.Default;
         btnDownloadRequest.IsEnabled = !string.IsNullOrEmpty(s.FTPPassword) && !string.IsNullOrEmpty(s.FTPUser) && IsDownloadPathSet;
      }
   }

   public class DownloadItem
   {
      public Guid DownloadId { get; set; }
      public string Text { get; set; }
      public override string ToString() => Text;
   }

   public class DurationItem
   {
      public string Name { get; }
      public TimeSpan Duration { get; }
      public DurationItem(string name, TimeSpan duration) { Name = name; Duration = duration; }
      public override string ToString() => Name;
   }
}
