//------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for
// license information.
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Linq;

namespace Microsoft.Azure.NotificationHubs.Messaging
{
    /// <summary>
    /// A data member of an XML contract. Members are listed in DataContractSerializer order: base class first, then by Order and name.
    /// </summary>
    internal sealed class XmlMember
    {
        XmlMember(string name, Action<object, XmlWriter> write, Action<object, XElement> read)
        {
            Name = name;
            Write = write;
            Read = read;
        }

        public string Name { get; }

        public Action<object, XmlWriter> Write { get; }

        public Action<object, XElement> Read { get; }

        public static XmlMember Create<T>(string name, Action<XmlWriter, string, T> write, Action<T, XElement> read) =>
            new XmlMember(name, (value, writer) => write(writer, name, (T)value), (value, element) => read((T)value, element));

        public static XmlMember[] Extend(XmlMember[] baseMembers, params XmlMember[] members) => baseMembers.Concat(members).ToArray();
    }

    /// <summary>
    /// Reads and writes XML in the format produced by DataContractSerializer, without reflection.
    /// </summary>
    internal static class XmlContract
    {
        public const string Namespace = ManagementStrings.Namespace;
        const string InstanceNamespace = "http://www.w3.org/2001/XMLSchema-instance";
        const string ArraysNamespace = "http://schemas.microsoft.com/2003/10/Serialization/Arrays";
        static readonly XName NilName = XName.Get("nil", InstanceNamespace);
        static readonly XName TypeName = XName.Get("type", InstanceNamespace);

        public static void WriteObject(XmlWriter writer, string name, object value, XmlMember[] members, string instanceType = null)
        {
            writer.WriteStartElement(name, Namespace);
            writer.WriteAttributeString("xmlns", "i", null, InstanceNamespace);
            WriteInstanceType(writer, instanceType);
            WriteMembers(writer, value, members);
            writer.WriteEndElement();
        }

        public static void WriteMembers(XmlWriter writer, object value, XmlMember[] members)
        {
            var unknownElements = (value as EntityDescription)?.UnknownXmlElements;
            WriteUnknownElements(writer, unknownElements, -1);
            for (var i = 0; i < members.Length; i++)
            {
                members[i].Write(value, writer);
                WriteUnknownElements(writer, unknownElements, i);
            }
        }

        // Like DataContractSerializer, members must appear in order; anything else is kept as an unknown element positioned after the last member read
        public static void ReadMembers(XElement element, object value, XmlMember[] members)
        {
            var entity = value as EntityDescription;
            var next = 0;
            foreach (var child in element.Elements())
            {
                var index = child.Name.NamespaceName == Namespace ? FindMember(members, next, child.Name.LocalName) : -1;
                if (index >= 0)
                {
                    members[index].Read(value, child);
                    next = index + 1;
                }
                else if (entity != null)
                {
                    (entity.UnknownXmlElements ??= new List<(int, XElement)>()).Add((next - 1, child));
                }
            }
        }

        public static XElement ReadElement(XmlReader reader, string name)
        {
            if (reader.MoveToContent() != XmlNodeType.Element)
            {
                throw new XmlException($"Expecting element '{name}'.");
            }

            if (reader.LocalName != name || reader.NamespaceURI != Namespace)
            {
                throw new SerializationException($"Expecting element '{name}' from namespace '{Namespace}'. Encountered '{reader.LocalName}' from namespace '{reader.NamespaceURI}'.");
            }

            return (XElement)XNode.ReadFrom(reader);
        }

        public static string ReadInstanceType(XElement element)
        {
            var type = (string)element.Attribute(TypeName);
            return type?.Substring(type.IndexOf(':') + 1);
        }

        public static bool IsNil(XElement element)
        {
            var nil = element.Attribute(NilName);
            return nil != null && XmlConvert.ToBoolean(nil.Value);
        }

        public static void WriteNil(XmlWriter writer, string name)
        {
            writer.WriteStartElement(name, Namespace);
            writer.WriteAttributeString("nil", InstanceNamespace, "true");
            writer.WriteEndElement();
        }

        public static void WriteNested(XmlWriter writer, string name, object value, XmlMember[] members, bool emitDefault, string instanceType = null)
        {
            if (value == null)
            {
                if (emitDefault)
                {
                    WriteNil(writer, name);
                }

                return;
            }

            writer.WriteStartElement(name, Namespace);
            WriteInstanceType(writer, instanceType);
            WriteMembers(writer, value, members);
            writer.WriteEndElement();
        }

        public static T ReadNested<T>(XElement element, T value, XmlMember[] members) where T : class
        {
            if (IsNil(element))
            {
                return null;
            }

            ReadMembers(element, value, members);
            return value;
        }

        public static void WriteList<T>(XmlWriter writer, string name, IEnumerable<T> items, string itemName, Action<XmlWriter, string, T> writeItem, bool emitDefault)
        {
            if (items == null)
            {
                if (emitDefault)
                {
                    WriteNil(writer, name);
                }

                return;
            }

            writer.WriteStartElement(name, Namespace);
            foreach (var item in items)
            {
                writeItem(writer, itemName, item);
            }

            writer.WriteEndElement();
        }

        public static List<T> ReadList<T>(XElement element, Func<XElement, T> readItem) =>
            IsNil(element) ? null : element.Elements().Select(readItem).ToList();

        public static void WriteDictionary<TValue>(XmlWriter writer, string name, IEnumerable<KeyValuePair<string, TValue>> items, string itemName, string keyName, string valueName, Action<XmlWriter, string, TValue> writeValue, bool emitDefault) =>
            WriteList(writer, name, items, itemName, (w, n, item) =>
            {
                w.WriteStartElement(n, Namespace);
                WriteString(w, keyName, item.Key, true);
                writeValue(w, valueName, item.Value);
                w.WriteEndElement();
            }, emitDefault);

        public static TDictionary ReadDictionary<TDictionary, TValue>(XElement element, TDictionary target, string keyName, string valueName, Func<XElement, TValue> readValue)
            where TDictionary : class, IDictionary<string, TValue>
        {
            if (IsNil(element))
            {
                return null;
            }

            foreach (var item in element.Elements())
            {
                target.Add(ReadString(item.Element(XName.Get(keyName, Namespace))), readValue(item.Element(XName.Get(valueName, Namespace))));
            }

            return target;
        }

        public static void WriteStringDictionary(XmlWriter writer, string name, IEnumerable<KeyValuePair<string, string>> items, string itemName, string keyName, string valueName, bool emitDefault) =>
            WriteDictionary(writer, name, items, itemName, keyName, valueName, (w, n, value) => WriteString(w, n, value, true), emitDefault);

        public static TDictionary ReadStringDictionary<TDictionary>(XElement element, TDictionary target, string keyName, string valueName)
            where TDictionary : class, IDictionary<string, string> =>
            ReadDictionary<TDictionary, string>(element, target, keyName, valueName, ReadString);

        // DataContractSerializer's default contract for Dictionary<string, string>
        public static void WriteArrayDictionary(XmlWriter writer, string name, IDictionary<string, string> items)
        {
            if (items == null)
            {
                return;
            }

            writer.WriteStartElement(name, Namespace);
            writer.WriteAttributeString("xmlns", "a", null, ArraysNamespace);
            foreach (var item in items)
            {
                writer.WriteStartElement("KeyValueOfstringstring", ArraysNamespace);
                writer.WriteElementString("Key", ArraysNamespace, item.Key);
                writer.WriteStartElement("Value", ArraysNamespace);
                if (item.Value == null)
                {
                    writer.WriteAttributeString("nil", InstanceNamespace, "true");
                }
                else
                {
                    writer.WriteString(item.Value);
                }

                writer.WriteEndElement();
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

        public static Dictionary<string, string> ReadArrayDictionary(XElement element)
        {
            if (IsNil(element))
            {
                return null;
            }

            var result = new Dictionary<string, string>();
            foreach (var item in element.Elements())
            {
                result.Add(ReadString(item.Element(XName.Get("Key", ArraysNamespace))), ReadString(item.Element(XName.Get("Value", ArraysNamespace))));
            }

            return result;
        }

        public static void WriteString(XmlWriter writer, string name, string value, bool emitDefault)
        {
            if (value != null)
            {
                writer.WriteElementString(name, Namespace, value);
            }
            else if (emitDefault)
            {
                WriteNil(writer, name);
            }
        }

        public static string ReadString(XElement element) => IsNil(element) ? null : element.Value;

        public static void WriteCData(XmlWriter writer, string name, CDataMember value)
        {
            if (value == null)
            {
                WriteNil(writer, name);
                return;
            }

            writer.WriteStartElement(name, Namespace);
            if (!string.IsNullOrEmpty(value.Value))
            {
                writer.WriteCData(value.Value);
            }

            writer.WriteEndElement();
        }

        public static CDataMember ReadCData(XElement element) => IsNil(element) ? null : new CDataMember(element.Value);

        public static void WriteUri(XmlWriter writer, string name, Uri value, bool emitDefault) =>
            WriteString(writer, name, value?.GetComponents(UriComponents.SerializationInfoString, UriFormat.UriEscaped), emitDefault);

        public static Uri ReadUri(XElement element)
        {
            var value = ReadString(element);
            return value == null ? null : new Uri(value, UriKind.RelativeOrAbsolute);
        }

        public static void WriteDateTime(XmlWriter writer, string name, DateTime? value)
        {
            if (value.HasValue)
            {
                writer.WriteElementString(name, Namespace, XmlConvert.ToString(value.Value, XmlDateTimeSerializationMode.RoundtripKind));
            }
        }

        public static DateTime? ReadDateTime(XElement element) =>
            IsNil(element) ? (DateTime?)null : XmlConvert.ToDateTime(element.Value, XmlDateTimeSerializationMode.RoundtripKind);

        public static void WriteTimeSpan(XmlWriter writer, string name, TimeSpan? value)
        {
            if (value.HasValue)
            {
                writer.WriteElementString(name, Namespace, XmlConvert.ToString(value.Value));
            }
        }

        public static TimeSpan? ReadTimeSpan(XElement element) => IsNil(element) ? (TimeSpan?)null : XmlConvert.ToTimeSpan(element.Value);

        public static void WriteLong(XmlWriter writer, string name, long? value)
        {
            if (value.HasValue)
            {
                writer.WriteElementString(name, Namespace, XmlConvert.ToString(value.Value));
            }
        }

        public static long? ReadLong(XElement element) => IsNil(element) ? (long?)null : XmlConvert.ToInt64(element.Value);

        public static void WriteInt(XmlWriter writer, string name, int? value)
        {
            if (value.HasValue)
            {
                writer.WriteElementString(name, Namespace, XmlConvert.ToString(value.Value));
            }
        }

        public static int? ReadInt(XElement element) => IsNil(element) ? (int?)null : XmlConvert.ToInt32(element.Value);

        public static void WriteDecimal(XmlWriter writer, string name, decimal? value)
        {
            if (value.HasValue)
            {
                writer.WriteElementString(name, Namespace, XmlConvert.ToString(value.Value));
            }
        }

        public static decimal? ReadDecimal(XElement element) => IsNil(element) ? (decimal?)null : XmlConvert.ToDecimal(element.Value);

        public static void WriteBool(XmlWriter writer, string name, bool? value)
        {
            if (value.HasValue)
            {
                writer.WriteElementString(name, Namespace, XmlConvert.ToString(value.Value));
            }
        }

        public static bool? ReadBool(XElement element) => IsNil(element) ? (bool?)null : XmlConvert.ToBoolean(element.Value);

        public static void WriteEnum<T>(XmlWriter writer, string name, T value) where T : struct, Enum =>
            writer.WriteElementString(name, Namespace, value.ToString());

        public static T ReadEnum<T>(XElement element) where T : struct =>
            Enum.TryParse(element.Value, out T value) ? value : throw new SerializationException($"Invalid {typeof(T).Name} value '{element.Value}'.");

        static int FindMember(XmlMember[] members, int start, string name)
        {
            for (var i = start; i < members.Length; i++)
            {
                if (members[i].Name == name)
                {
                    return i;
                }
            }

            return -1;
        }

        static void WriteInstanceType(XmlWriter writer, string instanceType)
        {
            if (instanceType != null)
            {
                writer.WriteAttributeString("type", InstanceNamespace, instanceType);
            }
        }

        static void WriteUnknownElements(XmlWriter writer, List<(int Position, XElement Element)> unknownElements, int position)
        {
            if (unknownElements == null)
            {
                return;
            }

            foreach (var unknown in unknownElements)
            {
                if (unknown.Position == position)
                {
                    unknown.Element.WriteTo(writer);
                }
            }
        }
    }
}
