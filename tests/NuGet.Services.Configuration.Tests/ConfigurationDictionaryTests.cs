// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Moq;
using NuGet.Services.KeyVault;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace NuGet.Services.Configuration.Tests
{
    public class ConfigurationDictionaryTests
    {
        [Fact]
        public void RefreshesSecretWhenItChanges()
        {
            // Arrange
            const string nameOfSecret = "hello i'm a secret";
            const string firstSecret = "secret1";
            const string secondSecret = "secret2";
            
            var mockSecretInjector = new Mock<ISecretInjector>();
            mockSecretInjector.Setup(x => x.InjectAsync(It.IsAny<string>())).Returns(Task.FromResult(firstSecret));

            var unprocessedDictionary = new Dictionary<string, string>()
            {
                {nameOfSecret, "fetch me from KeyVault pls"}
            };

            var configDict = new ConfigurationDictionary(mockSecretInjector.Object, unprocessedDictionary);

            // Act
            string value1 = configDict[nameOfSecret];
            value1 = configDict[nameOfSecret];

            // Assert
            mockSecretInjector.Verify(x => x.InjectAsync(It.IsAny<string>()), Times.Exactly(2));
            Assert.Equal(firstSecret, value1);

            // Arrange 2
            mockSecretInjector.Setup(x => x.InjectAsync(It.IsAny<string>())).Returns(Task.FromResult(secondSecret));

            // Act 2
            string value2 = configDict[nameOfSecret];
            value2 = configDict[nameOfSecret];

            // Assert 2
            mockSecretInjector.Verify(x => x.InjectAsync(It.IsAny<string>()), Times.Exactly(4));
            Assert.Equal(secondSecret, value2);
        }

        [Fact]
        public void HandlesKeyNotFound()
        {
            var fakeKey = "not a real key";
            ConfigurationDictionary dummy = CreateDummyConfigurationDictionary();
            Assert.Throws<KeyNotFoundException>(() => dummy[fakeKey]);
        }

        [Fact]
        public void HandlesNullOrEmptyArgument()
        {
            // Arrange
            ConfigurationDictionary dummy = CreateDummyConfigurationDictionary();

            var nullKey = "this key has a null value";
            string nullValue = null;
            dummy.Add(nullKey, nullValue);

            var emptyKey = "this key has an empty value";
            var emptyValue = "";
            dummy.Add(emptyKey, emptyValue);

            // Act and Assert
            Assert.Equal(nullValue, dummy[nullKey]);
            Assert.Equal(emptyValue, dummy[emptyKey]);
        }

        private ConfigurationDictionary CreateDummyConfigurationDictionary()
        {
            return new ConfigurationDictionary(new SecretInjector(new EmptySecretReader()), new Dictionary<string, string>());
        }
    }
}
