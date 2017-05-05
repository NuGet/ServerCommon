﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NuGet.Services.Cursor
{
    public abstract class ReadCursor<T>
    {
        public T Value { get; set; }

        public abstract Task Load(CancellationToken cancellationToken);
    }
}
