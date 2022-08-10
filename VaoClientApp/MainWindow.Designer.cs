
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
         this.label1 = new System.Windows.Forms.Label();
         this.txtHost = new System.Windows.Forms.TextBox();
         this.txtPort = new System.Windows.Forms.TextBox();
         this.label2 = new System.Windows.Forms.Label();
         this.pnlVideo = new System.Windows.Forms.Panel();
         this.lstMessages = new System.Windows.Forms.ListView();
         this.colTime = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
         this.colMessage = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
         this.txtUser = new System.Windows.Forms.TextBox();
         this.label4 = new System.Windows.Forms.Label();
         this.txtPassword = new System.Windows.Forms.TextBox();
         this.label5 = new System.Windows.Forms.Label();
         this.textBox1 = new System.Windows.Forms.TextBox();
         this.grpCameraSelection = new System.Windows.Forms.GroupBox();
         this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
         this.grpConnection = new System.Windows.Forms.GroupBox();
         this.grpMessages = new System.Windows.Forms.GroupBox();
         this.grpVideoControl = new System.Windows.Forms.GroupBox();
         this.splitContainer1 = new System.Windows.Forms.SplitContainer();
         this.grpCameraControl = new System.Windows.Forms.GroupBox();
         this.btnPanRight = new System.Windows.Forms.Button();
         this.btHome = new System.Windows.Forms.Button();
         this.btnPanLeft = new System.Windows.Forms.Button();
         this.btnTiltDown = new System.Windows.Forms.Button();
         this.btnTiltUp = new System.Windows.Forms.Button();
         this.splitContainer2 = new System.Windows.Forms.SplitContainer();
         this.grpCameraSelection.SuspendLayout();
         this.grpConnection.SuspendLayout();
         this.grpMessages.SuspendLayout();
         this.grpVideoControl.SuspendLayout();
         ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
         this.splitContainer1.Panel1.SuspendLayout();
         this.splitContainer1.Panel2.SuspendLayout();
         this.splitContainer1.SuspendLayout();
         this.grpCameraControl.SuspendLayout();
         ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
         this.splitContainer2.Panel1.SuspendLayout();
         this.splitContainer2.Panel2.SuspendLayout();
         this.splitContainer2.SuspendLayout();
         this.SuspendLayout();
         // 
         // btnStart
         // 
         this.btnStart.Location = new System.Drawing.Point(21, 146);
         this.btnStart.Name = "btnStart";
         this.btnStart.Size = new System.Drawing.Size(88, 32);
         this.btnStart.TabIndex = 0;
         this.btnStart.Text = "Start";
         this.btnStart.UseVisualStyleBackColor = true;
         this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
         // 
         // btnStop
         // 
         this.btnStop.Location = new System.Drawing.Point(115, 146);
         this.btnStop.Name = "btnStop";
         this.btnStop.Size = new System.Drawing.Size(88, 32);
         this.btnStop.TabIndex = 1;
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
         this.chkSecure.TabIndex = 2;
         this.chkSecure.Text = "Secure";
         this.chkSecure.UseVisualStyleBackColor = true;
         // 
         // label1
         // 
         this.label1.AutoSize = true;
         this.label1.Location = new System.Drawing.Point(18, 22);
         this.label1.Name = "label1";
         this.label1.Size = new System.Drawing.Size(29, 13);
         this.label1.TabIndex = 3;
         this.label1.Text = "Host";
         // 
         // txtHost
         // 
         this.txtHost.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.txtHost.Location = new System.Drawing.Point(103, 19);
         this.txtHost.Name = "txtHost";
         this.txtHost.Size = new System.Drawing.Size(110, 20);
         this.txtHost.TabIndex = 4;
         // 
         // txtPort
         // 
         this.txtPort.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.txtPort.Location = new System.Drawing.Point(103, 45);
         this.txtPort.Name = "txtPort";
         this.txtPort.Size = new System.Drawing.Size(110, 20);
         this.txtPort.TabIndex = 6;
         this.txtPort.Text = "444";
         // 
         // label2
         // 
         this.label2.AutoSize = true;
         this.label2.Location = new System.Drawing.Point(18, 48);
         this.label2.Name = "label2";
         this.label2.Size = new System.Drawing.Size(26, 13);
         this.label2.TabIndex = 5;
         this.label2.Text = "Port";
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
         this.pnlVideo.TabIndex = 7;
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
         this.lstMessages.TabIndex = 8;
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
         this.txtUser.TabIndex = 11;
         // 
         // label4
         // 
         this.label4.AutoSize = true;
         this.label4.Location = new System.Drawing.Point(18, 74);
         this.label4.Name = "label4";
         this.label4.Size = new System.Drawing.Size(29, 13);
         this.label4.TabIndex = 10;
         this.label4.Text = "User";
         // 
         // txtPassword
         // 
         this.txtPassword.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.txtPassword.Location = new System.Drawing.Point(103, 97);
         this.txtPassword.Name = "txtPassword";
         this.txtPassword.PasswordChar = '*';
         this.txtPassword.Size = new System.Drawing.Size(110, 20);
         this.txtPassword.TabIndex = 13;
         // 
         // label5
         // 
         this.label5.AutoSize = true;
         this.label5.Location = new System.Drawing.Point(18, 100);
         this.label5.Name = "label5";
         this.label5.Size = new System.Drawing.Size(53, 13);
         this.label5.TabIndex = 12;
         this.label5.Text = "Password";
         // 
         // textBox1
         // 
         this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.textBox1.Enabled = false;
         this.textBox1.Location = new System.Drawing.Point(6, 364);
         this.textBox1.Name = "textBox1";
         this.textBox1.Size = new System.Drawing.Size(838, 20);
         this.textBox1.TabIndex = 15;
         // 
         // grpCameraSelection
         // 
         this.grpCameraSelection.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.grpCameraSelection.Controls.Add(this.flowLayoutPanel1);
         this.grpCameraSelection.Location = new System.Drawing.Point(6, 333);
         this.grpCameraSelection.Name = "grpCameraSelection";
         this.grpCameraSelection.Size = new System.Drawing.Size(224, 308);
         this.grpCameraSelection.TabIndex = 25;
         this.grpCameraSelection.TabStop = false;
         this.grpCameraSelection.Text = "Camera Selection";
         // 
         // flowLayoutPanel1
         // 
         this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
         this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 16);
         this.flowLayoutPanel1.Name = "flowLayoutPanel1";
         this.flowLayoutPanel1.Size = new System.Drawing.Size(218, 289);
         this.flowLayoutPanel1.TabIndex = 25;
         // 
         // grpConnection
         // 
         this.grpConnection.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.grpConnection.Controls.Add(this.label1);
         this.grpConnection.Controls.Add(this.chkSecure);
         this.grpConnection.Controls.Add(this.txtHost);
         this.grpConnection.Controls.Add(this.label2);
         this.grpConnection.Controls.Add(this.txtPassword);
         this.grpConnection.Controls.Add(this.txtPort);
         this.grpConnection.Controls.Add(this.label5);
         this.grpConnection.Controls.Add(this.btnStop);
         this.grpConnection.Controls.Add(this.label4);
         this.grpConnection.Controls.Add(this.btnStart);
         this.grpConnection.Controls.Add(this.txtUser);
         this.grpConnection.Location = new System.Drawing.Point(6, 3);
         this.grpConnection.Name = "grpConnection";
         this.grpConnection.Size = new System.Drawing.Size(224, 189);
         this.grpConnection.TabIndex = 26;
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
         this.grpMessages.TabIndex = 27;
         this.grpMessages.TabStop = false;
         this.grpMessages.Text = "Messages";
         // 
         // grpVideoControl
         // 
         this.grpVideoControl.Controls.Add(this.pnlVideo);
         this.grpVideoControl.Controls.Add(this.textBox1);
         this.grpVideoControl.Dock = System.Windows.Forms.DockStyle.Fill;
         this.grpVideoControl.Location = new System.Drawing.Point(0, 0);
         this.grpVideoControl.Name = "grpVideoControl";
         this.grpVideoControl.Size = new System.Drawing.Size(850, 390);
         this.grpVideoControl.TabIndex = 28;
         this.grpVideoControl.TabStop = false;
         this.grpVideoControl.Text = "VLC Video Control";
         // 
         // splitContainer1
         // 
         this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
         this.splitContainer1.Location = new System.Drawing.Point(0, 0);
         this.splitContainer1.Name = "splitContainer1";
         // 
         // splitContainer1.Panel1
         // 
         this.splitContainer1.Panel1.BackColor = System.Drawing.Color.White;
         this.splitContainer1.Panel1.Controls.Add(this.grpCameraControl);
         this.splitContainer1.Panel1.Controls.Add(this.grpConnection);
         this.splitContainer1.Panel1.Controls.Add(this.grpCameraSelection);
         this.splitContainer1.Panel1MinSize = 200;
         // 
         // splitContainer1.Panel2
         // 
         this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
         this.splitContainer1.Size = new System.Drawing.Size(1087, 644);
         this.splitContainer1.SplitterDistance = 233;
         this.splitContainer1.TabIndex = 29;
         // 
         // grpCameraControl
         // 
         this.grpCameraControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.grpCameraControl.Controls.Add(this.btnPanRight);
         this.grpCameraControl.Controls.Add(this.btHome);
         this.grpCameraControl.Controls.Add(this.btnPanLeft);
         this.grpCameraControl.Controls.Add(this.btnTiltDown);
         this.grpCameraControl.Controls.Add(this.btnTiltUp);
         this.grpCameraControl.Location = new System.Drawing.Point(6, 198);
         this.grpCameraControl.Name = "grpCameraControl";
         this.grpCameraControl.Size = new System.Drawing.Size(224, 129);
         this.grpCameraControl.TabIndex = 27;
         this.grpCameraControl.TabStop = false;
         this.grpCameraControl.Text = "Camera Control";
         // 
         // btnPanRight
         // 
         this.btnPanRight.Location = new System.Drawing.Point(129, 53);
         this.btnPanRight.Name = "btnPanRight";
         this.btnPanRight.Size = new System.Drawing.Size(29, 32);
         this.btnPanRight.TabIndex = 4;
         this.btnPanRight.Text = "Ri";
         this.btnPanRight.UseVisualStyleBackColor = true;
         this.btnPanRight.MouseDown += new System.Windows.Forms.MouseEventHandler(this.OnControlCameraMouseDown);
         this.btnPanRight.MouseUp += new System.Windows.Forms.MouseEventHandler(this.OnControlCameraMouseUp);
         // 
         // btHome
         // 
         this.btHome.Location = new System.Drawing.Point(94, 53);
         this.btHome.Name = "btHome";
         this.btHome.Size = new System.Drawing.Size(29, 32);
         this.btHome.TabIndex = 3;
         this.btHome.Text = "Ho";
         this.btHome.UseVisualStyleBackColor = true;
         // 
         // btnPanLeft
         // 
         this.btnPanLeft.Location = new System.Drawing.Point(63, 53);
         this.btnPanLeft.Name = "btnPanLeft";
         this.btnPanLeft.Size = new System.Drawing.Size(29, 32);
         this.btnPanLeft.TabIndex = 2;
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
         this.btnTiltDown.TabIndex = 1;
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
         // splitContainer2
         // 
         this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
         this.splitContainer2.Location = new System.Drawing.Point(0, 0);
         this.splitContainer2.Name = "splitContainer2";
         this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
         // 
         // splitContainer2.Panel1
         // 
         this.splitContainer2.Panel1.Controls.Add(this.grpVideoControl);
         // 
         // splitContainer2.Panel2
         // 
         this.splitContainer2.Panel2.Controls.Add(this.grpMessages);
         this.splitContainer2.Size = new System.Drawing.Size(850, 644);
         this.splitContainer2.SplitterDistance = 390;
         this.splitContainer2.TabIndex = 0;
         // 
         // MainWindow
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(1087, 644);
         this.Controls.Add(this.splitContainer1);
         this.Name = "MainWindow";
         this.Text = "VAO Demo Client";
         this.grpCameraSelection.ResumeLayout(false);
         this.grpConnection.ResumeLayout(false);
         this.grpConnection.PerformLayout();
         this.grpMessages.ResumeLayout(false);
         this.grpVideoControl.ResumeLayout(false);
         this.grpVideoControl.PerformLayout();
         this.splitContainer1.Panel1.ResumeLayout(false);
         this.splitContainer1.Panel2.ResumeLayout(false);
         ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
         this.splitContainer1.ResumeLayout(false);
         this.grpCameraControl.ResumeLayout(false);
         this.splitContainer2.Panel1.ResumeLayout(false);
         this.splitContainer2.Panel2.ResumeLayout(false);
         ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
         this.splitContainer2.ResumeLayout(false);
         this.ResumeLayout(false);

      }

      #endregion

      private System.Windows.Forms.Button btnStart;
      private System.Windows.Forms.Button btnStop;
      private System.Windows.Forms.CheckBox chkSecure;
      private System.Windows.Forms.Label label1;
      private System.Windows.Forms.TextBox txtHost;
      private System.Windows.Forms.TextBox txtPort;
      private System.Windows.Forms.Label label2;
      private System.Windows.Forms.Panel pnlVideo;
      private System.Windows.Forms.ListView lstMessages;
      private System.Windows.Forms.ColumnHeader colTime;
      private System.Windows.Forms.ColumnHeader colMessage;
      private System.Windows.Forms.TextBox txtUser;
      private System.Windows.Forms.Label label4;
      private System.Windows.Forms.TextBox txtPassword;
      private System.Windows.Forms.Label label5;
      private System.Windows.Forms.TextBox textBox1;
      private System.Windows.Forms.GroupBox grpCameraSelection;
      private System.Windows.Forms.GroupBox grpConnection;
      private System.Windows.Forms.GroupBox grpMessages;
      private System.Windows.Forms.GroupBox grpVideoControl;
      private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
      private System.Windows.Forms.SplitContainer splitContainer1;
      private System.Windows.Forms.SplitContainer splitContainer2;
      private System.Windows.Forms.GroupBox grpCameraControl;
      private System.Windows.Forms.Button btnPanRight;
      private System.Windows.Forms.Button btHome;
      private System.Windows.Forms.Button btnPanLeft;
      private System.Windows.Forms.Button btnTiltDown;
      private System.Windows.Forms.Button btnTiltUp;
   }
}

