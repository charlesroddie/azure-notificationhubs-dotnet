using System.Text.Json.Serialization;

namespace Microsoft.Azure.NotificationHubs
{
    /// <summary>
    /// Browser Web Push subscription.
    /// </summary>
    public class BrowserPushSubscription
    {
        /// <summary>
        /// Gets or sets the push service endpoint URL.
        /// </summary>
        [JsonPropertyName("endpoint")]
        public string Endpoint { get; set; }

        /// <summary>
        /// Gets or sets the P-256 ECDH public key.
        /// </summary>
        [JsonPropertyName("p256dh")]
        public string P256DH { get; set; }

        /// <summary>
        /// Gets or sets the authentication secret.
        /// </summary>
        [JsonPropertyName("auth")]
        public string Auth { get; set; }
    }
}
