namespace Microsoft.Robotics.Services.Sample.SimpleVision
{
    partial class VisionForm
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
            this.picCamera = new System.Windows.Forms.PictureBox();
            this.button1 = new System.Windows.Forms.Button();
            this.picTest = new System.Windows.Forms.PictureBox();
            this.picTest2 = new System.Windows.Forms.PictureBox();
            this.rRange = new System.Windows.Forms.Label();
            this.gRange = new System.Windows.Forms.Label();
            this.bRange = new System.Windows.Forms.Label();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.normR = new System.Windows.Forms.Label();
            this.normG = new System.Windows.Forms.Label();
            this.normB = new System.Windows.Forms.Label();
            this.button4 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.textSmilarity = new System.Windows.Forms.TextBox();
            this.handGesture = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.picFace = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.picCamera)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picTest)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picTest2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picFace)).BeginInit();
            this.SuspendLayout();
            //
            // picCamera
            //
            this.picCamera.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.picCamera.Location = new System.Drawing.Point(19, 86);
            this.picCamera.Name = "picCamera";
            this.picCamera.Size = new System.Drawing.Size(160, 120);
            this.picCamera.TabIndex = 0;
            this.picCamera.TabStop = false;
            this.picCamera.MouseDown += new System.Windows.Forms.MouseEventHandler(this.picCamera_MouseDown);
            this.picCamera.MouseMove += new System.Windows.Forms.MouseEventHandler(this.picCamera_MouseMove);
            this.picCamera.Paint += new System.Windows.Forms.PaintEventHandler(this.picCamera_Paint);
            this.picCamera.MouseUp += new System.Windows.Forms.MouseEventHandler(this.picCamera_MouseUp);
            //
            // button1
            //
            this.button1.Location = new System.Drawing.Point(353, 21);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 1;
            this.button1.Text = "TrainColor";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            this.button1.KeyUp += new System.Windows.Forms.KeyEventHandler(this.VisionForm_KeyUp);
            this.button1.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.VisionForm_KeyPress);
            this.button1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.VisionForm_KeyDown);
            //
            // picTest
            //
            this.picTest.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.picTest.Location = new System.Drawing.Point(19, 257);
            this.picTest.Name = "picTest";
            this.picTest.Size = new System.Drawing.Size(160, 120);
            this.picTest.TabIndex = 2;
            this.picTest.TabStop = false;
            //
            // picTest2
            //
            this.picTest2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.picTest2.Location = new System.Drawing.Point(200, 257);
            this.picTest2.Name = "picTest2";
            this.picTest2.Size = new System.Drawing.Size(160, 120);
            this.picTest2.TabIndex = 3;
            this.picTest2.TabStop = false;
            //
            // rRange
            //
            this.rRange.AutoSize = true;
            this.rRange.Location = new System.Drawing.Point(350, 50);
            this.rRange.Name = "rRange";
            this.rRange.Size = new System.Drawing.Size(13, 13);
            this.rRange.TabIndex = 7;
            this.rRange.Text = "0";
            //
            // gRange
            //
            this.gRange.AutoSize = true;
            this.gRange.Location = new System.Drawing.Point(350, 74);
            this.gRange.Name = "gRange";
            this.gRange.Size = new System.Drawing.Size(13, 13);
            this.gRange.TabIndex = 8;
            this.gRange.Text = "0";
            //
            // bRange
            //
            this.bRange.AutoSize = true;
            this.bRange.Location = new System.Drawing.Point(350, 98);
            this.bRange.Name = "bRange";
            this.bRange.Size = new System.Drawing.Size(13, 13);
            this.bRange.TabIndex = 9;
            this.bRange.Text = "0";
            //
            // button2
            //
            this.button2.Location = new System.Drawing.Point(19, 50);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 10;
            this.button2.Text = "SHOW";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            this.button2.KeyUp += new System.Windows.Forms.KeyEventHandler(this.VisionForm_KeyUp);
            this.button2.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.VisionForm_KeyPress);
            this.button2.KeyDown += new System.Windows.Forms.KeyEventHandler(this.VisionForm_KeyDown);
            //
            // button3
            //
            this.button3.Location = new System.Drawing.Point(111, 50);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(75, 23);
            this.button3.TabIndex = 11;
            this.button3.Text = "Test";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            this.button3.KeyUp += new System.Windows.Forms.KeyEventHandler(this.VisionForm_KeyUp);
            this.button3.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.VisionForm_KeyPress);
            this.button3.KeyDown += new System.Windows.Forms.KeyEventHandler(this.VisionForm_KeyDown);
            //
            // normR
            //
            this.normR.AutoSize = true;
            this.normR.Location = new System.Drawing.Point(350, 131);
            this.normR.Name = "normR";
            this.normR.Size = new System.Drawing.Size(13, 13);
            this.normR.TabIndex = 12;
            this.normR.Text = "0";
            //
            // normG
            //
            this.normG.AutoSize = true;
            this.normG.Location = new System.Drawing.Point(350, 157);
            this.normG.Name = "normG";
            this.normG.Size = new System.Drawing.Size(13, 13);
            this.normG.TabIndex = 13;
            this.normG.Text = "0";
            //
            // normB
            //
            this.normB.AutoSize = true;
            this.normB.Location = new System.Drawing.Point(350, 185);
            this.normB.Name = "normB";
            this.normB.Size = new System.Drawing.Size(13, 13);
            this.normB.TabIndex = 14;
            this.normB.Text = "0";
            //
            // button4
            //
            this.button4.Location = new System.Drawing.Point(19, 21);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(75, 23);
            this.button4.TabIndex = 16;
            this.button4.Text = "RED";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            this.button4.KeyUp += new System.Windows.Forms.KeyEventHandler(this.VisionForm_KeyUp);
            this.button4.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.VisionForm_KeyPress);
            this.button4.KeyDown += new System.Windows.Forms.KeyEventHandler(this.VisionForm_KeyDown);
            //
            // button5
            //
            this.button5.Location = new System.Drawing.Point(111, 21);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(75, 23);
            this.button5.TabIndex = 17;
            this.button5.Text = "BLUE";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            this.button5.KeyUp += new System.Windows.Forms.KeyEventHandler(this.VisionForm_KeyUp);
            this.button5.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.VisionForm_KeyPress);
            this.button5.KeyDown += new System.Windows.Forms.KeyEventHandler(this.VisionForm_KeyDown);
            //
            // label5
            //
            this.label5.AutoSize = true;
            this.label5.ForeColor = System.Drawing.SystemColors.ActiveCaption;
            this.label5.Location = new System.Drawing.Point(249, 21);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(94, 13);
            this.label5.TabIndex = 18;
            this.label5.Text = "SimilarityThreshold";
            //
            // label1
            //
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(59, 219);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(43, 13);
            this.label1.TabIndex = 19;
            this.label1.Text = "Camera";
            //
            // label2
            //
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(59, 389);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(31, 13);
            this.label2.TabIndex = 20;
            this.label2.Text = "Color";
            //
            // label3
            //
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(258, 389);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(39, 13);
            this.label3.TabIndex = 21;
            this.label3.Text = "Motion";
            //
            // label4
            //
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(249, 159);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(31, 13);
            this.label4.TabIndex = 22;
            this.label4.Text = "Face";
            //
            // textSmilarity
            //
            this.textSmilarity.Location = new System.Drawing.Point(265, 43);
            this.textSmilarity.Name = "textSmilarity";
            this.textSmilarity.Size = new System.Drawing.Size(60, 20);
            this.textSmilarity.TabIndex = 23;
            this.textSmilarity.Text = "0.995";
            //
            // handGesture
            //
            this.handGesture.AutoSize = true;
            this.handGesture.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.handGesture.ForeColor = System.Drawing.SystemColors.ActiveCaption;
            this.handGesture.Location = new System.Drawing.Point(234, 210);
            this.handGesture.Name = "handGesture";
            this.handGesture.Size = new System.Drawing.Size(72, 25);
            this.handGesture.TabIndex = 24;
            this.handGesture.Text = "NONE";
            //
            // label6
            //
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(236, 193);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(70, 13);
            this.label6.TabIndex = 25;
            this.label6.Text = "HandGesture";
            //
            // picFace
            //
            this.picFace.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.picFace.Image = global::Microsoft.Robotics.Services.Sample.SimpleVision.Properties.Resources.Smile;
            this.picFace.Location = new System.Drawing.Point(237, 87);
            this.picFace.Name = "picFace";
            this.picFace.Size = new System.Drawing.Size(67, 68);
            this.picFace.TabIndex = 15;
            this.picFace.TabStop = false;
            //
            // VisionForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(440, 444);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.handGesture);
            this.Controls.Add(this.textSmilarity);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.picFace);
            this.Controls.Add(this.normB);
            this.Controls.Add(this.normG);
            this.Controls.Add(this.normR);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.bRange);
            this.Controls.Add(this.gRange);
            this.Controls.Add(this.rRange);
            this.Controls.Add(this.picTest2);
            this.Controls.Add(this.picTest);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.picCamera);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "VisionForm";
            this.Text = "Vision";
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.VisionForm_KeyPress);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.VisionForm_KeyUp);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.VisionForm_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.picCamera)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picTest)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picTest2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picFace)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox picCamera;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.PictureBox picTest;
        private System.Windows.Forms.PictureBox picTest2;
        private System.Windows.Forms.Label rRange;
        private System.Windows.Forms.Label gRange;
        private System.Windows.Forms.Label bRange;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Label normR;
        private System.Windows.Forms.Label normG;
        private System.Windows.Forms.Label normB;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textSmilarity;
        private System.Windows.Forms.Label handGesture;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.PictureBox picFace;
    }
}