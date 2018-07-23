using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace NuGet.Services.Status.Table
{
    public class CursorEntity : TableEntity
    {
        public const string DefaultPartitionKey = "cursors";
        public const string DefaultRowKey = "1";

        public CursorEntity()
        {
        }

        public CursorEntity(DateTime value)
            : base(DefaultPartitionKey, DefaultRowKey)
        {
            Value = value;
        }

        public DateTime Value { get; set; }
    }
}
