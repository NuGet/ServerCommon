// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NuGet.Services.KillSwitch;
using Xunit;

namespace NuGet.Services.Killswitch.Tests
{
    public class HttpKillswitchClientFacts
    {
        public class TheStartAsyncMethod : FactsBase
        {
            public TheStartAsyncMethod(KillswitchIntegrationTestFixture fixture) : base(fixture) { }

            [Fact]
            public async Task StartLoadsKillswitches()
            {
                // Arrange
                var testServer = await _fixture.GetTestServerAsync();
                var target = await _fixture.GetClientAsync();

                testServer.Response = "[\"Foo.Bar\"]";

                // Act & Assert
                using (var tokenSource = new CancellationTokenSource())
                {
                    await target.StartAsync(tokenSource.Token);

                    Assert.True(target.IsActive("Foo.Bar"));

                    tokenSource.Cancel();
                }
            }

            [Fact]
            public async Task ThrowsIfAlreadyStarted()
            {
                // Arrange
                var testServer = await _fixture.GetTestServerAsync();
                var target = await _fixture.GetClientAsync();

                testServer.Response = "[\"Foo.Bar\"]";

                // Act & Assert
                using (var tokenSource = new CancellationTokenSource())
                {
                    await target.StartAsync(tokenSource.Token);

                    Assert.True(target.IsActive("Foo.Bar"));

                    var e = await Assert.ThrowsAsync<InvalidOperationException>(() => target.StartAsync(CancellationToken.None));

                    Assert.Equal("The killswitch client has already been started!", e.Message);

                    tokenSource.Cancel();
                }
            }

            [Fact]
            public async Task ThrowsOnMalformedInitialResponse()
            {
                // Arrange
                var testServer = await _fixture.GetTestServerAsync();
                var target = await _fixture.GetClientAsync();

                testServer.Response = "Foo.Bar";

                // Act & Assert
                using (var tokenSource = new CancellationTokenSource())
                {
                    await Assert.ThrowsAsync<JsonReaderException>(() => target.StartAsync(tokenSource.Token));

                    var e = Assert.Throws<InvalidOperationException>(() => target.IsActive("Foo.Bar"));

                    Assert.Equal("Cannot check the killswitch as the client isn't started", e.Message);

                    tokenSource.Cancel();
                }
            }

            [Fact]
            public async Task StartRefreshesKillswitchesInBackground()
            {
                // Arrange
                var config = new HttpKillswitchConfig { RefreshInterval = TimeSpan.FromMilliseconds(10) };

                var testServer = await _fixture.GetTestServerAsync();
                var target = await _fixture.GetClientAsync(config);

                testServer.Response = "[\"Foo.Bar\"]";

                // Act & Assert
                using (var tokenSource = new CancellationTokenSource())
                {
                    await target.StartAsync(tokenSource.Token);

                    Assert.True(target.IsActive("Foo.Bar"));
                    Assert.False(target.IsActive("Hello.World"));

                    testServer.Response = "[\"hello.world\"]";

                    await Task.Delay(TimeSpan.FromSeconds(1));

                    Assert.False(target.IsActive("Foo.Bar"));
                    Assert.True(target.IsActive("Hello.World"));

                    tokenSource.Cancel();
                }
            }

            [Fact]
            public async Task DoesNotThrowOnMalformedSubsequentResponses()
            {
                // Arrange
                var config = new HttpKillswitchConfig { RefreshInterval = TimeSpan.FromMilliseconds(10) };

                var testServer = await _fixture.GetTestServerAsync();
                var target = await _fixture.GetClientAsync(config);

                testServer.Response = "[\"Foo.Bar\"]";

                // Act & Assert
                using (var tokenSource = new CancellationTokenSource())
                {
                    await target.StartAsync(tokenSource.Token);

                    Assert.True(target.IsActive("Foo.Bar"));
                    Assert.False(target.IsActive("Hello.World"));

                    testServer.Response = "Hello.World";

                    await Task.Delay(TimeSpan.FromSeconds(1));

                    Assert.True(target.IsActive("Foo.Bar"));
                    Assert.False(target.IsActive("Hello.World"));

                    testServer.Response = "[\"Hello.World\"]";

                    await Task.Delay(TimeSpan.FromSeconds(1));

                    Assert.False(target.IsActive("Foo.Bar"));
                    Assert.True(target.IsActive("Hello.World"));

                    tokenSource.Cancel();
                }
            }

            [Fact]
            public async Task BackgroundRefreshRespectsCancellationToken()
            {
                // Arrange
                var config = new HttpKillswitchConfig { RefreshInterval = TimeSpan.FromMilliseconds(10) };

                var testServer = await _fixture.GetTestServerAsync();
                var target = await _fixture.GetClientAsync(config);

                testServer.Response = "[\"Foo.Bar\"]";

                // Act & Assert
                using (var tokenSource = new CancellationTokenSource())
                {
                    await target.StartAsync(tokenSource.Token);

                    Assert.True(target.IsActive("Foo.Bar"));
                    Assert.False(target.IsActive("Hello.World"));

                    tokenSource.Cancel();
                    await Task.Delay(TimeSpan.FromSeconds(1));

                    testServer.Response = "[\"Hello.World\"]";
                    await Task.Delay(TimeSpan.FromSeconds(1));

                    Assert.True(target.IsActive("Foo.Bar"));
                    Assert.False(target.IsActive("Hello.World"));
                }
            }
        }

        public class TheRefreshAsyncMethod : FactsBase
        {
            public TheRefreshAsyncMethod(KillswitchIntegrationTestFixture fixture) : base(fixture) { }

            [Fact]
            public async Task ThrowsIfCancelled()
            {
                // Arrange
                var testServer = await _fixture.GetTestServerAsync();
                var target = await _fixture.GetClientAsync();

                testServer.Response = "[\"Foo.Bar\"]";

                using (var tokenSource = new CancellationTokenSource())
                {
                    // Act & Assert
                    await target.RefreshAsync(tokenSource.Token);

                    Assert.True(target.IsActive("Foo.Bar"));

                    tokenSource.Cancel();

                    await Assert.ThrowsAsync<OperationCanceledException>(() => target.RefreshAsync(tokenSource.Token));
                }
            }

            [Fact]
            public async Task Refreshes()
            {
                // Arrange
                var testServer = await _fixture.GetTestServerAsync();
                var target = await _fixture.GetClientAsync();

                testServer.Response = "[\"Foo.Bar\"]";

                // Act & Assert
                await target.RefreshAsync(CancellationToken.None);

                Assert.True(target.IsActive("Foo.Bar"));
            }

            [Fact]
            public async Task IfServerResponseIsMalformed_Throws()
            {
                // Arrange
                var testServer = await _fixture.GetTestServerAsync();
                var target = await _fixture.GetClientAsync();

                testServer.Response = "Foo.Bar";

                // Act & Assert
                await Assert.ThrowsAsync<JsonReaderException>(() => target.RefreshAsync(CancellationToken.None));

                var e = Assert.Throws<InvalidOperationException>(() => target.IsActive("Foo.Bar"));

                Assert.Equal("Cannot check the killswitch as the client isn't started", e.Message);
            }

            [Fact]
            public async Task ManualRefreshAllowedAfterStart()
            {
                // Arrange
                var config = new HttpKillswitchConfig { RefreshInterval = TimeSpan.FromDays(10) };

                var testServer = await _fixture.GetTestServerAsync();
                var target = await _fixture.GetClientAsync(config);

                testServer.Response = "[\"Foo.Bar\"]";

                // Act & Assert
                using (var tokenSource = new CancellationTokenSource())
                {
                    await target.StartAsync(tokenSource.Token);

                    Assert.True(target.IsActive("Foo.Bar"));
                    Assert.False(target.IsActive("Hello.World"));

                    testServer.Response = "[\"hello.world\"]";

                    await target.RefreshAsync(CancellationToken.None);

                    Assert.False(target.IsActive("Foo.Bar"));
                    Assert.True(target.IsActive("Hello.World"));

                    tokenSource.Cancel();
                }
            }
        }

        public class TheIsActiveMethod : FactsBase
        {
            public TheIsActiveMethod(KillswitchIntegrationTestFixture fixture) : base(fixture) { }

            [Fact]
            public async Task ThrowsIfKillswitchesNotLoaded()
            {
                var testServer = await _fixture.GetTestServerAsync();
                var target = await _fixture.GetClientAsync();

                testServer.Response = "[\"Foo.Bar\"]";

                var e = Assert.Throws<InvalidOperationException>(() => target.IsActive("Foo.Bar"));

                Assert.Equal("Cannot check the killswitch as the client isn't started", e.Message);
            }

            [Fact]
            public async Task ReturnsTrueForActiveKillswitches()
            {
                // Arrange
                var testServer = await _fixture.GetTestServerAsync();
                var target = await _fixture.GetClientAsync();

                testServer.Response = "[\"Foo.Bar\"]";

                // Act & Assert
                await target.RefreshAsync(CancellationToken.None);

                Assert.True(target.IsActive("Foo.Bar"));
                Assert.True(target.IsActive("foo.bar"));
                Assert.False(target.IsActive("Hello.World"));

                testServer.Response = "[\"hello.world\"]";

                await target.RefreshAsync(CancellationToken.None);

                Assert.False(target.IsActive("Foo.Bar"));
                Assert.False(target.IsActive("foo.bar"));
                Assert.True(target.IsActive("Hello.World"));

                testServer.Response = "[]";

                await target.RefreshAsync(CancellationToken.None);

                Assert.False(target.IsActive("Foo.Bar"));
                Assert.False(target.IsActive("foo.bar"));
                Assert.False(target.IsActive("Hello.World"));
            }

            [Fact]
            public async Task ThrowsIfPastStalenessThreshold()
            {
                // Arrange
                var config = new HttpKillswitchConfig
                {
                    RefreshInterval = TimeSpan.FromMilliseconds(10),
                    MaximumStaleness = TimeSpan.FromMilliseconds(100),
                };

                var testServer = await _fixture.GetTestServerAsync();
                var target = await _fixture.GetClientAsync(config);

                testServer.Response = "[\"Foo.Bar\"]";

                // Act & Assert
                using (var tokenSource = new CancellationTokenSource())
                {
                    await target.StartAsync(tokenSource.Token);

                    Assert.True(target.IsActive("Foo.Bar"));
                    Assert.False(target.IsActive("Hello.World"));

                    testServer.Response = "This response fails all refresh attempts thereby making the cache stale";

                    await Task.Delay(TimeSpan.FromSeconds(1));

                    var e = Assert.Throws<InvalidOperationException>(() => target.IsActive("Foo.Bar"));

                    Assert.Equal("Reached maximum killswitch staleness threshold", e.Message);

                    tokenSource.Cancel();
                }
            }
        }

        [Collection(KillswitchIntegrationTestCollection.Name)]
        public class FactsBase
        {
            protected readonly KillswitchIntegrationTestFixture _fixture;

            public FactsBase(KillswitchIntegrationTestFixture fixture)
            {
                _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
            }
        }
    }
}
