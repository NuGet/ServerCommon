// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace NuGet.Services.Configuration
{
    public static class Constants
    {
        public const string DefaultKeyVaultPrefix = "KeyVault_";

        public const string KeyVaultVaultNameKey = "VaultName";
        public const string KeyVaultUseManagedIdentity = "UseManagedIdentity";
        public const string KeyVaultTenantIdKey = "TenantId";
        public const string KeyVaultClientIdKey = "ClientId";
        public const string KeyVaultCertificateThumbprintKey = "CertificateThumbprint";
        public const string KeyVaultValidateCertificateKey = "ValidateCertificate";
        public const string KeyVaultStoreNameKey = "StoreName";
        public const string KeyVaultStoreLocationKey = "StoreLocation";
        public const string KeyVaultSendX5c = "SendX5c";
    }
}
