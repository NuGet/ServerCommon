// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;

namespace NuGet.Services.KeyVault
{
    public interface ISecretReader
    {
        /// <summary>
        /// Gets a secret.
        /// </summary>
        /// <param name="secret">A secret with the same name as the secret to acquire.</param>
        /// <returns>A secret specified by <paramref name="secret"/>.</returns>
        Task<Secret> GetSecretAsync(Secret secret);

        /// <summary>
        /// Gets a secret.
        /// </summary>
        /// <param name="secretName">The name of the secret to acquire.</param>
        /// <returns>A secret with the name specified by <paramref name="secretName"/>.</returns>
        Task<Secret> GetSecretAsync(string secretName);
    }
}