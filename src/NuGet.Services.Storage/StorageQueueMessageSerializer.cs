// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace NuGet.Services.Storage
{
    /// <summary>
    /// Base implementation of <see cref="IStorageQueueMessageSerializer{T}"/>.
    /// Uses <see cref="SerializedStorageQueueMessage"/>s and <see cref="DeserializedStorageQueueMessage{T}"/> to store state about the message being operated on.
    /// </summary>
    public abstract class StorageQueueMessageSerializer<T> : IStorageQueueMessageSerializer<T>
    {
        public abstract string Serialize(T contents);

        public abstract T Deserialize(string contents);

        public IStorageQueueMessage SerializeMessage(IStorageQueueMessage<T> message)
        {
            if (message is DeserializedStorageQueueMessage<T>)
            {
                return (message as DeserializedStorageQueueMessage<T>).Message;
            }
            else
            {
                return new SerializedStorageQueueMessage(Serialize(message.Contents));
            }
        }

        public IStorageQueueMessage<T> DeserializeMessage(IStorageQueueMessage message)
        {
            if (message == null)
            {
                return null;
            }

            return new DeserializedStorageQueueMessage<T>(Deserialize(message.Contents), message);
        }
    }
}