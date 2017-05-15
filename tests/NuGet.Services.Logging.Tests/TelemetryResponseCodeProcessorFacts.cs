// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Channel;

namespace NuGet.Services.Logging.Tests
{
    public class TelemetryResponseCodeProcessorFacts
    {
        [Theory]
        [InlineData("400")]
        [InlineData("404")]
        public void Marks400And404ResponseAsSuccessful(string responseCode)
        {
            // Arrange
            var failed = new RequestTelemetry
            {
                ResponseCode = responseCode,
                Success = false,
            };

            var successful = new RequestTelemetry
            {
                ResponseCode = responseCode,
                Success = true,
            };

            // Assert
            Assert.True(ProcessResonseCode(failed).Success);
            Assert.True(ProcessResonseCode(successful).Success);
        }

        [Theory]
        [InlineData("200")]
        [InlineData("301")]
        [InlineData("401")]
        [InlineData("403")]
        [InlineData("410")]
        [InlineData("500")]
        public void DoesntAffectOtherResponses(string responseCode)
        {
            // Arrange
            var failed = new RequestTelemetry
            {
                ResponseCode = responseCode,
                Success = false,
            };

            var successful = new RequestTelemetry
            {
                ResponseCode = responseCode,
                Success = true,
            };

            // Assert
            Assert.False(ProcessResonseCode(failed).Success);
            Assert.True(ProcessResonseCode(successful).Success);
        }

        private RequestTelemetry ProcessResonseCode(RequestTelemetry telemetry)
        {
            ITelemetry result = null;
            var processor = new TelemetryResponseCodeProcessor(new TelemetryCallbackProcessor(i => result = i));

            processor.Process(telemetry);

            return result as RequestTelemetry;
        }
    }
}
