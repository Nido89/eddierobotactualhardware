//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: SpeechRecognizerGui.cs $ $Revision: 5 $
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Net;
using System.Xml;
using W3C.Soap;

using Microsoft.Ccr.Core;
using Microsoft.Dss.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.Core.DsspHttp;
using Microsoft.Dss.Core.DsspHttpUtilities;
using Microsoft.Dss.Core.Utilities;
using mnt = Microsoft.Dss.Services.MountService;
using Microsoft.Dss.ServiceModel.Dssp;
using Microsoft.Dss.ServiceModel.DsspServiceBase;

using srgui = Microsoft.Robotics.Technologies.Speech.SpeechRecognizerGui;
using sr = Microsoft.Robotics.Technologies.Speech.SpeechRecognizer.Proxy;


namespace Microsoft.Robotics.Technologies.Speech.SpeechRecognizerGui
{
    /// <summary>
    /// SpeechRecognizerGui - Interface for configuring the Speech Recognizer
    /// </summary>
    [DisplayName("(User) SpeechRecognizerGui")]
    [Description("The SpeechRecognizerGui Service")]
    [Contract(Contract.Identifier)]
    [DssServiceDescription("http://msdn.microsoft.com/library/bb608250.aspx")]
    public class SpeechRecognizerGui : DsspServiceBase
    {
        /// <summary>
        /// Service state
        /// </summary>
        [ServiceState(StateTransform="Microsoft.Robotics.Technologies.Speech.SpeechRecognizerGui.Transforms.Default.user.xslt")]
        private SpeechRecognizerGuiState _state = new SpeechRecognizerGuiState();
        
        /// <summary>
        /// Main port
        /// </summary>
        [ServicePort("/speechrecognizergui", AllowMultipleInstances=false)]
        private SpeechRecognizerGuiOperations _mainPort = new SpeechRecognizerGuiOperations();

        /// <summary>
        /// Speech recognizer partner ports
        /// </summary>
        [Partner("SpeechRecognizer", Contract = sr.Contract.Identifier, CreationPolicy
            = PartnerCreationPolicy.UseExistingOrCreate)]
        sr.SpeechRecognizerOperations _srPort = new sr.SpeechRecognizerOperations();
        sr.SpeechRecognizerOperations _srNotifyPort = new sr.SpeechRecognizerOperations();

        /// <summary>
        /// HTTP utilities port
        /// </summary>
        DsspHttpUtilitiesPort _httpUtilities = new DsspHttpUtilitiesPort();

        /// <summary>
        /// Maximum number of past speech events received from the speech recognizer that shall
        /// be retained
        /// </summary>
        private const int MaxSpeechEventsToRetain = 20;

        /// <summary>
        /// Block size with which files are written to the mount service
        /// </summary>
        private const int MountServiceWriteBlockSize = 32768;

        /// <summary>
        /// Default service constructor
        /// </summary>
        public SpeechRecognizerGui(DsspServiceCreationPort creationPort) : 
                base(creationPort)
        {
        }
        
        /// <summary>
        /// Service start
        /// </summary>
        protected override void Start()
        {
			base.Start();

            // Needed for HttpPost
            _httpUtilities = DsspHttpUtilitiesService.Create(Environment);

            // Register handlers for notification from speech recognizer
            MainPortInterleave.CombineWith(new Interleave(
                new ExclusiveReceiverGroup(
                    Arbiter.Receive<sr.Replace>(true, _srNotifyPort, SpeechRecognizerReplaceHandler),
                    Arbiter.Receive<sr.SpeechDetected>(true, _srNotifyPort, SpeechDetectedHandler),
                    Arbiter.Receive<sr.SpeechRecognized>(true, _srNotifyPort, SpeechRecognizedHandler),
                    Arbiter.Receive<sr.SpeechRecognitionRejected>(true, _srNotifyPort, SpeechRecognitionRejectedHandler)
                ),
                new ConcurrentReceiverGroup()
            ));

            _srPort.Subscribe(_srNotifyPort);
        }

        /// <summary>
        /// Events query handler
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public IEnumerator<ITask> EventsQueryHandler(EventsQuery query)
        {
            EventsQueryRequestType request = query.Body;
            EventsQueryResponse response = new EventsQueryResponse();

            response.Events = new List<EventListEntry>();
            foreach (EventListEntry entry in _state.SpeechEvents)
            {
                if (entry.Timestamp > request.NewerThanTimestamp)
                {
                    response.Events.Add(entry);
                }
            }

            query.ResponsePort.Post(response);
            yield break;
        }

        /// <summary>
        /// SpeechRecognizer state query handler
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public IEnumerator<ITask> SpeechRecognizerStateQueryHandler(SpeechRecognizerStateQuery query)
        {
            query.ResponsePort.Post(_state.SpeechRecognizerState);
            yield break;
        }

        /// <summary>
        /// Http query handler
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public IEnumerator<ITask> HttpQueryHandler(HttpQuery query)
        {
            HttpQueryRequestType request = query.Body;
            NameValueCollection parameters = request.Query;
            string cmd = parameters["cmd"];
            if (cmd != null)
            {
                cmd = cmd.ToLowerInvariant();
                switch (cmd)
                {
                    case "events":
                        yield return new IterativeTask(delegate
                        {
                            return EventsHttpQueryHandler(query, parameters);
                        });
                        yield break;

                    case "speechrecognizerstate":
                        yield return new IterativeTask(delegate
                        {
                            return SpeechRecognizerStateHttpQueryHandler(query);
                        });
                        yield break;
                }
            }

            // Unknown query
            query.ResponsePort.Post(new HttpResponseType(
                HttpStatusCode.BadRequest,
                Fault.FromCodeSubcodeReason(
                    FaultCodes.Sender,
                    DsspFaultCodes.ActionNotSupported,
                    "Unknown query"
                )
            ));
            yield break;
        }

        /// <summary>
        /// Events HTTP query handler
        /// </summary>
        /// <param name="query"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private IEnumerator<ITask> EventsHttpQueryHandler(HttpQuery query, NameValueCollection parameters)
        {
            // Parse timestamp (TryParse sets the timestamp to zero if parsing fails)
            long timestamp;
            long.TryParse(parameters["timestamp"], out timestamp);

            // Send EventsQuery message to own service
            EventsQueryRequestType eventsQueryRequest = new EventsQueryRequestType();
            eventsQueryRequest.NewerThanTimestamp = timestamp;
            EventsQuery eventsQuery = new EventsQuery();
            eventsQuery.Body = eventsQueryRequest;
            _mainPort.Post(eventsQuery);

            DsspResponsePort<EventsQueryResponse> eventsResponse = eventsQuery.ResponsePort;
            yield return (Choice)eventsResponse;
            Fault eventsFault = (Fault)eventsResponse;
            if (eventsFault != null)
            {
                LogError(eventsFault);
                query.ResponsePort.Post(new HttpResponseType(
                    HttpStatusCode.InternalServerError,
                    eventsFault
                ));
                yield break;
            }

            // Return EventsQuery result
            query.ResponsePort.Post(new HttpResponseType(
                HttpStatusCode.OK,
                (EventsQueryResponse)eventsResponse
            ));
            yield break;
        }

        /// <summary>
        /// SpeechRecognizerState HTTP query handler
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        private IEnumerator<ITask> SpeechRecognizerStateHttpQueryHandler(HttpQuery query)
        {
            // Send SpeechRecognizerStateQuery message to own service
            SpeechRecognizerStateQuery srStateQuery = new SpeechRecognizerStateQuery();
            _mainPort.Post(srStateQuery);

            DsspResponsePort<sr.SpeechRecognizerState> srStateResponse = srStateQuery.ResponsePort;
            yield return (Choice)srStateResponse;
            Fault srStateFault = (Fault)srStateResponse;
            if (srStateFault != null)
            {
                LogError(srStateFault);
                query.ResponsePort.Post(new HttpResponseType(
                    HttpStatusCode.InternalServerError,
                    srStateFault
                ));
                yield break;
            }

            // Return SpeechRecognizerStateQuery result
            query.ResponsePort.Post(new HttpResponseType(
                HttpStatusCode.OK,
                (sr.SpeechRecognizerState)srStateResponse
            ));
            yield break;
        }

        /// <summary>
        /// Http post handler
        /// </summary>
        /// <param name="post">Request from the web page</param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public IEnumerator<ITask> HttpPostHandler(HttpPost post)
        {
            // Use helper to read form data
            ReadAllFormData readForm = new ReadAllFormData(post);
            _httpUtilities.Post(readForm);

            yield return (Choice)readForm.ResultPort;
            Exception exception = (Exception)readForm.ResultPort;
            if (exception != null)
            {
                LogError(exception);
                post.ResponsePort.Post(new HttpResponseType(
                    HttpStatusCode.OK,
                    Fault.FromException(exception)
                ));
                yield break;
            }

            HttpPostRequestData postData = (HttpPostRequestData)readForm.ResultPort;
            NameValueCollection parameters = postData.Parameters;
            string cmd = parameters["cmd"];
            if (cmd != null)
            {
                cmd = cmd.ToLowerInvariant();
                switch (cmd)
                {
                    case "savedictionary":
                        yield return new IterativeTask(
                            delegate
                            {
                                return SaveDictionaryHttpPostHandler(parameters, post);
                            }
                        );
                        yield break;

                    case "uploadsrgsfile":
                        yield return new IterativeTask(
                            delegate
                            {
                                return UploadSrgsFileHttpPostHandler(postData, post);
                            }
                        );
                        yield break;

                    case "useexistingsrgsfile":
                        yield return new IterativeTask(
                            delegate
                            {
                                return UseExistingSrgsFileHttpPostHandler(parameters, post);
                            }
                        );
                        yield break;
                }
            }

            PostHttpPostParameterError(post, "Unknown post command");
            yield break;
        }

        /// <summary>
        /// SaveDictionary HTTP post handler
        /// </summary>
        /// <param name="parameters">Dictionary entries</param>
        /// <param name="post">Request from the web page</param>
        /// <returns></returns>
        private IEnumerator<ITask> SaveDictionaryHttpPostHandler(NameValueCollection parameters, HttpPost post)
        {
            string[] entryTexts = parameters.GetValues("DictEntryText[]");
            string[] entrySemanticValues = parameters.GetValues("DictEntrySemanticValue[]");

            if (entryTexts == null || entrySemanticValues == null
                || entryTexts.Length != entrySemanticValues.Length)
            {
                PostHttpPostParameterError(post, "Both dictionary entry texts and semantic values "
                    + "must be posted, and occur in the same quantities");
            }

            // Set up replace request with new grammar dictionary for SpeechRecognizer service
            sr.SpeechRecognizerState srState = new sr.SpeechRecognizerState();
            srState.GrammarType = sr.GrammarType.DictionaryStyle;

            srState.DictionaryGrammar = new DssDictionary<string, string>();
            try
            {
                for (int i = 0; i < entryTexts.Length; i++)
                {
                    srState.DictionaryGrammar.Add(entryTexts[i].Trim(), entrySemanticValues[i].Trim());
                }
            }
            catch (Exception ex)
            {
                post.ResponsePort.Post(new HttpResponseType(
                    HttpStatusCode.OK,
                    Fault.FromException(ex)
                ));
                yield break;
            }

            // Post replace request to SpeechRecognizer service and check outcome
            sr.Replace replaceRequest = new sr.Replace(srState);
            _srPort.Post(replaceRequest);

            yield return (Choice)replaceRequest.ResponsePort;
            Fault fault = (Fault)replaceRequest.ResponsePort;
            if (fault != null)
            {
                post.ResponsePort.Post(new HttpResponseType(
                    HttpStatusCode.OK,
                    fault
                ));
                yield break;
            }

            _state.SpeechRecognizerState = srState;

            post.ResponsePort.Post(new HttpResponseType(
                HttpStatusCode.OK,
                HttpPostSuccess.Instance
            ));
            yield break;
        }

        /// <summary>
        /// UploadSrgsFile HTTP post handler
        /// </summary>
        /// <param name="postData"></param>
        /// <param name="post"></param>
        /// <returns></returns>
        private IEnumerator<ITask> UploadSrgsFileHttpPostHandler(HttpPostRequestData postData, HttpPost post)
        {
            string mountPointFileLocation = postData.Parameters["MountPointSaveLocation"];
            if (string.IsNullOrEmpty(mountPointFileLocation))
            {
                PostHttpPostParameterError(post, "No mount point save location was specified");
                yield break;
            }

            HttpPostFile file = postData.Files["SrgsFile"];
            if (file == null)
            {
                PostHttpPostParameterError(post, "No SRGS file uploaded");
                yield break;
            }

            // Write file to mount service
            SuccessFailurePort writerResponsePort = new SuccessFailurePort();
            yield return new IterativeTask(delegate
            {
                return WriteFileToMountService(
                    mountPointFileLocation,
                    file.InputStream,
                    writerResponsePort
                );
            });

            Exception writeException = (Exception)writerResponsePort;
            if (writeException != null)
            {
                LogWarning(writeException);
                post.ResponsePort.Post(new HttpResponseType(
                    HttpStatusCode.BadRequest,
                    Fault.FromException(writeException)
                ));
                yield break;
            }

            // Use newly uploaded file
            NameValueCollection parameters = new NameValueCollection();
            parameters["SrgsFileLocation"] = mountPointFileLocation;
            yield return new IterativeTask(delegate
            {
                return UseExistingSrgsFileHttpPostHandler(
                    parameters,
                    post
                );
            });

            // UseExistingSrgsFileHttpPostHandler already took care of posting
            // a result on the response port
            yield break;
        }

        /// <summary>
        /// UseExistingSrgsFile HTTP post handler
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="post"></param>
        /// <returns></returns>
        private IEnumerator<ITask> UseExistingSrgsFileHttpPostHandler(NameValueCollection parameters, HttpPost post)
        {
            string srgsFileLocation = parameters["SrgsFileLocation"];

            if (srgsFileLocation == null || srgsFileLocation.Trim().Length == 0)
            {
                PostHttpPostParameterError(post, "No SRGS file location was specified");
                yield break;
            }

            // Set up replace request with SRGS file location set
            sr.SpeechRecognizerState srState = new sr.SpeechRecognizerState();
            srState.GrammarType = sr.GrammarType.Srgs;
            srState.SrgsFileLocation = srgsFileLocation.Trim();

            // Post replace request to SpeechRecognizer service and check outcome
            sr.Replace replaceRequest = new sr.Replace(srState);
            _srPort.Post(replaceRequest);

            yield return (Choice)replaceRequest.ResponsePort;
            Fault fault = (Fault)replaceRequest.ResponsePort;
            if (fault != null)
            {
                post.ResponsePort.Post(new HttpResponseType(
                    HttpStatusCode.OK,
                    fault
                ));
                yield break;
            }

            _state.SpeechRecognizerState = srState;

            post.ResponsePort.Post(new HttpResponseType(
                HttpStatusCode.OK,
                HttpPostSuccess.Instance
            ));
            yield break;
        }

        /// <summary>
        /// Speech recognizer replace handler
        /// </summary>
        /// <param name="replace"></param>
        private void SpeechRecognizerReplaceHandler(sr.Replace replace)
        {
            _state.SpeechRecognizerState = replace.Body;
        }

        /// <summary>
        /// Speech detected handler
        /// </summary>
        /// <param name="detected"></param>
        private void SpeechDetectedHandler(sr.SpeechDetected detected)
        {
            // Keep speech event queue from growing infinitely
            if (_state.SpeechEvents.Count == MaxSpeechEventsToRetain)
            {
                _state.SpeechEvents.RemoveAt(0);
            }

            // Add latest event to queue
            _state.SpeechEvents.Add(new EventListEntry(detected.Body));
        }

        /// <summary>
        /// Speech recognized handler
        /// </summary>
        /// <param name="recognized"></param>
        private void SpeechRecognizedHandler(sr.SpeechRecognized recognized)
        {
            // Keep speech event queue from growing infinitely
            if (_state.SpeechEvents.Count == MaxSpeechEventsToRetain)
            {
                _state.SpeechEvents.RemoveAt(0);
            }

            // Add latest event to queue
            _state.SpeechEvents.Add(new EventListEntry(recognized.Body));
        }

        /// <summary>
        /// Speech recognition rejected handler
        /// </summary>
        /// <param name="rejected"></param>
        private void SpeechRecognitionRejectedHandler(sr.SpeechRecognitionRejected rejected)
        {
            // Keep speech event queue from growing infinitely
            if (_state.SpeechEvents.Count == MaxSpeechEventsToRetain)
            {
                _state.SpeechEvents.RemoveAt(0);
            }

            // Add latest event to queue
            _state.SpeechEvents.Add(new EventListEntry(rejected.Body));
        }

        /// <summary>
        /// Writes a binary file from a stream to the mount service
        /// </summary>
        /// <param name="filename">Filename and path where the file shall be store on the mount service</param>
        /// <param name="fileStream">Stream containing the file data</param>
        /// <param name="responsePort">File writer response port</param>
        /// <returns></returns>
        private IEnumerator<ITask> WriteFileToMountService(
            string filename,
            Stream fileStream,
            SuccessFailurePort responsePort)
        {
            // Stream needs to support seeking, otherwise we will never know when the end
            // is reached
            if (!fileStream.CanSeek)
            {
                throw new ArgumentException("File stream needs to support seeking");
            }

            // Construct URI to file
            string fileUri = "http://localhost" + ServicePaths.MountPoint;
            if (!filename.StartsWith("/"))
            {
                fileUri += "/";
            }
            fileUri += filename;

            // Establish channel with mount service
            mnt.MountServiceOperations mountPort = ServiceForwarder<mnt.MountServiceOperations>(fileUri);

            // Set up byte update message
            mnt.UpdateBytesRequest updateBytesRequest = new mnt.UpdateBytesRequest();
            mnt.UpdateBytes updateBytes = new mnt.UpdateBytes(updateBytesRequest);
            updateBytesRequest.Offset = 0;
            updateBytesRequest.Truncate = false;

            // Write file in blocks to mount service
            updateBytesRequest.Data = new byte[MountServiceWriteBlockSize];

            fileStream.Position = 0;
            int writeOffset = 0;
            while (fileStream.Position < fileStream.Length)
            {
                // Fill buffer
                int offset = 0;
                while (offset < MountServiceWriteBlockSize && fileStream.Position < fileStream.Length)
                {
                    int bytesRead = fileStream.Read(
                        updateBytesRequest.Data,
                        offset,
                        MountServiceWriteBlockSize - offset
                    );

                    offset += bytesRead;
                }

                if (offset < MountServiceWriteBlockSize)
                {
                    // Last message will most probably not contain a completely filled buffer
                    Array.Resize<byte>(ref updateBytesRequest.Data, offset);
                }

                if (fileStream.Position >= fileStream.Length)
                {
                    // End of stream reached, truncate file on mount service
                    // to current position
                    updateBytesRequest.Truncate = true;
                }

                updateBytesRequest.Offset = writeOffset;

                // Send message to mount service
                mountPort.Post(updateBytes);
                yield return (Choice)updateBytes.ResponsePort;

                Fault fault = (Fault)updateBytes.ResponsePort;
                if (fault != null)
                {
                    Exception exception = fault.ToException();
                    LogWarning(exception);
                    responsePort.Post(exception);
                    yield break;
                }

                // Clear response port
                DefaultUpdateResponseType success = (DefaultUpdateResponseType)updateBytes.ResponsePort;

                writeOffset += updateBytesRequest.Data.Length;
            }

            responsePort.Post(SuccessResult.Instance);
        }

        /// <summary>
        /// Posts a HTTP error message with a supplied error message back to the client
        /// </summary>
        /// <param name="post"></param>
        /// <param name="errorMsg"></param>
        private void PostHttpPostParameterError(HttpPost post, string errorMsg)
        {
            post.ResponsePort.Post(new HttpResponseType(
                HttpStatusCode.OK,
                Fault.FromCodeSubcodeReason(
                    FaultCodes.Sender,
                    DsspFaultCodes.ActionNotSupported,
                    errorMsg
                )
            ));
        }
    }
}
