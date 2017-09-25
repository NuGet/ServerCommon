// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace NuGet.Services.Storage
{
    public class StorageListItem
    {
        public Uri Uri { get; set; }

        public DateTime? LastModifiedUtc { get; set; }

        public string ETag { get; set; }

        public StorageListItem(Uri uri, DateTime? lastModifiedUtc, string eTag)
        {
            Uri = uri;
            LastModifiedUtc = lastModifiedUtc;
            ETag = eTag;
        }
    }
}