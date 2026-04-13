using DarkUI.Controls;
using System;
using System.Xml;
using Vao.Client.Components;

namespace Vao.Sample
{
   partial class AlarmActionWindow
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
         this.grpAlarmAction = new DarkUI.Controls.DarkSectionPanel();
         this.btnAlarmExpediate = new DarkUI.Controls.DarkButton();
         this.btnAlarmDisable = new DarkUI.Controls.DarkButton();
         this.btnAlarmEnable = new DarkUI.Controls.DarkButton();
         this.btnAlarmAcknowledge = new DarkUI.Controls.DarkButton();
         this.btnAlarmActivate = new DarkUI.Controls.DarkButton();
         this.btnCancel = new DarkUI.Controls.DarkButton();
         this.darkSectionPanel1 = new DarkUI.Controls.DarkSectionPanel();
         this.txtAlarmPriority = new DarkUI.Controls.DarkTextBox();
         this.lblAlarmPriority = new System.Windows.Forms.Label();
         this.txtAlarmExtendedStatus = new DarkUI.Controls.DarkTextBox();
         this.lblAarmExtendedStatus = new System.Windows.Forms.Label();
         this.txtAlarmStatus = new DarkUI.Controls.DarkTextBox();
         this.lblAlarmStatus = new System.Windows.Forms.Label();
         this.txtAlarmName = new DarkUI.Controls.DarkTextBox();
         this.lblAlarmName = new System.Windows.Forms.Label();
         this.grpAlarmAction.SuspendLayout();
         this.darkSectionPanel1.SuspendLayout();
         this.SuspendLayout();
         // 
         // grpAlarmAction
         // 
         this.grpAlarmAction.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.grpAlarmAction.Controls.Add(this.btnAlarmExpediate);
         this.grpAlarmAction.Controls.Add(this.btnAlarmDisable);
         this.grpAlarmAction.Controls.Add(this.btnAlarmEnable);
         this.grpAlarmAction.Controls.Add(this.btnAlarmAcknowledge);
         this.grpAlarmAction.Controls.Add(this.btnAlarmActivate);
         this.grpAlarmAction.Controls.Add(this.btnCancel);
         this.grpAlarmAction.Location = new System.Drawing.Point(0, -1);
         this.grpAlarmAction.Name = "grpAlarmAction";
         this.grpAlarmAction.SectionHeader = "Alarm action";
         this.grpAlarmAction.Size = new System.Drawing.Size(202, 247);
         this.grpAlarmAction.TabIndex = 8;
         // 
         // btnAlarmExpediate
         // 
         this.btnAlarmExpediate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
         this.btnAlarmExpediate.DialogResult = System.Windows.Forms.DialogResult.Cancel;
         this.btnAlarmExpediate.Location = new System.Drawing.Point(17, 165);
         this.btnAlarmExpediate.Name = "btnAlarmExpediate";
         this.btnAlarmExpediate.Padding = new System.Windows.Forms.Padding(5);
         this.btnAlarmExpediate.Size = new System.Drawing.Size(171, 24);
         this.btnAlarmExpediate.TabIndex = 20;
         this.btnAlarmExpediate.Text = "Expediate";
         this.btnAlarmExpediate.Click += new System.EventHandler(this.btnAlarmExpediate_Click);
         // 
         // btnAlarmDisable
         // 
         this.btnAlarmDisable.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
         this.btnAlarmDisable.DialogResult = System.Windows.Forms.DialogResult.Cancel;
         this.btnAlarmDisable.Location = new System.Drawing.Point(17, 135);
         this.btnAlarmDisable.Name = "btnAlarmDisable";
         this.btnAlarmDisable.Padding = new System.Windows.Forms.Padding(5);
         this.btnAlarmDisable.Size = new System.Drawing.Size(171, 24);
         this.btnAlarmDisable.TabIndex = 19;
         this.btnAlarmDisable.Text = "Disable";
         this.btnAlarmDisable.Click += new System.EventHandler(this.btnAlarmDisable_Click);
         // 
         // btnAlarmEnable
         // 
         this.btnAlarmEnable.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
         this.btnAlarmEnable.DialogResult = System.Windows.Forms.DialogResult.Cancel;
         this.btnAlarmEnable.Location = new System.Drawing.Point(17, 105);
         this.btnAlarmEnable.Name = "btnAlarmEnable";
         this.btnAlarmEnable.Padding = new System.Windows.Forms.Padding(5);
         this.btnAlarmEnable.Size = new System.Drawing.Size(171, 24);
         this.btnAlarmEnable.TabIndex = 18;
         this.btnAlarmEnable.Text = "Enable";
         this.btnAlarmEnable.Click += new System.EventHandler(this.btnAlarmEnable_Click);
         // 
         // btnAlarmAcknowledge
         // 
         this.btnAlarmAcknowledge.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
         this.btnAlarmAcknowledge.DialogResult = System.Windows.Forms.DialogResult.Cancel;
         this.btnAlarmAcknowledge.Location = new System.Drawing.Point(17, 75);
         this.btnAlarmAcknowledge.Name = "btnAlarmAcknowledge";
         this.btnAlarmAcknowledge.Padding = new System.Windows.Forms.Padding(5);
         this.btnAlarmAcknowledge.Size = new System.Drawing.Size(171, 24);
         this.btnAlarmAcknowledge.TabIndex = 17;
         this.btnAlarmAcknowledge.Text = "Acknowledge";
         this.btnAlarmAcknowledge.Click += new System.EventHandler(this.btnAlarmAcknowledge_Click);
         // 
         // btnAlarmActivate
         // 
         this.btnAlarmActivate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
         this.btnAlarmActivate.DialogResult = System.Windows.Forms.DialogResult.Cancel;
         this.btnAlarmActivate.Location = new System.Drawing.Point(17, 45);
         this.btnAlarmActivate.Name = "btnAlarmActivate";
         this.btnAlarmActivate.Padding = new System.Windows.Forms.Padding(5);
         this.btnAlarmActivate.Size = new System.Drawing.Size(171, 24);
         this.btnAlarmActivate.TabIndex = 16;
         this.btnAlarmActivate.Text = "Activate";
         this.btnAlarmActivate.Click += new System.EventHandler(this.btnAlarmActivate_Click);
         // 
         // btnCancel
         // 
         this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
         this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
         this.btnCancel.Location = new System.Drawing.Point(73, 204);
         this.btnCancel.Name = "btnCancel";
         this.btnCancel.Padding = new System.Windows.Forms.Padding(5);
         this.btnCancel.Size = new System.Drawing.Size(54, 29);
         this.btnCancel.TabIndex = 15;
         this.btnCancel.Text = "Cancel";
         this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
         // 
         // darkSectionPanel1
         // 
         this.darkSectionPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.darkSectionPanel1.Controls.Add(this.txtAlarmPriority);
         this.darkSectionPanel1.Controls.Add(this.lblAlarmPriority);
         this.darkSectionPanel1.Controls.Add(this.txtAlarmExtendedStatus);
         this.darkSectionPanel1.Controls.Add(this.lblAarmExtendedStatus);
         this.darkSectionPanel1.Controls.Add(this.txtAlarmStatus);
         this.darkSectionPanel1.Controls.Add(this.lblAlarmStatus);
         this.darkSectionPanel1.Controls.Add(this.txtAlarmName);
         this.darkSectionPanel1.Controls.Add(this.lblAlarmName);
         this.darkSectionPanel1.Location = new System.Drawing.Point(202, -1);
         this.darkSectionPanel1.Name = "darkSectionPanel1";
         this.darkSectionPanel1.SectionHeader = "Alarm information";
         this.darkSectionPanel1.Size = new System.Drawing.Size(228, 247);
         this.darkSectionPanel1.TabIndex = 11;
         // 
         // txtAlarmPriority
         // 
         this.txtAlarmPriority.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.txtAlarmPriority.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
         this.txtAlarmPriority.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
         this.txtAlarmPriority.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
         this.txtAlarmPriority.Location = new System.Drawing.Point(14, 176);
         this.txtAlarmPriority.Name = "txtAlarmPriority";
         this.txtAlarmPriority.ReadOnly = true;
         this.txtAlarmPriority.Size = new System.Drawing.Size(24, 20);
         this.txtAlarmPriority.TabIndex = 13;
         this.txtAlarmPriority.TabStop = false;
         // 
         // lblAlarmPriority
         // 
         this.lblAlarmPriority.AutoSize = true;
         this.lblAlarmPriority.ForeColor = System.Drawing.Color.Silver;
         this.lblAlarmPriority.Location = new System.Drawing.Point(14, 159);
         this.lblAlarmPriority.Name = "lblAlarmPriority";
         this.lblAlarmPriority.Size = new System.Drawing.Size(38, 13);
         this.lblAlarmPriority.TabIndex = 12;
         this.lblAlarmPriority.Text = "Priority";
         // 
         // txtAlarmExtendedStatus
         // 
         this.txtAlarmExtendedStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.txtAlarmExtendedStatus.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
         this.txtAlarmExtendedStatus.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
         this.txtAlarmExtendedStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
         this.txtAlarmExtendedStatus.Location = new System.Drawing.Point(14, 133);
         this.txtAlarmExtendedStatus.Name = "txtAlarmExtendedStatus";
         this.txtAlarmExtendedStatus.ReadOnly = true;
         this.txtAlarmExtendedStatus.Size = new System.Drawing.Size(187, 20);
         this.txtAlarmExtendedStatus.TabIndex = 11;
         this.txtAlarmExtendedStatus.TabStop = false;
         // 
         // lblAarmExtendedStatus
         // 
         this.lblAarmExtendedStatus.AutoSize = true;
         this.lblAarmExtendedStatus.ForeColor = System.Drawing.Color.Silver;
         this.lblAarmExtendedStatus.Location = new System.Drawing.Point(14, 116);
         this.lblAarmExtendedStatus.Name = "lblAarmExtendedStatus";
         this.lblAarmExtendedStatus.Size = new System.Drawing.Size(83, 13);
         this.lblAarmExtendedStatus.TabIndex = 10;
         this.lblAarmExtendedStatus.Text = "Extended status";
         // 
         // txtAlarmStatus
         // 
         this.txtAlarmStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.txtAlarmStatus.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
         this.txtAlarmStatus.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
         this.txtAlarmStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
         this.txtAlarmStatus.Location = new System.Drawing.Point(14, 90);
         this.txtAlarmStatus.Name = "txtAlarmStatus";
         this.txtAlarmStatus.ReadOnly = true;
         this.txtAlarmStatus.Size = new System.Drawing.Size(103, 20);
         this.txtAlarmStatus.TabIndex = 9;
         this.txtAlarmStatus.TabStop = false;
         // 
         // lblAlarmStatus
         // 
         this.lblAlarmStatus.AutoSize = true;
         this.lblAlarmStatus.ForeColor = System.Drawing.Color.Silver;
         this.lblAlarmStatus.Location = new System.Drawing.Point(14, 73);
         this.lblAlarmStatus.Name = "lblAlarmStatus";
         this.lblAlarmStatus.Size = new System.Drawing.Size(37, 13);
         this.lblAlarmStatus.TabIndex = 8;
         this.lblAlarmStatus.Text = "Status";
         // 
         // txtAlarmName
         // 
         this.txtAlarmName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.txtAlarmName.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
         this.txtAlarmName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
         this.txtAlarmName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
         this.txtAlarmName.HideSelection = false;
         this.txtAlarmName.Location = new System.Drawing.Point(14, 47);
         this.txtAlarmName.Name = "txtAlarmName";
         this.txtAlarmName.ReadOnly = true;
         this.txtAlarmName.Size = new System.Drawing.Size(103, 20);
         this.txtAlarmName.TabIndex = 7;
         this.txtAlarmName.TabStop = false;
         // 
         // lblAlarmName
         // 
         this.lblAlarmName.AutoSize = true;
         this.lblAlarmName.ForeColor = System.Drawing.Color.Silver;
         this.lblAlarmName.Location = new System.Drawing.Point(14, 30);
         this.lblAlarmName.Name = "lblAlarmName";
         this.lblAlarmName.Size = new System.Drawing.Size(35, 13);
         this.lblAlarmName.TabIndex = 6;
         this.lblAlarmName.Text = "Name";
         // 
         // AlarmActionWindow
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
         this.ClientSize = new System.Drawing.Size(426, 246);
         this.Controls.Add(this.darkSectionPanel1);
         this.Controls.Add(this.grpAlarmAction);
         this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
         this.MinimumSize = new System.Drawing.Size(100, 100);
         this.Name = "AlarmActionWindow";
         this.Text = "Camera absolute position";
         this.grpAlarmAction.ResumeLayout(false);
         this.darkSectionPanel1.ResumeLayout(false);
         this.darkSectionPanel1.PerformLayout();
         this.ResumeLayout(false);

      }





      #endregion

      private DarkSectionPanel grpAlarmAction;
      private DarkButton btnCancel;
      private DarkSectionPanel darkSectionPanel1;
      private DarkTextBox txtAlarmPriority;
      private System.Windows.Forms.Label lblAlarmPriority;
      private DarkTextBox txtAlarmExtendedStatus;
      private System.Windows.Forms.Label lblAarmExtendedStatus;
      private DarkTextBox txtAlarmStatus;
      private System.Windows.Forms.Label lblAlarmStatus;
      private DarkTextBox txtAlarmName;
      private System.Windows.Forms.Label lblAlarmName;
      private DarkButton btnAlarmExpediate;
      private DarkButton btnAlarmDisable;
      private DarkButton btnAlarmEnable;
      private DarkButton btnAlarmAcknowledge;
      private DarkButton btnAlarmActivate;
   }
}

