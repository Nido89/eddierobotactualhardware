namespace Robotics.MultipleSimulatedSensors
{
    partial class ImageProcessingResultForm
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
            this._webcamPanel = new System.Windows.Forms.PictureBox();
            this._sensorGrid = new System.Windows.Forms.PropertyGrid();
            this._lrfPanel = new System.Windows.Forms.PictureBox();
            this._lrfLabel = new System.Windows.Forms.Label();
            this._webcamLabel = new System.Windows.Forms.Label();
            this._analogSensorsLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this._webcamPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._lrfPanel)).BeginInit();
            this.SuspendLayout();
            // 
            // _webcamPanel
            // 
            this._webcamPanel.Location = new System.Drawing.Point(0, 25);
            this._webcamPanel.Name = "_webcamPanel";
            this._webcamPanel.Size = new System.Drawing.Size(320, 240);
            this._webcamPanel.TabIndex = 0;
            this._webcamPanel.TabStop = false;
            // 
            // _sensorGrid
            // 
            this._sensorGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._sensorGrid.HelpVisible = false;
            this._sensorGrid.Location = new System.Drawing.Point(326, 25);
            this._sensorGrid.Name = "_sensorGrid";
            this._sensorGrid.PropertySort = System.Windows.Forms.PropertySort.Alphabetical;
            this._sensorGrid.Size = new System.Drawing.Size(322, 240);
            this._sensorGrid.TabIndex = 2;
            this._sensorGrid.ToolbarVisible = false;
            // 
            // _lrfPanel
            // 
            this._lrfPanel.Location = new System.Drawing.Point(0, 291);
            this._lrfPanel.Name = "_lrfPanel";
            this._lrfPanel.Size = new System.Drawing.Size(648, 150);
            this._lrfPanel.TabIndex = 3;
            this._lrfPanel.TabStop = false;
            // 
            // _lrfLabel
            // 
            this._lrfLabel.AutoSize = true;
            this._lrfLabel.Location = new System.Drawing.Point(-3, 275);
            this._lrfLabel.Name = "_lrfLabel";
            this._lrfLabel.Size = new System.Drawing.Size(217, 13);
            this._lrfLabel.TabIndex = 5;
            this._lrfLabel.Text = "Laser Range Finder Distance Measurements";
            // 
            // _webcamLabel
            // 
            this._webcamLabel.AutoSize = true;
            this._webcamLabel.Location = new System.Drawing.Point(-3, 9);
            this._webcamLabel.Name = "_webcamLabel";
            this._webcamLabel.Size = new System.Drawing.Size(118, 13);
            this._webcamLabel.TabIndex = 5;
            this._webcamLabel.Text = "Edge Detection Display";
            // 
            // _analogSensorsLabel
            // 
            this._analogSensorsLabel.AutoSize = true;
            this._analogSensorsLabel.Location = new System.Drawing.Point(323, 9);
            this._analogSensorsLabel.Name = "_analogSensorsLabel";
            this._analogSensorsLabel.Size = new System.Drawing.Size(124, 13);
            this._analogSensorsLabel.TabIndex = 5;
            this._analogSensorsLabel.Text = "Analog Sensor Readings";
            // 
            // ImageProcessingResultForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(651, 451);
            this.ControlBox = false;
            this.Controls.Add(this._analogSensorsLabel);
            this.Controls.Add(this._webcamLabel);
            this.Controls.Add(this._lrfLabel);
            this.Controls.Add(this._lrfPanel);
            this.Controls.Add(this._sensorGrid);
            this.Controls.Add(this._webcamPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "ImageProcessingResultForm";
            this.Text = "Sensor Data Form";
            this.Load += new System.EventHandler(this.ImageProcessingResultForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this._webcamPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._lrfPanel)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox _webcamPanel;
        private System.Windows.Forms.PropertyGrid _sensorGrid;
        private System.Windows.Forms.PictureBox _lrfPanel;
        private System.Windows.Forms.Label _lrfLabel;
        private System.Windows.Forms.Label _webcamLabel;
        private System.Windows.Forms.Label _analogSensorsLabel;
    }
}