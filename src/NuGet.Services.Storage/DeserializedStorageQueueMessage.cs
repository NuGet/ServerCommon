// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace NuGet.Services.Storage
{
    /// <summary>
    /// Internal class used by <see cref="StorageQueueMessageSerializer{T}"/> to store a reference to the serialized message returned by the implementor of <see cref="StorageQueue{T}"/>.
    /// </summary>
    internal class DeserializedStorageQueueMessage<T> : IStorageQueueMessage<T>
    {
        public T Contents { get; }

        internal IStorageQueueMessage Message { get; }

        public DeserializedStorageQueueMessage(T contents, IStorageQueueMessage message)
        {
            Contents = contents;
            Message = message;
        }
    }
}