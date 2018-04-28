// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace NuGet.Services.KeyVault.Secret
{
    /// <summary>
    /// Base token implementation that contains token "value" and has default equality
    /// implementation that returns true iff other type is the same as the derived and 
    /// "values" are the same. It mostly exists to make testing easier
    /// </summary>
    /// <typeparam name="T">The concrete derived type</typeparam>
    /// <remarks>The first level of descendants of this class must be sealed</remarks>
    public abstract class BaseToken<T>
        where T: BaseToken<T>
    {
        protected string Value { get; }

        public BaseToken(string value)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        public bool Equals(IToken other)
        {
            var concreteOther = other as T;
            if (concreteOther == null)
            {
                return false;
            }

            return concreteOther.Value == Value;
        }

        public override string ToString()
            => $"{typeof(T).Name}({Value})";
    }
}
