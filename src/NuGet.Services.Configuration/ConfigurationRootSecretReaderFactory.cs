// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography.X509Certificates;
using NuGet.Services.KeyVault;

namespace NuGet.Services.Configuration
{
    public class ConfigurationRootSecretReaderFactory : ISecretReaderFactory
    {
        private string _vaultName;
        private bool _useManagedIdentity;
        private string _tenantId;
        private string _clientId;
        private string _certificateThumbprint;
        private string _storeName;
        private string _storeLocation;
        private bool _validateCertificate;
        private bool _sendX5c;

        public ConfigurationRootSecretReaderFactory(IConfigurationRoot config, string configurationItemPrefix = Constants.DefaultKeyVaultPrefix)
        {
            if (config == null)
            {
                throw new ArgumentNullException($"{nameof(config)}");
            }

            _vaultName = config[configurationItemPrefix + Constants.KeyVaultVaultNameKey];

            string useManagedIdentity = config[configurationItemPrefix + Constants.KeyVaultUseManagedIdentity];
            if (!string.IsNullOrEmpty(useManagedIdentity))
            {
                _useManagedIdentity = bool.Parse(useManagedIdentity);
            }

            _tenantId = config[configurationItemPrefix + Constants.KeyVaultTenantIdKey];
            _clientId = config[configurationItemPrefix + Constants.KeyVaultClientIdKey];
            _certificateThumbprint = config[configurationItemPrefix + Constants.KeyVaultCertificateThumbprintKey];
            if (_useManagedIdentity && IsCertificateConfigurationProvided())
            {
                throw new ArgumentException($"The KeyVault configuration specifies usage of both, the managed identity and certificate for accessing KeyVault resource. Please specify only one configuration to be used.");
            }

            _storeName = config[configurationItemPrefix + Constants.KeyVaultStoreNameKey];
            _storeLocation = config[configurationItemPrefix + Constants.KeyVaultStoreLocationKey];
            
            string validateCertificate = config[configurationItemPrefix + Constants.KeyVaultValidateCertificateKey];
            if (!string.IsNullOrEmpty(validateCertificate))
            {
                _validateCertificate = bool.Parse(validateCertificate);
            }

            string sendX5c = config[configurationItemPrefix + Constants.KeyVaultSendX5c];
            if (!string.IsNullOrEmpty(sendX5c))
            {
                _sendX5c = bool.Parse(sendX5c);
            }
        }

        public ISecretReader CreateSecretReader()
        {
            if (string.IsNullOrEmpty(_vaultName))
            {
                return new EmptySecretReader();
            }

            KeyVaultConfiguration keyVaultConfiguration;

            if (_useManagedIdentity)
            {
                keyVaultConfiguration = new KeyVaultConfiguration(_vaultName, _clientId);
            }
            else
            {
                var certificate = CertificateUtility.FindCertificateByThumbprint(
                    !string.IsNullOrEmpty(_storeName)
                        ? (StoreName)Enum.Parse(typeof(StoreName), _storeName)
                        : StoreName.My,
                    !string.IsNullOrEmpty(_storeLocation)
                        ? (StoreLocation)Enum.Parse(typeof(StoreLocation), _storeLocation)
                        : StoreLocation.LocalMachine,
                    _certificateThumbprint,
                    _validateCertificate);

                keyVaultConfiguration = new KeyVaultConfiguration(
                    _vaultName,
                    _tenantId,
                    _clientId,
                    certificate,
                    _sendX5c);
            }

            return new KeyVaultReader(keyVaultConfiguration);
        }

        public ISecretInjector CreateSecretInjector(ISecretReader secretReader)
        {
            return new SecretInjector(secretReader);
        }

        private bool IsCertificateConfigurationProvided()
        {
            return !string.IsNullOrEmpty(_certificateThumbprint)
                || !string.IsNullOrEmpty(_tenantId);
        }
    }
}
