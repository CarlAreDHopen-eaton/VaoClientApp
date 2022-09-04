
namespace Vao.Sample
{
   partial class MainWindow
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
         System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
         this.btnStart = new DarkUI.Controls.DarkButton();
         this.btnStop = new DarkUI.Controls.DarkButton();
         this.chkSecure = new DarkUI.Controls.DarkCheckBox();
         this.lblHost = new System.Windows.Forms.Label();
         this.txtHost = new DarkUI.Controls.DarkTextBox();
         this.txtPort = new DarkUI.Controls.DarkTextBox();
         this.lblPort = new System.Windows.Forms.Label();
         this.pnlVideo = new System.Windows.Forms.Panel();
         this.txtUser = new DarkUI.Controls.DarkTextBox();
         this.lblUser = new System.Windows.Forms.Label();
         this.txtPassword = new DarkUI.Controls.DarkTextBox();
         this.lblPassword = new System.Windows.Forms.Label();
         this.txtCurrentRtspUrl = new DarkUI.Controls.DarkTextBox();
         this.grpCameraSelection = new DarkUI.Controls.DarkSectionPanel();
         this.pnlCameraSelectFlowPanel = new System.Windows.Forms.FlowLayoutPanel();
         this.grpConnection = new DarkUI.Controls.DarkSectionPanel();
         this.grpMessages = new DarkUI.Controls.DarkSectionPanel();
         this.lstMessages = new DarkUI.Controls.DarkListView();
         this.btnClearMessages = new DarkUI.Controls.DarkButton();
         this.grpVideoControl = new System.Windows.Forms.Panel();
         this.chkPreferSubChannel = new DarkUI.Controls.DarkCheckBox();
         this.splitMainVerticalSplit = new System.Windows.Forms.SplitContainer();
         this.grpSelectPreset = new DarkUI.Controls.DarkSectionPanel();
         this.selPreset = new DarkUI.Controls.DarkComboBox();
         this.btnGotoPreset = new DarkUI.Controls.DarkButton();
         this.grpCameraControl = new DarkUI.Controls.DarkSectionPanel();
         this.btnFocusFar = new DarkUI.Controls.DarkButton();
         this.btnFocusNear = new DarkUI.Controls.DarkButton();
         this.pictureBox1 = new System.Windows.Forms.PictureBox();
         this.lblCurrentCamera = new System.Windows.Forms.Label();
         this.btnZoomOut = new DarkUI.Controls.DarkButton();
         this.btnZoomIn = new DarkUI.Controls.DarkButton();
         this.btnPanRight = new DarkUI.Controls.DarkButton();
         this.btnPanLeft = new DarkUI.Controls.DarkButton();
         this.btnTiltDown = new DarkUI.Controls.DarkButton();
         this.btnTiltUp = new DarkUI.Controls.DarkButton();
         this.splitHorizontalVideoAndMessageSplit = new System.Windows.Forms.SplitContainer();
         this.grpCameraSelection.SuspendLayout();
         this.grpConnection.SuspendLayout();
         this.grpMessages.SuspendLayout();
         this.grpVideoControl.SuspendLayout();
         ((System.ComponentModel.ISupportInitialize)(this.splitMainVerticalSplit)).BeginInit();
         this.splitMainVerticalSplit.Panel1.SuspendLayout();
         this.splitMainVerticalSplit.Panel2.SuspendLayout();
         this.splitMainVerticalSplit.SuspendLayout();
         this.grpSelectPreset.SuspendLayout();
         this.grpCameraControl.SuspendLayout();
         ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
         ((System.ComponentModel.ISupportInitialize)(this.splitHorizontalVideoAndMessageSplit)).BeginInit();
         this.splitHorizontalVideoAndMessageSplit.Panel1.SuspendLayout();
         this.splitHorizontalVideoAndMessageSplit.Panel2.SuspendLayout();
         this.splitHorizontalVideoAndMessageSplit.SuspendLayout();
         this.SuspendLayout();
         // 
         // btnStart
         // 
         this.btnStart.Location = new System.Drawing.Point(21, 156);
         this.btnStart.Name = "btnStart";
         this.btnStart.Padding = new System.Windows.Forms.Padding(5);
         this.btnStart.Size = new System.Drawing.Size(88, 28);
         this.btnStart.TabIndex = 9;
         this.btnStart.Text = "Start";
         this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
         // 
         // btnStop
         // 
         this.btnStop.Location = new System.Drawing.Point(115, 156);
         this.btnStop.Name = "btnStop";
         this.btnStop.Padding = new System.Windows.Forms.Padding(5);
         this.btnStop.Size = new System.Drawing.Size(88, 28);
         this.btnStop.TabIndex = 10;
         this.btnStop.Text = "Stop";
         this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
         // 
         // chkSecure
         // 
         this.chkSecure.AutoSize = true;
         this.chkSecure.Checked = true;
         this.chkSecure.CheckState = System.Windows.Forms.CheckState.Checked;
         this.chkSecure.Location = new System.Drawing.Point(21, 134);
         this.chkSecure.Name = "chkSecure";
         this.chkSecure.Size = new System.Drawing.Size(60, 17);
         this.chkSecure.TabIndex = 8;
         this.chkSecure.Text = "Secure";
         // 
         // lblHost
         // 
         this.lblHost.AutoSize = true;
         this.lblHost.ForeColor = System.Drawing.Color.Silver;
         this.lblHost.Location = new System.Drawing.Point(18, 34);
         this.lblHost.Name = "lblHost";
         this.lblHost.Size = new System.Drawing.Size(29, 13);
         this.lblHost.TabIndex = 0;
         this.lblHost.Text = "Host";
         // 
         // txtHost
         // 
         this.txtHost.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.txtHost.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
         this.txtHost.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
         this.txtHost.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
         this.txtHost.Location = new System.Drawing.Point(103, 31);
         this.txtHost.Name = "txtHost";
         this.txtHost.Size = new System.Drawing.Size(110, 20);
         this.txtHost.TabIndex = 1;
         // 
         // txtPort
         // 
         this.txtPort.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.txtPort.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
         this.txtPort.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
         this.txtPort.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
         this.txtPort.Location = new System.Drawing.Point(103, 57);
         this.txtPort.Name = "txtPort";
         this.txtPort.Size = new System.Drawing.Size(110, 20);
         this.txtPort.TabIndex = 3;
         this.txtPort.Text = "444";
         // 
         // lblPort
         // 
         this.lblPort.AutoSize = true;
         this.lblPort.ForeColor = System.Drawing.Color.Silver;
         this.lblPort.Location = new System.Drawing.Point(18, 60);
         this.lblPort.Name = "lblPort";
         this.lblPort.Size = new System.Drawing.Size(26, 13);
         this.lblPort.TabIndex = 2;
         this.lblPort.Text = "Port";
         // 
         // pnlVideo
         // 
         this.pnlVideo.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.pnlVideo.BackColor = System.Drawing.Color.Black;
         this.pnlVideo.Location = new System.Drawing.Point(1, 1);
         this.pnlVideo.Name = "pnlVideo";
         this.pnlVideo.Size = new System.Drawing.Size(988, 548);
         this.pnlVideo.TabIndex = 0;
         // 
         // txtUser
         // 
         this.txtUser.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.txtUser.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
         this.txtUser.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
         this.txtUser.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
         this.txtUser.Location = new System.Drawing.Point(103, 83);
         this.txtUser.Name = "txtUser";
         this.txtUser.Size = new System.Drawing.Size(110, 20);
         this.txtUser.TabIndex = 5;
         // 
         // lblUser
         // 
         this.lblUser.AutoSize = true;
         this.lblUser.ForeColor = System.Drawing.Color.Silver;
         this.lblUser.Location = new System.Drawing.Point(18, 86);
         this.lblUser.Name = "lblUser";
         this.lblUser.Size = new System.Drawing.Size(29, 13);
         this.lblUser.TabIndex = 4;
         this.lblUser.Text = "User";
         // 
         // txtPassword
         // 
         this.txtPassword.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.txtPassword.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
         this.txtPassword.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
         this.txtPassword.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
         this.txtPassword.Location = new System.Drawing.Point(103, 109);
         this.txtPassword.Name = "txtPassword";
         this.txtPassword.PasswordChar = '*';
         this.txtPassword.Size = new System.Drawing.Size(110, 20);
         this.txtPassword.TabIndex = 7;
         // 
         // lblPassword
         // 
         this.lblPassword.AutoSize = true;
         this.lblPassword.ForeColor = System.Drawing.Color.Silver;
         this.lblPassword.Location = new System.Drawing.Point(18, 112);
         this.lblPassword.Name = "lblPassword";
         this.lblPassword.Size = new System.Drawing.Size(53, 13);
         this.lblPassword.TabIndex = 6;
         this.lblPassword.Text = "Password";
         // 
         // txtCurrentRtspUrl
         // 
         this.txtCurrentRtspUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.txtCurrentRtspUrl.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
         this.txtCurrentRtspUrl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
         this.txtCurrentRtspUrl.Enabled = false;
         this.txtCurrentRtspUrl.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
         this.txtCurrentRtspUrl.Location = new System.Drawing.Point(0, 555);
         this.txtCurrentRtspUrl.Name = "txtCurrentRtspUrl";
         this.txtCurrentRtspUrl.Size = new System.Drawing.Size(862, 20);
         this.txtCurrentRtspUrl.TabIndex = 1;
         // 
         // grpCameraSelection
         // 
         this.grpCameraSelection.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.grpCameraSelection.Controls.Add(this.pnlCameraSelectFlowPanel);
         this.grpCameraSelection.Location = new System.Drawing.Point(6, 444);
         this.grpCameraSelection.Name = "grpCameraSelection";
         this.grpCameraSelection.SectionHeader = "Camera Selection";
         this.grpCameraSelection.Size = new System.Drawing.Size(224, 268);
         this.grpCameraSelection.TabIndex = 2;
         // 
         // pnlCameraSelectFlowPanel
         // 
         this.pnlCameraSelectFlowPanel.BackColor = System.Drawing.SystemColors.WindowFrame;
         this.pnlCameraSelectFlowPanel.Dock = System.Windows.Forms.DockStyle.Fill;
         this.pnlCameraSelectFlowPanel.Location = new System.Drawing.Point(1, 25);
         this.pnlCameraSelectFlowPanel.Name = "pnlCameraSelectFlowPanel";
         this.pnlCameraSelectFlowPanel.Size = new System.Drawing.Size(222, 242);
         this.pnlCameraSelectFlowPanel.TabIndex = 0;
         // 
         // grpConnection
         // 
         this.grpConnection.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.grpConnection.Controls.Add(this.lblHost);
         this.grpConnection.Controls.Add(this.chkSecure);
         this.grpConnection.Controls.Add(this.txtHost);
         this.grpConnection.Controls.Add(this.lblPort);
         this.grpConnection.Controls.Add(this.txtPassword);
         this.grpConnection.Controls.Add(this.txtPort);
         this.grpConnection.Controls.Add(this.lblPassword);
         this.grpConnection.Controls.Add(this.btnStop);
         this.grpConnection.Controls.Add(this.lblUser);
         this.grpConnection.Controls.Add(this.btnStart);
         this.grpConnection.Controls.Add(this.txtUser);
         this.grpConnection.Location = new System.Drawing.Point(6, 3);
         this.grpConnection.Name = "grpConnection";
         this.grpConnection.SectionHeader = "Connection";
         this.grpConnection.Size = new System.Drawing.Size(224, 189);
         this.grpConnection.TabIndex = 0;
         // 
         // grpMessages
         // 
         this.grpMessages.Controls.Add(this.lstMessages);
         this.grpMessages.Controls.Add(this.btnClearMessages);
         this.grpMessages.Dock = System.Windows.Forms.DockStyle.Fill;
         this.grpMessages.Location = new System.Drawing.Point(0, 0);
         this.grpMessages.Name = "grpMessages";
         this.grpMessages.SectionHeader = "Messages";
         this.grpMessages.Size = new System.Drawing.Size(989, 131);
         this.grpMessages.TabIndex = 0;
         // 
         // lstMessages
         // 
         this.lstMessages.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.lstMessages.Location = new System.Drawing.Point(3, 27);
         this.lstMessages.Name = "lstMessages";
         this.lstMessages.Size = new System.Drawing.Size(981, 64);
         this.lstMessages.TabIndex = 12;
         this.lstMessages.Text = "1";
         // 
         // btnClearMessages
         // 
         this.btnClearMessages.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
         this.btnClearMessages.Location = new System.Drawing.Point(897, 96);
         this.btnClearMessages.Name = "btnClearMessages";
         this.btnClearMessages.Padding = new System.Windows.Forms.Padding(5);
         this.btnClearMessages.Size = new System.Drawing.Size(88, 29);
         this.btnClearMessages.TabIndex = 11;
         this.btnClearMessages.Text = "Clear";
         this.btnClearMessages.Click += new System.EventHandler(this.btnClearMessages_Click);
         // 
         // grpVideoControl
         // 
         this.grpVideoControl.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
         this.grpVideoControl.Controls.Add(this.chkPreferSubChannel);
         this.grpVideoControl.Controls.Add(this.pnlVideo);
         this.grpVideoControl.Controls.Add(this.txtCurrentRtspUrl);
         this.grpVideoControl.Dock = System.Windows.Forms.DockStyle.Fill;
         this.grpVideoControl.ForeColor = System.Drawing.Color.White;
         this.grpVideoControl.Location = new System.Drawing.Point(0, 0);
         this.grpVideoControl.Name = "grpVideoControl";
         this.grpVideoControl.Size = new System.Drawing.Size(989, 580);
         this.grpVideoControl.TabIndex = 0;
         // 
         // chkPreferSubChannel
         // 
         this.chkPreferSubChannel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
         this.chkPreferSubChannel.AutoSize = true;
         this.chkPreferSubChannel.Location = new System.Drawing.Point(868, 556);
         this.chkPreferSubChannel.Name = "chkPreferSubChannel";
         this.chkPreferSubChannel.Size = new System.Drawing.Size(118, 17);
         this.chkPreferSubChannel.TabIndex = 9;
         this.chkPreferSubChannel.Text = "Prefer Sub Channel";
         this.chkPreferSubChannel.CheckedChanged += new System.EventHandler(this.chkPreferSubChannel_CheckedChanged);
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
         this.splitMainVerticalSplit.Panel1.Controls.Add(this.grpSelectPreset);
         this.splitMainVerticalSplit.Panel1.Controls.Add(this.grpCameraControl);
         this.splitMainVerticalSplit.Panel1.Controls.Add(this.grpConnection);
         this.splitMainVerticalSplit.Panel1.Controls.Add(this.grpCameraSelection);
         this.splitMainVerticalSplit.Panel1MinSize = 200;
         // 
         // splitMainVerticalSplit.Panel2
         // 
         this.splitMainVerticalSplit.Panel2.Controls.Add(this.splitHorizontalVideoAndMessageSplit);
         this.splitMainVerticalSplit.Size = new System.Drawing.Size(1226, 715);
         this.splitMainVerticalSplit.SplitterDistance = 233;
         this.splitMainVerticalSplit.TabIndex = 0;
         // 
         // grpSelectPreset
         // 
         this.grpSelectPreset.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.grpSelectPreset.Controls.Add(this.selPreset);
         this.grpSelectPreset.Controls.Add(this.btnGotoPreset);
         this.grpSelectPreset.Location = new System.Drawing.Point(6, 368);
         this.grpSelectPreset.Name = "grpSelectPreset";
         this.grpSelectPreset.SectionHeader = "Preset Selection";
         this.grpSelectPreset.Size = new System.Drawing.Size(221, 70);
         this.grpSelectPreset.TabIndex = 3;
         // 
         // selPreset
         // 
         this.selPreset.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.selPreset.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
         this.selPreset.FormattingEnabled = true;
         this.selPreset.Location = new System.Drawing.Point(9, 38);
         this.selPreset.Name = "selPreset";
         this.selPreset.Size = new System.Drawing.Size(157, 21);
         this.selPreset.TabIndex = 11;
         // 
         // btnGotoPreset
         // 
         this.btnGotoPreset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
         this.btnGotoPreset.Location = new System.Drawing.Point(172, 31);
         this.btnGotoPreset.Name = "btnGotoPreset";
         this.btnGotoPreset.Padding = new System.Windows.Forms.Padding(5);
         this.btnGotoPreset.Size = new System.Drawing.Size(43, 32);
         this.btnGotoPreset.TabIndex = 10;
         this.btnGotoPreset.Text = "Go";
         this.btnGotoPreset.Click += new System.EventHandler(this.btnGotoPreset_Click);
         // 
         // grpCameraControl
         // 
         this.grpCameraControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.grpCameraControl.Controls.Add(this.btnFocusFar);
         this.grpCameraControl.Controls.Add(this.btnFocusNear);
         this.grpCameraControl.Controls.Add(this.pictureBox1);
         this.grpCameraControl.Controls.Add(this.lblCurrentCamera);
         this.grpCameraControl.Controls.Add(this.btnZoomOut);
         this.grpCameraControl.Controls.Add(this.btnZoomIn);
         this.grpCameraControl.Controls.Add(this.btnPanRight);
         this.grpCameraControl.Controls.Add(this.btnPanLeft);
         this.grpCameraControl.Controls.Add(this.btnTiltDown);
         this.grpCameraControl.Controls.Add(this.btnTiltUp);
         this.grpCameraControl.Location = new System.Drawing.Point(6, 198);
         this.grpCameraControl.Name = "grpCameraControl";
         this.grpCameraControl.SectionHeader = "Camera Control";
         this.grpCameraControl.Size = new System.Drawing.Size(224, 164);
         this.grpCameraControl.TabIndex = 1;
         // 
         // btnFocusFar
         // 
         this.btnFocusFar.Image = global::Vao.Sample.Properties.Resources.flip_to_back_black_24dp;
         this.btnFocusFar.Location = new System.Drawing.Point(62, 107);
         this.btnFocusFar.Name = "btnFocusFar";
         this.btnFocusFar.Padding = new System.Windows.Forms.Padding(5);
         this.btnFocusFar.Size = new System.Drawing.Size(32, 32);
         this.btnFocusFar.TabIndex = 14;
         this.btnFocusFar.MouseDown += new System.Windows.Forms.MouseEventHandler(this.OnControlCameraMouseDown);
         this.btnFocusFar.MouseUp += new System.Windows.Forms.MouseEventHandler(this.OnControlCameraMouseUp);
         // 
         // btnFocusNear
         // 
         this.btnFocusNear.Image = global::Vao.Sample.Properties.Resources.flip_to_front_black_24dp;
         this.btnFocusNear.Location = new System.Drawing.Point(62, 31);
         this.btnFocusNear.Name = "btnFocusNear";
         this.btnFocusNear.Padding = new System.Windows.Forms.Padding(5);
         this.btnFocusNear.Size = new System.Drawing.Size(32, 32);
         this.btnFocusNear.TabIndex = 13;
         this.btnFocusNear.MouseDown += new System.Windows.Forms.MouseEventHandler(this.OnControlCameraMouseDown);
         this.btnFocusNear.MouseUp += new System.Windows.Forms.MouseEventHandler(this.OnControlCameraMouseUp);
         // 
         // pictureBox1
         // 
         this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
         this.pictureBox1.Location = new System.Drawing.Point(98, 69);
         this.pictureBox1.Name = "pictureBox1";
         this.pictureBox1.Size = new System.Drawing.Size(29, 32);
         this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
         this.pictureBox1.TabIndex = 12;
         this.pictureBox1.TabStop = false;
         // 
         // lblCurrentCamera
         // 
         this.lblCurrentCamera.AutoSize = true;
         this.lblCurrentCamera.ForeColor = System.Drawing.SystemColors.AppWorkspace;
         this.lblCurrentCamera.Location = new System.Drawing.Point(6, 146);
         this.lblCurrentCamera.Name = "lblCurrentCamera";
         this.lblCurrentCamera.Size = new System.Drawing.Size(156, 13);
         this.lblCurrentCamera.TabIndex = 11;
         this.lblCurrentCamera.Text = "Camera : (No Camera Selected)";
         // 
         // btnZoomOut
         // 
         this.btnZoomOut.Image = global::Vao.Sample.Properties.Resources.zoom_out_black_24dp;
         this.btnZoomOut.Location = new System.Drawing.Point(133, 107);
         this.btnZoomOut.Name = "btnZoomOut";
         this.btnZoomOut.Padding = new System.Windows.Forms.Padding(5);
         this.btnZoomOut.Size = new System.Drawing.Size(32, 32);
         this.btnZoomOut.TabIndex = 5;
         this.btnZoomOut.MouseDown += new System.Windows.Forms.MouseEventHandler(this.OnControlCameraMouseDown);
         this.btnZoomOut.MouseUp += new System.Windows.Forms.MouseEventHandler(this.OnControlCameraMouseUp);
         // 
         // btnZoomIn
         // 
         this.btnZoomIn.Image = global::Vao.Sample.Properties.Resources.zoom_in_black_24dp;
         this.btnZoomIn.Location = new System.Drawing.Point(133, 31);
         this.btnZoomIn.Name = "btnZoomIn";
         this.btnZoomIn.Padding = new System.Windows.Forms.Padding(5);
         this.btnZoomIn.Size = new System.Drawing.Size(32, 32);
         this.btnZoomIn.TabIndex = 4;
         this.btnZoomIn.MouseDown += new System.Windows.Forms.MouseEventHandler(this.OnControlCameraMouseDown);
         this.btnZoomIn.MouseUp += new System.Windows.Forms.MouseEventHandler(this.OnControlCameraMouseUp);
         // 
         // btnPanRight
         // 
         this.btnPanRight.Image = global::Vao.Sample.Properties.Resources.arrow_forward_black_24dp;
         this.btnPanRight.Location = new System.Drawing.Point(133, 69);
         this.btnPanRight.Name = "btnPanRight";
         this.btnPanRight.Padding = new System.Windows.Forms.Padding(5);
         this.btnPanRight.Size = new System.Drawing.Size(32, 32);
         this.btnPanRight.TabIndex = 2;
         this.btnPanRight.MouseDown += new System.Windows.Forms.MouseEventHandler(this.OnControlCameraMouseDown);
         this.btnPanRight.MouseUp += new System.Windows.Forms.MouseEventHandler(this.OnControlCameraMouseUp);
         // 
         // btnPanLeft
         // 
         this.btnPanLeft.Image = global::Vao.Sample.Properties.Resources.arrow_back_black_24dp;
         this.btnPanLeft.Location = new System.Drawing.Point(62, 69);
         this.btnPanLeft.Name = "btnPanLeft";
         this.btnPanLeft.Padding = new System.Windows.Forms.Padding(5);
         this.btnPanLeft.Size = new System.Drawing.Size(32, 32);
         this.btnPanLeft.TabIndex = 1;
         this.btnPanLeft.MouseDown += new System.Windows.Forms.MouseEventHandler(this.OnControlCameraMouseDown);
         this.btnPanLeft.MouseUp += new System.Windows.Forms.MouseEventHandler(this.OnControlCameraMouseUp);
         // 
         // btnTiltDown
         // 
         this.btnTiltDown.Image = global::Vao.Sample.Properties.Resources.arrow_downward_black_24dp;
         this.btnTiltDown.Location = new System.Drawing.Point(98, 107);
         this.btnTiltDown.Name = "btnTiltDown";
         this.btnTiltDown.Padding = new System.Windows.Forms.Padding(5);
         this.btnTiltDown.Size = new System.Drawing.Size(32, 32);
         this.btnTiltDown.TabIndex = 3;
         this.btnTiltDown.MouseDown += new System.Windows.Forms.MouseEventHandler(this.OnControlCameraMouseDown);
         this.btnTiltDown.MouseUp += new System.Windows.Forms.MouseEventHandler(this.OnControlCameraMouseUp);
         // 
         // btnTiltUp
         // 
         this.btnTiltUp.Image = global::Vao.Sample.Properties.Resources.arrow_upward_black_24dp;
         this.btnTiltUp.Location = new System.Drawing.Point(98, 31);
         this.btnTiltUp.Name = "btnTiltUp";
         this.btnTiltUp.Padding = new System.Windows.Forms.Padding(5);
         this.btnTiltUp.Size = new System.Drawing.Size(32, 32);
         this.btnTiltUp.TabIndex = 0;
         this.btnTiltUp.MouseDown += new System.Windows.Forms.MouseEventHandler(this.OnControlCameraMouseDown);
         this.btnTiltUp.MouseUp += new System.Windows.Forms.MouseEventHandler(this.OnControlCameraMouseUp);
         // 
         // splitHorizontalVideoAndMessageSplit
         // 
         this.splitHorizontalVideoAndMessageSplit.Dock = System.Windows.Forms.DockStyle.Fill;
         this.splitHorizontalVideoAndMessageSplit.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
         this.splitHorizontalVideoAndMessageSplit.Location = new System.Drawing.Point(0, 0);
         this.splitHorizontalVideoAndMessageSplit.Name = "splitHorizontalVideoAndMessageSplit";
         this.splitHorizontalVideoAndMessageSplit.Orientation = System.Windows.Forms.Orientation.Horizontal;
         // 
         // splitHorizontalVideoAndMessageSplit.Panel1
         // 
         this.splitHorizontalVideoAndMessageSplit.Panel1.Controls.Add(this.grpVideoControl);
         // 
         // splitHorizontalVideoAndMessageSplit.Panel2
         // 
         this.splitHorizontalVideoAndMessageSplit.Panel2.Controls.Add(this.grpMessages);
         this.splitHorizontalVideoAndMessageSplit.Size = new System.Drawing.Size(989, 715);
         this.splitHorizontalVideoAndMessageSplit.SplitterDistance = 580;
         this.splitHorizontalVideoAndMessageSplit.TabIndex = 0;
         // 
         // MainWindow
         // 
         this.AcceptButton = this.btnStart;
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(1226, 715);
         this.Controls.Add(this.splitMainVerticalSplit);
         this.MinimumSize = new System.Drawing.Size(400, 200);
         this.Name = "MainWindow";
         this.Text = "VAO Demo Client";
         this.grpCameraSelection.ResumeLayout(false);
         this.grpConnection.ResumeLayout(false);
         this.grpConnection.PerformLayout();
         this.grpMessages.ResumeLayout(false);
         this.grpVideoControl.ResumeLayout(false);
         this.grpVideoControl.PerformLayout();
         this.splitMainVerticalSplit.Panel1.ResumeLayout(false);
         this.splitMainVerticalSplit.Panel2.ResumeLayout(false);
         ((System.ComponentModel.ISupportInitialize)(this.splitMainVerticalSplit)).EndInit();
         this.splitMainVerticalSplit.ResumeLayout(false);
         this.grpSelectPreset.ResumeLayout(false);
         this.grpCameraControl.ResumeLayout(false);
         this.grpCameraControl.PerformLayout();
         ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
         this.splitHorizontalVideoAndMessageSplit.Panel1.ResumeLayout(false);
         this.splitHorizontalVideoAndMessageSplit.Panel2.ResumeLayout(false);
         ((System.ComponentModel.ISupportInitialize)(this.splitHorizontalVideoAndMessageSplit)).EndInit();
         this.splitHorizontalVideoAndMessageSplit.ResumeLayout(false);
         this.ResumeLayout(false);

      }

      #endregion

      private DarkUI.Controls.DarkButton btnStart;
      private DarkUI.Controls.DarkButton btnStop;
      private DarkUI.Controls.DarkCheckBox chkSecure;
      private System.Windows.Forms.Label lblHost;
      private DarkUI.Controls.DarkTextBox txtHost;
      private DarkUI.Controls.DarkTextBox txtPort;
      private System.Windows.Forms.Label lblPort;
      private System.Windows.Forms.Panel pnlVideo;
      private DarkUI.Controls.DarkTextBox txtUser;
      private System.Windows.Forms.Label lblUser;
      private DarkUI.Controls.DarkTextBox txtPassword;
      private System.Windows.Forms.Label lblPassword;
      private DarkUI.Controls.DarkTextBox txtCurrentRtspUrl;
      private DarkUI.Controls.DarkSectionPanel grpCameraSelection;
      private DarkUI.Controls.DarkSectionPanel grpConnection;
      private DarkUI.Controls.DarkSectionPanel grpMessages;
      private System.Windows.Forms.Panel grpVideoControl;
      private System.Windows.Forms.FlowLayoutPanel pnlCameraSelectFlowPanel;
      private System.Windows.Forms.SplitContainer splitMainVerticalSplit;
      private System.Windows.Forms.SplitContainer splitHorizontalVideoAndMessageSplit;
      private DarkUI.Controls.DarkSectionPanel grpCameraControl;
      private DarkUI.Controls.DarkButton btnPanRight;
      private DarkUI.Controls.DarkButton btnPanLeft;
      private DarkUI.Controls.DarkButton btnTiltDown;
      private DarkUI.Controls.DarkButton btnTiltUp;
      private DarkUI.Controls.DarkCheckBox chkPreferSubChannel;
      private DarkUI.Controls.DarkButton btnZoomOut;
      private DarkUI.Controls.DarkButton btnZoomIn;
      private System.Windows.Forms.Label lblCurrentCamera;
      private System.Windows.Forms.PictureBox pictureBox1;
      private DarkUI.Controls.DarkButton btnClearMessages;
      private DarkUI.Controls.DarkButton btnFocusFar;
      private DarkUI.Controls.DarkButton btnFocusNear;
      private DarkUI.Controls.DarkSectionPanel grpSelectPreset;
      private DarkUI.Controls.DarkComboBox selPreset;
      private DarkUI.Controls.DarkButton btnGotoPreset;
      private DarkUI.Controls.DarkListView lstMessages;
   }
}

