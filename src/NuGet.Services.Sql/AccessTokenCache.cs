// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace NuGet.Services.Sql
{
    public class AccessTokenCache
    {
        private const string AzureSqlResourceTokenUrl = "https://database.windows.net/";

        private const double DefaultMinExpirationInMinutes = 30;

        private ConcurrentDictionary<string, AccessTokenCacheValue> _cache = new ConcurrentDictionary<string, AccessTokenCacheValue>();

        public async Task<IAuthenticationResult> GetAsync(
            AzureSqlConnectionStringBuilder connectionString,
            string clientCertificateData,
            ILogger logger = null)
        {
            AccessTokenCacheValue accessToken;

            if (TryGetValue(connectionString, out accessToken) && !IsExpired(accessToken))
            {
                // Refresh access token in background, if near expiry or client certificate has changed.
                if (IsNearExpiry(accessToken) || ClientCertificateHasChanged(accessToken, clientCertificateData))
                {
                    TriggerBackgroundRefresh(connectionString, clientCertificateData, logger);
                }

                // Returned cached access token.
                return accessToken.AuthenticationResult;
            }

            // Acquire token in foreground, first time or if already expired.
            if (await TryRefreshAccessTokenAsync(connectionString, clientCertificateData, logger))
            {
                if (TryGetValue(connectionString, out accessToken))
                {
                    return accessToken.AuthenticationResult;
                }
            }

            throw new InvalidOperationException($"Failed to acquire access token for {connectionString.Sql.InitialCatalog}.");
        }

        /// <summary>
        /// Refresh the access token for a specific connection string, on a background thread.
        /// </summary>
        private void TriggerBackgroundRefresh(AzureSqlConnectionStringBuilder connectionString, string clientCertificateData, ILogger logger = null)
        {
            Task.Run(async () =>
            {
                await TryRefreshAccessTokenAsync(connectionString, clientCertificateData, logger);
            });
        }

        /// <summary>
        /// Refresh the access token for a specific connection string, on the current thread.
        /// </summary>
        private async Task<bool> TryRefreshAccessTokenAsync(
            AzureSqlConnectionStringBuilder connectionString,
            string clientCertificateData,
            ILogger logger = null)
        {
            try
            {
                using (logger?.BeginScope($"Refreshing access token for {connectionString.Sql.InitialCatalog}."))
                {
                    var accessToken = await AcquireAccessTokenAsync(connectionString, clientCertificateData);
                    if (accessToken != null)
                    {
                        _cache.AddOrUpdate(connectionString.ConnectionString, accessToken, (k, v) => accessToken);

                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                logger?.LogError($"Error acquiring access token for {connectionString.Sql.InitialCatalog}: {e}");
            }

            return false;
        }

        /// <summary>
        /// Access token has expired, and must be refreshed.
        /// </summary>
        private bool IsExpired(AccessTokenCacheValue accessToken)
        {
            return TokenExpiresIn(accessToken.AuthenticationResult, expirationInMinutes: 5);
        }
        
        /// <summary>
        /// Access token is near expiration, and should be refreshed soon.
        /// </summary>
        private bool IsNearExpiry(AccessTokenCacheValue accessToken)
        {
            return TokenExpiresIn(accessToken.AuthenticationResult, DefaultMinExpirationInMinutes);
        }
        
        /// <summary>
        /// AKV client certificate has been rotated, and client assertion and tokens should be refreshed soon. Old certificate
        /// (and tokens) should still be valid, so long as the rotation policy is not set to 100% of certificate lifetime.
        /// </summary>
        private bool ClientCertificateHasChanged(
            AccessTokenCacheValue accessToken,
            string clientCertificateData)
        {
            var result  = accessToken.ClientAssertionCertificate.RawData;
            return result.Equals(clientCertificateData, StringComparison.InvariantCulture);
        }

        private bool TokenExpiresIn(IAuthenticationResult token, double expirationInMinutes)
        {
            return (token.ExpiresOn - DateTimeOffset.Now).TotalMinutes < expirationInMinutes;
        }

        protected virtual bool TryGetValue(
            AzureSqlConnectionStringBuilder connectionString,
            out AccessTokenCacheValue accessToken)
        {
            return _cache.TryGetValue(connectionString.ConnectionString, out accessToken);
        }

        protected virtual async Task<AccessTokenCacheValue> AcquireAccessTokenAsync(
            AzureSqlConnectionStringBuilder connectionString,
            string clientCertificateData)
        {
            var certificate = new X509Certificate2(Convert.FromBase64String(clientCertificateData), string.Empty);
            var clientAssertion = new ClientAssertionCertificate(connectionString.AadClientId, certificate);
            var authContext = new AuthenticationContext(connectionString.AadAuthority, tokenCache: null);

            var authResult = await authContext.AcquireTokenAsync(AzureSqlResourceTokenUrl, clientAssertion, connectionString.AadSendX5c);

            return new AccessTokenCacheValue(clientAssertion, authResult);
        }
    }
}
