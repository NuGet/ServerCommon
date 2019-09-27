﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using NuGet.Versioning;

namespace NuGet.Services.Validation
{
    public class ProcessValidationSetData
    {
        public ProcessValidationSetData(
            string packageId,
            string packageVersion,
            Guid validationTrackingId,
            ValidatingType validatingType,
            int? entityKey)
        {
            if (validationTrackingId == Guid.Empty)
            {
                throw new ArgumentOutOfRangeException(nameof(validationTrackingId));
            }

            PackageId = packageId;
            PackageVersion = packageVersion;
            PackageNormalizedVersion = NuGetVersion.Parse(packageVersion).ToNormalizedString();
            ValidationTrackingId = validationTrackingId;
            ValidatingType = validatingType;
            EntityKey = entityKey;
        }

        public string PackageId { get; }
        public string PackageVersion { get; }
        public string PackageNormalizedVersion { get; }
        public Guid ValidationTrackingId { get; }
        public ValidatingType ValidatingType { get; }
        public int? EntityKey { get; }
    }
}
