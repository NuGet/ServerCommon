// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace NuGet.Services.Configuration
{
    using Extensions = Microsoft.Extensions.Configuration;

    public class DictionaryConfigurationSource : IConfigurationSource
    {
        private readonly IReadOnlyDictionary<string, string> _dictionary;

        public DictionaryConfigurationSource(IReadOnlyDictionary<string, string> dictionary)
        {
            _dictionary = dictionary;
        }

        public Extensions.IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new DictionaryConfigurationProvider(_dictionary);
        }
    }
}
