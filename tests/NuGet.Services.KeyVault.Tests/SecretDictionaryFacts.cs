// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace NuGet.Services.KeyVault.Tests
{
    public class SecretDictionaryFacts
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

            var configDict = CreateConfigurationDictionary(mockSecretInjector.Object, unprocessedDictionary);

            // Act
            var value1 = configDict[nameOfSecret];
            var value2 = configDict[nameOfSecret];

            // Assert
            mockSecretInjector.Verify(x => x.InjectAsync(It.IsAny<string>()), Times.Exactly(2));
            Assert.Equal(firstSecret, value1);
            Assert.Equal(value1, value2);

            // Arrange 2
            mockSecretInjector.Setup(x => x.InjectAsync(It.IsAny<string>())).Returns(Task.FromResult(secondSecret));

            // Act 2
            var value3 = configDict[nameOfSecret];
            var value4 = configDict[nameOfSecret];

            // Assert 2
            mockSecretInjector.Verify(x => x.InjectAsync(It.IsAny<string>()), Times.Exactly(4));
            Assert.Equal(secondSecret, value3);
            Assert.Equal(value3, value4);
        }

        [Fact]
        public void HandlesTryGet()
        {
            // Arrange
            const string secretKey = "hello i'm a secret";
            const string secretValue = "fetch me from KeyVault pls";
            const string secretInjected = "secret1";
            const string notFoundKey = "i'm not here!";

            var mockSecretInjector = new Mock<ISecretInjector>();
            mockSecretInjector.Setup(x => x.InjectAsync(It.IsAny<string>())).Returns(Task.FromResult(secretInjected));

            var unprocessedDictionary = new Dictionary<string, string>()
            {
                {secretKey, secretValue}
            };

            var configDict = CreateConfigurationDictionary(mockSecretInjector.Object, unprocessedDictionary);

            // Act
            string value, notFoundValue;
            var valueIsFound = configDict.TryGetValue(secretKey, out value);
            var notFoundIsFound = configDict.TryGetValue(notFoundKey, out notFoundValue);

            // Assert
            mockSecretInjector.Verify(x => x.InjectAsync(It.IsAny<string>()), Times.Once);
            Assert.True(valueIsFound);
            Assert.Equal(secretInjected, value);
            Assert.False(notFoundIsFound);
        }

        [Fact]
        public void WorksWithEnumerator()
        {
            // Arrange
            const string secretKey1 = "hello i'm a secret";
            const string secret1 = "secret1";
            const string secret1Value = "i was injected!";

            const string secretKey2 = "hello i'm another secret";
            const string secret2 = "secret2";
            const string secret2Value = "i was injected too!";

            const string secretKey3 = "third secret's the charm";
            const string secret3 = "secret3";
            const string secret3Value = "i was thirdly injected!";

            var unprocessedDictionary = new Dictionary<string, string>()
            {
                {secretKey1, secret1},
                {secretKey2, secret2},
                {secretKey3, secret3}
            };

            var injectKeyToValue = new Dictionary<string, string>()
            {
                {secret1, secret1Value},
                {secret2, secret2Value},
                {secret3, secret3Value}
            };

            var mockSecretInjector = new Mock<ISecretInjector>();
            mockSecretInjector.Setup(x => x.InjectAsync(It.IsAny<string>())).Returns<string>(input => Task.FromResult(injectKeyToValue[input]));

            var configDict = CreateConfigurationDictionary(mockSecretInjector.Object, unprocessedDictionary);

            var pairsToVerify = unprocessedDictionary.Select(pair => new KeyValuePair<string, string>(pair.Key, injectKeyToValue[pair.Value])).ToList();
            foreach (var pair in configDict)
            {
                Assert.Contains(pair, pairsToVerify);
            }
        }

        [Fact]
        public void HandlesKeyNotFound()
        {
            const string notFoundKey = "not a real key";
            var dummy = CreateDummyConfigurationDictionary();
            Assert.Throws<KeyNotFoundException>(() => dummy[notFoundKey]);

            string output;
            Assert.Equal(false, dummy.TryGetValue(notFoundKey, out output));
        }

        [Fact]
        public void HandlesNullOrEmptyArgument()
        {
            // Arrange
            var dummy = CreateDummyConfigurationDictionary();

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

        private static IDictionary<string, string> CreateDummyConfigurationDictionary()
        {
            return new SecretDictionary(new SecretInjector(new EmptySecretReader()), new Dictionary<string, string>());
        }

        private static IDictionary<string, string> CreateConfigurationDictionary(ISecretInjector secretInjector, IDictionary<string, string> unprocessedArgs)
        {
            return new SecretDictionary(secretInjector, unprocessedArgs);
        }
    }
}
