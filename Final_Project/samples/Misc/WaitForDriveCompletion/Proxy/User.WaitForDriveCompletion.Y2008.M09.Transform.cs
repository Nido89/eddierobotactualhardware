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
[assembly: global::Microsoft.Dss.Core.Attributes.ServiceDeclarationAttribute(global::Microsoft.Dss.Core.Attributes.DssServiceDeclaration.Transform, SourceAssemblyKey="User.WaitForDriveCompletion.Y2008.M09, Version=0.0.0.0, Culture=neutral, PublicKe" +
    "yToken=7f9074033fd3dcf7")]
[assembly: global::System.Security.SecurityTransparentAttribute()]
[assembly: global::System.Security.SecurityRulesAttribute(global::System.Security.SecurityRuleSet.Level1)]

namespace Dss.Transforms.TransformUser {
    
    
    public class Transforms : global::Microsoft.Dss.Core.Transforms.TransformBase {
        
        static Transforms() {
            Register();
        }
        
        public static void Register() {
            global::Microsoft.Dss.Core.Transforms.TransformBase.AddProxyTransform(typeof(global::Microsoft.Dss.Services.Samples.WaitForDriveCompletion.Proxy.WaitForDriveCompletionState), new global::Microsoft.Dss.Core.Attributes.Transform(Microsoft_Dss_Services_Samples_WaitForDriveCompletion_Proxy_WaitForDriveCompletionState_TO_Microsoft_Dss_Services_Samples_WaitForDriveCompletion_WaitForDriveCompletionState));
            global::Microsoft.Dss.Core.Transforms.TransformBase.AddSourceTransform(typeof(global::Microsoft.Dss.Services.Samples.WaitForDriveCompletion.WaitForDriveCompletionState), new global::Microsoft.Dss.Core.Attributes.Transform(Microsoft_Dss_Services_Samples_WaitForDriveCompletion_WaitForDriveCompletionState_TO_Microsoft_Dss_Services_Samples_WaitForDriveCompletion_Proxy_WaitForDriveCompletionState));
            global::Microsoft.Dss.Core.Transforms.TransformBase.AddProxyTransform(typeof(global::Microsoft.Dss.Services.Samples.WaitForDriveCompletion.Proxy.WaitRequestType), new global::Microsoft.Dss.Core.Attributes.Transform(Microsoft_Dss_Services_Samples_WaitForDriveCompletion_Proxy_WaitRequestType_TO_Microsoft_Dss_Services_Samples_WaitForDriveCompletion_WaitRequestType));
            global::Microsoft.Dss.Core.Transforms.TransformBase.AddSourceTransform(typeof(global::Microsoft.Dss.Services.Samples.WaitForDriveCompletion.WaitRequestType), new global::Microsoft.Dss.Core.Attributes.Transform(Microsoft_Dss_Services_Samples_WaitForDriveCompletion_WaitRequestType_TO_Microsoft_Dss_Services_Samples_WaitForDriveCompletion_Proxy_WaitRequestType));
            global::Microsoft.Dss.Core.Transforms.TransformBase.AddProxyTransform(typeof(global::Microsoft.Dss.Services.Samples.WaitForDriveCompletion.Proxy.WaitResponseType), new global::Microsoft.Dss.Core.Attributes.Transform(Microsoft_Dss_Services_Samples_WaitForDriveCompletion_Proxy_WaitResponseType_TO_Microsoft_Dss_Services_Samples_WaitForDriveCompletion_WaitResponseType));
            global::Microsoft.Dss.Core.Transforms.TransformBase.AddSourceTransform(typeof(global::Microsoft.Dss.Services.Samples.WaitForDriveCompletion.WaitResponseType), new global::Microsoft.Dss.Core.Attributes.Transform(Microsoft_Dss_Services_Samples_WaitForDriveCompletion_WaitResponseType_TO_Microsoft_Dss_Services_Samples_WaitForDriveCompletion_Proxy_WaitResponseType));
        }
        
        public static object Microsoft_Dss_Services_Samples_WaitForDriveCompletion_Proxy_WaitForDriveCompletionState_TO_Microsoft_Dss_Services_Samples_WaitForDriveCompletion_WaitForDriveCompletionState(object transformFrom) {
            global::Microsoft.Dss.Services.Samples.WaitForDriveCompletion.WaitForDriveCompletionState target = new global::Microsoft.Dss.Services.Samples.WaitForDriveCompletion.WaitForDriveCompletionState();
            global::Microsoft.Dss.Services.Samples.WaitForDriveCompletion.Proxy.WaitForDriveCompletionState from = ((global::Microsoft.Dss.Services.Samples.WaitForDriveCompletion.Proxy.WaitForDriveCompletionState)(transformFrom));
            target.LastStatus = from.LastStatus;
            return target;
        }
        
        public static object Microsoft_Dss_Services_Samples_WaitForDriveCompletion_WaitForDriveCompletionState_TO_Microsoft_Dss_Services_Samples_WaitForDriveCompletion_Proxy_WaitForDriveCompletionState(object transformFrom) {
            global::Microsoft.Dss.Services.Samples.WaitForDriveCompletion.Proxy.WaitForDriveCompletionState target = new global::Microsoft.Dss.Services.Samples.WaitForDriveCompletion.Proxy.WaitForDriveCompletionState();
            global::Microsoft.Dss.Services.Samples.WaitForDriveCompletion.WaitForDriveCompletionState from = ((global::Microsoft.Dss.Services.Samples.WaitForDriveCompletion.WaitForDriveCompletionState)(transformFrom));
            target.LastStatus = from.LastStatus;
            return target;
        }
        
        private static global::Microsoft.Dss.Services.Samples.WaitForDriveCompletion.Proxy.WaitRequestType _cachedInstance0 = new global::Microsoft.Dss.Services.Samples.WaitForDriveCompletion.Proxy.WaitRequestType();
        
        private static global::Microsoft.Dss.Services.Samples.WaitForDriveCompletion.WaitRequestType _cachedInstance = new global::Microsoft.Dss.Services.Samples.WaitForDriveCompletion.WaitRequestType();
        
        public static object Microsoft_Dss_Services_Samples_WaitForDriveCompletion_Proxy_WaitRequestType_TO_Microsoft_Dss_Services_Samples_WaitForDriveCompletion_WaitRequestType(object transformFrom) {
            return _cachedInstance;
        }
        
        public static object Microsoft_Dss_Services_Samples_WaitForDriveCompletion_WaitRequestType_TO_Microsoft_Dss_Services_Samples_WaitForDriveCompletion_Proxy_WaitRequestType(object transformFrom) {
            return _cachedInstance0;
        }
        
        public static object Microsoft_Dss_Services_Samples_WaitForDriveCompletion_Proxy_WaitResponseType_TO_Microsoft_Dss_Services_Samples_WaitForDriveCompletion_WaitResponseType(object transformFrom) {
            global::Microsoft.Dss.Services.Samples.WaitForDriveCompletion.WaitResponseType target = new global::Microsoft.Dss.Services.Samples.WaitForDriveCompletion.WaitResponseType();
            global::Microsoft.Dss.Services.Samples.WaitForDriveCompletion.Proxy.WaitResponseType from = ((global::Microsoft.Dss.Services.Samples.WaitForDriveCompletion.Proxy.WaitResponseType)(transformFrom));
            target.DriveStatus = from.DriveStatus;
            return target;
        }
        
        public static object Microsoft_Dss_Services_Samples_WaitForDriveCompletion_WaitResponseType_TO_Microsoft_Dss_Services_Samples_WaitForDriveCompletion_Proxy_WaitResponseType(object transformFrom) {
            global::Microsoft.Dss.Services.Samples.WaitForDriveCompletion.Proxy.WaitResponseType target = new global::Microsoft.Dss.Services.Samples.WaitForDriveCompletion.Proxy.WaitResponseType();
            global::Microsoft.Dss.Services.Samples.WaitForDriveCompletion.WaitResponseType from = ((global::Microsoft.Dss.Services.Samples.WaitForDriveCompletion.WaitResponseType)(transformFrom));
            target.DriveStatus = from.DriveStatus;
            return target;
        }
    }
}
