// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using NuGet.Services.Validation;

namespace NuGet.Services.Errors
{
    public class PackageIsSignedError : ValidationError
    {
        public PackageIsSignedError(string packageId, string packageVersion)
        {
            PackageId = packageId;
            PackageVersion = packageVersion;
        }

        public override ValidationErrorCode ErrorCode => ValidationErrorCode.PackageIsSignedError;

        public string PackageId { get; }

        public string PackageVersion { get; }

        public override string GetMessage() => $"Package {PackageId} {PackageVersion} is signed.";
    }
}
