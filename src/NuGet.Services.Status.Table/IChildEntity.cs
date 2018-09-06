// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace NuGet.Services.Status.Table
{
    /// <summary>
    /// An entity that has a parent.
    /// </summary>
    public interface IChildEntity<T>
    {
        string ParentRowKey { get; }

        bool IsLinked { get; }
    }
}
