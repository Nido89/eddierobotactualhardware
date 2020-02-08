//------------------------------------------------------------------------------
//  <copyright file="GrammarUtilities.cs" company="Microsoft Corporation">
//      Copyright (C) Microsoft Corporation.  All rights reserved.
//  </copyright>
//------------------------------------------------------------------------------


namespace Microsoft.Robotics.Services.Sensors.Kinect.MicArraySpeechRecognizer
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Text;
    using System.Xml;
    using System.Xml.Serialization;
    using Microsoft.Dss.Core;
    using Microsoft.Dss.Core.Attributes;
    using Microsoft.Dss.Core.Utilities;
    using Microsoft.Dss.Services.MountService;
    using Microsoft.Speech.Recognition;
    using Microsoft.Speech.Recognition.SrgsGrammar;

    /// <summary>
    /// Utility methods for dealing with speech recognition grammars
    /// </summary>
    static public class GrammarUtilities
    {
        /// <summary>
        /// Builds a grammar object from a W3C Speech Recognition Grammar Specification (SRGS)
        /// compliant XML file.
        /// </summary>
        /// <param name="grammarStream">Stream providing access to the XML file</param>
        /// <returns>The constructed grammar object</returns>
        public static Grammar BuildSrgsGrammar(Stream grammarStream)
        {
            SrgsDocument grammarDocument = new SrgsDocument(XmlReader.Create(grammarStream));
            return new Grammar(grammarDocument);
        }

        /// <summary>
        /// Builds a grammar object from a dictionary of text-to-meaning mappings.
        /// </summary>
        /// <param name="dictionary">The dictionary with the mappings</param>
        /// <returns>The constructed grammar object</returns>
        public static Grammar BuildDictionaryGrammar(Dictionary<string, string> dictionary)
        {
            if (dictionary.Count == 0)
            {
                return null;
            }

            Choices choices = new Choices();
            foreach (KeyValuePair<string, string> entry in dictionary)
            {
                if (string.IsNullOrEmpty(entry.Key) && string.IsNullOrEmpty(entry.Value))
                {
                    continue;
                }
                SemanticResultValue phraseToSemanticsMapping = new SemanticResultValue(entry.Key, entry.Value);
                choices.Add(new GrammarBuilder(phraseToSemanticsMapping));
            }

            return new Grammar(new GrammarBuilder(choices));
        }

        /// <summary>
        /// Inserts new entries into an existing dictionary. Throws an exception if an entry already exists.
        /// </summary>
        /// <param name="dictionary">Dictionary into which entries are inserted</param>
        /// <param name="newEntries">Entries which are inserted</param>
        public static void InsertDictionary(Dictionary<string, string> dictionary, Dictionary<string, string> newEntries)
        {
            foreach (string key in newEntries.Keys)
            {
                if (dictionary.ContainsKey(key))
                {
                    throw new ArgumentException("Grammar dictionary already contains an entry with the following key: " + key);
                }
            }

            foreach (KeyValuePair<string, string> entry in newEntries)
            {
                dictionary[entry.Key] = entry.Value;
            }
        }

        /// <summary>
        /// Updates existing entries in a dictionary with values of another dictionary
        /// </summary>
        /// <param name="dictionary">Dictionary which shall be update</param>
        /// <param name="entries">Updated entries</param>
        public static void UpdateDictionary(Dictionary<string, string> dictionary, Dictionary<string, string> entries)
        {
            foreach (KeyValuePair<string, string> entry in entries)
            {
                if (dictionary.ContainsKey(entry.Key))
                {
                    dictionary[entry.Key] = entry.Value;
                }
            }
        }

        /// <summary>
        /// Inserts new entries into an existing dictionary or updates existing entries if they exist already
        /// </summary>
        /// <param name="dictionary">Dictionary that shall be modified</param>
        /// <param name="entries">Entries to insert/update the existing dictionary</param>
        public static void UpsertDictionary(Dictionary<string, string> dictionary, Dictionary<string, string> entries)
        {
            foreach (KeyValuePair<string, string> entry in entries)
            {
                dictionary[entry.Key] = entry.Value;
            }
        }

        /// <summary>
        /// Deletes entries from an existing dictionary
        /// </summary>
        /// <param name="dictionary">Dictionary from which entries shall be deleted</param>
        /// <param name="entries">Entries that shall be deleted</param>
        public static void DeleteDictionary(Dictionary<string, string> dictionary, Dictionary<string, string> entries)
        {
            foreach (string key in entries.Keys)
            {
                dictionary.Remove(key);
            }
        }
    }

    /// <summary>
    /// Grammar type used in the speech recognition service 
    /// </summary>
    [DataContract]
    public enum GrammarType
    {
        /// <summary>
        /// A simple text-to-meaning mapping type grammar
        /// </summary>
        DictionaryStyle,

        /// <summary>
        /// A W3C Speech Recognition Grammar Specification (SRGS) type
        /// grammar
        /// </summary>
        Srgs
    }

    /// <summary>
    /// Semantic value class that aims to represent the data contained in a
    /// <see cref="Microsoft.Speech.Recognition.SemanticValue"/> instance. While
    /// SemanticValue is not serializable, this class is. This conversion is
    /// transparent to a partner service, that is, the partner service has to
    /// deal with SemanticValue instances only.
    /// </summary>
    [XmlRoot(Namespace=Contract.Identifier)]
    [DataContract(ExcludeFromProxy=true)]
    public partial class RecognizedSemanticValue : IDssSerializable
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public RecognizedSemanticValue()
        {
        }

        /// <summary>
        /// Constructor that creates an instance from a SemanticValue object's 
        /// internal data. The newly created instance aims to represent the same
        /// data a SemanticValue object does.
        /// </summary>
        /// <param name="keyName">The key under which this semantic value instance is
        /// referenced by its parent. Can be null for the the root value</param>
        /// <param name="semanticValue"></param>
        public RecognizedSemanticValue(string keyName, SemanticValue semanticValue)
        {
            this.KeyName = keyName;
            this.Confidence = semanticValue.Confidence;
            this.Value = semanticValue.Value;

            // Copy children as well
            Children = new DssDictionary<string, RecognizedSemanticValue>();
            foreach (KeyValuePair<string, SemanticValue> child in semanticValue)
            {
                Children.Add(child.Key, new RecognizedSemanticValue(child.Key, child.Value));
            }
        }

        private string keyName;
        /// <summary>
        /// The key under which this semantic value instance is referenced
        /// by its parent
        /// </summary>
        [DataMember]
        [Description("The key under which this semantic value instance is referenced by its parent.")]
        public string KeyName
        {
            get { return this.keyName; }
            set { this.keyName = value; }
        }

        private float confidence;
        /// <summary>
        /// A relative measure of the certainty as to the correctness of the semantic
        /// parsing that returned the current object
        /// </summary>
        [DataMember]
        [Description("A relative measure of the certainty as to the correctness of the semantic parsing that returned the current object.")]
        public float Confidence
        {
            get { return this.confidence; }
            set { this.confidence = value; }
        }

        private RecognizedValueType typeOfValue = RecognizedValueType.Undefined;
        /// <summary>
        /// Type of which the semantic value is
        /// </summary>
        [DataMember]
        [Description("Of what type the semantic value is.")]
        public RecognizedValueType TypeOfValue
        {
            get { return this.typeOfValue; }
            set { this.typeOfValue = value; }
        }

        private bool valueBool;
        /// <summary>
        /// The semantic boolean value
        /// </summary>
        [DataMember]
        [Description("The semantic boolean value.")]
        public bool ValueBool
        {
            get { return this.valueBool; }
            set { this.valueBool = value; }
        }

        private int valueInt;
        /// <summary>
        /// The semantic int value
        /// </summary>
        [DataMember]
        [Description("The semantic int value.")]
        public int ValueInt
        {
            get { return this.valueInt; }
            set { this.valueInt = value; }
        }

        private float valueFloat;
        /// <summary>
        /// The semantic float value
        /// </summary>
        [DataMember]
        [Description("The semantic float value.")]
        public float ValueFloat
        {
            get { return this.valueFloat; }
            set { this.valueFloat = value; }
        }

        private string valueString;
        /// <summary>
        /// The semantic string value
        /// </summary>
        [DataMember]
        [Description("The semantic string value.")]
        public string ValueString
        {
            get { return this.valueString; }
            set { this.valueString = value; }
        }

        /// <summary>
        /// The semantic value
        /// </summary>
        [XmlIgnore]
        public object Value
        {
            get
            {
                #region Return set member
                // Return value from correct property
                switch (TypeOfValue)
                {
                    case RecognizedValueType.Bool:
                        return ValueBool;
                    case RecognizedValueType.Float:
                        return ValueFloat;
                    case RecognizedValueType.Int:
                        return ValueInt;
                    case RecognizedValueType.String:
                    default:
                        return ValueString;
                }
                #endregion
            }

            set
            {
                #region Set member of appropriate type
                // Inspect underlying semantic value type and set the appropriate property
                if (value is bool)
                {
                    ValueBool = (bool)value;
                    TypeOfValue = RecognizedValueType.Bool;
                }
                else if (value is int)
                {
                    ValueInt = (int)value;
                    TypeOfValue = RecognizedValueType.Int;
                }
                else if (value is float)
                {
                    ValueFloat = (float)value;
                    TypeOfValue = RecognizedValueType.Float;
                }
                else
                {
                    ValueString = value == null ? null : value.ToString();
                    TypeOfValue = RecognizedValueType.String;
                }
                #endregion
            }
        }

        private DssDictionary<string, RecognizedSemanticValue> children;
        /// <summary>
        /// A collection of semantic value children
        /// </summary>
        [DataMember]
        [Description("A collection of semantic value children.")]
        public DssDictionary<string, RecognizedSemanticValue> Children
        {
            get { return this.children; }
            set { this.children = value; }
        }

        /// <summary>
        /// Creates a SemanticValue instance from this object
        /// </summary>
        /// <returns>The created semantic value instance or null if this object's
        /// value is not set</returns>
        public SemanticValue ToSemanticValue()
        {
            if (TypeOfValue == RecognizedValueType.Undefined)
            {
                return null;
            }

            SemanticValue semanticValue = new SemanticValue(KeyName, Value, Confidence);
            foreach (KeyValuePair<string, RecognizedSemanticValue> child in Children)
            {
                semanticValue[child.Key] = child.Value.ToSemanticValue();
            }

            return semanticValue;
        }

        #region IDssSerializable Members
        /// <summary>
        /// Creates a clone of the current instance
        /// </summary>
        /// <returns>clone of the current instance (deeply cloned)</returns>
        public object Clone()
        {
            RecognizedSemanticValue clone = new RecognizedSemanticValue();
            CopyTo(clone);
            return clone;
        }

        /// <summary>
        /// Copys the values of the current instance to the target instance
        /// </summary>
        /// <param name="target">target instance</param>
        public void CopyTo(IDssSerializable target)
        {
            RecognizedSemanticValue clone = (RecognizedSemanticValue)target;

            clone.TypeOfValue = this.TypeOfValue;
            clone.Value = this.Value;

            clone.KeyName = this.KeyName;
            clone.Confidence = this.Confidence;

            if (Children != null)
            {
                clone.Children = new DssDictionary<string, RecognizedSemanticValue>();
                foreach (var child in Children)
                {
                    clone.Children[child.Key] = child.Value == null ? null : (RecognizedSemanticValue)child.Value.Clone();
                }
            }
            else
            {
                clone.Children = null;
            }
        }

        /// <summary>
        /// Deserialize the values of the current instance
        /// </summary>
        /// <param name="reader">reader from which to deserialize</param>
        public object Deserialize(BinaryReader reader)
        {
            DeserializeValue(reader);
            DeserializeKeyName(reader);
            DeserializeChildren(reader);

            return this;
        }

        /// <summary>
        /// Deserializes the Value of the current instance
        /// </summary>
        /// <param name="reader">the reader from which to deserialize</param>
        private void DeserializeValue(BinaryReader reader)
        {
            this.TypeOfValue = (RecognizedValueType)reader.ReadInt32();

            switch (TypeOfValue)
            {
                case RecognizedValueType.Bool:
                    this.ValueBool = reader.ReadBoolean();
                    break;
                case RecognizedValueType.Float:
                    ValueFloat = reader.ReadSingle();
                    break;
                case RecognizedValueType.Int:
                    this.ValueInt = reader.ReadInt32();
                    break;
                case RecognizedValueType.String:
                    if (reader.ReadByte() != 0)
                    {
                        this.ValueString = reader.ReadString();
                    }
                    else
                    {
                        this.ValueString = null;
                    }
                    break;
                default:
                    this.Value = null;
                    break;
            }
        }

        /// <summary>
        /// Deserializes the KeyName of the current instance
        /// </summary>
        /// <param name="reader">the reader from which to deserialize</param>
        private void DeserializeKeyName(BinaryReader reader)
        {
            if (reader.ReadByte() != 0)
            {
                this.KeyName = reader.ReadString();
            }
            else
            {
                this.KeyName = null;
            }
        }

        /// <summary>
        /// Deserializes the Children of the current instance
        /// </summary>
        /// <param name="reader">the reader from which to deserialize</param>
        private void DeserializeChildren(BinaryReader reader)
        {
            if (reader.ReadByte() != 0)
            {
                Children = new DssDictionary<string, RecognizedSemanticValue>();
                int count = reader.ReadInt32();

                for (int i = 0; i < count; i++)
                {
                    string name = null;
                    RecognizedSemanticValue child = null;
                    if (reader.ReadByte() != 0)
                    {
                        name = reader.ReadString();
                    }
                    if (reader.ReadByte() != 0)
                    {
                        child = new RecognizedSemanticValue();
                        child.Deserialize(reader);
                    }
                    Children[name] = child;
                }
            }
            else
            {
                Children = null;
            }
        }

        /// <summary>
        /// Serializes the current instance using binary serialization
        /// </summary>
        /// <param name="writer">The writer to which to serialize</param>
        public void Serialize(BinaryWriter writer)
        {
            this.SerializeValue(writer);
            this.SerializeKeyName(writer);
            this.SerializeChildren(writer);
        }

        /// <summary>
        /// Serializes the Value of the current instance
        /// </summary>
        /// <param name="writer">The writer to which to serialize</param>
        private void SerializeChildren(BinaryWriter writer)
        {
            if (this.Children == null)
            {
                writer.Write((byte)0);
            }
            else
            {
                writer.Write((byte)1);
                writer.Write(this.Children.Count);

                foreach (string name in this.Children.Keys)
                {
                    if (name == null)
                    {
                        writer.Write((byte)0);
                    }
                    else
                    {
                        writer.Write((byte)1);
                        writer.Write(name);
                    }
                    RecognizedSemanticValue child = this.Children[name];
                    if (child == null)
                    {
                        writer.Write((byte)0);
                    }
                    else
                    {
                        writer.Write((byte)1);
                        child.Serialize(writer);
                    }
                }
            }
        }

        /// <summary>
        /// Serializes the Value of the current instance
        /// </summary>
        /// <param name="writer">The writer to which to serialize</param>
        private void SerializeKeyName(BinaryWriter writer)
        {
            if (this.KeyName == null)
            {
                writer.Write((byte)0);
            }
            else
            {
                writer.Write((byte)1);
                writer.Write(this.KeyName);
            }
        }

        /// <summary>
        /// Serializes the Value of the current instance
        /// </summary>
        /// <param name="writer">The writer to which to serialize</param>
        private void SerializeValue(BinaryWriter writer)
        {
            writer.Write((int)this.TypeOfValue);

            switch (this.TypeOfValue)
            {
                case RecognizedValueType.Bool:
                    writer.Write(this.ValueBool);
                    break;
                case RecognizedValueType.Float:
                    writer.Write(this.ValueFloat);
                    break;
                case RecognizedValueType.Int:
                    writer.Write(this.ValueInt);
                    break;
                case RecognizedValueType.String:
                    if (ValueString == null)
                    {
                        writer.Write((byte)0);
                    }
                    else
                    {
                        writer.Write((byte)1);
                        writer.Write(this.ValueString);
                    }
                    break;
                default:
                    break;
            }
        }
        #endregion
    }

    /// <summary>
    /// Type of which the semantic value can be
    /// </summary>
    [DataContract]
    public enum RecognizedValueType
    {
        /// <summary>
        /// Type bool
        /// </summary>
        Bool,

        /// <summary>
        /// Type float
        /// </summary>
        Float,

        /// <summary>
        /// Type int
        /// </summary>
        Int,

        /// <summary>
        /// Type string
        /// </summary>
        String,

        /// <summary>
        /// Undefined (not set)
        /// </summary>
        Undefined
    }
}
