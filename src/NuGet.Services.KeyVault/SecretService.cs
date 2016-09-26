// Copyright (c) .NET Foundation. All rights reserved. 
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information. 

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace NuGet.Services.KeyVault
{
    /// <summary>
    /// Maintains a dictionary of configuration or command line arguments and injects them with secrets using an ISecretInjector on every access to refresh them.
    /// </summary>
    public class SecretService : ISecretService
    {
        private readonly ISecretInjector _secretInjector;
        private readonly IDictionary<string, string> _arguments;
        private readonly IDictionary<string, string> _cachedArgumentValues = new Dictionary<string, string>();

        public SecretService(ISecretInjector secretInjector, IDictionary<string, string> arguments)
        {
            _secretInjector = secretInjector;
            _arguments = arguments;

            // Initially cache all arguments so that GetOrThrowSync and GetOrDefaultSync will not be called before the argument is cached.
            Task.Run(async () => await CacheAllArguments()).Wait();
        }

        private async Task CacheAllArguments()
        {
            foreach (var key in _arguments.Keys)
            {
                _cachedArgumentValues[key] = await _secretInjector.InjectAsync(_arguments[key]);
            }
        }

        /// <summary>
        /// Gets an argument and injects a secret into it.
        /// </summary>
        /// <param name="key">The key associated with the desired argument.</param>
        /// <returns>The argument associated with the given key.</returns>
        /// <exception cref="KeyNotFoundException">Thrown when the key is not found in the list of arguments.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the argument associated with the given key is null or empty.</exception>
        protected virtual async Task<string> Get(string key)
        {
            if (!_arguments.ContainsKey(key)) throw new KeyNotFoundException("Could not find key " + key + "!");

            _cachedArgumentValues[key] = await _secretInjector.InjectAsync(_arguments[key]);
            if (string.IsNullOrEmpty(_cachedArgumentValues[key])) throw new ArgumentNullException("Value for key " + key + " is null or empty!");

            return _cachedArgumentValues[key];
        }

        /// <summary>
        /// Converts a string into T using TypeDescriptor.GetConverter
        /// </summary>
        /// <typeparam name="T">Type to convert a string into.</typeparam>
        /// <param name="value">String to convert to T.</param>
        /// <returns></returns>
        private static T ConvertFromString<T>(string value)
        {
            var converter = TypeDescriptor.GetConverter(typeof(T));
            if (converter != null)
            {
                // This will throw a NotSupportedException if no conversion is possible.
                return (T)converter.ConvertFromString(value);
            }
            // If there is no converter, no conversion is possible, so throw a NotSupportedException.
            throw new NotSupportedException("No converter exists from string to " + typeof(T).Name + "!");
        }

        public async Task<T> GetOrThrow<T>(string key)
        {
            var argumentString = await Get(key);
            return ConvertFromString<T>(argumentString);
        }

        public async Task<T> GetOrDefault<T>(string key, T defaultValue = default(T))
        {
            try
            {
                return await GetOrThrow<T>(key);
            }
            catch (ArgumentNullException)
            {
                // The value for the specified key is null or empty.
            }
            catch (KeyNotFoundException)
            {
                // The specified key was not found in the arguments.
            }
            catch (NotSupportedException)
            {
                // Could not convert an object of type string into an object of type T.
            }
            return defaultValue;
        }

        public T GetOrThrowSync<T>(string key)
        {
            if (!_arguments.ContainsKey(key)) throw new KeyNotFoundException("Could not find key " + key + "!");
            if (string.IsNullOrEmpty(_cachedArgumentValues[key])) throw new ArgumentNullException("Value for key " + key + " is null or empty!");
            return ConvertFromString<T>(_cachedArgumentValues[key]);
        }

        public T GetOrDefaultSync<T>(string key, T defaultValue = default(T))
        {
            try
            {
                return GetOrThrowSync<T>(key);
            }
            catch (ArgumentNullException)
            {
                // The value for the specified key is null or empty.
            }
            catch (KeyNotFoundException)
            {
                // The specified key was not found in the arguments.
            }
            catch (NotSupportedException)
            {
                // Could not convert an object of type string into an object of type T.
            }
            return defaultValue;
        }
    }
}
