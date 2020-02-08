//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: GrammarUtilities.cs $ $Revision: 5 $
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.ComponentModel;
using System.Speech.Recognition;
using System.Speech.Recognition.SrgsGrammar;
using System.Xml;

using Microsoft.Dss.Services.MountService;
using Microsoft.Dss.Core;
using Microsoft.Dss.Core.Attributes;
using System.Xml.Serialization;
using Microsoft.Dss.Core.Utilities;

namespace Microsoft.Robotics.Technologies.Speech.SpeechRecognizer
{
    /// <summary>
    /// Utility methods for dealing with speech recognition grammars
    /// </summary>
    public class GrammarUtilities
    {
        private GrammarUtilities()
        {
        }

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
    /// <see cref="System.Speech.Recognition.SemanticValue"/> instance. While
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
            KeyName = keyName;
            Confidence = semanticValue.Confidence;
            Value = semanticValue.Value;

            // Copy children as well
            Children = new DssDictionary<string, RecognizedSemanticValue>();
            foreach (KeyValuePair<string, SemanticValue> child in semanticValue)
            {
                Children.Add(child.Key, new RecognizedSemanticValue(child.Key, child.Value));
            }
        }

        private string _keyName;
        /// <summary>
        /// The key under which this semantic value instance is referenced
        /// by its parent
        /// </summary>
        [DataMember]
        [Description("The key under which this semantic value instance is referenced by its parent.")]
        public string KeyName
        {
            get { return _keyName; }
            set { _keyName = value; }
        }

        private float _confidence;
        /// <summary>
        /// A relative measure of the certainty as to the correctness of the semantic
        /// parsing that returned the current object
        /// </summary>
        [DataMember]
        [Description("A relative measure of the certainty as to the correctness of the semantic parsing that returned the current object.")]
        public float Confidence
        {
            get { return _confidence; }
            set { _confidence = value; }
        }

        private RecognizedValueType _typeOfValue = RecognizedValueType.Undefined;
        /// <summary>
        /// Type of which the semantic value is
        /// </summary>
        [DataMember]
        [Description("Of what type the semantic value is.")]
        public RecognizedValueType TypeOfValue
        {
            get { return _typeOfValue; }
            set { _typeOfValue = value; }
        }

        private bool _valueBool;
        /// <summary>
        /// The semantic boolean value
        /// </summary>
        [DataMember]
        [Description("The semantic boolean value.")]
        public bool ValueBool
        {
            get { return _valueBool; }
            set { _valueBool = value; }
        }

        private int _valueInt;
        /// <summary>
        /// The semantic int value
        /// </summary>
        [DataMember]
        [Description("The semantic int value.")]
        public int ValueInt
        {
            get { return _valueInt; }
            set { _valueInt = value; }
        }

        private float _valueFloat;
        /// <summary>
        /// The semantic float value
        /// </summary>
        [DataMember]
        [Description("The semantic float value.")]
        public float ValueFloat
        {
            get { return _valueFloat; }
            set { _valueFloat = value; }
        }

        private string _valueString;
        /// <summary>
        /// The semantic string value
        /// </summary>
        [DataMember]
        [Description("The semantic string value.")]
        public string ValueString
        {
            get { return _valueString; }
            set { _valueString = value; }
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

        private DssDictionary<string, RecognizedSemanticValue> _children;
        /// <summary>
        /// A collection of semantic value children
        /// </summary>
        [DataMember]
        [Description("A collection of semantic value children.")]
        public DssDictionary<string, RecognizedSemanticValue> Children
        {
            get { return _children; }
            set { _children = value; }
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

            clone.TypeOfValue = TypeOfValue;
            clone.Value = Value;

            clone.KeyName = KeyName;
            clone.Confidence = Confidence;

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
            TypeOfValue = (RecognizedValueType)reader.ReadInt32();

            switch (TypeOfValue)
            {
                case RecognizedValueType.Bool:
                    ValueBool = reader.ReadBoolean();
                    break;
                case RecognizedValueType.Float:
                    ValueFloat = reader.ReadSingle();
                    break;
                case RecognizedValueType.Int:
                    ValueInt = reader.ReadInt32();
                    break;
                case RecognizedValueType.String:
                    if (reader.ReadByte() != 0)
                    {
                        ValueString = reader.ReadString();
                    }
                    else
                    {
                        ValueString = null;
                    }
                    break;
                default:
                    Value = null;
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
                KeyName = reader.ReadString();
            }
            else
            {
                KeyName = null;
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
            SerializeValue(writer);
            SerializeKeyName(writer);
            SerializeChildren(writer);
        }

        /// <summary>
        /// Serializes the Value of the current instance
        /// </summary>
        /// <param name="writer">The writer to which to serialize</param>
        private void SerializeChildren(BinaryWriter writer)
        {
            if (Children == null)
            {
                writer.Write((byte)0);
            }
            else
            {
                writer.Write((byte)1);
                writer.Write(Children.Count);

                foreach (string name in Children.Keys)
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
                    RecognizedSemanticValue child = Children[name];
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
            if (KeyName == null)
            {
                writer.Write((byte)0);
            }
            else
            {
                writer.Write((byte)1);
                writer.Write(KeyName);
            }
        }

        /// <summary>
        /// Serializes the Value of the current instance
        /// </summary>
        /// <param name="writer">The writer to which to serialize</param>
        private void SerializeValue(BinaryWriter writer)
        {
            writer.Write((int)TypeOfValue);

            switch (TypeOfValue)
            {
                case RecognizedValueType.Bool:
                    writer.Write(ValueBool);
                    break;
                case RecognizedValueType.Float:
                    writer.Write(ValueFloat);
                    break;
                case RecognizedValueType.Int:
                    writer.Write(ValueInt);
                    break;
                case RecognizedValueType.String:
                    if (ValueString == null)
                    {
                        writer.Write((byte)0);
                    }
                    else
                    {
                        writer.Write((byte)1);
                        writer.Write(ValueString);
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
