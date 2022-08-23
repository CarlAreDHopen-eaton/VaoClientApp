
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
         this.btnStart = new System.Windows.Forms.Button();
         this.btnStop = new System.Windows.Forms.Button();
         this.chkSecure = new System.Windows.Forms.CheckBox();
         this.lblHost = new System.Windows.Forms.Label();
         this.txtHost = new System.Windows.Forms.TextBox();
         this.txtPort = new System.Windows.Forms.TextBox();
         this.lblPort = new System.Windows.Forms.Label();
         this.pnlVideo = new System.Windows.Forms.Panel();
         this.lstMessages = new System.Windows.Forms.ListView();
         this.colTime = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
         this.colMessage = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
         this.txtUser = new System.Windows.Forms.TextBox();
         this.lblUser = new System.Windows.Forms.Label();
         this.txtPassword = new System.Windows.Forms.TextBox();
         this.lblPassword = new System.Windows.Forms.Label();
         this.txtCurrentRtspUrl = new System.Windows.Forms.TextBox();
         this.grpCameraSelection = new System.Windows.Forms.GroupBox();
         this.pnlCameraSelectFlowPanel = new System.Windows.Forms.FlowLayoutPanel();
         this.grpConnection = new System.Windows.Forms.GroupBox();
         this.grpMessages = new System.Windows.Forms.GroupBox();
         this.btnClearMessages = new System.Windows.Forms.Button();
         this.grpVideoControl = new System.Windows.Forms.GroupBox();
         this.chkPreferSubChannel = new System.Windows.Forms.CheckBox();
         this.splitMainVerticalSplit = new System.Windows.Forms.SplitContainer();
         this.grpCameraControl = new System.Windows.Forms.GroupBox();
         this.lblCurrentCamera = new System.Windows.Forms.Label();
         this.splitHorizontalVideoAndMessageSplit = new System.Windows.Forms.SplitContainer();
         this.btnFocusFar = new System.Windows.Forms.Button();
         this.btnFocusNear = new System.Windows.Forms.Button();
         this.pictureBox1 = new System.Windows.Forms.PictureBox();
         this.btnZoomOut = new System.Windows.Forms.Button();
         this.btnZoomIn = new System.Windows.Forms.Button();
         this.btnPanRight = new System.Windows.Forms.Button();
         this.btnPanLeft = new System.Windows.Forms.Button();
         this.btnTiltDown = new System.Windows.Forms.Button();
         this.btnTiltUp = new System.Windows.Forms.Button();
         this.colLevel = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
         this.grpCameraSelection.SuspendLayout();
         this.grpConnection.SuspendLayout();
         this.grpMessages.SuspendLayout();
         this.grpVideoControl.SuspendLayout();
         ((System.ComponentModel.ISupportInitialize)(this.splitMainVerticalSplit)).BeginInit();
         this.splitMainVerticalSplit.Panel1.SuspendLayout();
         this.splitMainVerticalSplit.Panel2.SuspendLayout();
         this.splitMainVerticalSplit.SuspendLayout();
         this.grpCameraControl.SuspendLayout();
         ((System.ComponentModel.ISupportInitialize)(this.splitHorizontalVideoAndMessageSplit)).BeginInit();
         this.splitHorizontalVideoAndMessageSplit.Panel1.SuspendLayout();
         this.splitHorizontalVideoAndMessageSplit.Panel2.SuspendLayout();
         this.splitHorizontalVideoAndMessageSplit.SuspendLayout();
         ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
         this.SuspendLayout();
         // 
         // btnStart
         // 
         this.btnStart.Location = new System.Drawing.Point(21, 146);
         this.btnStart.Name = "btnStart";
         this.btnStart.Size = new System.Drawing.Size(88, 32);
         this.btnStart.TabIndex = 9;
         this.btnStart.Text = "Start";
         this.btnStart.UseVisualStyleBackColor = true;
         this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
         // 
         // btnStop
         // 
         this.btnStop.Location = new System.Drawing.Point(115, 146);
         this.btnStop.Name = "btnStop";
         this.btnStop.Size = new System.Drawing.Size(88, 32);
         this.btnStop.TabIndex = 10;
         this.btnStop.Text = "Stop";
         this.btnStop.UseVisualStyleBackColor = true;
         this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
         // 
         // chkSecure
         // 
         this.chkSecure.AutoSize = true;
         this.chkSecure.Checked = true;
         this.chkSecure.CheckState = System.Windows.Forms.CheckState.Checked;
         this.chkSecure.Location = new System.Drawing.Point(21, 124);
         this.chkSecure.Name = "chkSecure";
         this.chkSecure.Size = new System.Drawing.Size(60, 17);
         this.chkSecure.TabIndex = 8;
         this.chkSecure.Text = "Secure";
         this.chkSecure.UseVisualStyleBackColor = true;
         // 
         // lblHost
         // 
         this.lblHost.AutoSize = true;
         this.lblHost.Location = new System.Drawing.Point(18, 22);
         this.lblHost.Name = "lblHost";
         this.lblHost.Size = new System.Drawing.Size(29, 13);
         this.lblHost.TabIndex = 0;
         this.lblHost.Text = "Host";
         // 
         // txtHost
         // 
         this.txtHost.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.txtHost.Location = new System.Drawing.Point(103, 19);
         this.txtHost.Name = "txtHost";
         this.txtHost.Size = new System.Drawing.Size(110, 20);
         this.txtHost.TabIndex = 1;
         // 
         // txtPort
         // 
         this.txtPort.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.txtPort.Location = new System.Drawing.Point(103, 45);
         this.txtPort.Name = "txtPort";
         this.txtPort.Size = new System.Drawing.Size(110, 20);
         this.txtPort.TabIndex = 3;
         this.txtPort.Text = "444";
         // 
         // lblPort
         // 
         this.lblPort.AutoSize = true;
         this.lblPort.Location = new System.Drawing.Point(18, 48);
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
         this.pnlVideo.Location = new System.Drawing.Point(6, 19);
         this.pnlVideo.Name = "pnlVideo";
         this.pnlVideo.Size = new System.Drawing.Size(838, 339);
         this.pnlVideo.TabIndex = 0;
         // 
         // lstMessages
         // 
         this.lstMessages.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.lstMessages.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colTime,
            this.colLevel,
            this.colMessage});
         this.lstMessages.FullRowSelect = true;
         this.lstMessages.HideSelection = false;
         this.lstMessages.Location = new System.Drawing.Point(3, 16);
         this.lstMessages.Name = "lstMessages";
         this.lstMessages.Size = new System.Drawing.Size(844, 196);
         this.lstMessages.TabIndex = 0;
         this.lstMessages.UseCompatibleStateImageBehavior = false;
         this.lstMessages.View = System.Windows.Forms.View.Details;
         // 
         // colTime
         // 
         this.colTime.Text = "Time";
         this.colTime.Width = 119;
         // 
         // colMessage
         // 
         this.colMessage.Text = "Message";
         this.colMessage.Width = 612;
         // 
         // txtUser
         // 
         this.txtUser.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.txtUser.Location = new System.Drawing.Point(103, 71);
         this.txtUser.Name = "txtUser";
         this.txtUser.Size = new System.Drawing.Size(110, 20);
         this.txtUser.TabIndex = 5;
         // 
         // lblUser
         // 
         this.lblUser.AutoSize = true;
         this.lblUser.Location = new System.Drawing.Point(18, 74);
         this.lblUser.Name = "lblUser";
         this.lblUser.Size = new System.Drawing.Size(29, 13);
         this.lblUser.TabIndex = 4;
         this.lblUser.Text = "User";
         // 
         // txtPassword
         // 
         this.txtPassword.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.txtPassword.Location = new System.Drawing.Point(103, 97);
         this.txtPassword.Name = "txtPassword";
         this.txtPassword.PasswordChar = '*';
         this.txtPassword.Size = new System.Drawing.Size(110, 20);
         this.txtPassword.TabIndex = 7;
         // 
         // lblPassword
         // 
         this.lblPassword.AutoSize = true;
         this.lblPassword.Location = new System.Drawing.Point(18, 100);
         this.lblPassword.Name = "lblPassword";
         this.lblPassword.Size = new System.Drawing.Size(53, 13);
         this.lblPassword.TabIndex = 6;
         this.lblPassword.Text = "Password";
         // 
         // txtCurrentRtspUrl
         // 
         this.txtCurrentRtspUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.txtCurrentRtspUrl.Enabled = false;
         this.txtCurrentRtspUrl.Location = new System.Drawing.Point(6, 364);
         this.txtCurrentRtspUrl.Name = "txtCurrentRtspUrl";
         this.txtCurrentRtspUrl.Size = new System.Drawing.Size(708, 20);
         this.txtCurrentRtspUrl.TabIndex = 1;
         // 
         // grpCameraSelection
         // 
         this.grpCameraSelection.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.grpCameraSelection.Controls.Add(this.pnlCameraSelectFlowPanel);
         this.grpCameraSelection.Location = new System.Drawing.Point(6, 358);
         this.grpCameraSelection.Name = "grpCameraSelection";
         this.grpCameraSelection.Size = new System.Drawing.Size(224, 283);
         this.grpCameraSelection.TabIndex = 2;
         this.grpCameraSelection.TabStop = false;
         this.grpCameraSelection.Text = "Camera Selection";
         // 
         // pnlCameraSelectFlowPanel
         // 
         this.pnlCameraSelectFlowPanel.Dock = System.Windows.Forms.DockStyle.Fill;
         this.pnlCameraSelectFlowPanel.Location = new System.Drawing.Point(3, 16);
         this.pnlCameraSelectFlowPanel.Name = "pnlCameraSelectFlowPanel";
         this.pnlCameraSelectFlowPanel.Size = new System.Drawing.Size(218, 264);
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
         this.grpConnection.Size = new System.Drawing.Size(224, 189);
         this.grpConnection.TabIndex = 0;
         this.grpConnection.TabStop = false;
         this.grpConnection.Text = "Connection";
         // 
         // grpMessages
         // 
         this.grpMessages.Controls.Add(this.btnClearMessages);
         this.grpMessages.Controls.Add(this.lstMessages);
         this.grpMessages.Dock = System.Windows.Forms.DockStyle.Fill;
         this.grpMessages.Location = new System.Drawing.Point(0, 0);
         this.grpMessages.Name = "grpMessages";
         this.grpMessages.Size = new System.Drawing.Size(850, 250);
         this.grpMessages.TabIndex = 0;
         this.grpMessages.TabStop = false;
         this.grpMessages.Text = "Messages";
         // 
         // btnClearMessages
         // 
         this.btnClearMessages.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
         this.btnClearMessages.Location = new System.Drawing.Point(759, 215);
         this.btnClearMessages.Name = "btnClearMessages";
         this.btnClearMessages.Size = new System.Drawing.Size(88, 32);
         this.btnClearMessages.TabIndex = 11;
         this.btnClearMessages.Text = "Clear";
         this.btnClearMessages.UseVisualStyleBackColor = true;
         this.btnClearMessages.Click += new System.EventHandler(this.btnClearMessages_Click);
         // 
         // grpVideoControl
         // 
         this.grpVideoControl.Controls.Add(this.chkPreferSubChannel);
         this.grpVideoControl.Controls.Add(this.pnlVideo);
         this.grpVideoControl.Controls.Add(this.txtCurrentRtspUrl);
         this.grpVideoControl.Dock = System.Windows.Forms.DockStyle.Fill;
         this.grpVideoControl.Location = new System.Drawing.Point(0, 0);
         this.grpVideoControl.Name = "grpVideoControl";
         this.grpVideoControl.Size = new System.Drawing.Size(850, 390);
         this.grpVideoControl.TabIndex = 0;
         this.grpVideoControl.TabStop = false;
         this.grpVideoControl.Text = "VLC Video Control";
         // 
         // chkPreferSubChannel
         // 
         this.chkPreferSubChannel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
         this.chkPreferSubChannel.AutoSize = true;
         this.chkPreferSubChannel.Location = new System.Drawing.Point(729, 366);
         this.chkPreferSubChannel.Name = "chkPreferSubChannel";
         this.chkPreferSubChannel.Size = new System.Drawing.Size(118, 17);
         this.chkPreferSubChannel.TabIndex = 9;
         this.chkPreferSubChannel.Text = "Prefer Sub Channel";
         this.chkPreferSubChannel.UseVisualStyleBackColor = true;
         this.chkPreferSubChannel.CheckedChanged += new System.EventHandler(this.chkPreferSubChannel_CheckedChanged);
         // 
         // splitMainVerticalSplit
         // 
         this.splitMainVerticalSplit.Dock = System.Windows.Forms.DockStyle.Fill;
         this.splitMainVerticalSplit.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
         this.splitMainVerticalSplit.Location = new System.Drawing.Point(0, 0);
         this.splitMainVerticalSplit.Name = "splitMainVerticalSplit";
         // 
         // splitMainVerticalSplit.Panel1
         // 
         this.splitMainVerticalSplit.Panel1.BackColor = System.Drawing.Color.White;
         this.splitMainVerticalSplit.Panel1.Controls.Add(this.grpCameraControl);
         this.splitMainVerticalSplit.Panel1.Controls.Add(this.grpConnection);
         this.splitMainVerticalSplit.Panel1.Controls.Add(this.grpCameraSelection);
         this.splitMainVerticalSplit.Panel1MinSize = 200;
         // 
         // splitMainVerticalSplit.Panel2
         // 
         this.splitMainVerticalSplit.Panel2.Controls.Add(this.splitHorizontalVideoAndMessageSplit);
         this.splitMainVerticalSplit.Size = new System.Drawing.Size(1087, 644);
         this.splitMainVerticalSplit.SplitterDistance = 233;
         this.splitMainVerticalSplit.TabIndex = 0;
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
         this.grpCameraControl.Size = new System.Drawing.Size(224, 154);
         this.grpCameraControl.TabIndex = 1;
         this.grpCameraControl.TabStop = false;
         this.grpCameraControl.Text = "Camera Control";
         // 
         // lblCurrentCamera
         // 
         this.lblCurrentCamera.AutoSize = true;
         this.lblCurrentCamera.Location = new System.Drawing.Point(6, 129);
         this.lblCurrentCamera.Name = "lblCurrentCamera";
         this.lblCurrentCamera.Size = new System.Drawing.Size(156, 13);
         this.lblCurrentCamera.TabIndex = 11;
         this.lblCurrentCamera.Text = "Camera : (No Camera Selected)";
         // 
         // splitHorizontalVideoAndMessageSplit
         // 
         this.splitHorizontalVideoAndMessageSplit.Dock = System.Windows.Forms.DockStyle.Fill;
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
         this.splitHorizontalVideoAndMessageSplit.Size = new System.Drawing.Size(850, 644);
         this.splitHorizontalVideoAndMessageSplit.SplitterDistance = 390;
         this.splitHorizontalVideoAndMessageSplit.TabIndex = 0;
         // 
         // btnFocusFar
         // 
         this.btnFocusFar.Image = global::Vao.Sample.Properties.Resources.flip_to_back_black_24dp;
         this.btnFocusFar.Location = new System.Drawing.Point(62, 90);
         this.btnFocusFar.Name = "btnFocusFar";
         this.btnFocusFar.Size = new System.Drawing.Size(32, 32);
         this.btnFocusFar.TabIndex = 14;
         this.btnFocusFar.UseVisualStyleBackColor = false;
         // 
         // btnFocusNear
         // 
         this.btnFocusNear.Image = global::Vao.Sample.Properties.Resources.flip_to_front_black_24dp;
         this.btnFocusNear.Location = new System.Drawing.Point(62, 14);
         this.btnFocusNear.Name = "btnFocusNear";
         this.btnFocusNear.Size = new System.Drawing.Size(32, 32);
         this.btnFocusNear.TabIndex = 13;
         this.btnFocusNear.UseVisualStyleBackColor = false;
         // 
         // pictureBox1
         // 
         this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
         this.pictureBox1.Location = new System.Drawing.Point(98, 52);
         this.pictureBox1.Name = "pictureBox1";
         this.pictureBox1.Size = new System.Drawing.Size(29, 32);
         this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
         this.pictureBox1.TabIndex = 12;
         this.pictureBox1.TabStop = false;
         // 
         // btnZoomOut
         // 
         this.btnZoomOut.Image = global::Vao.Sample.Properties.Resources.zoom_out_black_24dp;
         this.btnZoomOut.Location = new System.Drawing.Point(133, 90);
         this.btnZoomOut.Name = "btnZoomOut";
         this.btnZoomOut.Size = new System.Drawing.Size(32, 32);
         this.btnZoomOut.TabIndex = 5;
         this.btnZoomOut.UseVisualStyleBackColor = true;
         this.btnZoomOut.MouseDown += new System.Windows.Forms.MouseEventHandler(this.OnControlCameraMouseDown);
         this.btnZoomOut.MouseUp += new System.Windows.Forms.MouseEventHandler(this.OnControlCameraMouseUp);
         // 
         // btnZoomIn
         // 
         this.btnZoomIn.Image = global::Vao.Sample.Properties.Resources.zoom_in_black_24dp;
         this.btnZoomIn.Location = new System.Drawing.Point(133, 14);
         this.btnZoomIn.Name = "btnZoomIn";
         this.btnZoomIn.Size = new System.Drawing.Size(32, 32);
         this.btnZoomIn.TabIndex = 4;
         this.btnZoomIn.UseVisualStyleBackColor = true;
         this.btnZoomIn.MouseDown += new System.Windows.Forms.MouseEventHandler(this.OnControlCameraMouseDown);
         this.btnZoomIn.MouseUp += new System.Windows.Forms.MouseEventHandler(this.OnControlCameraMouseUp);
         // 
         // btnPanRight
         // 
         this.btnPanRight.Image = global::Vao.Sample.Properties.Resources.arrow_forward_black_24dp;
         this.btnPanRight.Location = new System.Drawing.Point(133, 52);
         this.btnPanRight.Name = "btnPanRight";
         this.btnPanRight.Size = new System.Drawing.Size(32, 32);
         this.btnPanRight.TabIndex = 2;
         this.btnPanRight.UseVisualStyleBackColor = false;
         this.btnPanRight.MouseDown += new System.Windows.Forms.MouseEventHandler(this.OnControlCameraMouseDown);
         this.btnPanRight.MouseUp += new System.Windows.Forms.MouseEventHandler(this.OnControlCameraMouseUp);
         // 
         // btnPanLeft
         // 
         this.btnPanLeft.Image = global::Vao.Sample.Properties.Resources.arrow_back_black_24dp;
         this.btnPanLeft.Location = new System.Drawing.Point(62, 52);
         this.btnPanLeft.Name = "btnPanLeft";
         this.btnPanLeft.Size = new System.Drawing.Size(32, 32);
         this.btnPanLeft.TabIndex = 1;
         this.btnPanLeft.UseVisualStyleBackColor = false;
         this.btnPanLeft.MouseDown += new System.Windows.Forms.MouseEventHandler(this.OnControlCameraMouseDown);
         this.btnPanLeft.MouseUp += new System.Windows.Forms.MouseEventHandler(this.OnControlCameraMouseUp);
         // 
         // btnTiltDown
         // 
         this.btnTiltDown.Image = global::Vao.Sample.Properties.Resources.arrow_downward_black_24dp;
         this.btnTiltDown.Location = new System.Drawing.Point(98, 90);
         this.btnTiltDown.Name = "btnTiltDown";
         this.btnTiltDown.Size = new System.Drawing.Size(32, 32);
         this.btnTiltDown.TabIndex = 3;
         this.btnTiltDown.UseVisualStyleBackColor = false;
         this.btnTiltDown.MouseDown += new System.Windows.Forms.MouseEventHandler(this.OnControlCameraMouseDown);
         this.btnTiltDown.MouseUp += new System.Windows.Forms.MouseEventHandler(this.OnControlCameraMouseUp);
         // 
         // btnTiltUp
         // 
         this.btnTiltUp.Image = global::Vao.Sample.Properties.Resources.arrow_upward_black_24dp;
         this.btnTiltUp.Location = new System.Drawing.Point(98, 14);
         this.btnTiltUp.Name = "btnTiltUp";
         this.btnTiltUp.Size = new System.Drawing.Size(32, 32);
         this.btnTiltUp.TabIndex = 0;
         this.btnTiltUp.UseVisualStyleBackColor = false;
         this.btnTiltUp.MouseDown += new System.Windows.Forms.MouseEventHandler(this.OnControlCameraMouseDown);
         this.btnTiltUp.MouseUp += new System.Windows.Forms.MouseEventHandler(this.OnControlCameraMouseUp);
         // 
         // colLevel
         // 
         this.colLevel.Text = "Level";
         this.colLevel.Width = 116;
         // 
         // MainWindow
         // 
         this.AcceptButton = this.btnStart;
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(1087, 644);
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
         this.grpCameraControl.ResumeLayout(false);
         this.grpCameraControl.PerformLayout();
         this.splitHorizontalVideoAndMessageSplit.Panel1.ResumeLayout(false);
         this.splitHorizontalVideoAndMessageSplit.Panel2.ResumeLayout(false);
         ((System.ComponentModel.ISupportInitialize)(this.splitHorizontalVideoAndMessageSplit)).EndInit();
         this.splitHorizontalVideoAndMessageSplit.ResumeLayout(false);
         ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
         this.ResumeLayout(false);

      }

      #endregion

      private System.Windows.Forms.Button btnStart;
      private System.Windows.Forms.Button btnStop;
      private System.Windows.Forms.CheckBox chkSecure;
      private System.Windows.Forms.Label lblHost;
      private System.Windows.Forms.TextBox txtHost;
      private System.Windows.Forms.TextBox txtPort;
      private System.Windows.Forms.Label lblPort;
      private System.Windows.Forms.Panel pnlVideo;
      private System.Windows.Forms.ListView lstMessages;
      private System.Windows.Forms.ColumnHeader colTime;
      private System.Windows.Forms.ColumnHeader colMessage;
      private System.Windows.Forms.TextBox txtUser;
      private System.Windows.Forms.Label lblUser;
      private System.Windows.Forms.TextBox txtPassword;
      private System.Windows.Forms.Label lblPassword;
      private System.Windows.Forms.TextBox txtCurrentRtspUrl;
      private System.Windows.Forms.GroupBox grpCameraSelection;
      private System.Windows.Forms.GroupBox grpConnection;
      private System.Windows.Forms.GroupBox grpMessages;
      private System.Windows.Forms.GroupBox grpVideoControl;
      private System.Windows.Forms.FlowLayoutPanel pnlCameraSelectFlowPanel;
      private System.Windows.Forms.SplitContainer splitMainVerticalSplit;
      private System.Windows.Forms.SplitContainer splitHorizontalVideoAndMessageSplit;
      private System.Windows.Forms.GroupBox grpCameraControl;
      private System.Windows.Forms.Button btnPanRight;
      private System.Windows.Forms.Button btnPanLeft;
      private System.Windows.Forms.Button btnTiltDown;
      private System.Windows.Forms.Button btnTiltUp;
      private System.Windows.Forms.CheckBox chkPreferSubChannel;
      private System.Windows.Forms.Button btnZoomOut;
      private System.Windows.Forms.Button btnZoomIn;
      private System.Windows.Forms.Label lblCurrentCamera;
      private System.Windows.Forms.PictureBox pictureBox1;
      private System.Windows.Forms.Button btnClearMessages;
      private System.Windows.Forms.Button btnFocusFar;
      private System.Windows.Forms.Button btnFocusNear;
      private System.Windows.Forms.ColumnHeader colLevel;
   }
}

