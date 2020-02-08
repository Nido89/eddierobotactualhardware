//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: StringTypes.cs $ $Revision: 12 $
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using W3C.Soap;

using Microsoft.Ccr.Core;
using Microsoft.Dss.ServiceModel.Dssp;
using Microsoft.Dss.Core.Attributes;

namespace Microsoft.Robotics.Services.Sample.StringHelper
{
    /// <summary>
    /// String Service contract
    /// </summary>
    public static class Contract
    {
        /// The Unique Contract Identifier for the String service
        public const string Identifier = "http://schemas.microsoft.com/robotics/2006/10/string.user.html";
    }

    /// <summary>
    /// String Service Operations PortSet
    /// </summary>
    [ServicePort]
    public class StringOperations : PortSet
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public StringOperations()
            : base(
                typeof(DsspDefaultLookup),
                typeof(IsNullOrEmpty),
                typeof(Contains),
                typeof(EndsWith),
                typeof(IndexOf),
                typeof(Insert),
                typeof(Join),
                typeof(LastIndexOf),
                typeof(Pad),
                typeof(Remove),
                typeof(Replace),
                typeof(Split),
                typeof(StartsWith),
                typeof(SubString),
                typeof(ToLower),
                typeof(ToUpper),
                typeof(Trim)
            )
        {
        }
    }

    #region IsNullOrEmpty
    /// <summary>
    /// IsNullOrEmptyRequest
    /// </summary>
    [DataContract]
    public class IsNullOrEmptyRequest
    {
        private string _string;
        /// <summary>
        /// String - The string to check
        /// </summary>
        [DataMember]
        [Description("The text string to be checked.")]
        public string String
        {
            get { return _string; }
            set { _string = value; }
        }
    }

    /// <summary>
    /// IsNullOrEmptyResponse
    /// </summary>
    [DataContract]
    public class IsNullOrEmptyResponse
    {
        private bool _isNullOrEmpty;
        /// <summary>
        /// IsNullOrEmpty - True if the string is empty or null (does not exist)
        /// </summary>
        [Description("Indicates if the string is null or empty.")]
        [DataMember]
        public bool IsNullOrEmpty
        {
            get { return _isNullOrEmpty; }
            set { _isNullOrEmpty = value; }
        }

        /// <summary>
        /// FromRequest - Performs the Is Null Or Empty function
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static IsNullOrEmptyResponse FromRequest(IsNullOrEmptyRequest request)
        {
            IsNullOrEmptyResponse response = new IsNullOrEmptyResponse();

            response._isNullOrEmpty = string.IsNullOrEmpty(request.String);

            return response;
        }
    }

    /// <summary>
    /// IsNullOrEmpty - Operation to see if a string is empty (null means there is no string)
    /// </summary>
    [DisplayName("(User) IsNullorEmpty")]
    [Description("Indicates whether the specified string is a null reference.\nReturns true if the text string is null or empty, else returns false.")]
    public class IsNullOrEmpty : Submit<IsNullOrEmptyRequest, PortSet<IsNullOrEmptyResponse, Fault>>
    {
    }

    #endregion

    #region Contains

    /// <summary>
    /// ContainsRequest
    /// </summary>
    [DataContract]
    public class ContainsRequest
    {
        private string _string;
        /// <summary>
        /// String - The string to search in
        /// </summary>
        [DataMember]
        [Description("A text string to be searched.")]
        public string String
        {
            get { return _string; }
            set { _string = value; }
        }

        private string _query;
        /// <summary>
        /// Query - The string to look for
        /// </summary>
        [DataMember]
        [Description("The text string to be searched for.")]
        public string Query
        {
            get { return _query; }
            set { _query = value; }
        }
    }

    /// <summary>
    /// ContainsResponse
    /// </summary>
    [DataContract]
    public class ContainsResponse
    {
        private bool _contains;
        /// <summary>
        /// Contains - True if string contains the query string
        /// </summary>
        [DataMember]
        public bool Contains
        {
            get { return _contains; }
            set { _contains = value; }
        }

        /// <summary>
        /// FromRequest - Performs the Contains function
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static ContainsResponse FromRequest(ContainsRequest request)
        {
            ContainsResponse response = new ContainsResponse();
#if URT_MINCLR
            response._contains = request.String.IndexOf(request.Query) >= 0;
#else
            response._contains = request.String.Contains(request.Query);
#endif

            return response;
        }
    }

    /// <summary>
    /// Contains - Operation
    /// </summary>
    [DisplayName("(User) Contains")]
    [Description("Returns a value indicating whether the specified string occurs within this string.\nReturns true if the searched for text is included in the string text, else false.")]
    public class Contains : Submit<ContainsRequest, PortSet<ContainsResponse, Fault>>
    {
    }
    #endregion

    #region EndsWith

    /// <summary>
    /// EndsWithRequest
    /// </summary>
    [DataContract]
    public class EndsWithRequest
    {
        private string _string;
        /// <summary>
        /// String - The string to search in
        /// </summary>
        [DataMember]
        [Description("The text string to be searched.")]
        public string String
        {
            get { return _string; }
            set { _string = value; }
        }

        private string _query;
        /// <summary>
        /// Query - The string to search for
        /// </summary>
        [DataMember]
        [Description("The query text string to be searched.")]
        public string Query
        {
            get { return _query; }
            set { _query = value; }
        }

        private bool _ignoreCase;
        /// <summary>
        /// IgnoreCase - Ignore case during search if true
        /// </summary>
        [DataMember]
        [Description("The option whether to ignore the case.\ntrue sets the option.")]
        public bool IgnoreCase
        {
            get { return _ignoreCase; }
            set { _ignoreCase = value; }
        }
    }

    /// <summary>
    /// EndsWithResponse
    /// </summary>
    [DataContract]
    public class EndsWithResponse
    {
        private bool _endsWith;
        /// <summary>
        /// EndsWith - String was found if true
        /// </summary>
        [DataMember]
        [Description("Identifies if the string was found.")]
        public bool EndsWith
        {
            get { return _endsWith; }
            set { _endsWith = value; }
        }

        /// <summary>
        /// FromRequest - Performs the Ends With function
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static EndsWithResponse FromRequest(EndsWithRequest request)
        {
            EndsWithResponse response = new EndsWithResponse();

#if URT_MINCLR
            response._endsWith = request.String.EndsWith(
                request.Query,
                request.IgnoreCase ?
                    StringComparison.CurrentCultureIgnoreCase :
                    StringComparison.CurrentCulture
            );
#else
            response._endsWith = request.String.EndsWith(
                request.Query,
                request.IgnoreCase,
                CultureInfo.CurrentUICulture
            );
#endif

            return response;
        }
    }

    /// <summary>
    /// EndsWith - Operation
    /// </summary>
    [DisplayName("(User) EndsWith")]
    [Description("Determines whether the end of a string matches a specified string.\nReturns true if the string ends with the specified query text string,\n using the IgnoreCase comparison option.")]
    public class EndsWith : Submit<EndsWithRequest, PortSet<EndsWithResponse, Fault>>
    {
    }

    #endregion

    #region IndexOf

    /// <summary>
    /// IndexOfRequest
    /// </summary>
    [DataContract]
    public class IndexOfRequest
    {
        private string _string;
        /// <summary>
        /// String - The string to search in
        /// </summary>
        [DataMember]
        [Description("The text string to search.")]
        public string String
        {
            get { return _string; }
            set { _string = value; }
        }

        private string _query;
        /// <summary>
        /// Query - The string to search for
        /// </summary>
        [DataMember]
        [Description("The query text string to search for.")]
        public string Query
        {
            get { return _query; }
            set { _query = value; }
        }

        private int _startIndex;
        /// <summary>
        /// StartIndex - Starting position for the search
        /// </summary>
        [DataMember]
        [Description("The starting index for the search.")]
        public int StartIndex
        {
            get { return _startIndex; }
            set { _startIndex = value; }
        }
    }

    /// <summary>
    /// IndexOfResponse
    /// </summary>
    [DataContract]
    public class IndexOfResponse
    {
        private int _indexOf;
        /// <summary>
        /// IndexOf - The index where the string was found, otherwise -1
        /// </summary>
        [DataMember]
        [Description("Identifies the index of where the string was found.")]
        public int IndexOf
        {
            get { return _indexOf; }
            set { _indexOf = value; }
        }

        /// <summary>
        /// FromRequest - Performs the Index Of function
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static IndexOfResponse FromRequest(IndexOfRequest request)
        {
            IndexOfResponse response = new IndexOfResponse();

            response._indexOf = request.String.IndexOf(request.Query, request.StartIndex);

            return response;
        }
    }

    /// <summary>
    /// IndexOf - Operation
    /// </summary>
    [DisplayName("(User) IndexOf")]
    [Description("Reports the index of the first occurrence of a string, or one or more characters, within this string.\nReturns the position of the first occurence of the Query text in the String text.\nThe search starts at the StartIndex character position.")]
    public class IndexOf : Submit<IndexOfRequest, PortSet<IndexOfResponse, Fault>>
    {
    }
    #endregion

    #region Insert

    /// <summary>
    /// InsertRequest
    /// </summary>
    [DataContract]
    public class InsertRequest
    {
        private string _string;
        /// <summary>
        /// String - The string to insert into
        /// </summary>
        [DataMember]
        [Description("A text string to receive the insertion.")]
        public string String
        {
            get { return _string; }
            set { _string = value; }
        }

        private string _insertion;
        /// <summary>
        /// Insertion - The text to be inserted
        /// </summary>
        [DataMember]
        [Description("The text string to insert.")]
        public string Insertion
        {
            get { return _insertion; }
            set { _insertion = value; }
        }

        private int _startIndex;
        /// <summary>
        /// StartIndex - The position to insert at
        /// </summary>
        [DataMember]
        [Description("The character position for the insertion.")]
        public int StartIndex
        {
            get { return _startIndex; }
            set { _startIndex = value; }
        }
    }

    /// <summary>
    /// InsertResponse
    /// </summary>
    [DataContract]
    public class InsertResponse
    {
        private string _inserted;
        /// <summary>
        /// Inserted - The output string (with inserted text)
        /// </summary>
        [DataMember]
        [Description("The new string with the inserted text.")]
        public string Inserted
        {
            get { return _inserted; }
            set { _inserted = value; }
        }

        /// <summary>
        /// FromRequest - Performs the Insert function
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static InsertResponse FromRequest(InsertRequest request)
        {
            InsertResponse response = new InsertResponse();

            response._inserted = request.String.Insert(
                request.StartIndex,
                request.Insertion
            );

            return response;
        }
    }

    /// <summary>
    /// Insert - Operation
    /// </summary>
    [DisplayName("(User) Insert")]
    [Description("Inserts a specified text string at a specified index position of another string.\nThe text is inserted at the StartIndex.")]
    public class Insert : Submit<InsertRequest, PortSet<InsertResponse, Fault>>
    {
    }

    #endregion

    #region Join

    /// <summary>
    /// JoinRequest
    /// </summary>
    [DataContract]
    public class JoinRequest
    {
        private string _separator;
        /// <summary>
        /// Separator - The text to insert between the strings
        /// </summary>
        [DataMember]
        [Description("The text string to be inserted.")]
        public string Separator
        {
            get { return _separator; }
            set { _separator = value; }
        }

        private string[] _strings = new string[0];
        /// <summary>
        /// Strings - The array of strings to join
        /// </summary>
        [DataMember(IsRequired = true)]
        [Description("The string array to concatenate.")]
        public string[] Strings
        {
            get { return _strings; }
            set { _strings = value; }
        }
    }

    /// <summary>
    /// JoinResponse
    /// </summary>
    [DataContract]
    public class JoinResponse
    {
        private string _joined;
        /// <summary>
        /// Joined - The output (joined) string
        /// </summary>
        [DataMember]
        [Description("The resulting concatenated string.")]
        public string Joined
        {
            get { return _joined; }
            set { _joined = value; }
        }

        /// <summary>
        /// FromRequest - Performs the Join function
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static JoinResponse FromRequest(JoinRequest request)
        {
            JoinResponse response = new JoinResponse();

            response._joined = string.Join(request.Separator, request.Strings);

            return response;
        }
    }

    /// <summary>
    /// Join - Operation to combine array of strings with separator in between
    /// </summary>
    [DisplayName("(User) Join")]
    [Description("Concatenates the Separator text between each element of a specified string array.\nResults in a single concatenated string.\nOpposite of Split.")]
    public class Join : Submit<JoinRequest, PortSet<JoinResponse, Fault>>
    {
    }

    #endregion

    #region LastIndexOf

    /// <summary>
    /// LastIndexOfRequest
    /// </summary>
    [DataContract]
    public class LastIndexOfRequest
    {
        private string _string;
        /// <summary>
        /// String - The string to search in
        /// </summary>
        [DataMember]
        [Description("A text string to be searched.")]
        public string String
        {
            get { return _string; }
            set { _string = value; }
        }

        private string _query;
        /// <summary>
        /// Query - The string to look for
        /// </summary>
        [DataMember]
        [Description("A text string seach for.")]
        public string Query
        {
            get { return _query; }
            set { _query = value; }
        }

        private int _startIndex;
        /// <summary>
        /// StartIndex - Starting position for the search
        /// </summary>
        [DataMember]
        [Description("The character position to start the search.")]
        public int StartIndex
        {
            get { return _startIndex; }
            set { _startIndex = value; }
        }
    }

    /// <summary>
    /// LastIndexOfResponse
    /// </summary>
    [DataContract]
    public class LastIndexOfResponse
    {
        private int _lastIndexOf;
        /// <summary>
        /// LastIndexOf - Starting position of the string if found, otherwise -1
        /// </summary>
        [DataMember]
        [Description("The index character position if the string is found, or -1 if it is not.")]
        public int LastIndexOf
        {
            get { return _lastIndexOf; }
            set { _lastIndexOf = value; }
        }

        /// <summary>
        /// FromRequest - Performs the Last Index Of function
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static LastIndexOfResponse FromRequest(LastIndexOfRequest request)
        {
            LastIndexOfResponse response = new LastIndexOfResponse();

            if (request.StartIndex > 0)
            {
                response._lastIndexOf = request.String.LastIndexOf(request.Query, request.StartIndex);
            }
            else
            {
                response._lastIndexOf = request.String.LastIndexOf(request.Query);
            }

            return response;
        }
    }

    /// <summary>
    /// LastIndexOf - Operation to find a query string searching backwards
    /// </summary>
    [DisplayName("(User) LastIndexOf")]
    [Description("Returns the character position of the last occurrence of the specified Query text within another string.\nThe search starts at the StartIndex character position.")]
    public class LastIndexOf : Submit<LastIndexOfRequest, PortSet<LastIndexOfResponse, Fault>>
    {
    }

    #endregion

    #region Pad

    /// <summary>
    /// PadRequest
    /// </summary>
    [DataContract]
    public class PadRequest
    {
        private string _string;
        /// <summary>
        /// String - The string to be padded
        /// </summary>
        [DataMember]
        [Description("Specifies the text string to be aligned by inserting padding characters.")]
        public string String
        {
            get { return _string; }
            set { _string = value; }
        }

        private int _leftPadding;
        /// <summary>
        /// LeftPadding - Number of padding characters on the left (can be zero)
        /// </summary>
        [DataMember]
        [Description("Specifies the number of padding characters to add to the left.")]
        public int LeftPadding
        {
            get { return _leftPadding; }
            set { _leftPadding = value; }
        }

        private int _rightPadding;
        /// <summary>
        /// RightPadding - Number of padding character on the right (can be zero)
        /// </summary>
        [DataMember]
        [Description("Specifies the number of padding characters to add to the right.")]
        public int RightPadding
        {
            get { return _rightPadding; }
            set { _rightPadding = value; }
        }

        private char _paddingChar;
        /// <summary>
        /// PaddingChar - Character to pad with (defaults to space)
        /// </summary>
        [DataMember]
        [Description("Specifies the padding characters to add.")]
        public char PaddingChar
        {
            get { return _paddingChar; }
            set { _paddingChar = value; }
        }
    }

    /// <summary>
    /// PadResponse
    /// </summary>
    [DataContract]
    public class PadResponse
    {
        private string _padded;
        /// <summary>
        /// Padded - The output string (with padding)
        /// </summary>
        [DataMember]
        [Description("Identifies the resulting padded text string.")]
        public string Padded
        {
            get { return _padded; }
            set { _padded = value; }
        }

        /// <summary>
        /// FromRequest - Perform the Pad function
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static PadResponse FromRequest(PadRequest request)
        {
            PadResponse response = new PadResponse();

            string padded;
            char padChar = request.PaddingChar;

            if (padChar == 0)
            {
                padChar = ' ';
            }

            if (request.LeftPadding > 0)
            {
                padded = request.String.PadLeft(
                    request.String.Length + request.LeftPadding,
                    padChar
                );
            }
            else
            {
                padded = request.String;
            }

            if (request.RightPadding > 0)
            {
                padded = padded.PadRight(
                    padded.Length + request.RightPadding,
                    padChar
                );
            }

            response._padded = padded;

            return response;
        }
    }

    /// <summary>
    /// Pad - Operation to pad a string (on left and/or right) with a character
    /// </summary>
    [DisplayName("(User) Pad")]
    [Description("Aligns the String text, using the PaddingChar text,\nusing PaddingLeft for the amount on left side and PaddingRight for the right side.")]
    public class Pad : Submit<PadRequest, PortSet<PadResponse, Fault>>
    {
    }

    #endregion

    #region Remove

    /// <summary>
    /// RemoveRequest
    /// </summary>
    [DataContract]
    public class RemoveRequest
    {
        private string _string;
        /// <summary>
        /// String - The string to be changed
        /// </summary>
        [DataMember]
        [Description("The text string the characters will be removed from.")]
        public string String
        {
            get { return _string; }
            set { _string = value; }
        }

        private int _startIndex;
        /// <summary>
        /// StartIndex - Starting position in the string
        /// </summary>
        [DataMember]
        [Description("The position to begin deleting characters.")]
        public int StartIndex
        {
            get { return _startIndex; }
            set { _startIndex = value; }
        }

        private int _count;
        /// <summary>
        /// Count - Number of characters to delete
        /// </summary>
        [DataMember]
        [Description("The number of characters to delete.")]
        public int Count
        {
            get { return _count; }
            set { _count = value; }
        }
    }

    /// <summary>
    /// RemoveResponse
    /// </summary>
    [DataContract]
    public class RemoveResponse
    {
        private string _removed;
        /// <summary>
        /// Removed - The output string (with text removed)
        /// </summary>
        [DataMember]
        [Description("The resulting string after the characters have been removed.")]
        public string Removed
        {
            get { return _removed; }
            set { _removed = value; }
        }

        /// <summary>
        /// FromRequest - Performs the Remove function
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static RemoveResponse FromRequest(RemoveRequest request)
        {
            RemoveResponse response = new RemoveResponse();

            if (request.Count > 0)
            {
                response._removed = request.String.Remove(
                    request.StartIndex,
                    request.Count
                );
            }
            else
            {
#if URT_MINCLR
                response._removed = request.String.Remove(
                    request.StartIndex,
                    request.String.Length - request.StartIndex
                );
#else
                response._removed = request.String.Remove(
                    request.StartIndex
                );
#endif
            }

            return response;
        }
    }

    /// <summary>
    /// Remove - Operation to delete a portion of a string
    /// </summary>
    [DisplayName("(User) Remove")]
    [Description("Deletes a specified number of characters from the text string, beginning at the StartIndex position.")]
    public class Remove : Submit<RemoveRequest, PortSet<RemoveResponse, Fault>>
    {
    }

    #endregion

    #region Replace

    /// <summary>
    /// ReplaceRequest
    /// </summary>
    [DataContract]
    public class ReplaceRequest
    {
        private string _string;
        /// <summary>
        /// String - The string to be changed
        /// </summary>
        [DataMember]
        [Description("The text string to modify.")]
        public string String
        {
            get { return _string; }
            set { _string = value; }
        }

        private string _oldValue;
        /// <summary>
        /// OldValue - The search string (to be replaced)
        /// </summary>
        [DataMember]
        [Description("The text in the string to replace.")]
        public string OldValue
        {
            get { return _oldValue; }
            set { _oldValue = value; }
        }

        private string _newValue;
        /// <summary>
        /// NewValue - The replacement string
        /// </summary>
        [DataMember]
        [Description("The text to replace all occurrences of OldValue.")]
        public string NewValue
        {
            get { return _newValue; }
            set { _newValue = value; }
        }
    }

    /// <summary>
    /// ReplaceResponse
    /// </summary>
    [DataContract]
    public class ReplaceResponse
    {
        private string _replaced;
        /// <summary>
        /// Replaced - The output string (after replacement)
        /// </summary>
        [DataMember]
        [Description("The resulting text string after replacement(s).")]
        public string Replaced
        {
            get { return _replaced; }
            set { _replaced = value; }
        }

        /// <summary>
        /// FromRequest - Performs the Replace function
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        static public ReplaceResponse FromRequest(ReplaceRequest request)
        {
            ReplaceResponse response = new ReplaceResponse();

            response._replaced = request.String.Replace(
                request.OldValue,
                request.NewValue
            );

            return response;
        }
    }

    /// <summary>
    /// Replace - Operation to replace all occurrences of a string
    /// </summary>
    [Description("Replaces all occurrences of specified text in the text string, with the NewValue text.")]
    public class Replace : Submit<ReplaceRequest, PortSet<ReplaceResponse, Fault>>
    {
    }

    #endregion

    #region Split

    /// <summary>
    /// SplitRequest
    /// </summary>
    [DataContract]
    public class SplitRequest
    {
        private string _string;
        /// <summary>
        /// String - The string to split up
        /// </summary>
        [DataMember]
        [Description("The text string to split into substrings.")]
        public string String
        {
            get { return _string; }
            set { _string = value; }
        }

        private string _separator;
        /// <summary>
        /// Separator - The token that appears between substrings
        /// </summary>
        [DataMember]
        [Description("The text to separate the strings.")]
        public string Separator
        {
            get { return _separator; }
            set { _separator = value; }
        }

        private int _count;
        /// <summary>
        /// Count - The number of substrings to extract
        /// </summary>
        [DataMember]
        [Description("The number of substrings to return.")]
        public int Count
        {
            get { return _count; }
            set { _count = value; }
        }
    }

    /// <summary>
    /// SplitResponse
    /// </summary>
    [DataContract]
    public class SplitResponse
    {
        private string[] _strings;
        /// <summary>
        /// Strings - Output array of strings
        /// </summary>
        [DataMember]
        [Description("The resulting text string array whose elements contain the substrings.")]
        public string[] Strings
        {
            get { return _strings; }
            set { _strings = value; }
        }

        /// <summary>
        /// FromRequest - Performs the Split function
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static SplitResponse FromRequest(SplitRequest request)
        {
            SplitResponse response = new SplitResponse();

#if URT_MINCLR
            response._strings = request.String.Split(request.Separator[0]);
#else
            if (request.Count > 0)
            {
                response._strings = request.String.Split(
                    new string[] { request.Separator },
                    request.Count,
                    StringSplitOptions.None
                );
            }
            else
            {
                response._strings = request.String.Split(
                    new string[] { request.Separator },
                    StringSplitOptions.None
                );
            }
#endif
            return response;
        }
    }

    /// <summary>
    /// Split - Operation to split a string into an array based on a separator
    /// </summary>
    [DisplayName("(User) Split")]
    [Description("Returns a Strings text array from the elements of the String text, using the Separator text to delimit the elements.")]
    public class Split : Submit<SplitRequest, PortSet<SplitResponse, Fault>>
    {
    }

    #endregion

    #region StartsWith

    /// <summary>
    /// StartsWithRequest
    /// </summary>
    [DataContract]
    public class StartsWithRequest
    {
        private string _string;
        /// <summary>
        /// String - The string to search
        /// </summary>
        [DataMember]
        [Description("The text string to search.")]
        public string String
        {
            get { return _string; }
            set { _string = value; }
        }

        private string _query;
        /// <summary>
        /// Query - The string to look for
        /// </summary>
        [DataMember]
        [Description("The text string to search for.")]
        public string Query
        {
            get { return _query; }
            set { _query = value; }
        }
    }

    /// <summary>
    /// StartsWithResponse
    /// </summary>
    [DataContract]
    public class StartsWithResponse
    {
        private bool _startsWith;
        /// <summary>
        /// StartsWith - Query string was found if true
        /// </summary>
        [DataMember]
        [Description("Indicates if the Query text was found.")]
        public bool StartsWith
        {
            get { return _startsWith; }
            set { _startsWith = value; }
        }

        /// <summary>
        /// FromRequest - Performs the Starts With function
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static StartsWithResponse FromRequest(StartsWithRequest request)
        {
            StartsWithResponse response = new StartsWithResponse();

            response._startsWith = request.String.StartsWith(request.Query);

            return response;
        }
    }

    /// <summary>
    /// StartsWith - Operation
    /// </summary>
    [DisplayName("(User) StartsWith")]
    [Description("Returns true if the beginning of text matches the specified Query text, else returns false.")]
    public class StartsWith : Submit<StartsWithRequest, PortSet<StartsWithResponse, Fault>>
    {
    }

    #endregion

    #region SubString

    /// <summary>
    /// SubStringRequest
    /// </summary>
    [DataContract]
    public class SubStringRequest
    {
        private string _string;
        /// <summary>
        /// String - The string to extract from
        /// </summary>
        [DataMember]
        [Description("The text string to extract the substring text from.")]
        public string String
        {
            get { return _string; }
            set { _string = value; }
        }

        private int _startIndex;
        /// <summary>
        /// StartIndex - Starting position in the string (zero based)
        /// </summary>
        [DataMember]
        [Description("The character position to start the substring.")]
        public int StartIndex
        {
            get { return _startIndex; }
            set { _startIndex = value; }
        }

        private int _length;
        /// <summary>
        /// Length - The length of the substring to extract
        /// </summary>
        [DataMember]
        [Description("The number of characters (after the StartIndex) to include.")]
        public int Length
        {
            get { return _length; }
            set { _length = value; }
        }
    }

    /// <summary>
    /// SubStringResponse
    /// </summary>
    [DataContract]
    public class SubStringResponse
    {
        private string _subString;
        /// <summary>
        /// SubString - The output substring
        /// </summary>
        [DataMember]
        [Description("The resulting substring text.")]
        public string SubString
        {
            get { return _subString; }
            set { _subString = value; }
        }

        /// <summary>
        /// FromRequest - Perform the substring function
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static SubStringResponse FromRequest(SubStringRequest request)
        {
            SubStringResponse response = new SubStringResponse();

            if (request.Length > 0)
            {
                response._subString = request.String.Substring(
                    request.StartIndex,
                    request.Length
                );
            }
            else
            {
                response._subString = request.String.Substring(
                    request.StartIndex
                );
            }
            return response;
        }
    }

    /// <summary>
    /// SubString - Operation
    /// </summary>
    [DisplayName("(User) Substring")]
    [Description("Returns the text substring from String text.\nThe substring starts the StartIndex character position\nand uses Length as the length.")]
    public class SubString : Submit<SubStringRequest, PortSet<SubStringResponse, Fault>>
    {
    }

    #endregion

    #region ToLower

    /// <summary>
    /// ToLowerRequest
    /// </summary>
    [DataContract]
    public class ToLowerRequest
    {
        private string _string;
        /// <summary>
        /// String - The string to lowercase
        /// </summary>
        [DataMember]
        [Description("The text string to convert.")]
        public string String
        {
            get { return _string; }
            set { _string = value; }
        }
    }

    /// <summary>
    /// ToLowerResponse
    /// </summary>
    [DataContract]
    public class ToLowerResponse
    {
        private string _lower;
        /// <summary>
        /// Lower - The output (lowercase) string
        /// </summary>
        [DataMember]
        [Description("The converted text string.")]
        public string Lower
        {
            get { return _lower; }
            set { _lower = value; }
        }

        /// <summary>
        /// FromRequest - Perform the Lowercase function
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static ToLowerResponse FromRequest(ToLowerRequest request)
        {
            ToLowerResponse response = new ToLowerResponse();

            response._lower = request.String.ToLower(CultureInfo.CurrentUICulture);

            return response;
        }
    }

    /// <summary>
    /// ToLower - Operation
    /// </summary>
    [DisplayName("(User) ToLower")]
    [Description("Returns a copy of the String text converted to lowercase.")]
    public class ToLower : Submit<ToLowerRequest, PortSet<ToLowerResponse, Fault>>
    {
    }

    #endregion

    #region ToUpper

    /// <summary>
    /// ToUpperRequest
    /// </summary>
    [DataContract]
    public class ToUpperRequest
    {
        private string _string;
        /// <summary>
        /// String - The string to uppercase
        /// </summary>
        [DataMember]
        [Description("The text string to convert.")]
        public string String
        {
            get { return _string; }
            set { _string = value; }
        }
    }

    /// <summary>
    /// ToUpperResponse
    /// </summary>
    [DataContract]
    public class ToUpperResponse
    {
        private string _upper;
        /// <summary>
        /// Upper - The output (uppercase) string
        /// </summary>
        [DataMember]
        [Description("The converted text string.")]
        public string Upper
        {
            get { return _upper; }
            set { _upper = value; }
        }

        /// <summary>
        /// FromRequest - Perform the Uppercase function
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static ToUpperResponse FromRequest(ToUpperRequest request)
        {
            ToUpperResponse response = new ToUpperResponse();

            response._upper = request.String.ToUpper(CultureInfo.CurrentUICulture);

            return response;
        }
    }

    /// <summary>
    /// ToUpper - Operation
    /// </summary>
    [DisplayName("(User) ToUpper")]
    [Description("Returns a copy of the String text converted to uppercase.")]
    public class ToUpper : Submit<ToUpperRequest, PortSet<ToUpperResponse, Fault>>
    {
    }

    #endregion

    #region Trim

    /// <summary>
    /// TrimRequest
    /// </summary>
    [DataContract]
    public class TrimRequest
    {
        private string _string;
        /// <summary>
        /// String - The string to process
        /// </summary>
        [DataMember]
        [Description("The text string to remove spaces from.")]
        public string String
        {
            get { return _string; }
            set { _string = value; }
        }

        private bool _preserveLeftSpace;
        /// <summary>
        /// PreserveLeftSpace - Remove leading spaces if true
        /// </summary>
        [Description("Indicates whether to remove leading spaces.")]
        [DataMember]
        public bool PreserveLeftSpace
        {
            get { return _preserveLeftSpace; }
            set { _preserveLeftSpace = value; }
        }

        private bool _preserveRightSpace;
        /// <summary>
        /// PreserveRightSpace - Remove trailing spaces if true
        /// </summary>
        [Description("Indicates whether to remove trailing spaces.")]
        [DataMember]
        public bool PreserveRightSpace
        {
            get { return _preserveRightSpace; }
            set { _preserveRightSpace = value; }
        }
    }

    /// <summary>
    /// TrimResponse
    /// </summary>
    [DataContract]
    public class TrimResponse
    {
        private string _trimmed;
        /// <summary>
        /// Trimmed - The output (trimmed) string
        /// </summary>
        [DataMember]
        [Description("The resulting trimmed string.")]
        public string Trimmed
        {
            get { return _trimmed; }
            set { _trimmed = value; }
        }

        /// <summary>
        /// FromRequest - Perform the Trim function
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static TrimResponse FromRequest(TrimRequest request)
        {
            TrimResponse response = new TrimResponse();

            if (request.PreserveLeftSpace && request.PreserveRightSpace)
            {
                response._trimmed = request.String;
            }
            else if (request.PreserveLeftSpace)
            {
                response._trimmed = request.String.TrimEnd(null);
            }
            else if (request.PreserveRightSpace)
            {
                response._trimmed = request.String.TrimStart(null);
            }
            else
            {
                response._trimmed = request.String.Trim();
            }

            return response;
        }
    }

    /// <summary>
    /// Trim - Operation to remove whitespace from a string
    /// </summary>
    [DisplayName("(User) Trim")]
    [Description("Returns a text string after removing all leading (using PreserveLeftSpace) and trailing (using PreserveRightSpace) white-space characters from the String text.")]
    public class Trim : Submit<TrimRequest, PortSet<TrimResponse, Fault>>
    {
    }

    #endregion
}
