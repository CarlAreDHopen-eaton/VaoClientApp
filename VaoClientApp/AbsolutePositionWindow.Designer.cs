using DarkUI.Controls;
using System;
using System.Xml;
using Vao.Client.Components;

namespace Vao.Sample
{
   partial class AbsolutePositionWindow
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
         this.grpFTPConnection = new DarkUI.Controls.DarkSectionPanel();
         this.btnCancel = new DarkUI.Controls.DarkButton();
         this.btnSendAbsolutePosition = new DarkUI.Controls.DarkButton();
         this.txtAbsoluteZoom = new DarkUI.Controls.DarkTextBox();
         this.lblZoom = new System.Windows.Forms.Label();
         this.txtAbsoluteTilt = new DarkUI.Controls.DarkTextBox();
         this.lblTilt = new System.Windows.Forms.Label();
         this.lblPan = new System.Windows.Forms.Label();
         this.txtAbsolutePan = new DarkUI.Controls.DarkTextBox();
         this.grpFTPConnection.SuspendLayout();
         this.SuspendLayout();
         // 
         // grpFTPConnection
         // 
         this.grpFTPConnection.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.grpFTPConnection.Controls.Add(this.btnCancel);
         this.grpFTPConnection.Controls.Add(this.btnSendAbsolutePosition);
         this.grpFTPConnection.Controls.Add(this.txtAbsoluteZoom);
         this.grpFTPConnection.Controls.Add(this.lblZoom);
         this.grpFTPConnection.Controls.Add(this.txtAbsoluteTilt);
         this.grpFTPConnection.Controls.Add(this.lblTilt);
         this.grpFTPConnection.Controls.Add(this.lblPan);
         this.grpFTPConnection.Controls.Add(this.txtAbsolutePan);
         this.grpFTPConnection.Location = new System.Drawing.Point(0, -1);
         this.grpFTPConnection.Name = "grpFTPConnection";
         this.grpFTPConnection.SectionHeader = "Absolute position";
         this.grpFTPConnection.Size = new System.Drawing.Size(156, 172);
         this.grpFTPConnection.TabIndex = 8;
         // 
         // btnCancel
         // 
         this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
         this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
         this.btnCancel.Location = new System.Drawing.Point(12, 119);
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
         this.btnSendAbsolutePosition.Enabled = false;
         this.btnSendAbsolutePosition.Location = new System.Drawing.Point(82, 119);
         this.btnSendAbsolutePosition.Name = "btnSendAbsolutePosition";
         this.btnSendAbsolutePosition.Padding = new System.Windows.Forms.Padding(5);
         this.btnSendAbsolutePosition.Size = new System.Drawing.Size(54, 29);
         this.btnSendAbsolutePosition.TabIndex = 14;
         this.btnSendAbsolutePosition.Text = "Send";
         this.btnSendAbsolutePosition.Click += new System.EventHandler(this.btnSendAbsolutePosition_Click);
         // 
         // txtAbsoluteZoom
         // 
         this.txtAbsoluteZoom.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.txtAbsoluteZoom.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
         this.txtAbsoluteZoom.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
         this.txtAbsoluteZoom.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
         this.txtAbsoluteZoom.Location = new System.Drawing.Point(65, 90);
         this.txtAbsoluteZoom.Name = "txtAbsoluteZoom";
         this.txtAbsoluteZoom.Size = new System.Drawing.Size(71, 20);
         this.txtAbsoluteZoom.TabIndex = 9;
         this.txtAbsoluteZoom.TextChanged += new System.EventHandler(this.txtAbsoluteZoom_TextChanged);
         this.txtAbsoluteZoom.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtAbsoluteZoom_KeyPress);
         // 
         // lblZoom
         // 
         this.lblZoom.AutoSize = true;
         this.lblZoom.ForeColor = System.Drawing.Color.Silver;
         this.lblZoom.Location = new System.Drawing.Point(18, 92);
         this.lblZoom.Name = "lblZoom";
         this.lblZoom.Size = new System.Drawing.Size(34, 13);
         this.lblZoom.TabIndex = 8;
         this.lblZoom.Text = "Zoom";
         // 
         // txtAbsoluteTilt
         // 
         this.txtAbsoluteTilt.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.txtAbsoluteTilt.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
         this.txtAbsoluteTilt.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
         this.txtAbsoluteTilt.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
         this.txtAbsoluteTilt.Location = new System.Drawing.Point(65, 64);
         this.txtAbsoluteTilt.Name = "txtAbsoluteTilt";
         this.txtAbsoluteTilt.Size = new System.Drawing.Size(71, 20);
         this.txtAbsoluteTilt.TabIndex = 7;
         this.txtAbsoluteTilt.TextChanged += new System.EventHandler(this.txtAbsoluteTilt_TextChanged);
         this.txtAbsoluteTilt.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtAbsoluteTilt_KeyPress);
         // 
         // lblTilt
         // 
         this.lblTilt.AutoSize = true;
         this.lblTilt.ForeColor = System.Drawing.Color.Silver;
         this.lblTilt.Location = new System.Drawing.Point(18, 66);
         this.lblTilt.Name = "lblTilt";
         this.lblTilt.Size = new System.Drawing.Size(21, 13);
         this.lblTilt.TabIndex = 6;
         this.lblTilt.Text = "Tilt";
         // 
         // lblPan
         // 
         this.lblPan.AutoSize = true;
         this.lblPan.ForeColor = System.Drawing.Color.Silver;
         this.lblPan.Location = new System.Drawing.Point(18, 41);
         this.lblPan.Name = "lblPan";
         this.lblPan.Size = new System.Drawing.Size(26, 13);
         this.lblPan.TabIndex = 4;
         this.lblPan.Text = "Pan";
         // 
         // txtAbsolutePan
         // 
         this.txtAbsolutePan.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.txtAbsolutePan.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
         this.txtAbsolutePan.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
         this.txtAbsolutePan.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
         this.txtAbsolutePan.Location = new System.Drawing.Point(65, 38);
         this.txtAbsolutePan.Name = "txtAbsolutePan";
         this.txtAbsolutePan.Size = new System.Drawing.Size(71, 20);
         this.txtAbsolutePan.TabIndex = 5;
         this.txtAbsolutePan.TextChanged += new System.EventHandler(this.txtAbsolutePan_TextChanged);
         this.txtAbsolutePan.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtAbsolutePan_KeyPress);
         // 
         // AbsolutePositionWindow
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
         this.ClientSize = new System.Drawing.Size(156, 170);
         this.Controls.Add(this.grpFTPConnection);
         this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
         this.MinimumSize = new System.Drawing.Size(100, 100);
         this.Name = "AbsolutePositionWindow";
         this.Text = "Camera absolute position";
         this.grpFTPConnection.ResumeLayout(false);
         this.grpFTPConnection.PerformLayout();
         this.ResumeLayout(false);

      }





      #endregion

      private DarkSectionPanel grpFTPConnection;
      private DarkButton btnSendAbsolutePosition;
      private DarkTextBox txtAbsoluteZoom;
      private System.Windows.Forms.Label lblZoom;
      private DarkTextBox txtAbsoluteTilt;
      private System.Windows.Forms.Label lblTilt;
      private System.Windows.Forms.Label lblPan;
      private DarkTextBox txtAbsolutePan;
      private DarkButton btnCancel;
   }
}

