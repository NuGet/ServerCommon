// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;

namespace NuGet.Services.KeyVault.Secret
{
    public class UrlEncodingToken : SecretToken, IToken
    {
        public UrlEncodingToken(string secretName, ISecretReader secretReader)
            : base(secretName, secretReader)
        {
        }

        public override async Task<string> ProcessAsync()
        {
            string value = await base.ProcessAsync();
            return Uri.EscapeDataString(value);
        }
    }
}
