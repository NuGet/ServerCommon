// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace NuGet.Services.Validation
{
    /// <summary>
    /// The state of an <see cref="IValidator"/>'s validation of a package. This should be used
    /// by each <see cref="IValidator"/> to keep track of its own state.
    /// </summary>
    public class ValidatorState
    {
        /// <summary>
        /// The database-mastered identifier for this validator state.
        /// </summary>
        public long Key { get; set; }

        /// <summary>
        /// The package key in the NuGet gallery database.
        /// </summary>
        public long PackageKey { get; set; }

        /// <summary>
        /// The unique identifier for this validation. The Validation Orchestrator generates different
        /// validation IDs for each validator it runs on a single package.
        /// </summary>
        public Guid ValidationId { get; set; }

        /// <summary>
        /// The current status for this validator.
        /// </summary>
        public ValidationStatus State { get; set; }
    }
}
