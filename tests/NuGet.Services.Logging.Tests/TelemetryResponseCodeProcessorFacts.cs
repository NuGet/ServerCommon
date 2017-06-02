// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Web;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.Implementation;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace NuGet.Services.Logging.Tests
{
    public class TelemetryResponseCodeProcessorFacts
    {
        [Fact]
        public void Constructor_ThrowsForNullNext()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new TelemetryResponseCodeProcessor(next: null));

            Assert.Equal("next", exception.ParamName);
        }

        [Fact]
        public void SuccessfulResponseCodes_IsEmptyByDefault()
        {
            var filter = new TelemetryResponseCodeProcessor(Mock.Of<ITelemetryProcessor>());

            Assert.Empty(filter.SuccessfulResponseCodes);
        }

        [Fact]
        public void Process_ThrowsForNullItem()
        {
            var filter = new TelemetryResponseCodeProcessor(Mock.Of<ITelemetryProcessor>());

            var exception = Assert.Throws<ArgumentNullException>(() => filter.Process(item: null));

            Assert.Equal("item", exception.ParamName);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Process_SetsSuccessToTrueForRequestTelemetryWithSuccessfulResponseCode(bool actualSuccess)
        {
            var responseCode = 400;
            var telemetry = new RequestTelemetry()
            {
                ResponseCode = responseCode.ToString(),
                Success = actualSuccess
            };
            var filter = new TelemetryResponseCodeProcessor(Mock.Of<ITelemetryProcessor>());

            filter.SuccessfulResponseCodes.Add(responseCode);
            filter.Process(telemetry);

            Assert.True(telemetry.Success);
        }

        [Theory]
        [InlineData(400)]
        [InlineData(404)]
        public void Process_MultipleSuccessfulResponseCodes_SetsSuccessToTrueForRequestTelemetryWithSuccessfulResponseCode(
            int responseCode)
        {
            var telemetry = new RequestTelemetry()
            {
                ResponseCode = responseCode.ToString(),
                Success = false
            };
            var filter = new TelemetryResponseCodeProcessor(Mock.Of<ITelemetryProcessor>());

            filter.SuccessfulResponseCodes.Add(400);
            filter.SuccessfulResponseCodes.Add(404);

            filter.Process(telemetry);

            Assert.True(telemetry.Success);
        }

        [Theory]
        [InlineData(null, true)]
        [InlineData(null, false)]
        [InlineData("", true)]
        [InlineData("", false)]
        [InlineData("a", true)]
        [InlineData("a", false)]
        [InlineData("-1", true)]
        [InlineData("-1", false)]
        [InlineData("1.2", true)]
        [InlineData("1.2", false)]
        [InlineData("401", true)]
        [InlineData("401", false)]
        public void Process_DoesNotModifySuccessForRequestTelemetryWithOtherResponseCodes(
            string responseCode,
            bool actualSuccess)
        {
            var telemetry = new RequestTelemetry()
            {
                ResponseCode = responseCode,
                Success = actualSuccess
            };
            var filter = new TelemetryResponseCodeProcessor(Mock.Of<ITelemetryProcessor>());

            filter.SuccessfulResponseCodes.Add(400);
            filter.Process(telemetry);

            Assert.Equal(actualSuccess, telemetry.Success);
        }

        [Theory]
        [InlineData(400)]
        [InlineData(401)]
        public void Process_AlwaysCallsNextProcessorForRequestTelemetry(int responseCode)
        {
            var telemetry = new RequestTelemetry()
            {
                ResponseCode = responseCode.ToString(),
                Success = false
            };
            var nextProcessor = new Mock<ITelemetryProcessor>(MockBehavior.Strict);

            nextProcessor.Setup(x => x.Process(It.Is<RequestTelemetry>(rt => ReferenceEquals(rt, telemetry))));

            var filter = new TelemetryResponseCodeProcessor(nextProcessor.Object);

            filter.SuccessfulResponseCodes.Add(400);
            filter.Process(telemetry);

            nextProcessor.Verify(
                x => x.Process(It.Is<RequestTelemetry>(rt => ReferenceEquals(rt, telemetry))),
                Times.Once());
        }

        [Fact]
        public void Process_CallsNextProcessorForExceptionTelemetryWithNullException()
        {
            var telemetry = new ExceptionTelemetry();

            // By default this property is null.
            // However, assert this fact anyway to guard against an unexpected change in the dependency
            // which would invalidate the setup for this test.
            Assert.Null(telemetry.Exception);

            var nextProcessor = new Mock<ITelemetryProcessor>(MockBehavior.Strict);

            nextProcessor.Setup(x => x.Process(It.Is<ExceptionTelemetry>(rt => ReferenceEquals(rt, telemetry))));

            var filter = new TelemetryResponseCodeProcessor(nextProcessor.Object);

            filter.Process(telemetry);

            nextProcessor.Verify(
                x => x.Process(It.Is<ExceptionTelemetry>(rt => ReferenceEquals(rt, telemetry))),
                Times.Once());
        }

        [Fact]
        public void Process_CallsNextProcessorForExceptionTelemetryWithExceptionThatIsNotHttpException()
        {
            var telemetry = new ExceptionTelemetry(new NullReferenceException());
            var nextProcessor = new Mock<ITelemetryProcessor>(MockBehavior.Strict);

            nextProcessor.Setup(x => x.Process(It.Is<ExceptionTelemetry>(rt => ReferenceEquals(rt, telemetry))));

            var filter = new TelemetryResponseCodeProcessor(nextProcessor.Object);

            filter.Process(telemetry);

            nextProcessor.Verify(
                x => x.Process(It.Is<ExceptionTelemetry>(rt => ReferenceEquals(rt, telemetry))),
                Times.Once());
        }

        [Theory]
        [InlineData(500)]
        [InlineData(503)]
        public void Process_CallsNextProcessorForExceptionTelemetryWithHttpStatusCodeGreaterThanOrEqualTo500(
            int statusCode)
        {
            var telemetry = new ExceptionTelemetry(new HttpException(statusCode, message: "a"));
            var nextProcessor = new Mock<ITelemetryProcessor>(MockBehavior.Strict);

            nextProcessor.Setup(x => x.Process(It.Is<ExceptionTelemetry>(rt => ReferenceEquals(rt, telemetry))));

            var filter = new TelemetryResponseCodeProcessor(nextProcessor.Object);

            filter.Process(telemetry);

            nextProcessor.Verify(
                x => x.Process(It.Is<ExceptionTelemetry>(rt => ReferenceEquals(rt, telemetry))),
                Times.Once());
        }

        [Fact]
        public void Process_CallsNextProcessorWithTraceTelemetryForExceptionTelemetryWithHttpStatusCodeLessThan500()
        {
            var telemetry = new ExceptionTelemetry(new HttpException(httpCode: 499, message: "a"));
            var nextProcessor = new Mock<ITelemetryProcessor>(MockBehavior.Strict);

            nextProcessor.Setup(x => x.Process(It.IsNotNull<TraceTelemetry>()));

            var filter = new TelemetryResponseCodeProcessor(nextProcessor.Object);

            filter.Process(telemetry);

            nextProcessor.Verify(x => x.Process(It.IsNotNull<TraceTelemetry>()), Times.Once());
        }

        [Fact]
        public void Process_CallsNextProcessorWithTraceTelemetryPopulatedFromExceptionTelemetry()
        {
            var exception = new HttpException(httpCode: 401, message: "a");
            var exceptionJson = JObject.FromObject(exception).ToString(Formatting.None);
            var timestamp = DateTimeOffset.UtcNow;
            var exceptionTelemetry = new ExceptionTelemetry(exception);

            exceptionTelemetry.Context.Cloud.RoleInstance = "b";
            exceptionTelemetry.Context.Cloud.RoleName = "c";
            exceptionTelemetry.Context.GetInternalContext().SdkVersion = "d";
            exceptionTelemetry.Context.InstrumentationKey = "e";
            exceptionTelemetry.Context.Operation.Id = "f";
            exceptionTelemetry.Context.Operation.Name = "g";
            exceptionTelemetry.Context.Operation.ParentId = "h";
            exceptionTelemetry.Context.Location.Ip = "127.0.0.1";
            exceptionTelemetry.Properties.Add("i", "j");
            exceptionTelemetry.Sequence = "k";
            exceptionTelemetry.Timestamp = timestamp;

            var nextProcessor = new Mock<ITelemetryProcessor>(MockBehavior.Strict);

            nextProcessor.Setup(x => x.Process(It.Is<TraceTelemetry>(
                    t => t.Message == "a" &&
                        t.SeverityLevel == SeverityLevel.Warning &&
                        t.Context.Cloud.RoleInstance == "b" &&
                        t.Context.Cloud.RoleName == "c" &&
                        t.Context.GetInternalContext().SdkVersion == "d" &&
                        t.Context.InstrumentationKey == "e" &&
                        t.Context.Operation.Id == "f" &&
                        t.Context.Operation.Name == "g" &&
                        t.Context.Operation.ParentId == "h" &&
                        t.Context.Location.Ip == "127.0.0.1" &&
                        t.Properties["i"] == "j" &&
                        t.Properties["exception"] == exceptionJson &&
                        t.Sequence == "k" &&
                        t.Timestamp == timestamp)))
                .Verifiable();

            var filter = new TelemetryResponseCodeProcessor(nextProcessor.Object);

            filter.Process(exceptionTelemetry);

            nextProcessor.Verify();
        }
    }
}