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
[assembly: global::Microsoft.Dss.Core.Attributes.ServiceDeclarationAttribute(global::Microsoft.Dss.Core.Attributes.DssServiceDeclaration.Transform, SourceAssemblyKey="User.WpfTextToSpeechUI.Y2008.M08, Version=0.0.0.0, Culture=neutral, PublicKeyToke" +
    "n=7f9074033fd3dcf7")]
[assembly: global::System.Security.SecurityTransparentAttribute()]
[assembly: global::System.Security.SecurityRulesAttribute(global::System.Security.SecurityRuleSet.Level1)]

namespace Dss.Transforms.TransformUser {
    
    
    public class Transforms : global::Microsoft.Dss.Core.Transforms.TransformBase {
        
        static Transforms() {
            Register();
        }
        
        public static void Register() {
            global::Microsoft.Dss.Core.Transforms.TransformBase.AddProxyTransform(typeof(global::Microsoft.Dss.Services.Samples.WpfTextToSpeechUI.Proxy.WpfTextToSpeechUIState), new global::Microsoft.Dss.Core.Attributes.Transform(Microsoft_Dss_Services_Samples_WpfTextToSpeechUI_Proxy_WpfTextToSpeechUIState_TO_Microsoft_Dss_Services_Samples_WpfTextToSpeechUI_WpfTextToSpeechUIState));
            global::Microsoft.Dss.Core.Transforms.TransformBase.AddSourceTransform(typeof(global::Microsoft.Dss.Services.Samples.WpfTextToSpeechUI.WpfTextToSpeechUIState), new global::Microsoft.Dss.Core.Attributes.Transform(Microsoft_Dss_Services_Samples_WpfTextToSpeechUI_WpfTextToSpeechUIState_TO_Microsoft_Dss_Services_Samples_WpfTextToSpeechUI_Proxy_WpfTextToSpeechUIState));
        }
        
        private static global::Microsoft.Dss.Services.Samples.WpfTextToSpeechUI.Proxy.WpfTextToSpeechUIState _cachedInstance0 = new global::Microsoft.Dss.Services.Samples.WpfTextToSpeechUI.Proxy.WpfTextToSpeechUIState();
        
        private static global::Microsoft.Dss.Services.Samples.WpfTextToSpeechUI.WpfTextToSpeechUIState _cachedInstance = new global::Microsoft.Dss.Services.Samples.WpfTextToSpeechUI.WpfTextToSpeechUIState();
        
        public static object Microsoft_Dss_Services_Samples_WpfTextToSpeechUI_Proxy_WpfTextToSpeechUIState_TO_Microsoft_Dss_Services_Samples_WpfTextToSpeechUI_WpfTextToSpeechUIState(object transformFrom) {
            return _cachedInstance;
        }
        
        public static object Microsoft_Dss_Services_Samples_WpfTextToSpeechUI_WpfTextToSpeechUIState_TO_Microsoft_Dss_Services_Samples_WpfTextToSpeechUI_Proxy_WpfTextToSpeechUIState(object transformFrom) {
            return _cachedInstance0;
        }
    }
}