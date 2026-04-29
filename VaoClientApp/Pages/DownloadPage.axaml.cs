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
using Vao.Sample.Navigation;

namespace Vao.Sample.Pages
{
    public partial class DownloadPage : NavigableViewBase
    {
        private bool mIsFtpConnected = false;
        private readonly FlexRApiClient mFlexRApiClient;
        private FtpClient mFtpClient;
        private Camera mCurrentCamera;

        private ObservableCollection<DownloadItem> mPendingDownloads = new();
        private ObservableCollection<string> mFinishedDownloads = new();
        private ObservableCollection<string> mDownloadMessages = new();

        public DownloadPage()
        {
            InitializeComponent();
        }

        public DownloadPage(FlexRApiClient client) : this()
        {
            mFlexRApiClient = client;
            mFlexRApiClient.OnMessage += FlexRApiClientOnMessage;

            var lstPending = this.FindControl<ListBox>("lstPendingDownloads");
            var lstFinished = this.FindControl<ListBox>("lstFinishedDownloads");
            var lstMessages = this.FindControl<ListBox>("lstDownloadMessages");

            if (lstPending != null) lstPending.ItemsSource = mPendingDownloads;
            if (lstFinished != null) lstFinished.ItemsSource = mFinishedDownloads;
            if (lstMessages != null) lstMessages.ItemsSource = mDownloadMessages;

            FillStreamSelectionList();
            FillDurationSelectionList();
            FillCameraSelectionList();
            UpdateEnabled();
            UpdateEnabledDownloadButton();
        }

        public override void OnNavigatedTo()
        {
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
            var grpDownload = this.FindControl<Border>("grpDownload");
            var grpSelect = this.FindControl<Border>("grpSelectRecording");
            var selRecording = this.FindControl<ComboBox>("selRecording");
            var selCamera = this.FindControl<ComboBox>("selCamera");
            var selStream = this.FindControl<ComboBox>("selStreamNumber");
            var selDuration = this.FindControl<ComboBox>("selDuration");
            var txtRecorder = this.FindControl<TextBox>("txtRecorderAddress");

            bool enabled = IsDownloadPathSet;
            if (grpDownload != null) grpDownload.IsEnabled = enabled;
            if (grpSelect != null) grpSelect.IsEnabled = enabled;
            if (selRecording != null) selRecording.IsEnabled = enabled;
            if (selCamera != null) selCamera.IsEnabled = enabled;
            if (selStream != null) selStream.IsEnabled = enabled;
            if (selDuration != null) selDuration.IsEnabled = enabled;
            if (txtRecorder != null) txtRecorder.IsEnabled = enabled;
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
            else if (IsFtpConnected && recorderAddress != mFtpClient.Host)
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
                var ftpStatus = mFtpClient.DownloadFile($@"{downloadPath}\{downloadName}.{fileType}", $"{fileId}");
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
            var selStream = this.FindControl<ComboBox>("selStreamNumber");
            if (selStream != null)
            {
                selStream.ItemsSource = new List<string> { "Main Channel", "Sub Channel" };
                selStream.SelectedIndex = 0;
            }
        }

        private void FillDurationSelectionList()
        {
            var selDuration = this.FindControl<ComboBox>("selDuration");
            if (selDuration != null)
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
        }

        private void FillRecordingSelectionList(Camera camera)
        {
            var selRecording = this.FindControl<ComboBox>("selRecording");
            if (selRecording == null) return;

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
            var selCamera = this.FindControl<ComboBox>("selCamera");
            if (selCamera == null) return;

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
            mFtpClient = new FtpClient(recorderAddress, s.FTPUser ?? "", s.FTPPassword ?? "", 0, ftpConfig);
            try
            {
                WriteMessageLog("FTP", $"Connecting to FTP server {recorderAddress}", LogLevel.Notice);
                mFtpClient.Connect();
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
            try { mFtpClient?.Disconnect(); IsFtpConnected = false; }
            catch (FluentFTP.Exceptions.FtpException ex)
            {
                WriteMessageLog("FTP", ex.InnerException?.Message ?? ex.Message, LogLevel.Error);
            }
        }

        private void selCamera_SelectedIndexChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox cmb && cmb.SelectedItem is Camera camera)
            {
                FillRecordingSelectionList(camera);
                mCurrentCamera = camera;
                UpdateEnabled();
                UpdateEnabledDownloadButton();
            }
        }

        private void selRecording_SelectedIndexChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox cmb && cmb.SelectedItem is PlaybackInfo recording)
            {
                try
                {
                    var txtRecorder = this.FindControl<TextBox>("txtRecorderAddress");
                    if (txtRecorder != null)
                        txtRecorder.Text = recording.RecorderAddress;

                    var selStream = this.FindControl<ComboBox>("selStreamNumber");
                    if (selStream != null)
                    {
                        if (recording.Stream == 1) selStream.SelectedIndex = 0;
                        if (recording.Stream == 2) selStream.SelectedIndex = 1;
                    }
                }
                catch { }
            }
        }

        private void btnDownloadRequest_Click(object sender, RoutedEventArgs e)
        {
            if (mCurrentCamera == null) return;

            var selStream = this.FindControl<ComboBox>("selStreamNumber");
            var selDuration = this.FindControl<ComboBox>("selDuration");
            var txtRecorder = this.FindControl<TextBox>("txtRecorderAddress");
            var dateStart = this.FindControl<DatePicker>("startDatePicker");
            var timeStart = this.FindControl<TimePicker>("startTimePicker");

            int streamNumber = (selStream?.SelectedItem as string) == "Main Channel" ? 1 : 2;
            string durationIsoString = "";

            if (selDuration?.SelectedItem is DurationItem selectedDuration)
            {
                try { durationIsoString = XmlConvert.ToString(selectedDuration.Duration); }
                catch { WriteMessageLog("FTP", "No duration specified", LogLevel.Error); return; }
            }

            var date = dateStart?.SelectedDate?.DateTime ?? DateTime.Now;
            var time = timeStart?.SelectedTime ?? TimeSpan.Zero;
            var startTimeStr = (date.Date + time).ToString("yyyy-MM-dd HH:mm:ss");

            DownloadInfo downloadInfo = mFlexRApiClient.GetDownloadInfo(mCurrentCamera, txtRecorder?.Text ?? "", streamNumber, startTimeStr, durationIsoString);
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
            var btnDownload = this.FindControl<Button>("btnDownloadRequest");
            if (btnDownload == null) return;
            var s = AppSettings.Default;
            btnDownload.IsEnabled = !string.IsNullOrEmpty(s.FTPPassword) && !string.IsNullOrEmpty(s.FTPUser) && IsDownloadPathSet;
        }

        private void btnBack_Click(object sender, RoutedEventArgs e) => GoBack();

        public override string GetPageTitle() => "Download";

        public override void OnNavigatingFrom()
        {
            DisconnectFromFtpServer();
            mFlexRApiClient.OnMessage -= FlexRApiClientOnMessage;
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
