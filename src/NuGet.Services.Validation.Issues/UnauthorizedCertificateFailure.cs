// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Newtonsoft.Json;

namespace NuGet.Services.Validation.Issues
{
    [Obsolete("This issue uses SHA-1. Use " + nameof(UnauthorizedCertificateSha256Failure) + " instead.")]
    public sealed class UnauthorizedCertificateFailure : ValidationIssue
    {
        [JsonConstructor]
        public UnauthorizedCertificateFailure(string sha1Thumbprint)
        {
            Sha1Thumbprint = sha1Thumbprint ?? throw new ArgumentNullException(nameof(sha1Thumbprint));
        }

        public override ValidationIssueCode IssueCode => ValidationIssueCode.PackageIsSignedWithUnauthorizedCertificate;

        [JsonProperty("t", Required = Required.Always)]
        public string Sha1Thumbprint { get; }
    }
}