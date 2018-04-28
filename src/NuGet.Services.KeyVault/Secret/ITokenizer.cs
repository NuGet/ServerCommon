// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace NuGet.Services.KeyVault.Secret
{
    public interface ITokenizer
    {
        /// <summary>
        /// Splits a string potentially containing secrets into a sequence of tokens
        /// </summary>
        IEnumerable<IToken> Tokenize(string input);
    }
}
