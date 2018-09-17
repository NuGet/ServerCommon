﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace NuGet.Services.Messaging
{
    public class EmailMessageData
    {
        public EmailMessageData(
            string subject,
            string plainTextBody,
            string htmlBody,
            string sender,
            IReadOnlyList<string> to,
            IReadOnlyList<string> cc,
            IReadOnlyList<string> bcc,
            Guid messageTrackingId)
            : this(subject, plainTextBody, htmlBody, sender, to, cc, bcc, messageTrackingId, deliveryCount: 0)
        {
        }

        public EmailMessageData(
            string subject,
            string plainTextBody,
            string htmlBody,
            string sender,
            IReadOnlyList<string> to,
            IReadOnlyList<string> cc,
            IReadOnlyList<string> bcc,
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
            PlainTextBody = plainTextBody ?? throw new ArgumentNullException(nameof(plainTextBody));
            HtmlBody = htmlBody ?? throw new ArgumentNullException(nameof(htmlBody));
            Subject = subject ?? throw new ArgumentNullException(nameof(subject));
            To = to ?? throw new ArgumentNullException(nameof(to));
            CC = cc;
            Bcc = bcc;
        }

        /// <summary>
        /// Unique identifier for the email message to allow for server and client-side tracking and correlation.
        /// </summary>
        public Guid MessageTrackingId { get; }

        /// <summary>
        /// Tracks how many times this particular message has been delivered to a processing message handler.
        /// </summary>
        public int DeliveryCount { get; }

        /// <summary>
        /// The email body in plain-text format.
        /// </summary>
        public string PlainTextBody { get; }

        /// <summary>
        /// The email body in HTML format.
        /// </summary>
        public string HtmlBody { get; }

        /// <summary>
        /// The email subject.
        /// </summary>
        public string Subject { get; }
        public string Sender { get; }
        public IReadOnlyList<string> To { get; }
        public IReadOnlyList<string> CC { get; }
        public IReadOnlyList<string> Bcc { get; }
    }
}
