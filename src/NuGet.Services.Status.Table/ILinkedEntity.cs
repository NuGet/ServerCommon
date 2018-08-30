// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.WindowsAzure.Storage.Table;

namespace NuGet.Services.Status.Table
{
    public interface ILinkedEntity<T> : ITableEntity
    {
        string ParentRowKey { get; set; }

        bool IsLinked { get; set; }
    }
}
