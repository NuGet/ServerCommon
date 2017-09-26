// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace NuGet.Services.Storage
{
    /// <summary>
    /// Serializes <see cref="IStorageQueueMessage{T}"/>s and deserializes <see cref="IStorageQueueMessage"/>s for use by a <see cref="IStorageQueue{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type to be serialized and deserialized.</typeparam>
    public interface IStorageQueueMessageSerializer<T>
    {
        /// <summary>
        /// Serializes a <see cref="IStorageQueueMessage{T}"/> into a <see cref="IStorageQueueMessage"/>.
        /// </summary>
        /// <param name="message">The message to serialize.</param>
        /// <returns></returns>
        IStorageQueueMessage SerializeMessage(IStorageQueueMessage<T> message);

        /// <summary>
        /// Deserializes a <see cref="IStorageQueueMessage"/> into a <see cref="IStorageQueueMessage{T}"/>.
        /// </summary>
        /// <param name="message">The message to deserialize.</param>
        /// <returns></returns>
        IStorageQueueMessage<T> DeserializeMessage(IStorageQueueMessage message);
    }
}