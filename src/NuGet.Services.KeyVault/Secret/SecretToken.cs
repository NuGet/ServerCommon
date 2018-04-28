// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;

namespace NuGet.Services.KeyVault.Secret
{
    /// <summary>
    /// <see cref="IToken"/> that gets simple secret name -> secret value substitution
    /// </summary>
    public sealed class SecretToken : BaseToken<SecretToken>, IToken
    {
        private readonly ISecretReader _secretReader;

        private string SecretName => Value;

        public SecretToken(string secretName, ISecretReader secretReader)
            : base(secretName)
        {
            _secretReader = secretReader ?? throw new ArgumentNullException(nameof(secretReader));
        }

        public async Task<string> ProcessAsync()
            => await _secretReader.GetSecretAsync(SecretName);
    }
}
