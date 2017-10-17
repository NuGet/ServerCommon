// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;

namespace NuGet.Services.ServiceBus
{
    /// <summary>
    /// The class that handles messages received by a <see cref="ISubscriptionListener{TMessage}"/>
    /// </summary>
    /// <typeparam name="TMessage">The type of messages this handler handles.</typeparam>
    public interface IMessageHandler<TMessage>
    {
        /// <summary>
        /// Handle the message.
        /// </summary>
        /// <param name="message">The received message.</param>
        /// <returns>A task that completes once the message has been handled.</returns>
        Task HandleAsync(TMessage message);
    }
}
