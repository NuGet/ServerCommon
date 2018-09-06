// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace NuGet.Services.Status.Table
{
    /// <summary>
    /// An entity that describes a period of time during which a component was affected.
    /// </summary>
    public interface IComponentAffectingEntity
    {
        string AffectedComponentPath { get; }

        /// <remarks>
        /// This should be a <see cref="ComponentStatus"/> converted to an enum.
        /// See https://github.com/Azure/azure-storage-net/issues/383
        /// </remarks>
        int AffectedComponentStatus { get; }

        DateTime StartTime { get; }

        DateTime? EndTime { get; }
        
        bool IsActive { get; }
    }
}
