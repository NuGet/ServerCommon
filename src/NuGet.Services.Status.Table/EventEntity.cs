// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace NuGet.Services.Status.Table
{
    /// <summary>
    /// Class used to serialize an <see cref="Event"/> in a table.
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

        public Event AsEvent(IEnumerable<Message> messages)
        {
            return new Event(AffectedComponentPath, (ComponentStatus)AffectedComponentStatus, StartTime, EndTime, messages);
        }

        public static string GetRowKey(string affectedComponentPath, DateTime startTime)
        {
            return $"{Utility.ToRowKeySafeComponentPath(affectedComponentPath)}_{startTime.ToString("o")}";
        }
    }
}
