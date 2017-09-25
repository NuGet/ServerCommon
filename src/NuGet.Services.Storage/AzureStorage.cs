// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace NuGet.Services.Storage
{
    public class AzureStorage : Storage
    {
        private readonly TimeSpan _defaultServerTimeout = TimeSpan.FromSeconds(30);
        private readonly CloudBlobDirectory _directory;
        private readonly BlobRequestOptions _blobRequestOptions;


        public AzureStorage(CloudStorageAccount account,
                            string containerName,
                            string path,
                            Uri baseAddress,
                            ILogger<AzureStorage> logger)
            : this(account, containerName, path, baseAddress, DefaultMaxExecutionTime, logger)
        {
        }

        public AzureStorage(CloudStorageAccount account,
                           string containerName,
                           string path,
                           Uri baseAddress,
                           TimeSpan maxExecutionTime,
                           ILogger<AzureStorage> logger)
           : this(account.CreateCloudBlobClient().GetContainerReference(containerName).GetDirectoryReference(path),
                 baseAddress,
                 maxExecutionTime,
                 logger)
        {
        }

        private AzureStorage(CloudBlobDirectory directory, 
                             Uri baseAddress, 
                             TimeSpan maxExecutionTime, 
                             ILogger<AzureStorage> logger)
            : base(baseAddress ?? GetDirectoryUri(directory), logger)
        {
            _directory = directory;

            if (_directory.Container.CreateIfNotExists())
            {
                _directory.Container.SetPermissions(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });

                if (Verbose)
                {
                    Logger.LogInformation("Created {ContainerName} publish container", _directory.Container.Name);
                }
            }

            ResetStatistics();
            this._blobRequestOptions = CreateBlobRequestOptions(maxExecutionTime);
        }

        public static TimeSpan DefaultMaxExecutionTime
        {
            get
            {
                return TimeSpan.FromSeconds(600);
            }
        }

        public bool CompressContent
        {
            get;
            set;
        }

        static Uri GetDirectoryUri(CloudBlobDirectory directory)
        {
            Uri uri = new UriBuilder(directory.Uri)
            {
                Scheme = "http",
                Port = 80
            }.Uri;

            return uri;
        }

        //Blob exists
        public override bool Exists(string fileName)
        {
            Uri packageRegistrationUri = ResolveUri(fileName);
            string blobName = GetName(packageRegistrationUri);

            CloudBlockBlob blob = _directory.GetBlockBlobReference(blobName);

            if (blob.Exists())
            {
                return true;
            }
            if (Verbose)
            {
                Logger.LogInformation("The blob {BlobUri} does not exist.", packageRegistrationUri);
            }
            return false;
        }

        public override async Task<IEnumerable<StorageListItem>> List(CancellationToken cancellationToken)
        {
            var files = await _directory.ListBlobsAsync(cancellationToken);

            return files.Select(GetStorageListItem).AsEnumerable();
        }

        private StorageListItem GetStorageListItem(IListBlobItem listBlobItem)
        {
            DateTime? lastModified = null;
            string eTag = null;

            var properties = (listBlobItem as CloudBlockBlob)?.Properties;
            if (properties != null)
            {
                lastModified = properties.LastModified?.UtcDateTime;
                eTag = properties.ETag;
            }

            return new StorageListItem(listBlobItem.Uri, lastModified, eTag);
        }

        //  save
        protected override Task OnSave(Uri resourceUri, StorageContent content, CancellationToken cancellationToken)
        {
            return OnSaveInternal(resourceUri, content, null, cancellationToken);
        }

        protected override Task OnSaveIfETag(Uri resourceUri, StorageContent content, string eTag, CancellationToken cancellationToken)
        {
            return OnSaveInternal(resourceUri, content, GetAccessConditionForETag(eTag), cancellationToken);
        }

        private async Task OnSaveInternal(Uri resourceUri, StorageContent content, AccessCondition accessCondition, CancellationToken cancellationToken)
        {
            string name = GetName(resourceUri);

            CloudBlockBlob blob = _directory.GetBlockBlobReference(name);
            blob.Properties.ContentType = content.ContentType;
            blob.Properties.CacheControl = content.CacheControl;

            if (CompressContent)
            {
                blob.Properties.ContentEncoding = "gzip";
                using (Stream stream = content.GetContentStream())
                {
                    MemoryStream destinationStream = new MemoryStream();

                    using (GZipStream compressionStream = new GZipStream(destinationStream, CompressionMode.Compress, true))
                    {
                        await stream.CopyToAsync(compressionStream);
                    }

                    destinationStream.Seek(0, SeekOrigin.Begin);
                    await blob.UploadFromStreamAsync(destinationStream, 
                        accessCondition: accessCondition, 
                        options: _blobRequestOptions, 
                        operationContext: null, 
                        cancellationToken: cancellationToken);
                    Logger.LogInformation("Saved uncompressed blob {BlobUri} to container {ContainerName}", blob.Uri.ToString(), _directory.Container.Name);
                }
            }
            else
            {
                using (Stream stream = content.GetContentStream())
                {
                    await blob.UploadFromStreamAsync(stream,
                        accessCondition: accessCondition,
                        options: _blobRequestOptions,
                        operationContext: null,
                        cancellationToken: cancellationToken);
                    Logger.LogInformation("Saved uncompressed blob {BlobUri} to container {ContainerName}", blob.Uri.ToString(), _directory.Container.Name);
                }
            }
            await TryTakeBlobSnapshotAsync(blob);
        }

        private BlobRequestOptions CreateBlobRequestOptions(TimeSpan maxExecutionTime)
        {
            return new BlobRequestOptions
            {
                ServerTimeout = _defaultServerTimeout,
                MaximumExecutionTime = maxExecutionTime,
                RetryPolicy = new ExponentialRetry()
            };
        }

        /// <summary>
        /// Take one snapshot only if there is not any snapshot for the specific blob
        /// This will prevent the blob to be deleted by a not intended delete action
        /// </summary>
        /// <param name="blob"></param>
        /// <returns></returns>
        private async Task<bool> TryTakeBlobSnapshotAsync(CloudBlockBlob blob)
        {
            if (blob == null)
            {
                //no action
                return false;
            }
            Stopwatch sw = new Stopwatch();
            sw.Start();
            try
            {
                var allSnapshots = blob.Container.
                                   ListBlobs(prefix: blob.Name,
                                             useFlatBlobListing: true,
                                             blobListingDetails: BlobListingDetails.Snapshots);
                //the above call will return at least one blob the original
                if (allSnapshots.Count() == 1)
                {
                    var snapshot = await blob.CreateSnapshotAsync();
                    sw.Stop();
                    Logger.LogInformation("SnapshotCreated:milliseconds={ElapsedMilliseconds}:{BlobUri}:{SnapshotUri}", sw.ElapsedMilliseconds, blob.Uri.ToString(), snapshot.SnapshotQualifiedUri);
                }
                return true;
            }
            catch (StorageException storageException)
            {
                sw.Stop();
                Logger.LogInformation("EXCEPTION:milliseconds={ElapsedMilliseconds}:CreateSnapshot: Failed to take the snapshot for blob {BlobUri}. {Exception}", sw.ElapsedMilliseconds, blob.Uri.ToString(), storageException);
                return false;
            }
        }

        //  load
        protected override async Task<StorageContent> OnLoad(Uri resourceUri, CancellationToken cancellationToken)
        {
            // the Azure SDK will treat a starting / as an absolute URL,
            // while we may be working in a subdirectory of a storage container
            // trim the starting slash to treat it as a relative path
            string name = GetName(resourceUri).TrimStart('/');

            CloudBlockBlob blob = _directory.GetBlockBlobReference(name);

            if (blob.Exists())
            {
                MemoryStream originalStream = new MemoryStream();
                await blob.DownloadToStreamAsync(originalStream,
                                                 accessCondition: null,
                                                 options: _blobRequestOptions,
                                                 operationContext: null,
                                                 cancellationToken: cancellationToken);

                originalStream.Seek(0, SeekOrigin.Begin);

                string content;

                if (blob.Properties.ContentEncoding == "gzip")
                {
                    using (var uncompressedStream = new GZipStream(originalStream, CompressionMode.Decompress))
                    {
                        using (var reader = new StreamReader(uncompressedStream))
                        {
                            content = await reader.ReadToEndAsync();
                        }
                    }
                }
                else
                {
                    using (var reader = new StreamReader(originalStream))
                    {
                        content = await reader.ReadToEndAsync();
                    }
                }

                return new StringStorageContent(content) { ETag = blob.Properties.ETag };
            }

            if (Verbose)
            {
                Logger.LogInformation("Can't load {BlobUri}. Blob doesn't exist", resourceUri);
            }

            return null;
        }

        //  delete
        protected override Task OnDelete(Uri resourceUri, CancellationToken cancellationToken)
        {
            return OnDeleteInternal(resourceUri, null, cancellationToken);
        }

        protected override Task OnDeleteIfETag(Uri resourceUri, string eTag, CancellationToken cancellationToken)
        {
            return OnDeleteInternal(resourceUri, GetAccessConditionForETag(eTag), cancellationToken);
        }

        private Task OnDeleteInternal(Uri resourceUri, AccessCondition accessCondition, CancellationToken cancellationToken)
        {
            string name = GetName(resourceUri);

            CloudBlockBlob blob = _directory.GetBlockBlobReference(name);
            return blob.DeleteAsync(
                deleteSnapshotsOption: DeleteSnapshotsOption.IncludeSnapshots, 
                accessCondition: accessCondition, 
                options: _blobRequestOptions, 
                operationContext: null, 
                cancellationToken: cancellationToken);
        }

        private AccessCondition GetAccessConditionForETag(string eTag)
        {
            return eTag != null ? AccessCondition.GenerateIfMatchCondition(eTag) : AccessCondition.GenerateIfNotExistsCondition();
        }
    }
}