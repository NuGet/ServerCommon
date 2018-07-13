// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Services.KeyVault;

namespace NuGet.Services.Sql
{
    public class AzureSqlConnectionFactory : ISqlConnectionFactory
    {
        private static ConcurrentDictionary<string, AzureSqlClientAuthenticator> AuthenticatorsCache { get; }

        private static SemaphoreSlim AuthenticatorsCacheLock = new SemaphoreSlim(1, 1);

        public string CacheKey { get; set; }

        private AzureSqlConnectionStringBuilder ConnectionString { get; }

        private ISecretInjector SecretInjector { get; }

        #region SqlConnectionStringBuilder properies

        public string ApplicationName => ConnectionString.Sql.ApplicationName;

        public int ConnectRetryInterval => ConnectionString.Sql.ConnectRetryInterval;

        public string DataSource => ConnectionString.Sql.DataSource;

        public string InitialCatalog => ConnectionString.Sql.InitialCatalog;

        #endregion

        static AzureSqlConnectionFactory()
        {
            AuthenticatorsCache = new ConcurrentDictionary<string, AzureSqlClientAuthenticator>();
        }

        public AzureSqlConnectionFactory(string connectionString, ISecretInjector secretInjector)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentException("Value cannot be null or empty", nameof(connectionString));
            }

            SecretInjector = secretInjector ?? throw new ArgumentNullException(nameof(secretInjector));

            CacheKey = connectionString;
            ConnectionString = new AzureSqlConnectionStringBuilder(connectionString);
        }

        public Task<SqlConnection> CreateAsync()
        {
            return ConnectAsync();
        }

        public async Task<SqlConnection> OpenAsync()
        {
            var connection = await ConnectAsync();

            await OpenConnectionAsync(connection);

            return connection;
        }

        private async Task<SqlConnection> ConnectAsync()
        {
            var connectionString = await SecretInjector.InjectAsync(ConnectionString.ConnectionString);
            var connection = new SqlConnection(connectionString);

            if (!string.IsNullOrWhiteSpace(ConnectionString.AadAuthority))
            {
                var authenticator = await GetClientAuthenticatorAsync();
                connection.AccessToken = await authenticator.AcquireTokenAsync();
            }

            return connection;
        }

        protected virtual Task OpenConnectionAsync(SqlConnection sqlConnection)
        {
            return sqlConnection.OpenAsync();
        }

        private async Task<AzureSqlClientAuthenticator> GetClientAuthenticatorAsync()
        {
            var clientCertificateData = await SecretInjector.InjectAsync(ConnectionString.AadCertificate);

            AzureSqlClientAuthenticator authenticator;
            if (AuthenticatorsCache.TryGetValue(CacheKey, out authenticator))
            {
                if (!authenticator.ClientCertificateHasChanged(clientCertificateData))
                {
                    return authenticator;
                }
            }

            await AuthenticatorsCacheLock.WaitAsync().ConfigureAwait(false);

            try
            {
                if (AuthenticatorsCache.TryGetValue(CacheKey, out authenticator))
                {
                    if (!authenticator.ClientCertificateHasChanged(clientCertificateData))
                    {
                        return authenticator;
                    }
                }

                authenticator = CreateAuthenticator(clientCertificateData);

                AuthenticatorsCache.AddOrUpdate(CacheKey, authenticator, (k, v) => authenticator);

                return authenticator;
            }
            finally
            {
                AuthenticatorsCacheLock.Release();
            }
        }

        protected virtual AzureSqlClientAuthenticator CreateAuthenticator(string clientCertificateData)
        {
            return new AzureSqlClientAuthenticator(ConnectionString, clientCertificateData);
        }
    }
}
