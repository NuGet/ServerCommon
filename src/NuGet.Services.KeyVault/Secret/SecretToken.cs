// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;

namespace NuGet.Services.KeyVault.Secret
{
    /// <summary>
    /// <see cref="IToken"/> that does simple secret name -> secret value substitution
    /// </summary>
    public class SecretToken : BaseToken, IToken
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
