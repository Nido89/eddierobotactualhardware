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
[assembly: global::Microsoft.Dss.Core.Attributes.ServiceDeclarationAttribute(global::Microsoft.Dss.Core.Attributes.DssServiceDeclaration.Transform, SourceAssemblyKey="User.SickLRF.Y2005.M12, Version=0.0.0.0, Culture=neutral, PublicKeyToken=7f907403" +
    "3fd3dcf7")]
[assembly: global::System.Security.SecurityTransparentAttribute()]
[assembly: global::System.Security.SecurityRulesAttribute(global::System.Security.SecurityRuleSet.Level1)]

namespace Dss.Transforms.TransformUser {
    
    
    public class Transforms : global::Microsoft.Dss.Core.Transforms.TransformBase {
        
        static Transforms() {
            Register();
        }
        
        public static void Register() {
            global::Microsoft.Dss.Core.Transforms.TransformBase.AddProxyTransform(typeof(global::Microsoft.Robotics.Services.Sensors.SickLRF.Proxy.State), new global::Microsoft.Dss.Core.Attributes.Transform(Microsoft_Robotics_Services_Sensors_SickLRF_Proxy_State_TO_Microsoft_Robotics_Services_Sensors_SickLRF_State));
            global::Microsoft.Dss.Core.Transforms.TransformBase.AddSourceTransform(typeof(global::Microsoft.Robotics.Services.Sensors.SickLRF.State), new global::Microsoft.Dss.Core.Attributes.Transform(Microsoft_Robotics_Services_Sensors_SickLRF_State_TO_Microsoft_Robotics_Services_Sensors_SickLRF_Proxy_State));
            global::Microsoft.Dss.Core.Transforms.TransformBase.AddProxyTransform(typeof(global::Microsoft.Robotics.Services.Sensors.SickLRF.Proxy.ResetType), new global::Microsoft.Dss.Core.Attributes.Transform(Microsoft_Robotics_Services_Sensors_SickLRF_Proxy_ResetType_TO_Microsoft_Robotics_Services_Sensors_SickLRF_ResetType));
            global::Microsoft.Dss.Core.Transforms.TransformBase.AddSourceTransform(typeof(global::Microsoft.Robotics.Services.Sensors.SickLRF.ResetType), new global::Microsoft.Dss.Core.Attributes.Transform(Microsoft_Robotics_Services_Sensors_SickLRF_ResetType_TO_Microsoft_Robotics_Services_Sensors_SickLRF_Proxy_ResetType));
        }
        
        public static object Microsoft_Robotics_Services_Sensors_SickLRF_Proxy_State_TO_Microsoft_Robotics_Services_Sensors_SickLRF_State(object transformFrom) {
            global::Microsoft.Robotics.Services.Sensors.SickLRF.State target = new global::Microsoft.Robotics.Services.Sensors.SickLRF.State();
            global::Microsoft.Robotics.Services.Sensors.SickLRF.Proxy.State from = ((global::Microsoft.Robotics.Services.Sensors.SickLRF.Proxy.State)(transformFrom));
            target.Description = from.Description;
            if ((from.DistanceMeasurements != null)) {
                int count = from.DistanceMeasurements.Length;
                int[] tmp = new int[count];
                global::System.Buffer.BlockCopy(from.DistanceMeasurements, 0, tmp, 0, global::System.Buffer.ByteLength(from.DistanceMeasurements));
                target.DistanceMeasurements = tmp;
            }
            else {
                target.DistanceMeasurements = null;
            }
            target.AngularRange = from.AngularRange;
            target.AngularResolution = from.AngularResolution;
            target.Units = ((global::Microsoft.Robotics.Services.Sensors.SickLRF.Units)(((int)(from.Units))));
            target.TimeStamp = from.TimeStamp;
            target.LinkState = from.LinkState;
            target.ComPort = from.ComPort;
            return target;
        }
        
        public static object Microsoft_Robotics_Services_Sensors_SickLRF_State_TO_Microsoft_Robotics_Services_Sensors_SickLRF_Proxy_State(object transformFrom) {
            global::Microsoft.Robotics.Services.Sensors.SickLRF.Proxy.State target = new global::Microsoft.Robotics.Services.Sensors.SickLRF.Proxy.State();
            global::Microsoft.Robotics.Services.Sensors.SickLRF.State from = ((global::Microsoft.Robotics.Services.Sensors.SickLRF.State)(transformFrom));
            target.Description = from.Description;
            int[] tmp = from.DistanceMeasurements;
            if ((tmp != null)) {
                int count = tmp.Length;
                int[] tmp0 = new int[count];
                global::System.Buffer.BlockCopy(tmp, 0, tmp0, 0, global::System.Buffer.ByteLength(tmp));
                target.DistanceMeasurements = tmp0;
            }
            target.AngularRange = from.AngularRange;
            target.AngularResolution = from.AngularResolution;
            target.Units = ((global::Microsoft.Robotics.Services.Sensors.SickLRF.Proxy.Units)(((int)(from.Units))));
            target.TimeStamp = from.TimeStamp;
            target.LinkState = from.LinkState;
            target.ComPort = from.ComPort;
            return target;
        }
        
        private static global::Microsoft.Robotics.Services.Sensors.SickLRF.Proxy.ResetType _cachedInstance0 = new global::Microsoft.Robotics.Services.Sensors.SickLRF.Proxy.ResetType();
        
        private static global::Microsoft.Robotics.Services.Sensors.SickLRF.ResetType _cachedInstance = new global::Microsoft.Robotics.Services.Sensors.SickLRF.ResetType();
        
        public static object Microsoft_Robotics_Services_Sensors_SickLRF_Proxy_ResetType_TO_Microsoft_Robotics_Services_Sensors_SickLRF_ResetType(object transformFrom) {
            return _cachedInstance;
        }
        
        public static object Microsoft_Robotics_Services_Sensors_SickLRF_ResetType_TO_Microsoft_Robotics_Services_Sensors_SickLRF_Proxy_ResetType(object transformFrom) {
            return _cachedInstance0;
        }
    }
}
