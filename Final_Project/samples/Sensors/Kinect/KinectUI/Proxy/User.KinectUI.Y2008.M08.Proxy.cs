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
[assembly: global::Microsoft.Dss.Core.Attributes.ServiceDeclarationAttribute(global::Microsoft.Dss.Core.Attributes.DssServiceDeclaration.Proxy, SourceAssemblyKey="User.KinectUI.Y2008.M08, Version=0.0.0.0, Culture=neutral, PublicKeyToken=7f90740" +
    "33fd3dcf7")]
[assembly: global::System.Security.SecurityTransparentAttribute()]
[assembly: global::System.Security.SecurityRulesAttribute(global::System.Security.SecurityRuleSet.Level1)]

namespace Microsoft.Dss.Services.Samples.KinectUI.Proxy {
    
    
    /// <summary>
    ///            KinectUI state
    ///            </summary>
    [global::Microsoft.Dss.Core.Attributes.DataContractAttribute(Namespace="http://schemas.microsoft.com/2008/08/kinectui.user.html")]
    [global::System.Xml.Serialization.XmlRootAttribute(Namespace="http://schemas.microsoft.com/2008/08/kinectui.user.html", ElementName="KinectUIState")]
    [global::System.ComponentModel.DescriptionAttribute("KinectUI state")]
    public class KinectUIState : global::Microsoft.Dss.Core.IDssSerializable, global::System.ICloneable {
        
        public KinectUIState() {
        }
        
        /// <summary>
        ///Copies the data member values of the current KinectUIState to the specified target object
        ///</summary>
        ///<param name="target">target object (must be an instance of)</param>
        public virtual void CopyTo(Microsoft.Dss.Core.IDssSerializable target) {
            global::Microsoft.Dss.Services.Samples.KinectUI.Proxy.KinectUIState typedTarget = ((global::Microsoft.Dss.Services.Samples.KinectUI.Proxy.KinectUIState)(target));
        }
        
        /// <summary>
        ///Clones KinectUIState
        ///</summary>
        ///<returns>cloned value</returns>
        public virtual object Clone() {
            global::Microsoft.Dss.Services.Samples.KinectUI.Proxy.KinectUIState target0 = new global::Microsoft.Dss.Services.Samples.KinectUI.Proxy.KinectUIState();
            this.CopyTo(target0);
            return target0;
        }
        
        /// <summary>
        ///Serializes the data member values of the current KinectUIState to the specified writer
        ///</summary>
        ///<param name="writer">the writer to which to serialize</param>
        public virtual void Serialize(System.IO.BinaryWriter writer) {
        }
        
        /// <summary>
        ///Deserializes KinectUIState
        ///</summary>
        ///<param name="reader">the reader from which to deserialize</param>
        ///<returns>deserialized KinectUIState</returns>
        public virtual object Deserialize(System.IO.BinaryReader reader) {
            return this;
        }
    }
    
    /// <summary>
    ///            KinectUI get operation
    ///            </summary>
    [global::System.ComponentModel.DescriptionAttribute("KinectUI get operation")]
    [global::System.Xml.Serialization.XmlTypeAttribute(IncludeInSchema=false)]
    public class Get : global::Microsoft.Dss.ServiceModel.Dssp.Get<global::Microsoft.Dss.ServiceModel.Dssp.GetRequestType, global:: Microsoft.Ccr.Core.PortSet<global::Microsoft.Dss.Services.Samples.KinectUI.Proxy.KinectUIState, global:: W3C.Soap.Fault>> {
        
        public Get() {
        }
        
        public Get(global::Microsoft.Dss.ServiceModel.Dssp.GetRequestType body) : 
                base(body) {
        }
        
        public Get(global::Microsoft.Dss.ServiceModel.Dssp.GetRequestType body, global::Microsoft.Ccr.Core.PortSet<global::Microsoft.Dss.Services.Samples.KinectUI.Proxy.KinectUIState, global:: W3C.Soap.Fault> responsePort) : 
                base(body, responsePort) {
        }
    }
    
    /// <summary>
    ///            KinectUI main operations port
    ///            </summary>
    [global::System.ComponentModel.DescriptionAttribute("KinectUI main operations port")]
    [global::System.Xml.Serialization.XmlTypeAttribute(IncludeInSchema=false)]
    public class KinectUIOperations : global::Microsoft.Ccr.Core.PortSet<global::Microsoft.Dss.ServiceModel.Dssp.DsspDefaultLookup, global:: Microsoft.Dss.ServiceModel.Dssp.DsspDefaultDrop, global:: Microsoft.Dss.Services.Samples.KinectUI.Proxy.Get> {
        
        public KinectUIOperations() {
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
        
        public virtual global::Microsoft.Ccr.Core.PortSet<global::Microsoft.Dss.Services.Samples.KinectUI.Proxy.KinectUIState, global:: W3C.Soap.Fault> Get() {
            global::Microsoft.Dss.ServiceModel.Dssp.GetRequestType body = new global::Microsoft.Dss.ServiceModel.Dssp.GetRequestType();
            global::Microsoft.Dss.Services.Samples.KinectUI.Proxy.Get operation = new global::Microsoft.Dss.Services.Samples.KinectUI.Proxy.Get(body);
            this.Post(operation);
            return operation.ResponsePort;
        }
        
        public virtual global::Microsoft.Ccr.Core.Choice Get(out global::Microsoft.Dss.Services.Samples.KinectUI.Proxy.Get operation) {
            global::Microsoft.Dss.ServiceModel.Dssp.GetRequestType body = new global::Microsoft.Dss.ServiceModel.Dssp.GetRequestType();
            operation = new global::Microsoft.Dss.Services.Samples.KinectUI.Proxy.Get(body);
            this.Post(operation);
            return operation.ResponsePort;
        }
        
        public virtual global::Microsoft.Ccr.Core.PortSet<global::Microsoft.Dss.Services.Samples.KinectUI.Proxy.KinectUIState, global:: W3C.Soap.Fault> Get(global::Microsoft.Dss.ServiceModel.Dssp.GetRequestType body) {
            if ((body == null)) {
                body = new global::Microsoft.Dss.ServiceModel.Dssp.GetRequestType();
            }
            global::Microsoft.Dss.Services.Samples.KinectUI.Proxy.Get operation = new global::Microsoft.Dss.Services.Samples.KinectUI.Proxy.Get(body);
            this.Post(operation);
            return operation.ResponsePort;
        }
        
        public virtual global::Microsoft.Ccr.Core.Choice Get(global::Microsoft.Dss.ServiceModel.Dssp.GetRequestType body, out global::Microsoft.Dss.Services.Samples.KinectUI.Proxy.Get operation) {
            if ((body == null)) {
                body = new global::Microsoft.Dss.ServiceModel.Dssp.GetRequestType();
            }
            operation = new global::Microsoft.Dss.Services.Samples.KinectUI.Proxy.Get(body);
            this.Post(operation);
            return operation.ResponsePort;
        }
    }
    
    /// <summary>
    ///            KinectUIService - Example of using Kinect service
    ///            </summary>
    [global::System.Xml.Serialization.XmlTypeAttribute(IncludeInSchema=false)]
    [global::System.ComponentModel.DescriptionAttribute("Sample WPF user interface for the Kinect service")]
    [global::System.ComponentModel.DisplayNameAttribute("(User) WPF UI for Kinect")]
    public class Contract {
        
        public const string Identifier = "http://schemas.microsoft.com/2008/08/kinectui.user.html";
        
        /// <summary>Creates a new instance of the service.</summary>
        /// <param name="constructorServicePort">Service constructor port</param>
        /// <param name="service">service path</param>
        /// <param name="partners">the partners of the service instance</param>
        /// <returns>create service response port</returns>
        public static global::Microsoft.Dss.ServiceModel.Dssp.DsspResponsePort<Microsoft.Dss.ServiceModel.Dssp.CreateResponse> CreateService(global::Microsoft.Dss.Services.Constructor.ConstructorPort constructorServicePort, string service, params Microsoft.Dss.ServiceModel.Dssp.PartnerType[] partners) {
            global::Microsoft.Dss.ServiceModel.Dssp.DsspResponsePort<Microsoft.Dss.ServiceModel.Dssp.CreateResponse> result = new global::Microsoft.Dss.ServiceModel.Dssp.DsspResponsePort<Microsoft.Dss.ServiceModel.Dssp.CreateResponse>();
            global::Microsoft.Dss.ServiceModel.Dssp.ServiceInfoType serviceInfo = new global::Microsoft.Dss.ServiceModel.Dssp.ServiceInfoType("http://schemas.microsoft.com/2008/08/kinectui.user.html", service);
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
            global::Microsoft.Dss.ServiceModel.Dssp.ServiceInfoType serviceInfo = new global::Microsoft.Dss.ServiceModel.Dssp.ServiceInfoType("http://schemas.microsoft.com/2008/08/kinectui.user.html", null);
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
            this.KinectUIOperations = new global::Microsoft.Dss.Services.Samples.KinectUI.Proxy.KinectUIOperations();
            base.Initialize(new global::Microsoft.Dss.Core.DssOperationsPortMetadata(this.KinectUIOperations, "http://schemas.microsoft.com/2008/08/kinectui.user.html", "KinectUIOperations", ""));
        }
        
        public global::Microsoft.Dss.Services.Samples.KinectUI.Proxy.KinectUIOperations KinectUIOperations;
    }
}