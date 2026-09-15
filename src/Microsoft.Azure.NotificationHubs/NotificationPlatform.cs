//------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved. 
// Licensed under the MIT License. See License.txt in the project root for 
// license information.
//------------------------------------------------------------

using System;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Microsoft.Azure.NotificationHubs
{
    /// <summary>
    /// Supported Intallation Platforms
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter<NotificationPlatform>))]
    public enum NotificationPlatform
    {
        /// <summary>
        /// WNS Installation Platform
        /// </summary>
        [EnumMember(Value = "wns")]
        [JsonStringEnumMemberName("wns")]
        Wns=1,

        /// <summary>
        /// APNS Installation Platform
        /// </summary>
        [EnumMember(Value = "apns")]
        [JsonStringEnumMemberName("apns")]
        Apns=2,

        /// <summary>
        /// Retired MPNS platform, kept so existing installations can be read
        /// </summary>
        [EnumMember(Value = "mpns")]
        [JsonStringEnumMemberName("mpns")]
        [Obsolete("MPNS is retired.")]
        Mpns=3,

        /// <summary>
        /// Retired GCM/legacy FCM platform, kept so existing installations can be read and migrated to FcmV1
        /// </summary>
        [EnumMember(Value = "gcm")]
        [JsonStringEnumMemberName("gcm")]
        [Obsolete("GCM and legacy FCM are retired. Use FcmV1.")]
        Gcm=4,

        /// <summary>
        /// ADM Installation Platform
        /// </summary>
        [EnumMember(Value = "adm")]
        [JsonStringEnumMemberName("adm")]
        Adm=5,

        /// <summary>
        /// Baidu Installation Platform
        /// </summary>
        [EnumMember(Value = "baidu")]
        [JsonStringEnumMemberName("baidu")]
        Baidu=6,

        /// <summary>
        /// Browser Installation Platform
        /// </summary>
        [EnumMember(Value = "browser")]
        [JsonStringEnumMemberName("browser")]
        Browser=8,

        /// <summary>
        /// FCM V1 Installation Platform
        /// </summary>
        [EnumMember(Value = "fcmV1")]
        [JsonStringEnumMemberName("fcmV1")]
        FcmV1=9,
    }
}
