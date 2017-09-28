// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;

namespace NuGet.Services.Storage
{
    /// <summary>
    /// Base implementation of <see cref="IStorageQueue{T}"/> using a <see cref="IStorageQueueMessageSerializer{T}"/> to serialize messages.
    /// The implementaiton of adding, receiving, and removing is delegated to the subclass.
    /// </summary>
    public abstract class StorageQueue<T> : IStorageQueue<T>
    {
        private IStorageQueueMessageSerializer<T> _serializer;

        /// <summary>
        /// Adds a <see cref="IStorageQueueMessage"/> to the queue.
        /// </summary>
        /// <param name="message">The message to add.</param>
        /// <param name="token">A token to cancel the task with.</param>
        protected abstract Task OnAdd(IStorageQueueMessage message, CancellationToken token);

        /// <summary>
        /// Receives a <see cref="IStorageQueueMessage"/> from the queue.
        /// </summary>
        /// <param name="token">A token to cancel the task with.</param>
        /// <returns>A message from the queue.</returns>
        protected abstract Task<IStorageQueueMessage> OnGetNext(CancellationToken token);

        /// <summary>
        /// Removes a <see cref="IStorageQueueMessage"/> from the queue.
        /// </summary>
        /// <param name="message">The message to remove from the queue.</param>
        /// <param name="token">A token to cancel the task with.</param>
        /// <remarks>
        /// This method should throw if <paramref name="message"/> was not returned by <see cref="OnGetNext(CancellationToken)"/>.
        /// </remarks>
        protected abstract Task OnRemove(IStorageQueueMessage message, CancellationToken token);

        public StorageQueue(IStorageQueueMessageSerializer<T> serializer)
        {
            _serializer = serializer;
        }

        public Task Add(T contents, CancellationToken token)
        {
            return OnAdd(_serializer.SerializeMessage(new StorageQueueMessage<T>(contents)), token);
        }

        public async Task<IStorageQueueMessage<T>> GetNextAsync(CancellationToken token)
        {
            return _serializer.DeserializeMessage(await OnGetNext(token));
        }

        public Task Remove(IStorageQueueMessage<T> message, CancellationToken token)
        {
            return OnRemove(_serializer.SerializeMessage(message), token);
        }
    }
}