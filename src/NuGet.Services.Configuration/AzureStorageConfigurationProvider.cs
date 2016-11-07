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
        private string _configurationBlobName;
        private CloudStorageAccount _storageAccount;
        private IDictionary<string, string> _configuration;

        public AzureStorageConfigurationProvider(string storageAccountName, string storageKeyValue, string configurationContainerName, string configurationBlobName)
        {
            _configurationContainerName = configurationContainerName;
            _configurationBlobName = configurationBlobName;

            var storageCredentials = new StorageCredentials(storageAccountName, storageKeyValue);
            _storageAccount = new CloudStorageAccount(storageCredentials, useHttps: true);

            LoadConfiguration();
        }

        public AzureStorageConfigurationProvider(string connectionString, string configurationContainerName, string configurationBlobName)
        {
            _configurationContainerName = configurationContainerName;
            _configurationBlobName = configurationBlobName;
            _storageAccount = CloudStorageAccount.Parse(connectionString);

            LoadConfiguration();
        }

        /// <summary>
        /// Pulls and loads configuration json from the configured blob storage account and file
        /// </summary>
        /// <returns>True if blob is loaded. False if blob fails to load.</returns>
        public void LoadConfiguration()
        {
            _configuration = new Dictionary<string, string>();

            var blobClient = _storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(_configurationContainerName);
            var blockBlob = container.GetBlockBlobReference(_configurationBlobName);

            var retrievedConfiguration = blockBlob.DownloadText();

            var parsedConfiguration = JObject.Parse(retrievedConfiguration);

            foreach (var key in parsedConfiguration)
            {
                _configuration.Add(key.ToString(), parsedConfiguration[key].ToString());
            }
        }

        /// <summary>
        /// Updates either the storage key or the config container
        /// </summary>
        /// <param name="connectionString">New connection string value.</param>
        /// <param name="storageKeyValue">New storage key value.</param>
        /// <param name="configurationContainerName">New container to check for config files.</param>
        /// <param name="configurationBlobName">New configuration blob name.</param>
        /// <remarks>Updating ConfigurationContainerName or ConfigurationBlobName will force a reload of configuration.</remarks>
        public void Update(string connectionString = null, string storageKeyValue = null, string configurationContainerName = null, string configurationBlobName = null)
        {
            bool needConfigReload = false;

            if (configurationBlobName != null)
            {
                _configurationBlobName = configurationBlobName;
                needConfigReload |= true;
            }

            if (configurationContainerName != null)
            {
                _configurationContainerName = configurationContainerName;
                needConfigReload |= true;
            }

            if (storageKeyValue != null)
            {
                _storageAccount.Credentials.UpdateKey(storageKeyValue);
            }

            if (connectionString != null)
            {
                _storageAccount = CloudStorageAccount.Parse(connectionString);
            }

            if (needConfigReload)
            {
                LoadConfiguration();
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
                throw new KeyNotFoundException($"Could not find key {key}!");
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
                throw new KeyNotFoundException($"Could not find key {key}!");
            }

            if (string.IsNullOrEmpty(_configuration[key]))
            {
                throw new ArgumentNullException($"Value for key {key} is null or empty!");
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