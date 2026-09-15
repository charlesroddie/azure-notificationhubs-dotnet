using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Azure.NotificationHubs.Messaging;
using Xunit;
using Xunit.Sdk;

namespace Microsoft.Azure.NotificationHubs.Tests
{
    // DataContractSerializer is the reference for the hand-written XML serialization
    public class XmlSerializationTests
    {
        static readonly XNamespace InstanceNamespace = "http://www.w3.org/2001/XMLSchema-instance";
        static readonly DateTime Time = new DateTime(2030, 1, 2, 3, 4, 5, 678, DateTimeKind.Utc);
        const string Special = "a<b>&\"'é";

        [Fact]
        public void WritingMatchesDataContractSerializer()
        {
            foreach (var (name, entity) in EntitySamples())
            {
                AssertSameXml(name, DcsWrite(entity, DeclaredType(entity)), new EntityDescriptionSerializer().Serialize(entity));
            }
        }

        [Fact]
        public void ReadingSamplesMatchesDataContractSerializer()
        {
            foreach (var (name, entity) in EntitySamples())
            {
                AssertRoundTripMatches(name, DcsWrite(entity, DeclaredType(entity)));
            }
        }

        [Fact]
        public void ReadingRecordedResponsesMatchesDataContractSerializer()
        {
            var recorded = RecordedEntityXml().ToList();
            Assert.NotEmpty(recorded);
            foreach (var (name, xml) in recorded)
            {
                AssertRoundTripMatches(name, xml);
            }
        }

        [Fact]
        public void UnknownElementsArePreservedLikeDataContractSerializer()
        {
            foreach (var (name, xml) in RecordedEntityXml())
            {
                var root = XElement.Parse(xml);
                var unknown = new XElement(XName.Get("Unknown", ManagementStrings.Namespace), new XElement(XName.Get("Nested", ManagementStrings.Namespace), "value"));
                var children = root.Elements().ToList();
                root.AddFirst(new XElement(unknown));
                if (children.Count > 1)
                {
                    children[1].AddAfterSelf(new XElement(unknown));
                }
                root.Add(new XElement(unknown));

                AssertRoundTripMatches($"{name} with unknown elements", root.ToString(SaveOptions.DisableFormatting));
            }
        }

        [Fact]
        public void RegistrationSerializeDeserializeMatchesDataContractSerializer()
        {
            var registration = new AppleTemplateRegistrationDescription("0123abcd", "{\"aps\":{\"alert\":\"$(message)\"}}", new[] { "tag" });
            var xml = registration.Serialize();

            AssertSameXml("Serialize", DcsWrite(registration, typeof(RegistrationDescription)), xml);
            AssertSameXml("Deserialize", xml, DcsWrite(RegistrationDescription.Deserialize(xml), typeof(RegistrationDescription)));
        }

        [Fact]
        public void ReadingNotificationOutcomeMatchesDataContractSerializer()
        {
            var samples = new[]
            {
                new NotificationOutcome
                {
                    Success = 2,
                    Failure = 1,
                    Results = new List<RegistrationResult>
                    {
                        new RegistrationResult { ApplicationPlatform = "apple", PnsHandle = Special, RegistrationId = "r", Outcome = "Succeeded" },
                        new RegistrationResult(),
                    }
                },
                new NotificationOutcome(),
            };

            foreach (var sample in samples)
            {
                var xml = DcsWrite(sample, typeof(NotificationOutcome));
                using (var reader = XmlReader.Create(new StringReader(xml)))
                {
                    AssertSameXml(nameof(NotificationOutcome), xml, DcsWrite(NotificationOutcome.FromXml(reader), typeof(NotificationOutcome)));
                }
            }
        }

        [Fact]
        public void ReadingNotificationDetailsMatchesDataContractSerializer()
        {
            var samples = new[]
            {
                new NotificationDetails
                {
                    NotificationId = "n",
                    Location = new Uri("https://ns.servicebus.windows.net/hub/messages/n?api-version=2017-04"),
                    State = NotificationOutcomeState.Completed,
                    EnqueueTime = Time,
                    StartTime = Time,
                    EndTime = Time,
                    NotificationBody = Special,
                    Tags = "a,b",
                    TargetPlatforms = "apple,fcmv1",
                    ApnsOutcomeCounts = new NotificationOutcomeCollection { { "Success", 3 }, { "InvalidToken", 1 } },
                    WnsOutcomeCounts = new NotificationOutcomeCollection { { "Success", 1 } },
                    AdmOutcomeCounts = new NotificationOutcomeCollection(),
                    FcmV1OutcomeCounts = new NotificationOutcomeCollection { { "Success", 5 } },
                    PnsErrorDetailsUri = "https://errors",
                },
                new NotificationDetails(),
            };

            foreach (var sample in samples)
            {
                var xml = DcsWrite(sample, typeof(NotificationDetails));
                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml)))
                {
                    AssertSameXml(nameof(NotificationDetails), xml, DcsWrite(NotificationDetails.FromXml(stream), typeof(NotificationDetails)));
                }
            }
        }

        static IEnumerable<(string Name, EntityDescription Entity)> EntitySamples()
        {
            yield return ("Apple", new AppleRegistrationDescription("0123abcd", new[] { "t1", "t2" }) { RegistrationId = "r1", ETag = "3", ExpirationTime = Time, PushVariables = new Dictionary<string, string> { { "v", Special } } });
            yield return ("AppleMinimal", new AppleRegistrationDescription("0123abcd"));

            var appleTemplate = new AppleTemplateRegistrationDescription("0123abcd", "{\"aps\":{\"alert\":\"$(m)\"}}", new[] { "t" }) { Expiry = "2030-01-01", TemplateName = Special, Priority = "10" };
            appleTemplate.ApnsHeaders.Add("apns-priority", "10");
            appleTemplate.ApnsHeaders.Add("apns-expiration", "0");
            yield return ("AppleTemplate", appleTemplate);
            yield return ("AppleTemplateNullBody", new AppleTemplateRegistrationDescription("0123abcd"));
            yield return ("AppleTemplateEmptyBody", new AppleTemplateRegistrationDescription("0123abcd") { BodyTemplate = new CDataMember(string.Empty), ApnsHeaders = null });

            yield return ("Windows", new WindowsRegistrationDescription(new Uri("https://db3.notify.windows.com/?token=a%2Bb"), new[] { "t" }) { SecondaryTileName = "tile" });
            yield return ("WindowsTemplate", new WindowsTemplateRegistrationDescription("https://db3.notify.windows.com/?token=x", "<toast><visual>$(m) ]]> end</visual></toast>", new Dictionary<string, string> { { "X-WNS-Type", "wns/toast" } }, new[] { "t" }) { TemplateName = "n" });
            yield return ("WindowsTemplateNoHeaders", new WindowsTemplateRegistrationDescription(new Uri("https://db3.notify.windows.com/?token=y")));

            yield return ("Adm", new AdmRegistrationDescription("adm-id", new[] { "t" }));
            yield return ("AdmTemplate", new AdmTemplateRegistrationDescription("adm-id", "{\"data\":{}}", new[] { "t" }) { TemplateName = "n" });

            yield return ("Baidu", new BaiduRegistrationDescription("user", "channel", new[] { "t" }));
            yield return ("BaiduTemplate", new BaiduTemplateRegistrationDescription("user", "channel", "{}", new[] { "t" }, 1) { TemplateName = "n" });
            yield return ("BaiduTemplateNoMessageType", new BaiduTemplateRegistrationDescription("user", "channel", "{}"));

            yield return ("FcmV1", new FcmV1RegistrationDescription("fcm", new[] { "t" }));
            yield return ("FcmV1Template", new FcmV1TemplateRegistrationDescription("fcm", "{}", new[] { "t" }) { TemplateName = "n" });

            var hub = new NotificationHubDescription("hub")
            {
                RegistrationTtl = TimeSpan.FromDays(90),
                UserMetadata = Special,
                IsDisabled = true,
                DailyOperations = 5,
                DailyMaxActiveDevices = 6,
                DailyMaxActiveRegistrations = 7,
                ApnsCredential = new ApnsCredential { BlockedOn = Time },
                WnsCredential = new WnsCredential(),
                AdmCredential = new AdmCredential(),
                BaiduCredential = new BaiduCredential(),
                FcmV1Credential = new FcmV1Credential(),
                BrowserCredential = new BrowserCredential(),
            };
            hub.ApnsCredential.Properties["Endpoint"] = "https://api.push.apple.com:443/3/device";
            hub.ApnsCredential.Properties["Token"] = Special;
            hub.FcmV1Credential.Properties["ProjectId"] = "p";
            hub.SetAccessPasswords("full", SharedAccessAuthorizationRule.GenerateRandomKey(), "listen", SharedAccessAuthorizationRule.GenerateRandomKey());
            yield return ("Hub", hub);
            yield return ("HubMinimal", new NotificationHubDescription("hub"));

            yield return ("Job", new NotificationHubJob
            {
                JobId = "j",
                Progress = 12.5m,
                JobType = NotificationHubJobType.ImportUpsertRegistrations,
                Status = NotificationHubJobStatus.Running,
                OutputContainerUri = new Uri("https://a.blob.core.windows.net/c?sv=1&sig=a%2B"),
                ImportFileUri = new Uri("https://a.blob.core.windows.net/c/f.txt"),
                InputProperties = new Dictionary<string, string> { { "k", Special } },
                Failure = "f",
                OutputProperties = new Dictionary<string, string> { { "OutputFilePath", "p" } },
                CreatedAt = Time,
                UpdatedAt = Time,
            });
            yield return ("JobMinimal", new NotificationHubJob { JobType = NotificationHubJobType.ExportRegistrations, OutputContainerUri = new Uri("https://a") });
        }

        static IEnumerable<(string Name, string Xml)> RecordedEntityXml()
        {
            XNamespace atom = "http://www.w3.org/2005/Atom";
            var serializer = new EntityDescriptionSerializer();
            foreach (var file in Directory.GetFiles("MockData", "*.http").OrderBy(f => f))
            {
                var session = JsonSerializer.Deserialize<TestServerSession>(File.ReadAllText(file), new JsonSerializerOptions { AllowTrailingCommas = true });
                foreach (var call in session.HttpCalls)
                {
                    if (call.Content == null || !call.Content.StartsWith("<"))
                    {
                        continue;
                    }

                    XDocument document;
                    try
                    {
                        document = XDocument.Parse(call.Content);
                    }
                    catch (XmlException)
                    {
                        continue;
                    }

                    foreach (var entity in document.Descendants(atom + "content").Select(c => c.Elements().FirstOrDefault()).Where(e => e != null && serializer.CanDeserialize(e.Name.LocalName)))
                    {
                        yield return ($"{Path.GetFileNameWithoutExtension(file)}: {entity.Name.LocalName}", entity.ToString(SaveOptions.DisableFormatting));
                    }
                }
            }
        }

        static void AssertRoundTripMatches(string name, string xml)
        {
            var rootName = XElement.Parse(xml).Name.LocalName;
            var rootType = typeof(EntityDescription).Assembly.GetType("Microsoft.Azure.NotificationHubs." + rootName, throwOnError: true);
            var viaDataContract = (EntityDescription)DcsRead(xml, rootType);

            EntityDescription viaXmlContract;
            using (var reader = XmlReader.Create(new StringReader(xml)))
            {
                reader.MoveToContent();
                viaXmlContract = new EntityDescriptionSerializer().Deserialize(reader, reader.LocalName);
            }

            AssertSameXml(name, DcsWrite(viaDataContract, DeclaredType(viaDataContract)), new EntityDescriptionSerializer().Serialize(viaXmlContract));
        }

        static Type DeclaredType(EntityDescription entity) => entity is RegistrationDescription ? typeof(RegistrationDescription) : entity.GetType();

        static string DcsWrite(object value, Type declaredType)
        {
            var builder = new StringBuilder();
            using (var writer = XmlWriter.Create(builder, new XmlWriterSettings { OmitXmlDeclaration = true }))
            {
                new DataContractSerializer(declaredType).WriteObject(writer, value);
            }

            return builder.ToString();
        }

        static object DcsRead(string xml, Type declaredType)
        {
            using (var reader = XmlReader.Create(new StringReader(xml)))
            {
                return new DataContractSerializer(declaredType).ReadObject(reader);
            }
        }

        static void AssertSameXml(string name, string expected, string actual)
        {
            var expectedCanonical = Canonical(expected);
            var actualCanonical = Canonical(actual);
            if (expectedCanonical != actualCanonical)
            {
                throw new XunitException($"{name}{Environment.NewLine}expected: {expectedCanonical}{Environment.NewLine}actual:   {actualCanonical}");
            }
        }

        // Ignores namespace prefixes, attribute order and CDATA versus escaped text
        static string Canonical(string xml) => Normalize(XElement.Parse(xml)).ToString(SaveOptions.DisableFormatting);

        static XElement Normalize(XElement element)
        {
            var attributes = element.Attributes()
                .Where(a => !a.IsNamespaceDeclaration)
                .Select(a => new XAttribute(a.Name, a.Name == InstanceNamespace + "type" ? ResolveQualifiedName(element, a.Value) : a.Value))
                .OrderBy(a => a.Name.ToString());
            var text = string.Concat(element.Nodes().OfType<XText>().Select(t => t.Value));
            var normalized = new XElement(element.Name, attributes, element.Elements().Select(Normalize));
            if (text.Length > 0)
            {
                normalized.Add(new XText(text));
            }

            return normalized;
        }

        static string ResolveQualifiedName(XElement element, string value)
        {
            var separator = value.IndexOf(':');
            var ns = separator < 0 ? element.GetDefaultNamespace() : element.GetNamespaceOfPrefix(value.Substring(0, separator));
            return "{" + ns + "}" + value.Substring(separator + 1);
        }
    }
}
