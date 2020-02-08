//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: FlexDialogForm.cs $ $Revision: 5 $
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Microsoft.Robotics.Services.Sample.FlexDialog
{
    /// <summary>
    /// FlexDialogForm - Form used by Flexible Dialog service
    /// </summary>
    public partial class FlexDialogForm : Form
    {
#if !URT_MINCLR
        FlexDialogOperations _mainPort;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mainPort"></param>
        public FlexDialogForm(FlexDialogOperations mainPort)
        {
            _mainPort = mainPort;

            InitializeComponent();
            Visible = false;
        }

        const int _margin = 6;
        const int _buttonWidth = 75;
        const int _buttonHeight = 23;

        #region Service Interfaces

        /// <summary>
        /// Initialize - Init the Dialog
        /// </summary>
        /// <param name="state"></param>
        public void Initialize(FlexDialogState state)
        {
            SetTitle(state.Title);
            SetVisibility(state.Visible);

            SuspendLayout();

            state.Controls.ForEach(InsertControl);
            state.Buttons.ForEach(InsertButton);

            if (controlPanel.HasChildren)
            {
                int bottom = controlPanel.Controls[controlPanel.Controls.Count - 1].Bottom;
                int width = ClientSize.Width;

                if (buttonPanel.HasChildren &&
                    buttonPanel.Controls[0].Left < _margin)
                {
                    width += _margin - buttonPanel.Controls[0].Left;
                }
                ClientSize = new Size(width, bottom + _margin + buttonPanel.Height);
            }
            ResumeLayout();
        }

        /// <summary>
        /// InsertButton - Add a new button to the dialog
        /// </summary>
        /// <param name="button"></param>
        public void InsertButton(FlexButton button)
        {
            Button formButton = new Button();

            int offset = _buttonWidth + _margin;

            formButton.Name = button.Id;
            formButton.Text = button.Text;
            formButton.Size = new Size(_buttonWidth, _buttonHeight);
            formButton.Location = new Point(
                buttonPanel.ClientSize.Width - (_buttonWidth + _margin),
                _margin
            );
            formButton.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            formButton.MouseDown += formButton_MouseDown;
            formButton.MouseUp += formButton_MouseUp;

            //
            // move all existing buttons left
            //
            foreach (Control control in buttonPanel.Controls)
            {
                control.Location = new Point(
                    control.Location.X - offset,
                    control.Location.Y
                );
            }

            buttonPanel.Controls.Add(formButton);
        }

        /// <summary>
        /// DeleteButton - Remove a button from the dialog
        /// </summary>
        /// <param name="button"></param>
        public void DeleteButton(FlexButton button)
        {
            buttonPanel.Controls.RemoveByKey(button.Id);

            if (buttonPanel.HasChildren)
            {
                int offset = _margin + _buttonWidth;
                int left = buttonPanel.ClientSize.Width - buttonPanel.Controls.Count * (offset);

                for (int i = 0; i < buttonPanel.Controls.Count; i++)
                {
                    buttonPanel.Controls[i].Left = left;
                    left += offset;
                }
            }
        }

        /// <summary>
        /// UpdateButton - Change the text on a button
        /// </summary>
        /// <param name="button"></param>
        public void UpdateButton(FlexButton button)
        {
            Control[] controls = buttonPanel.Controls.Find(button.Id, true);
            if (controls == null ||
                controls.Length != 1)
            {
                throw new ApplicationException("Unable to find the button to update");
            }
            Control formControl = controls[0];

            formControl.Text = button.Text;
            formControl.Tag = button;
        }

        /// <summary>
        /// InsertControl - Add a new control to the dialog
        /// </summary>
        /// <param name="control"></param>
        public void InsertControl(FlexControl control)
        {
            Control formControl = null;
            int height = _buttonHeight;
            int width = controlPanel.ClientSize.Width - _margin * 2;
            int top;
            AnchorStyles anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;

            if (controlPanel.Controls.Count > 0)
            {
                top = controlPanel.Controls[controlPanel.Controls.Count - 1].Bottom + _margin;
            }
            else
            {
                top = _margin;
            }

            switch (control.ControlType)
            {
                case FlexControlType.Label:
                    formControl = new Label();
                    formControl.Text = control.Text;
                    using (Graphics g = CreateGraphics())
                    {
                        SizeF size = g.MeasureString(control.Text, Font);
                        height = (int)System.Math.Ceiling(size.Height);
                    }
                    break;

                case FlexControlType.TextBox:
                    TextBox textbox = new TextBox();
                    textbox.Multiline = false;
                    textbox.Text = control.Value;
                    textbox.TextChanged += textbox_TextChanged;
                    formControl = textbox;
                    break;

                case FlexControlType.MultiLineTextBox:
                    TextBox multi = new TextBox();
                    multi.Multiline = true;
                    multi.ScrollBars = ScrollBars.Vertical;
                    multi.AcceptsReturn = true;
                    multi.Text = control.Value;
                    multi.TextChanged += textbox_TextChanged;
                    formControl = multi;
                    height *= 5;
                    break;

                case FlexControlType.Button:
                    Button button = new Button();
                    button.MouseDown += formButton_MouseDown;
                    button.MouseUp += formButton_MouseUp;
                    button.Text = control.Text;
                    width = 75;
                    using (Graphics g = CreateGraphics())
                    {
                        SizeF size = g.MeasureString(button.Text, Font);
                        if (size.Width + 2 * _margin > width)
                        {
                            width = (int)(size.Width + 2 * _margin);
                        }
                    }
                    anchor = AnchorStyles.Top | AnchorStyles.Left;
                    formControl = button;
                    break;

                case FlexControlType.CheckBox:
                    CheckBox checkbox = new CheckBox();
                    checkbox.Text = control.Text;
                    checkbox.Checked = bool.Parse(control.Value);
                    checkbox.CheckedChanged += checkbox_CheckedChanged;
                    formControl = checkbox;
                    break;

                case FlexControlType.RadioButton:
                    RadioButton radio = new RadioButton();
                    radio.Text = control.Text;
                    radio.Checked = bool.Parse(control.Value);
                    radio.CheckedChanged += radio_CheckedChanged;
                    formControl = radio;
                    break;

                case FlexControlType.ComboBox:
                    ComboBox combo = new ComboBox();
                    combo.DropDownStyle = ComboBoxStyle.DropDownList;
                    string[] items = control.Text.Split('|');
                    combo.MaxDropDownItems = items.Length;
                    combo.Items.AddRange(items);
                    if (!string.IsNullOrEmpty(control.Value))
                    {
                        combo.Text = control.Value;
                    }
                    combo.SelectedValueChanged += combo_SelectedValueChanged;
                    width = 0;
                    using (Graphics g = CreateGraphics())
                    {
                        foreach (string item in items)
                        {
                            SizeF size = g.MeasureString(item, Font);
                            width = System.Math.Max(width,
                                (int)(System.Math.Ceiling(size.Width) + 4 * _margin)
                            );
                        }
                    }
                    anchor = AnchorStyles.Top | AnchorStyles.Left;
                    formControl = combo;
                    break;

                default:
                case FlexControlType.Seperator:
                    Label label = new Label();
                    label.BorderStyle = BorderStyle.Fixed3D;
                    height = 2;
                    formControl = label;
                    break;
            }
            formControl.Name = control.Id;
            formControl.Location = new Point(_margin, top);
            formControl.Size = new Size(width, height);
            formControl.Anchor = anchor;
            formControl.Tag = control;

            controlPanel.Controls.Add(formControl);
        }

        /// <summary>
        /// DeleteControl - Remove a control from the dialog
        /// </summary>
        /// <param name="control"></param>
        public void DeleteControl(FlexControl control)
        {
            controlPanel.Controls.RemoveByKey(control.Id);

            int top = _margin;
            foreach (Control formControl in controlPanel.Controls)
            {
                formControl.Top = top;
                top = formControl.Bottom + _margin;
            }
        }

        /// <summary>
        /// UpdateControl - Replace the text on a control (depends on control type)
        /// </summary>
        /// <param name="control"></param>
        public void UpdateControl(FlexControl control)
        {
            Control[] controls = controlPanel.Controls.Find(control.Id, true);
            if (controls == null ||
                controls.Length != 1)
            {
                throw new ApplicationException("Could not find one control with the requested id: " + control.Id);
            }
            Control formControl = controls[0];
            FlexControl previous = formControl.Tag as FlexControl;
            if (previous == null)
            {
                throw new ApplicationException("Control does not have appropriate Tag");
            }

            if (previous.ControlType != control.ControlType)
            {
                throw new ApplicationException("Unable to convert control to a different type");
            }

            switch (control.ControlType)
            {
                case FlexControlType.Label:
                    formControl.Text = control.Text;
                    break;

                case FlexControlType.TextBox:
                    formControl.Text = control.Value;
                    break;

                case FlexControlType.MultiLineTextBox:
                    formControl.Text = control.Value;
                    break;

                case FlexControlType.Button:
                    formControl.Text = control.Text;
                    using (Graphics g = CreateGraphics())
                    {
                        SizeF size = g.MeasureString(control.Text, Font);
                        formControl.Width = System.Math.Max(
                            _buttonWidth,
                            (int)System.Math.Ceiling(size.Width) + 2 * _margin
                        );
                    }
                    break;

                case FlexControlType.CheckBox:
                    formControl.Text = control.Text;
                    CheckBox check = formControl as CheckBox;
                    check.Checked = bool.Parse(control.Value);
                    break;

                case FlexControlType.RadioButton:
                    formControl.Text = control.Text;
                    RadioButton radio = formControl as RadioButton;
                    radio.Checked = bool.Parse(control.Value);
                    break;

                case FlexControlType.ComboBox:
                    string[] items = control.Text.Split('|');
                    ComboBox combo = formControl as ComboBox;
                    combo.Items.Clear();
                    combo.Items.AddRange(items);
                    if (string.IsNullOrEmpty(control.Value))
                    {
                        combo.SelectedIndex = -1;
                    }
                    else
                    {
                        combo.Text = control.Value;
                    }
                    int width = 0;
                    using (Graphics g = CreateGraphics())
                    {
                        foreach (string item in items)
                        {
                            SizeF size = g.MeasureString(item, Font);
                            width = System.Math.Max(width,
                                (int)(System.Math.Ceiling(size.Width) + 4 * _margin)
                            );
                        }
                    }
                    formControl.Width = width;
                    break;

                default:
                case FlexControlType.Seperator:
                    break;
            }

            formControl.Tag = control;
        }


        /// <summary>
        /// SetVisibility - Show or hide the dialog
        /// </summary>
        /// <param name="visibility"></param>
        public void SetVisibility(bool visibility)
        {
            if (Visible != visibility)
            {
                if (visibility)
                {
                    Show();
                }
                else
                {
                    Hide();
                }
            }
        }

        /// <summary>
        /// SetTitle - Set the dialog's title
        /// </summary>
        /// <param name="title"></param>
        public void SetTitle(string title)
        {
            if (string.IsNullOrEmpty(title))
            {
                Text = "DSS Dialog";
            }
            else
            {
                Text = title;
            }
        }

        #endregion

        #region Event Handlers

        void formButton_MouseUp(object sender, MouseEventArgs e)
        {
            SendButtonPress(sender, false);
        }

        void formButton_MouseDown(object sender, MouseEventArgs e)
        {
            SendButtonPress(sender, true);
        }

        private void SendButtonPress(object sender, bool pressed)
        {
            Button button = sender as Button;
            if (button == null)
            {
                return;
            }

            ButtonPress press = new ButtonPress();
            press.Body.Id = button.Name;
            press.Body.Pressed = pressed;
            _mainPort.Post(press);
        }

        void combo_SelectedValueChanged(object sender, EventArgs e)
        {
            ComboBox combo = sender as ComboBox;
            if (combo == null)
            {
                return;
            }
            FlexControl control = combo.Tag as FlexControl;
            if (control == null)
            {
                return;
            }
            if (combo.SelectedIndex < 0)
            {
                control.Value = null;
            }
            else
            {
                control.Value = combo.Items[combo.SelectedIndex].ToString();
            }
            _mainPort.Post(new UpdateControl(control));
        }

        void textbox_TextChanged(object sender, EventArgs e)
        {
            Control formControl = sender as Control;
            if (formControl == null)
            {
                return;
            }

            FlexControl control = formControl.Tag as FlexControl;
            if (control == null)
            {
                return;
            }

            control.Value = formControl.Text;
            _mainPort.Post(new UpdateControl(control));
        }

        void radio_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton radio = sender as RadioButton;
            if (radio == null)
            {
                return;
            }

            FlexControl control = radio.Tag as FlexControl;
            if (control == null)
            {
                return;
            }

            control.Value = radio.Checked.ToString();
            _mainPort.Post(new UpdateControl(control));
        }

        void checkbox_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox checkbox = sender as CheckBox;
            if (checkbox == null)
            {
                return;
            }

            FlexControl control = checkbox.Tag as FlexControl;
            if (control == null)
            {
                return;
            }

            control.Value = checkbox.Checked.ToString();
            _mainPort.Post(new UpdateControl(control));
        }

        /// <summary>
        /// OnClosing - Signal to the main service that the dialog is being closed
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            _mainPort.Post(new Show(new ShowRequest(false)));
            Hide();
            base.OnClosing(e);
        }

        #endregion
#else
        public FlexDialogForm()
        {
            InitializeComponent();
        }
#endif
    }
}
