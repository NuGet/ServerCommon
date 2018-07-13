// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.SqlClient;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Moq;
using NuGet.Services.KeyVault;

namespace NuGet.Services.Sql.Tests
{
    public class MockFactory : AzureSqlConnectionFactory
    {
        public Mock<ISecretReader> MockSecretReader { get; }

        public MockAuthenticator MockAuthenticator { get; }

        public Tuple<string, string, bool, X509Certificate2> AcquireTokenArguments { get; private set; }

        public bool Opened { get; private set; }

        public MockFactory(string connectionString)
            : this(connectionString, CreateMockSecretReader())
        {
        }

        public MockFactory(string connectionString, Mock<ISecretReader> mockSecretReader)
         : base(connectionString, new SecretInjector(mockSecretReader.Object))
        {
            MockSecretReader = mockSecretReader;

            MockAuthenticator = new MockAuthenticator();
        }

        protected override Task OpenConnectionAsync(SqlConnection sqlConnection)
        {
            Opened = true;
            return Task.CompletedTask;
        }

        private static Mock<ISecretReader> CreateMockSecretReader()
        {
            var mockSecretReader = new Mock<ISecretReader>();

            mockSecretReader.Setup(x => x.GetSecretAsync(It.IsAny<string>()))
                .Returns<string>(key =>
                {
                    return Task.FromResult(key.Replace("$$", string.Empty));
                })
                .Verifiable();

            return mockSecretReader;
        }

        protected override AzureSqlClientAuthenticator CreateAuthenticator(string clientCertificateData)
        {
            return MockAuthenticator;
        }
    }
}
