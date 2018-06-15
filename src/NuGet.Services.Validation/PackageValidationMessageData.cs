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
            string packageNormalizedVersion,
            Guid validationTrackingId)
          : this(packageId, packageVersion, packageNormalizedVersion, validationTrackingId, deliveryCount: 0)
        {
        }

        internal PackageValidationMessageData(
            string packageId,
            string packageVersion,
            string packageNormalizedVersion,
            Guid validationTrackingId,
            int deliveryCount)
        {
            if (validationTrackingId == Guid.Empty)
            {
                throw new ArgumentOutOfRangeException(nameof(validationTrackingId));
            }

            PackageId = packageId ?? throw new ArgumentNullException(nameof(packageId));
            // This is the first step in the transition from PackageVersion to PackageNormalizedVersion
            PackageNormalizedVersion = packageNormalizedVersion ?? Normalize(packageVersion);
            PackageVersion = packageVersion ?? packageNormalizedVersion;
            if (PackageVersion == null && PackageNormalizedVersion == null)
            {
                throw new ArgumentNullException($"{nameof(packageVersion)} and {nameof(packageNormalizedVersion)}");
            }
            ValidationTrackingId = validationTrackingId;
            DeliveryCount = deliveryCount;
        }

        public string PackageId { get; }
        public string PackageVersion { get; }
        public string PackageNormalizedVersion { get; }
        public Guid ValidationTrackingId { get; }
        public int DeliveryCount { get; }

        private static string Normalize(string version)
        {
            if(version == null)
            {
                return null;
            }
            NuGetVersion parsed;
            if (!NuGetVersion.TryParse(version, out parsed))
            {
                return version;
            }

            return parsed.ToNormalizedString();
        }
    }
}
