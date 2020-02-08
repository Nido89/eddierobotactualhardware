namespace Microsoft.Robotics.Services.Samples
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
            this._formPicture = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this._formPicture)).BeginInit();
            this.SuspendLayout();
            // 
            // _formPicture
            // 
            this._formPicture.Dock = System.Windows.Forms.DockStyle.Fill;
            this._formPicture.Location = new System.Drawing.Point(0, 0);
            this._formPicture.Name = "_formPicture";
            this._formPicture.Size = new System.Drawing.Size(284, 264);
            this._formPicture.TabIndex = 0;
            this._formPicture.TabStop = false;
            // 
            // ImageProcessingResultForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 264);
            this.ControlBox = false;
            this.Controls.Add(this._formPicture);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "ImageProcessingResultForm";
            this.Text = "ImageProcessingResultForm";
            ((System.ComponentModel.ISupportInitialize)(this._formPicture)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox _formPicture;
    }
}