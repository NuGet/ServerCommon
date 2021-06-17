// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace NuGet.Services.KeyVault
{
    public class EmptySyncSecretReader : ISyncSecretReader
    {
        public string GetSecret(string secretName)
            => secretName;

        public ISecret GetSecretObject(string secretName)
            => new KeyVaultSecret(secretName, secretName, expiryDate: null);
    }
}