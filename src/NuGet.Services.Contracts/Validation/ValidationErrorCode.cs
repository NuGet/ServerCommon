// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace NuGet.Services.Validation
{
    /// <summary>
    /// The error code for a type of asynchronous validation failure.
    /// </summary>
    public enum ValidationErrorCode
    {
        // Used when signed packages are not accepted.
        PackageIsSignedError = 1,
    }
}