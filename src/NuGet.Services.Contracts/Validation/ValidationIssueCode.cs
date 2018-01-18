﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace NuGet.Services.Validation
{
    /// <summary>
    /// The codes for <see cref="IValidationIssue"/>.
    /// </summary>
    public enum ValidationIssueCode
    {
        /// <summary>
        /// An unknown issue has occurred.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Signed packages are blocked.
        /// </summary>
        PackageIsSigned = 1,

        /// <summary>
        /// A signing verification failure occurred, where the issue message is mastered by the client APIs.
        /// </summary>
        ClientSigningVerificationFailure = 2,

        /// <summary>
        /// Zip64 packages are not allowed.
        /// </summary>
        PackageIsZip64 = 3,

        /// <summary>
        /// Packages pushed should only have author signatures.
        /// </summary>
        OnlyAuthorSignaturesSupported = 4,

        /// <summary>
        /// Obsolete testing issue - do NOT use this!
        /// </summary>
        [Obsolete("This issue code should only be used for testing")]
        ObsoleteTesting = 9999,
    }
}