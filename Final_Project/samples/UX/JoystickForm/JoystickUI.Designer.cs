namespace Microsoft.Robotics.Services.Sample.JoystickForm
{
    partial class JoystickUI
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
            this.components = new System.ComponentModel.Container();
            this.picJoystick = new System.Windows.Forms.PictureBox();
            this.button1 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.button6 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.button8 = new System.Windows.Forms.Button();
            this.button9 = new System.Windows.Forms.Button();
            this.button7 = new System.Windows.Forms.Button();
            this.button0 = new System.Windows.Forms.Button();
            this.decayTimer = new System.Windows.Forms.Timer(this.components);
            this.lblX = new System.Windows.Forms.Label();
            this.lblY = new System.Windows.Forms.Label();
            this.chkSticky = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.picJoystick)).BeginInit();
            this.SuspendLayout();
            // 
            // picJoystick
            // 
            this.picJoystick.Location = new System.Drawing.Point(12, 12);
            this.picJoystick.Name = "picJoystick";
            this.picJoystick.Size = new System.Drawing.Size(138, 138);
            this.picJoystick.TabIndex = 0;
            this.picJoystick.TabStop = false;
            this.picJoystick.MouseLeave += new System.EventHandler(this.picJoystick_MouseLeave);
            this.picJoystick.MouseMove += new System.Windows.Forms.MouseEventHandler(this.picJoystick_MouseMove);
            this.picJoystick.MouseUp += new System.Windows.Forms.MouseEventHandler(this.picJoystick_MouseUp);
            // 
            // button1
            // 
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.Location = new System.Drawing.Point(156, 48);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(30, 30);
            this.button1.TabIndex = 1;
            this.button1.Text = "1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.button_MouseDown);
            this.button1.KeyUp += new System.Windows.Forms.KeyEventHandler(this.JoystickUI_KeyUp);
            this.button1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.button_MouseUp);
            this.button1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.JoystickUI_KeyDown);
            // 
            // button3
            // 
            this.button3.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button3.Location = new System.Drawing.Point(228, 48);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(30, 30);
            this.button3.TabIndex = 3;
            this.button3.Text = "3";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.MouseDown += new System.Windows.Forms.MouseEventHandler(this.button_MouseDown);
            this.button3.KeyUp += new System.Windows.Forms.KeyEventHandler(this.JoystickUI_KeyUp);
            this.button3.MouseUp += new System.Windows.Forms.MouseEventHandler(this.button_MouseUp);
            this.button3.KeyDown += new System.Windows.Forms.KeyEventHandler(this.JoystickUI_KeyDown);
            // 
            // button2
            // 
            this.button2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button2.Location = new System.Drawing.Point(192, 48);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(30, 30);
            this.button2.TabIndex = 2;
            this.button2.Text = "2";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.MouseDown += new System.Windows.Forms.MouseEventHandler(this.button_MouseDown);
            this.button2.KeyUp += new System.Windows.Forms.KeyEventHandler(this.JoystickUI_KeyUp);
            this.button2.MouseUp += new System.Windows.Forms.MouseEventHandler(this.button_MouseUp);
            this.button2.KeyDown += new System.Windows.Forms.KeyEventHandler(this.JoystickUI_KeyDown);
            // 
            // button5
            // 
            this.button5.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button5.Location = new System.Drawing.Point(192, 84);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(30, 30);
            this.button5.TabIndex = 5;
            this.button5.Text = "5";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.MouseDown += new System.Windows.Forms.MouseEventHandler(this.button_MouseDown);
            this.button5.KeyUp += new System.Windows.Forms.KeyEventHandler(this.JoystickUI_KeyUp);
            this.button5.MouseUp += new System.Windows.Forms.MouseEventHandler(this.button_MouseUp);
            this.button5.KeyDown += new System.Windows.Forms.KeyEventHandler(this.JoystickUI_KeyDown);
            // 
            // button6
            // 
            this.button6.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button6.Location = new System.Drawing.Point(228, 84);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(30, 30);
            this.button6.TabIndex = 6;
            this.button6.Text = "6";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.MouseDown += new System.Windows.Forms.MouseEventHandler(this.button_MouseDown);
            this.button6.KeyUp += new System.Windows.Forms.KeyEventHandler(this.JoystickUI_KeyUp);
            this.button6.MouseUp += new System.Windows.Forms.MouseEventHandler(this.button_MouseUp);
            this.button6.KeyDown += new System.Windows.Forms.KeyEventHandler(this.JoystickUI_KeyDown);
            // 
            // button4
            // 
            this.button4.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button4.Location = new System.Drawing.Point(156, 84);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(30, 30);
            this.button4.TabIndex = 4;
            this.button4.Text = "4";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.MouseDown += new System.Windows.Forms.MouseEventHandler(this.button_MouseDown);
            this.button4.KeyUp += new System.Windows.Forms.KeyEventHandler(this.JoystickUI_KeyUp);
            this.button4.MouseUp += new System.Windows.Forms.MouseEventHandler(this.button_MouseUp);
            this.button4.KeyDown += new System.Windows.Forms.KeyEventHandler(this.JoystickUI_KeyDown);
            // 
            // button8
            // 
            this.button8.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button8.Location = new System.Drawing.Point(192, 120);
            this.button8.Name = "button8";
            this.button8.Size = new System.Drawing.Size(30, 30);
            this.button8.TabIndex = 8;
            this.button8.Text = "8";
            this.button8.UseVisualStyleBackColor = true;
            this.button8.MouseDown += new System.Windows.Forms.MouseEventHandler(this.button_MouseDown);
            this.button8.KeyUp += new System.Windows.Forms.KeyEventHandler(this.JoystickUI_KeyUp);
            this.button8.MouseUp += new System.Windows.Forms.MouseEventHandler(this.button_MouseUp);
            this.button8.KeyDown += new System.Windows.Forms.KeyEventHandler(this.JoystickUI_KeyDown);
            // 
            // button9
            // 
            this.button9.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button9.Location = new System.Drawing.Point(228, 120);
            this.button9.Name = "button9";
            this.button9.Size = new System.Drawing.Size(30, 30);
            this.button9.TabIndex = 9;
            this.button9.Text = "9";
            this.button9.UseVisualStyleBackColor = true;
            this.button9.MouseDown += new System.Windows.Forms.MouseEventHandler(this.button_MouseDown);
            this.button9.KeyUp += new System.Windows.Forms.KeyEventHandler(this.JoystickUI_KeyUp);
            this.button9.MouseUp += new System.Windows.Forms.MouseEventHandler(this.button_MouseUp);
            this.button9.KeyDown += new System.Windows.Forms.KeyEventHandler(this.JoystickUI_KeyDown);
            // 
            // button7
            // 
            this.button7.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button7.Location = new System.Drawing.Point(156, 120);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(30, 30);
            this.button7.TabIndex = 7;
            this.button7.Text = "7";
            this.button7.UseVisualStyleBackColor = true;
            this.button7.MouseDown += new System.Windows.Forms.MouseEventHandler(this.button_MouseDown);
            this.button7.KeyUp += new System.Windows.Forms.KeyEventHandler(this.JoystickUI_KeyUp);
            this.button7.MouseUp += new System.Windows.Forms.MouseEventHandler(this.button_MouseUp);
            this.button7.KeyDown += new System.Windows.Forms.KeyEventHandler(this.JoystickUI_KeyDown);
            // 
            // button0
            // 
            this.button0.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button0.Location = new System.Drawing.Point(155, 12);
            this.button0.Name = "button0";
            this.button0.Size = new System.Drawing.Size(102, 30);
            this.button0.TabIndex = 0;
            this.button0.Text = "0";
            this.button0.UseVisualStyleBackColor = true;
            this.button0.MouseDown += new System.Windows.Forms.MouseEventHandler(this.button_MouseDown);
            this.button0.KeyUp += new System.Windows.Forms.KeyEventHandler(this.JoystickUI_KeyUp);
            this.button0.MouseUp += new System.Windows.Forms.MouseEventHandler(this.button_MouseUp);
            this.button0.KeyDown += new System.Windows.Forms.KeyEventHandler(this.JoystickUI_KeyDown);
            // 
            // decayTimer
            // 
            this.decayTimer.Enabled = true;
            this.decayTimer.Tick += new System.EventHandler(this.decayTimer_Tick);
            // 
            // lblX
            // 
            this.lblX.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblX.Location = new System.Drawing.Point(12, 156);
            this.lblX.Name = "lblX";
            this.lblX.Size = new System.Drawing.Size(66, 17);
            this.lblX.TabIndex = 10;
            this.lblX.Text = "X: 0";
            // 
            // lblY
            // 
            this.lblY.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblY.Location = new System.Drawing.Point(84, 156);
            this.lblY.Name = "lblY";
            this.lblY.Size = new System.Drawing.Size(66, 17);
            this.lblY.TabIndex = 11;
            this.lblY.Text = "Y: 0";
            // 
            // chkSticky
            // 
            this.chkSticky.AutoSize = true;
            this.chkSticky.Location = new System.Drawing.Point(163, 156);
            this.chkSticky.Name = "chkSticky";
            this.chkSticky.Size = new System.Drawing.Size(94, 17);
            this.chkSticky.TabIndex = 12;
            this.chkSticky.Text = "Stic&ky Buttons";
            this.chkSticky.UseVisualStyleBackColor = true;
            this.chkSticky.KeyUp += new System.Windows.Forms.KeyEventHandler(this.JoystickUI_KeyUp);
            this.chkSticky.CheckedChanged += new System.EventHandler(this.chkSticky_CheckedChanged);
            this.chkSticky.KeyDown += new System.Windows.Forms.KeyEventHandler(this.JoystickUI_KeyDown);
            // 
            // JoystickUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(270, 181);
            this.Controls.Add(this.chkSticky);
            this.Controls.Add(this.lblY);
            this.Controls.Add(this.lblX);
            this.Controls.Add(this.button0);
            this.Controls.Add(this.button8);
            this.Controls.Add(this.button9);
            this.Controls.Add(this.button7);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.button6);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.picJoystick);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "JoystickUI";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Desktop Joystick";
            this.TopMost = true;
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.JoystickUI_KeyUp);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.JoystickUI_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.picJoystick)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox picJoystick;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button8;
        private System.Windows.Forms.Button button9;
        private System.Windows.Forms.Button button7;
        private System.Windows.Forms.Button button0;
        private System.Windows.Forms.Timer decayTimer;
        private System.Windows.Forms.Label lblX;
        private System.Windows.Forms.Label lblY;
        private System.Windows.Forms.CheckBox chkSticky;
    }
}