using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NuGet.Services.Configuration
{
    public class ConfigurationNullOrEmptyException : Exception
    {
        public string Key { get; }

        public ConfigurationNullOrEmptyException()
        {
        }

        public ConfigurationNullOrEmptyException(string key)
            : base(GetMessageFromKey(key))
        {
            Key = key;
        }

        private static string GetMessageFromKey(string key)
        {
            return $"The configuration value associated with key {key} is null or empty.";
        }
    }
}
