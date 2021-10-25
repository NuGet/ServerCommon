// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace NuGet.Services.KeyVault
{
    public class EmptySecretReader : ICachingSecretReader
    {
        public Task<string> GetSecretAsync(string secretName)
        {
            return GetSecretAsync(secretName, logger: null);
        }

        public Task<string> GetSecretAsync(string secretName, ILogger logger)
        {
            return Task.FromResult(secretName);
        }

        public Task<ISecret> GetSecretObjectAsync(string secretName) => GetSecretObjectAsync(secretName, logger: null);

        public Task<ISecret> GetSecretObjectAsync(string secretName, ILogger logger)
        {
            return Task.FromResult((ISecret)new KeyVaultSecret(secretName, secretName, null));
        }

        public string TryGetCachedSecret(string secretName) => TryGetCachedSecret(secretName, logger: null);

        public string TryGetCachedSecret(string secretName, ILogger logger)
        {
            return secretName;
        }

        public ISecret TryGetCachedSecretObject(string secretName)
        {
            return TryGetCachedSecretObject(secretName, logger: null);
        }

        public ISecret TryGetCachedSecretObject(string secretName, ILogger logger)
        {
            return new KeyVaultSecret(secretName, secretName, null);
        }
    }
}