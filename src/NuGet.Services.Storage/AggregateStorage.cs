// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace NuGet.Services.Storage
{
    public class AggregateStorage : Storage
    {
        public delegate StorageContent WriteSecondaryStorageContentInterceptor(
            Uri primaryStorageBaseUri,
            Uri primaryResourceUri,
            Uri secondaryStorageBaseUri,
            Uri secondaryResourceUri,
            StorageContent content);

        private readonly Storage _primaryStorage;
        private readonly Storage[] _secondaryStorage;
        private readonly WriteSecondaryStorageContentInterceptor _writeSecondaryStorageContentInterceptor;
        
        public AggregateStorage(Uri baseAddress, Storage primaryStorage, Storage[] secondaryStorage,
            WriteSecondaryStorageContentInterceptor writeSecondaryStorageContentInterceptor,
            ILogger<AggregateStorage> logger)
            : base(baseAddress, logger)
        {
            _primaryStorage = primaryStorage;
            _secondaryStorage = secondaryStorage;
            _writeSecondaryStorageContentInterceptor = writeSecondaryStorageContentInterceptor;

            BaseAddress = _primaryStorage.BaseAddress;
        }

        protected override Task OnSave(Uri resourceUri, StorageContent content, CancellationToken cancellationToken)
        {
            return OnSaveInternal(resourceUri, content, (storage, uri, storageContent, token) => storage.Save(uri, storageContent, token), cancellationToken);
        }

        protected override Task OnSaveIfETag(Uri resourceUri, StorageContent content, string eTag, CancellationToken cancellationToken)
        {
            return OnSaveInternal(resourceUri, content, (storage, uri, storageContent, token) => storage.SaveIfETag(uri, storageContent, eTag, token), cancellationToken);
        }

        private Task OnSaveInternal(Uri resourceUri, StorageContent content, Func<IStorage, Uri, StorageContent, CancellationToken, Task> saveOperation, CancellationToken cancellationToken)
        {
            var tasks = new List<Task>();
            tasks.Add(saveOperation(_primaryStorage, resourceUri, content, cancellationToken));

            foreach (var storage in _secondaryStorage)
            {
                var secondaryResourceUri = new Uri(resourceUri.ToString()
                    .Replace(_primaryStorage.BaseAddress.ToString(), storage.BaseAddress.ToString()));

                var secondaryContent = content;
                if (_writeSecondaryStorageContentInterceptor != null)
                {
                    secondaryContent = _writeSecondaryStorageContentInterceptor(
                        _primaryStorage.BaseAddress,
                        resourceUri,
                        storage.BaseAddress,
                        secondaryResourceUri, content);
                }

                tasks.Add(saveOperation(storage, secondaryResourceUri, secondaryContent, cancellationToken));
            }

            return Task.WhenAll(tasks);
        }

        protected override Task<StorageContent> OnLoad(Uri resourceUri, CancellationToken cancellationToken)
        {
            return _primaryStorage.Load(resourceUri, cancellationToken);
        }

        protected override Task OnDelete(Uri resourceUri, CancellationToken cancellationToken)
        {
            return OnDeleteInternal(resourceUri, (storage, uri, token) => storage.Delete(uri, token), cancellationToken);
        }

        protected override Task OnDeleteIfETag(Uri resourceUri, string eTag, CancellationToken cancellationToken)
        {
            return OnDeleteInternal(resourceUri, (storage, uri, token) => storage.DeleteIfETag(uri, eTag, token), cancellationToken);
        }

        private Task OnDeleteInternal(Uri resourceUri, Func<IStorage, Uri, CancellationToken, Task> deletionOperation, CancellationToken cancellationToken)
        {
            var tasks = new List<Task>();
            tasks.Add(deletionOperation(_primaryStorage, resourceUri, cancellationToken));

            foreach (var storage in _secondaryStorage)
            {
                var secondaryResourceUri = new Uri(resourceUri.ToString()
                    .Replace(_primaryStorage.BaseAddress.ToString(), storage.BaseAddress.ToString()));

                tasks.Add(deletionOperation(storage, secondaryResourceUri, cancellationToken));
            }

            return Task.WhenAll(tasks);
        }

        public override bool Exists(string fileName)
        {
            return _primaryStorage.Exists(fileName);
        }

        public override Task<IEnumerable<StorageListItem>> List(CancellationToken cancellationToken)
        {
            return _primaryStorage.List(cancellationToken);
        }
    }
}