// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NuGet.Services.KillSwitch;

namespace NuGet.Services.Killswitch.Tests
{
    public class KillswitchIntegrationTestFixture : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly Lazy<Task<KillswitchTestServer>> _testServer;

        public KillswitchIntegrationTestFixture()
        {
            _httpClient = new HttpClient();
            _testServer = new Lazy<Task<KillswitchTestServer>>(KillswitchTestServer.CreateAsync);
        }

        public HttpClient GetHttpClient() => _httpClient;
        public Task<KillswitchTestServer> GetTestServerAsync() => _testServer.Value;

        public async Task<HttpKillswitchClient> GetClientAsync(HttpKillswitchConfig config = null)
        {
            var testServer = await GetTestServerAsync();

            config = config ?? new HttpKillswitchConfig();
            config.KillswitchEndpoint = testServer.Url;

            return new HttpKillswitchClient(_httpClient, config, Mock.Of<ILogger<HttpKillswitchClient>>());
        }

        public void Dispose()
        {
            _httpClient.Dispose();

            if (_testServer.IsValueCreated)
            {
                _testServer.Value.Result.Dispose();
            }
        }
    }
}
