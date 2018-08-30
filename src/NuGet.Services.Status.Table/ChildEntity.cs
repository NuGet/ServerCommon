using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace NuGet.Services.Status.Table
{
    public class ChildEntity<T> : TableEntity, IChildEntity<T>
        where T : ITableEntity
    {
        public ChildEntity()
        {
        }

        public ChildEntity(
            string partitionKey, 
            string rowKey, 
            T entity)
            : base(partitionKey, rowKey)
        {
            ParentRowKey = entity.RowKey;
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
            set { throw new NotSupportedException(); }
        }
    }
}
