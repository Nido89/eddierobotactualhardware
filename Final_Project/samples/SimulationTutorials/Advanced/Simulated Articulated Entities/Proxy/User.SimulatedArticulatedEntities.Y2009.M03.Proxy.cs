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
[assembly: global::Microsoft.Dss.Core.Attributes.ServiceDeclarationAttribute(global::Microsoft.Dss.Core.Attributes.DssServiceDeclaration.Proxy, SourceAssemblyKey="User.SimulatedArticulatedEntities.Y2009.M03, Version=0.0.0.0, Culture=neutral, Pu" +
    "blicKeyToken=7f9074033fd3dcf7")]
[assembly: global::System.Security.SecurityTransparentAttribute()]
[assembly: global::System.Security.SecurityRulesAttribute(global::System.Security.SecurityRuleSet.Level1)]

namespace Robotics.SimulatedArticulatedEntities.Proxy {
    
    
    [global::Microsoft.Dss.Core.Attributes.DataContractAttribute(Namespace="http://schemas.microsoft.com/2009/03/simulationarticulatedentities.user.html")]
    [global::System.Xml.Serialization.XmlRootAttribute(Namespace="http://schemas.microsoft.com/2009/03/simulationarticulatedentities.user.html", ElementName="State")]
    public class State : global::Microsoft.Dss.Core.IDssSerializable, global::System.ICloneable {
        
        public State() {
        }
        
        private string _ActiveJoint;
        
        [global::Microsoft.Dss.Core.Attributes.DataMemberAttribute(Order=-1)]
        public string ActiveJoint {
            get {
                return this._ActiveJoint;
            }
            set {
                this._ActiveJoint = value;
            }
        }
        
        /// <summary>
        ///Copies the data member values of the current State to the specified target object
        ///</summary>
        ///<param name="target">target object (must be an instance of)</param>
        public virtual void CopyTo(Microsoft.Dss.Core.IDssSerializable target) {
            global::Robotics.SimulatedArticulatedEntities.Proxy.State typedTarget = ((global::Robotics.SimulatedArticulatedEntities.Proxy.State)(target));
            typedTarget._ActiveJoint = this._ActiveJoint;
        }
        
        /// <summary>
        ///Clones State
        ///</summary>
        ///<returns>cloned value</returns>
        public virtual object Clone() {
            global::Robotics.SimulatedArticulatedEntities.Proxy.State target0 = new global::Robotics.SimulatedArticulatedEntities.Proxy.State();
            this.CopyTo(target0);
            return target0;
        }
        
        /// <summary>
        ///Serializes the data member values of the current State to the specified writer
        ///</summary>
        ///<param name="writer">the writer to which to serialize</param>
        public virtual void Serialize(System.IO.BinaryWriter writer) {
            if ((this._ActiveJoint == null)) {
                writer.Write(((byte)(0)));
            }
            else {
                writer.Write(((byte)(1)));
                writer.Write(this._ActiveJoint);
            }
        }
        
        /// <summary>
        ///Deserializes State
        ///</summary>
        ///<param name="reader">the reader from which to deserialize</param>
        ///<returns>deserialized State</returns>
        public virtual object Deserialize(System.IO.BinaryReader reader) {
            if ((reader.ReadByte() != 0)) {
                this._ActiveJoint = reader.ReadString();
            }
            return this;
        }
    }
    
    [global::System.Xml.Serialization.XmlTypeAttribute(IncludeInSchema=false)]
    public class Get : global::Microsoft.Dss.ServiceModel.Dssp.Get<global::Microsoft.Dss.ServiceModel.Dssp.GetRequestType, global:: Microsoft.Ccr.Core.PortSet<global::Robotics.SimulatedArticulatedEntities.Proxy.State, global:: W3C.Soap.Fault>> {
        
        public Get() {
        }
        
        public Get(global::Microsoft.Dss.ServiceModel.Dssp.GetRequestType body) : 
                base(body) {
        }
        
        public Get(global::Microsoft.Dss.ServiceModel.Dssp.GetRequestType body, global::Microsoft.Ccr.Core.PortSet<global::Robotics.SimulatedArticulatedEntities.Proxy.State, global:: W3C.Soap.Fault> responsePort) : 
                base(body, responsePort) {
        }
    }
    
    [global::System.Xml.Serialization.XmlTypeAttribute(IncludeInSchema=false)]
    public class Replace : global::Microsoft.Dss.ServiceModel.Dssp.Replace<global::Robotics.SimulatedArticulatedEntities.Proxy.State, global:: Microsoft.Ccr.Core.PortSet<global::Microsoft.Dss.ServiceModel.Dssp.DefaultReplaceResponseType, global:: W3C.Soap.Fault>> {
        
        public Replace() {
        }
        
        public Replace(global::Robotics.SimulatedArticulatedEntities.Proxy.State body) : 
                base(body) {
        }
        
        public Replace(global::Robotics.SimulatedArticulatedEntities.Proxy.State body, global::Microsoft.Ccr.Core.PortSet<global::Microsoft.Dss.ServiceModel.Dssp.DefaultReplaceResponseType, global:: W3C.Soap.Fault> responsePort) : 
                base(body, responsePort) {
        }
    }
    
    [global::System.Xml.Serialization.XmlTypeAttribute(IncludeInSchema=false)]
    public class SimulationTutorial4Operations : global::Microsoft.Ccr.Core.PortSet<global::Microsoft.Dss.ServiceModel.Dssp.DsspDefaultLookup, global:: Microsoft.Dss.ServiceModel.Dssp.DsspDefaultDrop, global:: Robotics.SimulatedArticulatedEntities.Proxy.Get, global:: Robotics.SimulatedArticulatedEntities.Proxy.Replace> {
        
        public SimulationTutorial4Operations() {
        }
        
        public virtual global::Microsoft.Ccr.Core.PortSet<global::Microsoft.Dss.ServiceModel.Dssp.LookupResponse, global::W3C.Soap.Fault> DsspDefaultLookup() {
            global::Microsoft.Dss.ServiceModel.Dssp.LookupRequestType body = new global::Microsoft.Dss.ServiceModel.Dssp.LookupRequestType();
            global::Microsoft.Dss.ServiceModel.Dssp.DsspDefaultLookup operation = new global::Microsoft.Dss.ServiceModel.Dssp.DsspDefaultLookup(body);
            this.Post(operation);
            return operation.ResponsePort;
        }
        
        public virtual global::Microsoft.Ccr.Core.Choice DsspDefaultLookup(out global::Microsoft.Dss.ServiceModel.Dssp.DsspDefaultLookup operation) {
            global::Microsoft.Dss.ServiceModel.Dssp.LookupRequestType body = new global::Microsoft.Dss.ServiceModel.Dssp.LookupRequestType();
            operation = new global::Microsoft.Dss.ServiceModel.Dssp.DsspDefaultLookup(body);
            this.Post(operation);
            return operation.ResponsePort;
        }
        
        public virtual global::Microsoft.Ccr.Core.PortSet<global::Microsoft.Dss.ServiceModel.Dssp.LookupResponse, global::W3C.Soap.Fault> DsspDefaultLookup(global::Microsoft.Dss.ServiceModel.Dssp.LookupRequestType body) {
            if ((body == null)) {
                body = new global::Microsoft.Dss.ServiceModel.Dssp.LookupRequestType();
            }
            global::Microsoft.Dss.ServiceModel.Dssp.DsspDefaultLookup operation = new global::Microsoft.Dss.ServiceModel.Dssp.DsspDefaultLookup(body);
            this.Post(operation);
            return operation.ResponsePort;
        }
        
        public virtual global::Microsoft.Ccr.Core.Choice DsspDefaultLookup(global::Microsoft.Dss.ServiceModel.Dssp.LookupRequestType body, out global::Microsoft.Dss.ServiceModel.Dssp.DsspDefaultLookup operation) {
            if ((body == null)) {
                body = new global::Microsoft.Dss.ServiceModel.Dssp.LookupRequestType();
            }
            operation = new global::Microsoft.Dss.ServiceModel.Dssp.DsspDefaultLookup(body);
            this.Post(operation);
            return operation.ResponsePort;
        }
        
        public virtual global::Microsoft.Ccr.Core.PortSet<global::Microsoft.Dss.ServiceModel.Dssp.DefaultDropResponseType, global::W3C.Soap.Fault> DsspDefaultDrop() {
            global::Microsoft.Dss.ServiceModel.Dssp.DropRequestType body = new global::Microsoft.Dss.ServiceModel.Dssp.DropRequestType();
            global::Microsoft.Dss.ServiceModel.Dssp.DsspDefaultDrop operation = new global::Microsoft.Dss.ServiceModel.Dssp.DsspDefaultDrop(body);
            this.Post(operation);
            return operation.ResponsePort;
        }
        
        public virtual global::Microsoft.Ccr.Core.Choice DsspDefaultDrop(out global::Microsoft.Dss.ServiceModel.Dssp.DsspDefaultDrop operation) {
            global::Microsoft.Dss.ServiceModel.Dssp.DropRequestType body = new global::Microsoft.Dss.ServiceModel.Dssp.DropRequestType();
            operation = new global::Microsoft.Dss.ServiceModel.Dssp.DsspDefaultDrop(body);
            this.Post(operation);
            return operation.ResponsePort;
        }
        
        public virtual global::Microsoft.Ccr.Core.PortSet<global::Microsoft.Dss.ServiceModel.Dssp.DefaultDropResponseType, global::W3C.Soap.Fault> DsspDefaultDrop(global::Microsoft.Dss.ServiceModel.Dssp.DropRequestType body) {
            if ((body == null)) {
                body = new global::Microsoft.Dss.ServiceModel.Dssp.DropRequestType();
            }
            global::Microsoft.Dss.ServiceModel.Dssp.DsspDefaultDrop operation = new global::Microsoft.Dss.ServiceModel.Dssp.DsspDefaultDrop(body);
            this.Post(operation);
            return operation.ResponsePort;
        }
        
        public virtual global::Microsoft.Ccr.Core.Choice DsspDefaultDrop(global::Microsoft.Dss.ServiceModel.Dssp.DropRequestType body, out global::Microsoft.Dss.ServiceModel.Dssp.DsspDefaultDrop operation) {
            if ((body == null)) {
                body = new global::Microsoft.Dss.ServiceModel.Dssp.DropRequestType();
            }
            operation = new global::Microsoft.Dss.ServiceModel.Dssp.DsspDefaultDrop(body);
            this.Post(operation);
            return operation.ResponsePort;
        }
        
        public virtual global::Microsoft.Ccr.Core.PortSet<global::Robotics.SimulatedArticulatedEntities.Proxy.State, global:: W3C.Soap.Fault> Get() {
            global::Microsoft.Dss.ServiceModel.Dssp.GetRequestType body = new global::Microsoft.Dss.ServiceModel.Dssp.GetRequestType();
            global::Robotics.SimulatedArticulatedEntities.Proxy.Get operation = new global::Robotics.SimulatedArticulatedEntities.Proxy.Get(body);
            this.Post(operation);
            return operation.ResponsePort;
        }
        
        public virtual global::Microsoft.Ccr.Core.Choice Get(out global::Robotics.SimulatedArticulatedEntities.Proxy.Get operation) {
            global::Microsoft.Dss.ServiceModel.Dssp.GetRequestType body = new global::Microsoft.Dss.ServiceModel.Dssp.GetRequestType();
            operation = new global::Robotics.SimulatedArticulatedEntities.Proxy.Get(body);
            this.Post(operation);
            return operation.ResponsePort;
        }
        
        public virtual global::Microsoft.Ccr.Core.PortSet<global::Robotics.SimulatedArticulatedEntities.Proxy.State, global:: W3C.Soap.Fault> Get(global::Microsoft.Dss.ServiceModel.Dssp.GetRequestType body) {
            if ((body == null)) {
                body = new global::Microsoft.Dss.ServiceModel.Dssp.GetRequestType();
            }
            global::Robotics.SimulatedArticulatedEntities.Proxy.Get operation = new global::Robotics.SimulatedArticulatedEntities.Proxy.Get(body);
            this.Post(operation);
            return operation.ResponsePort;
        }
        
        public virtual global::Microsoft.Ccr.Core.Choice Get(global::Microsoft.Dss.ServiceModel.Dssp.GetRequestType body, out global::Robotics.SimulatedArticulatedEntities.Proxy.Get operation) {
            if ((body == null)) {
                body = new global::Microsoft.Dss.ServiceModel.Dssp.GetRequestType();
            }
            operation = new global::Robotics.SimulatedArticulatedEntities.Proxy.Get(body);
            this.Post(operation);
            return operation.ResponsePort;
        }
        
        public virtual global::Microsoft.Ccr.Core.PortSet<global::Microsoft.Dss.ServiceModel.Dssp.DefaultReplaceResponseType, global:: W3C.Soap.Fault> Replace() {
            global::Robotics.SimulatedArticulatedEntities.Proxy.State body = new global::Robotics.SimulatedArticulatedEntities.Proxy.State();
            global::Robotics.SimulatedArticulatedEntities.Proxy.Replace operation = new global::Robotics.SimulatedArticulatedEntities.Proxy.Replace(body);
            this.Post(operation);
            return operation.ResponsePort;
        }
        
        public virtual global::Microsoft.Ccr.Core.Choice Replace(out global::Robotics.SimulatedArticulatedEntities.Proxy.Replace operation) {
            global::Robotics.SimulatedArticulatedEntities.Proxy.State body = new global::Robotics.SimulatedArticulatedEntities.Proxy.State();
            operation = new global::Robotics.SimulatedArticulatedEntities.Proxy.Replace(body);
            this.Post(operation);
            return operation.ResponsePort;
        }
        
        public virtual global::Microsoft.Ccr.Core.PortSet<global::Microsoft.Dss.ServiceModel.Dssp.DefaultReplaceResponseType, global:: W3C.Soap.Fault> Replace(global::Robotics.SimulatedArticulatedEntities.Proxy.State body) {
            if ((body == null)) {
                body = new global::Robotics.SimulatedArticulatedEntities.Proxy.State();
            }
            global::Robotics.SimulatedArticulatedEntities.Proxy.Replace operation = new global::Robotics.SimulatedArticulatedEntities.Proxy.Replace(body);
            this.Post(operation);
            return operation.ResponsePort;
        }
        
        public virtual global::Microsoft.Ccr.Core.Choice Replace(global::Robotics.SimulatedArticulatedEntities.Proxy.State body, out global::Robotics.SimulatedArticulatedEntities.Proxy.Replace operation) {
            if ((body == null)) {
                body = new global::Robotics.SimulatedArticulatedEntities.Proxy.State();
            }
            operation = new global::Robotics.SimulatedArticulatedEntities.Proxy.Replace(body);
            this.Post(operation);
            return operation.ResponsePort;
        }
    }
    
    [global::System.Xml.Serialization.XmlTypeAttribute(IncludeInSchema=false)]
    [global::System.ComponentModel.DescriptionAttribute("Simulated Articulated Entities Service")]
    [global::System.ComponentModel.DisplayNameAttribute("(User) Simulated Articulated Entities")]
    public class Contract {
        
        public const string Identifier = "http://schemas.microsoft.com/2009/03/simulationarticulatedentities.user.html";
        
        /// <summary>Creates a new instance of the service.</summary>
        /// <param name="constructorServicePort">Service constructor port</param>
        /// <param name="service">service path</param>
        /// <param name="partners">the partners of the service instance</param>
        /// <returns>create service response port</returns>
        public static global::Microsoft.Dss.ServiceModel.Dssp.DsspResponsePort<Microsoft.Dss.ServiceModel.Dssp.CreateResponse> CreateService(global::Microsoft.Dss.Services.Constructor.ConstructorPort constructorServicePort, string service, params Microsoft.Dss.ServiceModel.Dssp.PartnerType[] partners) {
            global::Microsoft.Dss.ServiceModel.Dssp.DsspResponsePort<Microsoft.Dss.ServiceModel.Dssp.CreateResponse> result = new global::Microsoft.Dss.ServiceModel.Dssp.DsspResponsePort<Microsoft.Dss.ServiceModel.Dssp.CreateResponse>();
            global::Microsoft.Dss.ServiceModel.Dssp.ServiceInfoType serviceInfo = new global::Microsoft.Dss.ServiceModel.Dssp.ServiceInfoType("http://schemas.microsoft.com/2009/03/simulationarticulatedentities.user.html", service);
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
            global::Microsoft.Dss.ServiceModel.Dssp.ServiceInfoType serviceInfo = new global::Microsoft.Dss.ServiceModel.Dssp.ServiceInfoType("http://schemas.microsoft.com/2009/03/simulationarticulatedentities.user.html", null);
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
            this.SimulationTutorial4Operations = new global::Robotics.SimulatedArticulatedEntities.Proxy.SimulationTutorial4Operations();
            base.Initialize(new global::Microsoft.Dss.Core.DssOperationsPortMetadata(this.SimulationTutorial4Operations, "http://schemas.microsoft.com/2009/03/simulationarticulatedentities.user.html", "SimulationTutorial4Operations", ""));
        }
        
        public global::Robotics.SimulatedArticulatedEntities.Proxy.SimulationTutorial4Operations SimulationTutorial4Operations;
    }
}
