// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using NuGet.Versioning;

namespace NuGet.Services.Validation
{
    public class PackageValidationMessageData
    {
        public PackageValidationMessageData(
           string packageId,
           string packageVersion,
           Guid validationTrackingId)
         : this(packageId, packageVersion, validationTrackingId, validatingEntityType: ValidatingEntityType.Package, deliveryCount: 0)
        {
        }

        public PackageValidationMessageData(
            string packageId,
            string packageVersion,
            Guid validationTrackingId,
            ValidatingEntityType validatingEntityType)
          : this(packageId, packageVersion, validationTrackingId, validatingEntityType, deliveryCount: 0)
        {
        }

        internal PackageValidationMessageData(
            string packageId,
            string packageVersion,
            Guid validationTrackingId,
            ValidatingEntityType validatingEntityType,
            int deliveryCount)
        {
            if (validationTrackingId == Guid.Empty)
            {
                throw new ArgumentOutOfRangeException(nameof(validationTrackingId));
            }

            PackageId = packageId ?? throw new ArgumentNullException(nameof(packageId));
            PackageVersion = packageVersion ?? throw new ArgumentNullException(nameof(packageVersion));
            PackageNormalizedVersion = NuGetVersion.Parse(packageVersion).ToNormalizedString();
            ValidationTrackingId = validationTrackingId;
            DeliveryCount = deliveryCount;
            ValidatingEntityType = validatingEntityType;
        }

        public string PackageId { get; }
        public string PackageVersion { get; }
        public string PackageNormalizedVersion { get; }
        public Guid ValidationTrackingId { get; }
        public int DeliveryCount { get; }
        public ValidatingEntityType ValidatingEntityType { get; }
    }
}
