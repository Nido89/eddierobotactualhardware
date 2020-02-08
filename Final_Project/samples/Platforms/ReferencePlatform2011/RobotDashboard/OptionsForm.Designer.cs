namespace Microsoft.Robotics.Services.RobotDashboard
{
    /// <summary>
    /// Form to display/update option settings
    /// </summary>
    partial class OptionsForm
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
            this.label1 = new System.Windows.Forms.Label();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.grpTrackball = new System.Windows.Forms.GroupBox();
            this.txtRotateScaleFactor = new System.Windows.Forms.TextBox();
            this.txtTranslateScaleFactor = new System.Windows.Forms.TextBox();
            this.txtDeadZoneY = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtDeadZoneX = new System.Windows.Forms.TextBox();
            this.grpCamera = new System.Windows.Forms.GroupBox();
            this.txtCameraInterval = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.grpTrackball.SuspendLayout();
            this.grpCamera.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(250, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Set the Option values. Remember to Save Settings.";
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(20, 271);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(64, 26);
            this.btnOK.TabIndex = 16;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.OKButtonClick);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(186, 271);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(64, 26);
            this.btnCancel.TabIndex = 17;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.CancelButtonClick);
            // 
            // grpTrackball
            // 
            this.grpTrackball.Controls.Add(this.txtRotateScaleFactor);
            this.grpTrackball.Controls.Add(this.txtTranslateScaleFactor);
            this.grpTrackball.Controls.Add(this.txtDeadZoneY);
            this.grpTrackball.Controls.Add(this.label5);
            this.grpTrackball.Controls.Add(this.label4);
            this.grpTrackball.Controls.Add(this.label3);
            this.grpTrackball.Controls.Add(this.label2);
            this.grpTrackball.Controls.Add(this.txtDeadZoneX);
            this.grpTrackball.Location = new System.Drawing.Point(20, 31);
            this.grpTrackball.Name = "grpTrackball";
            this.grpTrackball.Size = new System.Drawing.Size(230, 140);
            this.grpTrackball.TabIndex = 18;
            this.grpTrackball.TabStop = false;
            this.grpTrackball.Text = "Trackball";
            // 
            // txtRotateScaleFactor
            // 
            this.txtRotateScaleFactor.Location = new System.Drawing.Point(145, 110);
            this.txtRotateScaleFactor.Name = "txtRotateScaleFactor";
            this.txtRotateScaleFactor.Size = new System.Drawing.Size(66, 20);
            this.txtRotateScaleFactor.TabIndex = 16;
            this.txtRotateScaleFactor.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtTranslateScaleFactor
            // 
            this.txtTranslateScaleFactor.Location = new System.Drawing.Point(145, 81);
            this.txtTranslateScaleFactor.Name = "txtTranslateScaleFactor";
            this.txtTranslateScaleFactor.Size = new System.Drawing.Size(66, 20);
            this.txtTranslateScaleFactor.TabIndex = 14;
            this.txtTranslateScaleFactor.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtDeadZoneY
            // 
            this.txtDeadZoneY.Location = new System.Drawing.Point(145, 52);
            this.txtDeadZoneY.Name = "txtDeadZoneY";
            this.txtDeadZoneY.Size = new System.Drawing.Size(66, 20);
            this.txtDeadZoneY.TabIndex = 12;
            this.txtDeadZoneY.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(14, 114);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(105, 13);
            this.label5.TabIndex = 15;
            this.label5.Text = "Rotate Scale Factor:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(14, 85);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(117, 13);
            this.label4.TabIndex = 13;
            this.label4.Text = "Translate Scale Factor:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(14, 56);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(74, 13);
            this.label3.TabIndex = 11;
            this.label3.Text = "Dead Zone Y:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(14, 27);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(74, 13);
            this.label2.TabIndex = 9;
            this.label2.Text = "Dead Zone X:";
            // 
            // txtDeadZoneX
            // 
            this.txtDeadZoneX.Location = new System.Drawing.Point(145, 23);
            this.txtDeadZoneX.Name = "txtDeadZoneX";
            this.txtDeadZoneX.Size = new System.Drawing.Size(66, 20);
            this.txtDeadZoneX.TabIndex = 10;
            this.txtDeadZoneX.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // grpCamera
            // 
            this.grpCamera.Controls.Add(this.txtCameraInterval);
            this.grpCamera.Controls.Add(this.label11);
            this.grpCamera.Location = new System.Drawing.Point(21, 177);
            this.grpCamera.Name = "grpCamera";
            this.grpCamera.Size = new System.Drawing.Size(229, 61);
            this.grpCamera.TabIndex = 22;
            this.grpCamera.TabStop = false;
            this.grpCamera.Text = "WebCam Viewer";
            // 
            // txtCameraInterval
            // 
            this.txtCameraInterval.Location = new System.Drawing.Point(152, 23);
            this.txtCameraInterval.Name = "txtCameraInterval";
            this.txtCameraInterval.Size = new System.Drawing.Size(63, 20);
            this.txtCameraInterval.TabIndex = 1;
            this.txtCameraInterval.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(14, 26);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(105, 13);
            this.label11.TabIndex = 0;
            this.label11.Text = "Update Interval (ms):";
            // 
            // OptionsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(282, 322);
            this.Controls.Add(this.grpCamera);
            this.Controls.Add(this.grpTrackball);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.label1);
            this.Name = "OptionsForm";
            this.Text = "Options";
            this.Load += new System.EventHandler(this.OptionsFormLoad);
            this.grpTrackball.ResumeLayout(false);
            this.grpTrackball.PerformLayout();
            this.grpCamera.ResumeLayout(false);
            this.grpCamera.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.GroupBox grpTrackball;
        private System.Windows.Forms.TextBox txtRotateScaleFactor;
        private System.Windows.Forms.TextBox txtTranslateScaleFactor;
        private System.Windows.Forms.TextBox txtDeadZoneY;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtDeadZoneX;
        private System.Windows.Forms.GroupBox grpCamera;
        private System.Windows.Forms.TextBox txtCameraInterval;
        private System.Windows.Forms.Label label11;
    }
}