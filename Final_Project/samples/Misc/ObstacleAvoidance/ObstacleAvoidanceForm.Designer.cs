namespace Microsoft.Robotics.Services.ObstacleAvoidanceDrive
{
    partial class ObstacleAvoidanceForm
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
            this.depthProfileCtrl = new System.Windows.Forms.PictureBox();
            this.PIDControllerGroupBox = new System.Windows.Forms.GroupBox();
            this.KdIndicator = new System.Windows.Forms.Panel();
            this.KiIndicator = new System.Windows.Forms.Panel();
            this.KpIndicator = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.incrementNumeric = new System.Windows.Forms.NumericUpDown();
            this.KdGroupBox = new System.Windows.Forms.GroupBox();
            this.KdNumeric = new System.Windows.Forms.NumericUpDown();
            this.KiGroupBox = new System.Windows.Forms.GroupBox();
            this.KiNumeric = new System.Windows.Forms.NumericUpDown();
            this.KpGroupBox = new System.Windows.Forms.GroupBox();
            this.KpNumeric = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.depthProfileCtrl)).BeginInit();
            this.PIDControllerGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.incrementNumeric)).BeginInit();
            this.KdGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.KdNumeric)).BeginInit();
            this.KiGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.KiNumeric)).BeginInit();
            this.KpGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.KpNumeric)).BeginInit();
            this.SuspendLayout();
            // 
            // depthProfileCtrl
            // 
            this.depthProfileCtrl.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.depthProfileCtrl.Location = new System.Drawing.Point(13, 13);
            this.depthProfileCtrl.Name = "depthProfileCtrl";
            this.depthProfileCtrl.Size = new System.Drawing.Size(320, 330);
            this.depthProfileCtrl.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.depthProfileCtrl.TabIndex = 1;
            this.depthProfileCtrl.TabStop = false;
            // 
            // PIDControllerGroupBox
            // 
            this.PIDControllerGroupBox.Controls.Add(this.KdIndicator);
            this.PIDControllerGroupBox.Controls.Add(this.KiIndicator);
            this.PIDControllerGroupBox.Controls.Add(this.KpIndicator);
            this.PIDControllerGroupBox.Controls.Add(this.label1);
            this.PIDControllerGroupBox.Controls.Add(this.incrementNumeric);
            this.PIDControllerGroupBox.Controls.Add(this.KdGroupBox);
            this.PIDControllerGroupBox.Controls.Add(this.KiGroupBox);
            this.PIDControllerGroupBox.Controls.Add(this.KpGroupBox);
            this.PIDControllerGroupBox.Location = new System.Drawing.Point(13, 348);
            this.PIDControllerGroupBox.Name = "PIDControllerGroupBox";
            this.PIDControllerGroupBox.Size = new System.Drawing.Size(320, 126);
            this.PIDControllerGroupBox.TabIndex = 22;
            this.PIDControllerGroupBox.TabStop = false;
            this.PIDControllerGroupBox.Text = "PID Controller";
            // 
            // KdIndicator
            // 
            this.KdIndicator.BackColor = System.Drawing.Color.SlateGray;
            this.KdIndicator.Enabled = false;
            this.KdIndicator.Location = new System.Drawing.Point(230, 110);
            this.KdIndicator.Name = "KdIndicator";
            this.KdIndicator.Size = new System.Drawing.Size(60, 5);
            this.KdIndicator.TabIndex = 6;
            this.KdIndicator.Visible = false;
            // 
            // KiIndicator
            // 
            this.KiIndicator.BackColor = System.Drawing.Color.SlateGray;
            this.KiIndicator.Enabled = false;
            this.KiIndicator.Location = new System.Drawing.Point(130, 110);
            this.KiIndicator.Name = "KiIndicator";
            this.KiIndicator.Size = new System.Drawing.Size(60, 5);
            this.KiIndicator.TabIndex = 5;
            this.KiIndicator.Visible = false;
            // 
            // KpIndicator
            // 
            this.KpIndicator.BackColor = System.Drawing.Color.SlateGray;
            this.KpIndicator.Enabled = false;
            this.KpIndicator.Location = new System.Drawing.Point(27, 110);
            this.KpIndicator.Name = "KpIndicator";
            this.KpIndicator.Size = new System.Drawing.Size(60, 5);
            this.KpIndicator.TabIndex = 4;
            this.KpIndicator.Visible = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(19, 31);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(147, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Increment/Decrement Factor:";
            // 
            // incrementNumeric
            // 
            this.incrementNumeric.DecimalPlaces = 2;
            this.incrementNumeric.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.incrementNumeric.Location = new System.Drawing.Point(177, 29);
            this.incrementNumeric.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.incrementNumeric.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.incrementNumeric.Name = "incrementNumeric";
            this.incrementNumeric.Size = new System.Drawing.Size(67, 20);
            this.incrementNumeric.TabIndex = 0;
            this.incrementNumeric.Value = new decimal(new int[] {
            5,
            0,
            0,
            131072});
            this.incrementNumeric.ValueChanged += new System.EventHandler(this.IncrementNumeric_ValueChanged);
            // 
            // KdGroupBox
            // 
            this.KdGroupBox.Controls.Add(this.KdNumeric);
            this.KdGroupBox.Location = new System.Drawing.Point(215, 61);
            this.KdGroupBox.Name = "KdGroupBox";
            this.KdGroupBox.Size = new System.Drawing.Size(87, 49);
            this.KdGroupBox.TabIndex = 1;
            this.KdGroupBox.TabStop = false;
            this.KdGroupBox.Text = "Kd";
            // 
            // KdNumeric
            // 
            this.KdNumeric.DecimalPlaces = 2;
            this.KdNumeric.Increment = new decimal(new int[] {
            5,
            0,
            0,
            131072});
            this.KdNumeric.Location = new System.Drawing.Point(6, 19);
            this.KdNumeric.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.KdNumeric.Name = "KdNumeric";
            this.KdNumeric.Size = new System.Drawing.Size(75, 20);
            this.KdNumeric.TabIndex = 3;
            this.KdNumeric.ValueChanged += new System.EventHandler(this.PIDControllers_ValueChanged);
            // 
            // KiGroupBox
            // 
            this.KiGroupBox.Controls.Add(this.KiNumeric);
            this.KiGroupBox.Location = new System.Drawing.Point(115, 61);
            this.KiGroupBox.Name = "KiGroupBox";
            this.KiGroupBox.Size = new System.Drawing.Size(87, 49);
            this.KiGroupBox.TabIndex = 1;
            this.KiGroupBox.TabStop = false;
            this.KiGroupBox.Text = "Ki";
            // 
            // KiNumeric
            // 
            this.KiNumeric.DecimalPlaces = 2;
            this.KiNumeric.Increment = new decimal(new int[] {
            5,
            0,
            0,
            131072});
            this.KiNumeric.Location = new System.Drawing.Point(6, 19);
            this.KiNumeric.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.KiNumeric.Name = "KiNumeric";
            this.KiNumeric.Size = new System.Drawing.Size(75, 20);
            this.KiNumeric.TabIndex = 2;
            this.KiNumeric.ValueChanged += new System.EventHandler(this.PIDControllers_ValueChanged);
            // 
            // KpGroupBox
            // 
            this.KpGroupBox.Controls.Add(this.KpNumeric);
            this.KpGroupBox.Location = new System.Drawing.Point(16, 61);
            this.KpGroupBox.Name = "KpGroupBox";
            this.KpGroupBox.Size = new System.Drawing.Size(87, 49);
            this.KpGroupBox.TabIndex = 0;
            this.KpGroupBox.TabStop = false;
            this.KpGroupBox.Text = "Kp";
            // 
            // KpNumeric
            // 
            this.KpNumeric.DecimalPlaces = 2;
            this.KpNumeric.Increment = new decimal(new int[] {
            5,
            0,
            0,
            131072});
            this.KpNumeric.Location = new System.Drawing.Point(6, 19);
            this.KpNumeric.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.KpNumeric.Name = "KpNumeric";
            this.KpNumeric.Size = new System.Drawing.Size(75, 20);
            this.KpNumeric.TabIndex = 1;
            this.KpNumeric.ValueChanged += new System.EventHandler(this.PIDControllers_ValueChanged);
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(13, 479);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(320, 32);
            this.label2.TabIndex = 23;
            this.label2.Text = "Use Start/Back to switch the active PID parameter, then LB/RB to Decrement/Increm" +
                "ent its value";
            // 
            // ObstacleAvoidanceForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(345, 517);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.PIDControllerGroupBox);
            this.Controls.Add(this.depthProfileCtrl);
            this.MaximizeBox = false;
            this.Name = "ObstacleAvoidanceForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Obstacle Avoidance";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ObstacleAvoidanceForm_FormClosed);
            this.Load += new System.EventHandler(this.ObstacleAvoidanceForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.depthProfileCtrl)).EndInit();
            this.PIDControllerGroupBox.ResumeLayout(false);
            this.PIDControllerGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.incrementNumeric)).EndInit();
            this.KdGroupBox.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.KdNumeric)).EndInit();
            this.KiGroupBox.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.KiNumeric)).EndInit();
            this.KpGroupBox.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.KpNumeric)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox depthProfileCtrl;
        private System.Windows.Forms.GroupBox PIDControllerGroupBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown incrementNumeric;
        private System.Windows.Forms.GroupBox KdGroupBox;
        private System.Windows.Forms.NumericUpDown KdNumeric;
        private System.Windows.Forms.GroupBox KiGroupBox;
        private System.Windows.Forms.NumericUpDown KiNumeric;
        private System.Windows.Forms.GroupBox KpGroupBox;
        private System.Windows.Forms.NumericUpDown KpNumeric;
        private System.Windows.Forms.Panel KpIndicator;
        private System.Windows.Forms.Panel KdIndicator;
        private System.Windows.Forms.Panel KiIndicator;
        private System.Windows.Forms.Label label2;
    }
}