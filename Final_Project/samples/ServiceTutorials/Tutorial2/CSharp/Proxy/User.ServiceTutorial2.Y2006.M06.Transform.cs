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
[assembly: global::Microsoft.Dss.Core.Attributes.ServiceDeclarationAttribute(global::Microsoft.Dss.Core.Attributes.DssServiceDeclaration.Transform, SourceAssemblyKey="User.ServiceTutorial2.Y2006.M06, Version=0.0.0.0, Culture=neutral, PublicKeyToken" +
    "=7f9074033fd3dcf7")]
[assembly: global::System.Security.SecurityTransparentAttribute()]
[assembly: global::System.Security.SecurityRulesAttribute(global::System.Security.SecurityRuleSet.Level1)]

namespace Dss.Transforms.TransformUser {
    
    
    public class Transforms : global::Microsoft.Dss.Core.Transforms.TransformBase {
        
        static Transforms() {
            Register();
        }
        
        public static void Register() {
            global::Microsoft.Dss.Core.Transforms.TransformBase.AddProxyTransform(typeof(global::RoboticsServiceTutorial2.Proxy.ServiceTutorial2State), new global::Microsoft.Dss.Core.Attributes.Transform(RoboticsServiceTutorial2_Proxy_ServiceTutorial2State_TO_RoboticsServiceTutorial2_ServiceTutorial2State));
            global::Microsoft.Dss.Core.Transforms.TransformBase.AddSourceTransform(typeof(global::RoboticsServiceTutorial2.ServiceTutorial2State), new global::Microsoft.Dss.Core.Attributes.Transform(RoboticsServiceTutorial2_ServiceTutorial2State_TO_RoboticsServiceTutorial2_Proxy_ServiceTutorial2State));
            global::Microsoft.Dss.Core.Transforms.TransformBase.AddProxyTransform(typeof(global::RoboticsServiceTutorial2.Proxy.IncrementTickRequest), new global::Microsoft.Dss.Core.Attributes.Transform(RoboticsServiceTutorial2_Proxy_IncrementTickRequest_TO_RoboticsServiceTutorial2_IncrementTickRequest));
            global::Microsoft.Dss.Core.Transforms.TransformBase.AddSourceTransform(typeof(global::RoboticsServiceTutorial2.IncrementTickRequest), new global::Microsoft.Dss.Core.Attributes.Transform(RoboticsServiceTutorial2_IncrementTickRequest_TO_RoboticsServiceTutorial2_Proxy_IncrementTickRequest));
        }
        
        public static object RoboticsServiceTutorial2_Proxy_ServiceTutorial2State_TO_RoboticsServiceTutorial2_ServiceTutorial2State(object transformFrom) {
            global::RoboticsServiceTutorial2.ServiceTutorial2State target = new global::RoboticsServiceTutorial2.ServiceTutorial2State();
            global::RoboticsServiceTutorial2.Proxy.ServiceTutorial2State from = ((global::RoboticsServiceTutorial2.Proxy.ServiceTutorial2State)(transformFrom));
            target.Member = from.Member;
            target.Ticks = from.Ticks;
            return target;
        }
        
        public static object RoboticsServiceTutorial2_ServiceTutorial2State_TO_RoboticsServiceTutorial2_Proxy_ServiceTutorial2State(object transformFrom) {
            global::RoboticsServiceTutorial2.Proxy.ServiceTutorial2State target = new global::RoboticsServiceTutorial2.Proxy.ServiceTutorial2State();
            global::RoboticsServiceTutorial2.ServiceTutorial2State from = ((global::RoboticsServiceTutorial2.ServiceTutorial2State)(transformFrom));
            target.Member = from.Member;
            target.Ticks = from.Ticks;
            return target;
        }
        
        private static global::RoboticsServiceTutorial2.Proxy.IncrementTickRequest _cachedInstance0 = new global::RoboticsServiceTutorial2.Proxy.IncrementTickRequest();
        
        private static global::RoboticsServiceTutorial2.IncrementTickRequest _cachedInstance = new global::RoboticsServiceTutorial2.IncrementTickRequest();
        
        public static object RoboticsServiceTutorial2_Proxy_IncrementTickRequest_TO_RoboticsServiceTutorial2_IncrementTickRequest(object transformFrom) {
            return _cachedInstance;
        }
        
        public static object RoboticsServiceTutorial2_IncrementTickRequest_TO_RoboticsServiceTutorial2_Proxy_IncrementTickRequest(object transformFrom) {
            return _cachedInstance0;
        }
    }
}
