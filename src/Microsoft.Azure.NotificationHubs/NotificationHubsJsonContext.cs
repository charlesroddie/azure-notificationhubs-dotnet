//------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for
// license information.
//------------------------------------------------------------

using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Microsoft.Azure.NotificationHubs
{
    [JsonSerializable(typeof(Installation))]
    [JsonSerializable(typeof(PartialUpdateOperation[]), TypeInfoPropertyName = "PartialUpdateOperationArray")]
    [JsonSerializable(typeof(IList<PartialUpdateOperation>), TypeInfoPropertyName = "PartialUpdateOperationList")]
    [JsonSerializable(typeof(IList<string>), TypeInfoPropertyName = "StringList")]
    [JsonSerializable(typeof(IDictionary<string, string>), TypeInfoPropertyName = "StringDictionary")]
    [JsonSerializable(typeof(Dictionary<string, string>), TypeInfoPropertyName = "StringDictionaryConcrete")]
    [JsonSerializable(typeof(BrowserPushSubscription))]
    internal partial class NotificationHubsJsonContext : JsonSerializerContext
    {
        // Relaxed escaping keeps output identical to the former Newtonsoft.Json serialization
        internal static NotificationHubsJsonContext Instance { get; } = new NotificationHubsJsonContext(new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            PropertyNameCaseInsensitive = true,
        });
    }
}
