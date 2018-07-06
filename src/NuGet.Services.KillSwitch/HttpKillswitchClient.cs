// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace NuGet.Services.KillSwitch
{
    /// <summary>
    /// A killswitch service that queries an HTTP service periodically for the list of active killswitches.
    /// </summary>
    public class HttpKillswitchClient : IKillswitchClient
    {
        private readonly HttpClient _httpClient;
        private readonly HttpKillswitchConfig _config;
        private readonly ILogger<HttpKillswitchClient> _logger;

        private int _started;
        private KillswitchCache _cache;

        public HttpKillswitchClient(
            HttpClient httpClient,
            HttpKillswitchConfig config,
            ILogger<HttpKillswitchClient> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _started = 0;
            _cache = null;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Ensure that this client has not already been started.
            if (Interlocked.CompareExchange(ref _started, 1, 0) != 0)
            {
                _logger.LogError("The killswitch client has already been started!");

                throw new InvalidOperationException("The killswitch client has already been started!");
            }

            // Prime the killswitch cache.
            await RefreshAsync(cancellationToken);

            // Refresh the killswitch cache on a background thread periodically.
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Run(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        await Task.Delay(_config.RefreshInterval);
                        await RefreshAsync(cancellationToken);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(0, e, "Failed to refresh the killswitch cache due to exception");
                    }
                }
            });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        public async Task RefreshAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            _logger.LogInformation("Refreshing killswitches...");

            var duration = Stopwatch.StartNew();
            var response = await _httpClient.GetAsync(_config.KillswitchEndpoint, cancellationToken);
            var content = await response.Content.ReadAsStringAsync();

            var items = JsonConvert.DeserializeObject<List<string>>(content);

            _cache = new KillswitchCache(items);

            _logger.LogInformation("Refreshed killswitches after {ElapsedTime}", duration.Elapsed);
        }

        public bool IsActive(string name)
        {
            var cache = _cache;

            if (cache == null)
            {
                _logger.LogError("Cannot check the killswitch {Name} as the client isn't started", name);

                throw new InvalidOperationException("Cannot check the killswitch as the client isn't started");
            }

            if (cache.Staleness.Elapsed >= _config.MaximumStaleness)
            {
                _logger.LogError(
                    "Failed to determine whether the killswitch {Name} is active as the client's cache is {Staleness} stale",
                    name,
                    cache.Staleness.Elapsed);

                throw new InvalidOperationException("Reached maximum killswitch staleness threshold");
            }

            return cache.Items.Contains(name);
        }

        private class KillswitchCache
        {
            public KillswitchCache(List<string> items)
            {
                Staleness = Stopwatch.StartNew();
                Items = new HashSet<string>(items, StringComparer.OrdinalIgnoreCase);
            }

            public Stopwatch Staleness { get; }
            public HashSet<string> Items { get; }
        }
    }
}
