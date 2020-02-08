///////////////////////////////////////////////////////////////////////////////
// Activity: program.activity
// Diagram type definitions
///////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml;

using ccr = Microsoft.Ccr.Core;
using dss = Microsoft.Dss.Core;
using dssa = Microsoft.Dss.Core.Attributes;
using dssh = Microsoft.Dss.Core.DsspHttp;
using dssm = Microsoft.Dss.ServiceModel.DsspServiceBase;
using dssp = Microsoft.Dss.ServiceModel.Dssp;
using soap = W3C.Soap;

using submgr = Microsoft.Dss.Services.SubscriptionManager;

[assembly: dssa.ServiceDeclaration(dssa.DssServiceDeclaration.ServiceBehavior)]
[assembly: System.Runtime.InteropServices.ComVisible(false)]

namespace Robotics.FindObject.Diagram
{
    static class Contract
    {
        public const string Identifier = "http://schemas.tempuri.org/2013/09/findobject/diagram.html";
    }

    [dssa.DataContract]
    public class DiagramState
    {
        double _z;
        [dssa.DataMember]
        public double Z
        {
            get { return _z; }
            set { _z = value; }
        }
    }

    #region Custom message types

    [dssa.DataContract]
    public class ActionRequest
    {
    }

    [dssa.DataContract]
    public class ActionResponse
    {
    }

    #endregion

    #region Operations Port

    public class DiagramOperations : ccr.PortSet
    {
        public DiagramOperations() : base (
            typeof(dssp.DsspDefaultLookup),
            typeof(dssp.DsspDefaultDrop),
            typeof(dssh.HttpGet),
            typeof(Action),
            typeof(Get),
            typeof(Replace),
            typeof(Subscribe)
        )
        {
        }

        public static implicit operator ccr.Port<dssp.DsspDefaultLookup>(DiagramOperations portSet)
        {
            if (portSet == null)
            {
                return null;
            }
            return (ccr.Port<dssp.DsspDefaultLookup>)portSet[typeof(dssp.DsspDefaultLookup)];
        }

        public void Post(dssp.DsspDefaultLookup msg)
        {
            base.PostUnknownType(msg);
        }

        public static implicit operator ccr.Port<dssp.DsspDefaultDrop>(DiagramOperations portSet)
        {
            if (portSet == null)
            {
                return null;
            }
            return (ccr.Port<dssp.DsspDefaultDrop>)portSet[typeof(dssp.DsspDefaultDrop)];
        }

        public void Post(dssp.DsspDefaultDrop msg)
        {
            base.PostUnknownType(msg);
        }

        public static implicit operator ccr.Port<Get>(DiagramOperations portSet)
        {
            if (portSet == null)
            {
                return null;
            }
            return (ccr.Port<Get>)portSet[typeof(Get)];
        }

        public void Post(Get msg)
        {
            base.PostUnknownType(msg);
        }

        public static implicit operator ccr.Port<Replace>(DiagramOperations portSet)
        {
            if (portSet == null)
            {
                return null;
            }
            return (ccr.Port<Replace>)portSet[typeof(Replace)];
        }

        public void Post(Replace msg)
        {
            base.PostUnknownType(msg);
        }

        public static implicit operator ccr.Port<Subscribe>(DiagramOperations portSet)
        {
            if (portSet == null)
            {
                return null;
            }
            return (ccr.Port<Subscribe>)portSet[typeof(Subscribe)];
        }

        public void Post(Subscribe msg)
        {
            base.PostUnknownType(msg);
        }

        public static implicit operator ccr.Port<Action>(DiagramOperations portSet)
        {
            if (portSet == null)
            {
                return null;
            }
            return (ccr.Port<Action>)portSet[typeof(Action)];
        }

        public void Post(Action msg)
        {
            base.PostUnknownType(msg);
        }

        public dssp.DsspResponsePort<ActionResponse> Action(ActionRequest body)
        {
            Action message = new Action();
            message.Body = body;

            this.Post(message);

            return message.ResponsePort;
        }

        public dssp.DsspResponsePort<dssp.SubscribeResponseType> Subscribe(ccr.IPort notificationPort)
        {
            Subscribe message = new Subscribe();
            message.NotificationPort = notificationPort;

            this.Post(message);

            return message.ResponsePort;
        }
    }

    #endregion

    #region Standard DSSP message definitions

    [Description("Retrieves the current state of the service")]
    public class Get : dssp.Get<dssp.GetRequestType, dssp.DsspResponsePort<DiagramState>>
    {
    }

    [Description("Sets the current state of the service.\nThis is raised as an event whenever the state changes.")]
    public class Replace : dssp.Replace<DiagramState, dssp.DsspResponsePort<dssp.DefaultReplaceResponseType>>
    {
    }

    public class Subscribe : dssp.Subscribe<dssp.SubscribeRequestType, dssp.DsspResponsePort<dssp.SubscribeResponseType>>
    {
    }

    #endregion

    #region Custom message definitions

    public class Action : dssp.Submit<ActionRequest, dssp.DsspResponsePort<ActionResponse>>
    {
    }

    #endregion
}
