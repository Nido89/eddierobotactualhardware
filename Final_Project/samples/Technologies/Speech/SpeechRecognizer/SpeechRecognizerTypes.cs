//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: SpeechRecognizerTypes.cs $ $Revision: 11 $
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using sr = System.Speech.Recognition;
using W3C.Soap;

using Microsoft.Ccr.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.ServiceModel.Dssp;


namespace Microsoft.Robotics.Technologies.Speech.SpeechRecognizer
{
    
    /// <summary>
    /// SpeechRecognizer Contract class
    /// </summary>
    public sealed class Contract
    {
        /// <summary>
        /// The Dss service contract
        /// </summary>
        [DataMember]
        public const String Identifier = "http://schemas.microsoft.com/robotics/2008/02/speechrecognizer.user.html";
    }

    /// <summary>
    /// SpeechRecognizer Main Operations Port
    /// </summary>
    [ServicePort]
    public class SpeechRecognizerOperations : PortSet<
        DsspDefaultLookup,
        DsspDefaultDrop,
        Get,
        InsertGrammarEntry,
        UpdateGrammarEntry,
        UpsertGrammarEntry,
        DeleteGrammarEntry,
        SetSrgsGrammarFile,
        EmulateRecognize,
        Replace,
        Subscribe,
        SpeechDetected,
        SpeechRecognized,
        SpeechRecognitionRejected>
    {
    }

    /// <summary>
    /// SpeechRecognizer Get Operation
    /// </summary>
    public class Get : Get<GetRequestType, PortSet<SpeechRecognizerState, Fault>>
    {
        
        /// <summary>
        /// SpeechRecognizer Get Operation
        /// </summary>
        public Get()
        {
        }
        
        /// <summary>
        /// SpeechRecognizer Get Operation
        /// </summary>
        public Get(GetRequestType body) : base(body)
        {
        }
        
        /// <summary>
        /// SpeechRecognizer Get Operation
        /// </summary>
        public Get(GetRequestType body, PortSet<SpeechRecognizerState, Fault> responsePort) : 
                base(body, responsePort)
        {
        }
    }

    /// <summary>
    /// Insert operation which inserts all entries of the supplied grammar dictionary into the current
    /// dictionary. If certain entries exist already a fault is returned and the whole operation fails
    /// without the current dictionary being modified at all.
    /// </summary>
    public class InsertGrammarEntry : Insert<InsertGrammarRequest, PortSet<DefaultInsertResponseType, Fault>>
    {
    }

    /// <summary>
    /// Update operation which updates entries that already exist in the current grammar dictionary with
    /// the entries of the supplied dictionary. If certain entries in the supplied dictionary do not
    /// exist in the current dictionary no fault is returned. Instead, only the existing entries are
    /// updated.
    /// </summary>
    public class UpdateGrammarEntry : Update<UpdateGrammarRequest, PortSet<DefaultUpdateResponseType, Fault>>
    {
    }

    /// <summary>
    /// Update or insert operation which inserts entries from the supplied grammar directory into the
    /// current dictionary if they do not exist yet or updates entries that already exist with entries
    /// from the supplied directory.
    /// </summary>
    public class UpsertGrammarEntry : Upsert<UpsertGrammarRequest, PortSet<DefaultUpsertResponseType, Fault>>
    {
    }

    /// <summary>
    /// Delete operation which deletes those entries from the current grammar directory whose keys are
    /// equal to one of the entries in the supplied directory. If a key from the supplied directory
    /// does not exist in the current directory no fault is returend. Instead, only the matching entries
    /// are deleted.
    /// </summary>
    public class DeleteGrammarEntry : Delete<DeleteGrammarRequest, PortSet<DefaultDeleteResponseType, Fault>>
    {
    }

    /// <summary>
    /// Set SRGS grammar file operation which sets the grammar type to SRGS file and tries to load the
    /// specified file, which has to reside inside the DSS node's /store directory. If loading the file
    /// fails a fault is returned and the speech recognizer falls back into the state it was before it
    /// processed this message.
    /// </summary>
    /// <remarks>
    /// SRGS grammars require Microsoft Windows Vista and will not work with Microsoft Windows XP/Server 2003.
    /// </remarks>
    public class SetSrgsGrammarFile : Update<SetSrgsGrammarFileRequest, PortSet<DefaultUpdateResponseType, Fault>>
    {
    }

    /// <summary>
    /// Speech recognize emulation operation that orders the recognition engine to emulate speech with a
    /// specific content and to act upon this speech. This is mostly used for testing and debugging.
    /// </summary>
    public class EmulateRecognize : Update<EmulateRecognizeRequest, PortSet<DefaultUpdateResponseType, Fault>>
    {
    }

    /// <summary>
    /// Replace service state operation
    /// </summary>
    public class Replace : Replace<SpeechRecognizerState, PortSet<DefaultReplaceResponseType, Fault>>
    {
    }

    /// <summary>
    /// Subscribe operation
    /// </summary>
    public class Subscribe : Subscribe<SubscribeRequestType, PortSet<SubscribeResponseType, Fault>>
    {
    }

    /// <summary>
    /// Speech detected notificaiton
    /// </summary>
    public class SpeechDetected : Update<SpeechDetectedNotification, PortSet<DefaultUpdateResponseType, Fault>>
    {
    }

    /// <summary>
    /// Speech recognized notification
    /// </summary>
    public class SpeechRecognized : Update<SpeechRecognizedNotification, PortSet<DefaultUpdateResponseType, Fault>>
    {
    }

    /// <summary>
    /// Speech recognition rejected notification
    /// </summary>
    public class SpeechRecognitionRejected : Update<SpeechRecognitionRejectedNotification, PortSet<DefaultUpdateResponseType, Fault>>
    {
    }

    /// <summary>
    /// Replace grammar request
    /// </summary>
    public class LoadGrammarRequest : ModifyGrammarDictionaryRequest
    {
        private GrammarType _grammarType;
        /// <summary>
        /// Specifies the type of grammar that shall be set
        /// </summary>
        [Description("Specifies the type of grammar that shall be set.")]
        public GrammarType GrammarType
        {
            get { return _grammarType; }
            set { _grammarType = value; }
        }

        private string _srgsFileLocation;
        /// <summary>
        /// Location of SRGS (Speech Recognition Grammar Specification) file that shall
        /// be used as a grammar
        /// </summary>
        [Description("Location of SRGS (Speech Recognition Grammar Specification) file that shall be used as a grammar.")]
        public string SrgsFileLocation
        {
            get { return _srgsFileLocation; }
            set { _srgsFileLocation = value; }
        }
    }
}
