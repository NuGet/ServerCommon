// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace NuGet.Services.Status.Table
{
    /// <summary>
    /// Class used to serialize a <see cref="Message"/> in a table.
    /// </summary>
    public class MessageEntity : ChildEntity<EventEntity>
    {
        public const string DefaultPartitionKey = "messages";

        public MessageEntity()
        {
        }

        public MessageEntity(EventEntity eventEntity, DateTime time, string contents)
            : base(
                  DefaultPartitionKey,
                  GetRowKey(eventEntity, time),
                  eventEntity)
        {
            Time = time;
            Contents = contents;
        }

        public DateTime Time { get; set; }

        public string Contents { get; set; }

        public bool IsManual { get; set; }

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
