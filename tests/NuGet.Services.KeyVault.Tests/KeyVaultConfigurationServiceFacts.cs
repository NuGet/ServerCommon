// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Moq;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace NuGet.Services.KeyVault.Tests
{
    public class KeyVaultConfigurationServiceFacts
    {
        [Fact]
        public void HandlesSyncCalledFirst()
        {
            // Arrange
            const string secretName = "hello i'm a secret";
            const string secretKey = "fetch me from KeyVault pls";
            const string secretValue = "oohmysterious";

            const string secretName2 = "hello i'm another secret";
            const string secretKey2 = "fetch me from KeyVault too pls";
            const string secretValue2 = "oohevenmoremysterious";

            var mockSecretInjector = new Mock<ISecretInjector>();
            mockSecretInjector.Setup(x => x.InjectAsync(It.Is<string>(v => v == secretKey))).Returns(Task.FromResult(secretValue));
            mockSecretInjector.Setup(x => x.InjectAsync(It.Is<string>(v => v == secretKey2))).Returns(Task.FromResult(secretValue2));

            var arguments = new Dictionary<string, string>()
            {
                {secretName, secretKey}
            };

            var configService = new KeyVaultConfigurationService(mockSecretInjector.Object, arguments);

            // Act
            var value = configService.GetOrThrowSync<string>(secretName);

            // Assert
            Assert.Equal(secretValue, value);

            // Arrange 2
            configService.Add(secretName2, secretKey2);
            // Wait for the cache to update.
            Thread.Sleep(100);
            
            // Act 2
            var value2 = configService.GetOrDefaultSync<string>(secretName2);

            // Assert
            Assert.Equal(secretValue2, value2);
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

            var arguments = new Dictionary<string, string>()
            {
                {secretName, secretKey}
            };

            var configService = new KeyVaultConfigurationService(mockSecretInjector.Object, arguments);

            // Act
            var value1 = await configService.GetOrThrow<string>(secretName);
            var value2 = configService.GetOrThrowSync<string>(secretName);
            var value3 = await configService.GetOrDefault<string>(secretName);
            var value4 = configService.GetOrDefaultSync<string>(secretName);

            // Assert
            mockSecretInjector.Verify(x => x.InjectAsync(It.IsAny<string>()));
            Assert.Equal(firstSecret, value1);
            Assert.Equal(value1, value2);
            Assert.Equal(value1, value3);
            Assert.Equal(value1, value4);

            // Arrange 2
            mockSecretInjector.Setup(x => x.InjectAsync(It.IsAny<string>())).Returns(Task.FromResult(secondSecret));

            // Act 2
            value1 = await configService.GetOrThrow<string>(secretName);
            value2 = configService.GetOrThrowSync<string>(secretName);
            value3 = await configService.GetOrDefault<string>(secretName);
            value4 = configService.GetOrDefaultSync<string>(secretName);

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
            var dummy = CreateDummyConfigService();

            var getOrThrowMethod = typeof(IKeyVaultConfigurationService).GetMethod("GetOrThrow").MakeGenericMethod(type);
            var getOrDefaultMethod = typeof(IKeyVaultConfigurationService).GetMethod("GetOrDefault").MakeGenericMethod(type);
            var getOrThrowSyncMethod = typeof(IKeyVaultConfigurationService).GetMethod("GetOrThrowSync").MakeGenericMethod(type);
            var getOrDefaultSyncMethod = typeof(IKeyVaultConfigurationService).GetMethod("GetOrDefaultSync").MakeGenericMethod(type);
            
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

            /*
            // GetOrThrow throws KeyNotFoundException
            await Assert.ThrowsAsync<KeyNotFoundException>(() => dummy.GetOrThrow<string>(fakeKey));
            await Assert.ThrowsAsync<KeyNotFoundException>(() => dummy.GetOrThrow<int>(fakeKey));
            await Assert.ThrowsAsync<KeyNotFoundException>(() => dummy.GetOrThrow<bool>(fakeKey));
            await Assert.ThrowsAsync<KeyNotFoundException>(() => dummy.GetOrThrow<DateTime>(fakeKey));
            Assert.Throws<KeyNotFoundException>(() => dummy.GetOrThrowSync<string>(fakeKey));
            Assert.Throws<KeyNotFoundException>(() => dummy.GetOrThrowSync<int>(fakeKey));
            Assert.Throws<KeyNotFoundException>(() => dummy.GetOrThrowSync<bool>(fakeKey));
            Assert.Throws<KeyNotFoundException>(() => dummy.GetOrThrowSync<DateTime>(fakeKey));

            // GetOrDefault returns default(type)
            Assert.Equal(default(string), await dummy.GetOrDefault<string>(fakeKey));
            Assert.Equal(default(int), await dummy.GetOrDefault<int>(fakeKey));
            Assert.Equal(default(bool), await dummy.GetOrDefault<bool>(fakeKey));
            Assert.Equal(default(DateTime), await dummy.GetOrDefault<DateTime>(fakeKey));

            // GetOrDefault returns default passed in
            var randomDefaultString = "i'm a string";
            Assert.Equal(randomDefaultString, await dummy.GetOrDefault<string>(fakeKey, randomDefaultString));
            var randomDefaultInt = 58;
            Assert.Equal(randomDefaultInt, await dummy.GetOrDefault<int>(fakeKey, randomDefaultInt));
            var randomDefaultBool = !default(bool);
            Assert.Equal(randomDefaultBool, await dummy.GetOrDefault<bool>(fakeKey, randomDefaultBool));
            var randomDefaultDateTime = DateTime.Now.Subtract(new TimeSpan(1, 0, 0));
            Assert.Equal(randomDefaultDateTime, await dummy.GetOrDefault<DateTime>(fakeKey, randomDefaultDateTime));
            */
        }
        
        [Theory]
        [InlineData(typeof(string))]
        [InlineData(typeof(int))]
        [InlineData(typeof(bool))]
        [InlineData(typeof(DateTime))]
        public async void HandlesNullOrEmptyArgument(Type type)
        {
            // Arrange
            var dummy = CreateDummyConfigService();

            var getOrThrowMethod = typeof(IKeyVaultConfigurationService).GetMethod("GetOrThrow").MakeGenericMethod(type);
            var getOrDefaultMethod = typeof(IKeyVaultConfigurationService).GetMethod("GetOrDefault").MakeGenericMethod(type);
            var getOrThrowSyncMethod = typeof(IKeyVaultConfigurationService).GetMethod("GetOrThrowSync").MakeGenericMethod(type);
            var getOrDefaultSyncMethod = typeof(IKeyVaultConfigurationService).GetMethod("GetOrDefaultSync").MakeGenericMethod(type);
            
            var defaultOfType = GetDefault(type);
            var memberOfType = _typeToObject[type];

            var nullKey = "this key has a null value";
            dummy.Add(nullKey, null);
            object[] nullKeyThrowArgs = { nullKey };
            object[] nullKeyDefaultArgs = { nullKey, null };
            object[] nullKeyDefaultSpecifiedArgs = { nullKey, memberOfType };

            var emptyKey = "this key has an empty value";
            dummy.Add(emptyKey, "");
            object[] emptyKeyThrowArgs = { emptyKey };
            object[] emptyKeyDefaultArgs = { emptyKey, null };
            object[] emptyKeyDefaultSpecifiedArgs = { emptyKey, memberOfType };

            ////// Act and Assert
            
            //// Null Key

            // GetOrThrow
            await Assert.ThrowsAsync<ArgumentNullException>(() => (Task)getOrThrowMethod.Invoke(dummy, nullKeyThrowArgs));
            Assert.Throws<TargetInvocationException>(() => getOrThrowSyncMethod.Invoke(dummy, nullKeyThrowArgs));
            // GetOrDefault
            Assert.Equal(defaultOfType, await (dynamic)getOrDefaultMethod.Invoke(dummy, nullKeyDefaultArgs));
            Assert.Equal(defaultOfType, (dynamic)getOrDefaultSyncMethod.Invoke(dummy, nullKeyDefaultArgs));
            // GetOrDefault with default specified
            Assert.Equal(memberOfType, await (dynamic)getOrDefaultMethod.Invoke(dummy, nullKeyDefaultSpecifiedArgs));
            Assert.Equal(memberOfType, (dynamic)getOrDefaultSyncMethod.Invoke(dummy, nullKeyDefaultSpecifiedArgs));

            //// Empty Key
            
            // GetOrThrow
            await Assert.ThrowsAsync<ArgumentNullException>(() => (Task)getOrThrowMethod.Invoke(dummy, emptyKeyThrowArgs));
            Assert.Throws<TargetInvocationException>(() => getOrThrowSyncMethod.Invoke(dummy, emptyKeyThrowArgs));
            // GetOrDefault
            Assert.Equal(defaultOfType, await (dynamic)getOrDefaultMethod.Invoke(dummy, emptyKeyDefaultArgs));
            Assert.Equal(defaultOfType, (dynamic)getOrDefaultSyncMethod.Invoke(dummy, emptyKeyDefaultArgs));
            // GetOrDefault with default specified
            Assert.Equal(memberOfType, await (dynamic)getOrDefaultMethod.Invoke(dummy, emptyKeyDefaultSpecifiedArgs));
            Assert.Equal(memberOfType, (dynamic)getOrDefaultSyncMethod.Invoke(dummy, emptyKeyDefaultSpecifiedArgs));
        }

        public dynamic GetDefault(Type t)
        {
            return GetType().GetMethod("GetDefaultGeneric").MakeGenericMethod(t).Invoke(this, null);
        }

        public T GetDefaultGeneric<T>()
        {
            return default(T);
        }

        /// <summary>
        /// Used in some tests to create a member of a type.
        /// </summary>
        private readonly IDictionary<Type, object> _typeToObject = new Dictionary<Type, object>()
        {
            { typeof(string), "this is a string" },
            { typeof(int), 1234 },
            { typeof(bool), true },
            { typeof(DateTime), DateTime.Now }
        };

        private static KeyVaultConfigurationService CreateDummyConfigService()
        {
            return new KeyVaultConfigurationService(new SecretInjector(new EmptySecretReader()), new Dictionary<string, string>());
        }
    }
}
