
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
         this.grpVideoControl = new System.Windows.Forms.GroupBox();
         this.splitMainVerticalSplit = new System.Windows.Forms.SplitContainer();
         this.grpCameraControl = new System.Windows.Forms.GroupBox();
         this.btnPanRight = new System.Windows.Forms.Button();
         this.btnPanLeft = new System.Windows.Forms.Button();
         this.btnTiltDown = new System.Windows.Forms.Button();
         this.btnTiltUp = new System.Windows.Forms.Button();
         this.splitHorizontalVideoAndMessageSplit = new System.Windows.Forms.SplitContainer();
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
         this.lstMessages.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colTime,
            this.colMessage});
         this.lstMessages.Dock = System.Windows.Forms.DockStyle.Fill;
         this.lstMessages.HideSelection = false;
         this.lstMessages.Location = new System.Drawing.Point(3, 16);
         this.lstMessages.Name = "lstMessages";
         this.lstMessages.Size = new System.Drawing.Size(844, 231);
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
         this.txtCurrentRtspUrl.Size = new System.Drawing.Size(838, 20);
         this.txtCurrentRtspUrl.TabIndex = 1;
         // 
         // grpCameraSelection
         // 
         this.grpCameraSelection.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.grpCameraSelection.Controls.Add(this.pnlCameraSelectFlowPanel);
         this.grpCameraSelection.Location = new System.Drawing.Point(6, 333);
         this.grpCameraSelection.Name = "grpCameraSelection";
         this.grpCameraSelection.Size = new System.Drawing.Size(224, 308);
         this.grpCameraSelection.TabIndex = 2;
         this.grpCameraSelection.TabStop = false;
         this.grpCameraSelection.Text = "Camera Selection";
         // 
         // pnlCameraSelectFlowPanel
         // 
         this.pnlCameraSelectFlowPanel.Dock = System.Windows.Forms.DockStyle.Fill;
         this.pnlCameraSelectFlowPanel.Location = new System.Drawing.Point(3, 16);
         this.pnlCameraSelectFlowPanel.Name = "pnlCameraSelectFlowPanel";
         this.pnlCameraSelectFlowPanel.Size = new System.Drawing.Size(218, 289);
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
         this.grpMessages.Controls.Add(this.lstMessages);
         this.grpMessages.Dock = System.Windows.Forms.DockStyle.Fill;
         this.grpMessages.Location = new System.Drawing.Point(0, 0);
         this.grpMessages.Name = "grpMessages";
         this.grpMessages.Size = new System.Drawing.Size(850, 250);
         this.grpMessages.TabIndex = 0;
         this.grpMessages.TabStop = false;
         this.grpMessages.Text = "Messages";
         // 
         // grpVideoControl
         // 
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
         // splitMainVerticalSplit
         // 
         this.splitMainVerticalSplit.Dock = System.Windows.Forms.DockStyle.Fill;
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
         this.grpCameraControl.Controls.Add(this.btnPanRight);
         this.grpCameraControl.Controls.Add(this.btnPanLeft);
         this.grpCameraControl.Controls.Add(this.btnTiltDown);
         this.grpCameraControl.Controls.Add(this.btnTiltUp);
         this.grpCameraControl.Location = new System.Drawing.Point(6, 198);
         this.grpCameraControl.Name = "grpCameraControl";
         this.grpCameraControl.Size = new System.Drawing.Size(224, 129);
         this.grpCameraControl.TabIndex = 1;
         this.grpCameraControl.TabStop = false;
         this.grpCameraControl.Text = "Camera Control";
         // 
         // btnPanRight
         // 
         this.btnPanRight.Location = new System.Drawing.Point(129, 53);
         this.btnPanRight.Name = "btnPanRight";
         this.btnPanRight.Size = new System.Drawing.Size(29, 32);
         this.btnPanRight.TabIndex = 2;
         this.btnPanRight.Text = "Ri";
         this.btnPanRight.UseVisualStyleBackColor = true;
         this.btnPanRight.MouseDown += new System.Windows.Forms.MouseEventHandler(this.OnControlCameraMouseDown);
         this.btnPanRight.MouseUp += new System.Windows.Forms.MouseEventHandler(this.OnControlCameraMouseUp);
         // 
         // btnPanLeft
         // 
         this.btnPanLeft.Location = new System.Drawing.Point(59, 53);
         this.btnPanLeft.Name = "btnPanLeft";
         this.btnPanLeft.Size = new System.Drawing.Size(29, 32);
         this.btnPanLeft.TabIndex = 1;
         this.btnPanLeft.Text = "Le";
         this.btnPanLeft.UseVisualStyleBackColor = true;
         this.btnPanLeft.MouseDown += new System.Windows.Forms.MouseEventHandler(this.OnControlCameraMouseDown);
         this.btnPanLeft.MouseUp += new System.Windows.Forms.MouseEventHandler(this.OnControlCameraMouseUp);
         // 
         // btnTiltDown
         // 
         this.btnTiltDown.Location = new System.Drawing.Point(94, 91);
         this.btnTiltDown.Name = "btnTiltDown";
         this.btnTiltDown.Size = new System.Drawing.Size(29, 32);
         this.btnTiltDown.TabIndex = 3;
         this.btnTiltDown.Text = "Do";
         this.btnTiltDown.UseVisualStyleBackColor = true;
         this.btnTiltDown.MouseDown += new System.Windows.Forms.MouseEventHandler(this.OnControlCameraMouseDown);
         this.btnTiltDown.MouseUp += new System.Windows.Forms.MouseEventHandler(this.OnControlCameraMouseUp);
         // 
         // btnTiltUp
         // 
         this.btnTiltUp.Location = new System.Drawing.Point(94, 15);
         this.btnTiltUp.Name = "btnTiltUp";
         this.btnTiltUp.Size = new System.Drawing.Size(29, 32);
         this.btnTiltUp.TabIndex = 0;
         this.btnTiltUp.Text = "Up";
         this.btnTiltUp.UseVisualStyleBackColor = true;
         this.btnTiltUp.MouseDown += new System.Windows.Forms.MouseEventHandler(this.OnControlCameraMouseDown);
         this.btnTiltUp.MouseUp += new System.Windows.Forms.MouseEventHandler(this.OnControlCameraMouseUp);
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
         // MainWindow
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(1087, 644);
         this.Controls.Add(this.splitMainVerticalSplit);
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
         this.splitHorizontalVideoAndMessageSplit.Panel1.ResumeLayout(false);
         this.splitHorizontalVideoAndMessageSplit.Panel2.ResumeLayout(false);
         ((System.ComponentModel.ISupportInitialize)(this.splitHorizontalVideoAndMessageSplit)).EndInit();
         this.splitHorizontalVideoAndMessageSplit.ResumeLayout(false);
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
   }
}

