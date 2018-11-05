// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace NuGet.Services.KeyVault.Tests
{
    public class CachingSecretReaderFacts
    {
        [Fact]
        public async Task WhenGetSecretIsCalledCacheIsUsed()
        {
            // Arrange
            const string secret = "secret";
            var mockSecretReader = new Mock<ISecretReader>();
            mockSecretReader.Setup(x => x.GetSecretAsync(It.IsAny<string>())).Returns(Task.FromResult(secret));

            var cachingSecretReader = new CachingSecretReader(mockSecretReader.Object, int.MaxValue);

            // Act
            var value1 = await cachingSecretReader.GetSecretAsync("secretname");
            var value2 = await cachingSecretReader.GetSecretAsync("secretname");

            // Assert
            mockSecretReader.Verify(x => x.GetSecretAsync(It.IsAny<string>()), Times.Once);
            Assert.Equal(secret, value1);
            Assert.Equal(value1, value2);
        }

        [Fact]
        public async Task WhenGetSecretIsCalledCacheIsRefreshedIfPastInterval()
        {
            // Arrange
            const string secretName = "secretname";
            const string firstSecret = "secret1";
            const string secondSecret = "secret2";
            const int refreshIntervalSec = 1;

            var mockSecretReader = new Mock<ISecretReader>();

            mockSecretReader
                .SetupSequence(x => x.GetSecretAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(firstSecret))
                .Returns(Task.FromResult(secondSecret));

            var cachingSecretReader = new CachingSecretReader(mockSecretReader.Object, refreshIntervalSec);

            // Act
            var firstValue1 = await cachingSecretReader.GetSecretAsync(secretName);
            var firstValue2 = await cachingSecretReader.GetSecretAsync(secretName);

            // Assert
            mockSecretReader.Verify(x => x.GetSecretAsync(It.IsAny<string>()), Times.Once);
            Assert.Equal(firstSecret, firstValue1);
            Assert.Equal(firstSecret, firstValue2);

            // Arrange 2
            // We are now x seconds later after refreshIntervalSec has passed.
            await Task.Delay(TimeSpan.FromSeconds(refreshIntervalSec * 2));

            // Act 2
            var secondValue1 = await cachingSecretReader.GetSecretAsync(secretName);
            var secondValue2 = await cachingSecretReader.GetSecretAsync(secretName);

            // Assert 2
            mockSecretReader.Verify(x => x.GetSecretAsync(It.IsAny<string>()), Times.Exactly(2));
            Assert.Equal(secondSecret, secondValue1);
            Assert.Equal(secondSecret, secondValue2);
        }

        [Fact]
        public async Task WhenGetSecretIsCalledCacheIsRefreshedIfPastSecretExpiry()
        {
            // Arrange
            const string secretName = "secretname";
            const string firstSecretValue = "testValue";
            DateTime firstSecretExpiration = DateTime.UtcNow.AddSeconds(3);
            const string secondSecretValue = "testValue2";
            DateTime secondSecretExpiration = DateTime.UtcNow.AddHours(1);
            KeyVaultSecret secret1 = new KeyVaultSecret(secretName, firstSecretValue, firstSecretExpiration);
            KeyVaultSecret secret2 = new KeyVaultSecret(secretName, secondSecretValue, secondSecretExpiration);
            int refreshIntervalSec = 30;
            int refreshIntervalBeforeExpirySec = 0;

            var mockSecretReader = new Mock<ISecretReader>();
            mockSecretReader
                .SetupSequence(x => x.GetSecretObjectAsync(It.IsAny<string>()))
                .Returns(Task.FromResult((ISecret)secret1))
                .Returns(Task.FromResult((ISecret)secret2));

            var cachingSecretReader = new CachingSecretReader(mockSecretReader.Object, refreshIntervalSec, refreshIntervalBeforeExpirySec);

            // Act
            var secretObject1 = await cachingSecretReader.GetSecretObjectAsync(secretName);
            var secretObject2 = await cachingSecretReader.GetSecretObjectAsync(secretName);

            // Assert
            mockSecretReader.Verify(x => x.GetSecretObjectAsync(It.IsAny<string>()), Times.Once);
            Assert.Equal(secretObject1.Value, firstSecretValue);
            Assert.Equal(secretObject1.Expiration, firstSecretExpiration);
            Assert.Equal(secretObject2.Value, firstSecretValue);
            Assert.Equal(secretObject2.Expiration, firstSecretExpiration);

            // Arrange 2
            await Task.Delay(TimeSpan.FromSeconds(3));

            // Act 2
            var secretObject3 = await cachingSecretReader.GetSecretObjectAsync(secretName);
            var secretObject4 = await cachingSecretReader.GetSecretObjectAsync(secretName);

            // Assert 2
            mockSecretReader.Verify(x => x.GetSecretObjectAsync(It.IsAny<string>()), Times.Exactly(2));
            Assert.Equal(secretObject3.Value, secondSecretValue);
            Assert.Equal(secretObject3.Expiration, secondSecretExpiration);
            Assert.Equal(secretObject4.Value, secondSecretValue);
            Assert.Equal(secretObject4.Expiration, secondSecretExpiration);
        }
    }
}