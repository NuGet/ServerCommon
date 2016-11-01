﻿// Copyright (c) .NET Foundation. All rights reserved.
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
    public class SecretConfigurationProviderFacts
    {
        [Fact]
        public void HandlesSyncCalledFirst()
        {
            // Arrange
            const string secretName = "hello i'm a secret";
            const string secretKey = "fetch me from KeyVault pls";
            const string secretValue = "oohmysterious";

            var mockSecretInjector = new Mock<ISecretInjector>();
            mockSecretInjector.Setup(x => x.InjectAsync(It.Is<string>(v => v == secretKey))).Returns(Task.FromResult(secretValue));

            var arguments = new Dictionary<string, string>
            {
                {secretName, secretKey}
            };

            var configProvider = new SecretConfigurationProvider(mockSecretInjector.Object, arguments);

            // Act
            var value = configProvider.GetOrThrowSync<string>(secretName);

            // Assert
            Assert.Equal(secretValue, value);
        }

        [Fact]
        public async void RefreshesArgumentsIfKeyVaultChanges()
        {
            // Arrange
            const string secretName = "hello i'm a secret";
            const string secretKey = "fetch me from KeyVault pls";
            const string firstSecret = "secret1";
            const string secondSecret = "secret2";

            var mockSecretInjector = new Mock<ISecretInjector>();
            mockSecretInjector.Setup(x => x.InjectAsync(It.IsAny<string>())).Returns(Task.FromResult(firstSecret));

            var arguments = new Dictionary<string, string>
            {
                {secretName, secretKey}
            };

            var configProvider = new SecretConfigurationProvider(mockSecretInjector.Object, arguments);

            // Act
            var value1 = await configProvider.GetOrThrow<string>(secretName);
            var value2 = configProvider.GetOrThrowSync<string>(secretName);
            var value3 = await configProvider.GetOrDefault<string>(secretName);
            var value4 = configProvider.GetOrDefaultSync<string>(secretName);

            // Assert
            mockSecretInjector.Verify(x => x.InjectAsync(It.IsAny<string>()));
            Assert.Equal(firstSecret, value1);
            Assert.Equal(value1, value2);
            Assert.Equal(value1, value3);
            Assert.Equal(value1, value4);

            // Arrange 2
            mockSecretInjector.Setup(x => x.InjectAsync(It.IsAny<string>())).Returns(Task.FromResult(secondSecret));

            // Act 2
            value1 = await configProvider.GetOrThrow<string>(secretName);
            value2 = configProvider.GetOrThrowSync<string>(secretName);
            value3 = await configProvider.GetOrDefault<string>(secretName);
            value4 = configProvider.GetOrDefaultSync<string>(secretName);

            // Assert 2
            mockSecretInjector.Verify(x => x.InjectAsync(It.IsAny<string>()));
            Assert.Equal(secondSecret, value1);
            Assert.Equal(value1, value2);
            Assert.Equal(value1, value3);
            Assert.Equal(value1, value4);
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

            var getOrThrowMethod = typeof(ISettingsProvider).GetMethod("GetOrThrow").MakeGenericMethod(type);
            var getOrDefaultMethod = typeof(ISettingsProvider).GetMethod("GetOrDefault").MakeGenericMethod(type);
            var getOrThrowSyncMethod = typeof(IConfigurationProvider).GetMethod("GetOrThrowSync").MakeGenericMethod(type);
            var getOrDefaultSyncMethod = typeof(IConfigurationProvider).GetMethod("GetOrDefaultSync").MakeGenericMethod(type);

            var defaultOfType = GetDefault(type);
            var memberOfType = _typeToObject[type];

            var notFoundKey = "this key is not found";
            object[] notFoundKeyThrowArgs = { notFoundKey };
            object[] notFoundKeyDefaultArgs = { notFoundKey, null };
            object[] notFoundKeyDefaultSpecifiedArgs = { notFoundKey, memberOfType };

            ////// Act and Assert

            // GetOrThrow
            await Assert.ThrowsAsync<KeyNotFoundException>(() => (Task)getOrThrowMethod.Invoke(dummy, notFoundKeyThrowArgs));
            Assert.Throws<TargetInvocationException>(() => getOrThrowSyncMethod.Invoke(dummy, notFoundKeyThrowArgs));
            // GetOrDefault
            Assert.Equal(defaultOfType, await (dynamic)getOrDefaultMethod.Invoke(dummy, notFoundKeyDefaultArgs));
            Assert.Equal(defaultOfType, (dynamic)getOrDefaultSyncMethod.Invoke(dummy, notFoundKeyDefaultArgs));
            // GetOrDefault with default specified
            Assert.Equal(memberOfType, await (dynamic)getOrDefaultMethod.Invoke(dummy, notFoundKeyDefaultSpecifiedArgs));
            Assert.Equal(memberOfType, (dynamic)getOrDefaultSyncMethod.Invoke(dummy, notFoundKeyDefaultSpecifiedArgs));
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

            var mockSecretInjector = new Mock<ISecretInjector>();
            mockSecretInjector.Setup(x => x.InjectAsync(It.IsAny<string>())).Returns<string>(Task.FromResult);

            var arguments = new Dictionary<string, string>
            {
                {nullKey, null},
                {emptyKey, "" }
            };

            var configProvider = new SecretConfigurationProvider(mockSecretInjector.Object, arguments);

            var getOrThrowMethod = typeof(ISettingsProvider).GetMethod("GetOrThrow").MakeGenericMethod(type);
            var getOrDefaultMethod = typeof(ISettingsProvider).GetMethod("GetOrDefault").MakeGenericMethod(type);
            var getOrThrowSyncMethod = typeof(IConfigurationProvider).GetMethod("GetOrThrowSync").MakeGenericMethod(type);
            var getOrDefaultSyncMethod = typeof(IConfigurationProvider).GetMethod("GetOrDefaultSync").MakeGenericMethod(type);

            ////// Act and Assert

            //// Null Key

            // GetOrThrow
            await Assert.ThrowsAsync<ArgumentNullException>(() => (Task)getOrThrowMethod.Invoke(configProvider, nullKeyThrowArgs));
            Assert.Throws<TargetInvocationException>(() => getOrThrowSyncMethod.Invoke(configProvider, nullKeyThrowArgs));
            // GetOrDefault
            Assert.Equal(defaultOfType, await (dynamic)getOrDefaultMethod.Invoke(configProvider, nullKeyDefaultArgs));
            Assert.Equal(defaultOfType, (dynamic)getOrDefaultSyncMethod.Invoke(configProvider, nullKeyDefaultArgs));
            // GetOrDefault with default specified
            Assert.Equal(memberOfType, await (dynamic)getOrDefaultMethod.Invoke(configProvider, nullKeyDefaultSpecifiedArgs));
            Assert.Equal(memberOfType, (dynamic)getOrDefaultSyncMethod.Invoke(configProvider, nullKeyDefaultSpecifiedArgs));

            //// Empty Key

            // GetOrThrow
            await Assert.ThrowsAsync<ArgumentNullException>(() => (Task)getOrThrowMethod.Invoke(configProvider, emptyKeyThrowArgs));
            Assert.Throws<TargetInvocationException>(() => getOrThrowSyncMethod.Invoke(configProvider, emptyKeyThrowArgs));
            // GetOrDefault
            Assert.Equal(defaultOfType, await (dynamic)getOrDefaultMethod.Invoke(configProvider, emptyKeyDefaultArgs));
            Assert.Equal(defaultOfType, (dynamic)getOrDefaultSyncMethod.Invoke(configProvider, emptyKeyDefaultArgs));
            // GetOrDefault with default specified
            Assert.Equal(memberOfType, await (dynamic)getOrDefaultMethod.Invoke(configProvider, emptyKeyDefaultSpecifiedArgs));
            Assert.Equal(memberOfType, (dynamic)getOrDefaultSyncMethod.Invoke(configProvider, emptyKeyDefaultSpecifiedArgs));
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
            const string secretValue = "oohmysterious";

            var mockSecretInjector = new Mock<ISecretInjector>();
            mockSecretInjector.Setup(x => x.InjectAsync(It.IsAny<string>())).Returns(Task.FromResult(secretValue));

            var arguments = new Dictionary<string, string>
            {
                {secretName, secretKey}
            };

            var configProvider = new SecretConfigurationProvider(mockSecretInjector.Object, arguments);

            // Assert
            await Assert.ThrowsAsync<NotSupportedException>(
                async () => await configProvider.GetOrThrow<NoConversionFromStringToThisClass>(secretName));
            Assert.Throws<NotSupportedException>(
                () => configProvider.GetOrThrowSync<NoConversionFromStringToThisClass>(secretName));
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

        private static SecretConfigurationProvider CreateDummyConfigProvider()
        {
            return new SecretConfigurationProvider(new SecretInjector(new EmptySecretReader()), new Dictionary<string, string>());
        }
    }
}