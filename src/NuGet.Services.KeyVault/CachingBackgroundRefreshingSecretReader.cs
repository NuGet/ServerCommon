// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace NuGet.Services.KeyVault
{
    /// <summary>
    /// An <see cref="ISyncSecretReader"/> implementation that uses background task to refresh
    /// secrets and produces cached values each times a value is requested.
    /// </summary>
    public class CachingBackgroundRefreshingSecretReader
        : ISyncSecretReader
    {
        private const int DefaultRefreshIntervalSec = 60 * 60 * 24; // 1 day
        private const int DefaultBackgroundSleepIntervalSec = 60;

        private readonly ISecretReader _underlyingSecretReader;
        private readonly ILogger _logger;
        private readonly ConcurrentDictionary<string, CachedSecret> _cachedSecrets;
        private readonly ICachingBackgroundRefreshingSecretReaderTelemetryService _telemetryService;
        private readonly TimeSpan _refreshInterval;
        private readonly TimeSpan _backgroundTaskSleepInterval;

        /// <summary>
        /// An event that gets signaled upon the first successful secret refresh. Allows to wait
        /// until the object gets ready to serve requests.
        /// </summary>
        private readonly ManualResetEventSlim _firstRefreshComplete = new ManualResetEventSlim(false);

        /// <summary>
        /// The cancellation token source used to signal background task that we encountered
        /// an unexpected secret and would want to retrieve its value ASAP. This is an emergency
        /// mechanism and all occurrences of it should be logged and analyzed. Normally, the
        /// complete list of secrets to be used is supposed to be passed to the constructor.
        /// </summary>
        private CancellationTokenSource _forceRefresh = new CancellationTokenSource();

        /// <summary>
        /// The object constructor.
        /// </summary>
        /// <param name="underlyingSecretReader">The actual secret reader that would provide secret values to a background task.</param>
        /// <param name="backgroundTaskStarter">The hook into caller's infrastructure to run background tasks. A function that accepts a
        /// function that runs a background secret refresh task and makes sure background task infrastructure executes it. See example below.</param>
        /// <param name="secretNames">The list of secret names to maintain.</param>
        /// <param name="refreshInterval">The time for which secret values are going to be EditableFeatureFlagFileStorageServicecached.</param>
        /// <param name="backgroundTaskSleepInterval">The sleep time of the background task. Defines the
        /// frequency of expiration checks for the cache items.</param>
        /// <param name="logger">Logger.</param>
        /// <example>
        /// // In ASP.NET environment the value of <paramref name="backgroundTaskStarter"/> could be using
        /// // HostingEnvironment.QueueBackgroundWorkItem to start the background task:
        /// var secretReader = new CachingBackgroundRefreshingSecretReader(
        ///     actualSecretReader,
        ///     HostingEnvironment.QueueBackgroundWorkItem,
        ///     logger);
        /// </example>
        public CachingBackgroundRefreshingSecretReader(
            ISecretReader underlyingSecretReader,
            Action<Func<CancellationToken, Task>> backgroundTaskStarter,
            ICollection<string> secretNames,
            ICachingBackgroundRefreshingSecretReaderTelemetryService telemetryService = null,
            ILogger logger = null,
            TimeSpan? refreshInterval = null,
            TimeSpan? backgroundTaskSleepInterval = null)
        {
            _underlyingSecretReader = underlyingSecretReader ?? throw new ArgumentNullException(nameof(underlyingSecretReader));
            if (backgroundTaskStarter == null)
            {
                throw new ArgumentNullException(nameof(backgroundTaskStarter));
            }
            if (secretNames == null)
            {
                throw new ArgumentNullException(nameof(secretNames));
            }
            _cachedSecrets = new ConcurrentDictionary<string, CachedSecret>(secretNames.Select(s => new KeyValuePair<string, CachedSecret>(s, null)));
            _logger = logger;
            _telemetryService = telemetryService;
            _refreshInterval = refreshInterval ?? TimeSpan.FromSeconds(DefaultRefreshIntervalSec);
            _backgroundTaskSleepInterval = backgroundTaskSleepInterval ?? TimeSpan.FromSeconds(DefaultBackgroundSleepIntervalSec);
            _logger?.LogInformation("Starting background secret refresh for {SecretsNumber} secrets", _cachedSecrets.Count());
            backgroundTaskStarter(BackgroundRefreshTaskWrapper);
        }

        /// <summary>
        /// Waits for the first secret refresh to complete without errors.
        /// Which means that every secret would get successfully retrieved from KeyVault at least once.
        /// Once the first successful retrieval is complete, this method will always immediately return if called.
        /// 
        /// Should only be used once after creating an object to make sure cache is properly populated.
        /// </summary>
        public void WaitForTheFirstRefresh()
        {
            _firstRefreshComplete.Wait();
        }

        /// <summary>
        /// Retrieves the secret value from cache.
        /// </summary>
        /// <param name="secretName">The secret name to retrieve.</param>
        /// <returns>The secret object.</returns>
        /// <remarks>
        /// Unless something went really wrong with the initialization, we should never run into a situation
        /// when a unknown secret name is passed into this function. So, the exceptions thrown in it are to 
        /// indicate that the setup was incorrect.
        /// However, if for some bizzare reason we would really miss a secret, the code would add it to the
        /// refresh queue, so while the current operation would still fail, the future ones might succeed
        /// (if the refresh occurs by that time).
        /// </remarks>
        public ISecret GetSecretObject(string secretName)
        {
            if (secretName == null)
            {
                throw new ArgumentNullException(nameof(secretName));
            }
            if (string.IsNullOrWhiteSpace(secretName))
            {
                throw new ArgumentException($"{nameof(secretName)} must not be empty", nameof(secretName));
            }
            // Given that the user of the object called WaitForTheFirstRefresh() on their own
            // this call should never block
            WaitForTheFirstRefresh();
            if (_cachedSecrets.TryGetValue(secretName, out var cachedSecret))
            {
                if (cachedSecret == null)
                {
                    // The secret key exist in the dictionary, but the stored value is null.
                    // Most likely we just queued that secret for retrieval, but we haven't got the value yet.
                    _telemetryService?.TrackUnknownSecretRequested(secretName);
                    _logger?.LogError("Encountered null cache value while retrieving secret {SecretName}", secretName);
                    throw new InvalidOperationException($"Unexpected unknown secret {secretName}");
                }
                return cachedSecret.Secret;
            }

            // We haven't found our secret in the cache. We will fail the current call with an exception
            // (normally, all the secrets should have been passed to the constructor. This *is* an exceptional
            // situation), but for the service stability we will queue retrieval of the secret, too. So if a
            // retry happens after a while, we'd be ready for it with a value.
            _telemetryService?.TrackUnknownSecretRequested(secretName);
            _logger?.LogError("Encountered new secret name {SecretName}", secretName);
            AddSecretName(secretName);
            throw new InvalidOperationException($"Unexpected unknown secret {secretName}");
        }

        public string GetSecret(string secretName)
            => GetSecretObject(secretName).Value;

        private void AddSecretName(string secretName)
        {
            // the goal is to add a key to the dictionary, background update task would take care
            // of populating the value. But, if it just had it populated, we are not going to reset it
            // back to null and keep whatever was there.
            _cachedSecrets.AddOrUpdate(secretName, (CachedSecret)null, (_, v) => v);

            // signal background task to start update immediately.
            _forceRefresh.Cancel();
        }

        private async Task BackgroundRefreshTaskWrapper(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await DoBackgroundRefreshAsync(cancellationToken);
                }
                catch (Exception e)
                {
                    _telemetryService?.TrackBackgroundRefreshTaskLeakedException();
                    _logger?.LogError(e, "Background secret refresh task leaked exception");
                }
                // Don't want to consume CPU if exception is thrown in a tight loop.
                // Ideally, we never reach this code unless we are shutting down
                if (!cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                }
            }
        }

        private async Task DoBackgroundRefreshAsync(CancellationToken cancellationToken)
        {
            _logger?.LogInformation("Starting background secret refresh task");
            while (!cancellationToken.IsCancellationRequested)
            {
                _telemetryService?.TrackSecretRefreshIteration();
                bool allRefreshesSucceeded = true;
                foreach (var keyValuePair in _cachedSecrets)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    if (keyValuePair.Value is null || IsSecretOutdated(keyValuePair.Value))
                    {
                        try
                        {
                            var value = await _underlyingSecretReader.GetSecretObjectAsync(keyValuePair.Key, _logger);
                            var cachedSecret = new CachedSecret(value);
                            _cachedSecrets.AddOrUpdate(keyValuePair.Key, cachedSecret, (_, __) => cachedSecret);
                            _logger?.LogInformation("Refreshed the value for the secret {SecretName}", keyValuePair.Key);
                            _telemetryService?.TrackSecretRefreshed(keyValuePair.Key);
                        }
                        catch (Exception ex)
                        {
                            // we'll report the exception and ignore it, so we don't leak it outside the function
                            _telemetryService?.TrackSecretRefreshFailure(keyValuePair.Key);
                            _logger?.LogError(ex, "Exception while refreshing the secret {SecretName}", keyValuePair.Key);
                            allRefreshesSucceeded = false;
                        }
                    }
                }
                if (allRefreshesSucceeded)
                {
                    _firstRefreshComplete.Set();
                }
                try
                {
                    using (var combinedSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _forceRefresh.Token))
                    {
                        await Task.Delay(_backgroundTaskSleepInterval, combinedSource.Token);
                    }
                }
                catch (TaskCanceledException)
                {
                    if (_forceRefresh.IsCancellationRequested)
                    {
                        // We are assigning to _forceRefresh and wait for it only in this thread.
                        // Other threads only access _forceRefresh to call .Cancel() on it.
                        // The worst thing that can happen here is that the Task.Delay() call
                        // above would terminate, then another thread would grab a cancelled
                        // cancellation token source before the assignment below, hold on to it
                        // past that assigment, then call .Cancel() there, then that .Cancel()
                        // would go unnoticed.
                        // The probability of this happening is very low as the timing required to
                        // trigger it is pretty tight and if that does happen, it would just delay
                        // the secret refresh by one iteration of the refresh loop. Since this is
                        // an emergency recovery mechanism that should not normally get triggered
                        // at all, we can ignore this case.

                        var oldSource = _forceRefresh;
                        _forceRefresh = new CancellationTokenSource();
                        oldSource.Dispose();
                    }
                }
            }
            _logger?.LogInformation("Background secret refresh task got a signal to terminate.");
        }

        private bool IsSecretOutdated(CachedSecret cachedSecret)
            => (DateTimeOffset.UtcNow - cachedSecret.CacheTime) >= _refreshInterval;

        private class CachedSecret
        {
            public CachedSecret(ISecret secret)
            {
                Secret = secret;
                CacheTime = DateTimeOffset.UtcNow;
            }

            public ISecret Secret { get; }
            public DateTimeOffset CacheTime { get; }
        }
    }
}
