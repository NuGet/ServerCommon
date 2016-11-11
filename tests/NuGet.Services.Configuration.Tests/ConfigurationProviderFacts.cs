// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Moq;
using NuGet.Services.KeyVault;
using Xunit;

namespace NuGet.Services.Configuration.Tests
{
    public class ConfigurationProviderFacts
    {
        [Fact]
        public async void HandlesConfigurationChanges()
        {
            // Arrange
            const string secretName = "hello i'm a secret";
            const string firstSecret = "secret1";
            const string secondSecret = "secret2";
            
            var arguments = new Dictionary<string, string>
            {
                {secretName, firstSecret}
            };

            ConfigurationProvider configProvider = new TestConfigurationProvider(arguments);

            // Act
            var value1 = await configProvider.GetOrThrowAsync<string>(secretName);
            var value2 = await configProvider.GetOrDefaultAsync<string>(secretName);

            // Assert
            Assert.Equal(firstSecret, value1);
            Assert.Equal(value1, value2);

            // Arrange 2
            arguments[secretName] = secondSecret;

            // Act 2
            value1 = await configProvider.GetOrThrowAsync<string>(secretName);
            value2 = await configProvider.GetOrDefaultAsync<string>(secretName);

            // Assert 2
            Assert.Equal(secondSecret, value1);
            Assert.Equal(value1, value2);
        }

        [Theory]
        [InlineData(typeof(string))]
        [InlineData(typeof(int))]
        [InlineData(typeof(bool))]
        [InlineData(typeof(DateTime))]
        public async void HandlesKeyNotFound(Type type)
        {
            // Arrange
            var dummy = CreateDummyConfigProvider();

            var getOrThrowMethod = typeof(IConfigurationProvider).GetMethod(nameof(IConfigurationProvider.GetOrThrowAsync)).MakeGenericMethod(type);
            var getOrDefaultMethod = typeof(IConfigurationProvider).GetMethod(nameof(IConfigurationProvider.GetOrDefaultAsync)).MakeGenericMethod(type);

            var defaultOfType = GetDefault(type);
            var memberOfType = _typeToObject[type];

            var notFoundKey = "this key is not found";
            object[] notFoundKeyThrowArgs = { notFoundKey };
            object[] notFoundKeyDefaultArgs = { notFoundKey, null };
            object[] notFoundKeyDefaultSpecifiedArgs = { notFoundKey, memberOfType };

            ////// Act and Assert

            // GetOrThrow
            await Assert.ThrowsAsync<KeyNotFoundException>(() => (Task)getOrThrowMethod.Invoke(dummy, notFoundKeyThrowArgs));
            // GetOrDefault
            Assert.Equal(defaultOfType, await (dynamic)getOrDefaultMethod.Invoke(dummy, notFoundKeyDefaultArgs));
            // GetOrDefault with default specified
            Assert.Equal(memberOfType, await (dynamic)getOrDefaultMethod.Invoke(dummy, notFoundKeyDefaultSpecifiedArgs));
        }

        [Theory]
        [InlineData(typeof(string))]
        [InlineData(typeof(int))]
        [InlineData(typeof(bool))]
        [InlineData(typeof(DateTime))]
        public async void HandlesNullOrEmptyArgument(Type type)
        {
            // Arrange
            var defaultOfType = GetDefault(type);
            var memberOfType = _typeToObject[type];

            var nullKey = "this key has a null value";
            object[] nullKeyThrowArgs = { nullKey };
            object[] nullKeyDefaultArgs = { nullKey, null };
            object[] nullKeyDefaultSpecifiedArgs = { nullKey, memberOfType };

            var emptyKey = "this key has an empty value";
            object[] emptyKeyThrowArgs = { emptyKey };
            object[] emptyKeyDefaultArgs = { emptyKey, null };
            object[] emptyKeyDefaultSpecifiedArgs = { emptyKey, memberOfType };
            
            var arguments = new Dictionary<string, string>
            {
                {nullKey, null},
                {emptyKey, "" }
            };

            ConfigurationProvider configProvider = new TestConfigurationProvider(arguments);

            var getOrThrowMethod = typeof(IConfigurationProvider).GetMethod(nameof(IConfigurationProvider.GetOrThrowAsync)).MakeGenericMethod(type);
            var getOrDefaultMethod = typeof(IConfigurationProvider).GetMethod(nameof(IConfigurationProvider.GetOrDefaultAsync)).MakeGenericMethod(type);

            ////// Act and Assert

            //// Null Key

            // GetOrThrow
            await Assert.ThrowsAsync<ArgumentNullException>(() => (Task)getOrThrowMethod.Invoke(configProvider, nullKeyThrowArgs));
            // GetOrDefault
            Assert.Equal(defaultOfType, await (dynamic)getOrDefaultMethod.Invoke(configProvider, nullKeyDefaultArgs));
            // GetOrDefault with default specified
            Assert.Equal(memberOfType, await (dynamic)getOrDefaultMethod.Invoke(configProvider, nullKeyDefaultSpecifiedArgs));

            //// Empty Key

            // GetOrThrow
            await Assert.ThrowsAsync<ArgumentNullException>(() => (Task)getOrThrowMethod.Invoke(configProvider, emptyKeyThrowArgs));
            // GetOrDefault
            Assert.Equal(defaultOfType, await (dynamic)getOrDefaultMethod.Invoke(configProvider, emptyKeyDefaultArgs));
            // GetOrDefault with default specified
            Assert.Equal(memberOfType, await (dynamic)getOrDefaultMethod.Invoke(configProvider, emptyKeyDefaultSpecifiedArgs));
        }
        
        private class NoConversionFromStringToThisClass
        {
        }

        [Fact]
        public async void ThrowsNotFoundException()
        {
            // Arrange
            const string secretName = "hello i'm a secret";
            const string secretKey = "fetch me from KeyVault pls";
            
            var arguments = new Dictionary<string, string>
            {
                {secretName, secretKey}
            };

            ConfigurationProvider configProvider = new TestConfigurationProvider(arguments);

            // Assert
            await Assert.ThrowsAsync<NotSupportedException>(
                async () => await configProvider.GetOrThrowAsync<NoConversionFromStringToThisClass>(secretName));
        }

        public dynamic GetDefault(Type t)
        {
            return GetType().GetMethod(nameof(GetDefaultGeneric)).MakeGenericMethod(t).Invoke(this, null);
        }

        public T GetDefaultGeneric<T>()
        {
            return default(T);
        }

        /// <summary>
        /// Used in some tests to create a member of a type.
        /// </summary>
        private readonly IDictionary<Type, object> _typeToObject = new Dictionary<Type, object>
        {
            { typeof(string), "this is a string" },
            { typeof(int), 1234 },
            { typeof(bool), true },
            { typeof(DateTime), DateTime.Now }
        };

        private static ConfigurationProvider CreateDummyConfigProvider()
        {
            return new TestConfigurationProvider(new Dictionary<string, string>());
        }
    }
}