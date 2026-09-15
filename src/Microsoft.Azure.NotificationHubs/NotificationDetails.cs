//------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for
// license information.
//------------------------------------------------------------

using System;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Azure.NotificationHubs.Messaging;

namespace Microsoft.Azure.NotificationHubs
{
    /// <summary>
    /// Represents notification details
    /// </summary>
    [DataContract(Name = ManagementStrings.NotificationDetails, Namespace = ManagementStrings.Namespace)]
    public sealed class NotificationDetails : IExtensibleDataObject
    {
        /// <summary>
        /// Gets or sets the notification identifier.
        /// </summary>
        /// <value>
        /// The notification identifier.
        /// </value>
        [DataMember(Name = ManagementStrings.NotificationId, IsRequired = false, Order = 1000, EmitDefaultValue = false)]
        public string NotificationId { get; set; }

        /// <summary>
        /// Gets or sets the location.
        /// </summary>
        /// <value>
        /// The location URI.
        /// </value>
        [DataMember(Name = ManagementStrings.Location, IsRequired = false, Order = 1002, EmitDefaultValue = false)]
        public Uri Location { get; set; }

        [DataMember(Name = ManagementStrings.State, IsRequired = false, Order = 1003, EmitDefaultValue = true)]
        string NotificationState
        {
            get { return this.State.ToString(); }
            set
            {
                NotificationOutcomeState state;
                this.State = Enum.TryParse(value, true, out state) ? state : NotificationOutcomeState.Unknown;
            }
        }

        /// <summary>
        /// Gets or sets the notification state.
        /// </summary>
        /// <value>
        /// The notification state.
        /// </value>
        public NotificationOutcomeState State { get; set; }

        /// <summary>
        /// Gets or sets the notification enqueue time.
        /// </summary>
        /// <value>
        /// The notification enqueue time.
        /// </value>
        [DataMember(Name = ManagementStrings.EnqueueTime, IsRequired = false, Order = 1004, EmitDefaultValue = false)]
        public DateTime? EnqueueTime { get; set; }

        /// <summary>
        /// Gets or sets the notification start time.
        /// </summary>
        /// <value>
        /// The notification start time.
        /// </value>
        [DataMember(Name = ManagementStrings.StartTime, IsRequired = false, Order = 1005, EmitDefaultValue = false)]
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// Gets or sets the notification end time.
        /// </summary>
        /// <value>
        /// The notification end time.
        /// </value>
        [DataMember(Name = ManagementStrings.EndTime, IsRequired = false, Order = 1006, EmitDefaultValue = false)]
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// Gets or sets the notification body.
        /// </summary>
        /// <value>
        /// The notification body.
        /// </value>
        [DataMember(Name = ManagementStrings.NotificationBody, IsRequired = false, Order = 1007, EmitDefaultValue = false)]
        public string NotificationBody { get; set; }

        /// <summary>
        /// Gets or sets the notification tags.
        /// </summary>
        /// <value>
        /// The notification tags.
        /// </value>
        [DataMember(Name = ManagementStrings.Tags, IsRequired = false, Order = 1008, EmitDefaultValue = false)]
        public string Tags { get; set; }

        /// <summary>
        /// Gets or sets the notification target platforms.
        /// </summary>
        /// <value>
        /// The notification target platforms.
        /// </value>
        [DataMember(Name = ManagementStrings.TargetPlatforms, IsRequired = false, Order = 1009, EmitDefaultValue = false)]
        public string TargetPlatforms { get; set; }

        /// <summary>
        /// Gets or sets the notification apns outcome counts.
        /// </summary>
        /// <value>
        /// The notification apns outcome counts.
        /// </value>
        [DataMember(Name = ManagementStrings.ApnsOutcomeCounts, IsRequired = false, Order = 1010, EmitDefaultValue = false)]
        public NotificationOutcomeCollection ApnsOutcomeCounts { get; set; }

        /// <summary>
        /// Gets or sets the notification WNS outcome counts.
        /// </summary>
        /// <value>
        /// The notification WNS outcome counts.
        /// </value>
        [DataMember(Name = ManagementStrings.WnsOutcomeCounts, IsRequired = false, Order = 1012, EmitDefaultValue = false)]
        public NotificationOutcomeCollection WnsOutcomeCounts { get; set; }

        /// <summary>
        /// Gets or sets the notification FCM V1 outcome counts.
        /// </summary>
        /// <value>
        /// The notification FCM V1 outcome counts.
        /// </value>
        [DataMember(Name = ManagementStrings.FcmV1OutcomeCounts, IsRequired = false, Order = 1017, EmitDefaultValue = false)]
        public NotificationOutcomeCollection FcmV1OutcomeCounts { get; set; }

        /// <summary>
        /// Gets or sets the notification ADM outcome counts.
        /// </summary>
        /// <value>
        /// The notification ADM outcome counts.
        /// </value>
        [DataMember(Name = ManagementStrings.AdmOutcomeCounts, IsRequired = false, Order = 1014, EmitDefaultValue = false)]
        public NotificationOutcomeCollection AdmOutcomeCounts { get; set; }

        /// <summary>
        /// Gets the URI to blob containing errors returned by PNSes.
        /// </summary>
        /// <value>
        /// The blob URI containing error details from PNSes
        /// </value>
        [DataMember(Name = ManagementStrings.PnsErrorDetailsUri, IsRequired = false, Order = 1015, EmitDefaultValue = false)]
        public string PnsErrorDetailsUri { get; set; }

        /// <summary>
        /// Gets or sets the structure that contains extra data. This library does not populate it.
        /// </summary>
        /// <value>
        /// Information describing the extension.
        /// </value>
        public ExtensionDataObject ExtensionData { get; set; }

        static readonly XmlMember[] XmlMembers =
        {
            XmlMember.Create<NotificationDetails>(ManagementStrings.NotificationId, (w, n, o) => XmlContract.WriteString(w, n, o.NotificationId, false), (o, e) => o.NotificationId = XmlContract.ReadString(e)),
            XmlMember.Create<NotificationDetails>(ManagementStrings.Location, (w, n, o) => XmlContract.WriteUri(w, n, o.Location, false), (o, e) => o.Location = XmlContract.ReadUri(e)),
            XmlMember.Create<NotificationDetails>(ManagementStrings.State, (w, n, o) => XmlContract.WriteString(w, n, o.NotificationState, true), (o, e) => o.NotificationState = XmlContract.ReadString(e)),
            XmlMember.Create<NotificationDetails>(ManagementStrings.EnqueueTime, (w, n, o) => XmlContract.WriteDateTime(w, n, o.EnqueueTime), (o, e) => o.EnqueueTime = XmlContract.ReadDateTime(e)),
            XmlMember.Create<NotificationDetails>(ManagementStrings.StartTime, (w, n, o) => XmlContract.WriteDateTime(w, n, o.StartTime), (o, e) => o.StartTime = XmlContract.ReadDateTime(e)),
            XmlMember.Create<NotificationDetails>(ManagementStrings.EndTime, (w, n, o) => XmlContract.WriteDateTime(w, n, o.EndTime), (o, e) => o.EndTime = XmlContract.ReadDateTime(e)),
            XmlMember.Create<NotificationDetails>(ManagementStrings.NotificationBody, (w, n, o) => XmlContract.WriteString(w, n, o.NotificationBody, false), (o, e) => o.NotificationBody = XmlContract.ReadString(e)),
            XmlMember.Create<NotificationDetails>(ManagementStrings.Tags, (w, n, o) => XmlContract.WriteString(w, n, o.Tags, false), (o, e) => o.Tags = XmlContract.ReadString(e)),
            XmlMember.Create<NotificationDetails>(ManagementStrings.TargetPlatforms, (w, n, o) => XmlContract.WriteString(w, n, o.TargetPlatforms, false), (o, e) => o.TargetPlatforms = XmlContract.ReadString(e)),
            XmlMember.Create<NotificationDetails>(ManagementStrings.ApnsOutcomeCounts, (w, n, o) => WriteCounts(w, n, o.ApnsOutcomeCounts), (o, e) => o.ApnsOutcomeCounts = ReadCounts(e)),
            XmlMember.Create<NotificationDetails>(ManagementStrings.WnsOutcomeCounts, (w, n, o) => WriteCounts(w, n, o.WnsOutcomeCounts), (o, e) => o.WnsOutcomeCounts = ReadCounts(e)),
            XmlMember.Create<NotificationDetails>(ManagementStrings.AdmOutcomeCounts, (w, n, o) => WriteCounts(w, n, o.AdmOutcomeCounts), (o, e) => o.AdmOutcomeCounts = ReadCounts(e)),
            XmlMember.Create<NotificationDetails>(ManagementStrings.PnsErrorDetailsUri, (w, n, o) => XmlContract.WriteString(w, n, o.PnsErrorDetailsUri, false), (o, e) => o.PnsErrorDetailsUri = XmlContract.ReadString(e)),
            XmlMember.Create<NotificationDetails>(ManagementStrings.FcmV1OutcomeCounts, (w, n, o) => WriteCounts(w, n, o.FcmV1OutcomeCounts), (o, e) => o.FcmV1OutcomeCounts = ReadCounts(e)),
        };

        static void WriteCounts(XmlWriter writer, string name, NotificationOutcomeCollection counts) =>
            XmlContract.WriteDictionary(writer, name, counts, "Outcome", "Name", "Count", (w, n, count) => XmlContract.WriteLong(w, n, count), false);

        static NotificationOutcomeCollection ReadCounts(XElement element) =>
            XmlContract.ReadDictionary(element, new NotificationOutcomeCollection(), "Name", "Count", count => XmlContract.ReadLong(count) ?? 0);

        internal static NotificationDetails FromXml(Stream stream)
        {
            using (var reader = XmlReader.Create(stream))
            {
                var details = new NotificationDetails();
                XmlContract.ReadMembers(XmlContract.ReadElement(reader, ManagementStrings.NotificationDetails), details, XmlMembers);
                return details;
            }
        }
    }
}
