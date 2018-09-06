// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace NuGet.Services.Status.Table
{
    /// <summary>
    /// An incident that affects a component.
    /// Is aggregated by <see cref="IncidentGroupEntity"/>.
    /// </summary>
    public class IncidentEntity : AggregatedEntity<IncidentGroupEntity>
    {
        public const string DefaultPartitionKey = "incidents";

        public IncidentEntity()
        {
        }

        public IncidentEntity(
            string id,
            IncidentGroupEntity group,
            string affectedComponentPath, 
            ComponentStatus affectedComponentStatus, 
            DateTime startTime, 
            DateTime? endTime)
            : base(
                  DefaultPartitionKey, 
                  GetRowKey(id, affectedComponentPath, affectedComponentStatus), 
                  group,
                  affectedComponentPath,
                  startTime,
                  affectedComponentStatus,
                  endTime)
        {
            IncidentApiId = id;
        }

        public string IncidentApiId { get; set; }

        public static string GetRowKey(string id, string affectedComponentPath, ComponentStatus affectedComponentStatus)
        {
            return $"{id}_{Utility.ToRowKeySafeComponentPath(affectedComponentPath)}_{affectedComponentStatus}";
        }
    }
}
