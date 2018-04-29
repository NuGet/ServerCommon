// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Moq;
using NuGet.Services.KeyVault.Secret;
using Xunit;

namespace NuGet.Services.KeyVault.Tests
{
    public class RecursiveTokenFacts
    {
        [Fact]
        public void ThrowsWhenSecretNameIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new RecursiveToken(null, _secretReaderMock.Object, _secretInjectorMock.Object));
        }

        [Fact]
        public void ThrowsWhenSecretReaderIsNull()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new RecursiveToken("somename", null, _secretInjectorMock.Object));
            Assert.Equal("secretReader", ex.ParamName);
        }

        [Fact]
        public void ThrowsWhenSecretInjectorIsNull()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new RecursiveToken("somename", _secretReaderMock.Object, null));
            Assert.Equal("secretInjector", ex.ParamName);
        }

        [Fact]
        public async Task DoesRecursiveExpansion()
        {
            const string secretName = "someName";
            const string innerSecretName = "anotherSecretName";
            const string secretValue = "$$" + innerSecretName + "$$";
            const string innerSecretInjectionResult = "42";
            _secretReaderMock
                .Setup(sr => sr.GetSecretAsync(secretName))
                .ReturnsAsync(secretValue);
            _secretInjectorMock
                .Setup(si => si.InjectAsync(secretValue))
                .ReturnsAsync(innerSecretInjectionResult);

            var target = Create(secretName);

            var result = await target.ProcessAsync();

            _secretReaderMock
                .Verify(sr => sr.GetSecretAsync(secretName), Times.Once);
            _secretReaderMock
                .Verify(sr => sr.GetSecretAsync(It.IsAny<string>()), Times.Once);

            _secretInjectorMock
                .Verify(si => si.InjectAsync(secretValue), Times.Once);
            _secretInjectorMock
                .Verify(si => si.InjectAsync(It.IsAny<string>()), Times.Once);

            Assert.Same(innerSecretInjectionResult, result);
        }

        [Fact]
        public async Task DoesOnlyOneLevelOfExpansion()
        {
            const string topLevelSecretName = "topLevelName";
            const string secondLevelSecretName = "secondLevelName";
            const string thirdLevelSecretName = "thirdLevelName";
            const string topLevelSecretValue = "$$" + secondLevelSecretName + "$$";
            const string secondLevelSecretValue = "$$" + thirdLevelSecretName + "$$";

            _secretReaderMock
                .Setup(sr => sr.GetSecretAsync(topLevelSecretName))
                .ReturnsAsync(topLevelSecretValue);
            _secretInjectorMock
                .Setup(si => si.InjectAsync(topLevelSecretValue))
                .ReturnsAsync(secondLevelSecretValue);

            var target = Create(topLevelSecretName);
            var result = await target.ProcessAsync();

            Assert.Same(secondLevelSecretValue, result);
            _secretReaderMock
                .Verify(sr => sr.GetSecretAsync(secondLevelSecretName), Times.Never);
            _secretReaderMock
                .Verify(sr => sr.GetSecretAsync(thirdLevelSecretName), Times.Never);
            _secretInjectorMock
                .Verify(si => si.InjectAsync(secondLevelSecretValue), Times.Never);
        }

        private Mock<ISecretReader> _secretReaderMock;
        private Mock<ISecretInjector> _secretInjectorMock;

        public RecursiveTokenFacts()
        {
            _secretReaderMock = new Mock<ISecretReader>();
            _secretInjectorMock = new Mock<ISecretInjector>();
        }

        private RecursiveToken Create(string secretName)
            => new RecursiveToken(secretName, _secretReaderMock.Object, _secretInjectorMock.Object);
    }
}
