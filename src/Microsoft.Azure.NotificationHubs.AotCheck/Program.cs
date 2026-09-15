using System;
using Microsoft.Azure.NotificationHubs;

// Published with Native AOT in CI; exercises XML and JSON serialization and exits non-zero on failure

var registration = new AppleTemplateRegistrationDescription("0123abcd", "{\"aps\":{\"alert\":\"$(message)\"}}", new[] { "tag" }) { TemplateName = "template" };
var roundTripped = (AppleTemplateRegistrationDescription)RegistrationDescription.Deserialize(registration.Serialize());
if (roundTripped.DeviceToken != registration.DeviceToken
    || roundTripped.BodyTemplate.Value != registration.BodyTemplate.Value
    || roundTripped.TemplateName != registration.TemplateName
    || !roundTripped.Tags.Contains("tag"))
{
    Console.Error.WriteLine("Registration XML round trip failed.");
    return 1;
}

var installation = new BrowserInstallation("installation", new BrowserPushSubscription { Endpoint = "https://push", P256DH = "key", Auth = "auth" });
if (installation.PushChannel != "{\"endpoint\":\"https://push\",\"p256dh\":\"key\",\"auth\":\"auth\"}")
{
    Console.Error.WriteLine($"Browser push channel JSON was {installation.PushChannel}.");
    return 1;
}

NotificationHubClient.CreateClientFromConnectionString("Endpoint=sb://sample.servicebus.windows.net/;SharedAccessKeyName=key;SharedAccessKey=a2V5", "hub");

Console.WriteLine("AOT check passed.");
return 0;
