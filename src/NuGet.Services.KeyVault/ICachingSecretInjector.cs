// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.Logging;

namespace NuGet.Services.KeyVault
{
    public interface ICachingSecretInjector : ISecretInjector
    {
        string TryInjectCached(string input);
        string TryInjectCached(string input, ILogger logger);
    }
}
