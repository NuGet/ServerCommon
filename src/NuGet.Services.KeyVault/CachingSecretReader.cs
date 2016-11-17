// Copyright (c) .NET Foundation. All rights reserved. 
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information. 

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NuGet.Services.KeyVault
{
    public class CachingSecretReader : ISecretReader
    {
        public const int DefaultRefreshIntervalSec = 60 * 60 * 24; // 1 day
        private readonly int _refreshIntervalSec;

        private readonly ISecretReader _internalReader;
        private readonly Dictionary<string, Tuple<Secret, DateTime>> _cache;

        public CachingSecretReader(ISecretReader secretReader, int refreshIntervalSec = DefaultRefreshIntervalSec)
        {
            if (secretReader == null)
            {
                throw new ArgumentNullException(nameof(secretReader));
            }

            _internalReader = secretReader;
            _cache = new Dictionary<string, Tuple<Secret, DateTime>>();

            _refreshIntervalSec = refreshIntervalSec;
        }

        public virtual bool IsSecretOutdated(DateTime timeSecretCached)
        {
            return DateTime.UtcNow.Subtract(timeSecretCached).TotalSeconds >= _refreshIntervalSec;
        }

        public Task<Secret> GetSecretAsync(Secret secret)
        {
            return GetSecretAsync(secret.Name);
        }

        public async Task<Secret> GetSecretAsync(string secretName)
        {
            if (!_cache.ContainsKey(secretName) || IsSecretOutdated(_cache[secretName].Item2))
            {
                // Get the secret if it is not yet in the cache or it is outdated.
                var secret = await _internalReader.GetSecretAsync(secretName);
                _cache[secretName] = Tuple.Create(secret, DateTime.UtcNow);
            }

            return _cache[secretName].Item1;
        }
    }
}