// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace NuGet.Services.Storage
{
    public class StorageQueue<T> : IStorageQueue<T>
    {
        private IStorageQueue _queue;
        private IMessageSerializer<T> _messageSerializer;

        public StorageQueue(IStorageQueue queue, IMessageSerializer<T> messageSerializer)
        {
            _queue = queue ?? throw new ArgumentNullException(nameof(queue));
            _messageSerializer = messageSerializer ?? throw new ArgumentNullException(nameof(messageSerializer));
        }

        public Task AddAsync(T contents, CancellationToken token)
        {
            return _queue.AddAsync(_messageSerializer.Serialize(contents), token);
        }

        public async Task<IStorageQueueMessage<T>> GetNextAsync(CancellationToken token)
        {
            return DeserializeMessage(await _queue.GetNextAsync(token));
        }

        public Task RemoveAsync(IStorageQueueMessage<T> message, CancellationToken token)
        {
            return _queue.RemoveAsync(SerializeMessage(message), token);
        }

        private IStorageQueueMessage SerializeMessage(IStorageQueueMessage<T> message)
        {
            if (message is DeserializedStorageQueueMessage<T>)
            {
                return (message as DeserializedStorageQueueMessage<T>).Message;
            }
            else
            {
                return new StorageQueueMessage(_messageSerializer.Serialize(message.Contents));
            }
        }

        private IStorageQueueMessage<T> DeserializeMessage(IStorageQueueMessage message)
        {
            if (message == null)
            {
                return null;
            }

            return new DeserializedStorageQueueMessage<T>(_messageSerializer.Deserialize(message.Contents), message);
        }
    }
}