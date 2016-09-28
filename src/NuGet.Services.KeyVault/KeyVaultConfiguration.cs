﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Security.Cryptography.X509Certificates;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace NuGet.Services.KeyVault
{
    public class KeyVaultConfiguration
    {
        public string VaultName { get; }
        public string ClientId { get; }
        public string CertificateThumbprint { get; }
        public StoreName StoreName { get; }
        public StoreLocation StoreLocation { get; set; }
        public bool ValidateCertificate { get; }

        public KeyVaultConfiguration(string vaultName, string clientId, string certificateThumbprint, StoreName storeName, StoreLocation storeLocation, bool validateCertificate)
        {
            if (string.IsNullOrWhiteSpace(vaultName))
            {
                throw new ArgumentNullException(nameof(vaultName));
            }

            if (string.IsNullOrWhiteSpace(clientId))
            {
                throw new ArgumentNullException(nameof(clientId));
            }

            if (string.IsNullOrWhiteSpace(certificateThumbprint))
            {
                throw new ArgumentNullException(nameof(certificateThumbprint));
            }

            VaultName = vaultName;
            ClientId = clientId;
            CertificateThumbprint = certificateThumbprint;
            ValidateCertificate = validateCertificate;
            StoreName = storeName;
            StoreLocation = storeLocation;
        }
        
        public X509Certificate2 GetCertificate()
        {
            var store = new X509Store(StoreName, StoreLocation);
            try
            {
                store.Open(OpenFlags.ReadOnly);
                var col = store.Certificates.Find(X509FindType.FindByThumbprint, CertificateThumbprint, ValidateCertificate);
                if (col.Count == 0)
                {
                    throw new ArgumentException($"Certificate with thumbprint {CertificateThumbprint} was not found in store {StoreLocation} {StoreName} ");
                }

                return col[0];
            }
            finally
            {
                store.Close();
            }
        }

        public ClientAssertionCertificate GetClientAssertionCertificate()
        {
            return new ClientAssertionCertificate(ClientId, GetCertificate());
        }
    }
}
