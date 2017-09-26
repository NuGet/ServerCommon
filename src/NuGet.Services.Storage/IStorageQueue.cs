// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;

namespace NuGet.Services.Storage
{
    /// <summary>
    /// Represents a queue to add <see cref="IStorageQueueMessage{T}"/>s to and receive <see cref="IStorageQueueMessage{T}"/>s from.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IStorageQueue<T>
    {
        /// <summary>
        /// Adds a message containing <paramref name="contents"/> to the queue.
        /// </summary>
        /// <param name="contents">The contents of a message to be added to the queue.</param>
        /// <param name="token">A token to cancel the task with.</param>
        Task Add(T contents, CancellationToken token);

        /// <summary>
        /// Adds a message to the queue.
        /// </summary>
        /// <param name="message">A message to add to the queue.</param>
        /// <param name="token">A token to cancel the task with.</param>
        Task Add(IStorageQueueMessage<T> message, CancellationToken token);

        /// <summary>
        /// Receives a message from the queue.
        /// </summary>
        /// <param name="token">A token to cancel the task with.</param>
        /// <returns>The message from the queue.</returns>
        /// <remarks>
        /// The message is not removed when this method is called and may be returned by subsequent calls to this method.
        /// To remove the message, call <see cref="Remove(IStorageQueueMessage{T}, CancellationToken)"/>.
        /// </remarks>
        Task<IStorageQueueMessage<T>> GetNextAsync(CancellationToken token);

        /// <summary>
        /// Removes a message from the queue.
        /// </summary>
        /// <param name="message">The message to be removed from the queue.</param>
        /// <param name="token">A token to cancel the task with.</param>
        /// <remarks>
        /// <paramref name="message"/> MUST have been previously returned by <see cref="GetNextAsync(CancellationToken)"/>.
        /// </remarks>
        Task Remove(IStorageQueueMessage<T> message, CancellationToken token);
    }
}