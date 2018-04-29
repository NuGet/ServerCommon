// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Moq;
using NuGet.Services.KeyVault.Secret;
using Xunit;

namespace NuGet.Services.KeyVault.Tests
{
    public class SecretTokenFacts
    {
        [Fact]
        public void ThrowsWhenNameIsNull()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new SecretToken(null, _secretReaderMock.Object));
        }

        [Fact]
        public void ThrowsWhenSecretReaderIsNull()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new SecretToken("something", null));
            Assert.Equal("secretReader", ex.ParamName);
        }

        [Fact]
        public async Task ForwardsNameToSecretReader()
        {
            const string secretName = "secretName";
            var target = CreateToken(secretName);

            await target.ProcessAsync();

            _secretReaderMock
                .Verify(sr => sr.GetSecretAsync(secretName), Times.Once);
            _secretReaderMock
                .Verify(sr => sr.GetSecretAsync(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ReturnsValueFromSecretReader()
        {
            const string secretName = "secretName";
            const string secretValue = "secretValue";
            var target = CreateToken(secretName);
            _secretReaderMock
                .Setup(sr => sr.GetSecretAsync(secretName))
                .ReturnsAsync(secretValue);

            var response = await target.ProcessAsync();

            Assert.Same(secretValue, response);
        }

        private Mock<ISecretReader> _secretReaderMock;

        public SecretTokenFacts()
        {
            _secretReaderMock = new Mock<ISecretReader>();
        }

        private SecretToken CreateToken(string name)
            => new SecretToken(name, _secretReaderMock.Object);
    }
}
