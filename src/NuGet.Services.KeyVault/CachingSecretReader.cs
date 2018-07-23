// Copyright (c) .NET Foundation. All rights reserved. 
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information. 

using System;
using System.Collections.Concurrent;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace NuGet.Services.KeyVault
{
    public class CachingSecretReader : ISecretReader
    {
        public const int DefaultRefreshIntervalSec = 60 * 60 * 24; // 1 day

        private readonly ISecretReader _internalReader;
        private readonly ConcurrentDictionary<string, CachedSecret<string>> _stringCache;
        private readonly ConcurrentDictionary<string, CachedSecret<X509Certificate2>> _certCache;
        private readonly TimeSpan _refreshInterval;

        public CachingSecretReader(ISecretReader secretReader, int refreshIntervalSec = DefaultRefreshIntervalSec)
        {
            _internalReader = secretReader ?? throw new ArgumentNullException(nameof(secretReader));
            _stringCache = new ConcurrentDictionary<string, CachedSecret<string>>();
            _certCache = new ConcurrentDictionary<string, CachedSecret<X509Certificate2>>();

            _refreshInterval = TimeSpan.FromSeconds(refreshIntervalSec);
        }

        public Task<string> GetSecretAsync(string secretName)
        {
            return Get(secretName, _stringCache, s => _internalReader.GetSecretAsync(s));
        }

        public Task<X509Certificate2> GetCertificateAsync(string secretName)
        {
            return Get(secretName, _certCache, s => _internalReader.GetCertificateAsync(s));
        }

        private async Task<T> Get<T>(string secretName, ConcurrentDictionary<string, CachedSecret<T>> cache, Func<string, Task<T>> getSecret)
        {
            if (string.IsNullOrEmpty(secretName))
            {
                throw new ArgumentException("Null or empty secret name", nameof(secretName));
            }

            // If the cache contains the secret and it is not expired, return the cached value.
            if (cache.TryGetValue(secretName, out CachedSecret<T> result)
                && !IsSecretOutdated(result))
            {
                return result.Value;
            }

            // The cache does not contain a fresh copy of the secret. Fetch and cache the secret.
            var updatedValue = new CachedSecret<T>(await getSecret(secretName));

            return cache.AddOrUpdate(secretName, updatedValue, (key, old) => updatedValue)
                         .Value;
        }

        private bool IsSecretOutdated<T>(CachedSecret<T> secret)
        {
            return (DateTime.UtcNow - secret.CacheTime) >= _refreshInterval;
        }

        /// <summary>
        /// A cached secret.
        /// </summary>
        private class CachedSecret<T>
        {
            public CachedSecret(T value)
            {
                Value = value;
                CacheTime = DateTimeOffset.UtcNow;
            }

            /// <summary>
            /// The value of the cached secret.
            /// </summary>
            public T Value { get; }

            /// <summary>
            /// The time at which the secret was cached.
            /// </summary>
            public DateTimeOffset CacheTime { get; }
        }
    }
}