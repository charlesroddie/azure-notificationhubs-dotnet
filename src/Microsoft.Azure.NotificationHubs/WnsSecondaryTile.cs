//------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for
// license information.
//------------------------------------------------------------

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Microsoft.Azure.NotificationHubs
{
    /// <summary>
    /// Represents the WNS secondary tile
    /// </summary>
    public class WnsSecondaryTile
    {
        /// <summary>
        /// Gets or sets the push channel.
        /// </summary>
        /// <value>
        /// The push channel.
        /// </value>
        [JsonPropertyName("pushChannel")]
        [JsonRequired]
        public string PushChannel { get; set; }

        /// <summary>
        /// Gets or sets the push channel expiration property.
        /// </summary>
        /// <value>
        /// The push channel expiration property.
        /// </value>
        [JsonPropertyName("pushChannelExpired")]
        public bool? PushChannelExpired { get; set; }

        /// <summary>
        /// Gets or sets the tags.
        /// </summary>
        /// <value>
        /// The tags.
        /// </value>
        [JsonPropertyName("tags")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IList<string> Tags { get; set; }

        /// <summary>
        /// Gets or sets the dictionary of Templates.
        /// </summary>
        /// <value>
        /// The Dictionary of templates.
        /// </value>
        [JsonPropertyName("templates")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, InstallationTemplate> Templates { get; set; }
    }
}
