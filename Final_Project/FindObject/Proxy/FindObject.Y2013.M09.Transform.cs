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
[assembly: global::Microsoft.Dss.Core.Attributes.ServiceDeclarationAttribute(global::Microsoft.Dss.Core.Attributes.DssServiceDeclaration.Transform, SourceAssemblyKey="FindObject.Y2013.M09, Version=0.0.0.0, Culture=neutral, PublicKeyToken=7f9074033f" +
    "d3dcf7")]
[assembly: global::System.Security.SecurityTransparentAttribute()]
[assembly: global::System.Security.SecurityRulesAttribute(global::System.Security.SecurityRuleSet.Level1)]

namespace Dss.Transforms.TransformFindObject {
    
    
    public class Transforms : global::Microsoft.Dss.Core.Transforms.TransformBase {
        
        static Transforms() {
            Register();
        }
        
        public static void Register() {
            global::Microsoft.Dss.Core.Transforms.TransformBase.AddProxyTransform(typeof(global::Robotics.FindObject.Diagram.Proxy.ActionRequest), new global::Microsoft.Dss.Core.Attributes.Transform(Robotics_FindObject_Diagram_Proxy_ActionRequest_TO_Robotics_FindObject_Diagram_ActionRequest));
            global::Microsoft.Dss.Core.Transforms.TransformBase.AddSourceTransform(typeof(global::Robotics.FindObject.Diagram.ActionRequest), new global::Microsoft.Dss.Core.Attributes.Transform(Robotics_FindObject_Diagram_ActionRequest_TO_Robotics_FindObject_Diagram_Proxy_ActionRequest));
            global::Microsoft.Dss.Core.Transforms.TransformBase.AddProxyTransform(typeof(global::Robotics.FindObject.Diagram.Proxy.ActionResponse), new global::Microsoft.Dss.Core.Attributes.Transform(Robotics_FindObject_Diagram_Proxy_ActionResponse_TO_Robotics_FindObject_Diagram_ActionResponse));
            global::Microsoft.Dss.Core.Transforms.TransformBase.AddSourceTransform(typeof(global::Robotics.FindObject.Diagram.ActionResponse), new global::Microsoft.Dss.Core.Attributes.Transform(Robotics_FindObject_Diagram_ActionResponse_TO_Robotics_FindObject_Diagram_Proxy_ActionResponse));
            global::Microsoft.Dss.Core.Transforms.TransformBase.AddProxyTransform(typeof(global::Robotics.FindObject.Diagram.Proxy.DiagramState), new global::Microsoft.Dss.Core.Attributes.Transform(Robotics_FindObject_Diagram_Proxy_DiagramState_TO_Robotics_FindObject_Diagram_DiagramState));
            global::Microsoft.Dss.Core.Transforms.TransformBase.AddSourceTransform(typeof(global::Robotics.FindObject.Diagram.DiagramState), new global::Microsoft.Dss.Core.Attributes.Transform(Robotics_FindObject_Diagram_DiagramState_TO_Robotics_FindObject_Diagram_Proxy_DiagramState));
        }
        
        private static global::Robotics.FindObject.Diagram.Proxy.ActionRequest _cachedInstance0 = new global::Robotics.FindObject.Diagram.Proxy.ActionRequest();
        
        private static global::Robotics.FindObject.Diagram.ActionRequest _cachedInstance = new global::Robotics.FindObject.Diagram.ActionRequest();
        
        public static object Robotics_FindObject_Diagram_Proxy_ActionRequest_TO_Robotics_FindObject_Diagram_ActionRequest(object transformFrom) {
            return _cachedInstance;
        }
        
        public static object Robotics_FindObject_Diagram_ActionRequest_TO_Robotics_FindObject_Diagram_Proxy_ActionRequest(object transformFrom) {
            return _cachedInstance0;
        }
        
        private static global::Robotics.FindObject.Diagram.Proxy.ActionResponse _cachedInstance2 = new global::Robotics.FindObject.Diagram.Proxy.ActionResponse();
        
        private static global::Robotics.FindObject.Diagram.ActionResponse _cachedInstance1 = new global::Robotics.FindObject.Diagram.ActionResponse();
        
        public static object Robotics_FindObject_Diagram_Proxy_ActionResponse_TO_Robotics_FindObject_Diagram_ActionResponse(object transformFrom) {
            return _cachedInstance1;
        }
        
        public static object Robotics_FindObject_Diagram_ActionResponse_TO_Robotics_FindObject_Diagram_Proxy_ActionResponse(object transformFrom) {
            return _cachedInstance2;
        }
        
        public static object Robotics_FindObject_Diagram_Proxy_DiagramState_TO_Robotics_FindObject_Diagram_DiagramState(object transformFrom) {
            global::Robotics.FindObject.Diagram.DiagramState target = new global::Robotics.FindObject.Diagram.DiagramState();
            global::Robotics.FindObject.Diagram.Proxy.DiagramState from = ((global::Robotics.FindObject.Diagram.Proxy.DiagramState)(transformFrom));
            target.Z = from.Z;
            return target;
        }
        
        public static object Robotics_FindObject_Diagram_DiagramState_TO_Robotics_FindObject_Diagram_Proxy_DiagramState(object transformFrom) {
            global::Robotics.FindObject.Diagram.Proxy.DiagramState target = new global::Robotics.FindObject.Diagram.Proxy.DiagramState();
            global::Robotics.FindObject.Diagram.DiagramState from = ((global::Robotics.FindObject.Diagram.DiagramState)(transformFrom));
            target.Z = from.Z;
            return target;
        }
    }
}
