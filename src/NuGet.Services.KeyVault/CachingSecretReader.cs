// Copyright (c) .NET Foundation. All rights reserved. 
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information. 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading.Tasks;

namespace NuGet.Services.KeyVault
{
    public class CachingSecretReader : ISecretReader
    {
        public const int DefaultRefreshIntervalSec = 60 * 60 * 24; // 1 day
        private readonly int _refreshIntervalSec;

        private readonly ISecretReader _internalReader;
        private readonly Dictionary<string, Tuple<SecureString, DateTime>> _cache;

        public CachingSecretReader(ISecretReader secretReader, int refreshIntervalSec = DefaultRefreshIntervalSec)
        {
            if (secretReader == null)
            {
                throw new ArgumentNullException(nameof(secretReader));
            }

            _internalReader = secretReader;
            _cache = new Dictionary<string, Tuple<SecureString, DateTime>>();

            _refreshIntervalSec = refreshIntervalSec;
        }

        ~CachingSecretReader()
        {
            foreach (var secretTuple in _cache.Values)
            {
                secretTuple.Item1.Dispose();
            }
        }

        public virtual bool IsSecretOutdated(Tuple<SecureString, DateTime> cachedSecret)
        {
            return DateTime.UtcNow.Subtract(cachedSecret.Item2).TotalSeconds >= _refreshIntervalSec;
        }

        public async Task<string> GetSecretAsync(string secretName)
        {
            if (!_cache.ContainsKey(secretName) || IsSecretOutdated(_cache[secretName]))
            {
                // Get the secret if it is not yet in the cache or it is outdated.
                var secretValue = await _internalReader.GetSecretAsync(secretName);

                if (_cache.ContainsKey(secretName))
                {
                    var outdatedValue = _cache[secretName].Item1;
                    _cache[secretName] = null;
                    outdatedValue.Dispose();
                }

                _cache[secretName] = Tuple.Create(StringToSecureString(secretValue), DateTime.UtcNow);
            }

            return SecureStringToString(_cache[secretName].Item1);
        }

        private static SecureString StringToSecureString(string input)
        {
            var output = new SecureString();
            input.ToCharArray().ToList().ForEach(c => output.AppendChar(c));
            output.MakeReadOnly();
            return output;
        }

        private static string SecureStringToString(SecureString input)
        {
            var bstr = Marshal.SecureStringToBSTR(input);
            try
            {
                return Marshal.PtrToStringBSTR(bstr);
            }
            finally
            {
                Marshal.FreeBSTR(bstr);
            }
        }
    }
}