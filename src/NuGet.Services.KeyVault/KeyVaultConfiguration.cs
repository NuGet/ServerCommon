﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Security.Cryptography.X509Certificates;

namespace NuGet.Services.KeyVault
{
    public class KeyVaultConfiguration
    {
        public string VaultName { get; }
        public string ClientId { get; }
        public X509Certificate2 Certificate { get; }
        public bool SendX5c { get; }
        public KeyVaultConfiguration(string vaultName, string clientId, X509Certificate2 certificate, bool sendX5c=false)
        {
            if (string.IsNullOrWhiteSpace(vaultName))
            {
                throw new ArgumentNullException(nameof(vaultName));
            }

            if (string.IsNullOrWhiteSpace(clientId))
            {
                throw new ArgumentNullException(nameof(clientId));
            }
            
            VaultName = vaultName;
            ClientId = clientId;
            Certificate = certificate ?? throw new ArgumentNullException(nameof(certificate));
            SendX5c = sendX5c;
        }
    }
}
