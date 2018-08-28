// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace NuGet.Services.Status.Table
{
    /// <summary>
    /// Class used to serialize a <see cref="Message"/> in a table.
    /// </summary>
    public class MessageEntity : TableEntity, ILinkedEntity
    {
        public const string DefaultPartitionKey = "messages";

        public MessageEntity()
        {
        }

        public MessageEntity(string eventRowKey, DateTime time, string contents)
            : base(DefaultPartitionKey, GetRowKey(eventRowKey, time))
        {
            ParentRowKey = eventRowKey;
            Time = time;
            Contents = contents;
        }

        public MessageEntity(EventEntity eventEntity, DateTime time, string contents)
            : this(eventEntity.RowKey, time, contents)
        {
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

        public DateTime Time { get; set; }

        public string Contents { get; set; }

        public Message AsMessage()
        {
            return new Message(Time, Contents);
        }

        public static string GetRowKey(string eventRowKey, DateTime time)
        {
            return $"{eventRowKey}_{time.ToString("o")}";
        }

        public static string GetRowKey(EventEntity eventEntity, DateTime time)
        {
            return GetRowKey(eventEntity.RowKey, time);
        }
    }
}
