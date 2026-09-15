//------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for
// license information.
//------------------------------------------------------------

using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml;
using Microsoft.Azure.NotificationHubs.Messaging;

namespace Microsoft.Azure.NotificationHubs
{
    /// <summary>
    /// Represents a notification outcome.
    /// </summary>
    [DataContract(Name = ManagementStrings.NotificationOutcome, Namespace = ManagementStrings.Namespace)]
    public sealed class NotificationOutcome
    {
        internal bool DelayedRetry { get; set; }

        internal bool ReleaseSession { get; set; }

        /// <summary>
        /// Gets the state of this notification outcome.
        /// </summary>
        ///
        /// <returns>
        /// The state of this notification outcome.
        /// </returns>
        public NotificationOutcomeState State { get; internal set; }

        /// <summary>
        /// Gets or sets the number of devices that successfully received the notification.
        /// </summary>
        ///
        /// <returns>
        /// The number of devices that successfully received the notification.
        /// </returns>
        /// <remarks>
        /// The property contains value only when <see cref="NotificationHubClient.EnableTestSend"/> property is turned on for troubleshooting purposes.
        /// </remarks>
        /// <seealso cref="NotificationHubClient.EnableTestSend"/>
        [DataMember(Name = ManagementStrings.Success, IsRequired = true, Order = 1001, EmitDefaultValue = true)]
        public long Success { get; set; }

        /// <summary>
        /// Gets or sets the number of devices that failed to receive a notification.
        /// </summary>
        ///
        /// <returns>
        /// The number of devices that failed to receive a notification.
        /// </returns>
        /// <remarks>
        /// The property contains value only when <see cref="NotificationHubClient.EnableTestSend"/> property is turned on for troubleshooting purposes.
        /// </remarks>
        /// <seealso cref="NotificationHubClient.EnableTestSend"/>
        [DataMember(Name = ManagementStrings.Failure, IsRequired = true, Order = 1002, EmitDefaultValue = true)]
        public long Failure { get; set; }

        /// <summary>
        /// Gets or sets the list of notification outcome results for each device registered with the hub, to which this notification was sent.
        /// </summary>
        ///
        /// <returns>
        /// The list of notification outcome results for each device registered with the hub, to which this notification was sent.
        /// </returns>
        /// <remarks>
        /// The property contains value only when <see cref="NotificationHubClient.EnableTestSend"/> property is turned on for troubleshooting purposes.
        /// </remarks>
        /// <seealso cref="NotificationHubClient.EnableTestSend"/>
        [DataMember(Name = ManagementStrings.Results, IsRequired = true, Order = 1003, EmitDefaultValue = true)]
        public List<RegistrationResult> Results { get; set; }

        /// <summary>
        /// Gets or sets the notification ID.
        /// </summary>
        ///
        /// <returns>
        /// Notification ID.
        /// </returns>
        /// <remarks>
        /// The property contains value only when using Standard tier Notification Hubs.
        /// </remarks>
        public string NotificationId { get; set; }

        /// <summary>
        /// Gets the tracking ID.
        /// </summary>
        ///
        /// <returns>
        /// The tracking ID.
        /// </returns>
        public string TrackingId { get; internal set; }

        static readonly XmlMember[] XmlMembers =
        {
            XmlMember.Create<NotificationOutcome>(ManagementStrings.Success, (w, n, o) => XmlContract.WriteLong(w, n, o.Success), (o, e) => o.Success = XmlContract.ReadLong(e) ?? 0),
            XmlMember.Create<NotificationOutcome>(ManagementStrings.Failure, (w, n, o) => XmlContract.WriteLong(w, n, o.Failure), (o, e) => o.Failure = XmlContract.ReadLong(e) ?? 0),
            XmlMember.Create<NotificationOutcome>(ManagementStrings.Results,
                (w, n, o) => XmlContract.WriteList(w, n, o.Results, ManagementStrings.RegistrationResult, (xw, itemName, result) => XmlContract.WriteNested(xw, itemName, result, RegistrationResult.XmlMembers, true), true),
                (o, e) => o.Results = XmlContract.ReadList(e, item => XmlContract.ReadNested(item, new RegistrationResult(), RegistrationResult.XmlMembers))),
        };

        internal static NotificationOutcome FromXml(XmlReader reader)
        {
            var outcome = new NotificationOutcome();
            XmlContract.ReadMembers(XmlContract.ReadElement(reader, ManagementStrings.NotificationOutcome), outcome, XmlMembers);
            return outcome;
        }

        internal static NotificationOutcome GetUnknownOutCome()
        {
            RegistrationResult result = new RegistrationResult()
            {
                ApplicationPlatform = "Unknown",
                PnsHandle = "Unknown",
                RegistrationId = "Unknown",
                Outcome = "UnknownError"
            };

            return new NotificationOutcome()
            {
                Failure = 1,
                Success = 0,
                Results = new List<RegistrationResult>() { result }
            };
        }
    }
}
