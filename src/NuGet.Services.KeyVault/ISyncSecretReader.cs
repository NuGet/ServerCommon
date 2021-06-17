// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace NuGet.Services.KeyVault
{
    /// <summary>
    /// An alternative to <see cref="ISecretReader"/> that provides sync interface for
    /// secret retrieval. It is assumed that an implementation would not block calls to the interface method(s) for
    /// some kind of I/O, but it will retrieve them in appropriate context instead (for example background thread).
    /// </summary>
    public interface ISyncSecretReader
    {
        string GetSecret(string secretName);
        ISecret GetSecretObject(string secretName);
    }
}
