// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;

namespace NuGet.Services.KeyVault
{
    public class EmptySecretReader : ISecretReader
    {
        public Task<Secret> GetSecretAsync(Secret secret)
        {
            return Task.FromResult(secret);
        }

        public Task<Secret> GetSecretAsync(string secretName)
        {
            return Task.FromResult(new Secret(secretName, secretName));
        }
    }
}