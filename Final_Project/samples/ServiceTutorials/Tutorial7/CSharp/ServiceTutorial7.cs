//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: ServiceTutorial7.cs $ $Revision: 9 $
//-----------------------------------------------------------------------

using Microsoft.Ccr.Core;
using Microsoft.Dss.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.Core.DsspHttp;
using Microsoft.Dss.ServiceModel.Dssp;
using Microsoft.Dss.ServiceModel.DsspServiceBase;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml;
using W3C.Soap;

using servicetutorial7 = RoboticsServiceTutorial7;
using rst4 = RoboticsServiceTutorial4.Proxy;
#region CODECLIP 02-2
using ds = Microsoft.Dss.Services.Directory;
using cs = Microsoft.Dss.Services.Constructor;
#endregion


namespace RoboticsServiceTutorial7
{
    /// <summary>
    /// Implementation class for ServiceTutorial7
    /// </summary>
    [DisplayName("(User) Service Tutorial 7: Advanced Topics")]
    [Description("Demonstrates how to subscribe to and manage services running on other DSS nodes.")]
    [DssServiceDescription("http://msdn.microsoft.com/library/bb483059.aspx")]
    [Contract(Contract.Identifier)]
    public class ServiceTutorial7 : DsspServiceBase
    {
        // Embed the XSLT Transform into the service DLL
        [EmbeddedResource("RoboticsServiceTutorial7.ServiceTutorial7.user.xslt")]
        string _transform = null;

        /// <summary>
        /// Service State
        /// </summary>
        [ServiceState]
        private ServiceTutorial7State _state = new ServiceTutorial7State();

        /// <summary>
        /// Main Port
        /// </summary>
        [ServicePort("/ServiceTutorial7", AllowMultipleInstances = false)]
        private ServiceTutorial7Operations _mainPort = new ServiceTutorial7Operations();

        rst4.ServiceTutorial4Operations _clockPort = new rst4.ServiceTutorial4Operations();
        rst4.ServiceTutorial4Operations _clockNotify = new rst4.ServiceTutorial4Operations();

        #region CODECLIP 03-5
        [Partner("Local", Contract = rst4.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.UsePartnerListEntry)]
        rst4.ServiceTutorial4Operations _localClockPort = new rst4.ServiceTutorial4Operations();
        rst4.ServiceTutorial4Operations _localClockNotify = new rst4.ServiceTutorial4Operations();
        #endregion

        /// <summary>
        /// Default Service Constructor
        /// </summary>
        public ServiceTutorial7(DsspServiceCreationPort creationPort) :
                base(creationPort)
        {
        }

        /// <summary>
        /// Service Start
        /// </summary>
        protected override void Start()
        {
            base.Start();

            #region CODECLIP 03-6
            Activate<ITask>(
                Arbiter.Receive<rst4.IncrementTick>(true, _clockNotify, RemoteNotifyTickHandler),
                Arbiter.Receive<rst4.Replace>(true, _clockNotify, RemoteNotifyReplaceHandler),
                Arbiter.Receive<rst4.IncrementTick>(true, _localClockNotify, LocalNotifyTickHandler),
                Arbiter.Receive<rst4.Replace>(true, _localClockNotify, LocalNotifyReplaceHandler)
            );
            #endregion

            SpawnIterator(OnStartup);
        }

        private IEnumerator<ITask> OnStartup()
        {
            #region CODECLIP 02-3
            PartnerType remote = FindPartner("Remote");
            ds.DirectoryPort remoteDir = DirectoryPort;

            if (remote != null && !string.IsNullOrEmpty(remote.Service))
            {
                remoteDir = ServiceForwarder<ds.DirectoryPort>(remote.Service);
            }
            #endregion

            #region CODECLIP 02-4
            cs.ConstructorPort remoteConstructor = ConstructorPort;
            ds.Query query = new ds.Query(
                new ds.QueryRequestType(
                    new ServiceInfoType(cs.Contract.Identifier)
                )
            );

            remoteDir.Post(query);
            yield return (Choice)query.ResponsePort;
            ds.QueryResponseType queryRsp = query.ResponsePort;
            if (queryRsp != null)
            {
                remoteConstructor = ServiceForwarder<cs.ConstructorPort>(queryRsp.RecordList[0].Service);
            }
            #endregion

            #region CODECLIP 02-5
            string clockService = null;
            cs.Create create = new cs.Create(new ServiceInfoType(rst4.Contract.Identifier));
            remoteConstructor.Post(create);
            yield return (Choice)create.ResponsePort;
            CreateResponse createRsp = create.ResponsePort;
            if (createRsp != null)
            {
                clockService = createRsp.Service;
            }
            else
            {
                LogError((Fault)create.ResponsePort);
                yield break;
            }

            _clockPort = ServiceForwarder<rst4.ServiceTutorial4Operations>(clockService);
            #endregion

            rst4.Get get;
            yield return _clockPort.Get(GetRequestType.Instance, out get);
            rst4.ServiceTutorial4State state = get.ResponsePort;

            if (state != null)
            {
                ServiceTutorial7State initState = new ServiceTutorial7State();

                PartnerType partner = FindPartner("Local");
                if (partner != null)
                {
                    initState.Clocks.Add(partner.Service);
                }
                initState.Clocks.Add(clockService);

                Replace replace = new Replace();
                replace.Body = initState;

                _mainPort.Post(replace);
            }
            else
            {
                LogError("Unable to Get state from ServiceTutorial4", (Fault)get.ResponsePort);
                yield break;
            }

            rst4.Subscribe subscribe;
            yield return _clockPort.Subscribe(_clockNotify, out subscribe);
            if ((Fault)subscribe.ResponsePort != null)
            {
                LogError("Unable to subscribe to remote ServiceTutorial4", (Fault)subscribe.ResponsePort);
            }

            yield return _localClockPort.Subscribe(_localClockNotify, out subscribe);
            if ((Fault)subscribe.ResponsePort != null)
            {
                LogError("Unable to subscribe to local ServiceTutorial4", (Fault)subscribe.ResponsePort);
            }
        }

        /*
        /// <summary>
        /// Get Handler
        /// </summary>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public IEnumerator<ITask> GetHandler(Get get)
        {
            get.ResponsePort.Post(_state);
            yield break;
        }
        */

        /// <summary>
        /// Http Get Handler
        /// </summary>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public IEnumerator<ITask> HttpGetHandler(HttpGet httpGet)
        {
            // Before display, the state is transformed using an embedded XSLT stylesheet
            httpGet.ResponsePort.Post(new HttpResponseType(System.Net.HttpStatusCode.OK, _state, _transform));
            yield break;
        }

        /// <summary>
        /// Replace Handler
        /// </summary>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public IEnumerator<ITask> ReplaceHandler(Replace replace)
        {
            _state = replace.Body;
            replace.ResponsePort.Post(DefaultReplaceResponseType.Instance);
            yield break;
        }

        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public IEnumerator<ITask> IncrementTickHandler(IncrementTick incrementTick)
        {
            TickCount count = _state.TickCounts.Find(
                delegate(TickCount test)
                {
                    return test.Name == incrementTick.Body.Name;
                }
            );

            if (count != null)
            {
                count.Count++;
            }
            incrementTick.ResponsePort.Post(DefaultUpdateResponseType.Instance);
            yield break;
        }

        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public IEnumerator<ITask> SetTickCountHandler(SetTickCount setTickCount)
        {
            TickCount count = _state.TickCounts.Find(
                delegate(TickCount test)
                {
                    return test.Name == setTickCount.Body.Name;
                }
            );

            if (count == null)
            {
                _state.TickCounts.Add(setTickCount.Body);
            }
            else
            {
                count.Count = setTickCount.Body.Count;
            }

            setTickCount.ResponsePort.Post(DefaultUpdateResponseType.Instance);
            yield break;
        }

        #region CODECLIP 03-9
        [ServiceHandler(ServiceHandlerBehavior.Teardown)]
        public IEnumerator<ITask> DropHandler(DsspDefaultDrop drop)
        {
            _clockPort.DsspDefaultDrop();
            _localClockPort.DsspDefaultDrop();

            base.DefaultDropHandler(drop);
            yield break;
        }
        #endregion

        #region CODECLIP 03-7
        private void RemoteNotifyTickHandler(rst4.IncrementTick incrementTick)
        {
            LogInfo("Got Tick from remote");
            _mainPort.Post(new IncrementTick("Remote"));
        }

        private void RemoteNotifyReplaceHandler(rst4.Replace replace)
        {
            LogInfo("Remote Tick Count: " + replace.Body.Ticks);
            _mainPort.Post(new SetTickCount(replace.Body.Ticks, "Remote"));
        }
        #endregion

        #region CODECLIP 03-8
        private void LocalNotifyTickHandler(rst4.IncrementTick incrementTick)
        {
            LogInfo("Got Tick from local");
            _mainPort.Post(new IncrementTick("Local"));
        }

        private void LocalNotifyReplaceHandler(rst4.Replace replace)
        {
            LogInfo("Local Tick Count: " + replace.Body.Ticks);
            _mainPort.Post(new SetTickCount(replace.Body.Ticks, "Local"));
        }
        #endregion
    }
}
