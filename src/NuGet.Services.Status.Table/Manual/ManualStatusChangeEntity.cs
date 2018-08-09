// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace NuGet.Services.Status.Table.Manual
{
    public class ManualStatusChangeEntity : TableEntity
    {
        public const string DefaultPartitionKey = "manual";

        public ManualStatusChangeEntity()
        {
        }

        private ManualStatusChangeEntity(
            DateTime changeTimestamp,
            ManualStatusChangeType type)
            : base(DefaultPartitionKey, GetRowKey(changeTimestamp))
        {
            ChangeTimestamp = changeTimestamp;
            Type = (int)type;
        }

        protected ManualStatusChangeEntity(
            ManualStatusChangeType type)
            : this(DateTime.UtcNow, type)
        {
        }

        public DateTime ChangeTimestamp { get; set; }

        /// <remarks>
        /// This should be a <see cref="ManualStatusChangeType"/> converted to an enum.
        /// See https://github.com/Azure/azure-storage-net/issues/383
        /// </remarks>
        public int Type { get; set; }

        public static string GetRowKey(DateTime changeTimestamp)
        {
            return $"{changeTimestamp.ToString("o")}";
        }
    }
}
