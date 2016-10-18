﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.Azure.KeyVault;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace NuGet.Services.KeyVault
{
    /// <summary>
    /// Reads secretes from KeyVault.
    /// Authentication with KeyVault is done using a certificate in location:LocalMachine store name:My 
    /// </summary>
    public class KeyVaultReader : ISecretReader
    {
        private readonly KeyVaultConfiguration _configuration;
        private readonly string _vault;
        private readonly Lazy<KeyVaultClient> _keyVaultClient;
        private ClientAssertionCertificate _clientAssertionCertificate;

        public KeyVaultClient KeyVaultClient => _keyVaultClient.Value;

        public KeyVaultReader(KeyVaultConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            _configuration = configuration;
            _vault = $"https://{_configuration.VaultName}.vault.azure.net/";
            _keyVaultClient = new Lazy<KeyVaultClient>(InitializeClient);
        }

        public async Task<string> GetSecretAsync(string secretName)
        {
            var secret = await _keyVaultClient.Value.GetSecretAsync(_vault, secretName);
            return secret.Value;
        }

        private KeyVaultClient InitializeClient()
        {
            _clientAssertionCertificate = new ClientAssertionCertificate(_configuration.ClientId, GetCertificate());

            return new KeyVaultClient(GetTokenAsync);
        }

        public X509Certificate2 GetCertificate()
        {
            var store = new X509Store(_configuration.StoreName, _configuration.StoreLocation);
            try
            {
                store.Open(OpenFlags.ReadOnly);
                var col = store.Certificates.Find(X509FindType.FindByThumbprint, _configuration.CertificateThumbprint,
                    _configuration.ValidateCertificate);
                if (col.Count == 0)
                {
                    throw new ArgumentException(
                        $"Certificate with thumbprint {_configuration.CertificateThumbprint} was not found in store {_configuration.StoreLocation} {_configuration.StoreName} ");
                }

                return col[0];
            }
            finally
            {
                store.Close();
            }
        }

        private async Task<string> GetTokenAsync(string authority, string resource, string scope)
        {
            var authContext = new AuthenticationContext(authority);
            var result = await authContext.AcquireTokenAsync(resource, _clientAssertionCertificate);

            if (result == null)
            {
                throw new InvalidOperationException("Bearer token acquisition needed to call the KeyVault service failed");
            }

            return result.AccessToken;
        }
    }
}
