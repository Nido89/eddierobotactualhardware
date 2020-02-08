//------------------------------------------------------------------------------
//  <copyright file="LogFileLoading.cs" company="Microsoft Corporation">
//      Copyright (C) Microsoft Corporation.  All rights reserved.
//  </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Robotics.Tools.DssLogAnalyzer
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml;
    using Microsoft.Ccr.Core;
    using Microsoft.Dss.Core;
    using Microsoft.Dss.Services.Forwarders;
    using Microsoft.Dss.Services.Serializer;
    using Microsoft.Robotics.Tools.DssLogAnalyzer.Properties;

    /// <summary>
    /// Stateless helper class for deserializing envelopes
    /// </summary>
    public static class DssLogAnalyzerHelper
    {
        /// <summary>
        /// Deserialize the a envelope record
        /// </summary>
        /// <param name="envelope">Envelope to be deserialized</param>
        /// <returns>Deserialized envelope</returns>
        public static ForwarderEnvelope DeserializeEnvelope(Envelope envelope)
        {
            using (var fs = new FileStream(envelope.Filename, FileMode.Open, FileAccess.Read, FileShare.None, 1))
            {
                var reader = new BinaryReader(fs);
                reader.BaseStream.Seek(envelope.Offset, SeekOrigin.Begin);

                Type t = typeof(ForwarderEnvelope);
                var instance = (IDssSerializable)Activator.CreateInstance(t);
                
                return instance.Deserialize(reader) as ForwarderEnvelope;
            }
        }
    }

    /// <summary>
    /// Main service class
    /// </summary>
    public partial class DssLogAnalyzerService
    {
        /// <summary>
        /// The initial envelope list capacity
        /// </summary>
        private const int DefaultInitialEnvelopeCapacity = 128;

        /// <summary>
        /// Get guid byte length
        /// </summary>
        private static int guidByteLength = Guid.Empty.ToByteArray().Length;

        /// <summary>
        /// Enum for results of log processing
        /// </summary>
        private enum LogFileResult
        {
            /// <summary>
            /// Successful read
            /// </summary>
            Success,

            /// <summary>
            /// Failed to read file
            /// </summary>
            Failure,

            /// <summary>
            /// Xml log format file detected
            /// </summary>
            XmlLogsDetected
        }

        /// <summary>
        /// Read bytes until the next body node
        /// </summary>
        /// <param name="reader">The xml reader object</param>
        /// <returns>True for successful read</returns>
        private static bool ReadToNextBodyNode(XmlReader reader)
        {
            try
            {
                return reader.ReadToFollowing("Body", W3C.Soap.Contract.Identifier);
            }
            catch (System.Xml.XmlException)
            {
                // handle incomplete xml documents as if we reached the end of the file
                return false;
            }
        }

        /// <summary>
        /// Read bytes until the next envelope node
        /// </summary>
        /// <param name="reader">The xml reader object</param>
        /// <returns>True for successful read</returns>
        private static bool ReadToNextEnvelopeNode(XmlReader reader)
        {
            try
            {
                return reader.ReadToFollowing("Envelope", W3C.Soap.Contract.Identifier);
            }
            catch (System.Xml.XmlException)
            {
                // handle incomplete xml documents as if we reached the end of the file
                return false;
            }
        }

        /// <summary>
        /// Load the dss log files
        /// </summary>
        /// <param name="rootFolder">Name of the root directory</param>
        /// <returns>Load result</returns>
        private LogFileResult LoadLogs(string rootFolder)
        {
            string[] allLogFiles = Directory.GetFiles(rootFolder, "*.log", SearchOption.AllDirectories);

            if (allLogFiles.Length == 0)
            {
                this.DisplayError(Resources.FolderContainsNoLogFiles);
                return LogFileResult.Failure;
            }
            else
            {
                if (this.AreBinaryFiles(allLogFiles) == false)
                {
                    return LogFileResult.XmlLogsDetected;
                }

                if (this.serviceState.LogFileEnvelopes == null)
                {
                    this.serviceState.LogFileEnvelopes = new Dictionary<string, List<EnvelopeList>>();
                }
                else
                {
                    this.serviceState.LogFileEnvelopes.Clear();
                    if (this.serviceState.Envelopes != null)
                    {
                        this.serviceState.Envelopes.Clear();
                    }
                }

                foreach (string logFile in allLogFiles)
                {
                    List<EnvelopeList> envelopesFromFile = new List<EnvelopeList>();
                    this.serviceState.LogFileEnvelopes.Add(logFile, envelopesFromFile);
                    this.LoadBinaryLogs(logFile);
                }
            }

            this.serviceState.LogFilesFolder = rootFolder;

            this.mainPort.Post(new LogFilesLoaded());

            return LogFileResult.Success;
        }

        /// <summary>
        /// Check if files are binary and not xml
        /// </summary>
        /// <param name="logFiles">Array of log files</param>
        /// <returns>True if files are binary</returns>
        private bool AreBinaryFiles(string[] logFiles)
        {
            if (logFiles.Length > 0)
            {
                for (int i = 0; i < logFiles.Length; i++)
                {
                    using (var fs = new FileStream(logFiles[i], FileMode.Open, FileAccess.Read, FileShare.None, 1))
                    {
                        if (fs.Length == 0)
                        {
                            continue;
                        }

                        try
                        {
                            var xmlReader = XmlReader.Create(fs);
                            xmlReader.Read();
                        }
                        catch (System.Xml.XmlException)
                        {
                            return true;
                        }

                        // xml files
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Convert xml logs to binary
        /// </summary>
        /// <param name="logFilesFolder">Root directory of files to be converted</param>
        /// <param name="newLogFilesFolder">Directory to place converted files</param>
        /// <param name="progressUpdate">Progress update action</param>
        /// <returns>DSS result port</returns>
        private Port<LogFileResult> ConvertXmlLogsToBinary(
            string logFilesFolder,
            string newLogFilesFolder,
            Action<double> progressUpdate)
        {
            var resultPort = new Port<LogFileResult>();

            if (Directory.Exists(logFilesFolder) == false)
            {
                this.DisplayError(Resources.FolderNotFound);
                resultPort.Post(LogFileResult.Failure);
            }
            else
            {
                if (Directory.Exists(newLogFilesFolder) == false)
                {
                    Directory.CreateDirectory(newLogFilesFolder);
                }

                var logFiles = Directory.GetFiles(logFilesFolder, "*.log", SearchOption.AllDirectories);

                SpawnIterator(() => this.ConvertXmlFilesToBinary(logFilesFolder, newLogFilesFolder, resultPort, logFiles, progressUpdate));
            }

            return resultPort;
        }

        /// <summary>
        /// Convert xml log files to binary
        /// </summary>
        /// <param name="logFilesFolder">Root directory of files to be converted</param>
        /// <param name="newLogFilesFolder">Location of converted files</param>
        /// <param name="resultPort">Result dss port</param>
        /// <param name="logFiles">Array of log files</param>
        /// <param name="progressUpdate">Progress update action</param>
        /// <returns>DSS result port</returns>
        private IEnumerator<ITask> ConvertXmlFilesToBinary(
            string logFilesFolder,
            string newLogFilesFolder,
            Port<LogFileResult> resultPort,
            string[] logFiles, 
            Action<double> progressUpdate)
        {
            double currentProgress = 0;

            foreach (var logFile in logFiles)
            {
                var newLogFile = logFile.Replace(logFilesFolder, newLogFilesFolder);
                var newLogFileDirectory = Path.GetDirectoryName(newLogFile);
                if (Directory.Exists(newLogFileDirectory) == false)
                {
                    Directory.CreateDirectory(newLogFileDirectory);
                }

                yield return new IterativeTask(() => this.ConvertXmlLogToBinary(logFile, newLogFile, resultPort));

                currentProgress += 100.0 / logFiles.Length;
                progressUpdate(currentProgress);
            }

            resultPort.Post(LogFileResult.Success);
        }

        /// <summary>
        /// Convert xml files to binary
        /// </summary>
        /// <param name="logFile">File to be converted</param>
        /// <param name="newLogFile">New file name</param>
        /// <param name="resultPort">DSS result port</param>
        /// <returns>ITask enumerator</returns>
        private IEnumerator<ITask> ConvertXmlLogToBinary(string logFile, string newLogFile, Port<LogFileResult> resultPort)
        {
            var envelopes = new List<Envelope>(DefaultInitialEnvelopeCapacity);

            using (var fs = new FileStream(logFile, FileMode.Open, FileAccess.Read, FileShare.None, 1))
            {
                if (fs.Length != 0)
                {
                    using (var newFs = new FileStream(newLogFile, FileMode.Create, FileAccess.Write, FileShare.None, 1))
                    {
                        using (var bw = new BinaryWriter(newFs))
                        {
                            var reader = XmlReader.Create(fs);
                            while (ReadToNextEnvelopeNode(reader))
                            {
                                var deserHeader = new Deserialize(reader);
                                SerializerPort.Post(deserHeader);

                                DeserializeResult headerDeserResult = null;
                                yield return Arbiter.Choice(
                                    deserHeader.ResultPort,
                                    w3cHeader => headerDeserResult = w3cHeader,
                                    failure => LogError(failure));

                                if (headerDeserResult != null && ReadToNextBodyNode(reader))
                                {
                                    var deserBody = new Deserialize(reader);
                                    SerializerPort.Post(deserBody);

                                    DeserializeResult bodyDeserResult = null;

                                    yield return Arbiter.Choice(
                                        deserBody.ResultPort,
                                       bodyContents => bodyDeserResult = bodyContents,
                                       failure => LogError(failure));

                                    if (bodyDeserResult != null)
                                    {
                                        this.SerializeToBinary(headerDeserResult, bodyDeserResult, bw);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Serailizer to binary format
        /// </summary>
        /// <param name="headerDeserResult">Result of deserialization of header</param>
        /// <param name="bodyDeserResult">Result of deserialization of body</param>
        /// <param name="bw">Binary writer</param>
        private void SerializeToBinary(DeserializeResult headerDeserResult, DeserializeResult bodyDeserResult, BinaryWriter bw)
        {
            MemoryStream ms = null;
            BinaryWriter tempBw = null;

            try
            {
                ms = new MemoryStream();
                bw = new BinaryWriter(ms);

                var originalHeader = headerDeserResult.Instance as W3C.Soap.Header;

                var forwarderEnvelope = new ForwarderEnvelope();
                forwarderEnvelope.Action = HeaderParser.ParseAction(originalHeader.Any);
                forwarderEnvelope.MessageId = HeaderParser.ParseMessageId(originalHeader.Any);
                forwarderEnvelope.TimeStamp = HeaderParser.ParseTimeStamp(originalHeader.Any);
                forwarderEnvelope.To = HeaderParser.ParseTo(originalHeader.Any);
                forwarderEnvelope.From = HeaderParser.ParseFrom(originalHeader.Any);

                forwarderEnvelope.Body = bodyDeserResult.Instance;

                forwarderEnvelope.Serialize(tempBw);

                bw.Write(BitConverter.GetBytes(ms.Position), 0, sizeof(long));
                forwarderEnvelope.Serialize(bw);
            }
            finally
            {
                if (tempBw != null)
                {
                    tempBw.Dispose();
                }

                if (ms != null)
                {
                    ms.Dispose();
                }
            }
        }

        /// <summary>
        /// Load binary log envelopes
        /// </summary>
        /// <param name="logFile">Log file name</param>
        private void LoadBinaryLogs(string logFile)
        {
            var envelopes = new List<Envelope>(DefaultInitialEnvelopeCapacity);

            try
            {
                using (var fs = new FileStream(logFile, FileMode.Open, FileAccess.Read, FileShare.None, 1))
                {
                    var reader = new BinaryReader(fs);

                    while (true)
                    {
                        var envelopeLengthArray = new byte[8];
                        int bytesRead = reader.Read(envelopeLengthArray, 0, envelopeLengthArray.Length);
                        if (bytesRead == 0)
                        {
                            break;
                        }

                        var envelopeLength = BitConverter.ToInt64(envelopeLengthArray, 0);

                        if (envelopeLength == -1)
                        {
                            break;
                        }

                        long envelopeOffset = reader.BaseStream.Position;

                        var header = this.ReadHeader(reader);

                        reader.BaseStream.Seek(envelopeOffset + envelopeLength, SeekOrigin.Begin);

                        // add an envelope
                        envelopes.Add(new Envelope
                        {
                            Offset = envelopeOffset,
                            Filename = logFile,
                            Header = header
                        });
                    }
                }
                
                // Keep this for backward compatibility
                this.serviceState.Envelopes.Add(new EnvelopeList { Envelopes = envelopes });

                List<EnvelopeList> envelopeList;
                bool res = this.serviceState.LogFileEnvelopes.TryGetValue(logFile, out envelopeList);
                if (res)
                {
                    envelopeList.Add(new EnvelopeList { Envelopes = envelopes });
                }
            }
            catch (Exception ex)
            {
                LogError(logFile);
                LogError(ex);
            }
        }

        /// <summary>
        /// Read envelope header
        /// </summary>
        /// <param name="reader">Binary reader</param>
        /// <returns>Envelope header</returns>
        private EnvelopeHeader ReadHeader(BinaryReader reader)
        {
            var header = new EnvelopeHeader();

            header.Action = reader.ReadString();

            var originalRequestProperties = (DsspOperationProperties)reader.ReadByte();
            var from = reader.ReadString();
            if (string.IsNullOrEmpty(from) == false)
            {
                header.From = from;
            }
            else
            {
                header.From = null;
            }

            header.MessageId = new Guid(reader.ReadBytes(guidByteLength));
            header.TimeStamp = new DateTime(reader.ReadInt64(), DateTimeKind.Utc);
            header.To = reader.ReadString();

            return header;
        }

        /// <summary>
        /// Header parsing class
        /// </summary>
        private static class HeaderParser
        {
            // prefixes are hardcoded in DSS so safe to assume that's what they are in the logs

            /// <summary>
            /// Parse the to field
            /// </summary>
            /// <param name="headers">Header array</param>
            /// <returns>String result</returns>
            internal static string ParseTo(object[] headers)
            {
                return FindElement<string>(headers, "wsa:To", text => text, string.Empty);
            }

            /// <summary>
            /// Parse the from field
            /// </summary>
            /// <param name="headers">Header array</param>
            /// <returns>String result</returns>
            internal static string ParseFrom(object[] headers)
            {
                return FindElement<string>(headers, "wsa:ReplyTo", text => text, string.Empty);
            }

            /// <summary>
            /// Parse the timestamp
            /// </summary>
            /// <param name="headers">Header array</param>
            /// <returns>Datetime result</returns>
            internal static DateTime ParseTimeStamp(object[] headers)
            {
                return FindElement<DateTime>(headers, "d:Timestamp", text => DateTime.Parse(text), DateTime.Now);
            }

            /// <summary>
            /// Parse message id
            /// </summary>
            /// <param name="headers">Header array</param>
            /// <returns>Guid result</returns>
            internal static Guid ParseMessageId(object[] headers)
            {
                return FindElement<Guid>(
                    headers,
                    "wsa:MessageID",
                    text =>
                    {
                        var splitId = text.Split(new[] { ':' });
                        return new Guid(splitId[splitId.Length - 1]);
                    },
                    Guid.Empty);
            }

            /// <summary>
            /// Parse action
            /// </summary>
            /// <param name="headers">Header array</param>
            /// <returns>String result</returns>
            internal static string ParseAction(object[] headers)
            {
                return FindElement<string>(headers, "wsa:Action", text => text, string.Empty);
            }

            /// <summary>
            /// Find the specified element
            /// </summary>
            /// <typeparam name="T">Object type</typeparam>
            /// <param name="headers">Header array</param>
            /// <param name="elementName">Element name</param>
            /// <param name="textConverter">Text converter method</param>
            /// <param name="defaultValue">Default value</param>
            /// <returns>Return found type</returns>
            private static T FindElement<T>(object[] headers, string elementName, Func<string, T> textConverter, T defaultValue)
            {
                foreach (XmlElement element in headers)
                {
                    if (element.Name == elementName)
                    {
                        return textConverter(element.InnerText);
                    }
                }

                // timestamp not found
                return defaultValue;
            }
        }
    }
}
