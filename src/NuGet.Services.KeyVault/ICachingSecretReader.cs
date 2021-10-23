// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.Logging;

namespace NuGet.Services.KeyVault
{
    public interface ICachingSecretReader : ISecretReader
    {
        string TryGetCachedSecret(string secretName);
        string TryGetCachedSecret(string secretName, ILogger logger);
        ISecret TryGetCachedSecretObject(string secretName);
        ISecret TryGetCachedSecretObject(string secretName, ILogger logger);
    }
}
