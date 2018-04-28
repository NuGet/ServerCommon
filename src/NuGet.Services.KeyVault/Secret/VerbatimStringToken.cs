// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;

namespace NuGet.Services.KeyVault.Secret
{
    /// <summary>
    /// <see cref="IToken"/> that does no processing and just returns its value
    /// </summary>
    public sealed class VerbatimStringToken : BaseToken<VerbatimStringToken>, IToken
    {
        public VerbatimStringToken(string value)
            : base(value)
        {
        }

        public Task<string> ProcessAsync()
            => Task.FromResult(Value);
    }
}
