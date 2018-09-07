﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using NuGet.Services.ServiceBus;

namespace NuGet.Services.Messaging
{
    public class ServiceBusMessageSerializer : IServiceBusMessageSerializer
    {
        private const string EmailMessageSchemaName = "EmailMessageData";

        private static readonly BrokeredMessageSerializer<EmailMessageData1> _serializer = new BrokeredMessageSerializer<EmailMessageData1>();

        public EmailMessageData DeserializeEmailMessageData(IBrokeredMessage message)
        {
            var data = _serializer.Deserialize(message);

            return new EmailMessageData(
                data.Subject,
                data.Body,
                data.Sender,
                data.To,
                data.CC,
                data.Bcc,
                data.MessageTrackingId,
                message.DeliveryCount);
        }

        public IBrokeredMessage SerializeEmailMessageData(EmailMessageData message)
        {
            return _serializer.Serialize(new EmailMessageData1
            {
                Subject = message.Subject,
                Body = message.Body,
                Sender = message.Sender,
                To = message.To,
                CC = message.CC,
                Bcc = message.Bcc,
                MessageTrackingId = message.MessageTrackingId
            });
        }

        [Schema(Name = EmailMessageSchemaName, Version = 1)]
        private class EmailMessageData1
        {
            public string Subject { get; set; }
            public string Body { get; set; }
            public string Sender { get; set; }
            public Guid MessageTrackingId { get; set; }
            public IEnumerable<string> To { get; set; }
            public IEnumerable<string> CC { get; set; }
            public IEnumerable<string> Bcc { get; set; }
        }
    }
}
