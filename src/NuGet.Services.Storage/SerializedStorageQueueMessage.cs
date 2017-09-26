// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace NuGet.Services.Storage
{
    /// <summary>
    /// Internal class used by <see cref="StorageQueueMessageSerializer{T}"/> for messages that have never been serialized before.
    /// </summary>
    internal class SerializedStorageQueueMessage : IStorageQueueMessage
    {
        public string Contents { get; }

        public SerializedStorageQueueMessage(string contents)
        {
            Contents = contents;
        }
    }
}