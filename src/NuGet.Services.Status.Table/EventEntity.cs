// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace NuGet.Services.Status.Table
{
    /// <summary>
    /// An aggregation of all <see cref="IncidentGroupEntity"/>s affecting a component and its subcomponents during a period of time.
    /// See <see cref="Event"/>.
    /// </summary>
    public class EventEntity : ComponentAffectingEntity
    {
        public const string DefaultPartitionKey = "events";

        public EventEntity()
        {
        }

        public EventEntity(
            string affectedComponentPath,
            DateTime startTime,
            ComponentStatus affectedComponentStatus = ComponentStatus.Up,
            DateTime? endTime = null)
            : base(
                  DefaultPartitionKey, 
                  GetRowKey(affectedComponentPath, startTime),
                  affectedComponentPath,
                  startTime,
                  affectedComponentStatus,
                  endTime)
        {
        }

        public static string GetRowKey(string affectedComponentPath, DateTime startTime)
        {
            return $"{Utility.ToRowKeySafeComponentPath(affectedComponentPath)}_{startTime.ToString("o")}";
        }
    }
}
