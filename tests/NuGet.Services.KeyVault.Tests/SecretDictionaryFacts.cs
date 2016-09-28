// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Moq;
using System;
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
            var mockSecretInjector = new Mock<ISecretInjector>();
            mockSecretInjector.Setup(x => x.InjectAsync(It.IsAny<string>())).Returns(Task.FromResult(Secret1.InjectedValue));

            var unprocessedDictionary = new Dictionary<string, string>()
            {
                {Secret1.Key, Secret1.Value}
            };

            var secretDict = CreateSecretDictionary(mockSecretInjector.Object, unprocessedDictionary);

            // Act
            var value1 = secretDict[Secret1.Key];
            var value2 = secretDict[Secret1.Key];

            // Assert
            mockSecretInjector.Verify(x => x.InjectAsync(It.IsAny<string>()), Times.Exactly(2));
            Assert.Equal(Secret1.InjectedValue, value1);
            Assert.Equal(value1, value2);

            // Arrange 2
            mockSecretInjector.Setup(x => x.InjectAsync(It.IsAny<string>())).Returns(Task.FromResult(Secret2.InjectedValue));

            // Act 2
            var value3 = secretDict[Secret1.Key];
            var value4 = secretDict[Secret1.Key];

            // Assert 2
            mockSecretInjector.Verify(x => x.InjectAsync(It.IsAny<string>()), Times.Exactly(4));
            Assert.Equal(Secret2.InjectedValue, value3);
            Assert.Equal(value3, value4);
        }

        [Fact]
        public void HandlesTryGet()
        {
            // Arrange
            var secretDict = CreateSecretDictionary(new List<Secret> { Secret1 }, new List<string> { Secret1.Key });

            // Act
            string value, notFoundValue;
            var valueIsFound = secretDict.TryGetValue(Secret1.Key, out value);
            var notFoundIsFound = secretDict.TryGetValue(Secret2.Key, out notFoundValue);

            // Assert
            Assert.True(valueIsFound);
            Assert.Equal(Secret1.InjectedValue, value);
            Assert.False(notFoundIsFound);
        }

        [Fact]
        public void HandlesEnumerators()
        {
            // Arrange
            var secrets = new List<Secret> {Secret1, Secret2, Secret3};

            var secretDict = CreateSecretDictionary(secrets, secrets.Select(secret => secret.Key).ToList());

            // Act-Assert
            var pairsToVerify = secrets.Select(secret => new KeyValuePair<string, string>(secret.Key, secret.InjectedValue)).ToList();
            foreach (var pair in secretDict)
            {
                Assert.Contains(pair, pairsToVerify);
                pairsToVerify.Remove(pair);
            }
            Assert.Empty(pairsToVerify);
        }

        [Fact]
        public void HandlesKeys()
        {
            // Arrange
            var secrets = new List<Secret> { Secret1, Secret2, Secret3 };

            var secretDict = CreateSecretDictionary(secrets, secrets.Select(secret => secret.Key).ToList());

            // Act-Assert
            var keysToVerify = secrets.Select(secret => secret.Key).ToList();
            foreach (var secretKey in secretDict.Keys)
            {
                Assert.Contains(secretKey, keysToVerify);
                keysToVerify.Remove(secretKey);
            }
            Assert.Empty(keysToVerify);
        }

        [Fact]
        public void HandlesValues()
        {
            // Arrange
            var secrets = new List<Secret> { Secret1, Secret2, Secret3 };

            var secretDict = CreateSecretDictionary(secrets, secrets.Select(secret => secret.Key).ToList());

            // Act-Assert
            var secretsToVerify = secrets.Select(secret => secret.InjectedValue).ToList();
            foreach (var secretValue in secretDict.Values)
            {
                Assert.Contains(secretValue, secretsToVerify);
                secretsToVerify.Remove(secretValue);
            }
            Assert.Empty(secretsToVerify);
        }

        [Fact]
        public void HandlesKeyNotFound()
        {
            const string notFoundKey = "not a real key";
            var dummy = CreateSecretDictionary();
            Assert.Throws<KeyNotFoundException>(() => dummy[notFoundKey]);

            string output;
            Assert.Equal(false, dummy.TryGetValue(notFoundKey, out output));
        }

        [Fact]
        public void HandlesNullArgument()
        {
            // Arrange
            var dummy = CreateSecretDictionary();

            var nullKey = "this key has a null value";
            string nullValue = null;

            // Act
            dummy.Add(nullKey, nullValue);

            // Assert
            Assert.Equal(nullValue, dummy[nullKey]);
        }

        [Fact]
        public void HandlesNullOrEmptyArgument()
        {
            // Arrange
            var dummy = CreateSecretDictionary();
            
            var emptyKey = "this key has an empty value";
            var emptyValue = "";

            // Act
            dummy.Add(emptyKey, emptyValue);

            // Assert
            Assert.Equal(emptyValue, dummy[emptyKey]);
        }

        [Fact]
        public void AddToEmptyDictionary()
        {
            // Arrange
            var secretDict = CreateSecretDictionary(new List<Secret> { Secret1 }, new List<string>());

            // Act
            secretDict.Add(Secret1.Key, Secret1.Value);

            // Assert
            Assert.Equal(1, secretDict.Count);
            Assert.True(secretDict.ContainsKey(Secret1.Key));
            Assert.True(secretDict.Contains(new KeyValuePair<string, string>(Secret1.Key, Secret1.InjectedValue)));
            Assert.False(secretDict.Contains(new KeyValuePair<string, string>(Secret1.Key, Secret1.Value)));

            string tryget;
            Assert.True(secretDict.TryGetValue(Secret1.Key, out tryget));
            Assert.Equal(Secret1.InjectedValue, tryget);

            var result1 = secretDict[Secret1.Key];
            Assert.Equal(Secret1.InjectedValue, result1);

            Assert.Contains(new KeyValuePair<string, string>(Secret1.Key, Secret1.InjectedValue), secretDict);
            Assert.Contains(Secret1.Key, secretDict.Keys);
            Assert.Contains(Secret1.InjectedValue, secretDict.Values);
        }

        [Fact]
        public void AddToNonEmptyDictionary()
        {
            // Arrange
            var secretDict = CreateSecretDictionary(new List<Secret> { Secret1, Secret2 }, new List<string> { Secret1.Key });

            // Act
            secretDict.Add(new KeyValuePair<string, string>(Secret2.Key, Secret2.Value));

            // Assert
            Assert.Equal(2, secretDict.Count);
            Assert.True(secretDict.ContainsKey(Secret2.Key));
            Assert.True(secretDict.Contains(new KeyValuePair<string, string>(Secret2.Key, Secret2.InjectedValue)));
            Assert.False(secretDict.Contains(new KeyValuePair<string, string>(Secret2.Key, Secret2.Value)));

            string tryget;
            Assert.True(secretDict.TryGetValue(Secret2.Key, out tryget));
            Assert.Equal(Secret2.InjectedValue, tryget);

            var result1 = secretDict[Secret2.Key];
            Assert.Equal(Secret2.InjectedValue, result1);

            Assert.Contains(new KeyValuePair<string, string>(Secret2.Key, Secret2.InjectedValue), secretDict);
            Assert.Contains(Secret2.Key, secretDict.Keys);
            Assert.Contains(Secret2.InjectedValue, secretDict.Values);
        }

        [Fact]
        public void AddAlreadyExistingSameValue()
        {
            var secretDict = CreateSecretDictionary(new List<Secret> { Secret1 }, new List<string> { Secret1.Key });
            
            Assert.Throws<ArgumentException>(() => secretDict.Add(Secret1.Key, Secret1.Value));
            Assert.Throws<ArgumentException>(() => secretDict.Add(new KeyValuePair<string, string>(Secret1.Key, Secret1.Value)));
        }

        [Fact]
        public void AddAlreadyExistingDifferentValue()
        {
            var secretDict = CreateSecretDictionary(new List<Secret> { Secret1 }, new List<string> { Secret1.Key });
            
            Assert.Throws<ArgumentException>(() => secretDict.Add(Secret1.Key, Secret1.InjectedValue));
            Assert.Throws<ArgumentException>(() => secretDict.Add(new KeyValuePair<string, string>(Secret1.Key, Secret1.InjectedValue)));
        }

        [Fact]
        public void RemoveNotFoundByKey()
        {
            var secretDict = CreateSecretDictionary(new List<Secret> { Secret1 }, new List<string> { Secret1.Key });

            Assert.False(secretDict.Remove(Secret2.Key));
        }

        [Fact]
        public void RemoveNotFoundByPair()
        {
            var secretDict = CreateSecretDictionary(new List<Secret> { Secret1 }, new List<string> { Secret1.Key });

            Assert.False(secretDict.Remove(new KeyValuePair<string, string>(Secret2.Key, Secret2.Value)));
        }

        [Fact]
        public void RemoveNotFoundByPairWithUninjectedValue()
        {
            var secretDict = CreateSecretDictionary(new List<Secret> { Secret1 }, new List<string> { Secret1.Key });

            Assert.False(secretDict.Remove(new KeyValuePair<string, string>(Secret1.Key, Secret1.Value)));
        }

        [Fact]
        public void RemoveByKey()
        {
            var secretDict = CreateSecretDictionary(new List<Secret> { Secret1, Secret2 }, new List<string> { Secret1.Key, Secret2.Key });

            Assert.True(secretDict.Remove(Secret2.Key));

            Assert.Equal(1, secretDict.Count);
            Assert.False(secretDict.ContainsKey(Secret2.Key));
            Assert.False(secretDict.Contains(new KeyValuePair<string, string>(Secret2.Key, Secret2.InjectedValue)));
            Assert.False(secretDict.Contains(new KeyValuePair<string, string>(Secret2.Key, Secret2.Value)));
            Assert.False(secretDict.Contains(new KeyValuePair<string, string>(Secret2.Key, Secret1.Value)));

            string tryget;
            Assert.False(secretDict.TryGetValue(Secret2.Key, out tryget));

            Assert.Throws<KeyNotFoundException>(() => secretDict[Secret2.Key]);

            Assert.DoesNotContain(new KeyValuePair<string, string>(Secret2.Key, Secret2.InjectedValue), secretDict);
            Assert.DoesNotContain(Secret2.Key, secretDict.Keys);
            Assert.DoesNotContain(Secret2.InjectedValue, secretDict.Values);
        }

        [Fact]
        public void RemoveByPair()
        {
            var secretDict = CreateSecretDictionary(new List<Secret> { Secret1 }, new List<string> { Secret1.Key });

            Assert.True(secretDict.Remove(new KeyValuePair<string, string>(Secret1.Key, Secret1.InjectedValue)));

            Assert.Equal(0, secretDict.Count);
            Assert.False(secretDict.ContainsKey(Secret1.Key));
            Assert.False(secretDict.Contains(new KeyValuePair<string, string>(Secret1.Key, Secret1.InjectedValue)));

            string tryget;
            Assert.False(secretDict.TryGetValue(Secret1.Key, out tryget));

            Assert.Throws<KeyNotFoundException>(() => secretDict[Secret1.Key]);

            Assert.DoesNotContain(new KeyValuePair<string, string>(Secret1.Key, Secret1.InjectedValue), secretDict);
            Assert.DoesNotContain(Secret1.Key, secretDict.Keys);
            Assert.DoesNotContain(Secret1.InjectedValue, secretDict.Values);
        }

        private class Secret
        {
            public string Key { get; }

            public string Value { get; }

            public string InjectedValue { get; }

            public Secret(string key, string value, string injectedValue)
            {
                Key = key;
                Value = value;
                InjectedValue = injectedValue;
            }
        }

        // Constants for tests
        private static Secret Secret1 => new Secret("a", "1", "!");
        private static Secret Secret2 => new Secret("b", "2", "@");
        private static Secret Secret3 => new Secret("c", "3", "#");

        private static IDictionary<string, string> CreateSecretDictionary()
        {
            return new SecretDictionary(new SecretInjector(new EmptySecretReader()), new Dictionary<string, string>());
        }

        private static IDictionary<string, string> CreateSecretDictionary(ISecretInjector secretInjector, IDictionary<string, string> unprocessedArgs)
        {
            return new SecretDictionary(secretInjector, unprocessedArgs);
        }

        private static Mock<ISecretInjector> CreateMappedSecretInjectorMock(IDictionary<string, string> keyToValue)
        {
            var mockSecretInjector = new Mock<ISecretInjector>();
            mockSecretInjector.Setup(x => x.InjectAsync(It.IsAny<string>())).Returns<string>(key => Task.FromResult(keyToValue[key]));
            return mockSecretInjector;
        }

        private static IDictionary<string, string> CreateSecretDictionary(IList<Secret> secretsToMap,
            ICollection<string> secretsToInclude)
        {
            var unprocessedDictionary = secretsToMap
                .Where(secret => secretsToInclude.Contains(secret.Key))
                .ToDictionary(secret => secret.Key, secret => secret.Value);

            var valueToInjectedValue = secretsToMap.ToDictionary(secret => secret.Value, secret => secret.InjectedValue);

            var mockSecretInjector = CreateMappedSecretInjectorMock(valueToInjectedValue);

            return CreateSecretDictionary(mockSecretInjector.Object, unprocessedDictionary);
        }
    }
}
