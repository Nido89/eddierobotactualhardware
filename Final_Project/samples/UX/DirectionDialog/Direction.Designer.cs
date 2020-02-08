namespace Microsoft.Robotics.Services.Sample.DirectionDialog
{
    partial class Direction
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Direction));
            this.btnBackwards = new System.Windows.Forms.Button();
            this.btnLeft = new System.Windows.Forms.Button();
            this.btnForwards = new System.Windows.Forms.Button();
            this.btnRight = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.SuspendLayout();
            //
            // btnBackwards
            //
            this.btnBackwards.Font = new System.Drawing.Font("Marlett", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(2)));
            this.btnBackwards.Location = new System.Drawing.Point(93, 70);
            this.btnBackwards.Name = "btnBackwards";
            this.btnBackwards.Size = new System.Drawing.Size(75, 23);
            this.btnBackwards.TabIndex = 0;
            this.btnBackwards.Text = "6";
            this.btnBackwards.UseVisualStyleBackColor = true;
            this.btnBackwards.MouseDown += new System.Windows.Forms.MouseEventHandler(this.button_MouseDown);
            this.btnBackwards.MouseUp += new System.Windows.Forms.MouseEventHandler(this.button_MouseUp);
            //
            // btnLeft
            //
            this.btnLeft.Font = new System.Drawing.Font("Marlett", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(2)));
            this.btnLeft.Location = new System.Drawing.Point(12, 41);
            this.btnLeft.Name = "btnLeft";
            this.btnLeft.Size = new System.Drawing.Size(75, 23);
            this.btnLeft.TabIndex = 1;
            this.btnLeft.Text = "3";
            this.btnLeft.UseVisualStyleBackColor = true;
            this.btnLeft.MouseDown += new System.Windows.Forms.MouseEventHandler(this.button_MouseDown);
            this.btnLeft.MouseUp += new System.Windows.Forms.MouseEventHandler(this.button_MouseUp);
            //
            // btnForwards
            //
            this.btnForwards.Font = new System.Drawing.Font("Marlett", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(2)));
            this.btnForwards.Location = new System.Drawing.Point(93, 12);
            this.btnForwards.Name = "btnForwards";
            this.btnForwards.Size = new System.Drawing.Size(75, 23);
            this.btnForwards.TabIndex = 2;
            this.btnForwards.Text = "5";
            this.btnForwards.UseVisualStyleBackColor = true;
            this.btnForwards.MouseDown += new System.Windows.Forms.MouseEventHandler(this.button_MouseDown);
            this.btnForwards.MouseUp += new System.Windows.Forms.MouseEventHandler(this.button_MouseUp);
            //
            // btnRight
            //
            this.btnRight.Font = new System.Drawing.Font("Marlett", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(2)));
            this.btnRight.Location = new System.Drawing.Point(174, 41);
            this.btnRight.Name = "btnRight";
            this.btnRight.Size = new System.Drawing.Size(75, 23);
            this.btnRight.TabIndex = 3;
            this.btnRight.Text = "4";
            this.btnRight.UseVisualStyleBackColor = true;
            this.btnRight.MouseDown += new System.Windows.Forms.MouseEventHandler(this.button_MouseDown);
            this.btnRight.MouseUp += new System.Windows.Forms.MouseEventHandler(this.button_MouseUp);
            //
            // btnStop
            //
            this.btnStop.Location = new System.Drawing.Point(93, 41);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(75, 23);
            this.btnStop.TabIndex = 4;
            this.btnStop.Text = "Stop";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.MouseDown += new System.Windows.Forms.MouseEventHandler(this.button_MouseDown);
            this.btnStop.MouseUp += new System.Windows.Forms.MouseEventHandler(this.button_MouseUp);
            //
            // Direction
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(257, 102);
            this.ControlBox = false;
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.btnRight);
            this.Controls.Add(this.btnForwards);
            this.Controls.Add(this.btnLeft);
            this.Controls.Add(this.btnBackwards);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Direction";
            this.Text = "DSS Direction Dialog";
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.Direction_KeyUp);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Direction_KeyDown);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnBackwards;
        private System.Windows.Forms.Button btnLeft;
        private System.Windows.Forms.Button btnForwards;
        private System.Windows.Forms.Button btnRight;
        private System.Windows.Forms.Button btnStop;
    }
}