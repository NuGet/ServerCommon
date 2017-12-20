// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.Logging;

namespace NuGet.Services.ServiceBus
{
    public static class Event
    {
        // should be sufficiently different from other event id's, but no strict
        // requirement, numbers mean nothing so far
        private const int EventIdBase = 600; 

        public static EventId SubscriptionMessageHandlerException = new EventId(EventIdBase + 1, "Subscription event handler threw exception");
    }
}
