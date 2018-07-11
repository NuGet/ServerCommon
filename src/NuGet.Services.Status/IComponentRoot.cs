// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace NuGet.Services.Status
{
    public interface IComponentRoot<out TComponent> 
        where TComponent : IReadOnlyComponent
    {
        IEnumerable<TComponent> SubComponents { get; }
    }
}
