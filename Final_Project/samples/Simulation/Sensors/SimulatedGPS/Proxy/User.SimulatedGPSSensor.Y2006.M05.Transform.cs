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
[assembly: global::Microsoft.Dss.Core.Attributes.ServiceDeclarationAttribute(global::Microsoft.Dss.Core.Attributes.DssServiceDeclaration.Transform, SourceAssemblyKey="User.SimulatedGPSSensor.Y2006.M05, Version=0.0.0.0, Culture=neutral, PublicKeyTok" +
    "en=7f9074033fd3dcf7")]
[assembly: global::System.Security.SecurityTransparentAttribute()]
[assembly: global::System.Security.SecurityRulesAttribute(global::System.Security.SecurityRuleSet.Level1)]

namespace Dss.Transforms.TransformUser {
    
    
    public class Transforms : global::Microsoft.Dss.Core.Transforms.TransformBase {
        
        static Transforms() {
            Register();
        }
        
        public static void Register() {
            global::Microsoft.Dss.Core.Transforms.TransformBase.AddProxyTransform(typeof(global::Microsoft.Robotics.Services.Simulation.Sensors.GPSSensor.Proxy.GPSSensorState), new global::Microsoft.Dss.Core.Attributes.Transform(Microsoft_Robotics_Services_Simulation_Sensors_GPSSensor_Proxy_GPSSensorState_TO_Microsoft_Robotics_Services_Simulation_Sensors_GPSSensor_GPSSensorState));
            global::Microsoft.Dss.Core.Transforms.TransformBase.AddSourceTransform(typeof(global::Microsoft.Robotics.Services.Simulation.Sensors.GPSSensor.GPSSensorState), new global::Microsoft.Dss.Core.Attributes.Transform(Microsoft_Robotics_Services_Simulation_Sensors_GPSSensor_GPSSensorState_TO_Microsoft_Robotics_Services_Simulation_Sensors_GPSSensor_Proxy_GPSSensorState));
        }
        
        public static object Microsoft_Robotics_Services_Simulation_Sensors_GPSSensor_Proxy_GPSSensorState_TO_Microsoft_Robotics_Services_Simulation_Sensors_GPSSensor_GPSSensorState(object transformFrom) {
            global::Microsoft.Robotics.Services.Simulation.Sensors.GPSSensor.GPSSensorState target = new global::Microsoft.Robotics.Services.Simulation.Sensors.GPSSensor.GPSSensorState();
            global::Microsoft.Robotics.Services.Simulation.Sensors.GPSSensor.Proxy.GPSSensorState from = ((global::Microsoft.Robotics.Services.Simulation.Sensors.GPSSensor.Proxy.GPSSensorState)(transformFrom));
            target.X = from.X;
            target.Y = from.Y;
            target.Z = from.Z;
            target.TimeStamp = from.TimeStamp;
            return target;
        }
        
        public static object Microsoft_Robotics_Services_Simulation_Sensors_GPSSensor_GPSSensorState_TO_Microsoft_Robotics_Services_Simulation_Sensors_GPSSensor_Proxy_GPSSensorState(object transformFrom) {
            global::Microsoft.Robotics.Services.Simulation.Sensors.GPSSensor.Proxy.GPSSensorState target = new global::Microsoft.Robotics.Services.Simulation.Sensors.GPSSensor.Proxy.GPSSensorState();
            global::Microsoft.Robotics.Services.Simulation.Sensors.GPSSensor.GPSSensorState from = ((global::Microsoft.Robotics.Services.Simulation.Sensors.GPSSensor.GPSSensorState)(transformFrom));
            target.X = from.X;
            target.Y = from.Y;
            target.Z = from.Z;
            target.TimeStamp = from.TimeStamp;
            return target;
        }
    }
}
