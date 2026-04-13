using DarkUI.Controls;
using System;
using System.Xml;
using Vao.Client.Components;

namespace Vao.Sample
{
   partial class CameraLockWindow
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
         this.grpCameraLock = new DarkUI.Controls.DarkSectionPanel();
         this.lblSec = new System.Windows.Forms.Label();
         this.lblMin = new System.Windows.Forms.Label();
         this.lblHour = new System.Windows.Forms.Label();
         this.numUpDownSec = new DarkUI.Controls.DarkNumericUpDown();
         this.numUpDownMin = new DarkUI.Controls.DarkNumericUpDown();
         this.numUpDownHour = new DarkUI.Controls.DarkNumericUpDown();
         this.btnCancel = new DarkUI.Controls.DarkButton();
         this.btnSendAbsolutePosition = new DarkUI.Controls.DarkButton();
         this.lblTimeout = new System.Windows.Forms.Label();
         this.grpCameraLock.SuspendLayout();
         ((System.ComponentModel.ISupportInitialize)(this.numUpDownSec)).BeginInit();
         ((System.ComponentModel.ISupportInitialize)(this.numUpDownMin)).BeginInit();
         ((System.ComponentModel.ISupportInitialize)(this.numUpDownHour)).BeginInit();
         this.SuspendLayout();
         // 
         // grpCameraLock
         // 
         this.grpCameraLock.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.grpCameraLock.Controls.Add(this.lblSec);
         this.grpCameraLock.Controls.Add(this.lblMin);
         this.grpCameraLock.Controls.Add(this.lblHour);
         this.grpCameraLock.Controls.Add(this.numUpDownSec);
         this.grpCameraLock.Controls.Add(this.numUpDownMin);
         this.grpCameraLock.Controls.Add(this.numUpDownHour);
         this.grpCameraLock.Controls.Add(this.btnCancel);
         this.grpCameraLock.Controls.Add(this.btnSendAbsolutePosition);
         this.grpCameraLock.Controls.Add(this.lblTimeout);
         this.grpCameraLock.Location = new System.Drawing.Point(0, -1);
         this.grpCameraLock.Name = "grpCameraLock";
         this.grpCameraLock.SectionHeader = "Camera lock";
         this.grpCameraLock.Size = new System.Drawing.Size(203, 150);
         this.grpCameraLock.TabIndex = 8;
         // 
         // lblSec
         // 
         this.lblSec.AutoSize = true;
         this.lblSec.ForeColor = System.Drawing.Color.Silver;
         this.lblSec.Location = new System.Drawing.Point(150, 36);
         this.lblSec.Name = "lblSec";
         this.lblSec.Size = new System.Drawing.Size(26, 13);
         this.lblSec.TabIndex = 21;
         this.lblSec.Text = "Sec";
         // 
         // lblMin
         // 
         this.lblMin.AutoSize = true;
         this.lblMin.ForeColor = System.Drawing.Color.Silver;
         this.lblMin.Location = new System.Drawing.Point(105, 36);
         this.lblMin.Name = "lblMin";
         this.lblMin.Size = new System.Drawing.Size(24, 13);
         this.lblMin.TabIndex = 20;
         this.lblMin.Text = "Min";
         // 
         // lblHour
         // 
         this.lblHour.AutoSize = true;
         this.lblHour.ForeColor = System.Drawing.Color.Silver;
         this.lblHour.Location = new System.Drawing.Point(59, 37);
         this.lblHour.Name = "lblHour";
         this.lblHour.Size = new System.Drawing.Size(30, 13);
         this.lblHour.TabIndex = 19;
         this.lblHour.Text = "Hour";
         // 
         // numUpDownSec
         // 
         this.numUpDownSec.Location = new System.Drawing.Point(150, 54);
         this.numUpDownSec.Maximum = new decimal(new int[] {
            59,
            0,
            0,
            0});
         this.numUpDownSec.Name = "numUpDownSec";
         this.numUpDownSec.Size = new System.Drawing.Size(35, 20);
         this.numUpDownSec.TabIndex = 18;
         this.numUpDownSec.ValueChanged += new System.EventHandler(this.numUpDownSec_ValueChanged);
         // 
         // numUpDownMin
         // 
         this.numUpDownMin.Location = new System.Drawing.Point(105, 54);
         this.numUpDownMin.Maximum = new decimal(new int[] {
            59,
            0,
            0,
            0});
         this.numUpDownMin.Name = "numUpDownMin";
         this.numUpDownMin.Size = new System.Drawing.Size(35, 20);
         this.numUpDownMin.TabIndex = 17;
         this.numUpDownMin.ValueChanged += new System.EventHandler(this.numUpDownMin_ValueChanged);
         // 
         // numUpDownHour
         // 
         this.numUpDownHour.Location = new System.Drawing.Point(60, 54);
         this.numUpDownHour.Maximum = new decimal(new int[] {
            24,
            0,
            0,
            0});
         this.numUpDownHour.Name = "numUpDownHour";
         this.numUpDownHour.Size = new System.Drawing.Size(35, 20);
         this.numUpDownHour.TabIndex = 16;
         this.numUpDownHour.ValueChanged += new System.EventHandler(this.numUpDownHour_ValueChanged);
         // 
         // btnCancel
         // 
         this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
         this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
         this.btnCancel.Location = new System.Drawing.Point(41, 102);
         this.btnCancel.Name = "btnCancel";
         this.btnCancel.Padding = new System.Windows.Forms.Padding(5);
         this.btnCancel.Size = new System.Drawing.Size(54, 29);
         this.btnCancel.TabIndex = 15;
         this.btnCancel.Text = "Cancel";
         this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
         // 
         // btnSendAbsolutePosition
         // 
         this.btnSendAbsolutePosition.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
         this.btnSendAbsolutePosition.DialogResult = System.Windows.Forms.DialogResult.Cancel;
         this.btnSendAbsolutePosition.Location = new System.Drawing.Point(111, 102);
         this.btnSendAbsolutePosition.Name = "btnSendAbsolutePosition";
         this.btnSendAbsolutePosition.Padding = new System.Windows.Forms.Padding(5);
         this.btnSendAbsolutePosition.Size = new System.Drawing.Size(54, 29);
         this.btnSendAbsolutePosition.TabIndex = 14;
         this.btnSendAbsolutePosition.Text = "Send";
         this.btnSendAbsolutePosition.Click += new System.EventHandler(this.btnSendAbsolutePosition_Click);
         // 
         // lblTimeout
         // 
         this.lblTimeout.AutoSize = true;
         this.lblTimeout.ForeColor = System.Drawing.Color.Silver;
         this.lblTimeout.Location = new System.Drawing.Point(9, 57);
         this.lblTimeout.Name = "lblTimeout";
         this.lblTimeout.Size = new System.Drawing.Size(45, 13);
         this.lblTimeout.TabIndex = 4;
         this.lblTimeout.Text = "Timeout";
         // 
         // CameraLockWindow
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
         this.ClientSize = new System.Drawing.Size(203, 150);
         this.Controls.Add(this.grpCameraLock);
         this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
         this.MinimumSize = new System.Drawing.Size(100, 100);
         this.Name = "CameraLockWindow";
         this.Text = "Camera absolute position";
         this.grpCameraLock.ResumeLayout(false);
         this.grpCameraLock.PerformLayout();
         ((System.ComponentModel.ISupportInitialize)(this.numUpDownSec)).EndInit();
         ((System.ComponentModel.ISupportInitialize)(this.numUpDownMin)).EndInit();
         ((System.ComponentModel.ISupportInitialize)(this.numUpDownHour)).EndInit();
         this.ResumeLayout(false);

      }





      #endregion

      private DarkSectionPanel grpCameraLock;
      private DarkButton btnSendAbsolutePosition;
      private System.Windows.Forms.Label lblTimeout;
      private DarkButton btnCancel;
      private System.Windows.Forms.Label lblSec;
      private System.Windows.Forms.Label lblMin;
      private System.Windows.Forms.Label lblHour;
      private DarkNumericUpDown numUpDownSec;
      private DarkNumericUpDown numUpDownMin;
      private DarkNumericUpDown numUpDownHour;
   }
}

