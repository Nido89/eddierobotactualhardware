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
[assembly: global::Microsoft.Dss.Core.Attributes.ServiceDeclarationAttribute(global::Microsoft.Dss.Core.Attributes.DssServiceDeclaration.Transform, SourceAssemblyKey="User.RoboticsTutorial7.Y2007.M05, Version=0.0.0.0, Culture=neutral, PublicKeyToke" +
    "n=7f9074033fd3dcf7")]
[assembly: global::System.Security.SecurityTransparentAttribute()]
[assembly: global::System.Security.SecurityRulesAttribute(global::System.Security.SecurityRuleSet.Level1)]

namespace Dss.Transforms.TransformUser {
    
    
    public class Transforms : global::Microsoft.Dss.Core.Transforms.TransformBase {
        
        static Transforms() {
            Register();
        }
        
        public static void Register() {
            global::Microsoft.Dss.Core.Transforms.TransformBase.AddProxyTransform(typeof(global::Microsoft.Robotics.Services.RoboticsTutorial7.Proxy.RoboticsTutorial7State), new global::Microsoft.Dss.Core.Attributes.Transform(Microsoft_Robotics_Services_RoboticsTutorial7_Proxy_RoboticsTutorial7State_TO_Microsoft_Robotics_Services_RoboticsTutorial7_RoboticsTutorial7State));
            global::Microsoft.Dss.Core.Transforms.TransformBase.AddSourceTransform(typeof(global::Microsoft.Robotics.Services.RoboticsTutorial7.RoboticsTutorial7State), new global::Microsoft.Dss.Core.Attributes.Transform(Microsoft_Robotics_Services_RoboticsTutorial7_RoboticsTutorial7State_TO_Microsoft_Robotics_Services_RoboticsTutorial7_Proxy_RoboticsTutorial7State));
        }
        
        private static global::Microsoft.Robotics.Services.RoboticsTutorial7.Proxy.RoboticsTutorial7State _cachedInstance0 = new global::Microsoft.Robotics.Services.RoboticsTutorial7.Proxy.RoboticsTutorial7State();
        
        private static global::Microsoft.Robotics.Services.RoboticsTutorial7.RoboticsTutorial7State _cachedInstance = new global::Microsoft.Robotics.Services.RoboticsTutorial7.RoboticsTutorial7State();
        
        public static object Microsoft_Robotics_Services_RoboticsTutorial7_Proxy_RoboticsTutorial7State_TO_Microsoft_Robotics_Services_RoboticsTutorial7_RoboticsTutorial7State(object transformFrom) {
            return _cachedInstance;
        }
        
        public static object Microsoft_Robotics_Services_RoboticsTutorial7_RoboticsTutorial7State_TO_Microsoft_Robotics_Services_RoboticsTutorial7_Proxy_RoboticsTutorial7State(object transformFrom) {
            return _cachedInstance0;
        }
    }
}