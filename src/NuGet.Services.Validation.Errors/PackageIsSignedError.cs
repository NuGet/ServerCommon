// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace NuGet.Services.Validation.Errors
{
    public class PackageIsSignedError : ValidationError
    {
        public PackageIsSignedError(string packageId, string packageVersion)
        {
            if (string.IsNullOrEmpty(packageId))
            {
                throw new ArgumentNullException(nameof(packageId));
            }

            if (string.IsNullOrEmpty(packageVersion))
            {
                throw new ArgumentNullException(nameof(packageVersion));
            }

            PackageId = packageId;
            PackageVersion = packageVersion;
        }

        public override ValidationErrorCode ErrorCode => ValidationErrorCode.PackageIsSignedError;

        public string PackageId { get; }

        public string PackageVersion { get; }

        public override string GetMessage() => $"Package {PackageId} {PackageVersion} is signed.";
    }
}
