﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace NuGet.Services.ServiceBus
{
    public class BrokeredMessageWrapper : IBrokeredMessage
    {
        public BrokeredMessageWrapper(string data)
        {
            BrokeredMessage = new BrokeredMessage(data);
        }

        public BrokeredMessageWrapper(BrokeredMessage brokeredMessage)
        {
            BrokeredMessage = brokeredMessage ?? throw new ArgumentNullException(nameof(brokeredMessage));
        }

        public BrokeredMessage BrokeredMessage { get; }

        public IDictionary<string, object> Properties => BrokeredMessage.Properties;

        public DateTimeOffset ScheduledEnqueueTimeUtc
        {
            get => new DateTimeOffset(BrokeredMessage.ScheduledEnqueueTimeUtc);
            set => BrokeredMessage.ScheduledEnqueueTimeUtc = new DateTime(value.UtcTicks, DateTimeKind.Utc);
        }

        public string GetBody()
        {
            return BrokeredMessage.GetBody<string>();
        }

        public Task CompleteAsync()
        {
            return BrokeredMessage.CompleteAsync();
        }

        public Task DeadLetterAsync()
        {
            return BrokeredMessage.DeadLetterAsync();
        }

        public Task AbandonAsync()
        {
            return BrokeredMessage.AbandonAsync();
        }

        public IBrokeredMessage Clone()
        {
            return new BrokeredMessageWrapper(BrokeredMessage.Clone());
        }

        public void Dispose()
        {
            BrokeredMessage.Dispose();
        }
    }
}
