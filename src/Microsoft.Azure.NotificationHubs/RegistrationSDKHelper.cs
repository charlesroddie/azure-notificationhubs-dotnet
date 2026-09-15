//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for
// license information.
//----------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Microsoft.Azure.NotificationHubs
{
    internal static class RegistrationSDKHelper
    {
        internal const int TemplateMaxLength = 200;

        internal static void ValidateRegistration(RegistrationDescription registration)
        {
            var windowsTemplateRegistration = registration as WindowsTemplateRegistrationDescription;
            if (windowsTemplateRegistration != null)
            {
                windowsTemplateRegistration.SetWnsType();
            }

            // validate
            registration.Validate();
        }

        /// <summary>
        /// Find type from xml string, and it should set to WnsHeaders["X-WNS-Type"];
        /// If the header already there, this function won't overwrite.
        /// </summary>
        private static void SetWnsType(this WindowsTemplateRegistrationDescription registration)
        {
            if (registration == null || registration.IsJsonObjectPayLoad())
            {
                return;
            }

            if (registration.IsXmlPayLoad())
            {
                if (registration.WnsHeaders == null)
                {
                    registration.WnsHeaders = new WnsHeaderCollection();
                }

                if (registration.WnsHeaders.ContainsKey(WindowsRegistrationDescription.Type) &&
                registration.WnsHeaders[WindowsRegistrationDescription.Type].Equals(WindowsRegistrationDescription.Raw, StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        XmlDocument xmlPayload = new XmlDocument();
                        using (var reader = XmlTextReader.Create(new StringReader(registration.BodyTemplate)))
                        {
                            xmlPayload.Load(reader);
                        }
                    }
                    catch (XmlException)
                    {
                        throw new ArgumentException(SRClient.NotSupportedXMLFormatAsBodyTemplate);
                    }
                }
                else
                {
                    switch (DetectWindowsTemplateRegistationType(registration.BodyTemplate, SRClient.NotSupportedXMLFormatAsBodyTemplate))
                    {
                        case WindowsTemplateBodyType.Toast:
                            AddOrUpdateHeader(registration.WnsHeaders, WindowsRegistrationDescription.Type, WindowsRegistrationDescription.Toast);
                            break;
                        case WindowsTemplateBodyType.Tile:
                            AddOrUpdateHeader(registration.WnsHeaders, WindowsRegistrationDescription.Type, WindowsRegistrationDescription.Tile);
                            break;
                        case WindowsTemplateBodyType.Badge:
                            AddOrUpdateHeader(registration.WnsHeaders, WindowsRegistrationDescription.Type, WindowsRegistrationDescription.Badge);
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private static void AddOrUpdateHeader(SortedDictionary<string, string> headers, string key, string value)
        {
            if (!headers.ContainsKey(key))
            {
                headers.Add(key, value);
            }
            else
            {
                headers[key] = value;
            }
        }

        public static WindowsTemplateBodyType DetectWindowsTemplateRegistationType(string body, string errorMsg)
        {
            XmlDocument xmlPayload = new XmlDocument();

            using (var reader = XmlTextReader.Create(new StringReader(body)))
            {
                try
                {
                    xmlPayload.Load(reader);
                }
                catch (XmlException)
                {
                    throw new ArgumentException(errorMsg);
                }

                XmlNode node = xmlPayload.FirstChild;
                while (node != null && node.NodeType != XmlNodeType.Element)
                {
                    node = node.NextSibling;
                }

                if (node == null)
                {
                    throw new ArgumentException(errorMsg);
                }

                WindowsTemplateBodyType registrationType;
                if (node == null || !Enum.TryParse(node.Name, true, out registrationType))
                {
                    throw new ArgumentException(errorMsg);
                }

                return registrationType;
            }
        }

        public static string AddDeclarationToXml(string content)
        {
            XmlDocument xmlPayload = new XmlDocument();

            using (var reader = XmlTextReader.Create(new StringReader(content)))
            {
                xmlPayload.Load(reader);
                if (xmlPayload.FirstChild.NodeType != XmlNodeType.XmlDeclaration)
                {
                    XmlNode declarationNode = xmlPayload.CreateXmlDeclaration("1.0", "utf-16", null);
                    XmlNode root = xmlPayload.DocumentElement;
                    xmlPayload.InsertBefore(declarationNode, root);
                }

                return xmlPayload.InnerXml;
            }
        }
    }

    internal enum WindowsTemplateBodyType
    {
        Toast,
        Tile,
        Badge,
        Raw
    }
}
