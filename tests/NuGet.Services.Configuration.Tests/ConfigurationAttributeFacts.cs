using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace NuGet.Services.Configuration.Tests
{
    public class ConfigurationAttributeFacts
    {
        [Fact]
        public void ConfigurationKeyAttributeThrowsWhenKeyNull()
        {
            Assert.Throws<ArgumentNullException>(() => new ConfigurationKeyAttribute(null));
        }

        [Fact]
        public void ConfigurationKeyPrefixAttributeThrowsWhenPrefixNull()
        {
            Assert.Throws<ArgumentNullException>(() => new ConfigurationKeyPrefixAttribute(null));
        }

        [Fact]
        public void ConfigurationKeyAttributeThrowsWhenKeyEmpty()
        {
            Assert.Throws<ArgumentNullException>(() => new ConfigurationKeyAttribute(""));
        }

        [Fact]
        public void ConfigurationKeyPrefixAttributeThrowsWhenPrefixEmpty()
        {
            Assert.Throws<ArgumentNullException>(() => new ConfigurationKeyPrefixAttribute(""));
        }
    }
}
