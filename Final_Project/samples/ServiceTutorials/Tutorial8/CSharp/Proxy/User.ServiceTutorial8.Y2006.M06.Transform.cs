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
[assembly: global::Microsoft.Dss.Core.Attributes.ServiceDeclarationAttribute(global::Microsoft.Dss.Core.Attributes.DssServiceDeclaration.Transform, SourceAssemblyKey="User.ServiceTutorial8.Y2006.M06, Version=0.0.0.0, Culture=neutral, PublicKeyToken" +
    "=7f9074033fd3dcf7")]
[assembly: global::System.Security.SecurityTransparentAttribute()]
[assembly: global::System.Security.SecurityRulesAttribute(global::System.Security.SecurityRuleSet.Level1)]

namespace Dss.Transforms.TransformUser {
    
    
    public class Transforms : global::Microsoft.Dss.Core.Transforms.TransformBase {
        
        static Transforms() {
            Register();
        }
        
        public static void Register() {
            global::Microsoft.Dss.Core.Transforms.TransformBase.AddProxyTransform(typeof(global::ServiceTutorial8.Proxy.GenericState), new global::Microsoft.Dss.Core.Attributes.Transform(ServiceTutorial8_Proxy_GenericState_TO_ServiceTutorial8_GenericState));
            global::Microsoft.Dss.Core.Transforms.TransformBase.AddSourceTransform(typeof(global::ServiceTutorial8.GenericState), new global::Microsoft.Dss.Core.Attributes.Transform(ServiceTutorial8_GenericState_TO_ServiceTutorial8_Proxy_GenericState));
        }
        
        public static object ServiceTutorial8_Proxy_GenericState_TO_ServiceTutorial8_GenericState(object transformFrom) {
            global::ServiceTutorial8.GenericState target = new global::ServiceTutorial8.GenericState();
            global::ServiceTutorial8.Proxy.GenericState from = ((global::ServiceTutorial8.Proxy.GenericState)(transformFrom));
            target.FirstName = from.FirstName;
            target.LastName = from.LastName;
            return target;
        }
        
        public static object ServiceTutorial8_GenericState_TO_ServiceTutorial8_Proxy_GenericState(object transformFrom) {
            global::ServiceTutorial8.Proxy.GenericState target = new global::ServiceTutorial8.Proxy.GenericState();
            global::ServiceTutorial8.GenericState from = ((global::ServiceTutorial8.GenericState)(transformFrom));
            target.FirstName = from.FirstName;
            target.LastName = from.LastName;
            return target;
        }
    }
}