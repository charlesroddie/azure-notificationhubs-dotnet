//------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for
// license information.
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Microsoft.Azure.NotificationHubs
{
    /// <summary>
    /// Represents device in Azure Notification Hub
    /// </summary>
    public class Installation
    {
        /// <summary>
        /// Get or sets unique identifier for the installation
        /// </summary>
        [JsonPropertyName("installationId")]
        public string InstallationId { get; set; }

        /// <summary>
        /// Get or sets unique identifier for the user
        /// </summary>
        [JsonPropertyName("userId")]
        public string UserId { get; set; }

        /// <summary>
        /// Gets or set registration id, token or URI obtained from platform-specific notification service
        /// </summary>
        [JsonPropertyName("pushChannel")]
        public string PushChannel { get; set; }

        /// <summary>
        /// Gets if installation is expired or not
        /// </summary>
        [JsonPropertyName("pushChannelExpired")]
        public bool? PushChannelExpired { get; set; }

        /// <summary>
        /// Gets or sets notification platform for the installation
        /// </summary>
        [JsonPropertyName("platform")]
        public NotificationPlatform Platform { get; set; }

        /// <summary>
        /// Gets or sets expiration for the installation
        /// </summary>
        [JsonPropertyName("expirationTime")]
        public DateTime? ExpirationTime { get; set; }

        /// <summary>
        /// Gets or sets collection of tags
        /// </summary>
        [JsonPropertyName("tags")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IList<string> Tags { get; set; }

        /// <summary>
        /// Gets or sets collection of push variables
        /// </summary>
        [JsonPropertyName("pushVariables")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, string> PushVariables { get; set; }

        /// <summary>
        /// Gets or sets collection of templates
        /// </summary>
        [JsonPropertyName("templates")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, InstallationTemplate> Templates { get; set; }

        /// <summary>
        /// Gets or sets collection of secondary tiles for WNS
        /// </summary>
        [JsonPropertyName("secondaryTiles")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [Obsolete]
        public IDictionary<string, WnsSecondaryTile> SecondaryTiles { get; set; }

        internal string ToJson()
        {
            return JsonSerializer.Serialize(this, NotificationHubsJsonContext.Instance.Installation);
        }

        internal static Installation FromJson(string json)
        {
            return JsonSerializer.Deserialize(json, NotificationHubsJsonContext.Instance.Installation);
        }
    }
}
