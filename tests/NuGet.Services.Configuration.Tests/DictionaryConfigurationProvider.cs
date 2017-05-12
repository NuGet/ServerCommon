// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NuGet.Services.Configuration.Tests
{
    public class DictionaryConfigurationProvider : ConfigurationProvider
    {
        private readonly IDictionary<string, string> _configuration;

        public DictionaryConfigurationProvider(IDictionary<string, string> configuration)
        {
            _configuration = configuration;
        }

        public DictionaryConfigurationProvider(IDictionary<string, object> configuration)
        {
            _configuration = configuration.Where(tuple => tuple.Value != null)
                .ToDictionary(tuple => tuple.Key, tuple => tuple.Value.ToString());
        }

        protected override Task<string> Get(string key)
        {
            return Task.FromResult(_configuration[key]);
        }
    }
}
