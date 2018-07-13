// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace NuGet.Services.Sql.Tests
{
    public class AzureSqlClientAuthenticatorFacts
    {
        public class TheConstructor
        {
            [Fact]
            public void WhenConnectionStringIsNull_ThrowsArgumentNullException()
            {
                Assert.Throws<ArgumentNullException>(() =>
                {
                    new AzureSqlClientAuthenticator(null, "certData");
                });
            }

            [Theory]
            [InlineData("")]
            [InlineData(null)]
            public void WhenCertificateDataIsNullOrEmpty_ThrowsArgumentException(string certificateData)
            {
                Assert.Throws<ArgumentNullException>(() =>
                {
                    new AzureSqlClientAuthenticator(
                        new AzureSqlConnectionStringBuilder(""),
                        certificateData);
                });
            }
        }

        public class TheClientCertificateHasChangedMethod
        {
            [Fact]
            public void WhenDataDoesNotMatch_ReturnsTrue()
            {
                var authenticator = new MockAuthenticator();

                Assert.True(authenticator.ClientCertificateHasChanged("not-certificate"));
            }

            [Fact]
            public void WhenDataMatches_ReturnsFalse()
            {
                var authenticator = new MockAuthenticator();

                Assert.False(authenticator.ClientCertificateHasChanged("certificate"));
            }
        }

        public class TheAcquireTokenAsyncMethod
        {
            [Fact]
            public async Task WhenTokenExpired_ReturnsNewToken()
            {
                // Arrange
                var authenticator = new MockAuthenticator(initialResult: MockAuthenticator.ExpiredToken);

                // Act
                var result = await authenticator.AcquireTokenAsync();

                // Assert
                Assert.Equal("valid", result);
            }

            [Fact]
            public async Task WhenTokenNearExpiration_ReturnsNewToken()
            {
                // Arrange
                var authenticator = new MockAuthenticator(initialResult: MockAuthenticator.NearExpiredToken);

                // Act
                var result = await authenticator.AcquireTokenAsync();

                // Assert
                Assert.Equal("valid", result);
            }

            [Fact]
            public async Task WhenMultipleCalls_ReturnsCachedToken()
            {
                // Arrange
                var authenticator = new MockAuthenticator();
                List<Task> tasks = new List<Task>();

                // Act
                Parallel.For(0, 10, i =>
                {
                    tasks.Add(Task.Run(() => authenticator.AcquireTokenAsync()));
                });
                await Task.WhenAll(tasks);

                // Assert
                Assert.Equal(1, authenticator.AcquireTokenCounter);
            }
        }
    }
}
