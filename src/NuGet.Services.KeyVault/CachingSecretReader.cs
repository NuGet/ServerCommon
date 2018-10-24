// Copyright (c) .NET Foundation. All rights reserved. 
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information. 

using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace NuGet.Services.KeyVault
{
    public class CachingSecretReader : ISecretReader
    {
        public const int DefaultRefreshIntervalSec = 60 * 60 * 24; // 1 day
        public const int DefaultRefreshIntervalBeforeExpirySec = 60 * 30 ; // 30 minutes 

        private readonly ISecretReader _internalReader;
        private readonly ConcurrentDictionary<string, CachedSecret> _cache;
        private readonly TimeSpan _refreshInterval;        
        private readonly TimeSpan _refreshIntervalBeforeExpiry;

        public CachingSecretReader(ISecretReader secretReader, int refreshIntervalSec = DefaultRefreshIntervalSec, int refreshIntervalBeforeExpirySec = DefaultRefreshIntervalBeforeExpirySec)
        {
            _internalReader = secretReader ?? throw new ArgumentNullException(nameof(secretReader));
            _cache = new ConcurrentDictionary<string, CachedSecret>();

            _refreshInterval = TimeSpan.FromSeconds(refreshIntervalSec);
            _refreshIntervalBeforeExpiry = TimeSpan.FromSeconds(refreshIntervalBeforeExpirySec);
        }

        public async Task<string> GetSecretAsync(string secretName)
        {
            Tuple<string, DateTime?> secret = await GetSecretValueAndExpiryAsync(secretName);
            return secret.Item1;
        }

        public async Task<System.Tuple<string, DateTime?>> GetSecretValueAndExpiryAsync(string secretName)
        {
            if (string.IsNullOrEmpty(secretName))
            {
                throw new ArgumentException("Null or empty secret name", nameof(secretName));
            }

            // If the cache contains the secret and it is not expired, return the cached value.
            if (_cache.TryGetValue(secretName, out CachedSecret result)
                && !IsSecretOutdated(result))
            {
                return new Tuple<string, DateTime?> (result.Value, result.ExpiryDate);
            }

            // The cache does not contain a fresh copy of the secret. Fetch and cache the secret.
            Tuple<string, DateTime?> secret = await _internalReader.GetSecretValueAndExpiryAsync(secretName);
            var updatedValue = new CachedSecret(secret.Item1, secret.Item2);

            CachedSecret updatedCachedSecret = _cache.AddOrUpdate(secretName, updatedValue, (key, old) => updatedValue);
            return new Tuple<string, DateTime?>(updatedCachedSecret.Value, updatedCachedSecret.ExpiryDate);
        }

        private bool IsSecretOutdated(CachedSecret secret)
        {
            return (((DateTime.UtcNow - secret.CacheTime) >= _refreshInterval) ||
                (secret.ExpiryDate != null &&  (secret.ExpiryDate - DateTime.UtcNow) <= _refreshIntervalBeforeExpiry));
        }

        /// <summary>
        /// A cached secret.
        /// </summary>
        private class CachedSecret
        {
            public CachedSecret(string value, DateTime? expiryDate)
            {
                Value = value;
                CacheTime = DateTimeOffset.UtcNow;
                ExpiryDate = expiryDate;
            }

            /// <summary>
            /// The value of the cached secret.
            /// </summary>
            public string Value { get; }

            /// <summary>
            /// The time at which the secret was cached.
            /// </summary>
            public DateTimeOffset CacheTime { get; }

            /// <summary>
            /// Secret Expiry Date in UTC
            /// </summary>
            public DateTime? ExpiryDate { get; }


        }
    }
}