//-----------------------------------------------------------------------
//  This file is part of the Microsoft Robotics Studio Code Samples.
// 
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: DashboardForm.Designer.cs $ $Revision: 6 $
//-----------------------------------------------------------------------

namespace Microsoft.Robotics.Services.RobotDashboard
{
    partial class DashboardForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DashboardForm));
            this.JoystickComboBox = new System.Windows.Forms.ComboBox();
            this.lblX = new System.Windows.Forms.Label();
            this.lblY = new System.Windows.Forms.Label();
            this.lblZ = new System.Windows.Forms.Label();
            this.lblButtons = new System.Windows.Forms.Label();
            this.DirectDriveGroupBox = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.JoystickPicture = new System.Windows.Forms.PictureBox();
            this.RightWheelSpeed = new System.Windows.Forms.Label();
            this.LeftWheelSpeed = new System.Windows.Forms.Label();
            this.DiveCommandsGroupBox = new System.Windows.Forms.GroupBox();
            this.StopCheckBox = new System.Windows.Forms.CheckBox();
            this.DriveCheckBox = new System.Windows.Forms.CheckBox();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.MainMenu = new System.Windows.Forms.MenuStrip();
            this.FileMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.saveSettingsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolsMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.optionsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.HelpMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.HeadingLabel = new System.Windows.Forms.Label();
            this.TiltGroupBox = new System.Windows.Forms.GroupBox();
            this.TiltResetButton = new System.Windows.Forms.Button();
            this.TiltDownButton = new System.Windows.Forms.Button();
            this.TiltUpButton = new System.Windows.Forms.Button();
            this.SetTiltButton = new System.Windows.Forms.Button();
            this.TiltTextbox = new System.Windows.Forms.TextBox();
            this.TiltLabel = new System.Windows.Forms.Label();
            this.SensorsGroupBox = new System.Windows.Forms.GroupBox();
            this.label12 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.SonarRightLabel = new System.Windows.Forms.Label();
            this.IRRightLabel = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.SonarLeftLabel = new System.Windows.Forms.Label();
            this.IRCenterLabel = new System.Windows.Forms.Label();
            this.IRLeftLabel = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.BatteryLevelTxt = new System.Windows.Forms.Label();
            this.BatteryLevel = new System.Windows.Forms.ProgressBar();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnResetEncoders = new System.Windows.Forms.Button();
            this.RightEncoderTicks = new System.Windows.Forms.Label();
            this.LeftEncoderTicks = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.DeviceLabel = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.DirectDriveGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.JoystickPicture)).BeginInit();
            this.DiveCommandsGroupBox.SuspendLayout();
            this.MainMenu.SuspendLayout();
            this.TiltGroupBox.SuspendLayout();
            this.SensorsGroupBox.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // JoystickComboBox
            // 
            this.JoystickComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.JoystickComboBox.FormattingEnabled = true;
            this.JoystickComboBox.Location = new System.Drawing.Point(75, 27);
            this.JoystickComboBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.JoystickComboBox.Name = "JoystickComboBox";
            this.JoystickComboBox.Size = new System.Drawing.Size(176, 24);
            this.JoystickComboBox.TabIndex = 0;
            this.JoystickComboBox.SelectedIndexChanged += new System.EventHandler(this.JoystickSelectedIndexChanged);
            // 
            // lblX
            // 
            this.lblX.Location = new System.Drawing.Point(43, 85);
            this.lblX.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblX.Name = "lblX";
            this.lblX.Size = new System.Drawing.Size(47, 16);
            this.lblX.TabIndex = 2;
            this.lblX.Text = "0";
            this.lblX.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblY
            // 
            this.lblY.Location = new System.Drawing.Point(43, 106);
            this.lblY.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblY.Name = "lblY";
            this.lblY.Size = new System.Drawing.Size(47, 16);
            this.lblY.TabIndex = 3;
            this.lblY.Text = "0";
            this.lblY.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblZ
            // 
            this.lblZ.Location = new System.Drawing.Point(43, 127);
            this.lblZ.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblZ.Name = "lblZ";
            this.lblZ.Size = new System.Drawing.Size(47, 16);
            this.lblZ.TabIndex = 4;
            this.lblZ.Text = "0";
            this.lblZ.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblButtons
            // 
            this.lblButtons.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblButtons.Font = new System.Drawing.Font("Lucida Console", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblButtons.Location = new System.Drawing.Point(84, 186);
            this.lblButtons.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblButtons.Name = "lblButtons";
            this.lblButtons.Size = new System.Drawing.Size(168, 16);
            this.lblButtons.TabIndex = 9;
            this.lblButtons.Text = "O";
            // 
            // DirectDriveGroupBox
            // 
            this.DirectDriveGroupBox.Controls.Add(this.label1);
            this.DirectDriveGroupBox.Controls.Add(this.JoystickPicture);
            this.DirectDriveGroupBox.Controls.Add(DeviceLabel);
            this.DirectDriveGroupBox.Controls.Add(this.lblButtons);
            this.DirectDriveGroupBox.Controls.Add(this.JoystickComboBox);
            this.DirectDriveGroupBox.Controls.Add(this.lblX);
            this.DirectDriveGroupBox.Controls.Add(label7);
            this.DirectDriveGroupBox.Controls.Add(this.lblY);
            this.DirectDriveGroupBox.Controls.Add(label6);
            this.DirectDriveGroupBox.Controls.Add(this.lblZ);
            this.DirectDriveGroupBox.Controls.Add(label5);
            this.DirectDriveGroupBox.Location = new System.Drawing.Point(296, 68);
            this.DirectDriveGroupBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.DirectDriveGroupBox.Name = "DirectDriveGroupBox";
            this.DirectDriveGroupBox.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.DirectDriveGroupBox.Size = new System.Drawing.Size(260, 219);
            this.DirectDriveGroupBox.TabIndex = 10;
            this.DirectDriveGroupBox.TabStop = false;
            this.DirectDriveGroupBox.Text = "Direct Drive";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 183);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(60, 17);
            this.label1.TabIndex = 11;
            this.label1.Text = "Buttons:";
            // 
            // JoystickPicture
            // 
            this.JoystickPicture.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.JoystickPicture.Location = new System.Drawing.Point(97, 59);
            this.JoystickPicture.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.JoystickPicture.Name = "JoystickPicture";
            this.JoystickPicture.Size = new System.Drawing.Size(120, 111);
            this.JoystickPicture.TabIndex = 10;
            this.JoystickPicture.TabStop = false;
            this.JoystickPicture.MouseLeave += new System.EventHandler(this.JoystickPictureMouseLeave);
            this.JoystickPicture.MouseMove += new System.Windows.Forms.MouseEventHandler(this.JoystickPictureMouseMove);
            this.JoystickPicture.MouseUp += new System.Windows.Forms.MouseEventHandler(this.JoystickPictureMouseUp);
            // 
            // DeviceLabel
            // 
            this.DeviceLabel.AutoSize = true;
            this.DeviceLabel.Location = new System.Drawing.Point(8, 31);
            this.DeviceLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.DeviceLabel.Name = "DeviceLabel";
            this.DeviceLabel.Size = new System.Drawing.Size(55, 17);
            this.DeviceLabel.TabIndex = 1;
            this.DeviceLabel.Text = "Device:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(8, 127);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(21, 17);
            this.label7.TabIndex = 7;
            this.label7.Text = "Z:";
            this.label7.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(8, 106);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(21, 17);
            this.label6.TabIndex = 6;
            this.label6.Text = "Y:";
            this.label6.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(8, 85);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(21, 17);
            this.label5.TabIndex = 5;
            this.label5.Text = "X:";
            this.label5.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // RightWheelSpeed
            // 
            this.RightWheelSpeed.AutoSize = true;
            this.RightWheelSpeed.Location = new System.Drawing.Point(212, 69);
            this.RightWheelSpeed.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.RightWheelSpeed.Name = "RightWheelSpeed";
            this.RightWheelSpeed.Size = new System.Drawing.Size(36, 17);
            this.RightWheelSpeed.TabIndex = 29;
            this.RightWheelSpeed.Text = "0.00";
            this.RightWheelSpeed.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // LeftWheelSpeed
            // 
            this.LeftWheelSpeed.AutoSize = true;
            this.LeftWheelSpeed.Location = new System.Drawing.Point(155, 69);
            this.LeftWheelSpeed.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.LeftWheelSpeed.Name = "LeftWheelSpeed";
            this.LeftWheelSpeed.Size = new System.Drawing.Size(36, 17);
            this.LeftWheelSpeed.TabIndex = 29;
            this.LeftWheelSpeed.Text = "0.00";
            this.LeftWheelSpeed.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // DiveCommandsGroupBox
            // 
            this.DiveCommandsGroupBox.Controls.Add(this.StopCheckBox);
            this.DiveCommandsGroupBox.Controls.Add(this.DriveCheckBox);
            this.DiveCommandsGroupBox.Location = new System.Drawing.Point(16, 68);
            this.DiveCommandsGroupBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.DiveCommandsGroupBox.Name = "DiveCommandsGroupBox";
            this.DiveCommandsGroupBox.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.DiveCommandsGroupBox.Size = new System.Drawing.Size(260, 79);
            this.DiveCommandsGroupBox.TabIndex = 13;
            this.DiveCommandsGroupBox.TabStop = false;
            this.DiveCommandsGroupBox.Text = "Drive Commands";
            // 
            // StopCheckBox
            // 
            this.StopCheckBox.Appearance = System.Windows.Forms.Appearance.Button;
            this.StopCheckBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.StopCheckBox.Location = new System.Drawing.Point(163, 23);
            this.StopCheckBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.StopCheckBox.Name = "StopCheckBox";
            this.StopCheckBox.Size = new System.Drawing.Size(87, 39);
            this.StopCheckBox.TabIndex = 26;
            this.StopCheckBox.Text = "Stop";
            this.StopCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.StopCheckBox.UseVisualStyleBackColor = true;
            this.StopCheckBox.CheckedChanged += new System.EventHandler(this.StopCheckBoxCheckedChanged);
            // 
            // DriveCheckBox
            // 
            this.DriveCheckBox.Appearance = System.Windows.Forms.Appearance.Button;
            this.DriveCheckBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DriveCheckBox.Location = new System.Drawing.Point(19, 23);
            this.DriveCheckBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.DriveCheckBox.Name = "DriveCheckBox";
            this.DriveCheckBox.Size = new System.Drawing.Size(120, 39);
            this.DriveCheckBox.TabIndex = 25;
            this.DriveCheckBox.Text = "Enable Drive";
            this.DriveCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.DriveCheckBox.UseVisualStyleBackColor = true;
            this.DriveCheckBox.CheckedChanged += new System.EventHandler(this.DriveCheckBoxCheckedChanged);
            // 
            // saveFileDialog
            // 
            this.saveFileDialog.Filter = "Xml log file|*.log;*.xml|All files|*.*";
            this.saveFileDialog.Title = "Log File";
            this.saveFileDialog.FileOk += new System.ComponentModel.CancelEventHandler(this.SaveFileDialogFileOk);
            // 
            // MainMenu
            // 
            this.MainMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.FileMenu,
            this.ToolsMenu,
            this.HelpMenu});
            this.MainMenu.Location = new System.Drawing.Point(0, 0);
            this.MainMenu.Name = "MainMenu";
            this.MainMenu.Padding = new System.Windows.Forms.Padding(8, 2, 0, 2);
            this.MainMenu.Size = new System.Drawing.Size(572, 28);
            this.MainMenu.TabIndex = 16;
            this.MainMenu.Text = "menuStrip1";
            // 
            // FileMenu
            // 
            this.FileMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveSettingsMenuItem,
            this.exitMenuItem});
            this.FileMenu.Name = "FileMenu";
            this.FileMenu.Size = new System.Drawing.Size(44, 24);
            this.FileMenu.Text = "&File";
            // 
            // saveSettingsMenuItem
            // 
            this.saveSettingsMenuItem.Name = "saveSettingsMenuItem";
            this.saveSettingsMenuItem.Size = new System.Drawing.Size(166, 24);
            this.saveSettingsMenuItem.Text = "&Save Settings";
            this.saveSettingsMenuItem.Click += new System.EventHandler(this.SaveSettingsMenuItemClick);
            // 
            // exitMenuItem
            // 
            this.exitMenuItem.Name = "exitMenuItem";
            this.exitMenuItem.Size = new System.Drawing.Size(166, 24);
            this.exitMenuItem.Text = "E&xit";
            this.exitMenuItem.Click += new System.EventHandler(this.ExitMenuItemClick);
            // 
            // ToolsMenu
            // 
            this.ToolsMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.optionsMenuItem});
            this.ToolsMenu.Name = "ToolsMenu";
            this.ToolsMenu.Size = new System.Drawing.Size(57, 24);
            this.ToolsMenu.Text = "&Tools";
            // 
            // optionsMenuItem
            // 
            this.optionsMenuItem.Name = "optionsMenuItem";
            this.optionsMenuItem.Size = new System.Drawing.Size(139, 24);
            this.optionsMenuItem.Text = "&Options...";
            this.optionsMenuItem.Click += new System.EventHandler(this.OptionsMenuItemClick);
            // 
            // HelpMenu
            // 
            this.HelpMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutMenuItem});
            this.HelpMenu.Name = "HelpMenu";
            this.HelpMenu.Size = new System.Drawing.Size(53, 24);
            this.HelpMenu.Text = "Help";
            // 
            // aboutMenuItem
            // 
            this.aboutMenuItem.Name = "aboutMenuItem";
            this.aboutMenuItem.Size = new System.Drawing.Size(119, 24);
            this.aboutMenuItem.Text = "About";
            this.aboutMenuItem.Click += new System.EventHandler(this.AboutMenuItemClick);
            // 
            // HeadingLabel
            // 
            this.HeadingLabel.AutoSize = true;
            this.HeadingLabel.Font = new System.Drawing.Font("Arial", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.HeadingLabel.Location = new System.Drawing.Point(20, 37);
            this.HeadingLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.HeadingLabel.Name = "HeadingLabel";
            this.HeadingLabel.Size = new System.Drawing.Size(201, 27);
            this.HeadingLabel.TabIndex = 17;
            this.HeadingLabel.Text = "Robot Dashboard";
            // 
            // TiltGroupBox
            // 
            this.TiltGroupBox.Controls.Add(this.TiltResetButton);
            this.TiltGroupBox.Controls.Add(this.TiltDownButton);
            this.TiltGroupBox.Controls.Add(this.TiltUpButton);
            this.TiltGroupBox.Controls.Add(this.SetTiltButton);
            this.TiltGroupBox.Controls.Add(this.TiltTextbox);
            this.TiltGroupBox.Controls.Add(this.TiltLabel);
            this.TiltGroupBox.Location = new System.Drawing.Point(16, 294);
            this.TiltGroupBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.TiltGroupBox.Name = "TiltGroupBox";
            this.TiltGroupBox.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.TiltGroupBox.Size = new System.Drawing.Size(260, 130);
            this.TiltGroupBox.TabIndex = 18;
            this.TiltGroupBox.TabStop = false;
            this.TiltGroupBox.Text = "Kinect Tilt Angle";
            // 
            // TiltResetButton
            // 
            this.TiltResetButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TiltResetButton.Location = new System.Drawing.Point(163, 69);
            this.TiltResetButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.TiltResetButton.Name = "TiltResetButton";
            this.TiltResetButton.Size = new System.Drawing.Size(87, 39);
            this.TiltResetButton.TabIndex = 25;
            this.TiltResetButton.Text = "Reset";
            this.TiltResetButton.UseVisualStyleBackColor = true;
            this.TiltResetButton.Click += new System.EventHandler(this.TiltResetButtonClick);
            // 
            // TiltDownButton
            // 
            this.TiltDownButton.Image = ((System.Drawing.Image)(resources.GetObject("TiltDownButton.Image")));
            this.TiltDownButton.Location = new System.Drawing.Point(96, 70);
            this.TiltDownButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.TiltDownButton.Name = "TiltDownButton";
            this.TiltDownButton.Size = new System.Drawing.Size(43, 39);
            this.TiltDownButton.TabIndex = 24;
            this.TiltDownButton.UseVisualStyleBackColor = true;
            this.TiltDownButton.Click += new System.EventHandler(this.TiltDownButtonClick);
            // 
            // TiltUpButton
            // 
            this.TiltUpButton.Image = ((System.Drawing.Image)(resources.GetObject("TiltUpButton.Image")));
            this.TiltUpButton.Location = new System.Drawing.Point(39, 70);
            this.TiltUpButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.TiltUpButton.Name = "TiltUpButton";
            this.TiltUpButton.Size = new System.Drawing.Size(43, 39);
            this.TiltUpButton.TabIndex = 22;
            this.TiltUpButton.UseVisualStyleBackColor = true;
            this.TiltUpButton.Click += new System.EventHandler(this.TiltUpButtonClick);
            // 
            // SetTiltButton
            // 
            this.SetTiltButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SetTiltButton.Location = new System.Drawing.Point(163, 23);
            this.SetTiltButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.SetTiltButton.Name = "SetTiltButton";
            this.SetTiltButton.Size = new System.Drawing.Size(87, 39);
            this.SetTiltButton.TabIndex = 2;
            this.SetTiltButton.Text = "Set";
            this.SetTiltButton.UseVisualStyleBackColor = true;
            this.SetTiltButton.Click += new System.EventHandler(this.SetTiltButtonClick);
            // 
            // TiltTextbox
            // 
            this.TiltTextbox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TiltTextbox.Location = new System.Drawing.Point(80, 31);
            this.TiltTextbox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.TiltTextbox.Name = "TiltTextbox";
            this.TiltTextbox.Size = new System.Drawing.Size(52, 24);
            this.TiltTextbox.TabIndex = 1;
            this.TiltTextbox.Text = "0";
            this.TiltTextbox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // TiltLabel
            // 
            this.TiltLabel.AutoSize = true;
            this.TiltLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TiltLabel.Location = new System.Drawing.Point(35, 34);
            this.TiltLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.TiltLabel.Name = "TiltLabel";
            this.TiltLabel.Size = new System.Drawing.Size(31, 18);
            this.TiltLabel.TabIndex = 0;
            this.TiltLabel.Text = "Tilt:";
            this.TiltLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // SensorsGroupBox
            // 
            this.SensorsGroupBox.Controls.Add(this.label12);
            this.SensorsGroupBox.Controls.Add(this.label9);
            this.SensorsGroupBox.Controls.Add(this.SonarRightLabel);
            this.SensorsGroupBox.Controls.Add(this.IRRightLabel);
            this.SensorsGroupBox.Controls.Add(this.label4);
            this.SensorsGroupBox.Controls.Add(this.label3);
            this.SensorsGroupBox.Controls.Add(this.label2);
            this.SensorsGroupBox.Controls.Add(this.SonarLeftLabel);
            this.SensorsGroupBox.Controls.Add(this.IRCenterLabel);
            this.SensorsGroupBox.Controls.Add(this.IRLeftLabel);
            this.SensorsGroupBox.Location = new System.Drawing.Point(13, 432);
            this.SensorsGroupBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.SensorsGroupBox.Name = "SensorsGroupBox";
            this.SensorsGroupBox.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.SensorsGroupBox.Size = new System.Drawing.Size(543, 122);
            this.SensorsGroupBox.TabIndex = 20;
            this.SensorsGroupBox.TabStop = false;
            this.SensorsGroupBox.Text = "Sensors";
            // 
            // label12
            // 
            this.label12.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label12.Location = new System.Drawing.Point(9, 68);
            this.label12.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(84, 20);
            this.label12.TabIndex = 5;
            this.label12.Text = "IR Left";
            this.label12.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label9
            // 
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.Location = new System.Drawing.Point(121, 68);
            this.label9.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(84, 20);
            this.label9.TabIndex = 5;
            this.label9.Text = "Sonar Left";
            this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // SonarRightLabel
            // 
            this.SonarRightLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SonarRightLabel.Location = new System.Drawing.Point(128, 25);
            this.SonarRightLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.SonarRightLabel.Name = "SonarRightLabel";
            this.SonarRightLabel.Size = new System.Drawing.Size(70, 28);
            this.SonarRightLabel.TabIndex = 4;
            this.SonarRightLabel.Text = "0";
            this.SonarRightLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // IRRightLabel
            // 
            this.IRRightLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.IRRightLabel.Location = new System.Drawing.Point(16, 25);
            this.IRRightLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.IRRightLabel.Name = "IRRightLabel";
            this.IRRightLabel.Size = new System.Drawing.Size(70, 28);
            this.IRRightLabel.TabIndex = 2;
            this.IRRightLabel.Text = "0";
            this.IRRightLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label4
            // 
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(227, 68);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(84, 20);
            this.label4.TabIndex = 5;
            this.label4.Text = "IR Center";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label3
            // 
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(343, 68);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(84, 20);
            this.label3.TabIndex = 5;
            this.label3.Text = "Sonar Right";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(439, 68);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(84, 20);
            this.label2.TabIndex = 5;
            this.label2.Text = "IR Right";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // SonarLeftLabel
            // 
            this.SonarLeftLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SonarLeftLabel.Location = new System.Drawing.Point(350, 25);
            this.SonarLeftLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.SonarLeftLabel.Name = "SonarLeftLabel";
            this.SonarLeftLabel.Size = new System.Drawing.Size(70, 28);
            this.SonarLeftLabel.TabIndex = 3;
            this.SonarLeftLabel.Text = "0";
            this.SonarLeftLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // IRCenterLabel
            // 
            this.IRCenterLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.IRCenterLabel.Location = new System.Drawing.Point(234, 25);
            this.IRCenterLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.IRCenterLabel.Name = "IRCenterLabel";
            this.IRCenterLabel.Size = new System.Drawing.Size(70, 28);
            this.IRCenterLabel.TabIndex = 1;
            this.IRCenterLabel.Text = "0";
            this.IRCenterLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // IRLeftLabel
            // 
            this.IRLeftLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.IRLeftLabel.Location = new System.Drawing.Point(446, 25);
            this.IRLeftLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.IRLeftLabel.Name = "IRLeftLabel";
            this.IRLeftLabel.Size = new System.Drawing.Size(70, 28);
            this.IRLeftLabel.TabIndex = 0;
            this.IRLeftLabel.Text = "0";
            this.IRLeftLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.BatteryLevelTxt);
            this.groupBox1.Controls.Add(this.BatteryLevel);
            this.groupBox1.Location = new System.Drawing.Point(296, 295);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox1.Size = new System.Drawing.Size(260, 123);
            this.groupBox1.TabIndex = 21;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Battery";
            // 
            // BatteryLevelTxt
            // 
            this.BatteryLevelTxt.AutoSize = true;
            this.BatteryLevelTxt.Location = new System.Drawing.Point(101, 81);
            this.BatteryLevelTxt.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.BatteryLevelTxt.Name = "BatteryLevelTxt";
            this.BatteryLevelTxt.Size = new System.Drawing.Size(28, 17);
            this.BatteryLevelTxt.TabIndex = 1;
            this.BatteryLevelTxt.Text = "0%";
            this.BatteryLevelTxt.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // BatteryLevel
            // 
            this.BatteryLevel.Location = new System.Drawing.Point(12, 28);
            this.BatteryLevel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.BatteryLevel.Name = "BatteryLevel";
            this.BatteryLevel.Size = new System.Drawing.Size(240, 33);
            this.BatteryLevel.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.BatteryLevel.TabIndex = 0;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btnResetEncoders);
            this.groupBox2.Controls.Add(this.RightEncoderTicks);
            this.groupBox2.Controls.Add(this.LeftEncoderTicks);
            this.groupBox2.Controls.Add(this.RightWheelSpeed);
            this.groupBox2.Controls.Add(this.label13);
            this.groupBox2.Controls.Add(this.label11);
            this.groupBox2.Controls.Add(this.LeftWheelSpeed);
            this.groupBox2.Controls.Add(this.label10);
            this.groupBox2.Controls.Add(this.label8);
            this.groupBox2.Location = new System.Drawing.Point(16, 155);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox2.Size = new System.Drawing.Size(260, 123);
            this.groupBox2.TabIndex = 22;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Wheels";
            // 
            // btnResetEncoders
            // 
            this.btnResetEncoders.Location = new System.Drawing.Point(9, 25);
            this.btnResetEncoders.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnResetEncoders.Name = "btnResetEncoders";
            this.btnResetEncoders.Size = new System.Drawing.Size(132, 36);
            this.btnResetEncoders.TabIndex = 31;
            this.btnResetEncoders.Text = "Reset Encoders";
            this.btnResetEncoders.UseVisualStyleBackColor = true;
            this.btnResetEncoders.Click += new System.EventHandler(this.BtnResetEncodersClick);
            // 
            // RightEncoderTicks
            // 
            this.RightEncoderTicks.Location = new System.Drawing.Point(192, 94);
            this.RightEncoderTicks.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.RightEncoderTicks.Name = "RightEncoderTicks";
            this.RightEncoderTicks.Size = new System.Drawing.Size(57, 16);
            this.RightEncoderTicks.TabIndex = 30;
            this.RightEncoderTicks.Text = "0";
            this.RightEncoderTicks.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // LeftEncoderTicks
            // 
            this.LeftEncoderTicks.Location = new System.Drawing.Point(125, 94);
            this.LeftEncoderTicks.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.LeftEncoderTicks.Name = "LeftEncoderTicks";
            this.LeftEncoderTicks.Size = new System.Drawing.Size(67, 16);
            this.LeftEncoderTicks.TabIndex = 30;
            this.LeftEncoderTicks.Text = "0";
            this.LeftEncoderTicks.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(15, 69);
            this.label13.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(85, 17);
            this.label13.TabIndex = 3;
            this.label13.Text = "Speed (m/s)";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(15, 94);
            this.label11.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(98, 17);
            this.label11.TabIndex = 2;
            this.label11.Text = "Encoder Ticks";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(209, 34);
            this.label10.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(41, 17);
            this.label10.TabIndex = 1;
            this.label10.Text = "Right";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(159, 34);
            this.label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(32, 17);
            this.label8.TabIndex = 0;
            this.label8.Text = "Left";
            // 
            // DashboardForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(572, 569);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.SensorsGroupBox);
            this.Controls.Add(this.TiltGroupBox);
            this.Controls.Add(this.HeadingLabel);
            this.Controls.Add(this.DiveCommandsGroupBox);
            this.Controls.Add(this.DirectDriveGroupBox);
            this.Controls.Add(this.MainMenu);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MinimumSize = new System.Drawing.Size(581, 519);
            this.Name = "DashboardForm";
            this.Text = "Robot Dashboard";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.DashboardFormFormClosed);
            this.Load += new System.EventHandler(this.DashboardFormLoad);
            this.DirectDriveGroupBox.ResumeLayout(false);
            this.DirectDriveGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.JoystickPicture)).EndInit();
            this.DiveCommandsGroupBox.ResumeLayout(false);
            this.MainMenu.ResumeLayout(false);
            this.MainMenu.PerformLayout();
            this.TiltGroupBox.ResumeLayout(false);
            this.TiltGroupBox.PerformLayout();
            this.SensorsGroupBox.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox JoystickComboBox;
        private System.Windows.Forms.Label lblX;
        private System.Windows.Forms.Label lblY;
        private System.Windows.Forms.Label lblZ;
        private System.Windows.Forms.Label lblButtons;
        private System.Windows.Forms.GroupBox DirectDriveGroupBox;
        private System.Windows.Forms.PictureBox JoystickPicture;
        private System.Windows.Forms.Label DeviceLabel;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.GroupBox DiveCommandsGroupBox;
        private System.Windows.Forms.SaveFileDialog saveFileDialog;
        private System.Windows.Forms.MenuStrip MainMenu;
        private System.Windows.Forms.ToolStripMenuItem FileMenu;
        private System.Windows.Forms.ToolStripMenuItem saveSettingsMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ToolsMenu;
        private System.Windows.Forms.ToolStripMenuItem optionsMenuItem;
        private System.Windows.Forms.ToolStripMenuItem HelpMenu;
        private System.Windows.Forms.ToolStripMenuItem aboutMenuItem;
        private System.Windows.Forms.Label HeadingLabel;
        private System.Windows.Forms.GroupBox TiltGroupBox;
        private System.Windows.Forms.Button SetTiltButton;
        private System.Windows.Forms.Label TiltLabel;
        private System.Windows.Forms.GroupBox SensorsGroupBox;
        private System.Windows.Forms.Label SonarRightLabel;
        private System.Windows.Forms.Label SonarLeftLabel;
        private System.Windows.Forms.Label IRRightLabel;
        private System.Windows.Forms.Label IRCenterLabel;
        private System.Windows.Forms.Label IRLeftLabel;
        private System.Windows.Forms.CheckBox StopCheckBox;
        private System.Windows.Forms.CheckBox DriveCheckBox;
        private System.Windows.Forms.Button TiltResetButton;
        private System.Windows.Forms.Button TiltDownButton;
        private System.Windows.Forms.Button TiltUpButton;
        private System.Windows.Forms.Label label1;
        internal System.Windows.Forms.TextBox TiltTextbox;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ProgressBar BatteryLevel;
        private System.Windows.Forms.Label BatteryLevelTxt;
        private System.Windows.Forms.Label RightWheelSpeed;
        private System.Windows.Forms.Label LeftWheelSpeed;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label RightEncoderTicks;
        private System.Windows.Forms.Label LeftEncoderTicks;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Button btnResetEncoders;
    }
}