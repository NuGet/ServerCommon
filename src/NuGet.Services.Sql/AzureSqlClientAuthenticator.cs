// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace NuGet.Services.Sql
{
    public class AzureSqlClientAuthenticator
    {
        /// <summary>
        /// Minimum token lifetime remaining before we should request a new access token.
        /// </summary>
        private const double MinTokenExpiryInMinutes = 30;

        private const string AzureSqlResourceId = "https://database.windows.net/";

        private AzureSqlConnectionStringBuilder ConnectionString { get; }

        private string ClientCertificateData { get; }

        private AuthenticationContext AuthenticationContext { get; }

        private ClientAssertionCertificate ClientAssertionCertificate { get; }

        /// <summary>
        /// Cached AAD access token, because acquiring the access token takes an external request which can
        /// add 2-3s latency to the connection.
        /// </summary>
        protected IAuthenticationResult AuthenticationResult { get; set; }

        private SemaphoreSlim AuthenticationResultLock = new SemaphoreSlim(1, 1);

        public AzureSqlClientAuthenticator(AzureSqlConnectionStringBuilder connectionString, string clientCertificateData)
        {
            ConnectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));

            if (string.IsNullOrEmpty(clientCertificateData))
            {
                throw new ArgumentNullException(nameof(clientCertificateData), $"Argument must be specified");
            }

            ClientCertificateData = clientCertificateData;

            var certificate = new X509Certificate2(Convert.FromBase64String(ClientCertificateData), string.Empty);

            ClientAssertionCertificate = new ClientAssertionCertificate(
                ConnectionString.AadClientId,
                certificate);

            // Do not use the default TokenCache, which is not thread-safe.
            // see: https://docs.microsoft.com/en-us/azure/architecture/multitenant-identity/token-cache
            AuthenticationContext = new AuthenticationContext(
                ConnectionString.AadAuthority,
                tokenCache: null);
        }

        /// <summary>
        /// Test constructor that doesn't initialize AAD objects.
        /// </summary>
        /// <param name="clientCertificateData"></param>
        protected AzureSqlClientAuthenticator(string clientCertificateData)
        {
            ClientCertificateData = clientCertificateData ?? throw new ArgumentNullException(nameof(clientCertificateData));
        }

        public bool ClientCertificateHasChanged(string clientCertificateData)
        {
            return !ClientCertificateData.Equals(clientCertificateData, StringComparison.InvariantCulture);
        }

        public async Task<string> AcquireTokenAsync()
        {
            return (await AcquireTokenInternalAsync()).AccessToken;
        }

        /// <summary>
        /// Access token lifetime defaults to 1 hour, and is consistent with lifetimes seen in testing.
        /// Note that lifetime policy is set at the tenant, and does not appear to be something we can control.
        /// https://docs.microsoft.com/azure/active-directory/active-directory-configurable-token-lifetimes
        /// </summary>
        private static bool TokenIsNearExpiration(IAuthenticationResult token)
        {
            return (token.ExpiresOn - DateTimeOffset.Now).TotalMinutes < MinTokenExpiryInMinutes;
        }

        private async Task<IAuthenticationResult> AcquireTokenInternalAsync()
        {
            var result = AuthenticationResult;
            if (result != null && !TokenIsNearExpiration(result))
            {
                return result;
            }

            await AuthenticationResultLock.WaitAsync().ConfigureAwait(false);

            try
            {
                result = AuthenticationResult;
                if (result != null && !TokenIsNearExpiration(result))
                {
                    return result;
                }

                result = AuthenticationResult = await AcquireTokenFromContextAsync();
            }
            finally
            {
                AuthenticationResultLock.Release();
            }

            return result;
        }

        protected async virtual Task<IAuthenticationResult> AcquireTokenFromContextAsync()
        {
            return new AuthenticationResultWrapper(
                await AuthenticationContext.AcquireTokenAsync(
                    AzureSqlResourceId,
                    ClientAssertionCertificate,
                    ConnectionString.AadSendX5c
                    ));
        }
    }
}
