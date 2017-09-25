// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace NuGet.Services.Validation
{
    /// <summary>
    /// Represents the status of a package's signing.
    /// </summary>
    public enum SignatureStatus
    {
        /// <summary>
        /// One or more of the package's signature is invalid.
        /// </summary>
        Invalid = 0,

        /// <summary>
        /// The package has no signatures.
        /// </summary>
        Unsigned = 1,

        /// <summary>
        /// All of the package's signatures are valid.
        /// </summary>
        Valid = 2,
    }

    /// <summary>
    /// Represents a single package signature state.
    /// </summary>
    public class Package
    {
        /// <summary>
        /// The key referencing a package in the NuGet Gallery database. If a package is hard deleted then re-pushed,
        /// the <see cref="PackageId"/> and <see cref="PackageNormalizedVersion"/> version will be the same but the
        /// <see cref="PackageKey"/> will be different.
        /// </summary>
        public int PackageKey { get; set; }

        /// <summary>
        /// The package ID. Has a maximum length of 128 unicode characters as defined by the NuGet Gallery database.
        /// </summary>
        public string PackageId { get; set; }

        /// <summary>
        /// The normalized package version. Has a maximum length of 64 unicode characters as defined by the NuGet
        /// Gallery database.
        /// </summary>
        public string PackageNormalizedVersion { get; set; }

        /// <summary>
        /// The status of the package's signing.
        /// </summary>
        public SignatureStatus SignatureStatus { get; set; }

        /// <summary>
        /// The signatures used to ensure this package's integerity.
        /// </summary>
        public virtual ICollection<PackageSignature> PackageSignatures { get; set; }
    }
}
