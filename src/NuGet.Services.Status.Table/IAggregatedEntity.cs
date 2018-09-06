// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace NuGet.Services.Status.Table
{
    /// <summary>
    /// A <see cref="IComponentAffectingEntity"/> that is aggregated by <typeparamref name="T"/>.
    /// </summary>
    public interface IAggregatedEntity<T> : IChildEntity<T>, IComponentAffectingEntity
        where T : IComponentAffectingEntity
    {
    }
}
