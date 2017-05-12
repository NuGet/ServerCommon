// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;

namespace NuGet.Services.KeyVault
{
    public class KeyVaultWriter : KeyVaultReader, ISecretWriter
    {
        public KeyVaultWriter(KeyVaultConfiguration configuration) : 
            base(configuration)
        {
        }

        public Task<Secret> SetSecretAsync(Secret secret)
        {
            return SetSecretAsync(secret.Name, secret.Value, secret.Tags);
        }
        
        public async Task<Secret> SetSecretAsync(string secretName, string value, Dictionary<string, string> tags = null)
        {
            return new KeyVaultSecret(await KeyVaultClient.Value.SetSecretAsync(Vault, secretName, value, tags));
        }

        public Task<Secret> DeleteSecretAsync(Secret secret)
        {
            return DeleteSecretAsync(secret.Name);
        }

        public async Task<Secret> DeleteSecretAsync(string secretName)
        {
            return new KeyVaultSecret(await KeyVaultClient.Value.DeleteSecretAsync(Vault, secretName));
        }
    }
}
