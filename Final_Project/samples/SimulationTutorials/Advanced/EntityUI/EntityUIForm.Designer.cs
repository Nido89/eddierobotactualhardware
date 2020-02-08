namespace Robotics.EntityUI
{
    partial class EntityUIForm
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
            this._addMotorBaseBtn = new System.Windows.Forms.Button();
            this._entityPositionTxt = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this._webcamCheckBox = new System.Windows.Forms.CheckBox();
            this._lightCheckBox = new System.Windows.Forms.CheckBox();
            this._colorCheckBox = new System.Windows.Forms.CheckBox();
            this._sonarCheckBox = new System.Windows.Forms.CheckBox();
            this._lrfCheckBox = new System.Windows.Forms.CheckBox();
            this._irCheckBox = new System.Windows.Forms.CheckBox();
            this._compassCheckBox = new System.Windows.Forms.CheckBox();
            this._gpsCheckBox = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // _addMotorBaseBtn
            // 
            this._addMotorBaseBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._addMotorBaseBtn.Location = new System.Drawing.Point(145, 160);
            this._addMotorBaseBtn.Name = "_addMotorBaseBtn";
            this._addMotorBaseBtn.Size = new System.Drawing.Size(116, 23);
            this._addMotorBaseBtn.TabIndex = 1;
            this._addMotorBaseBtn.Text = "Add Motor Base";
            this._addMotorBaseBtn.UseVisualStyleBackColor = true;
            this._addMotorBaseBtn.Click += new System.EventHandler(this._addMotorBaseBtn_Click);
            // 
            // _entityPositionTxt
            // 
            this._entityPositionTxt.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._entityPositionTxt.Location = new System.Drawing.Point(145, 129);
            this._entityPositionTxt.Name = "_entityPositionTxt";
            this._entityPositionTxt.Size = new System.Drawing.Size(118, 20);
            this._entityPositionTxt.TabIndex = 2;
            this._entityPositionTxt.Text = "0, 0, 0";
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(2, 132);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(101, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Motor Base Position";
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button1.Location = new System.Drawing.Point(5, 160);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(116, 23);
            this.button1.TabIndex = 4;
            this.button1.Text = "Add Default Scene";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // _webcamCheckBox
            // 
            this._webcamCheckBox.AutoSize = true;
            this._webcamCheckBox.Checked = true;
            this._webcamCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this._webcamCheckBox.Location = new System.Drawing.Point(13, 24);
            this._webcamCheckBox.Name = "_webcamCheckBox";
            this._webcamCheckBox.Size = new System.Drawing.Size(69, 17);
            this._webcamCheckBox.TabIndex = 5;
            this._webcamCheckBox.Text = "Webcam";
            this._webcamCheckBox.UseVisualStyleBackColor = true;
            // 
            // _lightCheckBox
            // 
            this._lightCheckBox.AutoSize = true;
            this._lightCheckBox.Checked = true;
            this._lightCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this._lightCheckBox.Location = new System.Drawing.Point(13, 47);
            this._lightCheckBox.Name = "_lightCheckBox";
            this._lightCheckBox.Size = new System.Drawing.Size(85, 17);
            this._lightCheckBox.TabIndex = 5;
            this._lightCheckBox.Text = "Light Sensor";
            this._lightCheckBox.UseVisualStyleBackColor = true;
            // 
            // _colorCheckBox
            // 
            this._colorCheckBox.AutoSize = true;
            this._colorCheckBox.Checked = true;
            this._colorCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this._colorCheckBox.Location = new System.Drawing.Point(104, 24);
            this._colorCheckBox.Name = "_colorCheckBox";
            this._colorCheckBox.Size = new System.Drawing.Size(86, 17);
            this._colorCheckBox.TabIndex = 5;
            this._colorCheckBox.Text = "Color Sensor";
            this._colorCheckBox.UseVisualStyleBackColor = true;
            // 
            // _sonarCheckBox
            // 
            this._sonarCheckBox.AutoSize = true;
            this._sonarCheckBox.Checked = true;
            this._sonarCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this._sonarCheckBox.Location = new System.Drawing.Point(104, 47);
            this._sonarCheckBox.Name = "_sonarCheckBox";
            this._sonarCheckBox.Size = new System.Drawing.Size(54, 17);
            this._sonarCheckBox.TabIndex = 5;
            this._sonarCheckBox.Text = "Sonar";
            this._sonarCheckBox.UseVisualStyleBackColor = true;
            // 
            // _lrfCheckBox
            // 
            this._lrfCheckBox.AutoSize = true;
            this._lrfCheckBox.Checked = true;
            this._lrfCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this._lrfCheckBox.Location = new System.Drawing.Point(196, 24);
            this._lrfCheckBox.Name = "_lrfCheckBox";
            this._lrfCheckBox.Size = new System.Drawing.Size(46, 17);
            this._lrfCheckBox.TabIndex = 5;
            this._lrfCheckBox.Text = "LRF";
            this._lrfCheckBox.UseVisualStyleBackColor = true;
            // 
            // _irCheckBox
            // 
            this._irCheckBox.AutoSize = true;
            this._irCheckBox.Checked = true;
            this._irCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this._irCheckBox.Location = new System.Drawing.Point(196, 47);
            this._irCheckBox.Name = "_irCheckBox";
            this._irCheckBox.Size = new System.Drawing.Size(62, 17);
            this._irCheckBox.TabIndex = 5;
            this._irCheckBox.Text = "Infrared";
            this._irCheckBox.UseVisualStyleBackColor = true;
            // 
            // _compassCheckBox
            // 
            this._compassCheckBox.AutoSize = true;
            this._compassCheckBox.Checked = true;
            this._compassCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this._compassCheckBox.Location = new System.Drawing.Point(13, 70);
            this._compassCheckBox.Name = "_compassCheckBox";
            this._compassCheckBox.Size = new System.Drawing.Size(69, 17);
            this._compassCheckBox.TabIndex = 5;
            this._compassCheckBox.Text = "Compass";
            this._compassCheckBox.UseVisualStyleBackColor = true;
            // 
            // _gpsCheckBox
            // 
            this._gpsCheckBox.AutoSize = true;
            this._gpsCheckBox.Checked = true;
            this._gpsCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this._gpsCheckBox.Location = new System.Drawing.Point(104, 70);
            this._gpsCheckBox.Name = "_gpsCheckBox";
            this._gpsCheckBox.Size = new System.Drawing.Size(48, 17);
            this._gpsCheckBox.TabIndex = 5;
            this._gpsCheckBox.Text = "GPS";
            this._gpsCheckBox.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this._gpsCheckBox);
            this.groupBox1.Controls.Add(this._webcamCheckBox);
            this.groupBox1.Controls.Add(this._compassCheckBox);
            this.groupBox1.Controls.Add(this._lightCheckBox);
            this.groupBox1.Controls.Add(this._irCheckBox);
            this.groupBox1.Controls.Add(this._colorCheckBox);
            this.groupBox1.Controls.Add(this._lrfCheckBox);
            this.groupBox1.Controls.Add(this._sonarCheckBox);
            this.groupBox1.Location = new System.Drawing.Point(5, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(260, 107);
            this.groupBox1.TabIndex = 6;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Motor Base sensors";
            // 
            // EntityUIForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(268, 195);
            this.ControlBox = false;
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this._entityPositionTxt);
            this.Controls.Add(this._addMotorBaseBtn);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "EntityUIForm";
            this.Text = "EntityUI";
            this.Load += new System.EventHandler(this.EntityUIForm_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button _addMotorBaseBtn;
        private System.Windows.Forms.TextBox _entityPositionTxt;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.CheckBox _webcamCheckBox;
        private System.Windows.Forms.CheckBox _lightCheckBox;
        private System.Windows.Forms.CheckBox _colorCheckBox;
        private System.Windows.Forms.CheckBox _sonarCheckBox;
        private System.Windows.Forms.CheckBox _lrfCheckBox;
        private System.Windows.Forms.CheckBox _irCheckBox;
        private System.Windows.Forms.CheckBox _compassCheckBox;
        private System.Windows.Forms.CheckBox _gpsCheckBox;
        private System.Windows.Forms.GroupBox groupBox1;
    }
}