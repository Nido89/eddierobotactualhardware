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
[assembly: global::Microsoft.Dss.Core.Attributes.ServiceDeclarationAttribute(global::Microsoft.Dss.Core.Attributes.DssServiceDeclaration.Proxy, SourceAssemblyKey="User.ServiceTutorial7.Y2006.M06, Version=0.0.0.0, Culture=neutral, PublicKeyToken" +
    "=7f9074033fd3dcf7")]
[assembly: global::System.Security.SecurityTransparentAttribute()]
[assembly: global::System.Security.SecurityRulesAttribute(global::System.Security.SecurityRuleSet.Level1)]

namespace RoboticsServiceTutorial7.Proxy {
    
    
    [global::Microsoft.Dss.Core.Attributes.DataContractAttribute(Namespace="http://schemas.microsoft.com/2006/06/servicetutorial7.user.html")]
    [global::System.Xml.Serialization.XmlRootAttribute(Namespace="http://schemas.microsoft.com/2006/06/servicetutorial7.user.html", ElementName="ServiceTutorial7State")]
    public class ServiceTutorial7State : global::Microsoft.Dss.Core.IDssSerializable, global::System.ICloneable {
        
        public ServiceTutorial7State() {
        }
        
        private global::System.Collections.Generic.List<string> _Clocks = new global::System.Collections.Generic.List<string>();
        
        [global::Microsoft.Dss.Core.Attributes.DataMemberAttribute(IsRequired=true, Order=-1)]
        public global::System.Collections.Generic.List<string> Clocks {
            get {
                return this._Clocks;
            }
            set {
                this._Clocks = value;
            }
        }
        
        private global::System.Collections.Generic.List<global::RoboticsServiceTutorial7.Proxy.TickCount> _TickCounts = new global::System.Collections.Generic.List<global::RoboticsServiceTutorial7.Proxy.TickCount>();
        
        [global::Microsoft.Dss.Core.Attributes.DataMemberAttribute(IsRequired=true, Order=-1)]
        public global::System.Collections.Generic.List<global::RoboticsServiceTutorial7.Proxy.TickCount> TickCounts {
            get {
                return this._TickCounts;
            }
            set {
                this._TickCounts = value;
            }
        }
        
        /// <summary>
        ///Copies the data member values of the current ServiceTutorial7State to the specified target object
        ///</summary>
        ///<param name="target">target object (must be an instance of)</param>
        public virtual void CopyTo(Microsoft.Dss.Core.IDssSerializable target) {
            global::RoboticsServiceTutorial7.Proxy.ServiceTutorial7State typedTarget = ((global::RoboticsServiceTutorial7.Proxy.ServiceTutorial7State)(target));
            if ((this._Clocks != null)) {
                int count = this._Clocks.Count;
                global::System.Collections.Generic.List<string> tmp = new global::System.Collections.Generic.List<string>(count);
                tmp.AddRange(this._Clocks);
                typedTarget._Clocks = tmp;
            }
            if ((this._TickCounts != null)) {
                int count0 = this._TickCounts.Count;
                global::System.Collections.Generic.List<global::RoboticsServiceTutorial7.Proxy.TickCount> tmp0 = new global::System.Collections.Generic.List<global::RoboticsServiceTutorial7.Proxy.TickCount>(count0);
                for (int index = 0; (index < count0); index = (index + 1)) {
                    global::RoboticsServiceTutorial7.Proxy.TickCount tmp1 = default(global::RoboticsServiceTutorial7.Proxy.TickCount);
                    if ((this._TickCounts[index] != null)) {
                        global::RoboticsServiceTutorial7.Proxy.TickCount tmp2 = new global::RoboticsServiceTutorial7.Proxy.TickCount();
                        ((Microsoft.Dss.Core.IDssSerializable)(this._TickCounts[index])).CopyTo(((Microsoft.Dss.Core.IDssSerializable)(tmp2)));
                        tmp1 = tmp2;
                    }
                    tmp0.Add(tmp1);
                }
                typedTarget._TickCounts = tmp0;
            }
        }
        
        /// <summary>
        ///Clones ServiceTutorial7State
        ///</summary>
        ///<returns>cloned value</returns>
        public virtual object Clone() {
            global::RoboticsServiceTutorial7.Proxy.ServiceTutorial7State target0 = new global::RoboticsServiceTutorial7.Proxy.ServiceTutorial7State();
            this.CopyTo(target0);
            return target0;
        }
        
        /// <summary>
        ///Serializes the data member values of the current ServiceTutorial7State to the specified writer
        ///</summary>
        ///<param name="writer">the writer to which to serialize</param>
        public virtual void Serialize(System.IO.BinaryWriter writer) {
            if ((this._Clocks == null)) {
                writer.Write(((byte)(0)));
            }
            else {
                writer.Write(((byte)(1)));
                writer.Write(this._Clocks.Count);
                for (int indexClocks = 0; (indexClocks < this._Clocks.Count); indexClocks = (indexClocks + 1)) {
                    if ((this._Clocks[indexClocks] == null)) {
                        writer.Write(((byte)(0)));
                    }
                    else {
                        writer.Write(((byte)(1)));
                        writer.Write(this._Clocks[indexClocks]);
                    }
                }
            }
            if ((this._TickCounts == null)) {
                writer.Write(((byte)(0)));
            }
            else {
                writer.Write(((byte)(1)));
                writer.Write(this._TickCounts.Count);
                for (int indexTickCounts = 0; (indexTickCounts < this._TickCounts.Count); indexTickCounts = (indexTickCounts + 1)) {
                    if ((this._TickCounts[indexTickCounts] == null)) {
                        writer.Write(((byte)(0)));
                    }
                    else {
                        writer.Write(((byte)(1)));
                        ((Microsoft.Dss.Core.IDssSerializable)(this._TickCounts[indexTickCounts])).Serialize(writer);
                    }
                }
            }
        }
        
        /// <summary>
        ///Deserializes ServiceTutorial7State
        ///</summary>
        ///<param name="reader">the reader from which to deserialize</param>
        ///<returns>deserialized ServiceTutorial7State</returns>
        public virtual object Deserialize(System.IO.BinaryReader reader) {
            if ((reader.ReadByte() != 0)) {
                int count1 = reader.ReadInt32();
                this._Clocks = new global::System.Collections.Generic.List<string>(count1);
                for (int index0 = 0; (index0 < count1); index0 = (index0 + 1)) {
                    this._Clocks.Add(default(string));
                    if ((reader.ReadByte() != 0)) {
                        this._Clocks[index0] = reader.ReadString();
                    }
                }
            }
            if ((reader.ReadByte() != 0)) {
                int count2 = reader.ReadInt32();
                this._TickCounts = new global::System.Collections.Generic.List<global::RoboticsServiceTutorial7.Proxy.TickCount>(count2);
                for (int index1 = 0; (index1 < count2); index1 = (index1 + 1)) {
                    this._TickCounts.Add(default(global::RoboticsServiceTutorial7.Proxy.TickCount));
                    if ((reader.ReadByte() != 0)) {
                        this._TickCounts[index1] = ((global::RoboticsServiceTutorial7.Proxy.TickCount)(((Microsoft.Dss.Core.IDssSerializable)(new global::RoboticsServiceTutorial7.Proxy.TickCount())).Deserialize(reader)));
                    }
                }
            }
            return this;
        }
    }
    
    [global::Microsoft.Dss.Core.Attributes.DataContractAttribute(Namespace="http://schemas.microsoft.com/2006/06/servicetutorial7.user.html")]
    [global::System.Xml.Serialization.XmlRootAttribute(Namespace="http://schemas.microsoft.com/2006/06/servicetutorial7.user.html", ElementName="TickCount")]
    public class TickCount : global::Microsoft.Dss.Core.IDssSerializable, global::System.ICloneable {
        
        public TickCount() {
        }
        
        private string _Name;
        
        [global::Microsoft.Dss.Core.Attributes.DataMemberAttribute(Order=-1)]
        public string Name {
            get {
                return this._Name;
            }
            set {
                this._Name = value;
            }
        }
        
        private int _Count;
        
        [global::Microsoft.Dss.Core.Attributes.DataMemberAttribute(Order=-1)]
        public int Count {
            get {
                return this._Count;
            }
            set {
                this._Count = value;
            }
        }
        
        /// <summary>
        ///Copies the data member values of the current TickCount to the specified target object
        ///</summary>
        ///<param name="target">target object (must be an instance of)</param>
        public virtual void CopyTo(Microsoft.Dss.Core.IDssSerializable target) {
            global::RoboticsServiceTutorial7.Proxy.TickCount typedTarget = ((global::RoboticsServiceTutorial7.Proxy.TickCount)(target));
            typedTarget._Name = this._Name;
            typedTarget._Count = this._Count;
        }
        
        /// <summary>
        ///Clones TickCount
        ///</summary>
        ///<returns>cloned value</returns>
        public virtual object Clone() {
            global::RoboticsServiceTutorial7.Proxy.TickCount target0 = new global::RoboticsServiceTutorial7.Proxy.TickCount();
            this.CopyTo(target0);
            return target0;
        }
        
        /// <summary>
        ///Serializes the data member values of the current TickCount to the specified writer
        ///</summary>
        ///<param name="writer">the writer to which to serialize</param>
        public virtual void Serialize(System.IO.BinaryWriter writer) {
            if ((this._Name == null)) {
                writer.Write(((byte)(0)));
            }
            else {
                writer.Write(((byte)(1)));
                writer.Write(this._Name);
            }
            writer.Write(this._Count);
        }
        
        /// <summary>
        ///Deserializes TickCount
        ///</summary>
        ///<param name="reader">the reader from which to deserialize</param>
        ///<returns>deserialized TickCount</returns>
        public virtual object Deserialize(System.IO.BinaryReader reader) {
            if ((reader.ReadByte() != 0)) {
                this._Name = reader.ReadString();
            }
            this._Count = reader.ReadInt32();
            return this;
        }
    }
    
    [global::Microsoft.Dss.Core.Attributes.DataContractAttribute(Namespace="http://schemas.microsoft.com/2006/06/servicetutorial7.user.html")]
    [global::System.Xml.Serialization.XmlRootAttribute(Namespace="http://schemas.microsoft.com/2006/06/servicetutorial7.user.html", ElementName="IncrementTickRequest")]
    public class IncrementTickRequest : global::Microsoft.Dss.Core.IDssSerializable, global::System.ICloneable {
        
        public IncrementTickRequest() {
        }
        
        public IncrementTickRequest(string name) {
            this._Name = name;
        }
        
        private string _Name;
        
        [global::Microsoft.Dss.Core.Attributes.DataMemberAttribute(Order=-1)]
        [global::Microsoft.Dss.Core.Attributes.DataMemberConstructorAttribute(Order=1)]
        public string Name {
            get {
                return this._Name;
            }
            set {
                this._Name = value;
            }
        }
        
        /// <summary>
        ///Copies the data member values of the current IncrementTickRequest to the specified target object
        ///</summary>
        ///<param name="target">target object (must be an instance of)</param>
        public virtual void CopyTo(Microsoft.Dss.Core.IDssSerializable target) {
            global::RoboticsServiceTutorial7.Proxy.IncrementTickRequest typedTarget = ((global::RoboticsServiceTutorial7.Proxy.IncrementTickRequest)(target));
            typedTarget._Name = this._Name;
        }
        
        /// <summary>
        ///Clones IncrementTickRequest
        ///</summary>
        ///<returns>cloned value</returns>
        public virtual object Clone() {
            global::RoboticsServiceTutorial7.Proxy.IncrementTickRequest target0 = new global::RoboticsServiceTutorial7.Proxy.IncrementTickRequest();
            this.CopyTo(target0);
            return target0;
        }
        
        /// <summary>
        ///Serializes the data member values of the current IncrementTickRequest to the specified writer
        ///</summary>
        ///<param name="writer">the writer to which to serialize</param>
        public virtual void Serialize(System.IO.BinaryWriter writer) {
            if ((this._Name == null)) {
                writer.Write(((byte)(0)));
            }
            else {
                writer.Write(((byte)(1)));
                writer.Write(this._Name);
            }
        }
        
        /// <summary>
        ///Deserializes IncrementTickRequest
        ///</summary>
        ///<param name="reader">the reader from which to deserialize</param>
        ///<returns>deserialized IncrementTickRequest</returns>
        public virtual object Deserialize(System.IO.BinaryReader reader) {
            if ((reader.ReadByte() != 0)) {
                this._Name = reader.ReadString();
            }
            return this;
        }
    }
    
    [global::System.Xml.Serialization.XmlTypeAttribute(IncludeInSchema=false)]
    public class Get : global::Microsoft.Dss.ServiceModel.Dssp.Get<global::Microsoft.Dss.ServiceModel.Dssp.GetRequestType, global:: Microsoft.Ccr.Core.PortSet<global::RoboticsServiceTutorial7.Proxy.ServiceTutorial7State, global:: W3C.Soap.Fault>> {
        
        public Get() {
        }
        
        public Get(global::Microsoft.Dss.ServiceModel.Dssp.GetRequestType body) : 
                base(body) {
        }
        
        public Get(global::Microsoft.Dss.ServiceModel.Dssp.GetRequestType body, global::Microsoft.Ccr.Core.PortSet<global::RoboticsServiceTutorial7.Proxy.ServiceTutorial7State, global:: W3C.Soap.Fault> responsePort) : 
                base(body, responsePort) {
        }
    }
    
    [global::System.Xml.Serialization.XmlTypeAttribute(IncludeInSchema=false)]
    public class Replace : global::Microsoft.Dss.ServiceModel.Dssp.Replace<global::RoboticsServiceTutorial7.Proxy.ServiceTutorial7State, global:: Microsoft.Ccr.Core.PortSet<global::Microsoft.Dss.ServiceModel.Dssp.DefaultReplaceResponseType, global:: W3C.Soap.Fault>> {
        
        public Replace() {
        }
        
        public Replace(global::RoboticsServiceTutorial7.Proxy.ServiceTutorial7State body) : 
                base(body) {
        }
        
        public Replace(global::RoboticsServiceTutorial7.Proxy.ServiceTutorial7State body, global::Microsoft.Ccr.Core.PortSet<global::Microsoft.Dss.ServiceModel.Dssp.DefaultReplaceResponseType, global:: W3C.Soap.Fault> responsePort) : 
                base(body, responsePort) {
        }
    }
    
    [global::System.Xml.Serialization.XmlTypeAttribute(IncludeInSchema=false)]
    public class IncrementTick : global::Microsoft.Dss.ServiceModel.Dssp.Update<global::RoboticsServiceTutorial7.Proxy.IncrementTickRequest, global:: Microsoft.Ccr.Core.PortSet<global::Microsoft.Dss.ServiceModel.Dssp.DefaultUpdateResponseType, global:: W3C.Soap.Fault>> {
        
        public IncrementTick() {
        }
        
        public IncrementTick(global::RoboticsServiceTutorial7.Proxy.IncrementTickRequest body) : 
                base(body) {
        }
        
        public IncrementTick(global::RoboticsServiceTutorial7.Proxy.IncrementTickRequest body, global::Microsoft.Ccr.Core.PortSet<global::Microsoft.Dss.ServiceModel.Dssp.DefaultUpdateResponseType, global:: W3C.Soap.Fault> responsePort) : 
                base(body, responsePort) {
        }
    }
    
    [global::System.Xml.Serialization.XmlTypeAttribute(IncludeInSchema=false)]
    public class SetTickCount : global::Microsoft.Dss.ServiceModel.Dssp.Update<global::RoboticsServiceTutorial7.Proxy.TickCount, global:: Microsoft.Ccr.Core.PortSet<global::Microsoft.Dss.ServiceModel.Dssp.DefaultUpdateResponseType, global:: W3C.Soap.Fault>> {
        
        public SetTickCount() {
        }
        
        public SetTickCount(global::RoboticsServiceTutorial7.Proxy.TickCount body) : 
                base(body) {
        }
        
        public SetTickCount(global::RoboticsServiceTutorial7.Proxy.TickCount body, global::Microsoft.Ccr.Core.PortSet<global::Microsoft.Dss.ServiceModel.Dssp.DefaultUpdateResponseType, global:: W3C.Soap.Fault> responsePort) : 
                base(body, responsePort) {
        }
    }
    
    [global::System.Xml.Serialization.XmlTypeAttribute(IncludeInSchema=false)]
    public class ServiceTutorial7Operations : global::Microsoft.Ccr.Core.PortSet<global::Microsoft.Dss.ServiceModel.Dssp.DsspDefaultLookup, global:: Microsoft.Dss.ServiceModel.Dssp.DsspDefaultDrop, global:: RoboticsServiceTutorial7.Proxy.Get, global:: Microsoft.Dss.Core.DsspHttp.HttpGet, global:: RoboticsServiceTutorial7.Proxy.Replace, global:: RoboticsServiceTutorial7.Proxy.IncrementTick, global:: RoboticsServiceTutorial7.Proxy.SetTickCount> {
        
        public ServiceTutorial7Operations() {
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
        
        public virtual global::Microsoft.Ccr.Core.PortSet<global::RoboticsServiceTutorial7.Proxy.ServiceTutorial7State, global:: W3C.Soap.Fault> Get() {
            global::Microsoft.Dss.ServiceModel.Dssp.GetRequestType body = new global::Microsoft.Dss.ServiceModel.Dssp.GetRequestType();
            global::RoboticsServiceTutorial7.Proxy.Get operation = new global::RoboticsServiceTutorial7.Proxy.Get(body);
            this.Post(operation);
            return operation.ResponsePort;
        }
        
        public virtual global::Microsoft.Ccr.Core.Choice Get(out global::RoboticsServiceTutorial7.Proxy.Get operation) {
            global::Microsoft.Dss.ServiceModel.Dssp.GetRequestType body = new global::Microsoft.Dss.ServiceModel.Dssp.GetRequestType();
            operation = new global::RoboticsServiceTutorial7.Proxy.Get(body);
            this.Post(operation);
            return operation.ResponsePort;
        }
        
        public virtual global::Microsoft.Ccr.Core.PortSet<global::RoboticsServiceTutorial7.Proxy.ServiceTutorial7State, global:: W3C.Soap.Fault> Get(global::Microsoft.Dss.ServiceModel.Dssp.GetRequestType body) {
            if ((body == null)) {
                body = new global::Microsoft.Dss.ServiceModel.Dssp.GetRequestType();
            }
            global::RoboticsServiceTutorial7.Proxy.Get operation = new global::RoboticsServiceTutorial7.Proxy.Get(body);
            this.Post(operation);
            return operation.ResponsePort;
        }
        
        public virtual global::Microsoft.Ccr.Core.Choice Get(global::Microsoft.Dss.ServiceModel.Dssp.GetRequestType body, out global::RoboticsServiceTutorial7.Proxy.Get operation) {
            if ((body == null)) {
                body = new global::Microsoft.Dss.ServiceModel.Dssp.GetRequestType();
            }
            operation = new global::RoboticsServiceTutorial7.Proxy.Get(body);
            this.Post(operation);
            return operation.ResponsePort;
        }
        
        public virtual global::Microsoft.Ccr.Core.PortSet<global::Microsoft.Dss.Core.DsspHttp.HttpResponseType, global::W3C.Soap.Fault> HttpGet() {
            global::Microsoft.Dss.Core.DsspHttp.HttpGetRequestType body = new global::Microsoft.Dss.Core.DsspHttp.HttpGetRequestType();
            global::Microsoft.Dss.Core.DsspHttp.HttpGet operation = new global::Microsoft.Dss.Core.DsspHttp.HttpGet(body);
            this.Post(operation);
            return operation.ResponsePort;
        }
        
        public virtual global::Microsoft.Ccr.Core.Choice HttpGet(out global::Microsoft.Dss.Core.DsspHttp.HttpGet operation) {
            global::Microsoft.Dss.Core.DsspHttp.HttpGetRequestType body = new global::Microsoft.Dss.Core.DsspHttp.HttpGetRequestType();
            operation = new global::Microsoft.Dss.Core.DsspHttp.HttpGet(body);
            this.Post(operation);
            return operation.ResponsePort;
        }
        
        public virtual global::Microsoft.Ccr.Core.PortSet<global::Microsoft.Dss.Core.DsspHttp.HttpResponseType, global::W3C.Soap.Fault> HttpGet(global::Microsoft.Dss.Core.DsspHttp.HttpGetRequestType body) {
            if ((body == null)) {
                body = new global::Microsoft.Dss.Core.DsspHttp.HttpGetRequestType();
            }
            global::Microsoft.Dss.Core.DsspHttp.HttpGet operation = new global::Microsoft.Dss.Core.DsspHttp.HttpGet(body);
            this.Post(operation);
            return operation.ResponsePort;
        }
        
        public virtual global::Microsoft.Ccr.Core.Choice HttpGet(global::Microsoft.Dss.Core.DsspHttp.HttpGetRequestType body, out global::Microsoft.Dss.Core.DsspHttp.HttpGet operation) {
            if ((body == null)) {
                body = new global::Microsoft.Dss.Core.DsspHttp.HttpGetRequestType();
            }
            operation = new global::Microsoft.Dss.Core.DsspHttp.HttpGet(body);
            this.Post(operation);
            return operation.ResponsePort;
        }
        
        public virtual global::Microsoft.Ccr.Core.PortSet<global::Microsoft.Dss.ServiceModel.Dssp.DefaultReplaceResponseType, global:: W3C.Soap.Fault> Replace() {
            global::RoboticsServiceTutorial7.Proxy.ServiceTutorial7State body = new global::RoboticsServiceTutorial7.Proxy.ServiceTutorial7State();
            global::RoboticsServiceTutorial7.Proxy.Replace operation = new global::RoboticsServiceTutorial7.Proxy.Replace(body);
            this.Post(operation);
            return operation.ResponsePort;
        }
        
        public virtual global::Microsoft.Ccr.Core.Choice Replace(out global::RoboticsServiceTutorial7.Proxy.Replace operation) {
            global::RoboticsServiceTutorial7.Proxy.ServiceTutorial7State body = new global::RoboticsServiceTutorial7.Proxy.ServiceTutorial7State();
            operation = new global::RoboticsServiceTutorial7.Proxy.Replace(body);
            this.Post(operation);
            return operation.ResponsePort;
        }
        
        public virtual global::Microsoft.Ccr.Core.PortSet<global::Microsoft.Dss.ServiceModel.Dssp.DefaultReplaceResponseType, global:: W3C.Soap.Fault> Replace(global::RoboticsServiceTutorial7.Proxy.ServiceTutorial7State body) {
            if ((body == null)) {
                body = new global::RoboticsServiceTutorial7.Proxy.ServiceTutorial7State();
            }
            global::RoboticsServiceTutorial7.Proxy.Replace operation = new global::RoboticsServiceTutorial7.Proxy.Replace(body);
            this.Post(operation);
            return operation.ResponsePort;
        }
        
        public virtual global::Microsoft.Ccr.Core.Choice Replace(global::RoboticsServiceTutorial7.Proxy.ServiceTutorial7State body, out global::RoboticsServiceTutorial7.Proxy.Replace operation) {
            if ((body == null)) {
                body = new global::RoboticsServiceTutorial7.Proxy.ServiceTutorial7State();
            }
            operation = new global::RoboticsServiceTutorial7.Proxy.Replace(body);
            this.Post(operation);
            return operation.ResponsePort;
        }
        
        public virtual global::Microsoft.Ccr.Core.PortSet<global::Microsoft.Dss.ServiceModel.Dssp.DefaultUpdateResponseType, global:: W3C.Soap.Fault> IncrementTick(string name) {
            global::RoboticsServiceTutorial7.Proxy.IncrementTickRequest body = new global::RoboticsServiceTutorial7.Proxy.IncrementTickRequest(name);
            global::RoboticsServiceTutorial7.Proxy.IncrementTick operation = new global::RoboticsServiceTutorial7.Proxy.IncrementTick(body);
            this.Post(operation);
            return operation.ResponsePort;
        }
        
        public virtual global::Microsoft.Ccr.Core.Choice IncrementTick(string name, out global::RoboticsServiceTutorial7.Proxy.IncrementTick operation) {
            global::RoboticsServiceTutorial7.Proxy.IncrementTickRequest body = new global::RoboticsServiceTutorial7.Proxy.IncrementTickRequest(name);
            operation = new global::RoboticsServiceTutorial7.Proxy.IncrementTick(body);
            this.Post(operation);
            return operation.ResponsePort;
        }
        
        public virtual global::Microsoft.Ccr.Core.PortSet<global::Microsoft.Dss.ServiceModel.Dssp.DefaultUpdateResponseType, global:: W3C.Soap.Fault> IncrementTick(global::RoboticsServiceTutorial7.Proxy.IncrementTickRequest body) {
            if ((body == null)) {
                body = new global::RoboticsServiceTutorial7.Proxy.IncrementTickRequest();
            }
            global::RoboticsServiceTutorial7.Proxy.IncrementTick operation = new global::RoboticsServiceTutorial7.Proxy.IncrementTick(body);
            this.Post(operation);
            return operation.ResponsePort;
        }
        
        public virtual global::Microsoft.Ccr.Core.Choice IncrementTick(global::RoboticsServiceTutorial7.Proxy.IncrementTickRequest body, out global::RoboticsServiceTutorial7.Proxy.IncrementTick operation) {
            if ((body == null)) {
                body = new global::RoboticsServiceTutorial7.Proxy.IncrementTickRequest();
            }
            operation = new global::RoboticsServiceTutorial7.Proxy.IncrementTick(body);
            this.Post(operation);
            return operation.ResponsePort;
        }
        
        public virtual global::Microsoft.Ccr.Core.PortSet<global::Microsoft.Dss.ServiceModel.Dssp.DefaultUpdateResponseType, global:: W3C.Soap.Fault> SetTickCount() {
            global::RoboticsServiceTutorial7.Proxy.TickCount body = new global::RoboticsServiceTutorial7.Proxy.TickCount();
            global::RoboticsServiceTutorial7.Proxy.SetTickCount operation = new global::RoboticsServiceTutorial7.Proxy.SetTickCount(body);
            this.Post(operation);
            return operation.ResponsePort;
        }
        
        public virtual global::Microsoft.Ccr.Core.Choice SetTickCount(out global::RoboticsServiceTutorial7.Proxy.SetTickCount operation) {
            global::RoboticsServiceTutorial7.Proxy.TickCount body = new global::RoboticsServiceTutorial7.Proxy.TickCount();
            operation = new global::RoboticsServiceTutorial7.Proxy.SetTickCount(body);
            this.Post(operation);
            return operation.ResponsePort;
        }
        
        public virtual global::Microsoft.Ccr.Core.PortSet<global::Microsoft.Dss.ServiceModel.Dssp.DefaultUpdateResponseType, global:: W3C.Soap.Fault> SetTickCount(global::RoboticsServiceTutorial7.Proxy.TickCount body) {
            if ((body == null)) {
                body = new global::RoboticsServiceTutorial7.Proxy.TickCount();
            }
            global::RoboticsServiceTutorial7.Proxy.SetTickCount operation = new global::RoboticsServiceTutorial7.Proxy.SetTickCount(body);
            this.Post(operation);
            return operation.ResponsePort;
        }
        
        public virtual global::Microsoft.Ccr.Core.Choice SetTickCount(global::RoboticsServiceTutorial7.Proxy.TickCount body, out global::RoboticsServiceTutorial7.Proxy.SetTickCount operation) {
            if ((body == null)) {
                body = new global::RoboticsServiceTutorial7.Proxy.TickCount();
            }
            operation = new global::RoboticsServiceTutorial7.Proxy.SetTickCount(body);
            this.Post(operation);
            return operation.ResponsePort;
        }
    }
    
    [global::System.Xml.Serialization.XmlTypeAttribute(IncludeInSchema=false)]
    [global::System.ComponentModel.DescriptionAttribute("Demonstrates how to subscribe to and manage services running on other DSS nodes.")]
    [global::System.ComponentModel.DisplayNameAttribute("(User) Service Tutorial 7: Advanced Topics")]
    public class Contract {
        
        public const string Identifier = "http://schemas.microsoft.com/2006/06/servicetutorial7.user.html";
        
        /// <summary>Creates a new instance of the service.</summary>
        /// <param name="constructorServicePort">Service constructor port</param>
        /// <param name="service">service path</param>
        /// <param name="partners">the partners of the service instance</param>
        /// <returns>create service response port</returns>
        public static global::Microsoft.Dss.ServiceModel.Dssp.DsspResponsePort<Microsoft.Dss.ServiceModel.Dssp.CreateResponse> CreateService(global::Microsoft.Dss.Services.Constructor.ConstructorPort constructorServicePort, string service, params Microsoft.Dss.ServiceModel.Dssp.PartnerType[] partners) {
            global::Microsoft.Dss.ServiceModel.Dssp.DsspResponsePort<Microsoft.Dss.ServiceModel.Dssp.CreateResponse> result = new global::Microsoft.Dss.ServiceModel.Dssp.DsspResponsePort<Microsoft.Dss.ServiceModel.Dssp.CreateResponse>();
            global::Microsoft.Dss.ServiceModel.Dssp.ServiceInfoType serviceInfo = new global::Microsoft.Dss.ServiceModel.Dssp.ServiceInfoType("http://schemas.microsoft.com/2006/06/servicetutorial7.user.html", service);
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
            global::Microsoft.Dss.ServiceModel.Dssp.ServiceInfoType serviceInfo = new global::Microsoft.Dss.ServiceModel.Dssp.ServiceInfoType("http://schemas.microsoft.com/2006/06/servicetutorial7.user.html", null);
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
            this.ServiceTutorial7Operations = new global::RoboticsServiceTutorial7.Proxy.ServiceTutorial7Operations();
            base.Initialize(new global::Microsoft.Dss.Core.DssOperationsPortMetadata(this.ServiceTutorial7Operations, "http://schemas.microsoft.com/2006/06/servicetutorial7.user.html", "ServiceTutorial7Operations", ""));
        }
        
        public global::RoboticsServiceTutorial7.Proxy.ServiceTutorial7Operations ServiceTutorial7Operations;
    }
}