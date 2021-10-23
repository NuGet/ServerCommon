// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace NuGet.Services.KeyVault.Tests
{
    public class SecretInjectorFacts
    {
        [Fact]
        public void TryInjectCachedReturnsNullIfUnderlyingReaderIsNotCaching()
        {
            var readerMock = new Mock<ISecretReader>();
            var injector = new SecretInjector(readerMock.Object);

            var result = injector.TryInjectCached("$$secretname$$");

            Assert.Null(result);
        }

        [Fact]
        public void TryInjectCachedWithLoggerReturnsNullIfUnderlyingReaderIsNotCaching()
        {
            var readerMock = new Mock<ISecretReader>();
            var injector = new SecretInjector(readerMock.Object);

            var result = injector.TryInjectCached("$$secretname$$", Mock.Of<ILogger>());

            Assert.Null(result);
        }

        [Fact]
        public void TryInjectCachedReturnsInputStringIfNoSecrets()
        {
            const string inputString = "no_secrets";
            var readerMock = new Mock<ISecretReader>();
            var injector = new SecretInjector(readerMock.Object);

            var result = injector.TryInjectCached(inputString);

            Assert.Equal(inputString, result);
        }

        [Fact]
        public void TryInjectCachedWithLoggerReturnsInputStringIfNoSecrets()
        {
            const string inputString = "no_secrets";
            var readerMock = new Mock<ISecretReader>();
            var injector = new SecretInjector(readerMock.Object);

            var result = injector.TryInjectCached(inputString, Mock.Of<ILogger>());

            Assert.Equal(inputString, result);
        }

        [Fact]
        public void TryInjectCachedReturnsNullIfSecretExpired()
        {
            const string secretName = "secretName";
            const string inputString = "$$" + secretName + "$$";
            var readerMock = new Mock<ICachingSecretReader>();
            readerMock
                .Setup(r => r.TryGetCachedSecret(secretName))
                .Returns((string)null);
            var injector = new SecretInjector(readerMock.Object);

            var result = injector.TryInjectCached(inputString);

            Assert.Null(result);
        }
        
        [Fact]
        public void TryInjectCachedWithLoggerReturnsNullIfSecretExpired()
        {
            const string secretName = "secretName";
            const string inputString = "$$" + secretName + "$$";
            var readerMock = new Mock<ICachingSecretReader>();
            readerMock
                .Setup(r => r.TryGetCachedSecret(secretName, It.IsAny<ILogger>()))
                .Returns((string)null);
            var injector = new SecretInjector(readerMock.Object);

            var result = injector.TryInjectCached(inputString, Mock.Of<ILogger>());

            Assert.Null(result);
        }

        [Fact]
        public void TryInjectCachedInjects()
        {
            const string secretName = "secretName";
            const string inputString = "$$" + secretName + "$$";
            const string secretValue = "secretValue";
            var readerMock = new Mock<ICachingSecretReader>();
            readerMock
                .Setup(r => r.TryGetCachedSecret(secretName, It.IsAny<ILogger>()))
                .Returns(secretValue);
            var injector = new SecretInjector(readerMock.Object);

            var result = injector.TryInjectCached(inputString);

            Assert.Equal(secretValue, result);
        }

        [Fact]
        public void TryInjectCachedWithLoggerInjects()
        {
            const string secretName = "secretName";
            const string inputString = "$$" + secretName + "$$";
            const string secretValue = "secretValue";
            var readerMock = new Mock<ICachingSecretReader>();
            readerMock
                .Setup(r => r.TryGetCachedSecret(secretName, It.IsAny<ILogger>()))
                .Returns(secretValue);
            var injector = new SecretInjector(readerMock.Object);

            var result = injector.TryInjectCached(inputString, Mock.Of<ILogger>());

            Assert.Equal(secretValue, result);
        }
    }
}
