//------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for
// license information.
//------------------------------------------------------------

using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Microsoft.Azure.NotificationHubs
{
    /// <summary>
    /// Patch an installation with this object
    /// </summary>
    public class PartialUpdateOperation
    {
        /// <summary>
        /// Get or set the update operation type
        /// </summary>
        ///
        /// <returns>
        /// Update operation type.
        /// </returns>
        [JsonPropertyName("op")]
        public UpdateOperationType Operation { get; set; }

        /// <summary>
        /// Get or set the path to the json which is to be updated
        /// </summary>
        ///
        /// <returns>
        /// Path to the installation json
        /// </returns>
        [JsonPropertyName("path")]
        public string Path { get; set; }

        /// <summary>
        /// Get or set the Value to the updated with
        /// </summary>
        ///
        /// <returns>
        /// Value
        /// </returns>
        [JsonPropertyName("value")]
        public string Value { get; set; }

        internal static IList<PartialUpdateOperation> ListFromJson(string json)
        {
            return JsonSerializer.Deserialize(json, NotificationHubsJsonContext.Instance.PartialUpdateOperationArray);
        }
    }
}
