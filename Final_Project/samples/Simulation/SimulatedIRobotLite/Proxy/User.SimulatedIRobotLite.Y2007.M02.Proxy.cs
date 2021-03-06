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
[assembly: global::Microsoft.Dss.Core.Attributes.ServiceDeclarationAttribute(global::Microsoft.Dss.Core.Attributes.DssServiceDeclaration.Proxy, SourceAssemblyKey="User.SimulatedIRobotLite.Y2007.M02, Version=0.0.0.0, Culture=neutral, PublicKeyTo" +
    "ken=7f9074033fd3dcf7")]
[assembly: global::System.Security.SecurityTransparentAttribute()]
[assembly: global::System.Security.SecurityRulesAttribute(global::System.Security.SecurityRuleSet.Level1)]

namespace Microsoft.Robotics.Services.Sample.SimulatedIRobotLite.Proxy {
    
    
    /// <summary>
    ///            An IR distance sensor.
    ///            </summary>
    [global::Microsoft.Dss.Core.Attributes.DataContractAttribute(Namespace="http://schemas.microsoft.com/robotics/2007/02/simulatedirobotlite.user.html")]
    [global::System.Xml.Serialization.XmlRootAttribute(Namespace="http://schemas.microsoft.com/robotics/2007/02/simulatedirobotlite.user.html", ElementName="IREntity")]
    [global::System.ComponentModel.DescriptionAttribute("An IR distance sensor.")]
    public class IREntity : global::Microsoft.Robotics.Simulation.Engine.Proxy.VisualEntity, global::Microsoft.Dss.Core.IDssSerializable, global::System.ICloneable {
        
        public IREntity() {
        }
        
        private float _Distance;
        
        /// <summary>
        ///            The shortest distance to an impact point
        ///            </summary>
        [global::Microsoft.Dss.Core.Attributes.DataMemberAttribute(Order=-1)]
        [global::System.ComponentModel.DescriptionAttribute("The shortest distance to an impact point")]
        public float Distance {
            get {
                return this._Distance;
            }
            set {
                this._Distance = value;
            }
        }
        
        private float _DispersionConeAngle;
        
        /// <summary>
        ///            angle is in degrees
        ///            </summary>
        [global::Microsoft.Dss.Core.Attributes.DataMemberAttribute(Order=-1)]
        [global::System.ComponentModel.DescriptionAttribute("angle is in degrees")]
        public float DispersionConeAngle {
            get {
                return this._DispersionConeAngle;
            }
            set {
                this._DispersionConeAngle = value;
            }
        }
        
        private float _Samples;
        
        /// <summary>
        ///            the number of rays in each direction
        ///            </summary>
        [global::Microsoft.Dss.Core.Attributes.DataMemberAttribute(Order=-1)]
        [global::System.ComponentModel.DescriptionAttribute("the number of rays in each direction")]
        public float Samples {
            get {
                return this._Samples;
            }
            set {
                this._Samples = value;
            }
        }
        
        private float _MaximumRange;
        
        /// <summary>
        ///            Default value is 30 inches converted to meters
        ///            </summary>
        [global::Microsoft.Dss.Core.Attributes.DataMemberAttribute(Order=-1)]
        [global::System.ComponentModel.DescriptionAttribute("Default value is 30 inches converted to meters")]
        public float MaximumRange {
            get {
                return this._MaximumRange;
            }
            set {
                this._MaximumRange = value;
            }
        }
        
        /// <summary>
        ///Copies the data member values of the current IREntity to the specified target object
        ///</summary>
        ///<param name="target">target object (must be an instance of)</param>
        public override void CopyTo(Microsoft.Dss.Core.IDssSerializable target) {
            base.CopyTo(target);
            global::Microsoft.Robotics.Services.Sample.SimulatedIRobotLite.Proxy.IREntity typedTarget = ((global::Microsoft.Robotics.Services.Sample.SimulatedIRobotLite.Proxy.IREntity)(target));
            typedTarget._Distance = this._Distance;
            typedTarget._DispersionConeAngle = this._DispersionConeAngle;
            typedTarget._Samples = this._Samples;
            typedTarget._MaximumRange = this._MaximumRange;
        }
        
        /// <summary>
        ///Clones IREntity
        ///</summary>
        ///<returns>cloned value</returns>
        public override object Clone() {
            global::Microsoft.Robotics.Services.Sample.SimulatedIRobotLite.Proxy.IREntity target0 = new global::Microsoft.Robotics.Services.Sample.SimulatedIRobotLite.Proxy.IREntity();
            this.CopyTo(target0);
            return target0;
        }
        
        /// <summary>
        ///Serializes the data member values of the current IREntity to the specified writer
        ///</summary>
        ///<param name="writer">the writer to which to serialize</param>
        public override void Serialize(System.IO.BinaryWriter writer) {
            base.Serialize(writer);
            writer.Write(this._Distance);
            writer.Write(this._DispersionConeAngle);
            writer.Write(this._Samples);
            writer.Write(this._MaximumRange);
        }
        
        /// <summary>
        ///Deserializes IREntity
        ///</summary>
        ///<param name="reader">the reader from which to deserialize</param>
        ///<returns>deserialized IREntity</returns>
        public override object Deserialize(System.IO.BinaryReader reader) {
            base.Deserialize(reader);
            this._Distance = reader.ReadSingle();
            this._DispersionConeAngle = reader.ReadSingle();
            this._Samples = reader.ReadSingle();
            this._MaximumRange = reader.ReadSingle();
            return this;
        }
    }
    
    /// <summary>
    ///            Simulated iRobot Lite Service
    ///            </summary>
    [global::System.Xml.Serialization.XmlTypeAttribute(IncludeInSchema=false)]
    [global::System.ComponentModel.DescriptionAttribute("Provides direct access to the iRobot Create Lite Simulation service which contain" +
        "s a reduced subset of the iRobot Create commands and is available in the simulat" +
        "ion environment.")]
    [global::System.ComponentModel.DisplayNameAttribute("(User) iRobot� Create Lite Simulation")]
    public class Contract {
        
        public const string Identifier = "http://schemas.microsoft.com/robotics/2007/02/simulatedirobotlite.user.html";
        
        /// <summary>Creates a new instance of the service.</summary>
        /// <param name="constructorServicePort">Service constructor port</param>
        /// <param name="service">service path</param>
        /// <param name="partners">the partners of the service instance</param>
        /// <returns>create service response port</returns>
        public static global::Microsoft.Dss.ServiceModel.Dssp.DsspResponsePort<Microsoft.Dss.ServiceModel.Dssp.CreateResponse> CreateService(global::Microsoft.Dss.Services.Constructor.ConstructorPort constructorServicePort, string service, params Microsoft.Dss.ServiceModel.Dssp.PartnerType[] partners) {
            global::Microsoft.Dss.ServiceModel.Dssp.DsspResponsePort<Microsoft.Dss.ServiceModel.Dssp.CreateResponse> result = new global::Microsoft.Dss.ServiceModel.Dssp.DsspResponsePort<Microsoft.Dss.ServiceModel.Dssp.CreateResponse>();
            global::Microsoft.Dss.ServiceModel.Dssp.ServiceInfoType serviceInfo = new global::Microsoft.Dss.ServiceModel.Dssp.ServiceInfoType("http://schemas.microsoft.com/robotics/2007/02/simulatedirobotlite.user.html", service);
            if ((partners != null)) {
                serviceInfo.PartnerList = new System.Collections.Generic.List<Microsoft.Dss.ServiceModel.Dssp.PartnerType>(partners);
            }
            global::Microsoft.Dss.Services.Constructor.Create create = new global::Microsoft.Dss.Services.Constructor.Create(serviceInfo, result);
            constructorServicePort.Post(create);
            return result;
        }
        
        /// <summary>Creates a new instance of the service.</summary>
        /// <param name="constructorServicePort">Service constructor port</param>
        /// <param name="partners">the partners of the service instance</param>
        /// <returns>create service response port</returns>
        public static global::Microsoft.Dss.ServiceModel.Dssp.DsspResponsePort<Microsoft.Dss.ServiceModel.Dssp.CreateResponse> CreateService(global::Microsoft.Dss.Services.Constructor.ConstructorPort constructorServicePort, params Microsoft.Dss.ServiceModel.Dssp.PartnerType[] partners) {
            global::Microsoft.Dss.ServiceModel.Dssp.DsspResponsePort<Microsoft.Dss.ServiceModel.Dssp.CreateResponse> result = new global::Microsoft.Dss.ServiceModel.Dssp.DsspResponsePort<Microsoft.Dss.ServiceModel.Dssp.CreateResponse>();
            global::Microsoft.Dss.ServiceModel.Dssp.ServiceInfoType serviceInfo = new global::Microsoft.Dss.ServiceModel.Dssp.ServiceInfoType("http://schemas.microsoft.com/robotics/2007/02/simulatedirobotlite.user.html", null);
            if ((partners != null)) {
                serviceInfo.PartnerList = new System.Collections.Generic.List<Microsoft.Dss.ServiceModel.Dssp.PartnerType>(partners);
            }
            global::Microsoft.Dss.Services.Constructor.Create create = new global::Microsoft.Dss.Services.Constructor.Create(serviceInfo, result);
            constructorServicePort.Post(create);
            return result;
        }
    }
    
    public class CombinedOperationsPort : global::Microsoft.Dss.Core.DssCombinedOperationsPort {
        
        public CombinedOperationsPort() {
            this.IRobotLiteOperations = new global::Microsoft.Robotics.Services.IRobot.Lite.Proxy.IRobotLiteOperations();
            base.Initialize(new global::Microsoft.Dss.Core.DssOperationsPortMetadata(this.IRobotLiteOperations, "http://schemas.microsoft.com/robotics/2007/02/simulatedirobotlite.user.html", "IRobotLiteOperations", ""));
        }
        
        public global::Microsoft.Robotics.Services.IRobot.Lite.Proxy.IRobotLiteOperations IRobotLiteOperations;
    }
}
