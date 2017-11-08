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

        private readonly ISecretReader _internalReader;
        private readonly ConcurrentDictionary<string, CachedSecret> _cache;
        private readonly TimeSpan _refreshInterval;

        public CachingSecretReader(ISecretReader secretReader, int refreshIntervalSec = DefaultRefreshIntervalSec)
        {
            _internalReader = secretReader ?? throw new ArgumentNullException(nameof(secretReader)); ;
            _cache = new ConcurrentDictionary<string, CachedSecret>();

            _refreshInterval = TimeSpan.FromSeconds(refreshIntervalSec);
        }

        public async Task<string> GetSecretAsync(string secretName)
        {
            // If the cache contains the secret and it is not expired, returned the cached value.
            if (_cache.TryGetValue(secretName, out CachedSecret result))
            {
                if (!IsSecretOutdated(result))
                {
                    return result.Value;
                }
            }

            // The cache does not contain a fresh copy of the secret. Fetch and cache the secret.
            result.Value = await _internalReader.GetSecretAsync(secretName);
            result.CacheTimeUtc = DateTime.UtcNow;

            var updatedResult = _cache.AddOrUpdate(secretName, result, (key, old) => result);

            return updatedResult.Value;
        }

        private bool IsSecretOutdated(CachedSecret secret)
        {
            return (DateTime.UtcNow - secret.CacheTimeUtc) >= _refreshInterval;
        }

        /// <summary>
        /// A cached secret.
        /// </summary>
        private struct CachedSecret
        {
            /// <summary>
            /// The value of the cached secret.
            /// </summary>
            public string Value { get; set; }

            /// <summary>
            /// The time at which the secret was cached.
            /// </summary>
            public DateTime CacheTimeUtc { get; set; }
        }
    }
}