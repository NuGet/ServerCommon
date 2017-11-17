// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace NuGet.Services.Validation
{
    /// <summary>
    /// Represents a validation error encountered while validating the package from the corresponding
    /// <see cref="PackageValidationSet"/>.
    /// </summary>
    public class PackageValidationError
    {
        /// <summary>
        /// The database-mastered identifier for this validaiton error.
        /// </summary>
        public long Key { get; set; }

        /// <summary>
        /// The foreign key referencing a <see cref="PackageValidationSet"/>.
        /// </summary>
        public long PackageValidationSetKey { get; set; }

        /// <summary>
        /// The message for this error.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// The set that this validation is part of.
        /// </summary>
        public PackageValidationSet PackageValidationSet { get; set; }
    }
}
