using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NuGet.Services.Configuration
{
    /// <summary>
    /// Used to specify a configuration key in a <see cref="Configuration"/> subclass while using a <see cref="ConfigurationFactory"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ConfigurationKeyAttribute : Attribute
    {
        public ConfigurationKeyAttribute(string key)
        {
            Key = key;
        }

        /// <summary>
        /// The configuration key to pass into the <see cref="IConfigurationProvider"/> used by the <see cref="ConfigurationFactory"/>.
        /// </summary>
        public string Key { get; }
    }
}
