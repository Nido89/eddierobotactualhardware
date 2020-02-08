///////////////////////////////////////////////////////////////////////////////
// Activity: program.activity
// Diagram service implementation
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
using user = Microsoft.Robotics.Proxy;
using drive = Microsoft.Robotics.Services.Drive.Proxy;

namespace Robotics.AutonomusDrive.Diagram
{
    [DisplayName("AutonomusDrive")]
    [Description("A user defined activity.")]
    [dssa.Contract(Contract.Identifier)]
    public class DiagramService : dssm.DsspServiceBase
    {
        // Service state
        [dssa.InitialStatePartner(Optional = true)]
        private DiagramState _state;

        // Service operations port
        [dssa.ServicePort("/AutonomusDrive", AllowMultipleInstances = true)]
        private DiagramOperations _mainPort = new DiagramOperations();

        #region Partner services

        [dssa.Partner("SubMgr", Contract = submgr.Contract.Identifier, CreationPolicy = dssa.PartnerCreationPolicy.CreateAlways)]
        private submgr.SubscriptionManagerPort _subMgr = new submgr.SubscriptionManagerPort();

        // Partner: UserDriveInSquare, Contract: http://schemas.microsoft.com/robotics/2008/09/driveinsquare.user.html
        [dssa.Partner("UserDriveInSquare", Contract = user.Contract.Identifier, CreationPolicy = dssa.PartnerCreationPolicy.UsePartnerListEntry)]
        user.DriveInSquareOperations _userDriveInSquarePort = new user.DriveInSquareOperations();

        // Partner: ObstacleAvoidanceDrive, Contract: http://schemas.microsoft.com/robotics/2006/05/drive.html
        [dssa.Partner("ObstacleAvoidanceDrive", Contract = drive.Contract.Identifier, CreationPolicy = dssa.PartnerCreationPolicy.UsePartnerListEntry)]
        drive.DriveOperations _obstacleAvoidanceDrivePort = new drive.DriveOperations();

        #endregion

        public DiagramService(dssp.DsspServiceCreationPort creationPort)
            : base(creationPort)
        {
        }

        protected override void Start()
        {
            // If there was no initial state partner, then the service state will be null.
            if (_state == null)
            {
                // The state MUST be created before the service starts processing messages.
                _state = new DiagramState();
            }

            // The rest of the start process requires the ability to wait for responses from
            // services and from the start handler (if any). So execution now proceeds in an
            // iterator function.
            SpawnIterator(DoStart);
        }

        private IEnumerator<ccr.ITask> DoStart()
        {

            // Start the RunHandler, this represents the parts of the diagram that
            // are are not run in the context of an operation or notification.
            StartHandler start = new StartHandler(this, Environment.TaskQueue);
            SpawnIterator(start.RunHandler);
            // Wait until the RunHandler has completed.
            yield return ccr.Arbiter.Receive(false, start.Complete, EmptyHandler);

            // Start operation handlers and insert into directory service.
            StartHandlers();

            yield break;
        }

        private void StartHandlers()
        {
            // Activate message handlers for this service and insert into the directory.
            base.Start();
        }

        #region Standard DSS message handlers

        [dssa.ServiceHandler(dssa.ServiceHandlerBehavior.Concurrent)]
        public virtual IEnumerator<ccr.ITask> GetHandler(Get get)
        {
            get.ResponsePort.Post(_state);
            yield break;
        }

        [dssa.ServiceHandler(dssa.ServiceHandlerBehavior.Concurrent)]
        public virtual IEnumerator<ccr.ITask> HttpGetHandler(dssh.HttpGet httpGet)
        {
            httpGet.ResponsePort.Post(new dssh.HttpResponseType(_state));
            yield break;
        }

        [dssa.ServiceHandler(dssa.ServiceHandlerBehavior.Exclusive)]
        public virtual IEnumerator<ccr.ITask> ReplaceHandler(Replace replace)
        {
            _state = replace.Body;

            replace.ResponsePort.Post(dssp.DefaultReplaceResponseType.Instance);
            base.SendNotification<Replace>(_subMgr, replace);

            yield break;
        }

        [dssa.ServiceHandler(dssa.ServiceHandlerBehavior.Concurrent)]
        public virtual IEnumerator<ccr.ITask> SubscribeHandler(Subscribe subscribe)
        {
            dssp.SubscribeRequestType request = subscribe.Body;

            yield return ccr.Arbiter.Choice(
                SubscribeHelper(_subMgr, request, subscribe.ResponsePort),
                delegate(ccr.SuccessResult success)
                {
                    base.SendNotificationToTarget<Replace>(request.Subscriber, _subMgr, _state);
                },
                delegate(Exception e) { }
            );
        }

        #endregion

        #region Custom message handlers

        [dssa.ServiceHandler(dssa.ServiceHandlerBehavior.Independent)]
        public virtual IEnumerator<ccr.ITask> ActionHandler(Action message)
        {
            // Empty handler. Respond with default response type.
            message.ResponsePort.Post(new ActionResponse());
            yield break;
        }

        #endregion

        #region StartHandler class

        class StartHandler : HandlerBase
        {
            ///////////////////////////////////////////////////////////////////
            // program.activity.Start+start
            // snippet.element
            // __use__1.snippet0.call - __use__1.snippet0.call.iftype
            // [(__use__.snippet.snippet.noop - __use__.snippet.snippet.expr - __use__.snippet.snippet.join)] - __use__.snippet.call - __use__.snippet.call.iftype
            ///////////////////////////////////////////////////////////////////

            public StartHandler(DiagramService service, ccr.DispatcherQueue queue)
                : base(service, queue)
            {
            }

            ///////////////////////////////////////////////////////////////////
            // program.activity.Start+start
            // __use__1.snippet0.call - __use__1.snippet0.call.iftype
            // snippet.element
            ///////////////////////////////////////////////////////////////////

            public IEnumerator<ccr.ITask> RunHandler()
            {
                Increment();

                Increment();
                Activate(
                    ccr.Arbiter.Choice(
                        UserDriveInSquarePort.Get(new dssp.GetRequestType()),
                        OnGetRequestTypeSuccess,
                        delegate(soap.Fault fault)
                        {
                            base.FaultHandler(fault, @"UserDriveInSquarePort.Get(new dssp.GetRequestType())");
                            Decrement();
                        }
                    )
                );

                Decrement();

                yield return WaitUntilComplete();
            }

            void OnGetRequestTypeSuccess(user.DriveInSquareState response)
            {
                drive.EnableDriveRequest request = new drive.EnableDriveRequest();
                request.Enable = true;
                ObstacleAvoidanceDrivePort.EnableDrive(request);

                Decrement();
            }

            class JoinAlpha
            {
                public string Enable;

                public JoinAlpha()
                {
                }

                public JoinAlpha(object[] args)
                {
                    Enable = args[0].ToString();
                }
            }

            ccr.Port<object>[] _joinAlphaPorts = new ccr.Port<object>[1]{
                new ccr.Port<object>()
            };
        }

        #endregion

        #region Handler utility base class

        class HandlerBase : ccr.CcrServiceBase
        {
            ccr.Port<ccr.EmptyValue> _complete = new ccr.Port<ccr.EmptyValue>();
            ccr.Port<ccr.EmptyValue> _shutdown = new ccr.Port<ccr.EmptyValue>();
            List<ccr.Port<object>[]> _joins = new List<ccr.Port<object>[]>();
            DiagramService _service;
            int _count;

            public HandlerBase(DiagramService service, ccr.DispatcherQueue queue)
                : base(queue)
            {
                _service = service;
            }

            protected DiagramState State
            {
                get { return _service._state; }
            }

            bool _stateChanged;

            public bool StateChanged
            {
                get { return _stateChanged; }
                set { _stateChanged = value; }
            }

            protected user.DriveInSquareOperations UserDriveInSquarePort
            {
                get { return _service._userDriveInSquarePort; }
            }

            protected drive.DriveOperations ObstacleAvoidanceDrivePort
            {
                get { return _service._obstacleAvoidanceDrivePort; }
            }

            public ccr.Port<ccr.EmptyValue> Complete
            {
                get { return _complete; }
            }

            protected void Increment()
            {
                System.Threading.Interlocked.Increment(ref _count);
            }

            protected void Decrement()
            {
                int offset = 0;

                foreach (ccr.Port<object>[] join in _joins)
                {
                    bool complete = true;
                    int joinCount = 0;

                    foreach (ccr.Port<object> sink in join)
                    {
                        if (sink.ItemCount != 0)
                        {
                            joinCount += sink.ItemCount;
                        }
                        else
                        {
                            complete = false;
                        }
                    }
                    if (complete == false)
                    {
                        offset += joinCount;
                    }
                }

                if (System.Threading.Interlocked.Decrement(ref _count) <= offset)
                {
                    _shutdown.Post(ccr.EmptyValue.SharedInstance);
                    _complete.Post(ccr.EmptyValue.SharedInstance);
                }
            }

            protected void Decrement(int count)
            {
                for(int i = 0; i < count; i++)
                {
                    Decrement();
                }
            }

            protected void RegisterJoin(ccr.Port<object>[] join)
            {
                _joins.Add(join);
            }

            protected ccr.ITask WaitUntilComplete()
            {
                return ccr.Arbiter.Receive(false, _shutdown, NullDelegate);
            }

            protected static void NullDelegate(ccr.EmptyValue token)
            {
            }

            protected void SendNotification<T>(object notification)
                where T : dssp.DsspOperation, new()
            {
                _service.SendNotification<T>(_service._subMgr, notification);
            }

            protected void FaultHandler(soap.Fault fault, string msg)
            {
                _service.LogError(null, msg, fault);
            }

            protected static string Stringize(object obj)
            {
                if (obj == null)
                {
                    return string.Empty;
                }
                else
                {
                    return obj.ToString();
                }
            }

            protected void UnhandledResponse(W3C.Soap.Fault fault)
            {
                _service.LogError("Unhandled fault response from partner service", fault);
                Decrement();
            }

            protected void UnhandledResponse<T>(T response)
            {
                Decrement();
            }

            #region Type Cast Functions

            protected static bool BoolCast(object obj)
            {
                if (obj != null && obj is IConvertible)
                {
                    try
                    {
                        return ((IConvertible)obj).ToBoolean(System.Globalization.CultureInfo.InvariantCulture);
                    }
                    catch (FormatException)
                    {
                    }
                }
                return false;
            }

            protected static byte ByteCast(object obj)
            {
                if (obj != null && obj is IConvertible)
                {
                    try
                    {
                        return ((IConvertible)obj).ToByte(System.Globalization.CultureInfo.InvariantCulture);
                    }
                    catch (FormatException)
                    {
                    }
                }
                return 0;
            }

            protected static char CharCast(object obj)
            {
                if (obj != null && obj is IConvertible)
                {
                    try
                    {
                        return ((IConvertible)obj).ToChar(System.Globalization.CultureInfo.InvariantCulture);
                    }
                    catch (FormatException)
                    {
                    }
                }
                return '\0';
            }

            protected static decimal DecimalCast(object obj)
            {
                if (obj != null && obj is IConvertible)
                {
                    try
                    {
                        return ((IConvertible)obj).ToDecimal(System.Globalization.CultureInfo.InvariantCulture);
                    }
                    catch (FormatException)
                    {
                    }
                }
                return 0;
            }

            protected static double DoubleCast(object obj)
            {
                if (obj != null && obj is IConvertible)
                {
                    try
                    {
                        return ((IConvertible)obj).ToDouble(System.Globalization.CultureInfo.InvariantCulture);
                    }
                    catch (FormatException)
                    {
                    }
                }
                return 0;
            }

            protected static float FloatCast(object obj)
            {
                if (obj != null && obj is IConvertible)
                {
                    try
                    {
                        return ((IConvertible)obj).ToSingle(System.Globalization.CultureInfo.InvariantCulture);
                    }
                    catch (FormatException)
                    {
                    }
                }
                return 0;
            }

            protected static int IntCast(object obj)
            {
                if (obj != null && obj is IConvertible)
                {
                    try
                    {
                        return ((IConvertible)obj).ToInt32(System.Globalization.CultureInfo.InvariantCulture);
                    }
                    catch (FormatException)
                    {
                    }
                }
                return 0;
            }

            protected static long LongCast(object obj)
            {
                if (obj != null && obj is IConvertible)
                {
                    try
                    {
                        return ((IConvertible)obj).ToInt64(System.Globalization.CultureInfo.InvariantCulture);
                    }
                    catch (FormatException)
                    {
                    }
                }
                return 0;
            }

            protected static sbyte SByteCast(object obj)
            {
                if (obj != null && obj is IConvertible)
                {
                    try
                    {
                        return ((IConvertible)obj).ToSByte(System.Globalization.CultureInfo.InvariantCulture);
                    }
                    catch (FormatException)
                    {
                    }
                }
                return 0;
            }

            protected static short ShortCast(object obj)
            {
                if (obj != null && obj is IConvertible)
                {
                    try
                    {
                        return ((IConvertible)obj).ToInt16(System.Globalization.CultureInfo.InvariantCulture);
                    }
                    catch (FormatException)
                    {
                    }
                }
                return 0;
            }

            protected static uint UIntCast(object obj)
            {
                if (obj != null && obj is IConvertible)
                {
                    try
                    {
                        return ((IConvertible)obj).ToUInt32(System.Globalization.CultureInfo.InvariantCulture);
                    }
                    catch (FormatException)
                    {
                    }
                }
                return 0;
            }

            protected static ulong ULongCast(object obj)
            {
                if (obj != null && obj is IConvertible)
                {
                    try
                    {
                        return ((IConvertible)obj).ToUInt64(System.Globalization.CultureInfo.InvariantCulture);
                    }
                    catch (FormatException)
                    {
                    }
                }
                return 0;
            }

            protected static ushort UShortCast(object obj)
            {
                if (obj != null && obj is IConvertible)
                {
                    try
                    {
                        return ((IConvertible)obj).ToUInt16(System.Globalization.CultureInfo.InvariantCulture);
                    }
                    catch (FormatException)
                    {
                    }
                }
                return 0;
            }

            #endregion

            #region List Functions

            protected List<T> ListAdd<T>(List<T> list, T item)
            {
                List<T> output = new List<T>(list);
                output.Add(item);
                return output;
            }

            protected List<T> ListConcat<T>(List<T> head, List<T> tail)
            {
                List<T> output = new List<T>(head);
                output.AddRange(tail);
                return output;
            }

            protected int ListIndex<T>(List<T> list, T item)
            {
                return list.IndexOf(item);
            }

            protected List<T> ListRemove<T>(List<T> list, int index)
            {
                List<T> output = new List<T>(list);
                output.RemoveAt(index);
                return output;
            }

            protected List<T> ListReverse<T>(List<T> list)
            {
                List<T> output = new List<T>(list);
                output.Reverse();
                return output;
            }

            protected List<T> ListSort<T>(List<T> list)
            {
                List<T> output = new List<T>(list);
                output.Sort();
                return output;
            }

            protected List<T> ListInsert<T>(List<T> list, T item, int index)
            {
                List<T> output = new List<T>(list);
                output.Insert(index, item);
                return output;
            }

            #endregion
        }

        #endregion
    }
}
