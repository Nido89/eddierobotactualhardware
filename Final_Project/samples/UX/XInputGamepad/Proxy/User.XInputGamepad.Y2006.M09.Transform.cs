//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.296
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

[assembly: global::System.Reflection.AssemblyVersionAttribute("0.0.0.0")]
[assembly: global::Microsoft.Dss.Core.Attributes.ServiceDeclarationAttribute(global::Microsoft.Dss.Core.Attributes.DssServiceDeclaration.Transform, SourceAssemblyKey="User.XInputGamepad.Y2006.M09, Version=0.0.0.0, Culture=neutral, PublicKeyToken=7f" +
    "9074033fd3dcf7")]
[assembly: global::System.Security.SecurityTransparentAttribute()]
[assembly: global::System.Security.SecurityRulesAttribute(global::System.Security.SecurityRuleSet.Level1)]

namespace Dss.Transforms.TransformUser {
    
    
    public class Transforms : global::Microsoft.Dss.Core.Transforms.TransformBase {
        
        static Transforms() {
            Register();
        }
        
        public static void Register() {
            global::Microsoft.Dss.Core.Transforms.TransformBase.AddProxyTransform(typeof(global::Microsoft.Robotics.Services.Sample.XInputGamepad.Proxy.XInputGamepadState), new global::Microsoft.Dss.Core.Attributes.Transform(Microsoft_Robotics_Services_Sample_XInputGamepad_Proxy_XInputGamepadState_TO_Microsoft_Robotics_Services_Sample_XInputGamepad_XInputGamepadState));
            global::Microsoft.Dss.Core.Transforms.TransformBase.AddSourceTransform(typeof(global::Microsoft.Robotics.Services.Sample.XInputGamepad.XInputGamepadState), new global::Microsoft.Dss.Core.Attributes.Transform(Microsoft_Robotics_Services_Sample_XInputGamepad_XInputGamepadState_TO_Microsoft_Robotics_Services_Sample_XInputGamepad_Proxy_XInputGamepadState));
            global::Microsoft.Dss.Core.Transforms.TransformBase.AddProxyTransform(typeof(global::Microsoft.Robotics.Services.Sample.XInputGamepad.Proxy.Controller), new global::Microsoft.Dss.Core.Attributes.Transform(Microsoft_Robotics_Services_Sample_XInputGamepad_Proxy_Controller_TO_Microsoft_Robotics_Services_Sample_XInputGamepad_Controller));
            global::Microsoft.Dss.Core.Transforms.TransformBase.AddSourceTransform(typeof(global::Microsoft.Robotics.Services.Sample.XInputGamepad.Controller), new global::Microsoft.Dss.Core.Attributes.Transform(Microsoft_Robotics_Services_Sample_XInputGamepad_Controller_TO_Microsoft_Robotics_Services_Sample_XInputGamepad_Proxy_Controller));
            global::Microsoft.Dss.Core.Transforms.TransformBase.AddProxyTransform(typeof(global::Microsoft.Robotics.Services.Sample.XInputGamepad.Proxy.DPad), new global::Microsoft.Dss.Core.Attributes.Transform(Microsoft_Robotics_Services_Sample_XInputGamepad_Proxy_DPad_TO_Microsoft_Robotics_Services_Sample_XInputGamepad_DPad));
            global::Microsoft.Dss.Core.Transforms.TransformBase.AddSourceTransform(typeof(global::Microsoft.Robotics.Services.Sample.XInputGamepad.DPad), new global::Microsoft.Dss.Core.Attributes.Transform(Microsoft_Robotics_Services_Sample_XInputGamepad_DPad_TO_Microsoft_Robotics_Services_Sample_XInputGamepad_Proxy_DPad));
            global::Microsoft.Dss.Core.Transforms.TransformBase.AddProxyTransform(typeof(global::Microsoft.Robotics.Services.Sample.XInputGamepad.Proxy.Buttons), new global::Microsoft.Dss.Core.Attributes.Transform(Microsoft_Robotics_Services_Sample_XInputGamepad_Proxy_Buttons_TO_Microsoft_Robotics_Services_Sample_XInputGamepad_Buttons));
            global::Microsoft.Dss.Core.Transforms.TransformBase.AddSourceTransform(typeof(global::Microsoft.Robotics.Services.Sample.XInputGamepad.Buttons), new global::Microsoft.Dss.Core.Attributes.Transform(Microsoft_Robotics_Services_Sample_XInputGamepad_Buttons_TO_Microsoft_Robotics_Services_Sample_XInputGamepad_Proxy_Buttons));
            global::Microsoft.Dss.Core.Transforms.TransformBase.AddProxyTransform(typeof(global::Microsoft.Robotics.Services.Sample.XInputGamepad.Proxy.Triggers), new global::Microsoft.Dss.Core.Attributes.Transform(Microsoft_Robotics_Services_Sample_XInputGamepad_Proxy_Triggers_TO_Microsoft_Robotics_Services_Sample_XInputGamepad_Triggers));
            global::Microsoft.Dss.Core.Transforms.TransformBase.AddSourceTransform(typeof(global::Microsoft.Robotics.Services.Sample.XInputGamepad.Triggers), new global::Microsoft.Dss.Core.Attributes.Transform(Microsoft_Robotics_Services_Sample_XInputGamepad_Triggers_TO_Microsoft_Robotics_Services_Sample_XInputGamepad_Proxy_Triggers));
            global::Microsoft.Dss.Core.Transforms.TransformBase.AddProxyTransform(typeof(global::Microsoft.Robotics.Services.Sample.XInputGamepad.Proxy.Thumbsticks), new global::Microsoft.Dss.Core.Attributes.Transform(Microsoft_Robotics_Services_Sample_XInputGamepad_Proxy_Thumbsticks_TO_Microsoft_Robotics_Services_Sample_XInputGamepad_Thumbsticks));
            global::Microsoft.Dss.Core.Transforms.TransformBase.AddSourceTransform(typeof(global::Microsoft.Robotics.Services.Sample.XInputGamepad.Thumbsticks), new global::Microsoft.Dss.Core.Attributes.Transform(Microsoft_Robotics_Services_Sample_XInputGamepad_Thumbsticks_TO_Microsoft_Robotics_Services_Sample_XInputGamepad_Proxy_Thumbsticks));
            global::Microsoft.Dss.Core.Transforms.TransformBase.AddProxyTransform(typeof(global::Microsoft.Robotics.Services.Sample.XInputGamepad.Proxy.Vibration), new global::Microsoft.Dss.Core.Attributes.Transform(Microsoft_Robotics_Services_Sample_XInputGamepad_Proxy_Vibration_TO_Microsoft_Robotics_Services_Sample_XInputGamepad_Vibration));
            global::Microsoft.Dss.Core.Transforms.TransformBase.AddSourceTransform(typeof(global::Microsoft.Robotics.Services.Sample.XInputGamepad.Vibration), new global::Microsoft.Dss.Core.Attributes.Transform(Microsoft_Robotics_Services_Sample_XInputGamepad_Vibration_TO_Microsoft_Robotics_Services_Sample_XInputGamepad_Proxy_Vibration));
            global::Microsoft.Dss.Core.Transforms.TransformBase.AddProxyTransform(typeof(global::Microsoft.Robotics.Services.Sample.XInputGamepad.Proxy.PollRequest), new global::Microsoft.Dss.Core.Attributes.Transform(Microsoft_Robotics_Services_Sample_XInputGamepad_Proxy_PollRequest_TO_Microsoft_Robotics_Services_Sample_XInputGamepad_PollRequest));
            global::Microsoft.Dss.Core.Transforms.TransformBase.AddSourceTransform(typeof(global::Microsoft.Robotics.Services.Sample.XInputGamepad.PollRequest), new global::Microsoft.Dss.Core.Attributes.Transform(Microsoft_Robotics_Services_Sample_XInputGamepad_PollRequest_TO_Microsoft_Robotics_Services_Sample_XInputGamepad_Proxy_PollRequest));
            global::Microsoft.Dss.Core.Transforms.TransformBase.AddProxyTransform(typeof(global::Microsoft.Robotics.Services.Sample.XInputGamepad.Proxy.QueryControllersRequest), new global::Microsoft.Dss.Core.Attributes.Transform(Microsoft_Robotics_Services_Sample_XInputGamepad_Proxy_QueryControllersRequest_TO_Microsoft_Robotics_Services_Sample_XInputGamepad_QueryControllersRequest));
            global::Microsoft.Dss.Core.Transforms.TransformBase.AddSourceTransform(typeof(global::Microsoft.Robotics.Services.Sample.XInputGamepad.QueryControllersRequest), new global::Microsoft.Dss.Core.Attributes.Transform(Microsoft_Robotics_Services_Sample_XInputGamepad_QueryControllersRequest_TO_Microsoft_Robotics_Services_Sample_XInputGamepad_Proxy_QueryControllersRequest));
            global::Microsoft.Dss.Core.Transforms.TransformBase.AddProxyTransform(typeof(global::Microsoft.Robotics.Services.Sample.XInputGamepad.Proxy.QueryControllersResponse), new global::Microsoft.Dss.Core.Attributes.Transform(Microsoft_Robotics_Services_Sample_XInputGamepad_Proxy_QueryControllersResponse_TO_Microsoft_Robotics_Services_Sample_XInputGamepad_QueryControllersResponse));
            global::Microsoft.Dss.Core.Transforms.TransformBase.AddSourceTransform(typeof(global::Microsoft.Robotics.Services.Sample.XInputGamepad.QueryControllersResponse), new global::Microsoft.Dss.Core.Attributes.Transform(Microsoft_Robotics_Services_Sample_XInputGamepad_QueryControllersResponse_TO_Microsoft_Robotics_Services_Sample_XInputGamepad_Proxy_QueryControllersResponse));
            global::Microsoft.Dss.Core.Transforms.TransformBase.AddProxyTransform(typeof(global::Microsoft.Robotics.Services.Sample.XInputGamepad.Proxy.ControllerCaps), new global::Microsoft.Dss.Core.Attributes.Transform(Microsoft_Robotics_Services_Sample_XInputGamepad_Proxy_ControllerCaps_TO_Microsoft_Robotics_Services_Sample_XInputGamepad_ControllerCaps));
            global::Microsoft.Dss.Core.Transforms.TransformBase.AddSourceTransform(typeof(global::Microsoft.Robotics.Services.Sample.XInputGamepad.ControllerCaps), new global::Microsoft.Dss.Core.Attributes.Transform(Microsoft_Robotics_Services_Sample_XInputGamepad_ControllerCaps_TO_Microsoft_Robotics_Services_Sample_XInputGamepad_Proxy_ControllerCaps));
        }
        
        public static object Microsoft_Robotics_Services_Sample_XInputGamepad_Proxy_XInputGamepadState_TO_Microsoft_Robotics_Services_Sample_XInputGamepad_XInputGamepadState(object transformFrom) {
            global::Microsoft.Robotics.Services.Sample.XInputGamepad.XInputGamepadState target = new global::Microsoft.Robotics.Services.Sample.XInputGamepad.XInputGamepadState();
            global::Microsoft.Robotics.Services.Sample.XInputGamepad.Proxy.XInputGamepadState from = ((global::Microsoft.Robotics.Services.Sample.XInputGamepad.Proxy.XInputGamepadState)(transformFrom));
            target.TimeStamp = from.TimeStamp;
            if ((from.Controller != null)) {
                target.Controller = ((global::Microsoft.Robotics.Services.Sample.XInputGamepad.Controller)(Microsoft_Robotics_Services_Sample_XInputGamepad_Proxy_Controller_TO_Microsoft_Robotics_Services_Sample_XInputGamepad_Controller(from.Controller)));
            }
            else {
                target.Controller = null;
            }
            if ((from.DPad != null)) {
                target.DPad = ((global::Microsoft.Robotics.Services.Sample.XInputGamepad.DPad)(Microsoft_Robotics_Services_Sample_XInputGamepad_Proxy_DPad_TO_Microsoft_Robotics_Services_Sample_XInputGamepad_DPad(from.DPad)));
            }
            else {
                target.DPad = null;
            }
            if ((from.Buttons != null)) {
                target.Buttons = ((global::Microsoft.Robotics.Services.Sample.XInputGamepad.Buttons)(Microsoft_Robotics_Services_Sample_XInputGamepad_Proxy_Buttons_TO_Microsoft_Robotics_Services_Sample_XInputGamepad_Buttons(from.Buttons)));
            }
            else {
                target.Buttons = null;
            }
            if ((from.Triggers != null)) {
                target.Triggers = ((global::Microsoft.Robotics.Services.Sample.XInputGamepad.Triggers)(Microsoft_Robotics_Services_Sample_XInputGamepad_Proxy_Triggers_TO_Microsoft_Robotics_Services_Sample_XInputGamepad_Triggers(from.Triggers)));
            }
            else {
                target.Triggers = null;
            }
            if ((from.Thumbsticks != null)) {
                target.Thumbsticks = ((global::Microsoft.Robotics.Services.Sample.XInputGamepad.Thumbsticks)(Microsoft_Robotics_Services_Sample_XInputGamepad_Proxy_Thumbsticks_TO_Microsoft_Robotics_Services_Sample_XInputGamepad_Thumbsticks(from.Thumbsticks)));
            }
            else {
                target.Thumbsticks = null;
            }
            if ((from.Vibration != null)) {
                target.Vibration = ((global::Microsoft.Robotics.Services.Sample.XInputGamepad.Vibration)(Microsoft_Robotics_Services_Sample_XInputGamepad_Proxy_Vibration_TO_Microsoft_Robotics_Services_Sample_XInputGamepad_Vibration(from.Vibration)));
            }
            else {
                target.Vibration = null;
            }
            return target;
        }
        
        public static object Microsoft_Robotics_Services_Sample_XInputGamepad_XInputGamepadState_TO_Microsoft_Robotics_Services_Sample_XInputGamepad_Proxy_XInputGamepadState(object transformFrom) {
            global::Microsoft.Robotics.Services.Sample.XInputGamepad.Proxy.XInputGamepadState target = new global::Microsoft.Robotics.Services.Sample.XInputGamepad.Proxy.XInputGamepadState();
            global::Microsoft.Robotics.Services.Sample.XInputGamepad.XInputGamepadState from = ((global::Microsoft.Robotics.Services.Sample.XInputGamepad.XInputGamepadState)(transformFrom));
            target.TimeStamp = from.TimeStamp;
            global::Microsoft.Robotics.Services.Sample.XInputGamepad.Controller tmp = from.Controller;
            if ((tmp != null)) {
                target.Controller = ((global::Microsoft.Robotics.Services.Sample.XInputGamepad.Proxy.Controller)(Microsoft_Robotics_Services_Sample_XInputGamepad_Controller_TO_Microsoft_Robotics_Services_Sample_XInputGamepad_Proxy_Controller(tmp)));
            }
            global::Microsoft.Robotics.Services.Sample.XInputGamepad.DPad tmp0 = from.DPad;
            if ((tmp0 != null)) {
                target.DPad = ((global::Microsoft.Robotics.Services.Sample.XInputGamepad.Proxy.DPad)(Microsoft_Robotics_Services_Sample_XInputGamepad_DPad_TO_Microsoft_Robotics_Services_Sample_XInputGamepad_Proxy_DPad(tmp0)));
            }
            global::Microsoft.Robotics.Services.Sample.XInputGamepad.Buttons tmp1 = from.Buttons;
            if ((tmp1 != null)) {
                target.Buttons = ((global::Microsoft.Robotics.Services.Sample.XInputGamepad.Proxy.Buttons)(Microsoft_Robotics_Services_Sample_XInputGamepad_Buttons_TO_Microsoft_Robotics_Services_Sample_XInputGamepad_Proxy_Buttons(tmp1)));
            }
            global::Microsoft.Robotics.Services.Sample.XInputGamepad.Triggers tmp2 = from.Triggers;
            if ((tmp2 != null)) {
                target.Triggers = ((global::Microsoft.Robotics.Services.Sample.XInputGamepad.Proxy.Triggers)(Microsoft_Robotics_Services_Sample_XInputGamepad_Triggers_TO_Microsoft_Robotics_Services_Sample_XInputGamepad_Proxy_Triggers(tmp2)));
            }
            global::Microsoft.Robotics.Services.Sample.XInputGamepad.Thumbsticks tmp3 = from.Thumbsticks;
            if ((tmp3 != null)) {
                target.Thumbsticks = ((global::Microsoft.Robotics.Services.Sample.XInputGamepad.Proxy.Thumbsticks)(Microsoft_Robotics_Services_Sample_XInputGamepad_Thumbsticks_TO_Microsoft_Robotics_Services_Sample_XInputGamepad_Proxy_Thumbsticks(tmp3)));
            }
            global::Microsoft.Robotics.Services.Sample.XInputGamepad.Vibration tmp4 = from.Vibration;
            if ((tmp4 != null)) {
                target.Vibration = ((global::Microsoft.Robotics.Services.Sample.XInputGamepad.Proxy.Vibration)(Microsoft_Robotics_Services_Sample_XInputGamepad_Vibration_TO_Microsoft_Robotics_Services_Sample_XInputGamepad_Proxy_Vibration(tmp4)));
            }
            return target;
        }
        
        public static object Microsoft_Robotics_Services_Sample_XInputGamepad_Proxy_Controller_TO_Microsoft_Robotics_Services_Sample_XInputGamepad_Controller(object transformFrom) {
            global::Microsoft.Robotics.Services.Sample.XInputGamepad.Controller target = new global::Microsoft.Robotics.Services.Sample.XInputGamepad.Controller();
            global::Microsoft.Robotics.Services.Sample.XInputGamepad.Proxy.Controller from = ((global::Microsoft.Robotics.Services.Sample.XInputGamepad.Proxy.Controller)(transformFrom));
            target.TimeStamp = from.TimeStamp;
            target.PlayerIndex = ((global::Microsoft.Robotics.Services.Sample.XInputGamepad.PlayerIndex)(((int)(from.PlayerIndex))));
            target.IsConnected = from.IsConnected;
            return target;
        }
        
        public static object Microsoft_Robotics_Services_Sample_XInputGamepad_Controller_TO_Microsoft_Robotics_Services_Sample_XInputGamepad_Proxy_Controller(object transformFrom) {
            global::Microsoft.Robotics.Services.Sample.XInputGamepad.Proxy.Controller target = new global::Microsoft.Robotics.Services.Sample.XInputGamepad.Proxy.Controller();
            global::Microsoft.Robotics.Services.Sample.XInputGamepad.Controller from = ((global::Microsoft.Robotics.Services.Sample.XInputGamepad.Controller)(transformFrom));
            target.TimeStamp = from.TimeStamp;
            target.PlayerIndex = ((global::Microsoft.Robotics.Services.Sample.XInputGamepad.Proxy.PlayerIndex)(((int)(from.PlayerIndex))));
            target.IsConnected = from.IsConnected;
            return target;
        }
        
        public static object Microsoft_Robotics_Services_Sample_XInputGamepad_Proxy_DPad_TO_Microsoft_Robotics_Services_Sample_XInputGamepad_DPad(object transformFrom) {
            global::Microsoft.Robotics.Services.Sample.XInputGamepad.DPad target = new global::Microsoft.Robotics.Services.Sample.XInputGamepad.DPad();
            global::Microsoft.Robotics.Services.Sample.XInputGamepad.Proxy.DPad from = ((global::Microsoft.Robotics.Services.Sample.XInputGamepad.Proxy.DPad)(transformFrom));
            target.TimeStamp = from.TimeStamp;
            target.Left = from.Left;
            target.Down = from.Down;
            target.Right = from.Right;
            target.Up = from.Up;
            return target;
        }
        
        public static object Microsoft_Robotics_Services_Sample_XInputGamepad_DPad_TO_Microsoft_Robotics_Services_Sample_XInputGamepad_Proxy_DPad(object transformFrom) {
            global::Microsoft.Robotics.Services.Sample.XInputGamepad.Proxy.DPad target = new global::Microsoft.Robotics.Services.Sample.XInputGamepad.Proxy.DPad();
            global::Microsoft.Robotics.Services.Sample.XInputGamepad.DPad from = ((global::Microsoft.Robotics.Services.Sample.XInputGamepad.DPad)(transformFrom));
            target.TimeStamp = from.TimeStamp;
            target.Left = from.Left;
            target.Down = from.Down;
            target.Right = from.Right;
            target.Up = from.Up;
            return target;
        }
        
        public static object Microsoft_Robotics_Services_Sample_XInputGamepad_Proxy_Buttons_TO_Microsoft_Robotics_Services_Sample_XInputGamepad_Buttons(object transformFrom) {
            global::Microsoft.Robotics.Services.Sample.XInputGamepad.Buttons target = new global::Microsoft.Robotics.Services.Sample.XInputGamepad.Buttons();
            global::Microsoft.Robotics.Services.Sample.XInputGamepad.Proxy.Buttons from = ((global::Microsoft.Robotics.Services.Sample.XInputGamepad.Proxy.Buttons)(transformFrom));
            target.TimeStamp = from.TimeStamp;
            target.A = from.A;
            target.B = from.B;
            target.X = from.X;
            target.Y = from.Y;
            target.Back = from.Back;
            target.Start = from.Start;
            target.LeftStick = from.LeftStick;
            target.RightStick = from.RightStick;
            target.LeftShoulder = from.LeftShoulder;
            target.RightShoulder = from.RightShoulder;
            return target;
        }
        
        public static object Microsoft_Robotics_Services_Sample_XInputGamepad_Buttons_TO_Microsoft_Robotics_Services_Sample_XInputGamepad_Proxy_Buttons(object transformFrom) {
            global::Microsoft.Robotics.Services.Sample.XInputGamepad.Proxy.Buttons target = new global::Microsoft.Robotics.Services.Sample.XInputGamepad.Proxy.Buttons();
            global::Microsoft.Robotics.Services.Sample.XInputGamepad.Buttons from = ((global::Microsoft.Robotics.Services.Sample.XInputGamepad.Buttons)(transformFrom));
            target.TimeStamp = from.TimeStamp;
            target.A = from.A;
            target.B = from.B;
            target.X = from.X;
            target.Y = from.Y;
            target.Back = from.Back;
            target.Start = from.Start;
            target.LeftStick = from.LeftStick;
            target.RightStick = from.RightStick;
            target.LeftShoulder = from.LeftShoulder;
            target.RightShoulder = from.RightShoulder;
            return target;
        }
        
        public static object Microsoft_Robotics_Services_Sample_XInputGamepad_Proxy_Triggers_TO_Microsoft_Robotics_Services_Sample_XInputGamepad_Triggers(object transformFrom) {
            global::Microsoft.Robotics.Services.Sample.XInputGamepad.Triggers target = new global::Microsoft.Robotics.Services.Sample.XInputGamepad.Triggers();
            global::Microsoft.Robotics.Services.Sample.XInputGamepad.Proxy.Triggers from = ((global::Microsoft.Robotics.Services.Sample.XInputGamepad.Proxy.Triggers)(transformFrom));
            target.TimeStamp = from.TimeStamp;
            target.Left = from.Left;
            target.Right = from.Right;
            return target;
        }
        
        public static object Microsoft_Robotics_Services_Sample_XInputGamepad_Triggers_TO_Microsoft_Robotics_Services_Sample_XInputGamepad_Proxy_Triggers(object transformFrom) {
            global::Microsoft.Robotics.Services.Sample.XInputGamepad.Proxy.Triggers target = new global::Microsoft.Robotics.Services.Sample.XInputGamepad.Proxy.Triggers();
            global::Microsoft.Robotics.Services.Sample.XInputGamepad.Triggers from = ((global::Microsoft.Robotics.Services.Sample.XInputGamepad.Triggers)(transformFrom));
            target.TimeStamp = from.TimeStamp;
            target.Left = from.Left;
            target.Right = from.Right;
            return target;
        }
        
        public static object Microsoft_Robotics_Services_Sample_XInputGamepad_Proxy_Thumbsticks_TO_Microsoft_Robotics_Services_Sample_XInputGamepad_Thumbsticks(object transformFrom) {
            global::Microsoft.Robotics.Services.Sample.XInputGamepad.Thumbsticks target = new global::Microsoft.Robotics.Services.Sample.XInputGamepad.Thumbsticks();
            global::Microsoft.Robotics.Services.Sample.XInputGamepad.Proxy.Thumbsticks from = ((global::Microsoft.Robotics.Services.Sample.XInputGamepad.Proxy.Thumbsticks)(transformFrom));
            target.TimeStamp = from.TimeStamp;
            target.LeftX = from.LeftX;
            target.LeftY = from.LeftY;
            target.RightX = from.RightX;
            target.RightY = from.RightY;
            return target;
        }
        
        public static object Microsoft_Robotics_Services_Sample_XInputGamepad_Thumbsticks_TO_Microsoft_Robotics_Services_Sample_XInputGamepad_Proxy_Thumbsticks(object transformFrom) {
            global::Microsoft.Robotics.Services.Sample.XInputGamepad.Proxy.Thumbsticks target = new global::Microsoft.Robotics.Services.Sample.XInputGamepad.Proxy.Thumbsticks();
            global::Microsoft.Robotics.Services.Sample.XInputGamepad.Thumbsticks from = ((global::Microsoft.Robotics.Services.Sample.XInputGamepad.Thumbsticks)(transformFrom));
            target.TimeStamp = from.TimeStamp;
            target.LeftX = from.LeftX;
            target.LeftY = from.LeftY;
            target.RightX = from.RightX;
            target.RightY = from.RightY;
            return target;
        }
        
        public static object Microsoft_Robotics_Services_Sample_XInputGamepad_Proxy_Vibration_TO_Microsoft_Robotics_Services_Sample_XInputGamepad_Vibration(object transformFrom) {
            global::Microsoft.Robotics.Services.Sample.XInputGamepad.Vibration target = new global::Microsoft.Robotics.Services.Sample.XInputGamepad.Vibration();
            global::Microsoft.Robotics.Services.Sample.XInputGamepad.Proxy.Vibration from = ((global::Microsoft.Robotics.Services.Sample.XInputGamepad.Proxy.Vibration)(transformFrom));
            target.TimeStamp = from.TimeStamp;
            target.Left = from.Left;
            target.Right = from.Right;
            return target;
        }
        
        public static object Microsoft_Robotics_Services_Sample_XInputGamepad_Vibration_TO_Microsoft_Robotics_Services_Sample_XInputGamepad_Proxy_Vibration(object transformFrom) {
            global::Microsoft.Robotics.Services.Sample.XInputGamepad.Proxy.Vibration target = new global::Microsoft.Robotics.Services.Sample.XInputGamepad.Proxy.Vibration();
            global::Microsoft.Robotics.Services.Sample.XInputGamepad.Vibration from = ((global::Microsoft.Robotics.Services.Sample.XInputGamepad.Vibration)(transformFrom));
            target.TimeStamp = from.TimeStamp;
            target.Left = from.Left;
            target.Right = from.Right;
            return target;
        }
        
        private static global::Microsoft.Robotics.Services.Sample.XInputGamepad.Proxy.PollRequest _cachedInstance0 = new global::Microsoft.Robotics.Services.Sample.XInputGamepad.Proxy.PollRequest();
        
        private static global::Microsoft.Robotics.Services.Sample.XInputGamepad.PollRequest _cachedInstance = new global::Microsoft.Robotics.Services.Sample.XInputGamepad.PollRequest();
        
        public static object Microsoft_Robotics_Services_Sample_XInputGamepad_Proxy_PollRequest_TO_Microsoft_Robotics_Services_Sample_XInputGamepad_PollRequest(object transformFrom) {
            return _cachedInstance;
        }
        
        public static object Microsoft_Robotics_Services_Sample_XInputGamepad_PollRequest_TO_Microsoft_Robotics_Services_Sample_XInputGamepad_Proxy_PollRequest(object transformFrom) {
            return _cachedInstance0;
        }
        
        private static global::Microsoft.Robotics.Services.Sample.XInputGamepad.Proxy.QueryControllersRequest _cachedInstance2 = new global::Microsoft.Robotics.Services.Sample.XInputGamepad.Proxy.QueryControllersRequest();
        
        private static global::Microsoft.Robotics.Services.Sample.XInputGamepad.QueryControllersRequest _cachedInstance1 = new global::Microsoft.Robotics.Services.Sample.XInputGamepad.QueryControllersRequest();
        
        public static object Microsoft_Robotics_Services_Sample_XInputGamepad_Proxy_QueryControllersRequest_TO_Microsoft_Robotics_Services_Sample_XInputGamepad_QueryControllersRequest(object transformFrom) {
            return _cachedInstance1;
        }
        
        public static object Microsoft_Robotics_Services_Sample_XInputGamepad_QueryControllersRequest_TO_Microsoft_Robotics_Services_Sample_XInputGamepad_Proxy_QueryControllersRequest(object transformFrom) {
            return _cachedInstance2;
        }
        
        public static object Microsoft_Robotics_Services_Sample_XInputGamepad_Proxy_QueryControllersResponse_TO_Microsoft_Robotics_Services_Sample_XInputGamepad_QueryControllersResponse(object transformFrom) {
            global::Microsoft.Robotics.Services.Sample.XInputGamepad.QueryControllersResponse target = new global::Microsoft.Robotics.Services.Sample.XInputGamepad.QueryControllersResponse();
            global::Microsoft.Robotics.Services.Sample.XInputGamepad.Proxy.QueryControllersResponse from = ((global::Microsoft.Robotics.Services.Sample.XInputGamepad.Proxy.QueryControllersResponse)(transformFrom));
            if ((from.Controllers != null)) {
                int count = from.Controllers.Count;
                global::System.Collections.Generic.List<global::Microsoft.Robotics.Services.Sample.XInputGamepad.ControllerCaps> tmp = new global::System.Collections.Generic.List<global::Microsoft.Robotics.Services.Sample.XInputGamepad.ControllerCaps>(count);
                for (int index = 0; (index < count); index = (index + 1)) {
                    global::Microsoft.Robotics.Services.Sample.XInputGamepad.ControllerCaps tmp0 = default(global::Microsoft.Robotics.Services.Sample.XInputGamepad.ControllerCaps);
                    if ((from.Controllers[index] != null)) {
                        tmp0 = ((global::Microsoft.Robotics.Services.Sample.XInputGamepad.ControllerCaps)(Microsoft_Robotics_Services_Sample_XInputGamepad_Proxy_ControllerCaps_TO_Microsoft_Robotics_Services_Sample_XInputGamepad_ControllerCaps(from.Controllers[index])));
                    }
                    else {
                        tmp0 = null;
                    }
                    tmp.Add(tmp0);
                }
                target.Controllers = tmp;
            }
            else {
                target.Controllers = null;
            }
            return target;
        }
        
        public static object Microsoft_Robotics_Services_Sample_XInputGamepad_QueryControllersResponse_TO_Microsoft_Robotics_Services_Sample_XInputGamepad_Proxy_QueryControllersResponse(object transformFrom) {
            global::Microsoft.Robotics.Services.Sample.XInputGamepad.Proxy.QueryControllersResponse target = new global::Microsoft.Robotics.Services.Sample.XInputGamepad.Proxy.QueryControllersResponse();
            global::Microsoft.Robotics.Services.Sample.XInputGamepad.QueryControllersResponse from = ((global::Microsoft.Robotics.Services.Sample.XInputGamepad.QueryControllersResponse)(transformFrom));
            global::System.Collections.Generic.List<global::Microsoft.Robotics.Services.Sample.XInputGamepad.ControllerCaps> tmp = from.Controllers;
            if ((tmp != null)) {
                int count = tmp.Count;
                global::System.Collections.Generic.List<global::Microsoft.Robotics.Services.Sample.XInputGamepad.Proxy.ControllerCaps> tmp0 = new global::System.Collections.Generic.List<global::Microsoft.Robotics.Services.Sample.XInputGamepad.Proxy.ControllerCaps>(count);
                for (int index = 0; (index < count); index = (index + 1)) {
                    global::Microsoft.Robotics.Services.Sample.XInputGamepad.Proxy.ControllerCaps tmp1 = default(global::Microsoft.Robotics.Services.Sample.XInputGamepad.Proxy.ControllerCaps);
                    global::Microsoft.Robotics.Services.Sample.XInputGamepad.ControllerCaps tmp2 = tmp[index];
                    if ((tmp2 != null)) {
                        tmp1 = ((global::Microsoft.Robotics.Services.Sample.XInputGamepad.Proxy.ControllerCaps)(Microsoft_Robotics_Services_Sample_XInputGamepad_ControllerCaps_TO_Microsoft_Robotics_Services_Sample_XInputGamepad_Proxy_ControllerCaps(tmp2)));
                    }
                    tmp0.Add(tmp1);
                }
                target.Controllers = tmp0;
            }
            return target;
        }
        
        public static object Microsoft_Robotics_Services_Sample_XInputGamepad_Proxy_ControllerCaps_TO_Microsoft_Robotics_Services_Sample_XInputGamepad_ControllerCaps(object transformFrom) {
            global::Microsoft.Robotics.Services.Sample.XInputGamepad.ControllerCaps target = new global::Microsoft.Robotics.Services.Sample.XInputGamepad.ControllerCaps();
            global::Microsoft.Robotics.Services.Sample.XInputGamepad.Proxy.ControllerCaps from = ((global::Microsoft.Robotics.Services.Sample.XInputGamepad.Proxy.ControllerCaps)(transformFrom));
            target.ControllerType = ((global::Microsoft.Robotics.Services.Sample.XInputGamepad.ControllerType)(((int)(from.ControllerType))));
            target.TimeStamp = from.TimeStamp;
            target.PlayerIndex = ((global::Microsoft.Robotics.Services.Sample.XInputGamepad.PlayerIndex)(((int)(from.PlayerIndex))));
            target.IsConnected = from.IsConnected;
            return target;
        }
        
        public static object Microsoft_Robotics_Services_Sample_XInputGamepad_ControllerCaps_TO_Microsoft_Robotics_Services_Sample_XInputGamepad_Proxy_ControllerCaps(object transformFrom) {
            global::Microsoft.Robotics.Services.Sample.XInputGamepad.Proxy.ControllerCaps target = new global::Microsoft.Robotics.Services.Sample.XInputGamepad.Proxy.ControllerCaps();
            global::Microsoft.Robotics.Services.Sample.XInputGamepad.ControllerCaps from = ((global::Microsoft.Robotics.Services.Sample.XInputGamepad.ControllerCaps)(transformFrom));
            target.ControllerType = ((global::Microsoft.Robotics.Services.Sample.XInputGamepad.Proxy.ControllerType)(((int)(from.ControllerType))));
            target.TimeStamp = from.TimeStamp;
            target.PlayerIndex = ((global::Microsoft.Robotics.Services.Sample.XInputGamepad.Proxy.PlayerIndex)(((int)(from.PlayerIndex))));
            target.IsConnected = from.IsConnected;
            return target;
        }
    }
}
