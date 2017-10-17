// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using NuGet.Services.ServiceBus;

namespace NuGet.Services.Validation
{
    public class ServiceBusMessageSerializer : IServiceBusMessageSerializer
    {
        private const string PackageValidationSchemaName = "PackageValidationMessageData";
        private const string PackageSignaturesValidationSchemaName = "PackageSignaturesValidationMessageData";

        private static readonly BrokeredMessageSerializer<PackageValidationMessageData1> _packageValidationSerializer = new BrokeredMessageSerializer<PackageValidationMessageData1>();
        private static readonly BrokeredMessageSerializer<PackageSignaturesValidationMessageData1> _packageSignaturesValidationSerializer = new BrokeredMessageSerializer<PackageSignaturesValidationMessageData1>();

        public IBrokeredMessage SerializePackageValidationMessageData(PackageValidationMessageData message)
        {
            return _packageValidationSerializer.Serialize(new PackageValidationMessageData1
            {
                PackageId = message.PackageId,
                PackageVersion = message.PackageVersion,
                ValidationTrackingId = message.ValidationTrackingId,
            });
        }

        public IBrokeredMessage SerializePackageSignaturesValidationMessageData(PackageSignaturesValidationMessageData message)
        {
            return _packageSignaturesValidationSerializer.Serialize(new PackageSignaturesValidationMessageData1
            {
                PackageId = message.PackageId,
                PackageVersion = message.PackageVersion,
                ValidationTrackingId = message.ValidationTrackingId,
            });
        }

        public PackageValidationMessageData DeserializePackageValidationMessageData(IBrokeredMessage message)
        {
            var data = _packageValidationSerializer.Deserialize(message);

            return new PackageValidationMessageData(
                data.PackageId,
                data.PackageVersion,
                data.ValidationTrackingId);
        }

        public PackageSignaturesValidationMessageData DeserializePackageSignaturesValidationMessageData(IBrokeredMessage message)
        {
            var data = _packageSignaturesValidationSerializer.Deserialize(message);

            return new PackageSignaturesValidationMessageData(
                data.PackageId,
                data.PackageVersion,
                data.ValidationTrackingId);
        }

        [Schema(Name = PackageValidationSchemaName, Version = 1)]
        private class PackageValidationMessageData1
        {
            public string PackageId { get; set; }
            public string PackageVersion { get; set; }
            public Guid ValidationTrackingId { get; set; }
        }

        [Schema(Name = PackageSignaturesValidationSchemaName, Version = 1)]
        private class PackageSignaturesValidationMessageData1
        {
            public string PackageId { get; set; }
            public string PackageVersion { get; set; }
            public Guid ValidationTrackingId { get; set; }
        }
    }
}
