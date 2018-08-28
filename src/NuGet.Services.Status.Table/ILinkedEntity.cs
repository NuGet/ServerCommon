// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace NuGet.Services.Status.Table
{
    public interface ILinkedEntity
    {
        string ParentRowKey { get; set; }

        bool IsLinked { get; set; }
    }
}
