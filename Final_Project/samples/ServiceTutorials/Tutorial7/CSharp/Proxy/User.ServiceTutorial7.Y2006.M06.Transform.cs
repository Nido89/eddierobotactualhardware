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
[assembly: global::Microsoft.Dss.Core.Attributes.ServiceDeclarationAttribute(global::Microsoft.Dss.Core.Attributes.DssServiceDeclaration.Transform, SourceAssemblyKey="User.ServiceTutorial7.Y2006.M06, Version=0.0.0.0, Culture=neutral, PublicKeyToken" +
    "=7f9074033fd3dcf7")]
[assembly: global::System.Security.SecurityTransparentAttribute()]
[assembly: global::System.Security.SecurityRulesAttribute(global::System.Security.SecurityRuleSet.Level1)]

namespace Dss.Transforms.TransformUser {
    
    
    public class Transforms : global::Microsoft.Dss.Core.Transforms.TransformBase {
        
        static Transforms() {
            Register();
        }
        
        public static void Register() {
            global::Microsoft.Dss.Core.Transforms.TransformBase.AddProxyTransform(typeof(global::RoboticsServiceTutorial7.Proxy.ServiceTutorial7State), new global::Microsoft.Dss.Core.Attributes.Transform(RoboticsServiceTutorial7_Proxy_ServiceTutorial7State_TO_RoboticsServiceTutorial7_ServiceTutorial7State));
            global::Microsoft.Dss.Core.Transforms.TransformBase.AddSourceTransform(typeof(global::RoboticsServiceTutorial7.ServiceTutorial7State), new global::Microsoft.Dss.Core.Attributes.Transform(RoboticsServiceTutorial7_ServiceTutorial7State_TO_RoboticsServiceTutorial7_Proxy_ServiceTutorial7State));
            global::Microsoft.Dss.Core.Transforms.TransformBase.AddProxyTransform(typeof(global::RoboticsServiceTutorial7.Proxy.TickCount), new global::Microsoft.Dss.Core.Attributes.Transform(RoboticsServiceTutorial7_Proxy_TickCount_TO_RoboticsServiceTutorial7_TickCount));
            global::Microsoft.Dss.Core.Transforms.TransformBase.AddSourceTransform(typeof(global::RoboticsServiceTutorial7.TickCount), new global::Microsoft.Dss.Core.Attributes.Transform(RoboticsServiceTutorial7_TickCount_TO_RoboticsServiceTutorial7_Proxy_TickCount));
            global::Microsoft.Dss.Core.Transforms.TransformBase.AddProxyTransform(typeof(global::RoboticsServiceTutorial7.Proxy.IncrementTickRequest), new global::Microsoft.Dss.Core.Attributes.Transform(RoboticsServiceTutorial7_Proxy_IncrementTickRequest_TO_RoboticsServiceTutorial7_IncrementTickRequest));
            global::Microsoft.Dss.Core.Transforms.TransformBase.AddSourceTransform(typeof(global::RoboticsServiceTutorial7.IncrementTickRequest), new global::Microsoft.Dss.Core.Attributes.Transform(RoboticsServiceTutorial7_IncrementTickRequest_TO_RoboticsServiceTutorial7_Proxy_IncrementTickRequest));
        }
        
        public static object RoboticsServiceTutorial7_Proxy_ServiceTutorial7State_TO_RoboticsServiceTutorial7_ServiceTutorial7State(object transformFrom) {
            global::RoboticsServiceTutorial7.ServiceTutorial7State target = new global::RoboticsServiceTutorial7.ServiceTutorial7State();
            global::RoboticsServiceTutorial7.Proxy.ServiceTutorial7State from = ((global::RoboticsServiceTutorial7.Proxy.ServiceTutorial7State)(transformFrom));
            if ((from.Clocks != null)) {
                int count = from.Clocks.Count;
                global::System.Collections.Generic.List<string> tmp = new global::System.Collections.Generic.List<string>(count);
                tmp.AddRange(from.Clocks);
                target.Clocks = tmp;
            }
            else {
                target.Clocks = null;
            }
            if ((from.TickCounts != null)) {
                int count0 = from.TickCounts.Count;
                global::System.Collections.Generic.List<global::RoboticsServiceTutorial7.TickCount> tmp0 = new global::System.Collections.Generic.List<global::RoboticsServiceTutorial7.TickCount>(count0);
                for (int index = 0; (index < count0); index = (index + 1)) {
                    global::RoboticsServiceTutorial7.TickCount tmp1 = default(global::RoboticsServiceTutorial7.TickCount);
                    if ((from.TickCounts[index] != null)) {
                        tmp1 = ((global::RoboticsServiceTutorial7.TickCount)(RoboticsServiceTutorial7_Proxy_TickCount_TO_RoboticsServiceTutorial7_TickCount(from.TickCounts[index])));
                    }
                    else {
                        tmp1 = null;
                    }
                    tmp0.Add(tmp1);
                }
                target.TickCounts = tmp0;
            }
            else {
                target.TickCounts = null;
            }
            return target;
        }
        
        public static object RoboticsServiceTutorial7_ServiceTutorial7State_TO_RoboticsServiceTutorial7_Proxy_ServiceTutorial7State(object transformFrom) {
            global::RoboticsServiceTutorial7.Proxy.ServiceTutorial7State target = new global::RoboticsServiceTutorial7.Proxy.ServiceTutorial7State();
            global::RoboticsServiceTutorial7.ServiceTutorial7State from = ((global::RoboticsServiceTutorial7.ServiceTutorial7State)(transformFrom));
            global::System.Collections.Generic.List<string> tmp = from.Clocks;
            if ((tmp != null)) {
                int count = tmp.Count;
                global::System.Collections.Generic.List<string> tmp0 = new global::System.Collections.Generic.List<string>(count);
                tmp0.AddRange(tmp);
                target.Clocks = tmp0;
            }
            global::System.Collections.Generic.List<global::RoboticsServiceTutorial7.TickCount> tmp1 = from.TickCounts;
            if ((tmp1 != null)) {
                int count0 = tmp1.Count;
                global::System.Collections.Generic.List<global::RoboticsServiceTutorial7.Proxy.TickCount> tmp2 = new global::System.Collections.Generic.List<global::RoboticsServiceTutorial7.Proxy.TickCount>(count0);
                for (int index = 0; (index < count0); index = (index + 1)) {
                    global::RoboticsServiceTutorial7.Proxy.TickCount tmp3 = default(global::RoboticsServiceTutorial7.Proxy.TickCount);
                    global::RoboticsServiceTutorial7.TickCount tmp4 = tmp1[index];
                    if ((tmp4 != null)) {
                        tmp3 = ((global::RoboticsServiceTutorial7.Proxy.TickCount)(RoboticsServiceTutorial7_TickCount_TO_RoboticsServiceTutorial7_Proxy_TickCount(tmp4)));
                    }
                    tmp2.Add(tmp3);
                }
                target.TickCounts = tmp2;
            }
            return target;
        }
        
        public static object RoboticsServiceTutorial7_Proxy_TickCount_TO_RoboticsServiceTutorial7_TickCount(object transformFrom) {
            global::RoboticsServiceTutorial7.TickCount target = new global::RoboticsServiceTutorial7.TickCount();
            global::RoboticsServiceTutorial7.Proxy.TickCount from = ((global::RoboticsServiceTutorial7.Proxy.TickCount)(transformFrom));
            target.Name = from.Name;
            target.Count = from.Count;
            return target;
        }
        
        public static object RoboticsServiceTutorial7_TickCount_TO_RoboticsServiceTutorial7_Proxy_TickCount(object transformFrom) {
            global::RoboticsServiceTutorial7.Proxy.TickCount target = new global::RoboticsServiceTutorial7.Proxy.TickCount();
            global::RoboticsServiceTutorial7.TickCount from = ((global::RoboticsServiceTutorial7.TickCount)(transformFrom));
            target.Name = from.Name;
            target.Count = from.Count;
            return target;
        }
        
        public static object RoboticsServiceTutorial7_Proxy_IncrementTickRequest_TO_RoboticsServiceTutorial7_IncrementTickRequest(object transformFrom) {
            global::RoboticsServiceTutorial7.IncrementTickRequest target = new global::RoboticsServiceTutorial7.IncrementTickRequest();
            global::RoboticsServiceTutorial7.Proxy.IncrementTickRequest from = ((global::RoboticsServiceTutorial7.Proxy.IncrementTickRequest)(transformFrom));
            target.Name = from.Name;
            return target;
        }
        
        public static object RoboticsServiceTutorial7_IncrementTickRequest_TO_RoboticsServiceTutorial7_Proxy_IncrementTickRequest(object transformFrom) {
            global::RoboticsServiceTutorial7.Proxy.IncrementTickRequest target = new global::RoboticsServiceTutorial7.Proxy.IncrementTickRequest();
            global::RoboticsServiceTutorial7.IncrementTickRequest from = ((global::RoboticsServiceTutorial7.IncrementTickRequest)(transformFrom));
            target.Name = from.Name;
            return target;
        }
    }
}
