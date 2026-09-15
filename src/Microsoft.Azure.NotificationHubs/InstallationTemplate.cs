//------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for
// license information.
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Microsoft.Azure.NotificationHubs
{
    /// <summary>
    /// Represents template which may belong to an instance of <see cref="T:Microsoft.Azure.NotificationHubs.Installation"/> class
    /// </summary>
    public class InstallationTemplate
    {
        /// <summary>
        /// Gets or sets a template body for notification payload which may contain placeholders to be filled in with actual data during the send operation
        /// </summary>
        [JsonPropertyName("body")]
        public string Body { get; set; }

        /// <summary>
        /// Gets or set collection of headers applicable for MPNS-targeted notifications
        /// </summary>
        [JsonPropertyName("headers")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, string> Headers { get; set; }

        /// <summary>
        /// Gets or sets expiry applicable for APNS-targeted notifications
        /// </summary>
        [JsonPropertyName("expiry")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Expiry { get; set; }

        /// <summary>
        /// Gets or sets collection of tags for particular template. Ususaly only one tag (template name) should be here.
        /// </summary>
        [JsonPropertyName("tags")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [Obsolete]
        public IList<string> Tags { get; set; }
    }
}
