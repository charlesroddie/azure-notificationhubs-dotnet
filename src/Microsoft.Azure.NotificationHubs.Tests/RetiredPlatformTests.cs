using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RichardSzalay.MockHttp;
using Xunit;

namespace Microsoft.Azure.NotificationHubs.Tests
{
    public class RetiredPlatformTests
    {
        const string Namespaces = "xmlns=\"http://schemas.microsoft.com/netservices/2010/10/servicebus/connect\" xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\"";

        static string Entry(string content) =>
            $"<entry xmlns:a=\"http://schemas.microsoft.com/ado/2007/08/dataservices/metadata\"><title type=\"text\">id</title><content type=\"application/xml\">{content}</content></entry>";

        [Fact]
        public async Task ListingRegistrationsSkipsRetiredPlatforms()
        {
            var feed = "<feed xmlns=\"http://www.w3.org/2005/Atom\"><title type=\"text\">Registrations</title>"
                + Entry($"<GcmRegistrationDescription {Namespaces}><ETag>1</ETag><ExpirationTime>9999-12-31T23:59:59.999Z</ExpirationTime><RegistrationId>1</RegistrationId><GcmRegistrationId>token</GcmRegistrationId></GcmRegistrationDescription>")
                + Entry($"<MpnsRegistrationDescription {Namespaces}><ETag>1</ETag><ExpirationTime>9999-12-31T23:59:59.999Z</ExpirationTime><RegistrationId>2</RegistrationId><ChannelUri>https://some.url/</ChannelUri></MpnsRegistrationDescription>")
                + Entry($"<AppleRegistrationDescription {Namespaces}><ETag>1</ETag><ExpirationTime>9999-12-31T23:59:59.999Z</ExpirationTime><RegistrationId>3</RegistrationId><DeviceToken>ABCDEF</DeviceToken></AppleRegistrationDescription>")
                + "</feed>";
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When("https://sample.servicebus.windows.net/hub-name/registrations*").Respond("application/atom+xml", feed);
            var client = new NotificationHubClient(
                "Endpoint=sb://sample.servicebus.windows.net/;SharedAccessKeyName=DefaultListenSharedAccessSignature;SharedAccessKey=xxxxxx",
                "hub-name",
                new NotificationHubSettings { HttpClient = mockHttp.ToHttpClient() });

            var registrations = await client.GetAllRegistrationsAsync(100);

            var registration = Assert.IsType<AppleRegistrationDescription>(Assert.Single(registrations));
            Assert.Equal("3", registration.RegistrationId);
        }

        [Theory]
        [InlineData("gcm")]
        [InlineData("mpns")]
        public void InstallationWithRetiredPlatformCanBeRead(string platform)
        {
            var installation = JsonConvert.DeserializeObject<Installation>($"{{\"installationId\":\"id\",\"platform\":\"{platform}\",\"pushChannel\":\"token\"}}");

            Assert.Equal(platform, JsonConvert.SerializeObject(installation.Platform, new Newtonsoft.Json.Converters.StringEnumConverter()).Trim('"'));
        }
    }
}
