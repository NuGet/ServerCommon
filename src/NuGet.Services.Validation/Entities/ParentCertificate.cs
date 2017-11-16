// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.


namespace NuGet.Services.Validation
{
    /// <summary>
    /// A X.509 Intermediary or Root Certificate used by one or more end-<see cref="Validation.EndCertificate" />s, 
    /// together forming a certificate chain, used by one or more <see cref="PackageSignature"/>s.
    /// </summary>
    public class ParentCertificate
    {
        /// <summary>
        /// The database-mastered identifier for this certificate.
        /// </summary>
        public long Key { get; set; }

        /// <summary>
        /// The key to the end-<see cref="Validation.EndCertificate"/> of the certificate chain this parent-certificate is part of.
        /// </summary>
        public long EndCertificateKey { get; set; }

        /// <summary>
        /// The SHA1 thumbprint that uniquely identifies this certificate. This is a string with exactly 40 characters.
        /// </summary>
        public string Thumbprint { get; set; }

        /// <summary>
        /// The end-<see cref="Validation.EndCertificate"/> of the certificate chain this parent-certificate is part of.
        /// </summary>
        public virtual EndCertificate EndCertificate { get; set; }
    }
}
