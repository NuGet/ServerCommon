// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace NuGet.Services.Configuration
{
    using Extensions = Microsoft.Extensions.Configuration;

    public class DictionaryConfigurationProvider : Extensions.ConfigurationProvider
    {
        private IDictionary<string, string> _dictionary;

        public DictionaryConfigurationProvider(IReadOnlyDictionary<string, string> dictionary)
        {
            _dictionary = (IDictionary<string, string>)dictionary ?? throw new ArgumentNullException(nameof(dictionary));
        }

        public override void Load()
        {
            Data = _dictionary.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }
    }
}
