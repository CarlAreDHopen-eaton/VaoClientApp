using DarkUI.Controls;
using System;
using System.Xml;
using Vao.Client.Components;

namespace Vao.Sample
{
   partial class DownloadWindow
   {
      /// <summary>
      /// Required designer variable.
      /// </summary>
      private System.ComponentModel.IContainer components = null;

      /// <summary>
      /// Clean up any resources being used.
      /// </summary>
      /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
      protected override void Dispose(bool disposing)
      {
         if (disposing && (components != null))
         {
            components.Dispose();
         }
         base.Dispose(disposing);
      }

      #region Windows Form Designer generated code

      /// <summary>
      /// Required method for Designer support - do not modify
      /// the contents of this method with the code editor.
      /// </summary>
      private void InitializeComponent()
      {
         this.splitMainVerticalSplit = new System.Windows.Forms.SplitContainer();
         this.grpFTPConnection = new DarkUI.Controls.DarkSectionPanel();
         this.txtFTPPassword = new DarkUI.Controls.DarkTextBox();
         this.lblPassword = new System.Windows.Forms.Label();
         this.lblUser = new System.Windows.Forms.Label();
         this.txtFTPUser = new DarkUI.Controls.DarkTextBox();
         this.grpDownloadPath = new DarkUI.Controls.DarkSectionPanel();
         this.btnBrowse = new DarkUI.Controls.DarkButton();
         this.txtDownloadPath = new DarkUI.Controls.DarkTextBox();
         this.grpDownload = new DarkUI.Controls.DarkSectionPanel();
         this.selDuration = new System.Windows.Forms.ComboBox();
         this.selStreamNumber = new System.Windows.Forms.ComboBox();
         this.startTimePicker = new System.Windows.Forms.DateTimePicker();
         this.recorderAddress = new System.Windows.Forms.Label();
         this.txtRecorderAddress = new DarkUI.Controls.DarkTextBox();
         this.stream = new System.Windows.Forms.Label();
         this.duration = new System.Windows.Forms.Label();
         this.startTime = new System.Windows.Forms.Label();
         this.btnDownloadRequest = new DarkUI.Controls.DarkButton();
         this.grpSelectRecording = new DarkUI.Controls.DarkSectionPanel();
         this.lblRecordings = new System.Windows.Forms.Label();
         this.lblCamera = new System.Windows.Forms.Label();
         this.selCamera = new System.Windows.Forms.ComboBox();
         this.selRecording = new DarkUI.Controls.DarkComboBox();
         this.grpMessages = new DarkUI.Controls.DarkSectionPanel();
         this.btnClearDownloadMessages = new DarkUI.Controls.DarkButton();
         this.lstDownloadMessages = new DarkUI.Controls.DarkListView();
         this.btnClearMessages = new DarkUI.Controls.DarkButton();
         this.grpFinishedDownloads = new DarkUI.Controls.DarkSectionPanel();
         this.btnClearFinishedDownloads = new DarkUI.Controls.DarkButton();
         this.lstFinishedDownloads = new DarkUI.Controls.DarkListView();
         this.grpPendingDownloads = new DarkUI.Controls.DarkSectionPanel();
         this.btnClearPendingDownloads = new DarkUI.Controls.DarkButton();
         this.lstPendingDownloads = new DarkUI.Controls.DarkListView();
         ((System.ComponentModel.ISupportInitialize)(this.splitMainVerticalSplit)).BeginInit();
         this.splitMainVerticalSplit.Panel1.SuspendLayout();
         this.splitMainVerticalSplit.Panel2.SuspendLayout();
         this.splitMainVerticalSplit.SuspendLayout();
         this.grpFTPConnection.SuspendLayout();
         this.grpDownloadPath.SuspendLayout();
         this.grpDownload.SuspendLayout();
         this.grpSelectRecording.SuspendLayout();
         this.grpMessages.SuspendLayout();
         this.grpFinishedDownloads.SuspendLayout();
         this.grpPendingDownloads.SuspendLayout();
         this.SuspendLayout();
         // 
         // splitMainVerticalSplit
         // 
         this.splitMainVerticalSplit.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
         this.splitMainVerticalSplit.Dock = System.Windows.Forms.DockStyle.Fill;
         this.splitMainVerticalSplit.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
         this.splitMainVerticalSplit.Location = new System.Drawing.Point(0, 0);
         this.splitMainVerticalSplit.Name = "splitMainVerticalSplit";
         // 
         // splitMainVerticalSplit.Panel1
         // 
         this.splitMainVerticalSplit.Panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
         this.splitMainVerticalSplit.Panel1.Controls.Add(this.grpFTPConnection);
         this.splitMainVerticalSplit.Panel1.Controls.Add(this.grpDownloadPath);
         this.splitMainVerticalSplit.Panel1.Controls.Add(this.grpDownload);
         this.splitMainVerticalSplit.Panel1.Controls.Add(this.grpSelectRecording);
         this.splitMainVerticalSplit.Panel1MinSize = 400;
         // 
         // splitMainVerticalSplit.Panel2
         // 
         this.splitMainVerticalSplit.Panel2.Controls.Add(this.grpMessages);
         this.splitMainVerticalSplit.Panel2.Controls.Add(this.grpFinishedDownloads);
         this.splitMainVerticalSplit.Panel2.Controls.Add(this.grpPendingDownloads);
         this.splitMainVerticalSplit.Size = new System.Drawing.Size(1118, 573);
         this.splitMainVerticalSplit.SplitterDistance = 400;
         this.splitMainVerticalSplit.TabIndex = 0;
         // 
         // grpFTPConnection
         // 
         this.grpFTPConnection.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.grpFTPConnection.Controls.Add(this.txtFTPPassword);
         this.grpFTPConnection.Controls.Add(this.lblPassword);
         this.grpFTPConnection.Controls.Add(this.lblUser);
         this.grpFTPConnection.Controls.Add(this.txtFTPUser);
         this.grpFTPConnection.Location = new System.Drawing.Point(12, 12);
         this.grpFTPConnection.Name = "grpFTPConnection";
         this.grpFTPConnection.SectionHeader = "FTP Connection";
         this.grpFTPConnection.Size = new System.Drawing.Size(378, 101);
         this.grpFTPConnection.TabIndex = 7;
         // 
         // txtFTPPassword
         // 
         this.txtFTPPassword.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.txtFTPPassword.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
         this.txtFTPPassword.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
         this.txtFTPPassword.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
         this.txtFTPPassword.Location = new System.Drawing.Point(103, 64);
         this.txtFTPPassword.Name = "txtFTPPassword";
         this.txtFTPPassword.PasswordChar = '*';
         this.txtFTPPassword.Size = new System.Drawing.Size(136, 20);
         this.txtFTPPassword.TabIndex = 7;
         this.txtFTPPassword.TextChanged += new System.EventHandler(this.txtFTPPassword_TextChanged);
         // 
         // lblPassword
         // 
         this.lblPassword.AutoSize = true;
         this.lblPassword.ForeColor = System.Drawing.Color.Silver;
         this.lblPassword.Location = new System.Drawing.Point(18, 66);
         this.lblPassword.Name = "lblPassword";
         this.lblPassword.Size = new System.Drawing.Size(53, 13);
         this.lblPassword.TabIndex = 6;
         this.lblPassword.Text = "Password";
         // 
         // lblUser
         // 
         this.lblUser.AutoSize = true;
         this.lblUser.ForeColor = System.Drawing.Color.Silver;
         this.lblUser.Location = new System.Drawing.Point(18, 41);
         this.lblUser.Name = "lblUser";
         this.lblUser.Size = new System.Drawing.Size(29, 13);
         this.lblUser.TabIndex = 4;
         this.lblUser.Text = "User";
         // 
         // txtFTPUser
         // 
         this.txtFTPUser.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.txtFTPUser.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
         this.txtFTPUser.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
         this.txtFTPUser.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
         this.txtFTPUser.Location = new System.Drawing.Point(103, 38);
         this.txtFTPUser.Name = "txtFTPUser";
         this.txtFTPUser.Size = new System.Drawing.Size(136, 20);
         this.txtFTPUser.TabIndex = 5;
         this.txtFTPUser.TextChanged += new System.EventHandler(this.txtFTPUser_TextChanged);
         // 
         // grpDownloadPath
         // 
         this.grpDownloadPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.grpDownloadPath.Controls.Add(this.btnBrowse);
         this.grpDownloadPath.Controls.Add(this.txtDownloadPath);
         this.grpDownloadPath.Location = new System.Drawing.Point(12, 131);
         this.grpDownloadPath.Name = "grpDownloadPath";
         this.grpDownloadPath.SectionHeader = "Download path";
         this.grpDownloadPath.Size = new System.Drawing.Size(378, 102);
         this.grpDownloadPath.TabIndex = 6;
         // 
         // btnBrowse
         // 
         this.btnBrowse.Location = new System.Drawing.Point(11, 68);
         this.btnBrowse.Name = "btnBrowse";
         this.btnBrowse.Padding = new System.Windows.Forms.Padding(5);
         this.btnBrowse.Size = new System.Drawing.Size(68, 21);
         this.btnBrowse.TabIndex = 18;
         this.btnBrowse.Text = "Browse";
         this.btnBrowse.Click += new System.EventHandler(this.btnBrowseDownloadPath_Click);
         // 
         // txtDownloadPath
         // 
         this.txtDownloadPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.txtDownloadPath.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
         this.txtDownloadPath.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
         this.txtDownloadPath.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
         this.txtDownloadPath.Location = new System.Drawing.Point(11, 38);
         this.txtDownloadPath.Name = "txtDownloadPath";
         this.txtDownloadPath.Size = new System.Drawing.Size(355, 20);
         this.txtDownloadPath.TabIndex = 17;
         this.txtDownloadPath.Text = "C:\\";
         // 
         // grpDownload
         // 
         this.grpDownload.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.grpDownload.Controls.Add(this.selDuration);
         this.grpDownload.Controls.Add(this.selStreamNumber);
         this.grpDownload.Controls.Add(this.startTimePicker);
         this.grpDownload.Controls.Add(this.recorderAddress);
         this.grpDownload.Controls.Add(this.txtRecorderAddress);
         this.grpDownload.Controls.Add(this.stream);
         this.grpDownload.Controls.Add(this.duration);
         this.grpDownload.Controls.Add(this.startTime);
         this.grpDownload.Controls.Add(this.btnDownloadRequest);
         this.grpDownload.Location = new System.Drawing.Point(12, 372);
         this.grpDownload.Name = "grpDownload";
         this.grpDownload.SectionHeader = "Request Download";
         this.grpDownload.Size = new System.Drawing.Size(378, 189);
         this.grpDownload.TabIndex = 5;
         // 
         // selDuration
         // 
         this.selDuration.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.selDuration.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
         this.selDuration.FormattingEnabled = true;
         this.selDuration.Location = new System.Drawing.Point(103, 110);
         this.selDuration.Name = "selDuration";
         this.selDuration.Size = new System.Drawing.Size(136, 21);
         this.selDuration.TabIndex = 17;
         // 
         // selStreamNumber
         // 
         this.selStreamNumber.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.selStreamNumber.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
         this.selStreamNumber.FormattingEnabled = true;
         this.selStreamNumber.Location = new System.Drawing.Point(103, 56);
         this.selStreamNumber.Name = "selStreamNumber";
         this.selStreamNumber.Size = new System.Drawing.Size(136, 21);
         this.selStreamNumber.TabIndex = 15;
         // 
         // startTimePicker
         // 
         this.startTimePicker.CalendarMonthBackground = System.Drawing.SystemColors.ScrollBar;
         this.startTimePicker.CalendarTitleForeColor = System.Drawing.SystemColors.Highlight;
         this.startTimePicker.CustomFormat = "yyyy-MM-dd HH:mm:ss";
         this.startTimePicker.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
         this.startTimePicker.Location = new System.Drawing.Point(103, 83);
         this.startTimePicker.MinDate = new System.DateTime(2000, 1, 1, 0, 0, 0, 0);
         this.startTimePicker.Name = "startTimePicker";
         this.startTimePicker.Size = new System.Drawing.Size(136, 20);
         this.startTimePicker.TabIndex = 13;
         // 
         // recorderAddress
         // 
         this.recorderAddress.AutoSize = true;
         this.recorderAddress.ForeColor = System.Drawing.Color.Silver;
         this.recorderAddress.Location = new System.Drawing.Point(8, 33);
         this.recorderAddress.Name = "recorderAddress";
         this.recorderAddress.Size = new System.Drawing.Size(92, 13);
         this.recorderAddress.TabIndex = 0;
         this.recorderAddress.Text = "Recorder Address";
         // 
         // txtRecorderAddress
         // 
         this.txtRecorderAddress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.txtRecorderAddress.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
         this.txtRecorderAddress.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
         this.txtRecorderAddress.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
         this.txtRecorderAddress.Location = new System.Drawing.Point(103, 31);
         this.txtRecorderAddress.Name = "txtRecorderAddress";
         this.txtRecorderAddress.Size = new System.Drawing.Size(136, 20);
         this.txtRecorderAddress.TabIndex = 1;
         // 
         // stream
         // 
         this.stream.AutoSize = true;
         this.stream.ForeColor = System.Drawing.Color.Silver;
         this.stream.Location = new System.Drawing.Point(8, 59);
         this.stream.Name = "stream";
         this.stream.Size = new System.Drawing.Size(40, 13);
         this.stream.TabIndex = 2;
         this.stream.Text = "Stream";
         // 
         // duration
         // 
         this.duration.AutoSize = true;
         this.duration.ForeColor = System.Drawing.Color.Silver;
         this.duration.Location = new System.Drawing.Point(8, 111);
         this.duration.Name = "duration";
         this.duration.Size = new System.Drawing.Size(47, 13);
         this.duration.TabIndex = 6;
         this.duration.Text = "Duration";
         // 
         // startTime
         // 
         this.startTime.AutoSize = true;
         this.startTime.ForeColor = System.Drawing.Color.Silver;
         this.startTime.Location = new System.Drawing.Point(8, 85);
         this.startTime.Name = "startTime";
         this.startTime.Size = new System.Drawing.Size(51, 13);
         this.startTime.TabIndex = 4;
         this.startTime.Text = "Start time";
         // 
         // btnDownloadRequest
         // 
         this.btnDownloadRequest.Location = new System.Drawing.Point(103, 146);
         this.btnDownloadRequest.Name = "btnDownloadRequest";
         this.btnDownloadRequest.Padding = new System.Windows.Forms.Padding(5);
         this.btnDownloadRequest.Size = new System.Drawing.Size(88, 28);
         this.btnDownloadRequest.TabIndex = 9;
         this.btnDownloadRequest.Text = "Download";
         this.btnDownloadRequest.Click += new System.EventHandler(this.btnDownloadRequest_Click);
         // 
         // grpSelectRecording
         // 
         this.grpSelectRecording.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.grpSelectRecording.Controls.Add(this.lblRecordings);
         this.grpSelectRecording.Controls.Add(this.lblCamera);
         this.grpSelectRecording.Controls.Add(this.selCamera);
         this.grpSelectRecording.Controls.Add(this.selRecording);
         this.grpSelectRecording.Location = new System.Drawing.Point(12, 252);
         this.grpSelectRecording.Name = "grpSelectRecording";
         this.grpSelectRecording.SectionHeader = "Available Recordings";
         this.grpSelectRecording.Size = new System.Drawing.Size(378, 101);
         this.grpSelectRecording.TabIndex = 4;
         // 
         // lblRecordings
         // 
         this.lblRecordings.AutoSize = true;
         this.lblRecordings.ForeColor = System.Drawing.Color.Silver;
         this.lblRecordings.Location = new System.Drawing.Point(25, 71);
         this.lblRecordings.Name = "lblRecordings";
         this.lblRecordings.Size = new System.Drawing.Size(61, 13);
         this.lblRecordings.TabIndex = 14;
         this.lblRecordings.Text = "Recordings";
         // 
         // lblCamera
         // 
         this.lblCamera.AutoSize = true;
         this.lblCamera.ForeColor = System.Drawing.Color.Silver;
         this.lblCamera.Location = new System.Drawing.Point(26, 36);
         this.lblCamera.Name = "lblCamera";
         this.lblCamera.Size = new System.Drawing.Size(43, 13);
         this.lblCamera.TabIndex = 13;
         this.lblCamera.Text = "Camera";
         // 
         // selCamera
         // 
         this.selCamera.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.selCamera.BackColor = System.Drawing.SystemColors.Window;
         this.selCamera.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
         this.selCamera.FormattingEnabled = true;
         this.selCamera.Location = new System.Drawing.Point(103, 33);
         this.selCamera.Name = "selCamera";
         this.selCamera.Size = new System.Drawing.Size(134, 21);
         this.selCamera.TabIndex = 12;
         this.selCamera.SelectedIndexChanged += new System.EventHandler(this.selCamera_SelectedIndexChanged);
         // 
         // selRecording
         // 
         this.selRecording.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.selRecording.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
         this.selRecording.FormattingEnabled = true;
         this.selRecording.Location = new System.Drawing.Point(103, 68);
         this.selRecording.Name = "selRecording";
         this.selRecording.Size = new System.Drawing.Size(265, 21);
         this.selRecording.TabIndex = 11;
         this.selRecording.SelectedIndexChanged += new System.EventHandler(this.selRecording_SelectedIndexChanged);
         // 
         // grpMessages
         // 
         this.grpMessages.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.grpMessages.Controls.Add(this.btnClearDownloadMessages);
         this.grpMessages.Controls.Add(this.lstDownloadMessages);
         this.grpMessages.Controls.Add(this.btnClearMessages);
         this.grpMessages.Location = new System.Drawing.Point(12, 408);
         this.grpMessages.Name = "grpMessages";
         this.grpMessages.SectionHeader = "Messages";
         this.grpMessages.Size = new System.Drawing.Size(690, 155);
         this.grpMessages.TabIndex = 7;
         // 
         // btnClearDownloadMessages
         // 
         this.btnClearDownloadMessages.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
         this.btnClearDownloadMessages.Location = new System.Drawing.Point(595, 120);
         this.btnClearDownloadMessages.Name = "btnClearDownloadMessages";
         this.btnClearDownloadMessages.Padding = new System.Windows.Forms.Padding(5);
         this.btnClearDownloadMessages.Size = new System.Drawing.Size(88, 29);
         this.btnClearDownloadMessages.TabIndex = 13;
         this.btnClearDownloadMessages.Text = "Clear";
         this.btnClearDownloadMessages.Click += new System.EventHandler(this.btnClearDownloadMessages_Click);
         // 
         // lstDownloadMessages
         // 
         this.lstDownloadMessages.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.lstDownloadMessages.Location = new System.Drawing.Point(3, 27);
         this.lstDownloadMessages.Name = "lstDownloadMessages";
         this.lstDownloadMessages.Size = new System.Drawing.Size(683, 91);
         this.lstDownloadMessages.TabIndex = 12;
         this.lstDownloadMessages.Text = "1";
         // 
         // btnClearMessages
         // 
         this.btnClearMessages.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
         this.btnClearMessages.Location = new System.Drawing.Point(598, 544);
         this.btnClearMessages.Name = "btnClearMessages";
         this.btnClearMessages.Padding = new System.Windows.Forms.Padding(5);
         this.btnClearMessages.Size = new System.Drawing.Size(88, 29);
         this.btnClearMessages.TabIndex = 11;
         this.btnClearMessages.Text = "Clear";
         // 
         // grpFinishedDownloads
         // 
         this.grpFinishedDownloads.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.grpFinishedDownloads.Controls.Add(this.btnClearFinishedDownloads);
         this.grpFinishedDownloads.Controls.Add(this.lstFinishedDownloads);
         this.grpFinishedDownloads.Location = new System.Drawing.Point(12, 208);
         this.grpFinishedDownloads.Name = "grpFinishedDownloads";
         this.grpFinishedDownloads.SectionHeader = "Finished Downloads";
         this.grpFinishedDownloads.Size = new System.Drawing.Size(690, 185);
         this.grpFinishedDownloads.TabIndex = 6;
         // 
         // btnClearFinishedDownloads
         // 
         this.btnClearFinishedDownloads.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
         this.btnClearFinishedDownloads.Location = new System.Drawing.Point(595, 151);
         this.btnClearFinishedDownloads.Name = "btnClearFinishedDownloads";
         this.btnClearFinishedDownloads.Padding = new System.Windows.Forms.Padding(5);
         this.btnClearFinishedDownloads.Size = new System.Drawing.Size(88, 29);
         this.btnClearFinishedDownloads.TabIndex = 12;
         this.btnClearFinishedDownloads.Text = "Clear";
         this.btnClearFinishedDownloads.Click += new System.EventHandler(this.btnClearFinishedDownloads_Click);
         // 
         // lstFinishedDownloads
         // 
         this.lstFinishedDownloads.Location = new System.Drawing.Point(4, 28);
         this.lstFinishedDownloads.Name = "lstFinishedDownloads";
         this.lstFinishedDownloads.Size = new System.Drawing.Size(682, 122);
         this.lstFinishedDownloads.TabIndex = 0;
         // 
         // grpPendingDownloads
         // 
         this.grpPendingDownloads.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.grpPendingDownloads.Controls.Add(this.btnClearPendingDownloads);
         this.grpPendingDownloads.Controls.Add(this.lstPendingDownloads);
         this.grpPendingDownloads.Location = new System.Drawing.Point(12, 12);
         this.grpPendingDownloads.Name = "grpPendingDownloads";
         this.grpPendingDownloads.SectionHeader = "Pending Downloads";
         this.grpPendingDownloads.Size = new System.Drawing.Size(690, 185);
         this.grpPendingDownloads.TabIndex = 5;
         // 
         // btnClearPendingDownloads
         // 
         this.btnClearPendingDownloads.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
         this.btnClearPendingDownloads.Location = new System.Drawing.Point(595, 151);
         this.btnClearPendingDownloads.Name = "btnClearPendingDownloads";
         this.btnClearPendingDownloads.Padding = new System.Windows.Forms.Padding(5);
         this.btnClearPendingDownloads.Size = new System.Drawing.Size(88, 29);
         this.btnClearPendingDownloads.TabIndex = 12;
         this.btnClearPendingDownloads.Text = "Clear";
         this.btnClearPendingDownloads.Click += new System.EventHandler(this.btnClearPendingDownloads_Click);
         // 
         // lstPendingDownloads
         // 
         this.lstPendingDownloads.Location = new System.Drawing.Point(4, 28);
         this.lstPendingDownloads.Name = "lstPendingDownloads";
         this.lstPendingDownloads.Size = new System.Drawing.Size(682, 122);
         this.lstPendingDownloads.TabIndex = 0;
         // 
         // DownloadWindow
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(1118, 573);
         this.Controls.Add(this.splitMainVerticalSplit);
         this.MinimumSize = new System.Drawing.Size(600, 200);
         this.Name = "DownloadWindow";
         this.Text = "Download Recording";
         this.splitMainVerticalSplit.Panel1.ResumeLayout(false);
         this.splitMainVerticalSplit.Panel2.ResumeLayout(false);
         ((System.ComponentModel.ISupportInitialize)(this.splitMainVerticalSplit)).EndInit();
         this.splitMainVerticalSplit.ResumeLayout(false);
         this.grpFTPConnection.ResumeLayout(false);
         this.grpFTPConnection.PerformLayout();
         this.grpDownloadPath.ResumeLayout(false);
         this.grpDownloadPath.PerformLayout();
         this.grpDownload.ResumeLayout(false);
         this.grpDownload.PerformLayout();
         this.grpSelectRecording.ResumeLayout(false);
         this.grpSelectRecording.PerformLayout();
         this.grpMessages.ResumeLayout(false);
         this.grpFinishedDownloads.ResumeLayout(false);
         this.grpPendingDownloads.ResumeLayout(false);
         this.ResumeLayout(false);

      }





      #endregion
      private System.Windows.Forms.SplitContainer splitMainVerticalSplit;
      //private System.Windows.Forms.SplitContainer splitHorizontalVideoAndMessageSplit;
      private DarkUI.Controls.DarkSectionPanel grpDownload;
      private System.Windows.Forms.Label recorderAddress;
      private DarkUI.Controls.DarkTextBox txtRecorderAddress;
      private System.Windows.Forms.Label stream;
      private System.Windows.Forms.Label duration;
      private System.Windows.Forms.Label startTime;
      private DarkUI.Controls.DarkButton btnDownloadRequest;
      private DarkUI.Controls.DarkSectionPanel grpSelectRecording;
      private System.Windows.Forms.DateTimePicker startTimePicker;
      private DarkUI.Controls.DarkComboBox selRecording;
      private DarkSectionPanel grpPendingDownloads;
      private DarkListView lstPendingDownloads;
      private DarkSectionPanel grpFinishedDownloads;
      private DarkListView lstFinishedDownloads;
      private DarkSectionPanel grpDownloadPath;
      private DarkButton btnBrowse;
      private DarkTextBox txtDownloadPath;
      private System.Windows.Forms.ComboBox selStreamNumber;
      private System.Windows.Forms.ComboBox selDuration;
      private DarkSectionPanel grpFTPConnection;
      private DarkTextBox txtFTPPassword;
      private System.Windows.Forms.Label lblPassword;
      private System.Windows.Forms.Label lblUser;
      private DarkTextBox txtFTPUser;
      private System.Windows.Forms.Label lblRecordings;
      private System.Windows.Forms.Label lblCamera;
      private System.Windows.Forms.ComboBox selCamera;
      private DarkSectionPanel grpMessages;
      private DarkListView lstDownloadMessages;
      private DarkButton btnClearMessages;
      private DarkButton btnClearDownloadMessages;
      private DarkButton btnClearFinishedDownloads;
      private DarkButton btnClearPendingDownloads;
   }
}

