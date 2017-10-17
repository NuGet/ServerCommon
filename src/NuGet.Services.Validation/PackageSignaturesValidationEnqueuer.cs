// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using NuGet.Services.ServiceBus;

namespace NuGet.Services.Validation
{
    public class PackageSignaturesValidationEnqueuer : IPackageSignaturesValidationEnqueuer
    {
        private readonly ITopicClient _topicClient;
        private readonly IServiceBusMessageSerializer _serializer;

        public PackageSignaturesValidationEnqueuer(ITopicClient topicClient, IServiceBusMessageSerializer serializer)
        {
            _topicClient = topicClient;
            _serializer = serializer;
        }

        public async Task StartSignaturesValidationAsync(PackageSignaturesValidationMessageData message)
        {
            await StartSignaturesValidationAsync(message, DateTimeOffset.MinValue);
        }

        public async Task StartSignaturesValidationAsync(PackageSignaturesValidationMessageData message, DateTimeOffset postponeProcessingTill)
        {
            var brokeredMessage = _serializer.SerializePackageSignaturesValidationMessageData(message);
            brokeredMessage.ScheduledEnqueueTimeUtc = postponeProcessingTill;
            await _topicClient.SendAsync(brokeredMessage);
        }
    }
}
