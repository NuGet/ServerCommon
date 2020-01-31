// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Security.Cryptography.X509Certificates;

namespace NuGet.Services.KeyVault
{
    public class KeyVaultConfiguration
    {
        public string VaultName { get; }
        public bool UseManagedIdentity { get; }
        public string ClientId { get; private set; }
        public X509Certificate2 Certificate { get; private set; }
        public bool SendX5c { get; private set; }

        /// <summary>
        /// The constructor for keyvault configuration when using managed identities or certificate
        /// </summary>
        public KeyVaultConfiguration(string vaultName,
            bool useManagedIdentity,
            string clientId,
            CertificateConfiguration certConfig,
            bool sendX5c = false)
        {
            if (string.IsNullOrWhiteSpace(vaultName))
            {
                throw new ArgumentNullException(nameof(vaultName));
            }

            VaultName = vaultName;
            UseManagedIdentity = useManagedIdentity;
            if (!UseManagedIdentity)
            {
                var certificate = CertificateUtility.FindCertificateByThumbprint(certConfig);
                SetupConfiguration(clientId, certificate, sendX5c);
            }
        }

        /// <summary>
        /// The constructor for keyvault configuraiton when using the certificate
        /// </summary>
        /// <param name="vaultName">The name of the keyvault</param>
        /// <param name="clientId">Keyvault client id</param>
        /// <param name="certificate">Certificate required to access the keyvault</param>
        /// <param name="sendX5c">SendX5c property</param>
        public KeyVaultConfiguration(string vaultName, string clientId, X509Certificate2 certificate, bool sendX5c = false)
        {
            if (string.IsNullOrWhiteSpace(vaultName))
            {
                throw new ArgumentNullException(nameof(vaultName));
            }

            UseManagedIdentity = false;
            SetupConfiguration(clientId, certificate, sendX5c);
        }

        private void SetupConfiguration(string clientId, X509Certificate2 certificate, bool sendX5c)
        {
            if (string.IsNullOrWhiteSpace(clientId))
            {
                throw new ArgumentNullException(nameof(clientId));
            }

            ClientId = clientId;
            Certificate = certificate ?? throw new ArgumentNullException(nameof(certificate));
            SendX5c = sendX5c;
        }
    }
}
