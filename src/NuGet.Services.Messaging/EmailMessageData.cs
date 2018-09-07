// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace NuGet.Services.Messaging
{
    public class EmailMessageData
    {
        public EmailMessageData(
            string subject,
            string body,
            string sender,
            IEnumerable<string> to,
            IEnumerable<string> cc,
            IEnumerable<string> bcc,
            Guid messageTrackingId)
            : this(subject, body, sender, to, cc, bcc, messageTrackingId, deliveryCount: 0)
        {
        }

        public EmailMessageData(
            string subject,
            string body,
            string sender,
            IEnumerable<string> to,
            IEnumerable<string> cc,
            IEnumerable<string> bcc,
            Guid messageTrackingId,
            int deliveryCount)
        {
            if (messageTrackingId == Guid.Empty)
            {
                throw new ArgumentOutOfRangeException(nameof(messageTrackingId));
            }

            MessageTrackingId = messageTrackingId;
            DeliveryCount = deliveryCount;
            Sender = sender ?? throw new ArgumentNullException(nameof(sender));
            Body = body ?? throw new ArgumentNullException(nameof(body));
            Subject = subject ?? throw new ArgumentNullException(nameof(subject));
            To = to;
            CC = cc;
            BCC = bcc;
        }

        public Guid MessageTrackingId { get; }
        public int DeliveryCount { get; }
        public string Body { get; }
        public string Subject { get; }
        public string Sender { get; }
        public IEnumerable<string> To { get; }
        public IEnumerable<string> CC { get; }
        public IEnumerable<string> BCC { get; }
    }
}
