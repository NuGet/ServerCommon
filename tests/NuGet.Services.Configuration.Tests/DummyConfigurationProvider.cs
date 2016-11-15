using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NuGet.Services.Configuration.Tests
{
    /// <summary>
    /// Returns "test value" for all configuration keys unless the given key is null or empty.
    /// </summary>
    public class DummyConfigurationProvider : ConfigurationProvider
    {
        protected override Task<string> Get(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(key);
            }

            return Task.FromResult("test value");
        }
    }
}
