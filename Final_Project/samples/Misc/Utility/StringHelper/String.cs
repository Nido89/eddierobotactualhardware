//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: String.cs $ $Revision: 11 $
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Dss.ServiceModel.DsspServiceBase;
using Microsoft.Dss.Core.Attributes;
using System.ComponentModel;
using System.Security;
using System.Security.Permissions;
using Microsoft.Dss.ServiceModel.Dssp;
using Microsoft.Ccr.Core;
using W3C.Soap;

namespace Microsoft.Robotics.Services.Sample.StringHelper
{
    /// <summary>
    /// StringService - Handles Text (String) functions
    /// </summary>
    [DisplayName("(User) Text Functions")]
    [Description("Provides functions that operate on text strings.")]
    [Contract(Contract.Identifier)]
    [DssServiceDescription("http://msdn.microsoft.com/library/dd126874.aspx")]
    public class StringService : DsspServiceBase
    {
        [ServicePort("string", AllowMultipleInstances = false)]
        StringOperations _mainPort = new StringOperations();

        /// <summary>
        /// Constructor for service
        /// </summary>
        /// <param name="creationPort"></param>
        public StringService(DsspServiceCreationPort creationPort) :
                base(creationPort)
        {
        }

        /// <summary>
        /// Service Start
        /// </summary>
        protected override void Start()
        {
            base.Start();
        }

        /// <summary>
        /// IsNullOrEmptyHandler - Checks if a string is empty
        /// </summary>
        /// <param name="isNullOrEmpty"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public IEnumerator<ITask> IsNullOrEmptyHandler(IsNullOrEmpty isNullOrEmpty)
        {
            isNullOrEmpty.ResponsePort.Post(
                IsNullOrEmptyResponse.FromRequest(isNullOrEmpty.Body)
            );
            yield break;
        }

        /// <summary>
        /// ContainsHandler - Checks if specified substring is in a string
        /// </summary>
        /// <param name="contains"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public IEnumerator<ITask> ContainsHandler(Contains contains)
        {
            contains.ResponsePort.Post(
                ContainsResponse.FromRequest(contains.Body)
            );
            yield break;
        }

        /// <summary>
        /// EndsWithHandler - Checks if string ends with specified substring
        /// </summary>
        /// <param name="endsWith"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public IEnumerator<ITask> EndsWithHandler(EndsWith endsWith)
        {
            endsWith.ResponsePort.Post(
                EndsWithResponse.FromRequest(endsWith.Body)
            );
            yield break;
        }

        /// <summary>
        /// IndexOfHandler - Finds location of a specified substring in a string
        /// </summary>
        /// <param name="indexOf"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public IEnumerator<ITask> IndexOfHandler(IndexOf indexOf)
        {
            indexOf.ResponsePort.Post(
                IndexOfResponse.FromRequest(indexOf.Body)
            );
            yield break;
        }

        /// <summary>
        /// InsertHandler - Inserts one string into another one
        /// </summary>
        /// <param name="insert"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public IEnumerator<ITask> InsertHandler(Insert insert)
        {
            insert.ResponsePort.Post(
                InsertResponse.FromRequest(insert.Body)
            );
            yield break;
        }

        /// <summary>
        /// JoinHandler - Concatenates an array of strings with a separator (Opposite of Split)
        /// </summary>
        /// <param name="join"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public IEnumerator<ITask> JoinHandler(Join join)
        {
            join.ResponsePort.Post(
                JoinResponse.FromRequest(join.Body)
            );
            yield break;
        }

        /// <summary>
        /// LastIndexOfHandler - Locates specified substring backwards from the end of string
        /// </summary>
        /// <param name="lastIndexOf"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public IEnumerator<ITask> LastIndexOfHandler(LastIndexOf lastIndexOf)
        {
            lastIndexOf.ResponsePort.Post(
                LastIndexOfResponse.FromRequest(lastIndexOf.Body)
            );
            yield break;
        }

        /// <summary>
        /// PadHandler - Pads out a string
        /// </summary>
        /// <param name="pad"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public IEnumerator<ITask> PadHandler(Pad pad)
        {
            pad.ResponsePort.Post(
                PadResponse.FromRequest(pad.Body)
            );
            yield break;
        }

        /// <summary>
        /// RemoveHandler - Removes specified substring from a string
        /// </summary>
        /// <param name="remove"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public IEnumerator<ITask> RemoveHandler(Remove remove)
        {
            remove.ResponsePort.Post(
                RemoveResponse.FromRequest(remove.Body)
            );
            yield break;
        }

        /// <summary>
        /// ReplaceHandler - Replaces specified substring in a string
        /// </summary>
        /// <param name="replace"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public IEnumerator<ITask> ReplaceHandler(Replace replace)
        {
            replace.ResponsePort.Post(
                ReplaceResponse.FromRequest(replace.Body)
            );
            yield break;
        }

        /// <summary>
        /// SplitHandler - Splits a string into many strings based on a token
        /// </summary>
        /// <param name="split"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public IEnumerator<ITask> SplitHandler(Split split)
        {
            split.ResponsePort.Post(
                SplitResponse.FromRequest(split.Body)
            );
            yield break;
        }

        /// <summary>
        /// StartsWithHandler - Checks if a string starts with specified substring
        /// </summary>
        /// <param name="startsWith"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public IEnumerator<ITask> StartsWithHandler(StartsWith startsWith)
        {
            startsWith.ResponsePort.Post(
                StartsWithResponse.FromRequest(startsWith.Body)
            );
            yield break;
        }

        /// <summary>
        /// SubStringHandler - Extracts a substring from a string
        /// </summary>
        /// <param name="subString"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public IEnumerator<ITask> SubStringHandler(SubString subString)
        {
            subString.ResponsePort.Post(
                SubStringResponse.FromRequest(subString.Body)
            );
            yield break;
        }

        /// <summary>
        /// ToLowerHandler - Converts a string to Lowercase
        /// </summary>
        /// <param name="toLower"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public IEnumerator<ITask> ToLowerHandler(ToLower toLower)
        {
            toLower.ResponsePort.Post(
                ToLowerResponse.FromRequest(toLower.Body)
            );
            yield break;
        }

        /// <summary>
        /// ToUpperHandler - Converts a string to Uppercase
        /// </summary>
        /// <param name="toUpper"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public IEnumerator<ITask> ToUpperHandler(ToUpper toUpper)
        {
            toUpper.ResponsePort.Post(
                ToUpperResponse.FromRequest(toUpper.Body)
            );
            yield break;
        }

        /// <summary>
        /// TrimHandler - Trims spaces off a string
        /// </summary>
        /// <param name="trim"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public IEnumerator<ITask> TrimHandler(Trim trim)
        {
            trim.ResponsePort.Post(
                TrimResponse.FromRequest(trim.Body)
            );
            yield break;
        }
    }
}
