// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace NuGet.Services.Validation
{
    /// <summary>
    /// The signature for a <see cref="Package"/>.
    /// </summary>
    public class PackageSignature
    {
        /// <summary>
        /// The database-mastered identifier for this signature.
        /// </summary>
        public long Key { get; set; }

        /// <summary>
        /// The key to the <see cref="Package"/> this signature is for.
        /// </summary>
        public int PackageKey { get; set; }

        /// <summary>
        /// The time at which this signature was created. A signature is valid as long as it was signed
        /// before its certificates were revoked and/or expired. This timestamp SHOULD come from a trusted
        /// timestamp authority.
        /// </summary>
        public DateTime SignedAt { get; set; }

        /// <summary>
        /// The time at which this record was inserted into the database. This is used to detect signatures
        /// that have been stuck in a "InGracePeriod" state for too long.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// The status for this signature.
        /// </summary>
        public PackageSignatureStatus Status { get; set; }

        /// <summary>
        /// The <see cref="Package"/> this signature is for.
        /// </summary>
        public virtual Package Package { get; set; }

        /// <summary>
        /// The <see cref="Certificate"/>s used to generate this signature.
        /// </summary>
        public virtual ICollection<Certificate> Certificates { get; set; }
    }
}
