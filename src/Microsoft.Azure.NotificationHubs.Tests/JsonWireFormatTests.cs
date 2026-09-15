using System;
using System.Collections.Generic;
using Xunit;

namespace Microsoft.Azure.NotificationHubs.Tests
{
    // Expected strings were captured from the Newtonsoft.Json implementation
    public class JsonWireFormatTests
    {
        [Fact]
        public void FullInstallation()
        {
#pragma warning disable CS0612, CS0618
            var installation = new AppleInstallation("inst-1", "token-1")
            {
                UserId = "user-1",
                ExpirationTime = new DateTime(2030, 1, 2, 3, 4, 5, DateTimeKind.Utc),
                PushChannelExpired = false,
                Tags = new List<string> { "a", "b" },
                PushVariables = new Dictionary<string, string> { { "v1", "héllo <b>&'\"" } },
                Templates = new Dictionary<string, InstallationTemplate>
                {
                    { "t1", new InstallationTemplate { Body = "{\"aps\":{\"alert\":\"$(message)\"}}", Headers = new Dictionary<string, string> { { "apns-priority", "10" } }, Expiry = "2030-01-01", Tags = new List<string> { "tt" } } }
                },
                SecondaryTiles = new Dictionary<string, WnsSecondaryTile>
                {
                    { "tile", new WnsSecondaryTile { PushChannel = "https://tile", Tags = new List<string> { "x" } } }
                }
            };
#pragma warning restore CS0612, CS0618
            const string expected = """{"installationId":"inst-1","userId":"user-1","pushChannel":"token-1","pushChannelExpired":false,"platform":"apns","expirationTime":"2030-01-02T03:04:05Z","tags":["a","b"],"pushVariables":{"v1":"héllo <b>&'\""},"templates":{"t1":{"body":"{\"aps\":{\"alert\":\"$(message)\"}}","headers":{"apns-priority":"10"},"expiry":"2030-01-01","tags":["tt"]}},"secondaryTiles":{"tile":{"pushChannel":"https://tile","pushChannelExpired":null,"tags":["x"]}}}""";

            Assert.Equal(expected, installation.ToJson());
            Assert.Equal(expected, Installation.FromJson(expected).ToJson());
        }

        [Fact]
        public void MinimalInstallation()
        {
            Assert.Equal(
                """{"installationId":"inst-2","userId":null,"pushChannel":"token-2","pushChannelExpired":null,"platform":"fcmV1","expirationTime":null}""",
                new FcmV1Installation("inst-2", "token-2").ToJson());
        }

        [Fact]
        public void PartialUpdateOperations()
        {
            var operations = new List<PartialUpdateOperation>
            {
                new PartialUpdateOperation { Operation = UpdateOperationType.Add, Path = "/tags", Value = "t" },
                new PartialUpdateOperation { Operation = UpdateOperationType.Remove, Path = "/tags/t" },
                new PartialUpdateOperation { Operation = UpdateOperationType.Replace, Path = "/pushChannel", Value = "new" },
            };
            const string expected = """[{"op":"add","path":"/tags","value":"t"},{"op":"remove","path":"/tags/t","value":null},{"op":"replace","path":"/pushChannel","value":"new"}]""";

            Assert.Equal(expected, operations.ToJson());
            Assert.Equal(expected, PartialUpdateOperation.ListFromJson(expected).ToJson());
        }

        [Fact]
        public void TemplateNotificationBody()
        {
            var notification = new TemplateNotification(new Dictionary<string, string> { { "message", "héllo <b>&'\"" } });
            notification.ValidateAndPopulateHeaders();

            Assert.Equal("""{"message":"héllo <b>&'\""}""", notification.Body);
        }

        [Fact]
        public void RegistrationPushVariables()
        {
            var registration = new AppleRegistrationDescription("token") { PushVariables = new Dictionary<string, string> { { "v1", "héllo" } } };

            Assert.Equal("""{"v1":"héllo"}""", registration.PropertyBagString);
        }

        [Fact]
        public void BrowserPushChannel()
        {
            var installation = new BrowserInstallation("inst-3", new BrowserPushSubscription { Endpoint = "https://e", P256DH = "p", Auth = "a" });

            Assert.Equal("""{"endpoint":"https://e","p256dh":"p","auth":"a"}""", installation.PushChannel);
        }
    }
}
