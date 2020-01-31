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
        private string _clientId;
        private string _certificateThumbprint;
        private string _storeName;
        private string _storeLocation;
        private bool _validateCertificate;
        private bool _sendX5c;

        public ConfigurationRootSecretReaderFactory(IConfigurationRoot config)
        {
            _vaultName = config[Constants.KeyVaultVaultNameKey];
            string useManagedIdentity = config[Constants.KeyVaultUseManagedIdentity];
            if (!string.IsNullOrEmpty(useManagedIdentity))
            {
                _useManagedIdentity = bool.Parse(useManagedIdentity);
            }

            _clientId = config[Constants.KeyVaultClientIdKey];
            _certificateThumbprint = config[Constants.KeyVaultCertificateThumbprintKey];
            _storeName = config[Constants.KeyVaultStoreNameKey];
            _storeLocation = config[Constants.KeyVaultStoreLocationKey];
            
            string validateCertificate = config[Constants.KeyVaultValidateCertificateKey];
            if (!string.IsNullOrEmpty(validateCertificate))
            {
                _validateCertificate = bool.Parse(validateCertificate);
            }

            string sendX5c = config[Constants.KeyVaultSendX5c];
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

            var certificateConfig = new CertificateConfiguration(_storeName,
                _storeLocation,
                _certificateThumbprint,
                _validateCertificate);

            var keyVaultConfiguration = new KeyVaultConfiguration(
                _vaultName,
                _useManagedIdentity,
                _clientId,
                certificateConfig,
                _sendX5c);

            return new KeyVaultReader(keyVaultConfiguration);
        }

        public ISecretInjector CreateSecretInjector(ISecretReader secretReader)
        {
            return new SecretInjector(secretReader);
        }
    }
}
