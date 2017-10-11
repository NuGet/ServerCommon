﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using NuGet.Services.ServiceBus;

namespace NuGet.Services.Validation
{
    public class ServiceBusMessageSerializer : IServiceBusMessageSerializer
    {
        private static readonly BrokeredMessageSerializer<PackageValidationMessageData1> _serializer = new BrokeredMessageSerializer<PackageValidationMessageData1>();

        public IBrokeredMessage SerializePackageValidationMessageData(PackageValidationMessageData message)
        {
            return _serializer.Serialize(new PackageValidationMessageData1
            {
                PackageId = message.PackageId,
                PackageVersion = message.PackageVersion,
                ValidationTrackingId = message.ValidationTrackingId,
            });
        }

        public PackageValidationMessageData DeserializePackageValidationMessageData(IBrokeredMessage message)
        {
            var data = _serializer.Deserialize(message);

            return new PackageValidationMessageData(
                data.PackageId,
                data.PackageVersion,
                data.ValidationTrackingId);
        }

        [SchemaVersion(1)]
        private class PackageValidationMessageData1
        {
            public string PackageId { get; set; }
            public string PackageVersion { get; set; }
            public Guid ValidationTrackingId { get; set; }
        }
    }
}
