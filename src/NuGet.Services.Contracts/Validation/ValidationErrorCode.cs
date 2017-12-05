// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace NuGet.Services.Validation
{
    /// <summary>
    /// The error code for a type of asynchronous validation failure. <see cref="ValidationE"/>
    /// </summary>
    public enum ValidationErrorCode
    {
        /// <summary>
        /// An unknown error has occurred.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Signed packages are blocked.
        /// </summary>
        PackageIsSignedError = 1,
    }
}