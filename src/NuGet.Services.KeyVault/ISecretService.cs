﻿// Copyright (c) .NET Foundation. All rights reserved. 
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information. 

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NuGet.Services.KeyVault
{
    /// <summary>
    /// A dictionary of configuration or command line arguments.
    /// </summary>
    public interface ISecretService
    {
        /// <summary>
        /// Gets an argument from the dictionary.
        /// </summary>
        /// <typeparam name="T">Converts the argument from a string into this type.</typeparam>
        /// <param name="key">The key mapping to the desired argument.</param>
        /// <returns>The argument mapped to by the key converted to type T.</returns>
        /// <exception cref="KeyNotFoundException">Thrown when the key is not mapped to an argument.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the argument mapped to by the key is null or empty.</exception>
        /// <exception cref="NotSupportedException">Thrown when the argument mapped to by the key cannot be converted into an object of type T.</exception>
        Task<T> GetOrThrow<T>(string key);

        /// <summary>
        /// Gets an argument from the dictionary.
        /// </summary>
        /// <typeparam name="T">Converts the argument from a string into this type.</typeparam>
        /// <param name="key">The key mapping to the desired argument.</param>
        /// <param name="defaultValue">The value returned if there is an issue getting the argument from the cache.</param>
        /// <returns>The argument mapped to by the key converted to type T or defaultValue if the argument could not be acquired and converted.</returns>
        Task<T> GetOrDefault<T>(string key, T defaultValue = default(T));

        /// <summary>
        /// Gets an argument from the dictionary synchronously.
        /// </summary>
        /// <typeparam name="T">Converts the argument from a string into this type.</typeparam>
        /// <param name="key">The key mapping to the desired argument.</param>
        /// <returns>The argument mapped to by the key converted to type T.</returns>
        /// <exception cref="KeyNotFoundException">Thrown when the key is not mapped to an argument.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the argument mapped to by the key is null or empty.</exception>
        /// <exception cref="NotSupportedException">Thrown when the argument mapped to by the key cannot be converted into an object of type T.</exception>
        [Obsolete("Use GetOrThrow unless a synchronous context is completely necessary.")]
        T GetOrThrowSync<T>(string key);

        /// <summary>
        /// Gets an argument from the dictionary synchronously.
        /// </summary>
        /// <typeparam name="T">Converts the argument from a string into this type.</typeparam>
        /// <param name="key">The key mapping to the desired argument.</param>
        /// <param name="defaultValue">The value returned if there is an issue getting the argument from the cache.</param>
        /// <returns>The argument mapped to by the key converted to type T or defaultValue if the argument could not be acquired and converted.</returns>
        [Obsolete("Use GetOrDefault unless a synchronous context is completely necessary.")]
        T GetOrDefaultSync<T>(string key, T defaultValue = default(T));
    }
}
