﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Newtonsoft.Json;

namespace NuGet.Services.Validation.Issues
{
    public sealed class UnauthorizedCertificateFailureSha256 : ValidationIssue
    {
        [JsonConstructor]
        public UnauthorizedCertificateFailureSha256(string sha256Thumbprint)
        {
            Sha256Thumbprint = sha256Thumbprint ?? throw new ArgumentNullException(nameof(sha256Thumbprint));
        }

        public override ValidationIssueCode IssueCode => ValidationIssueCode.PackageIsSignedWithUnauthorizedCertificateSha256;

        [JsonProperty("t", Required = Required.Always)]
        public string Sha256Thumbprint { get; }
    }
}