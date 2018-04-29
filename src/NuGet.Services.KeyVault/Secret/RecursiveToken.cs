// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;

namespace NuGet.Services.KeyVault.Secret
{
    /// <summary>
    /// <see cref="SecretToken"/> that treats its value as a string containing other secrets and does
    /// additional (single) level of expansion.
    /// </summary>
    public class RecursiveToken : SecretToken, IToken
    {
        private readonly ISecretInjector _secretInjector;

        public RecursiveToken(string secretName, ISecretReader secretReader, ISecretInjector secretInjector)
            : base(secretName, secretReader)
        {
            _secretInjector = secretInjector ?? throw new ArgumentNullException(nameof(secretInjector));
        }

        public override async Task<string> ProcessAsync()
        {
            var value = await base.ProcessAsync();
            return await _secretInjector.InjectAsync(value);
        }
    }
}
