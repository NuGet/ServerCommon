// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGet.Services.KeyVault
{
    public class SyncSecretInjector : ISyncSecretInjector
    {
        private readonly string _frame;
        private readonly ISyncSecretReader _secretReader;

        public SyncSecretInjector(ISyncSecretReader secretReader) : this(secretReader, SecretInjector.DefaultFrame)
        {
        }

        public SyncSecretInjector(ISyncSecretReader secretReader, string frame)
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

        public string Inject(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            var output = new StringBuilder(input);
            var secretNames = SecretUtilities.GetSecretNames(input, _frame);

            foreach (var secretName in secretNames)
            {
                var secretValue = _secretReader.GetSecret(secretName);
                output.Replace($"{_frame}{secretName}{_frame}", secretValue);
            }

            return output.ToString();
        }
    }
}
