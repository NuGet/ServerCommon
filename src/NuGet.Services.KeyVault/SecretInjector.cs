﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace NuGet.Services.KeyVault
{
    public class SecretInjector : ISecretInjector
    {
        public const string DefaultFrame = "$$";

        private readonly string _frame;
        private readonly ISecretReader _secretReader;

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
            var secretNames = SecretUtilities.GetSecretNames(input, _frame);

            foreach (var secretName in secretNames)
            {
                var secretValue = await _secretReader.GetSecretAsync(secretName, logger);
                output.Replace($"{_frame}{secretName}{_frame}", secretValue);
            }

            return output.ToString();
        }
    }
}