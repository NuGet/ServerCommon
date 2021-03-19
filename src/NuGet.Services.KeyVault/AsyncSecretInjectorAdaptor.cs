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
    /// <summary>
    /// Adapts <see cref="ISyncSecretInjector"/> to be used in place of <see cref="ISecretInjector"/>
    /// </summary>
    public class AsyncSecretInjectorAdaptor : ISecretInjector
    {
        private readonly ISyncSecretInjector _syncSecretInjector;

        public AsyncSecretInjectorAdaptor(ISyncSecretInjector syncSecretInjector)
        {
            _syncSecretInjector = syncSecretInjector ?? throw new ArgumentNullException(nameof(syncSecretInjector));
        }

        public Task<string> InjectAsync(string input)
            => Task.FromResult(_syncSecretInjector.Inject(input));

        public Task<string> InjectAsync(string input, ILogger logger)
            => Task.FromResult(_syncSecretInjector.Inject(input));
    }
}
