// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace NuGet.Services.Status.Table
{
    public class IncidentGroupEntity : TableEntity, IEntityAggregation, IAggregatedEntity
    {
        public const string DefaultPartitionKey = "groups";

        public IncidentGroupEntity()
        {
        }

        public IncidentGroupEntity(
            string affectedComponentPath,
            int affectedComponentStatus,
            DateTime startTime,
            DateTime? endTime = null)
            : base(DefaultPartitionKey, GetRowKey(affectedComponentPath, startTime))
        {
            AffectedComponentPath = affectedComponentPath;
            AffectedComponentStatus = affectedComponentStatus;
            StartTime = startTime;
            EndTime = endTime;
        }

        public IncidentGroupEntity(
            string affectedComponentPath,
            ComponentStatus affectedComponentStatus,
            DateTime startTime,
            DateTime? endTime = null)
            : this(affectedComponentPath, (int)affectedComponentStatus, startTime, endTime)
        {
        }

        public IncidentGroupEntity(IncidentEntity incidentEntity)
            : this(incidentEntity.AffectedComponentPath, incidentEntity.AffectedComponentStatus, incidentEntity.StartTime)
        {
            incidentEntity.ParentRowKey = RowKey;
        }

        public string ParentRowKey { get; set; }

        /// <remarks>
        /// This is a readonly property we would like to serialize.
        /// Unfortunately, it must have a public getter and a public setter for <see cref="TableEntity"/> to serialize it.
        /// The empty setter is intended to trick <see cref="TableEntity"/> into serializing it.
        /// See https://github.com/Azure/azure-storage-net/blob/e01de1b34c316255f1ffe8f5e80917150325b088/Lib/Common/Table/TableEntity.cs#L426
        /// </remarks>
        public bool IsLinked
        {
            get { return !string.IsNullOrEmpty(ParentRowKey); }
            set { }
        }

        public string AffectedComponentPath { get; set; }

        /// <remarks>
        /// This should be a <see cref="ComponentStatus"/> converted to an enum.
        /// See https://github.com/Azure/azure-storage-net/issues/383
        /// </remarks>
        public int AffectedComponentStatus { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        /// <remarks>
        /// This is a readonly property we would like to serialize.
        /// Unfortunately, it must have a public getter and a public setter for <see cref="TableEntity"/> to serialize it.
        /// The empty setter is intended to trick <see cref="TableEntity"/> into serializing it.
        /// See https://github.com/Azure/azure-storage-net/blob/e01de1b34c316255f1ffe8f5e80917150325b088/Lib/Common/Table/TableEntity.cs#L426
        /// </remarks>
        public bool IsActive
        {
            get { return EndTime == null; }
            set { }
        }

        public static string GetRowKey(string affectedComponentPath, DateTime startTime)
        {
            return $"{Utility.ToRowKeySafeComponentPath(affectedComponentPath)}_{startTime.ToString("o")}";
        }
    }
}
