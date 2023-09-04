// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;

namespace NuGet.Services.ServiceBus
{
    public class ServiceBusMessageWrapper : IBrokeredMessage
    {
        public ServiceBusMessageWrapper(string data)
        {
            ServiceBusMessage = new ServiceBusMessage(ServiceBusClientHelper.SerializeXmlDataContract(data));
        }

        public ServiceBusMessageWrapper(ServiceBusMessage brokeredMessage)
        {
            ServiceBusMessage = brokeredMessage ?? throw new ArgumentNullException(nameof(brokeredMessage));
        }

        public ServiceBusMessage ServiceBusMessage { get; }

        public TimeSpan TimeToLive
        {
            get => ServiceBusMessage.TimeToLive;
            set => ServiceBusMessage.TimeToLive = value;
        }

        public IDictionary<string, object> Properties => ServiceBusMessage.ApplicationProperties;

        public string MessageId
        {
            get => ServiceBusMessage.MessageId;
            set => ServiceBusMessage.MessageId = value;
        }

        public DateTimeOffset ScheduledEnqueueTimeUtc
        {
            get => ServiceBusMessage.ScheduledEnqueueTime;
            set => ServiceBusMessage.ScheduledEnqueueTime = value;
        }

        public string GetBody()
        {
            return GetBody<string>();
        }

        public TStream GetBody<TStream>()
        {
            return ServiceBusClientHelper.DeserializeXmlDataContract<TStream>(ServiceBusMessage.Body);
        }
    }
}
