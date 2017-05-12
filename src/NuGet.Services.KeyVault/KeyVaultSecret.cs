// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace NuGet.Services.KeyVault
{
    public class KeyVaultSecret : Secret
    {
        public KeyVaultSecret(Microsoft.Azure.KeyVault.Secret secret)
            : base(secret.SecretIdentifier.Name, secret.Value, secret.Tags)
        {
        }
    }
}
