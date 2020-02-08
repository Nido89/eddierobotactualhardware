//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: XBoxCtrlGraphic.xaml.cs $ $Revision: 4 $
//-----------------------------------------------------------------------
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Xml.Linq;
using System.Linq;
using System.Collections.Generic;

namespace XBoxCtrlViewer
{
	public partial class XBoxCtrlGraphic : UserControl
	{
        public static string XInputContract = "http://schemas.microsoft.com/robotics/2006/09/xinputgamepad.user.html";
        public static XNamespace XinputNS = XInputContract;

		public XBoxCtrlGraphic()
		{
			InitializeComponent();

            rightThumbstick = new ThumbStick("Right", RightStickHat, RightStickLeftPosition, RightStickTopPosition, RightStickMove);
            leftThumbstick = new ThumbStick("Left", LeftStickHat, LeftStickLeftPosition, LeftStickTopPosition, LeftStickMove);
        }

        public void ProcessXInputGamepadState(XElement root)
        {
            try
            {
                ProcessDpad(root.Element(XinputNS + "DPad"));
                ProcessButtons(root.Element(XinputNS + "Buttons"));
                rightThumbstick.ProcessThumbstick(root.Element(XinputNS + "Thumbsticks"));
                leftThumbstick.ProcessThumbstick(root.Element(XinputNS + "Thumbsticks"));
                ProcessControllerInfo(root.Element(XinputNS + "Controller"));
            }
            catch (Exception) { }
        }

        // dpad
        private bool dpadLeftPressed = false;
        private bool dpadRightPressed = false;
        private bool dpadTopPressed = false;
        private bool dpadBottomPressed = false;

        public void ProcessDpad(XElement item)
        {
            CheckXElementBoolValue(item.Element(XinputNS + "Left"), ref dpadLeftPressed, PadLeftDown, PadLeftUp);
            CheckXElementBoolValue(item.Element(XinputNS + "Right"), ref dpadRightPressed, PadRightDown, PadRightUp);
            CheckXElementBoolValue(item.Element(XinputNS + "Up"), ref dpadTopPressed, PadUpDown, PadUpUp);
            CheckXElementBoolValue(item.Element(XinputNS + "Down"), ref dpadBottomPressed, PadDownDown, PadDownUp);
        }

        // buttons
        private bool A_Pressed = false;
        private bool B_Pressed = false;
        private bool X_Pressed = false;
        private bool Y_Pressed = false;

        private bool back_Pressed = false;
        private bool start_Pressed = false;
        private bool leftStick_Pressed = false;
        private bool rightStick_Pressed = false;

        private bool leftShoulderPressed = false;
        private bool rightShoulderPressed = false;

        public void ProcessButtons(XElement item)
        {
            CheckXElementBoolValue(item.Element(XinputNS + "A"), ref A_Pressed, A_letter_down, A_letter_up);
            CheckXElementBoolValue(item.Element(XinputNS + "B"), ref B_Pressed, B_letter_down, B_letter_up);
            CheckXElementBoolValue(item.Element(XinputNS + "X"), ref X_Pressed, X_letter_down, X_letter_up);
            CheckXElementBoolValue(item.Element(XinputNS + "Y"), ref Y_Pressed, Y_letter_down, Y_letter_up);

            CheckXElementBoolValue(item.Element(XinputNS + "Start"), ref start_Pressed, StartButtonDown, StartButtonUp);
            CheckXElementBoolValue(item.Element(XinputNS + "Back"), ref back_Pressed, BackButtonDown, BackButtonUp);
            CheckXElementBoolValue(item.Element(XinputNS + "LeftStick"), ref leftStick_Pressed, LeftStickDown, LeftStickUp);
            CheckXElementBoolValue(item.Element(XinputNS + "RightStick"), ref rightStick_Pressed, RightStickDown, RightStickUp);

            CheckXElementBoolValue(item.Element(XinputNS + "LeftShoulder"), ref leftShoulderPressed, LeftShoulderDown, LeftShoulderUp);
            CheckXElementBoolValue(item.Element(XinputNS + "RightShoulder"), ref rightShoulderPressed, RightShoulderDown, RightShoulderUp);
        }

        // thumbstics
        ThumbStick rightThumbstick;
        ThumbStick leftThumbstick;

        // controller info
        private string controllerIndex = "";

        public void ProcessControllerInfo(XElement item)
        {
            string index = (string)item.Element(XinputNS + "PlayerIndex");
            if (controllerIndex != index)
            {
                controllerIndex = index;
                X_one.Visibility = Visibility.Collapsed;
                X_two.Visibility = Visibility.Collapsed;
                X_three.Visibility = Visibility.Collapsed;
                X_four.Visibility = Visibility.Collapsed;

                switch (index)
                {
                    case "One":
                        X_one.Visibility = Visibility.Visible;
                        break;
                    case "Two":
                        X_two.Visibility = Visibility.Visible;
                        break;
                    case "Three":
                        X_three.Visibility = Visibility.Visible;
                        break;
                    case "Four":
                        X_four.Visibility = Visibility.Visible;
                        break;
                }
            }
        }

        void CheckXElementBoolValue(XElement item, ref bool internalValue, Storyboard animationTrue, Storyboard animationFalse)
        {
            bool value = (bool)item;
            if (internalValue != value)
            {
                internalValue = value;
                if (value)
                {
                    animationTrue.Begin();
                }
                else
                {
                    animationFalse.Begin();
                }
            }
        }

        public class ThumbStick
        {
            double positionInitialLeft;
            double positionInitialTop;
            double positionCurrentLeft;
            double positionCurrentTop;
            string name;
            DoubleAnimationUsingKeyFrames animationLeft;
            DoubleAnimationUsingKeyFrames animationTop;
            Storyboard storyboard;

            public ThumbStick(string Name,
                Canvas VisualItem,
                DoubleAnimationUsingKeyFrames AnimationLeft,
                DoubleAnimationUsingKeyFrames AnimationTop,
                Storyboard StoryboardAnimation)
            {
                this.name = Name;
                this.animationLeft = AnimationLeft;
                this.animationTop = AnimationTop;
                this.storyboard = StoryboardAnimation;

                positionInitialLeft = (double)VisualItem.GetValue(Canvas.LeftProperty);
                positionInitialTop = (double)VisualItem.GetValue(Canvas.TopProperty);
                positionCurrentLeft = positionInitialLeft;
                positionCurrentTop = positionInitialTop;
            }
            public void ProcessThumbstick(XElement item)
            {
                double X = (double)item.Element(XinputNS + name + "X");
                double Y = -(double)item.Element(XinputNS + name + "Y");

                X = (X * 15) + positionInitialLeft;
                Y = (Y * 15) + positionInitialTop;

                if (X != positionCurrentLeft || Y != positionCurrentTop)
                {
                    animationLeft.KeyFrames[0].Value = positionCurrentLeft;
                    animationLeft.KeyFrames[1].Value = X;
                    animationTop.KeyFrames[0].Value = positionCurrentTop;
                    animationTop.KeyFrames[1].Value = Y;
                    storyboard.Begin();
                    positionCurrentLeft = X;
                    positionCurrentTop = Y;
                }
            }
        }

	}
}