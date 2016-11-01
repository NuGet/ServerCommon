﻿// Copyright (c) .NET Foundation. All rights reserved. 
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information. 

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NuGet.Services.Configuration
{
    /// <summary>
    /// Asynchronously provides configuration or command line arguments.
    /// </summary>
    public interface IConfigurationProvider : ISettingsProvider
    {
        /// <summary>
        /// Gets an argument from the service synchronously.
        /// Should use <see cref="ISettingsProvider.GetOrThrow{T}"/> unless a synchronous context is completely necessary.
        /// </summary>
        /// <typeparam name="T">Converts the argument from a string into this type.</typeparam>
        /// <param name="key">The key mapping to the desired argument.</param>
        /// <returns>The argument mapped to by the key converted to type T.</returns>
        /// <exception cref="KeyNotFoundException">Thrown when the key is not mapped to an argument.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the argument mapped to by the key is null or empty.</exception>
        /// <exception cref="NotSupportedException">Thrown when the argument mapped to by the key cannot be converted into an object of type T.</exception>
        T GetOrThrowSync<T>(string key);

        /// <summary>
        /// Gets an argument from the service synchronously.
        /// Should use <see cref="ISettingsProvider.GetOrDefault{T}"/> unless a synchronous context is completely necessary.
        /// </summary>
        /// <typeparam name="T">Converts the argument from a string into this type.</typeparam>
        /// <param name="key">The key mapping to the desired argument.</param>
        /// <param name="defaultValue">The value returned if there is an issue getting the argument from the cache.</param>
        /// <returns>The argument mapped to by the key converted to type T or defaultValue if the argument could not be acquired and converted.</returns>
        T GetOrDefaultSync<T>(string key, T defaultValue = default(T));
    }
}