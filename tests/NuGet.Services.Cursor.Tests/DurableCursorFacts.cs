// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using NuGet.Services.Storage;
using Xunit;

namespace NuGet.Services.Cursor.Tests
{
    public class DurableCursorFacts
    {
        [Fact]
        public void SavesToStorage()
        {
            var storageMock = CreateStorageMock();
            storageMock
                .Protected().Setup<Task>("OnSave", ItExpr.IsAny<Uri>(), ItExpr.IsAny<StorageContent>(), ItExpr.IsAny<CancellationToken>())
                .Returns(Task.FromResult(0))
                .Verifiable();

            var durableCursor = new DurableCursor(new Uri("http://localhost/cursor.json"), storageMock.Object, new DateTimeOffset(2017, 5, 5, 17, 8, 42, TimeSpan.Zero));
            durableCursor.Save(CancellationToken.None).Wait();
            storageMock.Verify();
        }

        [Fact]
        public void LoadsFromStorage()
        {
            var storageMock = CreateStorageMock();
            storageMock
                .Protected().Setup<Task>("OnLoad", ItExpr.IsAny<Uri>(), ItExpr.IsAny<CancellationToken>())
                .Returns(Task.FromResult<StorageContent>(null))
                .Verifiable();

            var durableCursor = new DurableCursor(new Uri("http://localhost/cursor.json"), storageMock.Object, new DateTimeOffset(2017, 5, 5, 17, 8, 42, TimeSpan.Zero));
            durableCursor.Load(CancellationToken.None).Wait();
            storageMock.Verify();
        }

        [Fact]
        public void UsesDefaultValue()
        {
            var storageMock = CreateStorageMock();
            storageMock
                .Protected().Setup<Task>("OnLoad", ItExpr.IsAny<Uri>(), ItExpr.IsAny<CancellationToken>())
                .Returns(Task.FromResult<StorageContent>(null));

            DateTimeOffset defaultValue = new DateTimeOffset(2017, 5, 5, 17, 8, 42, TimeSpan.Zero);
            var durableCursor = new DurableCursor(new Uri("http://localhost/cursor.json"), storageMock.Object, defaultValue);
            durableCursor.Load(CancellationToken.None).Wait();

            Assert.Equal(defaultValue, durableCursor.Value);
        }

        private static Mock<Storage.Storage> CreateStorageMock()
        {
            var loggerMock = new Mock<ILogger<Storage.Storage>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock
                .Setup(f => f.CreateLogger(It.IsAny<string>()))
                .Returns(loggerMock.Object);

            return new Mock<Storage.Storage>(MockBehavior.Strict, new Uri("http://localhost/"), loggerFactoryMock.Object);
        }
    }
}
