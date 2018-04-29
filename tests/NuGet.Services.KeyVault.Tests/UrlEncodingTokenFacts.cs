// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Moq;
using NuGet.Services.KeyVault.Secret;

namespace NuGet.Services.KeyVault.Tests
{
    public class UrlEncodingTokenFacts
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

        [Theory]
        [InlineData("", "")]
        [InlineData("some@email.com", "some%40email.com")]
        [InlineData("fh@34F!j#&^", "fh%4034F%21j%23%26%5E")]
        public async Task UrlEncodesValue(string secretValue, string expectedOutput)
        {
            const string secretName = "secretName";
            var target = CreateToken(secretName);
            _secretReaderMock
                .Setup(sr => sr.GetSecretAsync(secretName))
                .ReturnsAsync(secretValue);

            var response = await target.ProcessAsync();

            Assert.Equal(expectedOutput, response);
        }

        private Mock<ISecretReader> _secretReaderMock;

        public UrlEncodingTokenFacts()
        {
            _secretReaderMock = new Mock<ISecretReader>();
            _secretReaderMock
                .Setup(sr => sr.GetSecretAsync(It.IsAny<string>()))
                .ReturnsAsync("somevalue");
        }

        private UrlEncodingToken CreateToken(string name)
            => new UrlEncodingToken(name, _secretReaderMock.Object);
    }
}
