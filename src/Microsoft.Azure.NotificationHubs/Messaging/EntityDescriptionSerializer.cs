//------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for
// license information.
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

namespace Microsoft.Azure.NotificationHubs.Messaging
{
    internal class EntityDescriptionSerializer
    {
        const string RegistrationDescriptionName = nameof(RegistrationDescription);

        static readonly Dictionary<string, Func<EntityDescription>> Factories = new Dictionary<string, Func<EntityDescription>>
        {
            { nameof(WindowsRegistrationDescription), () => new WindowsRegistrationDescription() },
            { nameof(WindowsTemplateRegistrationDescription), () => new WindowsTemplateRegistrationDescription() },
            { nameof(AppleRegistrationDescription), () => new AppleRegistrationDescription() },
            { nameof(AppleTemplateRegistrationDescription), () => new AppleTemplateRegistrationDescription() },
            { nameof(FcmV1RegistrationDescription), () => new FcmV1RegistrationDescription() },
            { nameof(FcmV1TemplateRegistrationDescription), () => new FcmV1TemplateRegistrationDescription() },
            { nameof(AdmRegistrationDescription), () => new AdmRegistrationDescription() },
            { nameof(AdmTemplateRegistrationDescription), () => new AdmTemplateRegistrationDescription() },
            { nameof(BaiduRegistrationDescription), () => new BaiduRegistrationDescription() },
            { nameof(BaiduTemplateRegistrationDescription), () => new BaiduTemplateRegistrationDescription() },
            { nameof(NotificationHubJob), () => new NotificationHubJob() },
            { nameof(NotificationHubDescription), () => new NotificationHubDescription() },
        };

        public bool CanDeserialize(string typeName) => Factories.ContainsKey(typeName);

        public EntityDescription Deserialize(XmlReader reader, string typeName)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }
            if (string.IsNullOrEmpty(typeName))
            {
                throw new ArgumentNullException(nameof(typeName));
            }

            var element = XmlContract.ReadElement(reader, typeName);
            if (typeName == RegistrationDescriptionName)
            {
                typeName = XmlContract.ReadInstanceType(element) ?? throw new SerializationException($"Element '{RegistrationDescriptionName}' does not specify a registration type.");
            }

            var entity = Factories.TryGetValue(typeName, out var factory) ? factory() : throw new InvalidOperationException($"Unknown entity type {typeName}");
            XmlContract.ReadMembers(element, entity, entity.XmlMembers);
            return entity;
        }

        public string Serialize(EntityDescription description)
        {
            var stringBuilder = new StringBuilder();
            var settings = new XmlWriterSettings
            {
                OmitXmlDeclaration = true
            };

            using (var xmlWriter = XmlWriter.Create(stringBuilder, settings))
            {
                Serialize(description, xmlWriter);
            }

            return stringBuilder.ToString();
        }

        // Registrations are written as their base contract with an instance type, as before
        public void Serialize(EntityDescription description, XmlWriter writer)
        {
            var typeName = description.GetType().Name;
            if (!Factories.ContainsKey(typeName))
            {
                throw new InvalidOperationException($"Unknown entity type {typeName}");
            }

            if (description is RegistrationDescription)
            {
                XmlContract.WriteObject(writer, RegistrationDescriptionName, description, description.XmlMembers, typeName);
            }
            else
            {
                XmlContract.WriteObject(writer, typeName, description, description.XmlMembers);
            }
        }
    }
}
