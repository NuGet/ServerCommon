// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace NuGet.Services.Validation
{
    /// <summary>
    /// The status for a given <see cref="Certificate"/>.
    /// </summary>
    public enum CertificateStatus
    {
        /// <summary>
        /// The <see cref="Certificate" /> has not been revoked.
        /// </summary>
        Good = 0,

        /// <summary>
        /// The <see cref="Certificate" /> has been revoked.
        /// </summary>
        Revoked = 1,
    }

    /// <summary>
    /// A X.509 Certificate used by one or more <see cref="PackageSignature" />s for one or more <see cref="Package"/>s.
    /// </summary>
    public class Certificate
    {
        /// <summary>
        /// The database-mastered identifier for this certificate.
        /// </summary>
        public long Key { get; set; }

        /// <summary>
        /// The SHA1 thumbprint that uniquely identifies this certificate. This is a binary string with
        /// a maximum length of 64 bytes.
        /// </summary>
        public byte[] Thumbprint { get; set;}

        /// <summary>
        /// The last known status for this certificate. This may be stale.
        /// </summary>
        public CertificateStatus Status { get; set; }

        /// <summary>
        /// The time at which the status was known to be correct, according to the Certificate Authority.
        /// </summary>
        public DateTime StatusUpdateTime { get; set; }

        /// <summary>
        /// The time at or before which newer information will be available about the certificate's status,
        /// according to the Certificate Authority.
        /// </summary>
        public DateTime NextStatusUpdateTime { get; set; }

        /// <summary>
        /// The last time this certificate's metadata was updated using online verification. NULL if online
        /// verification have never been completed for this certificate.
        /// </summary>
        public DateTime? LastVerificationTime { get; set; }

        /// <summary>
        /// The time at which the certificate was revoked. NULL if the certificate has not been revoked.
        /// </summary>
        public DateTime? RevocationTime { get; set; }

        /// <summary>
        /// The number of times online revocation checking failed without a "Good" or "Revoked" response.
        /// This counter should be reset each time the certificate is properly verified.
        /// </summary>
        public int ValidationFailures { get; set; }

        /// <summary>
        /// The package signatures that depend on this certificate. If this certificate ever gets revoked,
        /// each of these signatures that were signed after the invalidity period begins MUST be invalidated.
        /// </summary>
        public virtual ICollection<PackageSignature> PackageSignatures { get; set; }
    }
}
