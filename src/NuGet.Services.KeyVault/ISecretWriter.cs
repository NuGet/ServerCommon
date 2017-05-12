// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;

namespace NuGet.Services.KeyVault
{
    public interface ISecretWriter
    {
        /// <summary>
        /// Sets the value of a secret.
        /// If a secret with the same name as <paramref name="secret"/> already exists, it will be overwritten.
        /// </summary>
        /// <param name="secret">A secret to set.</param>
        /// <returns>The secret that has been set.</returns>
        Task<Secret> SetSecretAsync(Secret secret);

        /// <summary>
        /// Sets the value of a secret.
        /// If a secret with the same name as <paramref name="secretName"/> already exists, it will be overwritten.
        /// </summary>
        /// <param name="secretName">The name of the secret to set.</param>
        /// <param name="value">The value of the secret to set.</param>
        /// <param name="tags">The tags of the secret to set.</param>
        /// <returns>The secret that has been set.</returns>
        Task<Secret> SetSecretAsync(string secretName, string value, Dictionary<string, string> tags = null);

        /// <summary>
        /// Deletes a secret.
        /// </summary>
        /// <param name="secret">A secret with the same name as the secret to delete.</param>
        /// <returns>The deleted secret.</returns>
        Task<Secret> DeleteSecretAsync(Secret secret);

        /// <summary>
        /// Deletes a secret.
        /// </summary>
        /// <param name="secretName">The name of the secret to delete.</param>
        /// <returns>The deleted secret.</returns>
        Task<Secret> DeleteSecretAsync(string secretName);
    }
}
