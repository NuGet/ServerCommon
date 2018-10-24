﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;

namespace NuGet.Services.KeyVault
{
    public class EmptySecretReader : ISecretReader
    {
        public Task<string> GetSecretAsync(string secretName)
        {
            return Task.FromResult(secretName);
        }

        public Task<Tuple<string, DateTime?>> GetSecretValueAndExpiryAsync(string secretName)
        {            
            return Task.FromResult(new Tuple<string, DateTime?>(secretName, null));
        }
    }
}