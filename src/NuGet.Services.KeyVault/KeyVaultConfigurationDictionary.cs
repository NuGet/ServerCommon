// Copyright (c) .NET Foundation. All rights reserved. 
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information. 

using System.Collections.Generic;

namespace NuGet.Services.KeyVault
{
    public class KeyVaultConfigurationDictionary : Dictionary<string, string>
    {
        private readonly ISecretInjector _secretInjector;

        public KeyVaultConfigurationDictionary(ISecretInjector secretInjector, IDictionary<string, string> unprocessedArguments)
        {
            _secretInjector = secretInjector;

            foreach (var key in unprocessedArguments.Keys)
            {
                base[key] = unprocessedArguments[key];
            }
        }

        public new string this[string key]
        {
            get
            {
                return _secretInjector.InjectAsync(base[key]).Result;
            }
            set
            {
                base[key] = value;
            }
        }
    }
}
