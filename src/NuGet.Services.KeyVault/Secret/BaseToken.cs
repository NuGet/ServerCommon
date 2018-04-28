// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace NuGet.Services.KeyVault.Secret
{
    /// <summary>
    /// Base token implementation that contains token "value".
    /// It mostly exists to make testing easier
    /// </summary>
    public abstract class BaseToken
    {
        public string Value { get; }

        public BaseToken(string value)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        public override string ToString()
            => $"{GetType().Name}({Value})";
    }
}
