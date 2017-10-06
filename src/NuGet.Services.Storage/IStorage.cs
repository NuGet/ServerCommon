﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NuGet.Services.Storage
{
    public interface IStorage
    {
        Task Save(Uri resourceUri, StorageContent content, CancellationToken cancellationToken);
        Task SaveIfETag(Uri resourceUri, StorageContent content, string eTag, CancellationToken cancellationToken);
        Task<StorageContent> Load(Uri resourceUri, CancellationToken cancellationToken);
        Task Delete(Uri resourceUri, CancellationToken cancellationToken);
        Task DeleteIfETag(Uri resourceUri, string eTag, CancellationToken cancellationToken);
        Task<string> LoadString(Uri resourceUri, CancellationToken cancellationToken);
        bool Exists(string fileName);
        Uri BaseAddress { get; }
        Uri ResolveUri(string relativeUri);
        Task<IEnumerable<StorageListItem>> List(CancellationToken cancellationToken);
    }
}