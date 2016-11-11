using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NuGet.Services.Configuration.Tests
{
    public class TestConfigurationProvider : ConfigurationProvider
    {
        private readonly IDictionary<string, string> _configuration;

        public TestConfigurationProvider(IDictionary<string, string> configuration)
        {
            _configuration = configuration;
        }

        public TestConfigurationProvider(IDictionary<string, object> configuration)
        {
            _configuration = configuration.Where(tuple => tuple.Value != null)
                .ToDictionary(tuple => tuple.Key, tuple => tuple.Value.ToString());
        }

        protected override Task<string> Get(string key)
        {
            string value;

            if (!_configuration.TryGetValue(key, out value))
            {
                throw new KeyNotFoundException();
            }

            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException();
            }

            return Task.FromResult(value);
        }
    }
}
