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
[assembly: global::Microsoft.Dss.Core.Attributes.ServiceDeclarationAttribute(global::Microsoft.Dss.Core.Attributes.DssServiceDeclaration.Transform, SourceAssemblyKey="User.BlobTrackerCalibrate.Y2007.M04, Version=0.0.0.0, Culture=neutral, PublicKeyT" +
    "oken=7f9074033fd3dcf7")]
[assembly: global::System.Security.SecurityTransparentAttribute()]
[assembly: global::System.Security.SecurityRulesAttribute(global::System.Security.SecurityRuleSet.Level1)]

namespace Dss.Transforms.TransformUser {
    
    
    public class Transforms : global::Microsoft.Dss.Core.Transforms.TransformBase {
        
        static Transforms() {
            Register();
        }
        
        public static void Register() {
            global::Microsoft.Dss.Core.Transforms.TransformBase.AddProxyTransform(typeof(global::Microsoft.Robotics.Services.Sample.BlobTrackerCalibrate.Proxy.BlobTrackerCalibrateState), new global::Microsoft.Dss.Core.Attributes.Transform(Microsoft_Robotics_Services_Sample_BlobTrackerCalibrate_Proxy_BlobTrackerCalibrateState_TO_Microsoft_Robotics_Services_Sample_BlobTrackerCalibrate_BlobTrackerCalibrateState));
            global::Microsoft.Dss.Core.Transforms.TransformBase.AddSourceTransform(typeof(global::Microsoft.Robotics.Services.Sample.BlobTrackerCalibrate.BlobTrackerCalibrateState), new global::Microsoft.Dss.Core.Attributes.Transform(Microsoft_Robotics_Services_Sample_BlobTrackerCalibrate_BlobTrackerCalibrateState_TO_Microsoft_Robotics_Services_Sample_BlobTrackerCalibrate_Proxy_BlobTrackerCalibrateState));
            global::Microsoft.Dss.Core.Transforms.TransformBase.AddProxyTransform(typeof(global::Microsoft.Robotics.Services.Sample.BlobTrackerCalibrate.Proxy.UpdateProcessingRequest), new global::Microsoft.Dss.Core.Attributes.Transform(Microsoft_Robotics_Services_Sample_BlobTrackerCalibrate_Proxy_UpdateProcessingRequest_TO_Microsoft_Robotics_Services_Sample_BlobTrackerCalibrate_UpdateProcessingRequest));
            global::Microsoft.Dss.Core.Transforms.TransformBase.AddSourceTransform(typeof(global::Microsoft.Robotics.Services.Sample.BlobTrackerCalibrate.UpdateProcessingRequest), new global::Microsoft.Dss.Core.Attributes.Transform(Microsoft_Robotics_Services_Sample_BlobTrackerCalibrate_UpdateProcessingRequest_TO_Microsoft_Robotics_Services_Sample_BlobTrackerCalibrate_Proxy_UpdateProcessingRequest));
        }
        
        public static object Microsoft_Robotics_Services_Sample_BlobTrackerCalibrate_Proxy_BlobTrackerCalibrateState_TO_Microsoft_Robotics_Services_Sample_BlobTrackerCalibrate_BlobTrackerCalibrateState(object transformFrom) {
            global::Microsoft.Robotics.Services.Sample.BlobTrackerCalibrate.BlobTrackerCalibrateState target = new global::Microsoft.Robotics.Services.Sample.BlobTrackerCalibrate.BlobTrackerCalibrateState();
            global::Microsoft.Robotics.Services.Sample.BlobTrackerCalibrate.Proxy.BlobTrackerCalibrateState from = ((global::Microsoft.Robotics.Services.Sample.BlobTrackerCalibrate.Proxy.BlobTrackerCalibrateState)(transformFrom));
            target.Processing = from.Processing;
            target.Shutdown = from.Shutdown;
            return target;
        }
        
        public static object Microsoft_Robotics_Services_Sample_BlobTrackerCalibrate_BlobTrackerCalibrateState_TO_Microsoft_Robotics_Services_Sample_BlobTrackerCalibrate_Proxy_BlobTrackerCalibrateState(object transformFrom) {
            global::Microsoft.Robotics.Services.Sample.BlobTrackerCalibrate.Proxy.BlobTrackerCalibrateState target = new global::Microsoft.Robotics.Services.Sample.BlobTrackerCalibrate.Proxy.BlobTrackerCalibrateState();
            global::Microsoft.Robotics.Services.Sample.BlobTrackerCalibrate.BlobTrackerCalibrateState from = ((global::Microsoft.Robotics.Services.Sample.BlobTrackerCalibrate.BlobTrackerCalibrateState)(transformFrom));
            target.Processing = from.Processing;
            target.Shutdown = from.Shutdown;
            return target;
        }
        
        public static object Microsoft_Robotics_Services_Sample_BlobTrackerCalibrate_Proxy_UpdateProcessingRequest_TO_Microsoft_Robotics_Services_Sample_BlobTrackerCalibrate_UpdateProcessingRequest(object transformFrom) {
            global::Microsoft.Robotics.Services.Sample.BlobTrackerCalibrate.UpdateProcessingRequest target = new global::Microsoft.Robotics.Services.Sample.BlobTrackerCalibrate.UpdateProcessingRequest();
            global::Microsoft.Robotics.Services.Sample.BlobTrackerCalibrate.Proxy.UpdateProcessingRequest from = ((global::Microsoft.Robotics.Services.Sample.BlobTrackerCalibrate.Proxy.UpdateProcessingRequest)(transformFrom));
            target.Processing = from.Processing;
            return target;
        }
        
        public static object Microsoft_Robotics_Services_Sample_BlobTrackerCalibrate_UpdateProcessingRequest_TO_Microsoft_Robotics_Services_Sample_BlobTrackerCalibrate_Proxy_UpdateProcessingRequest(object transformFrom) {
            global::Microsoft.Robotics.Services.Sample.BlobTrackerCalibrate.Proxy.UpdateProcessingRequest target = new global::Microsoft.Robotics.Services.Sample.BlobTrackerCalibrate.Proxy.UpdateProcessingRequest();
            global::Microsoft.Robotics.Services.Sample.BlobTrackerCalibrate.UpdateProcessingRequest from = ((global::Microsoft.Robotics.Services.Sample.BlobTrackerCalibrate.UpdateProcessingRequest)(transformFrom));
            target.Processing = from.Processing;
            return target;
        }
    }
}
