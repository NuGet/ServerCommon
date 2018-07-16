// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Moq;
using NuGet.Services.KeyVault;
using Xunit;

namespace NuGet.Services.Sql.Tests
{
    public class AzureSqlConnectionFactoryFacts
    {
        public const string AadTenant = "aadTenant";
        public const string AadClientId = "aadClientId";

        public const string BaseConnectionString = "Data Source=tcp:DB.database.windows.net;Initial Catalog=DB";

        public static readonly string SqlConnectionString = $"{BaseConnectionString};User ID=$$user$$;Password=$$pass$$";
        public static readonly string AadSqlConnectionString = $"{BaseConnectionString};AadTenant={AadTenant};AadClientId={AadClientId};AadCertificate=$$cert$$";

        public class TheConstructor
        {
            [Theory]
            [InlineData("")]
            [InlineData(null)]
            public void WhenConnectionStringMissing_ThrowsArgumentException(string connectionString)
            {
                Assert.Throws<ArgumentException>(() => new AzureSqlConnectionFactory(
                    connectionString,
                    new Mock<ISecretInjector>().Object));
            }

            [Fact]
            public void WhenSecretInjectorIsNull_ThrowsArgumentException()
            {
                Assert.Throws<ArgumentNullException>(() => new AzureSqlConnectionFactory(
                    SqlConnectionString,
                    null));
            }
        }

        public class TheCreateAndOpenAsyncMethods
        {
            [Theory]
            [InlineData(true)]
            [InlineData(false)]
            public async Task WhenSqlConnectionString_InjectsSecrets(bool shouldOpen)
            {
                // Arrange
                var factory = new MockFactory(SqlConnectionString);

                // Act
                var connection = await ConnectAsync(factory, shouldOpen);

                // Assert
                factory.MockSecretReader.Verify(x => x.GetSecretAsync(It.IsAny<string>()), Times.Exactly(2));
                factory.MockSecretReader.Verify(x => x.GetSecretAsync("user"), Times.Once);
                factory.MockSecretReader.Verify(x => x.GetSecretAsync("pass"), Times.Once);

                Assert.True(connection.ConnectionString.Equals(
                    $"{BaseConnectionString};User ID=user;Password=pass", StringComparison.InvariantCultureIgnoreCase));

                Assert.Equal(shouldOpen, factory.Opened);
            }

            [Theory]
            [InlineData(true)]
            [InlineData(false)]
            public async Task WhenAadConnectionString_InjectsSecrets(bool shouldOpen)
            {
                // Arrange
                var factory = new MockFactory(AadSqlConnectionString);

                // Act
                var connection = await ConnectAsync(factory, shouldOpen);

                // Assert
                factory.MockSecretReader.Verify(x => x.GetSecretAsync("cert"), Times.Once);

                // Note that AAD keys are extracted for internal use only
                Assert.True(connection.ConnectionString.Equals(
                    $"{BaseConnectionString}", StringComparison.InvariantCultureIgnoreCase));

                Assert.Equal(shouldOpen, factory.Opened);
            }

            [Theory]
            [InlineData(true)]
            [InlineData(false)]
            public async Task WhenSqlConnectionString_DoesNotAcquireAccessToken(bool shouldOpen)
            {
                // Arrange
                var factory = new MockFactory(SqlConnectionString);

                // Act
                var connection = await ConnectAsync(factory, shouldOpen);

                // Assert
                Assert.True(string.IsNullOrEmpty(connection.AccessToken));
                Assert.Null(factory.AcquireTokenArguments);

                Assert.Equal(shouldOpen, factory.Opened);
            }

            [Theory]
            [InlineData(true)]
            [InlineData(false)]
            public async Task WhenAadConnectionString_AcquiresAccessToken(bool shouldOpen)
            {
                // Arrange
                var factory = new MockFactory(AadSqlConnectionString);

                // Act
                var connection = await ConnectAsync(factory, shouldOpen);

                // Assert
                Assert.Equal("valid", connection.AccessToken);
                Assert.Equal(1, factory.MockAuthenticator.AcquireTokenCounter);
                Assert.Equal(shouldOpen, factory.Opened);
            }

            private Task<SqlConnection> ConnectAsync(MockFactory factory, bool shouldOpen)
            {
                return shouldOpen ? factory.OpenAsync() : factory.CreateAsync();
            }
        }
    }
}
