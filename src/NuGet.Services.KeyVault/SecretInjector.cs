// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text;
using System.Threading.Tasks;
using NuGet.Services.KeyVault.Secret;

namespace NuGet.Services.KeyVault
{
    public class SecretInjector : ISecretInjector
    {
        private readonly ITokenizer _tokenizer;

        public SecretInjector(ISecretReader secretReader) 
        {
            _tokenizer = new SimpleTokenizer(secretReader, this);
        }

        public SecretInjector(ITokenizer tokenizer)
        {
            _tokenizer = tokenizer ?? throw new ArgumentNullException(nameof(tokenizer));
        }

        public async Task<string> InjectAsync(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            var output = new StringBuilder();

            foreach (var token in _tokenizer.Tokenize(input))
            {
                output.Append(await token.ProcessAsync());
            }

            return output.ToString();
        }
    }
}
