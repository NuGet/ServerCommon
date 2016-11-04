// Copyright (c) .NET Foundation. All rights reserved. 
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information. 

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Newtonsoft.Json.Linq;

namespace NuGet.Services.Configuration
{
    /// <summary>
    /// Load configuration files pulled from Azure blob storage
    /// </summary>
    public class AzureStorageConfigurationProvider : IConfigurationProvider
    {
        private string _configurationContainerName;
        private StorageCredentials _storageCredentials;
        private CloudStorageAccount _storageAccount;
        private IDictionary<string, JObject> _fileToParsedMap;
        private IDictionary<string, string> _configuration;

        public AzureStorageConfigurationProvider(string storageAccountName, string storageKeyValue, string configurationContainerName)
        {
            _configurationContainerName = configurationContainerName;
            _storageCredentials = new StorageCredentials(storageAccountName, storageKeyValue);
            _storageAccount = new CloudStorageAccount(_storageCredentials, useHttps: true);
            _fileToParsedMap = new Dictionary<string, JObject>();
            _configuration = new Dictionary<string, string>();
        }

        private static KeyNotFoundException GetKeyNotFoundException(string key)
        {
            return new KeyNotFoundException("Could not find key " + key + "!");
        }

        private static ArgumentNullException GetArgumentNullException(string key)
        {
            return new ArgumentNullException("Value for key " + key + " is null or empty!");
        }

        /// <summary>
        /// Pulls and primes configuration json from the configured blob storage account
        /// </summary>
        /// <param name="blobName">The name of the configuration json to load.</param>
        /// <param name="forceReload">Indicates that we should force refetch from network.</param>
        /// <returns>True if blob is loaded/was already loaded. False if blob fails to load.</returns>
        /// <remarks>Note that this method does not actually load the configuration values for retrieval. It only primes the file into memory from blob storage. Call LoadConfigurationForKey to actually load the configuration for retrieval.</remarks>
        public bool PrimeConfigurationBlob(string blobName, bool forceReload = false)
        {
            try
            {
                if (!_fileToParsedMap.ContainsKey(blobName) || forceReload)
                {
                    var blobClient = _storageAccount.CreateCloudBlobClient();
                    var container = blobClient.GetContainerReference(_configurationContainerName);
                    var blockBlob = container.GetBlockBlobReference(blobName);

                    var retrievedConfiguration = blockBlob.DownloadText();

                    var parsedConfiguration = JObject.Parse(retrievedConfiguration);

                    _fileToParsedMap.Add(blobName, parsedConfiguration);
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Loads values from primed configuration json files into our actual configuration for retrieval.
        /// </summary>
        /// <param name="primaryKey">The name of the key that should be loaded from the parsed json config files.</param>
        /// <returns>True if config was loaded without exception. False if there was a problem loading. This likely means that we primed a json file that is valid json, but in a format that we don't know about.</returns>
        public bool LoadConfigurationForKey(string primaryKey)
        {
            _configuration = new Dictionary<string, string>();

            try
            {
                foreach (var rawConfig in _fileToParsedMap.Values)
                {
                    var primaryKeyRawConfig = rawConfig[primaryKey];
                    if (primaryKeyRawConfig != null)
                    {
                        foreach (var key in primaryKeyRawConfig)
                        {
                            _configuration.Add(key.ToObject<string>(), primaryKeyRawConfig[key].ToObject<string>());
                        }
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Updates either the storage key or the config container
        /// </summary>
        /// <param name="storageKeyValue">New storage key value.</param>
        /// <param name="configurationContainerName">New container to check for config files.</param>
        /// <remarks>Updating ConfigurationContainerName does NOT force a reload of currently loaded config files. To reload a file that has already been loaded from another container, call PrimeConfigurationBlob with forceReload = true.</remarks>
        public void Update(string storageKeyValue = null, string configurationContainerName = null)
        {
            if (configurationContainerName != null)
            {
                _configurationContainerName = configurationContainerName;
            }

            if (storageKeyValue != null)
            {
                _storageAccount.Credentials.UpdateKey(storageKeyValue);
            }
        }

        /// <summary>
        /// Gets an argument
        /// </summary>
        /// <param name="key">The key associated with the desired argument.</param>
        /// <returns>The argument associated with the given key.</returns>
        /// <exception cref="KeyNotFoundException">Thrown when the key is not found in the list of arguments.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the argument associated with the given key is null or empty.</exception>
        protected virtual async Task<string> Get(string key)
        {
            if (!_configuration.ContainsKey(key))
            {
                throw GetKeyNotFoundException(key);
            }

            return _configuration[key];
        }

        public async Task<T> GetOrThrow<T>(string key)
        {
            var argumentString = await Get(key);
            return ConfigurationUtility.ConvertFromString<T>(argumentString);
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
            if (!_configuration.ContainsKey(key))
            {
                throw GetKeyNotFoundException(key);
            }

            if (string.IsNullOrEmpty(_configuration[key]))
            {
                throw GetArgumentNullException(key);
            }

            return ConfigurationUtility.ConvertFromString<T>(_configuration[key]);
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