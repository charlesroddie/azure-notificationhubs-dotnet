//------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for
// license information.
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Microsoft.Azure.NotificationHubs.Messaging;
using Microsoft.Azure.NotificationHubs.Tests;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.Azure.NotificationHubs.DotNetCore.Tests
{
    public class ManagementApiE2ETests
    {
        private const string IncorrectConnectionString = "Endpoint=sb://sample-test.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SharedAccessKey";
        private const string NotificationHubNamespaceUriString = "NotificationHubNamespaceUriString";
        private const string NotificationHubConnectionString = "NotificationHubConnectionString";
        private const string NotificationHubName = "NotificationHubName";
        private const string InputFileName = "ImportRegistrations.txt";

        private const string StorageAccount = "StorageAccount";
        private const string StoragePassword = "StoragePassword";
        private const string StorageEndpointString = "StorageEndpointString";
        private const string ContainerName = "ContainerName";

        private NamespaceManager _namespaceManager;
        private string _notificationHubName;
        private readonly Uri _inputFileSasUri;
        private readonly Uri _outputContainerSasUri;
        private readonly TestServerProxy _testServer;

        private string _namespaceUriString;
        private string _notificationHubConnectionString;

        public ManagementApiE2ETests()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            _namespaceUriString = Environment.GetEnvironmentVariable(NotificationHubNamespaceUriString.ToUpperInvariant()) ?? configuration[NotificationHubNamespaceUriString];
            _notificationHubConnectionString = Environment.GetEnvironmentVariable(NotificationHubConnectionString.ToUpperInvariant()) ?? configuration[NotificationHubConnectionString];
            _notificationHubName = Environment.GetEnvironmentVariable(NotificationHubName.ToUpperInvariant()) ?? configuration[NotificationHubName];

            var storageAccount = Environment.GetEnvironmentVariable(StorageAccount.ToUpperInvariant()) ?? configuration[StorageAccount];
            var storagePassword = Environment.GetEnvironmentVariable(StoragePassword.ToUpperInvariant()) ?? configuration[StoragePassword];
            var storageEndpointAddress = Environment.GetEnvironmentVariable(StorageEndpointString.ToUpperInvariant()) ?? configuration[StorageEndpointString];
            var containerName = Environment.GetEnvironmentVariable(ContainerName.ToUpperInvariant()) ?? configuration[ContainerName];


            _testServer = new TestServerProxy();
            if (_notificationHubConnectionString == "<insert value here before running tests>")
            {
                _notificationHubConnectionString = "Endpoint=sb://sample.servicebus.windows.net/;SharedAccessKeyName=DefaultListenSharedAccessSignature;SharedAccessKey=xxxxxx";
                _namespaceManager = CreateNamespaceManager(RecordingMode.Playback, _notificationHubConnectionString);
            }
            else
            {
                _namespaceManager = CreateNamespaceManager(RecordingMode.Recording, _notificationHubConnectionString);
            }
            if (storageAccount != "<insert value here before running tests>")
            {
                CleanUp();
                _testServer.RecordingMode = RecordingMode.Recording;
                var blobServiceClient = new BlobServiceClient(
                   new Uri(storageEndpointAddress),
                   new StorageSharedKeyCredential(storageAccount, storagePassword));

                var container = blobServiceClient.GetBlobContainerClient(containerName);

                _outputContainerSasUri = GetOutputDirectoryUrl(container);
                _inputFileSasUri = GetInputFileUrl(container, InputFileName);
            }
            else
            {
                _testServer.RecordingMode = RecordingMode.Playback;
                _outputContainerSasUri = new Uri("https://test.blob.core.windows.net/");
                _inputFileSasUri = new Uri("https://test.blob.core.windows.net/");
            }
        }

        [Fact]
        public void ManagementApi_ShouldCreateGetOrDeleteHubOrRecieveConflictException()
        {
            LoadMockData();
            try
            {
                bool notificationHubExists = true;
                IEnumerable<NotificationHubDescription> notificationHubDescriptions;

                // Check that GetNotification returns MessagingEntityNotFoundException than hub does not exist
                Assert.Throws<MessagingEntityNotFoundException>(() => _namespaceManager.GetNotificationHub(_notificationHubName));

                // Check that NotificationHubExists returns false when notification hub does not exist
                notificationHubExists = _namespaceManager.NotificationHubExists(_notificationHubName);
                Assert.False(notificationHubExists);

                // Check that GetNotificationHubs returns collection without hub
                notificationHubDescriptions = _namespaceManager.GetNotificationHubs();
                Assert.DoesNotContain(notificationHubDescriptions, nhd => nhd.Path == _notificationHubName);

                // Check that CreateNotificationHub method create hub with correct Path
                var createNotificationHubDescription = _namespaceManager.CreateNotificationHub(_notificationHubName);
                Assert.Equal(_notificationHubName, createNotificationHubDescription.Path);

                // Check that NotificationHubExists return true when notification hub exist
                notificationHubExists = _namespaceManager.NotificationHubExists(_notificationHubName);
                Assert.True(notificationHubExists);

                // Check that GetNotificationHubs returns collection with existed hub
                notificationHubDescriptions = _namespaceManager.GetNotificationHubs();
                Assert.Single(notificationHubDescriptions);

                // Check that CreateNotificationHub returns MessagingEntityAlreadyExistsException than hub is alredy exist
                Assert.Throws<MessagingEntityAlreadyExistsException>(() => _namespaceManager.CreateNotificationHub(_notificationHubName));

                // Check that GetNotificationHub returns correct hub
                var getNotificationHubDescription = _namespaceManager.GetNotificationHub(_notificationHubName);
                Assert.NotNull(getNotificationHubDescription);

                // Check that UpdateNotificationHub correctly update hub
                createNotificationHubDescription.IsDisabled = true;
                var updatedNotificationHubDescription = _namespaceManager.UpdateNotificationHub(createNotificationHubDescription);
                Assert.True(updatedNotificationHubDescription.IsDisabled);

                // Check that DeleteNotificationHub correctly remove hub
                _namespaceManager.DeleteNotificationHub(_notificationHubName);

                // Check that NotificationHubExists return false when notification hub is not exist
                notificationHubExists = _namespaceManager.NotificationHubExists(_notificationHubName);
                Assert.False(notificationHubExists);

                // Check that GetNotificationHubs returns collection without not existed hub
                notificationHubDescriptions = _namespaceManager.GetNotificationHubs();
                Assert.Empty(notificationHubDescriptions);

                // Check that DeleteNotificationHub returns MessagingEntityNotFoundException than hub is not exist
                Assert.Throws<MessagingEntityNotFoundException>(() => _namespaceManager.DeleteNotificationHub(_notificationHubName));
            }
            finally
            {
                RecordTestResults();
            }
        }

        [Fact]
        public async System.Threading.Tasks.Task ManagementApi_ShouldReceiveCorrectJobs()
        {
            LoadMockData();
            try
            {
                _namespaceManager.CreateNotificationHub(_notificationHubName);

                var notificationHubJob = new NotificationHubJob
                {
                    JobType = NotificationHubJobType.ImportCreateRegistrations,
                    OutputContainerUri = _outputContainerSasUri,
                    ImportFileUri = _inputFileSasUri
                };

                var submitedNotificationHubJob
                    = await _namespaceManager.SubmitNotificationHubJobAsync(notificationHubJob, _notificationHubName);
                Assert.NotNull(submitedNotificationHubJob);
                Assert.NotEmpty(submitedNotificationHubJob.JobId);

                var recievedNotificationHubJob
                    = await _namespaceManager.GetNotificationHubJobAsync(submitedNotificationHubJob.JobId, _notificationHubName);

                Assert.Equal(submitedNotificationHubJob.JobId, recievedNotificationHubJob.JobId);
            }
            finally
            {
                RecordTestResults();
            }
        }

        [Fact]
        public void ManagementApi_FailsWithAuthorizationException()
        {
            LoadMockData();
            try
            {
                var namespaceManager = CreateNamespaceManager(_testServer.RecordingMode, IncorrectConnectionString);

                // Check that CreateNotificationHub returns UnauthorizedAccessException when connection string is incorrect
                Assert.Throws<UnauthorizedException>(() => namespaceManager.CreateNotificationHub(_notificationHubName));

                // We must create hub to recieve UnauthorizedAccessException when GetNotificationHub and DeleteNotificationHub execute
                var notificationHubDescription = _namespaceManager.CreateNotificationHub(_notificationHubName);

                // Check that GetNotificationHub returns UnauthorizedAccessException when connection string is incorrect
                Assert.Throws<UnauthorizedException>(() => namespaceManager.GetNotificationHub(_notificationHubName));

                // Check that NotificationHubExists returns UnauthorizedAccessException when connection string is incorrect
                Assert.Throws<UnauthorizedException>(() => namespaceManager.NotificationHubExists(_notificationHubName));

                // Check that UpdateNotificationHub returns UnauthorizedAccessException when connection string is incorrect
                Assert.Throws<UnauthorizedException>(() => namespaceManager.UpdateNotificationHub(notificationHubDescription));

                // Check that DeleteNotificationHub returns UnauthorizedAccessException when connection string is incorrect
                Assert.Throws<UnauthorizedException>(() => namespaceManager.DeleteNotificationHub(_notificationHubName));
            }
            finally
            {
                RecordTestResults();
            }
        }

        private void CleanUp()
        {
            try
            {
                _namespaceManager.DeleteNotificationHub(_notificationHubName);
            }
            catch
            {
                // no-op
            }
        }

        private static Uri GetInputFileUrl(BlobContainerClient container, string filePath)
        {
            return container.GetBlobClient(filePath).GenerateSasUri(
                BlobSasPermissions.Write | BlobSasPermissions.Read | BlobSasPermissions.Create,
                DateTimeOffset.UtcNow.AddHours(4));
        }

        private static Uri GetOutputDirectoryUrl(BlobContainerClient container)
        {
            return container.GenerateSasUri(
                BlobContainerSasPermissions.Write | BlobContainerSasPermissions.List | BlobContainerSasPermissions.Read,
                DateTimeOffset.UtcNow.AddHours(4));
        }

        private string GetMockDataFilePath(string methodName)
        {
            string[] dataFilePaths = new string[]
            {
                Path.Combine("MockData", $"{methodName}.http")
            };

            foreach (var dataFilePath in dataFilePaths)
            {
                if (File.Exists(dataFilePath))
                {
                    return dataFilePath;
                }
            }
            return null;
        }

        private void LoadMockData([CallerMemberName] string methodName = "")
        {
            if (_testServer.RecordingMode == RecordingMode.Playback)
            {
                string filePath = GetMockDataFilePath(methodName);
                if (filePath == null)
                {
                    throw new Exception($"Cannot find data file for method '{methodName}'. Test data must be recorded first.");
                }

                var payloads = JsonConvert.DeserializeObject<TestServerSession>(File.ReadAllText(filePath));

                _testServer.LoadResponses(payloads);
                _testServer.BaseUri = "http://test";
            }
        }

        private void RecordTestResults([CallerMemberName] string methodName = "")
        {
            if (_testServer.RecordingMode == RecordingMode.Recording)
            {
                File.WriteAllText($"MockData\\{methodName}.http", JsonConvert.SerializeObject(_testServer.Session));
            }
        }

        private NamespaceManager CreateNamespaceManager(RecordingMode recordingMode, string connectionString)
        {
            if (recordingMode == RecordingMode.Playback)
            {
                _notificationHubName = "test";
                _namespaceUriString = "https://sample.servicebus.windows.net/";
            }

            var namespaceManagerSettings = new NotificationHubSettings();
            namespaceManagerSettings.MessageHandler = _testServer;
            return new NamespaceManager(connectionString, namespaceManagerSettings);
        }
    }
}
