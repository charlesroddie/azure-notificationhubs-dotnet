//------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved. 
// Licensed under the MIT License. See License.txt in the project root for 
// license information.
//------------------------------------------------------------

using System.Runtime.Serialization;

namespace Microsoft.Azure.NotificationHubs
{
    /// <summary>
    /// Supported Intallation Platforms
    /// </summary>
    public enum NotificationPlatform
    {
        /// <summary>
        /// WNS Installation Platform
        /// </summary>
        [EnumMember(Value = "wns")]
        Wns=1,

        /// <summary>
        /// APNS Installation Platform
        /// </summary>
        [EnumMember(Value = "apns")]
        Apns=2,

        /// <summary>
        /// ADM Installation Platform
        /// </summary>
        [EnumMember(Value = "adm")]
        Adm=5,

        /// <summary>
        /// Baidu Installation Platform
        /// </summary>
        [EnumMember(Value = "baidu")]
        Baidu=6,

        /// <summary>
        /// Browser Installation Platform
        /// </summary>
        [EnumMember(Value = "browser")]
        Browser=8,

        /// <summary>
        /// FCM V1 Installation Platform
        /// </summary>
        [EnumMember(Value = "fcmV1")]
        FcmV1=9,
    }
}
