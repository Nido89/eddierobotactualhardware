namespace ProMRDS.Simulation.JointMover
{
    partial class SimulatedBipedMoverUI
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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this._entityGroup = new System.Windows.Forms.GroupBox();
            this._entityNameComboBox = new System.Windows.Forms.ComboBox();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this._entityGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this._entityGroup);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.AutoScroll = true;
            this.splitContainer1.Size = new System.Drawing.Size(1159, 632);
            this.splitContainer1.SplitterDistance = 143;
            this.splitContainer1.TabIndex = 2;
            // 
            // _entityGroup
            // 
            this._entityGroup.Controls.Add(this._entityNameComboBox);
            this._entityGroup.Location = new System.Drawing.Point(12, 12);
            this._entityGroup.Name = "_entityGroup";
            this._entityGroup.Size = new System.Drawing.Size(124, 55);
            this._entityGroup.TabIndex = 4;
            this._entityGroup.TabStop = false;
            this._entityGroup.Text = "Entity Name";
            // 
            // _entityNameComboBox
            // 
            this._entityNameComboBox.FormattingEnabled = true;
            this._entityNameComboBox.Location = new System.Drawing.Point(3, 19);
            this._entityNameComboBox.Name = "_entityNameComboBox";
            this._entityNameComboBox.Size = new System.Drawing.Size(115, 21);
            this._entityNameComboBox.TabIndex = 6;
            this._entityNameComboBox.Text = "Select an Entity";
            this._entityNameComboBox.MouseClick += new System.Windows.Forms.MouseEventHandler(this._entityNameComboBox_MouseClick);
            this._entityNameComboBox.SelectionChangeCommitted += new System.EventHandler(this._entityNameComboBox_SelectionChangeCommitted);
            this._entityNameComboBox.SelectedIndexChanged += new System.EventHandler(this._entityNameComboBox_SelectedIndexChanged);
            this._entityNameComboBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this._entityNameComboBox_KeyPress);
            this._entityNameComboBox.SelectedValueChanged += new System.EventHandler(this._entityNameComboBox_SelectedValueChanged);
            // 
            // SimulatedBipedMoverUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1159, 632);
            this.Controls.Add(this.splitContainer1);
            this.Name = "SimulatedBipedMoverUI";
            this.Text = "Joint Mover";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this._entityGroup.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.GroupBox _entityGroup;
        private System.Windows.Forms.ComboBox _entityNameComboBox;

    }
}