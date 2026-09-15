//------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved. 
// Licensed under the MIT License. See License.txt in the project root for 
// license information.
//------------------------------------------------------------

using System.Runtime.Serialization;
using Microsoft.Azure.NotificationHubs.Messaging;

namespace Microsoft.Azure.NotificationHubs
{
    /// <summary>
    /// Represents the result of the registration.
    /// </summary>
    [DataContract(Name = ManagementStrings.RegistrationResult, Namespace = ManagementStrings.Namespace)]
    public sealed class RegistrationResult
    {
        /// <summary>
        /// Gets or sets the application platform.
        /// </summary>
        /// 
        /// <returns>
        /// The application platform.
        /// </returns>
        [DataMember(Name = ManagementStrings.ApplicationPlatform, IsRequired = true, Order = 1001, EmitDefaultValue = true)]
        public string ApplicationPlatform
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the PNS handle.
        /// </summary>
        /// 
        /// <returns>
        /// The PNS handle.
        /// </returns>
        [DataMember(Name = ManagementStrings.PnsHandle, IsRequired = true, Order = 1002, EmitDefaultValue = true)]
        public string PnsHandle
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the registration ID.
        /// </summary>
        /// 
        /// <returns>
        /// The registration ID.
        /// </returns>
        [DataMember(Name = ManagementStrings.RegistrationId, IsRequired = true, Order = 1003, EmitDefaultValue = true)]
        public string RegistrationId
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the outcome of the registration.
        /// </summary>
        /// 
        /// <returns>
        /// The outcome of the registration.
        /// </returns>
        [DataMember(Name = ManagementStrings.Outcome, Order = 1004, EmitDefaultValue = true)]
        public string Outcome
        {
            get;
            set;
        }

        internal static readonly XmlMember[] XmlMembers =
        {
            XmlMember.Create<RegistrationResult>(ManagementStrings.ApplicationPlatform, (w, n, o) => XmlContract.WriteString(w, n, o.ApplicationPlatform, true), (o, e) => o.ApplicationPlatform = XmlContract.ReadString(e)),
            XmlMember.Create<RegistrationResult>(ManagementStrings.PnsHandle, (w, n, o) => XmlContract.WriteString(w, n, o.PnsHandle, true), (o, e) => o.PnsHandle = XmlContract.ReadString(e)),
            XmlMember.Create<RegistrationResult>(ManagementStrings.RegistrationId, (w, n, o) => XmlContract.WriteString(w, n, o.RegistrationId, true), (o, e) => o.RegistrationId = XmlContract.ReadString(e)),
            XmlMember.Create<RegistrationResult>(ManagementStrings.Outcome, (w, n, o) => XmlContract.WriteString(w, n, o.Outcome, true), (o, e) => o.Outcome = XmlContract.ReadString(e)),
        };
    }
}