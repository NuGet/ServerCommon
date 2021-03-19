// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace NuGet.Services.KeyVault.Tests
{
    public class CachingBackgroundRefreshingSecretReaderFacts : IDisposable
    {
        [Fact]
        public void ReadsAllSecrets()
        {
            var secretList = new[] {
                "secret1",
                "123",
                "someOtherSecret"
            };

            var readSecrets = new HashSet<string>();

            _secretReaderMock
                .Setup(sr => sr.GetSecretObjectAsync(It.IsAny<string>(), _loggerMock.Object))
                .Callback<string, ILogger>((secretName, _) => readSecrets.Add(secretName))
                .ReturnsAsync(new KeyVaultSecret("secretName", "secretValue", null));

            var target = CreateTarget(secretList, refreshInterval: TimeSpan.MaxValue, backgroundThreadSleepInterval: TimeSpan.FromSeconds(1000));
            target.WaitForTheFirstRefresh();

            Assert.Equal(secretList.Length, readSecrets.Count);
            readSecrets.IntersectWith(secretList);
            Assert.Equal(secretList.Length, readSecrets.Count);
        }

        [Fact]
        public void ThrowsOnAccessingUnknownSecret()
        {
            var target = CreateTarget(new string[0], TimeSpan.MaxValue, TimeSpan.FromSeconds(1000));
            target.WaitForTheFirstRefresh();

            var ex = Record.Exception(() => target.GetSecret("unknownSecret"));
            Assert.IsType<InvalidOperationException>(ex);
            Assert.Contains("unknownSecret", ex.Message);
        }

        [Fact]
        public void QueuesUnknownSecretsForRefresh()
        {
            var target = CreateTarget(new string[0], TimeSpan.MaxValue, TimeSpan.FromSeconds(1000));
            target.WaitForTheFirstRefresh();

            const string secretName = "unknownSecret";
            const string secretValue = "secretValue";

            var mre = new ManualResetEventSlim(false);

            _secretReaderMock
                .Setup(sr => sr.GetSecretObjectAsync(secretName, It.IsAny<ILogger>()))
                .ReturnsAsync(new KeyVaultSecret(secretName, secretValue, null))
                .Callback(() => mre.Set());

            Record.Exception(() => target.GetSecret(secretName));
            Assert.True(mre.Wait(1000));
            var newValue = target.GetSecret(secretName);
            Assert.Equal(secretValue, newValue);
        }

        [Fact]
        public async Task RefreshesExpiredSecretsAtExpectedCadence()
        {
            const string secretName = "secretName";
            var target = CreateTarget(new[] { secretName }, TimeSpan.FromMilliseconds(500), TimeSpan.FromMilliseconds(100));
            target.WaitForTheFirstRefresh();

            _secretReaderMock
                .Setup(sr => sr.GetSecretObjectAsync(secretName, It.IsAny<ILogger>()))
                .ReturnsAsync(new KeyVaultSecret(secretName, "someSecretValue", null));

            await Task.Delay(1600);

            // target.WaitForTheFirstRefresh() should refresh secrets once, then during 1600 ms delay above
            // background refresh task should refresh the secret 3 times (every 500 ms), we'll give
            // it some slack and say it might be 2, too; so the final expected number of calls is 3 to 4.
            _secretReaderMock
                .Verify(sr => sr.GetSecretObjectAsync(secretName, It.IsAny<ILogger>()), Times.AtLeast(3));
            _secretReaderMock
                .Verify(sr => sr.GetSecretObjectAsync(secretName, It.IsAny<ILogger>()), Times.AtMost(4));
        }

        [Fact]
        public void ConstructorThrowsWhenSecretReaderIsNull()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new CachingBackgroundRefreshingSecretReader(
                null,
                _loggerMock.Object,
                _telemetryServiceMock.Object,
                (_) => { },
                new string[0],
                TimeSpan.MaxValue,
                TimeSpan.Zero));

            Assert.Equal("underlyingSecretReader", ex.ParamName);
        }

        [Fact]
        public void ConstructorThrowsWhenLoggerIsNull()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new CachingBackgroundRefreshingSecretReader(
                _secretReaderMock.Object,
                null,
                _telemetryServiceMock.Object,
                (_) => { },
                new string[0],
                TimeSpan.MaxValue,
                TimeSpan.Zero));

            Assert.Equal("logger", ex.ParamName);
        }

        [Fact]
        public void ConstructorThrowsWhenTelemetryServiceIsNull()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new CachingBackgroundRefreshingSecretReader(
                _secretReaderMock.Object,
                _loggerMock.Object,
                null,
                (_) => { },
                new string[0],
                TimeSpan.MaxValue,
                TimeSpan.Zero));

            Assert.Equal("telemetryService", ex.ParamName);
        }

        [Fact]
        public void ConstructorThrowsWhenBackgroundTaskStarterIsNull()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new CachingBackgroundRefreshingSecretReader(
                _secretReaderMock.Object,
                _loggerMock.Object,
                _telemetryServiceMock.Object,
                null,
                new string[0],
                TimeSpan.MaxValue,
                TimeSpan.Zero));

            Assert.Equal("backgroundTaskStarter", ex.ParamName);
        }

        [Fact]
        public void ConstructorThrowsWhenSecretNamesIsNull()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new CachingBackgroundRefreshingSecretReader(
                _secretReaderMock.Object,
                _loggerMock.Object,
                _telemetryServiceMock.Object,
                (_) => { },
                null,
                TimeSpan.MaxValue,
                TimeSpan.Zero));

            Assert.Equal("secretNames", ex.ParamName);
        }

        private Mock<ISecretReader> _secretReaderMock;
        private Mock<ILogger> _loggerMock;
        private Mock<ICachingBackgroundRefreshingSecretReaderTelemetryService> _telemetryServiceMock;
        private CancellationTokenSource _backgroundThreadCancellationTokenSource;
        private Task _backgroundRefreshTask;

        public CachingBackgroundRefreshingSecretReaderFacts()
        {
            _secretReaderMock = new Mock<ISecretReader>();
            _loggerMock = new Mock<ILogger>();
            _telemetryServiceMock = new Mock<ICachingBackgroundRefreshingSecretReaderTelemetryService>();
            _backgroundThreadCancellationTokenSource = new CancellationTokenSource();
        }

        private CachingBackgroundRefreshingSecretReader CreateTarget(
            ICollection<string> secretNames,
            TimeSpan? refreshInterval = null,
            TimeSpan? backgroundThreadSleepInterval = null)
        {
            return new CachingBackgroundRefreshingSecretReader(
                _secretReaderMock.Object,
                _loggerMock.Object,
                _telemetryServiceMock.Object,
                StartBackgroundTask,
                secretNames,
                refreshInterval ?? TimeSpan.MaxValue,
                backgroundThreadSleepInterval ?? TimeSpan.Zero);
        }

        private void StartBackgroundTask(Func<CancellationToken, Task> backgroundTask)
        {
            _backgroundRefreshTask = Task.Run(async () => await backgroundTask(_backgroundThreadCancellationTokenSource.Token));
        }

        public void Dispose()
        {
            _backgroundThreadCancellationTokenSource.Cancel();
            if (_backgroundRefreshTask != null)
            {
                _backgroundRefreshTask.Wait();
            }
            _backgroundThreadCancellationTokenSource.Dispose();
        }
    }
}
