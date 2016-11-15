using System;
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
            return Task.FromResult("test value");
        }
    }
}
