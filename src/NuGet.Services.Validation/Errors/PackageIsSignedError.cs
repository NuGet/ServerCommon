// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace NuGet.Services.Validation
{
    public class PackageIsSignedError : AbstractError
    {
        public override ValidationErrorCode ErrorCode => ValidationErrorCode.PackageIsSignedError;

        public string PackageId { get; set; }

        public string PackageVersion { get; set; }

        public override string ErrorMessage => $"Package {PackageId} {PackageVersion} is signed.";

    }
}