// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Azure;

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Azure.Data.Tables;
using System;

namespace NuGet.Services.Status.Table
{
    /// <summary>
    /// Base implementation of <see cref="IChildEntity{T}"/>.
    /// </summary>
    public class ChildEntity<TParent> : ITableEntity, IChildEntity<TParent>
        where TParent : ITableEntity
    {
        private ITableEntity _tableEntity;

        public ChildEntity()
        {
            _tableEntity = new TableEntity();
        }

        public ChildEntity(
            string partitionKey,
            string rowKey,
            string parentRowKey)
        {
            ParentRowKey = parentRowKey;
            _tableEntity = new TableEntity(partitionKey, rowKey);
        }

        public ChildEntity(
            string partitionKey, 
            string rowKey, 
            TParent entity)
            : this(partitionKey, rowKey, entity.RowKey)
        {
        }

        public string ParentRowKey { get; set; }
        public string PartitionKey { get => _tableEntity.PartitionKey; set => _tableEntity.PartitionKey = value; }
        public string RowKey { get => _tableEntity.RowKey; set => _tableEntity.RowKey = value; }
        public DateTimeOffset? Timestamp { get => _tableEntity.Timestamp; set => _tableEntity.Timestamp = value; }
        public ETag ETag { get => _tableEntity.ETag; set => _tableEntity.ETag = value; }

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

    }
}
