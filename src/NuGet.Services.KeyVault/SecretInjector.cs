// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace NuGet.Services.KeyVault
{
    public class SecretInjector : ICachingSecretInjector
    {
        public const string DefaultFrame = "$$";
        private readonly string _frame;
        private readonly ISecretReader _secretReader;
        private readonly ICachingSecretReader _cachingSecretReader = null;

        public SecretInjector(ISecretReader secretReader) : this(secretReader, DefaultFrame)
        {
        }

        public SecretInjector(ISecretReader secretReader, string frame)
        {
            if (secretReader == null)
            {
                throw new ArgumentNullException(nameof(secretReader));
            }

            if (string.IsNullOrWhiteSpace(frame))
            {
                throw new ArgumentException("Frame argument is null or empty.");
            }

            _frame = frame;
            _secretReader = secretReader;
        }

        public SecretInjector(ICachingSecretReader cachingSecretReader) : this(cachingSecretReader, DefaultFrame)
        {
        }

        public SecretInjector(ICachingSecretReader cachingSecretReader, string frame) : this((ISecretReader)cachingSecretReader, frame)
        {
            _cachingSecretReader = cachingSecretReader;
        }

        public Task<string> InjectAsync(string input)
        {
            return InjectAsync(input, logger: null);
        }

        public async Task<string> InjectAsync(string input, ILogger logger)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            var output = new StringBuilder(input);
            var secretNames = GetSecretNames(input);

            foreach (var secretName in secretNames)
            {
                var secretValue = await _secretReader.GetSecretAsync(secretName, logger);
                output.Replace($"{_frame}{secretName}{_frame}", secretValue);
            }

            return output.ToString();
        }

        public string TryInjectCached(string input)
        {
            return TryInjectCached(input, logger: null);
        }

        public string TryInjectCached(string input, ILogger logger)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            var output = new StringBuilder(input);
            var secretNames = GetSecretNames(input);

            if (secretNames.Count > 0 && _cachingSecretReader == null)
            {
                return null;
            }

            foreach (var secretName in secretNames)
            {
                var secretValue = _cachingSecretReader?.TryGetCachedSecret(secretName, logger);
                if (secretValue == null)
                {
                    return null;
                }
                output.Replace($"{_frame}{secretName}{_frame}", secretValue);
            }

            return output.ToString();
        }

        private ICollection<string> GetSecretNames(string input)
        {
            var secretNames = new HashSet<string>();

            int startIndex = 0;
            int foundIndex;
            bool insideFrame = false;

            do
            {
                foundIndex = input.IndexOf(_frame, startIndex, StringComparison.InvariantCulture);

                if (insideFrame && foundIndex > 0)
                {
                    var secret = input.Substring(startIndex, foundIndex - startIndex);
                    if (!string.IsNullOrWhiteSpace(secret))
                    {
                        secretNames.Add(secret);
                    }

                    insideFrame = false;
                }
                else
                {
                    insideFrame = true;
                }

                startIndex = foundIndex + _frame.Length;

            } while (foundIndex >= 0);

            return secretNames;
        }
    }
}