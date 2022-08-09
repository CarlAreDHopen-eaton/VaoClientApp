
namespace VaoClientApp
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
         this.button1 = new System.Windows.Forms.Button();
         this.textBox1 = new System.Windows.Forms.TextBox();
         this.button2 = new System.Windows.Forms.Button();
         this.button3 = new System.Windows.Forms.Button();
         this.button4 = new System.Windows.Forms.Button();
         this.button5 = new System.Windows.Forms.Button();
         this.button6 = new System.Windows.Forms.Button();
         this.button7 = new System.Windows.Forms.Button();
         this.button8 = new System.Windows.Forms.Button();
         this.button9 = new System.Windows.Forms.Button();
         this.grpCameraSelection = new System.Windows.Forms.GroupBox();
         this.grpConnection = new System.Windows.Forms.GroupBox();
         this.grpMessages = new System.Windows.Forms.GroupBox();
         this.groupBox4 = new System.Windows.Forms.GroupBox();
         this.grpCameraSelection.SuspendLayout();
         this.grpConnection.SuspendLayout();
         this.grpMessages.SuspendLayout();
         this.groupBox4.SuspendLayout();
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
         this.txtHost.Location = new System.Drawing.Point(103, 19);
         this.txtHost.Name = "txtHost";
         this.txtHost.Size = new System.Drawing.Size(159, 20);
         this.txtHost.TabIndex = 4;
         this.txtHost.Text = "192.168.101.150";
         // 
         // txtPort
         // 
         this.txtPort.Location = new System.Drawing.Point(103, 45);
         this.txtPort.Name = "txtPort";
         this.txtPort.Size = new System.Drawing.Size(159, 20);
         this.txtPort.TabIndex = 6;
         this.txtPort.Text = "4444";
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
         this.pnlVideo.Size = new System.Drawing.Size(396, 194);
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
         this.lstMessages.Size = new System.Drawing.Size(402, 116);
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
         this.txtUser.Location = new System.Drawing.Point(103, 71);
         this.txtUser.Name = "txtUser";
         this.txtUser.Size = new System.Drawing.Size(159, 20);
         this.txtUser.TabIndex = 11;
         this.txtUser.Text = "Service";
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
         this.txtPassword.Location = new System.Drawing.Point(103, 97);
         this.txtPassword.Name = "txtPassword";
         this.txtPassword.Size = new System.Drawing.Size(159, 20);
         this.txtPassword.TabIndex = 13;
         this.txtPassword.Text = "Default333";
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
         // button1
         // 
         this.button1.Location = new System.Drawing.Point(8, 19);
         this.button1.Name = "button1";
         this.button1.Size = new System.Drawing.Size(50, 28);
         this.button1.TabIndex = 14;
         this.button1.Text = "Cam 1";
         this.button1.UseVisualStyleBackColor = true;
         this.button1.Click += new System.EventHandler(this.button1_Click);
         // 
         // textBox1
         // 
         this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.textBox1.Enabled = false;
         this.textBox1.Location = new System.Drawing.Point(6, 219);
         this.textBox1.Name = "textBox1";
         this.textBox1.Size = new System.Drawing.Size(396, 20);
         this.textBox1.TabIndex = 15;
         // 
         // button2
         // 
         this.button2.Location = new System.Drawing.Point(64, 19);
         this.button2.Name = "button2";
         this.button2.Size = new System.Drawing.Size(50, 28);
         this.button2.TabIndex = 16;
         this.button2.Text = "Cam 2";
         this.button2.UseVisualStyleBackColor = true;
         this.button2.Click += new System.EventHandler(this.button2_Click);
         // 
         // button3
         // 
         this.button3.Location = new System.Drawing.Point(120, 19);
         this.button3.Name = "button3";
         this.button3.Size = new System.Drawing.Size(50, 28);
         this.button3.TabIndex = 17;
         this.button3.Text = "Cam 3";
         this.button3.UseVisualStyleBackColor = true;
         this.button3.Click += new System.EventHandler(this.button3_Click);
         // 
         // button4
         // 
         this.button4.Location = new System.Drawing.Point(64, 58);
         this.button4.Name = "button4";
         this.button4.Size = new System.Drawing.Size(50, 28);
         this.button4.TabIndex = 20;
         this.button4.Text = "Cam 6";
         this.button4.UseVisualStyleBackColor = true;
         this.button4.Click += new System.EventHandler(this.button4_Click);
         // 
         // button5
         // 
         this.button5.Location = new System.Drawing.Point(8, 58);
         this.button5.Name = "button5";
         this.button5.Size = new System.Drawing.Size(50, 28);
         this.button5.TabIndex = 19;
         this.button5.Text = "Cam 5";
         this.button5.UseVisualStyleBackColor = true;
         this.button5.Click += new System.EventHandler(this.button5_Click);
         // 
         // button6
         // 
         this.button6.Location = new System.Drawing.Point(176, 19);
         this.button6.Name = "button6";
         this.button6.Size = new System.Drawing.Size(50, 28);
         this.button6.TabIndex = 18;
         this.button6.Text = "Cam 4";
         this.button6.UseVisualStyleBackColor = true;
         this.button6.Click += new System.EventHandler(this.button6_Click);
         // 
         // button7
         // 
         this.button7.Location = new System.Drawing.Point(8, 97);
         this.button7.Name = "button7";
         this.button7.Size = new System.Drawing.Size(50, 28);
         this.button7.TabIndex = 23;
         this.button7.Text = "Cam 9";
         this.button7.UseVisualStyleBackColor = true;
         this.button7.Click += new System.EventHandler(this.button7_Click);
         // 
         // button8
         // 
         this.button8.Location = new System.Drawing.Point(176, 58);
         this.button8.Name = "button8";
         this.button8.Size = new System.Drawing.Size(50, 28);
         this.button8.TabIndex = 22;
         this.button8.Text = "Cam 8";
         this.button8.UseVisualStyleBackColor = true;
         this.button8.Click += new System.EventHandler(this.button8_Click);
         // 
         // button9
         // 
         this.button9.Location = new System.Drawing.Point(120, 58);
         this.button9.Name = "button9";
         this.button9.Size = new System.Drawing.Size(50, 28);
         this.button9.TabIndex = 21;
         this.button9.Text = "Cam 7";
         this.button9.UseVisualStyleBackColor = true;
         this.button9.Click += new System.EventHandler(this.button9_Click);
         // 
         // grpCameraSelection
         // 
         this.grpCameraSelection.Controls.Add(this.button1);
         this.grpCameraSelection.Controls.Add(this.button2);
         this.grpCameraSelection.Controls.Add(this.button7);
         this.grpCameraSelection.Controls.Add(this.button3);
         this.grpCameraSelection.Controls.Add(this.button8);
         this.grpCameraSelection.Controls.Add(this.button6);
         this.grpCameraSelection.Controls.Add(this.button9);
         this.grpCameraSelection.Controls.Add(this.button5);
         this.grpCameraSelection.Controls.Add(this.button4);
         this.grpCameraSelection.Location = new System.Drawing.Point(7, 199);
         this.grpCameraSelection.Name = "grpCameraSelection";
         this.grpCameraSelection.Size = new System.Drawing.Size(273, 132);
         this.grpCameraSelection.TabIndex = 25;
         this.grpCameraSelection.TabStop = false;
         this.grpCameraSelection.Text = "Camera Selection";
         // 
         // grpConnection
         // 
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
         this.grpConnection.Location = new System.Drawing.Point(7, 4);
         this.grpConnection.Name = "grpConnection";
         this.grpConnection.Size = new System.Drawing.Size(273, 189);
         this.grpConnection.TabIndex = 26;
         this.grpConnection.TabStop = false;
         this.grpConnection.Text = "Connection";
         // 
         // grpMessages
         // 
         this.grpMessages.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.grpMessages.Controls.Add(this.lstMessages);
         this.grpMessages.Location = new System.Drawing.Point(286, 255);
         this.grpMessages.Name = "grpMessages";
         this.grpMessages.Size = new System.Drawing.Size(408, 135);
         this.grpMessages.TabIndex = 27;
         this.grpMessages.TabStop = false;
         this.grpMessages.Text = "Messages";
         // 
         // groupBox4
         // 
         this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.groupBox4.Controls.Add(this.pnlVideo);
         this.groupBox4.Controls.Add(this.textBox1);
         this.groupBox4.Location = new System.Drawing.Point(286, 4);
         this.groupBox4.Name = "groupBox4";
         this.groupBox4.Size = new System.Drawing.Size(408, 245);
         this.groupBox4.TabIndex = 28;
         this.groupBox4.TabStop = false;
         this.groupBox4.Text = "VLC Video Control";
         // 
         // MainWindow
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(706, 402);
         this.Controls.Add(this.groupBox4);
         this.Controls.Add(this.grpMessages);
         this.Controls.Add(this.grpConnection);
         this.Controls.Add(this.grpCameraSelection);
         this.Name = "MainWindow";
         this.Text = "VAO Demo Client";
         this.grpCameraSelection.ResumeLayout(false);
         this.grpConnection.ResumeLayout(false);
         this.grpConnection.PerformLayout();
         this.grpMessages.ResumeLayout(false);
         this.groupBox4.ResumeLayout(false);
         this.groupBox4.PerformLayout();
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
      private System.Windows.Forms.Button button1;
      private System.Windows.Forms.TextBox textBox1;
      private System.Windows.Forms.Button button2;
      private System.Windows.Forms.Button button3;
      private System.Windows.Forms.Button button4;
      private System.Windows.Forms.Button button5;
      private System.Windows.Forms.Button button6;
      private System.Windows.Forms.Button button7;
      private System.Windows.Forms.Button button8;
      private System.Windows.Forms.Button button9;
      private System.Windows.Forms.GroupBox grpCameraSelection;
      private System.Windows.Forms.GroupBox grpConnection;
      private System.Windows.Forms.GroupBox grpMessages;
      private System.Windows.Forms.GroupBox groupBox4;
   }
}

