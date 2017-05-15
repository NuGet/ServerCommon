// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using System;

namespace NuGet.Services.Logging
{
    /// <summary>
    /// Application Insights marks 400 and 404 responses as failed requests by default.
    /// This processor overrides this behaviour, and instead, marks them as successful.
    /// </summary>
    public class TelemetryResponseCodeProcessor : ITelemetryProcessor
    {
        private readonly ITelemetryProcessor _next;

        public TelemetryResponseCodeProcessor(ITelemetryProcessor next)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }

        public void Process(ITelemetry item)
        {
            var request = item as RequestTelemetry;
            int responseCode;

            if (request != null && int.TryParse(request.ResponseCode, out responseCode))
            {
                if (responseCode == 400 || responseCode == 404)
                {
                    request.Success = true;
                }
            }

            _next.Process(item);
        }
    }
}