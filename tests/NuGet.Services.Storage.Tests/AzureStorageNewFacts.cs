// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.DataMovement.Blobs;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace NuGet.Services.Storage.Tests
{
    public class AzureStorageNewFacts
    {
        private AzureStorage storage;
        private Mock<BlobClient> _blobClientMock = new Mock<BlobClient>();
        private Mock<BlobServiceClient> _blobServiceClientMock = new Mock<BlobServiceClient>();
        private Mock<BlobContainerClient> _blobContainerClientMock = new Mock<BlobContainerClient>();
        private Mock<BlobsStorageResourceProvider> _blobStorageResourceProviderMock = new Mock<BlobsStorageResourceProvider>();
        private Mock<ILogger<AzureStorage>> _loggerMock = new Mock<ILogger<AzureStorage>>();

        public AzureStorageNewFacts() 
        {   }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Exists(bool expected)
        {
            var azureResponse = new Mock<Azure.Response>();
            _blobClientMock.Setup(c => c.Exists(It.IsAny<CancellationToken>())).Returns(Azure.Response.FromValue(expected, azureResponse.Object));
            _blobContainerClientMock.Setup(bsc => bsc.GetBlobClient(It.IsAny<string>()))
                .Returns(_blobClientMock.Object);
            _blobServiceClientMock.Setup(bsc => bsc.GetBlobContainerClient(It.IsAny<string>())).Returns(_blobContainerClientMock.Object);
            var azureStorage = new AzureStorage(_blobServiceClientMock.Object, _blobStorageResourceProviderMock.Object, "containerName", "path", new Uri("http://baseAddress"), useServerSideCopy: true, initializeContainer: true, _loggerMock.Object);

            Assert.Equal(azureStorage.Exists("file"), expected);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ExistsAsync(bool expected)
        {
            var azureResponse = new Mock<Azure.Response>();
            _blobClientMock.Setup(c => c.ExistsAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(Azure.Response.FromValue(expected, azureResponse.Object)));
            _blobContainerClientMock.Setup(bsc => bsc.GetBlobClient(It.IsAny<string>()))
                .Returns(_blobClientMock.Object);
            _blobServiceClientMock.Setup(bsc => bsc.GetBlobContainerClient(It.IsAny<string>())).Returns(_blobContainerClientMock.Object);
            var azureStorage = new AzureStorage(_blobServiceClientMock.Object, _blobStorageResourceProviderMock.Object, "containerName", "path", new Uri("http://baseAddress"), useServerSideCopy: true, initializeContainer: true, _loggerMock.Object);

            Assert.Equal(await azureStorage.ExistsAsync("file", CancellationToken.None), expected);
        }
    }
}
