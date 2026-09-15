//------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved. 
// Licensed under the MIT License. See License.txt in the project root for 
// license information.
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Microsoft.Azure.NotificationHubs.Messaging;
using System.Text.Json;

namespace Microsoft.Azure.NotificationHubs
{
    /// <summary>
    /// Represents a registration description.
    /// </summary>
    [DataContract(Namespace = ManagementStrings.Namespace)]
    [KnownType(typeof(AppleRegistrationDescription))]
    [KnownType(typeof(AppleTemplateRegistrationDescription))]
    [KnownType(typeof(WindowsRegistrationDescription))]
    [KnownType(typeof(WindowsTemplateRegistrationDescription))]
    [KnownType(typeof(AdmRegistrationDescription))]
    [KnownType(typeof(AdmTemplateRegistrationDescription))]
    [KnownType(typeof(BaiduRegistrationDescription))]
    [KnownType(typeof(BaiduTemplateRegistrationDescription))]
    [KnownType(typeof(FcmV1RegistrationDescription))]
    [KnownType(typeof(FcmV1TemplateRegistrationDescription))]
    public abstract class RegistrationDescription : EntityDescription
    {
        internal const string TemplateRegistrationType = "template";
        internal static Regex SingleTagRegex = new Regex(@"^(\$(InstallationId|UserId):\{[-_@#.:=\w]+\}|[-_@#.:\w]+)$", RegexOptions.IgnoreCase);
        internal static Regex UserIdRegex = new Regex(@"\$UserId:\{([-_@#.:=\w]+)\}", RegexOptions.IgnoreCase);
        internal static Regex InstallationIdRegex = new Regex(@"\$InstallationId:\{([-_@#.:=\w]+)\}", RegexOptions.IgnoreCase);

        // SQL omits - (hypen)  during comparison
        // Following values are omitted to make array length 32 - 0, 8 , I, O, X. Because work order count is fully divisble by 32  
        internal static string[] RegistrationRange = new string[] { "_", "1", "2", "3", "4", "5", "6", "7", "9",
                                   "A", "B", "C", "D", "E", "F", "G", "H", "J", "K", "L", "M", "N", "P", "Q", "R", "S", "T", "U", "V", "W", "Y", "ZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZ"};

        /// <summary>
        /// Initializes a new instance of the <see cref="RegistrationDescription"/> class.
        /// </summary>
        /// <param name="registration">The registration.</param>
        public RegistrationDescription(RegistrationDescription registration)
        {
            this.NotificationHubPath = registration.NotificationHubPath;
            this.RegistrationId = registration.RegistrationId;
            this.Tags = registration.Tags;
            this.ETag = registration.ETag;
            this.PropertyBagString = registration.PropertyBagString;
        }

        internal RegistrationDescription(string notificationHubPath)
        {
            this.NotificationHubPath = notificationHubPath;
        }

        internal RegistrationDescription(string notificationHubPath, string registrationId)
        {
            this.NotificationHubPath = notificationHubPath;
            this.RegistrationId = registrationId;
        }

        internal abstract string AppPlatForm
        {
            get;
        }

        internal abstract string RegistrationType
        {
            get;
        }

        internal abstract string PlatformType
        {
            get;
        }

        /// <summary>
        /// Gets the ETag associated with this description.
        /// </summary>
        /// 
        /// <returns>
        /// The ETag associated with this description.
        /// </returns>
        [DataMember(Name = ManagementStrings.ETag, IsRequired = false, Order = 1001, EmitDefaultValue = false)]
        public string ETag { get; internal set; }

        /// <summary>
        /// Gets the expiration time of the registration.
        /// </summary>
        /// 
        /// <returns>
        /// The expiration time of the registration.
        /// </returns>
        [DataMember(Name = ManagementStrings.ExpirationTime, IsRequired = false, Order = 1002, EmitDefaultValue = false)]
        public DateTime? ExpirationTime { get; internal set; }

        /// <summary>
        /// Gets or sets the registration ID.
        /// </summary>
        /// 
        /// <returns>
        /// The registration ID.
        /// </returns>
        [DataMember(Name = ManagementStrings.RegistrationId, Order = 1003, IsRequired = false)]
        public string RegistrationId { get; set; }

        internal string TagsStringLightweight
        {
            get { return GetTagsString(); }
            set
            {
                SetTagsString(value, false);
            }
        }

        [DataMember(Name = ManagementStrings.Tags, IsRequired = false, Order = 1004, EmitDefaultValue = false)]
        internal string TagsString
        {
            get { return GetTagsString(); }
            set
            {
                SetTagsString(value, true);
            }
        }

        [DataMember(Name = ManagementStrings.PushVariables, IsRequired = false, Order = 1005, EmitDefaultValue = false)]
        internal string PropertyBagString
        {
            get
            {
                if (this.PushVariables != null && this.PushVariables.Count > 0)
                {
                    return JsonSerializer.Serialize(this.PushVariables, NotificationHubsJsonContext.Instance.StringDictionary);
                }

                return null;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    return;
                }

                this.PushVariables = JsonSerializer.Deserialize(value, NotificationHubsJsonContext.Instance.StringDictionaryConcrete);
            }
        }

        // Only used in client to help easier manipulate tagsString
        /// <summary>
        /// Gets or sets a set of tags associated with the registration.
        /// </summary>
        /// 
        /// <returns>
        /// A set of tags associated with the registration.
        /// </returns>
        public ISet<string> Tags
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a dictionary of push variables associated with property bag.
        /// </summary>
        /// 
        /// <returns>
        /// Push variables associated with property bag string.
        /// </returns>
        public IDictionary<string, string> PushVariables
        {
            get;
            set;
        }

        internal string FormattedETag
        {
            get
            {
                return string.Format(CultureInfo.InvariantCulture, "W/\"{0}\"", this.ETag);
            }
        }

        internal int DbVersion { get; set; }

        internal string NotificationHubPath { get; set; }

        internal string NotificationHubRuntimeUrl { get; set; }

        internal long NotificationHubId { get; set; }

        internal long DatabaseId { get; set; }

        internal string Namespace { get; set; }

        internal string InstallationVersion { get; set; }

        internal bool ChannelExpired { get; set; }     

        internal abstract string GetPnsHandle();

        internal abstract void SetPnsHandle(string pnsHandle);

        internal abstract RegistrationDescription Clone();

        /// <summary>
        /// Returns platform-specific Pns handle.
        /// </summary>
        /// 
        /// <returns>
        /// Platform-specific Pns handle.
        /// </returns>
        public string PnsHandle => GetPnsHandle();

        /// <summary>
        /// Validates the given tags.
        /// </summary>
        /// 
        /// <returns>
        /// true if the tags are validated; otherwise, false.
        /// </returns>
        /// <param name="tags">The tags to validate.</param>
        public static bool ValidateTags(string tags)
        {
            if (tags == null)
            {
                throw new ArgumentNullException(nameof(tags), "Value cannot be null");
            }

            if (InstallationIdRegex.Matches(tags).Count > 1)
            {
                return false;
            }

            if (UserIdRegex.Matches(tags).Count > 1)
            {
                return false;
            }

            foreach (var tag in tags.Split(','))
            {
                if (!SingleTagRegex.IsMatch(tag))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Returns the total number of tags.
        /// </summary>
        /// 
        /// <returns>
        /// The total number of tags.
        /// </returns>
        /// <param name="tags">The tags.</param>
        public static int TagCount(string tags)
        {
            return tags.Split(',').Length;
        }

        /// <summary>
        /// Serializes the registration description.
        /// </summary>
        /// 
        /// <returns>
        /// The serialized registration description.
        /// </returns>
        public string Serialize()
        {
            this.Validate(false);

            var serializer = new EntityDescriptionSerializer();
            return serializer.Serialize(this);
        }

        /// <summary>
        /// Extracts the registration description from the serialized data.
        /// </summary>
        /// 
        /// <returns>
        /// The registration description.
        /// </returns>
        /// <param name="descriptionString">The description associated with the registration.</param>
        public static RegistrationDescription Deserialize(string descriptionString)
        {
            using (XmlReader xmlReader = XmlReader.Create(new StringReader(descriptionString)))
            {
                xmlReader.MoveToContent();
                return (RegistrationDescription)new EntityDescriptionSerializer().Deserialize(xmlReader, xmlReader.LocalName);
            }
        }

        internal RegistrationDescription()
        {
        }

        internal static readonly XmlMember[] RegistrationXmlMembers =
        {
            XmlMember.Create<RegistrationDescription>(ManagementStrings.ETag, (w, n, o) => XmlContract.WriteString(w, n, o.ETag, false), (o, e) => o.ETag = XmlContract.ReadString(e)),
            XmlMember.Create<RegistrationDescription>(ManagementStrings.ExpirationTime, (w, n, o) => XmlContract.WriteDateTime(w, n, o.ExpirationTime), (o, e) => o.ExpirationTime = XmlContract.ReadDateTime(e)),
            XmlMember.Create<RegistrationDescription>(ManagementStrings.RegistrationId, (w, n, o) => XmlContract.WriteString(w, n, o.RegistrationId, true), (o, e) => o.RegistrationId = XmlContract.ReadString(e)),
            XmlMember.Create<RegistrationDescription>(ManagementStrings.Tags, (w, n, o) => XmlContract.WriteString(w, n, o.TagsString, false), (o, e) => o.TagsString = XmlContract.ReadString(e)),
            XmlMember.Create<RegistrationDescription>(ManagementStrings.PushVariables, (w, n, o) => XmlContract.WriteString(w, n, o.PropertyBagString, false), (o, e) => o.PropertyBagString = XmlContract.ReadString(e)),
        };

        internal void Validate(bool checkExpirationTime = true)
        {
            if (checkExpirationTime && this.ExpirationTime != null)
            {
                throw new InvalidDataContractException(SRClient.CannotSpecifyExpirationTime);
            }

            this.OnValidate();
        } 

        internal virtual void OnValidate()
        {
        }

        internal bool InvalidTags { get; private set; }

        private string GetTagsString()
        {
            if (this.Tags != null && this.Tags.Count > 0)
            {
                var sb = new StringBuilder();
                var tags = this.Tags.ToArray();
                for (var i = 0; i < tags.Length; i++)
                {
                    sb.Append(tags[i]);
                    if (i < tags.Length - 1)
                    {
                        sb.Append(",");
                    }
                }

                return sb.ToString();
            }

            return null;
        }

        private void SetTagsString(string tagsString, bool validate)
        {
            var tags = string.IsNullOrEmpty(tagsString)
                    ? new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                    : new HashSet<string>(tagsString.Split(','), StringComparer.OrdinalIgnoreCase);

            this.InvalidTags = false;

            if (validate)
            {
                this.InvalidTags = !string.IsNullOrEmpty(tagsString) && !ValidateTags(tagsString);

                if (!this.InvalidTags)
                {
                    foreach (string tag in tags)
                    {
                        if (tag.Length > Constants.MaximumTagSize)
                        {
                            this.InvalidTags = true;
                            break;
                        }
                    }
                }
            }

            if (this.InvalidTags)
            {
                Tags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }
            else
            {
                Tags = tags;
            }
        }
    }
}
