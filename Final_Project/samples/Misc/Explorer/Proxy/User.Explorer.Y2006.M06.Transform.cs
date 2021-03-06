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
[assembly: global::System.Reflection.AssemblyTitleAttribute("DSS Explorer Service")]
[assembly: global::Microsoft.Dss.Core.Attributes.ServiceDeclarationAttribute(global::Microsoft.Dss.Core.Attributes.DssServiceDeclaration.Transform, SourceAssemblyKey="User.Explorer.Y2006.M06, Version=0.0.0.0, Culture=neutral, PublicKeyToken=7f90740" +
    "33fd3dcf7")]
[assembly: global::System.Security.SecurityTransparentAttribute()]
[assembly: global::System.Security.SecurityRulesAttribute(global::System.Security.SecurityRuleSet.Level1)]

namespace Dss.Transforms.TransformUser {
    
    
    public class Transforms : global::Microsoft.Dss.Core.Transforms.TransformBase {
        
        static Transforms() {
            Register();
        }
        
        public static void Register() {
            global::Microsoft.Dss.Core.Transforms.TransformBase.AddProxyTransform(typeof(global::Microsoft.Robotics.Services.Explorer.Proxy.State), new global::Microsoft.Dss.Core.Attributes.Transform(Microsoft_Robotics_Services_Explorer_Proxy_State_TO_Microsoft_Robotics_Services_Explorer_State));
            global::Microsoft.Dss.Core.Transforms.TransformBase.AddSourceTransform(typeof(global::Microsoft.Robotics.Services.Explorer.State), new global::Microsoft.Dss.Core.Attributes.Transform(Microsoft_Robotics_Services_Explorer_State_TO_Microsoft_Robotics_Services_Explorer_Proxy_State));
            global::Microsoft.Dss.Core.Transforms.TransformBase.AddProxyTransform(typeof(global::Microsoft.Robotics.Services.Explorer.Proxy.WatchDogUpdateRequest), new global::Microsoft.Dss.Core.Attributes.Transform(Microsoft_Robotics_Services_Explorer_Proxy_WatchDogUpdateRequest_TO_Microsoft_Robotics_Services_Explorer_WatchDogUpdateRequest));
            global::Microsoft.Dss.Core.Transforms.TransformBase.AddSourceTransform(typeof(global::Microsoft.Robotics.Services.Explorer.WatchDogUpdateRequest), new global::Microsoft.Dss.Core.Attributes.Transform(Microsoft_Robotics_Services_Explorer_WatchDogUpdateRequest_TO_Microsoft_Robotics_Services_Explorer_Proxy_WatchDogUpdateRequest));
        }
        
        public static object Microsoft_Robotics_Services_Explorer_Proxy_State_TO_Microsoft_Robotics_Services_Explorer_State(object transformFrom) {
            global::Microsoft.Robotics.Services.Explorer.State target = new global::Microsoft.Robotics.Services.Explorer.State();
            global::Microsoft.Robotics.Services.Explorer.Proxy.State from = ((global::Microsoft.Robotics.Services.Explorer.Proxy.State)(transformFrom));
            if ((from.DriveState != null)) {
                global::Microsoft.Robotics.Services.Drive.Proxy.DriveDifferentialTwoWheelState tmp = new global::Microsoft.Robotics.Services.Drive.Proxy.DriveDifferentialTwoWheelState();
                ((Microsoft.Dss.Core.IDssSerializable)(from.DriveState)).CopyTo(((Microsoft.Dss.Core.IDssSerializable)(tmp)));
                target.DriveState = tmp;
            }
            else {
                target.DriveState = null;
            }
            target.Countdown = from.Countdown;
            target.LogicalState = ((global::Microsoft.Robotics.Services.Explorer.LogicalState)(((int)(from.LogicalState))));
            target.NewHeading = from.NewHeading;
            target.Velocity = from.Velocity;
            if ((from.South != null)) {
                global::Microsoft.Robotics.Services.Sensors.SickLRF.Proxy.State tmp0 = new global::Microsoft.Robotics.Services.Sensors.SickLRF.Proxy.State();
                ((Microsoft.Dss.Core.IDssSerializable)(from.South)).CopyTo(((Microsoft.Dss.Core.IDssSerializable)(tmp0)));
                target.South = tmp0;
            }
            else {
                target.South = null;
            }
            target.Mapped = from.Mapped;
            target.MostRecentLaser = from.MostRecentLaser;
            return target;
        }
        
        public static object Microsoft_Robotics_Services_Explorer_State_TO_Microsoft_Robotics_Services_Explorer_Proxy_State(object transformFrom) {
            global::Microsoft.Robotics.Services.Explorer.Proxy.State target = new global::Microsoft.Robotics.Services.Explorer.Proxy.State();
            global::Microsoft.Robotics.Services.Explorer.State from = ((global::Microsoft.Robotics.Services.Explorer.State)(transformFrom));
            global::Microsoft.Robotics.Services.Drive.Proxy.DriveDifferentialTwoWheelState tmp = from.DriveState;
            if ((tmp != null)) {
                global::Microsoft.Robotics.Services.Drive.Proxy.DriveDifferentialTwoWheelState tmp0 = new global::Microsoft.Robotics.Services.Drive.Proxy.DriveDifferentialTwoWheelState();
                ((Microsoft.Dss.Core.IDssSerializable)(tmp)).CopyTo(((Microsoft.Dss.Core.IDssSerializable)(tmp0)));
                target.DriveState = tmp0;
            }
            target.Countdown = from.Countdown;
            target.LogicalState = ((global::Microsoft.Robotics.Services.Explorer.Proxy.LogicalState)(((int)(from.LogicalState))));
            target.NewHeading = from.NewHeading;
            target.Velocity = from.Velocity;
            global::Microsoft.Robotics.Services.Sensors.SickLRF.Proxy.State tmp1 = from.South;
            if ((tmp1 != null)) {
                global::Microsoft.Robotics.Services.Sensors.SickLRF.Proxy.State tmp2 = new global::Microsoft.Robotics.Services.Sensors.SickLRF.Proxy.State();
                ((Microsoft.Dss.Core.IDssSerializable)(tmp1)).CopyTo(((Microsoft.Dss.Core.IDssSerializable)(tmp2)));
                target.South = tmp2;
            }
            target.Mapped = from.Mapped;
            target.MostRecentLaser = from.MostRecentLaser;
            return target;
        }
        
        public static object Microsoft_Robotics_Services_Explorer_Proxy_WatchDogUpdateRequest_TO_Microsoft_Robotics_Services_Explorer_WatchDogUpdateRequest(object transformFrom) {
            global::Microsoft.Robotics.Services.Explorer.WatchDogUpdateRequest target = new global::Microsoft.Robotics.Services.Explorer.WatchDogUpdateRequest();
            global::Microsoft.Robotics.Services.Explorer.Proxy.WatchDogUpdateRequest from = ((global::Microsoft.Robotics.Services.Explorer.Proxy.WatchDogUpdateRequest)(transformFrom));
            target.TimeStamp = from.TimeStamp;
            return target;
        }
        
        public static object Microsoft_Robotics_Services_Explorer_WatchDogUpdateRequest_TO_Microsoft_Robotics_Services_Explorer_Proxy_WatchDogUpdateRequest(object transformFrom) {
            global::Microsoft.Robotics.Services.Explorer.Proxy.WatchDogUpdateRequest target = new global::Microsoft.Robotics.Services.Explorer.Proxy.WatchDogUpdateRequest();
            global::Microsoft.Robotics.Services.Explorer.WatchDogUpdateRequest from = ((global::Microsoft.Robotics.Services.Explorer.WatchDogUpdateRequest)(transformFrom));
            target.TimeStamp = from.TimeStamp;
            return target;
        }
    }
}
