//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: FlexDialog.cs $ $Revision: 12 $
//-----------------------------------------------------------------------
#if !URT_MINCLR
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Net;
using System.Text;
using System.Windows.Forms;

using Microsoft.Ccr.Core;
using Microsoft.Dss.Core;
using Microsoft.Ccr.Adapters.WinForms;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.Core.DsspHttp;
using Microsoft.Dss.Core.DsspHttpUtilities;
using Microsoft.Dss.ServiceModel.Dssp;
using Microsoft.Dss.ServiceModel.DsspServiceBase;
using W3C.Soap;

using submgr = Microsoft.Dss.Services.SubscriptionManager;
using dir = Microsoft.Dss.Services.Directory;

namespace Microsoft.Robotics.Services.Sample.FlexDialog
{
    /// <summary>
    /// FlexibleDialogService - Dynamic dialog box
    /// </summary>
    [DisplayName("(User) Flexible Dialog")]
    [Description("Provides access to a configurable dialog box.")]
    [ContractAttribute(Contract.Identifier)]
    [DssServiceDescription("http://msdn.microsoft.com/library/dd126865.aspx")]
    class FlexDialogService : DsspServiceBase
    {
        [InitialStatePartner(Optional = true, ServiceUri = ServicePaths.Store + "/FlexDialog.config.xml")]
        FlexDialogState _state;

        [ServicePort("/flexdialog", AllowMultipleInstances = true)]
        FlexDialogOperations _mainPort = new FlexDialogOperations();

        [Partner("SubMgr", Contract = submgr.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.CreateAlways)]
        submgr.SubscriptionManagerPort _submgrPort = new submgr.SubscriptionManagerPort();

        [EmbeddedResource("Microsoft.Robotics.Services.Sample.FlexDialog.FlexDialog.user.xslt")]
        string _transform = string.Empty;

        Dictionary<string, Queue<DsspOperation>> _waitingNotifications = new Dictionary<string, Queue<DsspOperation>>();

        DsspHttpUtilitiesPort _utilities = null;

        FlexDialogForm _form;

        /// <summary>
        /// Constructor for service
        /// </summary>
        /// <param name="creationPort"></param>
        public FlexDialogService(DsspServiceCreationPort creationPort) :
                base(creationPort)
        {
        }

        protected override void Start()
        {
            if (_state == null)
            {
                _state = new FlexDialogState();
            }

            SpawnIterator(DoStart);
        }

        IEnumerator<ITask> DoStart()
        {
            try
            {
                _utilities = DsspHttpUtilitiesService.Create(Environment);

                RunForm runForm = new RunForm(CreateForm);

                WinFormsServicePort.Post(runForm);
                yield return Arbiter.Choice(
                    runForm.pResult,
                    EmptyHandler,
                    EmptyHandler
                );

                FormInvoke invoke = new FormInvoke(
                    delegate
                    {
                        _form.Initialize(_state);
                    }
                );

                WinFormsServicePort.Post(invoke);
                yield return Arbiter.Choice(
                    invoke.ResultPort,
                    EmptyHandler,
                    EmptyHandler
                );

            }
            finally
            {
                FinishStarting();
            }
        }

        void FinishStarting()
        {
            base.Start();
        }

        Form CreateForm()
        {
            _form = new FlexDialogForm(ServiceForwarder<FlexDialogOperations>(ServiceInfo.Service));

            return _form;
        }

        #region Basic Dssp messages

        /// <summary>
        /// OnGet - Return the state of the dialog 
        /// </summary>
        /// <param name="get"></param>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public void OnGet(Get get)
        {
            get.ResponsePort.Post(_state);
        }

        /// <summary>
        /// DoSendNotification - Send a notification
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message"></param>
        public void DoSendNotification<T>(T message)
            where T : DsspOperation
        {
            SendNotification<T>(_submgrPort, message);
            foreach (string session in _waitingNotifications.Keys)
            {
                _waitingNotifications[session].Enqueue(message);
            }
        }

        /// <summary>
        /// OnHttpGet - Return state as a web page
        /// </summary>
        /// <param name="get"></param>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public void OnHttpGet(HttpGet get)
        {
            get.ResponsePort.Post(new HttpResponseType(HttpStatusCode.OK, _state, _transform));
        }

        /// <summary>
        /// OnHttpQuery - Handle Http Query requests
        /// </summary>
        /// <param name="query"></param>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public void OnHttpQuery(HttpQuery query)
        {
            string session = query.Body.Query["Session"];
            if (string.IsNullOrEmpty(session))
            {
                query.ResponsePort.Post(new HttpResponseType(HttpStatusCode.OK, _state, _transform));
                return;
            }

            if (!_waitingNotifications.ContainsKey(session))
            {
                _waitingNotifications.Add(session, new Queue<DsspOperation>());
            }

            Queue<DsspOperation> queue = _waitingNotifications[session];
            HttpNotification notification;

            if (queue.Count == 0)
            {
                notification = new HttpNotification();
                notification.Operation = "None";
            }
            else
            {
                DsspOperation operation = queue.Dequeue();
                notification = HttpNotification.FromOperation(operation);
            }

            query.ResponsePort.Post(new HttpResponseType(notification));
        }

        /// <summary>
        /// OnHttpPost - Handle Http Post requests to update the dialog
        /// </summary>
        /// <param name="post"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public IEnumerator<ITask> OnHttpPost(HttpPost post)
        {
            Fault fault = null;

            NameValueCollection parameters = null;

            ReadFormData readForm = new ReadFormData(post);
            _utilities.Post(readForm);
            yield return Arbiter.Choice(
                readForm.ResultPort,
                delegate(NameValueCollection success)
                {
                    parameters = success;
                },
                delegate(Exception e)
                {
                    fault = Fault.FromException(e);
                }
            );

            string operation = parameters["Operation"];

            if (operation == "UpdateControl")
            {
                UpdateControl message = new UpdateControl();
                FlexControl update = message.Body;
                update.Id = parameters["ID"];
                update.Value = parameters["Value"];
                update.Text = parameters["Text"];

                FlexControl control = _state.Controls.Find(update.CompareId);

                if (control == null)
                {
                    post.ResponsePort.Post(Fault.FromCodeSubcodeReason(
                        FaultCodes.Receiver,
                        DsspFaultCodes.UnknownEntry,
                        "Cannot find control with ID: " + update.Id
                    ));
                    yield break;
                }
                update.ControlType = control.ControlType;

                yield return Arbiter.ExecuteToCompletion(
                    TaskQueue,
                    new IterativeTask<UpdateControl>(message, OnUpdateControl)
                );

                post.ResponsePort.Post(new HttpResponseType(DefaultUpdateResponseType.Instance));
            }
            else if (operation == "ButtonPress")
            {
                ButtonPress message = new ButtonPress();
                ButtonPressRequest request = message.Body;
                request.Id = parameters["ID"];
                request.Pressed = bool.Parse(parameters["Pressed"]);

                OnButtonPress(message);

                post.ResponsePort.Post(new HttpResponseType(DefaultUpdateResponseType.Instance));
            }
            else
            {
                fault = Fault.FromCodeSubcodeReason(
                    FaultCodes.Receiver,
                    DsspFaultCodes.MessageNotSupported,
                    "Unknown operation: " + operation
                );
            }

            if (fault != null)
            {
                post.ResponsePort.Post(fault);
                yield break;
            }
        }

        /// <summary>
        /// OnSubscribe - Handle subscription messages
        /// </summary>
        /// <param name="subscribe"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public IEnumerator<ITask> OnSubscribe(Subscribe subscribe)
        {
            SubscribeRequestType request = subscribe.Body;

            yield return Arbiter.Choice(
                SubscribeHelper(_submgrPort, request, subscribe.ResponsePort),
                EmptyHandler,
                EmptyHandler
            );
        }

        #endregion

        #region Control management messages

        /// <summary>
        /// OnInsertControl - Insert requested control into the dialog
        /// </summary>
        /// <param name="insert"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public IEnumerator<ITask> OnInsertControl(InsertControl insert)
        {
            if (_state.Controls.Exists(insert.Body.CompareId) ||
                _state.Buttons.Exists(insert.Body.CompareId))
            {
                insert.ResponsePort.Post(
                    Fault.FromCodeSubcodeReason(
                        FaultCodes.Receiver,
                        DsspFaultCodes.DuplicateEntry,
                        "A control with the same ID already exists: " + insert.Body.Id
                    )
                );
            }
            else
            {
                Fault fault = null;
                FormInvoke invoke = new FormInvoke(
                    delegate
                    {
                        _form.InsertControl(insert.Body);
                    }
                );

                WinFormsServicePort.Post(invoke);
                yield return Arbiter.Choice(
                    invoke.ResultPort,
                    EmptyHandler,
                    delegate(Exception e)
                    {
                        fault = Fault.FromException(e);
                    }
                );

                if (fault != null)
                {
                    insert.ResponsePort.Post(fault);
                }
                else
                {
                    _state.Controls.Add(insert.Body);
                    insert.ResponsePort.Post(DefaultUpdateResponseType.Instance);
                    DoSendNotification(insert);
                }
            }
        }

        /// <summary>
        /// OnDeleteControl - Delete requested control from dialog
        /// </summary>
        /// <param name="delete"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public IEnumerator<ITask> OnDeleteControl(DeleteControl delete)
        {
            FlexControl existing = _state.Controls.Find(delete.Body.CompareId);
            if (existing == null)
            {
                delete.ResponsePort.Post(
                    Fault.FromCodeSubcodeReason(
                        FaultCodes.Receiver,
                        DsspFaultCodes.UnknownEntry,
                        "A control with the requested ID does not exist: " + delete.Body.Id
                    )
                );
            }
            else
            {
                Fault fault = null;
                FormInvoke invoke = new FormInvoke(
                    delegate
                    {
                        _form.DeleteControl(delete.Body);
                    }
                );

                WinFormsServicePort.Post(invoke);
                yield return Arbiter.Choice(
                    invoke.ResultPort,
                    EmptyHandler,
                    delegate(Exception e)
                    {
                        fault = Fault.FromException(e);
                    }
                );

                if (fault != null)
                {
                    delete.ResponsePort.Post(fault);
                }
                else
                {
                    _state.Controls.Remove(existing);
                    delete.ResponsePort.Post(DefaultDeleteResponseType.Instance);
                    DoSendNotification(delete);
                }
            }
        }

        /// <summary>
        /// OnUpdateControl - Update the requested control on the dialog
        /// </summary>
        /// <param name="update"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public IEnumerator<ITask> OnUpdateControl(UpdateControl update)
        {
            int index = _state.Controls.FindIndex(update.Body.CompareId);
            if (index < 0)
            {
                update.ResponsePort.Post(
                    Fault.FromCodeSubcodeReason(
                        FaultCodes.Receiver,
                        DsspFaultCodes.UnknownEntry,
                        "A control with the requested ID does not exist: " + update.Body.Id
                    )
                );
            }
            else
            {
                bool isInternal = update.GetHeader<InternalUpdate>() != null;

                if (!isInternal)
                {
                    Fault fault = null;
                    FormInvoke invoke = new FormInvoke(
                        delegate
                        {
                            _form.UpdateControl(update.Body);
                        }
                    );

                    WinFormsServicePort.Post(invoke);
                    yield return Arbiter.Choice(
                        invoke.ResultPort,
                        EmptyHandler,
                        delegate(Exception e)
                        {
                            fault = Fault.FromException(e);
                        }
                    );

                    if (fault != null)
                    {
                        update.ResponsePort.Post(fault);
                        yield break;
                    }
                }

                _state.Controls[index] = update.Body;
                update.ResponsePort.Post(DefaultUpdateResponseType.Instance);
                DoSendNotification(update);
            }
        }

        #endregion

        #region Button management messages

        /// <summary>
        /// OnInsertButton - Insert requested button into the dialog
        /// </summary>
        /// <param name="insert"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public IEnumerator<ITask> OnInsertButton(InsertButton insert)
        {
            if (_state.Buttons.Exists(insert.Body.CompareId) ||
                _state.Controls.Exists(insert.Body.CompareId))
            {
                insert.ResponsePort.Post(
                    Fault.FromCodeSubcodeReason(
                        FaultCodes.Receiver,
                        DsspFaultCodes.DuplicateEntry,
                        "A button with the same ID already exists: " + insert.Body.Id
                    )
                );
            }
            else
            {
                Fault fault = null;
                FormInvoke invoke = new FormInvoke(
                    delegate
                    {
                        _form.InsertButton(insert.Body);
                    }
                );

                WinFormsServicePort.Post(invoke);
                yield return Arbiter.Choice(
                    invoke.ResultPort,
                    EmptyHandler,
                    delegate(Exception e)
                    {
                        fault = Fault.FromException(e);
                    }
                );

                if (fault != null)
                {
                    insert.ResponsePort.Post(fault);
                }
                else
                {
                    _state.Buttons.Add(insert.Body);
                    insert.ResponsePort.Post(DefaultUpdateResponseType.Instance);
                    DoSendNotification(insert);
                }
            }
        }

        /// <summary>
        /// OnDeleteButton - Delete requested button from the dialog
        /// </summary>
        /// <param name="delete"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public IEnumerator<ITask> OnDeleteButton(DeleteButton delete)
        {
            FlexButton existing = _state.Buttons.Find(delete.Body.CompareId);
            if (existing == null)
            {
                delete.ResponsePort.Post(
                    Fault.FromCodeSubcodeReason(
                        FaultCodes.Receiver,
                        DsspFaultCodes.UnknownEntry,
                        "A button with the requested ID does not exist: " + delete.Body.Id
                    )
                );
            }
            else
            {
                Fault fault = null;
                FormInvoke invoke = new FormInvoke(
                    delegate
                    {
                        _form.DeleteButton(delete.Body);
                    }
                );

                WinFormsServicePort.Post(invoke);
                yield return Arbiter.Choice(
                    invoke.ResultPort,
                    EmptyHandler,
                    delegate(Exception e)
                    {
                        fault = Fault.FromException(e);
                    }
                );

                if (fault != null)
                {
                    delete.ResponsePort.Post(fault);
                }
                else
                {
                    _state.Buttons.Remove(existing);
                    delete.ResponsePort.Post(DefaultDeleteResponseType.Instance);
                    DoSendNotification(delete);
                }
            }
        }

        /// <summary>
        /// OnUpdateButton - Update the requested button on the dialog
        /// </summary>
        /// <param name="update"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public IEnumerator<ITask> OnUpdateButton(UpdateButton update)
        {
            int index = _state.Buttons.FindIndex(update.Body.CompareId);
            if (index < 0)
            {
                update.ResponsePort.Post(
                    Fault.FromCodeSubcodeReason(
                        FaultCodes.Receiver,
                        DsspFaultCodes.UnknownEntry,
                        "A button with the requested ID does not exist: " + update.Body.Id
                    )
                );
            }
            else
            {
                bool isInternal = update.GetHeader<InternalUpdate>() != null;

                if (!isInternal)
                {
                    Fault fault = null;
                    FormInvoke invoke = new FormInvoke(
                        delegate
                        {
                            _form.UpdateButton(update.Body);
                        }
                    );

                    WinFormsServicePort.Post(invoke);
                    yield return Arbiter.Choice(
                        invoke.ResultPort,
                        EmptyHandler,
                        delegate(Exception e)
                        {
                            fault = Fault.FromException(e);
                        }
                    );

                    if (fault != null)
                    {
                        update.ResponsePort.Post(fault);
                        yield break;
                    }
                }

                _state.Buttons[index] = update.Body;
                update.ResponsePort.Post(DefaultUpdateResponseType.Instance);
                DoSendNotification(update);
            }
        }

        /// <summary>
        /// OnButtonPress - Send notification when a button is pressed
        /// </summary>
        /// <param name="press"></param>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public void OnButtonPress(ButtonPress press)
        {
            press.ResponsePort.Post(DefaultUpdateResponseType.Instance);
            DoSendNotification(press);
        }

        #endregion

        #region Dialog Control messages

        /// <summary>
        /// OnShow - Make the dialog visible / invisible
        /// </summary>
        /// <param name="show"></param>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public void OnShow(Show show)
        {
            FormInvoke invoke = new FormInvoke(
                delegate
                {
                    _form.SetVisibility(show.Body.Show);
                }
            );
            WinFormsServicePort.Post(invoke);

            _state.Visible = show.Body.Show;
            show.ResponsePort.Post(DefaultUpdateResponseType.Instance);
            DoSendNotification(show);
        }

        /// <summary>
        /// OnSetTitle - Set the title for the dialog
        /// </summary>
        /// <param name="setTitle"></param>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public void OnSetTitle(SetTitle setTitle)
        {
            FormInvoke invoke = new FormInvoke(
                delegate
                {
                    _form.SetTitle(setTitle.Body.Title);
                }
            );
            WinFormsServicePort.Post(invoke);

            _state.Title = setTitle.Body.Title;
            setTitle.ResponsePort.Post(DefaultUpdateResponseType.Instance);
            DoSendNotification(setTitle);
        }

        /// <summary>
        /// OnHandOff - Hand control to another dialog (web only)
        /// </summary>
        /// <param name="handOff"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public IEnumerator<ITask> OnHandOff(HandOff handOff)
        {
            Fault fault = null;
            ServiceInfoType[] services = null;

            dir.QueryRequestType request = new dir.QueryRequestType(
                new ServiceInfoType(Contract.Identifier)
            );
            dir.Query query = new dir.Query(request);
            query.TimeSpan = new TimeSpan(0, 0, 2);
            DirectoryPort.Post(query);

            yield return Arbiter.Choice(
                query.ResponsePort,
                delegate(dir.QueryResponseType success)
                {
                    services = success.RecordList;
                },
                delegate(Fault f)
                {
                    fault = f;
                }
            );

            if (fault != null)
            {
                handOff.ResponsePort.Post(fault);
                yield break;
            }

            foreach (ServiceInfoType service in services)
            {
                FlexDialogOperations fwd = ServiceForwarder<FlexDialogOperations>(service.Service);
                FlexDialogState state = null;

                Get get = new Get();
                get.TimeSpan = new TimeSpan(0, 0, 2);
                fwd.Post(get);

                yield return Arbiter.Choice(
                    get.ResponsePort,
                    delegate(FlexDialogState success)
                    {
                        state = success;
                    },
                    delegate(Fault f)
                    {
                        fault = f;
                    }
                );

                if (state != null && state.Title == handOff.Body.Title)
                {
                    handOff.Body.Service = service.HttpServiceAlias.ToString();
                    DoSendNotification(handOff);
                    handOff.ResponsePort.Post(handOff.Body);
                    yield break;
                }
            }

            handOff.ResponsePort.Post(
                Fault.FromCodeSubcodeReason(
                    FaultCodes.Receiver,
                    DsspFaultCodes.UnknownEntry,
                    "Unable to find dialog with title: " + handOff.Body.Title
                )
            );
        }

        #endregion
    }
}
#endif