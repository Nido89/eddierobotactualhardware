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
[assembly: global::Microsoft.Dss.Core.Attributes.ServiceDeclarationAttribute(global::Microsoft.Dss.Core.Attributes.DssServiceDeclaration.Proxy, SourceAssemblyKey="User.SimulatedGPSSensor.Y2006.M05, Version=0.0.0.0, Culture=neutral, PublicKeyTok" +
    "en=7f9074033fd3dcf7")]
[assembly: global::System.Security.SecurityTransparentAttribute()]
[assembly: global::System.Security.SecurityRulesAttribute(global::System.Security.SecurityRuleSet.Level1)]

namespace Microsoft.Robotics.Services.Simulation.Sensors.GPSSensor.Proxy {
    
    
    /// <summary>
    ///            GPSSensor state
    ///            </summary>
    [global::Microsoft.Dss.Core.Attributes.DataContractAttribute(Namespace="http://schemas.microsoft.com/2008/11/simgpssensor.user.html")]
    [global::System.Xml.Serialization.XmlRootAttribute(Namespace="http://schemas.microsoft.com/2008/11/simgpssensor.user.html", ElementName="GPSSensorState")]
    [global::System.ComponentModel.DescriptionAttribute("GPSSensor state")]
    public class GPSSensorState : global::Microsoft.Dss.Core.IDssSerializable, global::System.ICloneable {
        
        public GPSSensorState() {
        }
        
        private double _X;
        
        /// <summary>
        ///            X component
        ///            </summary>
        [global::Microsoft.Dss.Core.Attributes.DataMemberAttribute(Order=-1)]
        [global::System.ComponentModel.DescriptionAttribute("X component")]
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public double X {
            get {
                return this._X;
            }
            set {
                this._X = value;
            }
        }
        
        private double _Y;
        
        /// <summary>
        ///            Y component
        ///            </summary>
        [global::Microsoft.Dss.Core.Attributes.DataMemberAttribute(Order=-1)]
        [global::System.ComponentModel.DescriptionAttribute("Y component")]
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public double Y {
            get {
                return this._Y;
            }
            set {
                this._Y = value;
            }
        }
        
        private double _Z;
        
        /// <summary>
        ///            Z component
        ///            </summary>
        [global::Microsoft.Dss.Core.Attributes.DataMemberAttribute(Order=-1)]
        [global::System.ComponentModel.DescriptionAttribute("Z component")]
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public double Z {
            get {
                return this._Z;
            }
            set {
                this._Z = value;
            }
        }
        
        private global::System.DateTime _TimeStamp;
        
        /// <summary>
        ///            Timestamp of this sample
        ///            </summary>
        [global::Microsoft.Dss.Core.Attributes.DataMemberAttribute(Order=-1, XmlOmitDefaultValue=true)]
        [global::System.ComponentModel.DescriptionAttribute("Indicates the timestamp of the sensor reading.")]
        [global::System.ComponentModel.BrowsableAttribute(false)]
        [global::System.ComponentModel.DefaultValueAttribute(typeof(global::System.DateTime), "0001-01-01T00:00:00")]
        public global::System.DateTime TimeStamp {
            get {
                return this._TimeStamp;
            }
            set {
                this._TimeStamp = value;
            }
        }
        
        /// <summary>
        ///Copies the data member values of the current GPSSensorState to the specified target object
        ///</summary>
        ///<param name="target">target object (must be an instance of)</param>
        public virtual void CopyTo(Microsoft.Dss.Core.IDssSerializable target) {
            global::Microsoft.Robotics.Services.Simulation.Sensors.GPSSensor.Proxy.GPSSensorState typedTarget = ((global::Microsoft.Robotics.Services.Simulation.Sensors.GPSSensor.Proxy.GPSSensorState)(target));
            typedTarget._X = this._X;
            typedTarget._Y = this._Y;
            typedTarget._Z = this._Z;
            typedTarget._TimeStamp = this._TimeStamp;
        }
        
        /// <summary>
        ///Clones GPSSensorState
        ///</summary>
        ///<returns>cloned value</returns>
        public virtual object Clone() {
            global::Microsoft.Robotics.Services.Simulation.Sensors.GPSSensor.Proxy.GPSSensorState target0 = new global::Microsoft.Robotics.Services.Simulation.Sensors.GPSSensor.Proxy.GPSSensorState();
            this.CopyTo(target0);
            return target0;
        }
        
        /// <summary>
        ///Serializes the data member values of the current GPSSensorState to the specified writer
        ///</summary>
        ///<param name="writer">the writer to which to serialize</param>
        public virtual void Serialize(System.IO.BinaryWriter writer) {
            writer.Write(this._X);
            writer.Write(this._Y);
            writer.Write(this._Z);
            global::Microsoft.Dss.Services.Serializer.BinarySerializationHelper.SerializeDateTime(this._TimeStamp, writer);
        }
        
        /// <summary>
        ///Deserializes GPSSensorState
        ///</summary>
        ///<param name="reader">the reader from which to deserialize</param>
        ///<returns>deserialized GPSSensorState</returns>
        public virtual object Deserialize(System.IO.BinaryReader reader) {
            this._X = reader.ReadDouble();
            this._Y = reader.ReadDouble();
            this._Z = reader.ReadDouble();
            this._TimeStamp = global::Microsoft.Dss.Services.Serializer.BinarySerializationHelper.DeserializeDateTime(reader);
            return this;
        }
    }
    
    /// <summary>
    ///            GPSSensor get operation
    ///            </summary>
    [global::System.ComponentModel.DescriptionAttribute("GPSSensor get operation")]
    [global::System.Xml.Serialization.XmlTypeAttribute(IncludeInSchema=false)]
    public class Get : global::Microsoft.Dss.ServiceModel.Dssp.Get<global::Microsoft.Dss.ServiceModel.Dssp.GetRequestType, global:: Microsoft.Ccr.Core.PortSet<global::Microsoft.Robotics.Services.Simulation.Sensors.GPSSensor.Proxy.GPSSensorState, global:: W3C.Soap.Fault>> {
        
        public Get() {
        }
        
        public Get(global::Microsoft.Dss.ServiceModel.Dssp.GetRequestType body) : 
                base(body) {
        }
        
        public Get(global::Microsoft.Dss.ServiceModel.Dssp.GetRequestType body, global::Microsoft.Ccr.Core.PortSet<global::Microsoft.Robotics.Services.Simulation.Sensors.GPSSensor.Proxy.GPSSensorState, global:: W3C.Soap.Fault> responsePort) : 
                base(body, responsePort) {
        }
    }
    
    /// <summary>
    ///            GPSSensor subscribe operation
    ///            </summary>
    [global::System.ComponentModel.DescriptionAttribute("GPSSensor subscribe operation")]
    [global::System.Xml.Serialization.XmlTypeAttribute(IncludeInSchema=false)]
    public class Subscribe : global::Microsoft.Dss.ServiceModel.Dssp.Subscribe<global::Microsoft.Dss.ServiceModel.Dssp.SubscribeRequestType, global:: Microsoft.Ccr.Core.PortSet<global::Microsoft.Dss.ServiceModel.Dssp.SubscribeResponseType, global:: W3C.Soap.Fault>> {
        
        public Subscribe() {
        }
        
        public Subscribe(global::Microsoft.Dss.ServiceModel.Dssp.SubscribeRequestType body) : 
                base(body) {
        }
        
        public Subscribe(global::Microsoft.Dss.ServiceModel.Dssp.SubscribeRequestType body, global::Microsoft.Ccr.Core.PortSet<global::Microsoft.Dss.ServiceModel.Dssp.SubscribeResponseType, global:: W3C.Soap.Fault> responsePort) : 
                base(body, responsePort) {
        }
    }
    
    /// <summary>
    ///            GPSSensor replace operation
    ///            </summary>
    [global::System.ComponentModel.DescriptionAttribute("GPSSensor replace operation")]
    [global::System.Xml.Serialization.XmlTypeAttribute(IncludeInSchema=false)]
    public class Replace : global::Microsoft.Dss.ServiceModel.Dssp.Replace<global::Microsoft.Robotics.Services.Simulation.Sensors.GPSSensor.Proxy.GPSSensorState, global:: Microsoft.Ccr.Core.PortSet<global::Microsoft.Dss.ServiceModel.Dssp.DefaultReplaceResponseType, global:: W3C.Soap.Fault>> {
        
        public Replace() {
        }
        
        public Replace(global::Microsoft.Robotics.Services.Simulation.Sensors.GPSSensor.Proxy.GPSSensorState body) : 
                base(body) {
        }
        
        public Replace(global::Microsoft.Robotics.Services.Simulation.Sensors.GPSSensor.Proxy.GPSSensorState body, global::Microsoft.Ccr.Core.PortSet<global::Microsoft.Dss.ServiceModel.Dssp.DefaultReplaceResponseType, global:: W3C.Soap.Fault> responsePort) : 
                base(body, responsePort) {
        }
    }
    
    /// <summary>
    ///            GPSSensor reliable subscribe operation
    ///            </summary>
    [global::System.ComponentModel.DescriptionAttribute("GPSSensor reliable subscribe operation")]
    [global::System.Xml.Serialization.XmlTypeAttribute(IncludeInSchema=false)]
    public class ReliableSubscribe : global::Microsoft.Dss.ServiceModel.Dssp.Subscribe<global::Microsoft.Dss.ServiceModel.Dssp.ReliableSubscribeRequestType, global:: Microsoft.Dss.ServiceModel.Dssp.DsspResponsePort<global::Microsoft.Dss.ServiceModel.Dssp.SubscribeResponseType>, global:: Microsoft.Robotics.Services.Simulation.Sensors.GPSSensor.Proxy.GPSSensorOperations> {
        
        public ReliableSubscribe() {
        }
        
        public ReliableSubscribe(global::Microsoft.Dss.ServiceModel.Dssp.ReliableSubscribeRequestType body) : 
                base(body) {
        }
        
        public ReliableSubscribe(global::Microsoft.Dss.ServiceModel.Dssp.ReliableSubscribeRequestType body, global::Microsoft.Dss.ServiceModel.Dssp.DsspResponsePort<global::Microsoft.Dss.ServiceModel.Dssp.SubscribeResponseType> responsePort) : 
                base(body, responsePort) {
        }
        
        public ReliableSubscribe(global::Microsoft.Dss.ServiceModel.Dssp.ReliableSubscribeRequestType body, global::Microsoft.Dss.ServiceModel.Dssp.DsspResponsePort<global::Microsoft.Dss.ServiceModel.Dssp.SubscribeResponseType> responsePort, global::Microsoft.Robotics.Services.Simulation.Sensors.GPSSensor.Proxy.GPSSensorOperations notificationPort) : 
                base(body, responsePort, notificationPort) {
        }
    }
    
    /// <summary>
    ///            GPSSensor main operations port
    ///            </summary>
    [global::System.ComponentModel.DescriptionAttribute("GPSSensor main operations port")]
    [global::System.Xml.Serialization.XmlTypeAttribute(IncludeInSchema=false)]
    public class GPSSensorOperations : global::Microsoft.Ccr.Core.PortSet<global::Microsoft.Dss.ServiceModel.Dssp.DsspDefaultLookup, global:: Microsoft.Dss.ServiceModel.Dssp.DsspDefaultDrop, global:: Microsoft.Robotics.Services.Simulation.Sensors.GPSSensor.Proxy.Get, global:: Microsoft.Robotics.Services.Simulation.Sensors.GPSSensor.Proxy.Subscribe, global:: Microsoft.Robotics.Services.Simulation.Sensors.GPSSensor.Proxy.Replace, global:: Microsoft.Robotics.Services.Simulation.Sensors.GPSSensor.Proxy.ReliableSubscribe> {
        
        public GPSSensorOperations() {
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
        
        public virtual global::Microsoft.Ccr.Core.PortSet<global::Microsoft.Robotics.Services.Simulation.Sensors.GPSSensor.Proxy.GPSSensorState, global:: W3C.Soap.Fault> Get() {
            global::Microsoft.Dss.ServiceModel.Dssp.GetRequestType body = new global::Microsoft.Dss.ServiceModel.Dssp.GetRequestType();
            global::Microsoft.Robotics.Services.Simulation.Sensors.GPSSensor.Proxy.Get operation = new global::Microsoft.Robotics.Services.Simulation.Sensors.GPSSensor.Proxy.Get(body);
            this.Post(operation);
            return operation.ResponsePort;
        }
        
        public virtual global::Microsoft.Ccr.Core.Choice Get(out global::Microsoft.Robotics.Services.Simulation.Sensors.GPSSensor.Proxy.Get operation) {
            global::Microsoft.Dss.ServiceModel.Dssp.GetRequestType body = new global::Microsoft.Dss.ServiceModel.Dssp.GetRequestType();
            operation = new global::Microsoft.Robotics.Services.Simulation.Sensors.GPSSensor.Proxy.Get(body);
            this.Post(operation);
            return operation.ResponsePort;
        }
        
        public virtual global::Microsoft.Ccr.Core.PortSet<global::Microsoft.Robotics.Services.Simulation.Sensors.GPSSensor.Proxy.GPSSensorState, global:: W3C.Soap.Fault> Get(global::Microsoft.Dss.ServiceModel.Dssp.GetRequestType body) {
            if ((body == null)) {
                body = new global::Microsoft.Dss.ServiceModel.Dssp.GetRequestType();
            }
            global::Microsoft.Robotics.Services.Simulation.Sensors.GPSSensor.Proxy.Get operation = new global::Microsoft.Robotics.Services.Simulation.Sensors.GPSSensor.Proxy.Get(body);
            this.Post(operation);
            return operation.ResponsePort;
        }
        
        public virtual global::Microsoft.Ccr.Core.Choice Get(global::Microsoft.Dss.ServiceModel.Dssp.GetRequestType body, out global::Microsoft.Robotics.Services.Simulation.Sensors.GPSSensor.Proxy.Get operation) {
            if ((body == null)) {
                body = new global::Microsoft.Dss.ServiceModel.Dssp.GetRequestType();
            }
            operation = new global::Microsoft.Robotics.Services.Simulation.Sensors.GPSSensor.Proxy.Get(body);
            this.Post(operation);
            return operation.ResponsePort;
        }
        
        public virtual global::Microsoft.Ccr.Core.PortSet<global::Microsoft.Dss.ServiceModel.Dssp.SubscribeResponseType, global:: W3C.Soap.Fault> Subscribe(global::Microsoft.Ccr.Core.IPort notificationPort, params System.Type[] types) {
            global::Microsoft.Dss.ServiceModel.Dssp.SubscribeRequestType body = new global::Microsoft.Dss.ServiceModel.Dssp.SubscribeRequestType();
            global::Microsoft.Robotics.Services.Simulation.Sensors.GPSSensor.Proxy.Subscribe operation = new global::Microsoft.Robotics.Services.Simulation.Sensors.GPSSensor.Proxy.Subscribe(body);
            operation.NotificationPort = notificationPort;
            if ((types != null)) {
                body.TypeFilter = new string[types.Length];
                for (int index = 0; (index < types.Length); index = (index + 1)) {
                    body.TypeFilter[index] = global::Microsoft.Dss.ServiceModel.DsspServiceBase.DsspServiceBase.GetTypeFilterDescription(types[index]);
                }
            }
            this.Post(operation);
            return operation.ResponsePort;
        }
        
        public virtual global::Microsoft.Ccr.Core.Choice Subscribe(global::Microsoft.Ccr.Core.IPort notificationPort, out global::Microsoft.Robotics.Services.Simulation.Sensors.GPSSensor.Proxy.Subscribe operation, params System.Type[] types) {
            global::Microsoft.Dss.ServiceModel.Dssp.SubscribeRequestType body = new global::Microsoft.Dss.ServiceModel.Dssp.SubscribeRequestType();
            operation = new global::Microsoft.Robotics.Services.Simulation.Sensors.GPSSensor.Proxy.Subscribe(body);
            operation.NotificationPort = notificationPort;
            if ((types != null)) {
                body.TypeFilter = new string[types.Length];
                for (int index = 0; (index < types.Length); index = (index + 1)) {
                    body.TypeFilter[index] = global::Microsoft.Dss.ServiceModel.DsspServiceBase.DsspServiceBase.GetTypeFilterDescription(types[index]);
                }
            }
            this.Post(operation);
            return operation.ResponsePort;
        }
        
        public virtual global::Microsoft.Ccr.Core.PortSet<global::Microsoft.Dss.ServiceModel.Dssp.SubscribeResponseType, global:: W3C.Soap.Fault> Subscribe(global::Microsoft.Dss.ServiceModel.Dssp.SubscribeRequestType body, global::Microsoft.Ccr.Core.IPort notificationPort, params System.Type[] types) {
            if ((body == null)) {
                body = new global::Microsoft.Dss.ServiceModel.Dssp.SubscribeRequestType();
            }
            global::Microsoft.Robotics.Services.Simulation.Sensors.GPSSensor.Proxy.Subscribe operation = new global::Microsoft.Robotics.Services.Simulation.Sensors.GPSSensor.Proxy.Subscribe(body);
            operation.NotificationPort = notificationPort;
            if ((types != null)) {
                body.TypeFilter = new string[types.Length];
                for (int index = 0; (index < types.Length); index = (index + 1)) {
                    body.TypeFilter[index] = global::Microsoft.Dss.ServiceModel.DsspServiceBase.DsspServiceBase.GetTypeFilterDescription(types[index]);
                }
            }
            this.Post(operation);
            return operation.ResponsePort;
        }
        
        public virtual global::Microsoft.Ccr.Core.Choice Subscribe(global::Microsoft.Dss.ServiceModel.Dssp.SubscribeRequestType body, global::Microsoft.Ccr.Core.IPort notificationPort, out global::Microsoft.Robotics.Services.Simulation.Sensors.GPSSensor.Proxy.Subscribe operation, params System.Type[] types) {
            if ((body == null)) {
                body = new global::Microsoft.Dss.ServiceModel.Dssp.SubscribeRequestType();
            }
            operation = new global::Microsoft.Robotics.Services.Simulation.Sensors.GPSSensor.Proxy.Subscribe(body);
            operation.NotificationPort = notificationPort;
            if ((types != null)) {
                body.TypeFilter = new string[types.Length];
                for (int index = 0; (index < types.Length); index = (index + 1)) {
                    body.TypeFilter[index] = global::Microsoft.Dss.ServiceModel.DsspServiceBase.DsspServiceBase.GetTypeFilterDescription(types[index]);
                }
            }
            this.Post(operation);
            return operation.ResponsePort;
        }
        
        public virtual global::Microsoft.Ccr.Core.PortSet<global::Microsoft.Dss.ServiceModel.Dssp.DefaultReplaceResponseType, global:: W3C.Soap.Fault> Replace() {
            global::Microsoft.Robotics.Services.Simulation.Sensors.GPSSensor.Proxy.GPSSensorState body = new global::Microsoft.Robotics.Services.Simulation.Sensors.GPSSensor.Proxy.GPSSensorState();
            global::Microsoft.Robotics.Services.Simulation.Sensors.GPSSensor.Proxy.Replace operation = new global::Microsoft.Robotics.Services.Simulation.Sensors.GPSSensor.Proxy.Replace(body);
            this.Post(operation);
            return operation.ResponsePort;
        }
        
        public virtual global::Microsoft.Ccr.Core.Choice Replace(out global::Microsoft.Robotics.Services.Simulation.Sensors.GPSSensor.Proxy.Replace operation) {
            global::Microsoft.Robotics.Services.Simulation.Sensors.GPSSensor.Proxy.GPSSensorState body = new global::Microsoft.Robotics.Services.Simulation.Sensors.GPSSensor.Proxy.GPSSensorState();
            operation = new global::Microsoft.Robotics.Services.Simulation.Sensors.GPSSensor.Proxy.Replace(body);
            this.Post(operation);
            return operation.ResponsePort;
        }
        
        public virtual global::Microsoft.Ccr.Core.PortSet<global::Microsoft.Dss.ServiceModel.Dssp.DefaultReplaceResponseType, global:: W3C.Soap.Fault> Replace(global::Microsoft.Robotics.Services.Simulation.Sensors.GPSSensor.Proxy.GPSSensorState body) {
            if ((body == null)) {
                body = new global::Microsoft.Robotics.Services.Simulation.Sensors.GPSSensor.Proxy.GPSSensorState();
            }
            global::Microsoft.Robotics.Services.Simulation.Sensors.GPSSensor.Proxy.Replace operation = new global::Microsoft.Robotics.Services.Simulation.Sensors.GPSSensor.Proxy.Replace(body);
            this.Post(operation);
            return operation.ResponsePort;
        }
        
        public virtual global::Microsoft.Ccr.Core.Choice Replace(global::Microsoft.Robotics.Services.Simulation.Sensors.GPSSensor.Proxy.GPSSensorState body, out global::Microsoft.Robotics.Services.Simulation.Sensors.GPSSensor.Proxy.Replace operation) {
            if ((body == null)) {
                body = new global::Microsoft.Robotics.Services.Simulation.Sensors.GPSSensor.Proxy.GPSSensorState();
            }
            operation = new global::Microsoft.Robotics.Services.Simulation.Sensors.GPSSensor.Proxy.Replace(body);
            this.Post(operation);
            return operation.ResponsePort;
        }
        
        public virtual global::Microsoft.Dss.ServiceModel.Dssp.DsspResponsePort<global::Microsoft.Dss.ServiceModel.Dssp.SubscribeResponseType> ReliableSubscribe(global::Microsoft.Ccr.Core.IPort notificationPort, params System.Type[] types) {
            global::Microsoft.Dss.ServiceModel.Dssp.ReliableSubscribeRequestType body = new global::Microsoft.Dss.ServiceModel.Dssp.ReliableSubscribeRequestType();
            global::Microsoft.Robotics.Services.Simulation.Sensors.GPSSensor.Proxy.ReliableSubscribe operation = new global::Microsoft.Robotics.Services.Simulation.Sensors.GPSSensor.Proxy.ReliableSubscribe(body);
            operation.NotificationPort = notificationPort;
            if ((types != null)) {
                body.TypeFilter = new string[types.Length];
                for (int index = 0; (index < types.Length); index = (index + 1)) {
                    body.TypeFilter[index] = global::Microsoft.Dss.ServiceModel.DsspServiceBase.DsspServiceBase.GetTypeFilterDescription(types[index]);
                }
            }
            this.Post(operation);
            return operation.ResponsePort;
        }
        
        public virtual global::Microsoft.Ccr.Core.Choice ReliableSubscribe(global::Microsoft.Ccr.Core.IPort notificationPort, out global::Microsoft.Robotics.Services.Simulation.Sensors.GPSSensor.Proxy.ReliableSubscribe operation, params System.Type[] types) {
            global::Microsoft.Dss.ServiceModel.Dssp.ReliableSubscribeRequestType body = new global::Microsoft.Dss.ServiceModel.Dssp.ReliableSubscribeRequestType();
            operation = new global::Microsoft.Robotics.Services.Simulation.Sensors.GPSSensor.Proxy.ReliableSubscribe(body);
            operation.NotificationPort = notificationPort;
            if ((types != null)) {
                body.TypeFilter = new string[types.Length];
                for (int index = 0; (index < types.Length); index = (index + 1)) {
                    body.TypeFilter[index] = global::Microsoft.Dss.ServiceModel.DsspServiceBase.DsspServiceBase.GetTypeFilterDescription(types[index]);
                }
            }
            this.Post(operation);
            return operation.ResponsePort;
        }
        
        public virtual global::Microsoft.Dss.ServiceModel.Dssp.DsspResponsePort<global::Microsoft.Dss.ServiceModel.Dssp.SubscribeResponseType> ReliableSubscribe(global::Microsoft.Dss.ServiceModel.Dssp.ReliableSubscribeRequestType body, global::Microsoft.Ccr.Core.IPort notificationPort, params System.Type[] types) {
            if ((body == null)) {
                body = new global::Microsoft.Dss.ServiceModel.Dssp.ReliableSubscribeRequestType();
            }
            global::Microsoft.Robotics.Services.Simulation.Sensors.GPSSensor.Proxy.ReliableSubscribe operation = new global::Microsoft.Robotics.Services.Simulation.Sensors.GPSSensor.Proxy.ReliableSubscribe(body);
            operation.NotificationPort = notificationPort;
            if ((types != null)) {
                body.TypeFilter = new string[types.Length];
                for (int index = 0; (index < types.Length); index = (index + 1)) {
                    body.TypeFilter[index] = global::Microsoft.Dss.ServiceModel.DsspServiceBase.DsspServiceBase.GetTypeFilterDescription(types[index]);
                }
            }
            this.Post(operation);
            return operation.ResponsePort;
        }
        
        public virtual global::Microsoft.Ccr.Core.Choice ReliableSubscribe(global::Microsoft.Dss.ServiceModel.Dssp.ReliableSubscribeRequestType body, global::Microsoft.Ccr.Core.IPort notificationPort, out global::Microsoft.Robotics.Services.Simulation.Sensors.GPSSensor.Proxy.ReliableSubscribe operation, params System.Type[] types) {
            if ((body == null)) {
                body = new global::Microsoft.Dss.ServiceModel.Dssp.ReliableSubscribeRequestType();
            }
            operation = new global::Microsoft.Robotics.Services.Simulation.Sensors.GPSSensor.Proxy.ReliableSubscribe(body);
            operation.NotificationPort = notificationPort;
            if ((types != null)) {
                body.TypeFilter = new string[types.Length];
                for (int index = 0; (index < types.Length); index = (index + 1)) {
                    body.TypeFilter[index] = global::Microsoft.Dss.ServiceModel.DsspServiceBase.DsspServiceBase.GetTypeFilterDescription(types[index]);
                }
            }
            this.Post(operation);
            return operation.ResponsePort;
        }
    }
    
    /// <summary>
    ///            SimPhotoCell Service
    ///            Simulates a photocell brightness sensor.
    ///            </summary>
    [global::System.Xml.Serialization.XmlTypeAttribute(IncludeInSchema=false)]
    [global::System.ComponentModel.DescriptionAttribute("Provides a simulated GPS sensor")]
    [global::System.ComponentModel.DisplayNameAttribute("(User) Simulated GPS Sensor")]
    public class Contract {
        
        public const string Identifier = "http://schemas.microsoft.com/2008/11/simgpssensor.user.html";
        
        /// <summary>Creates a new instance of the service.</summary>
        /// <param name="constructorServicePort">Service constructor port</param>
        /// <param name="service">service path</param>
        /// <param name="partners">the partners of the service instance</param>
        /// <returns>create service response port</returns>
        public static global::Microsoft.Dss.ServiceModel.Dssp.DsspResponsePort<Microsoft.Dss.ServiceModel.Dssp.CreateResponse> CreateService(global::Microsoft.Dss.Services.Constructor.ConstructorPort constructorServicePort, string service, params Microsoft.Dss.ServiceModel.Dssp.PartnerType[] partners) {
            global::Microsoft.Dss.ServiceModel.Dssp.DsspResponsePort<Microsoft.Dss.ServiceModel.Dssp.CreateResponse> result = new global::Microsoft.Dss.ServiceModel.Dssp.DsspResponsePort<Microsoft.Dss.ServiceModel.Dssp.CreateResponse>();
            global::Microsoft.Dss.ServiceModel.Dssp.ServiceInfoType serviceInfo = new global::Microsoft.Dss.ServiceModel.Dssp.ServiceInfoType("http://schemas.microsoft.com/2008/11/simgpssensor.user.html", service);
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
            global::Microsoft.Dss.ServiceModel.Dssp.ServiceInfoType serviceInfo = new global::Microsoft.Dss.ServiceModel.Dssp.ServiceInfoType("http://schemas.microsoft.com/2008/11/simgpssensor.user.html", null);
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
            this.GPSSensorOperations = new global::Microsoft.Robotics.Services.Simulation.Sensors.GPSSensor.Proxy.GPSSensorOperations();
            base.Initialize(new global::Microsoft.Dss.Core.DssOperationsPortMetadata(this.GPSSensorOperations, "http://schemas.microsoft.com/2008/11/simgpssensor.user.html", "GPSSensorOperations", ""));
        }
        
        public global::Microsoft.Robotics.Services.Simulation.Sensors.GPSSensor.Proxy.GPSSensorOperations GPSSensorOperations;
    }
}
