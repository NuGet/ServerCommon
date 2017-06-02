// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Web;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.Implementation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NuGet.Services.Logging
{
    /// <summary>
    /// Application Insights inspects a request's response code to decide if the operation
    /// was successful or not. This processor can be used to override the default behavior.
    /// </summary>
    public class TelemetryResponseCodeProcessor : ITelemetryProcessor
    {
        private readonly ITelemetryProcessor _next;

        /// <summary>
        /// The response codes that should always be marked as successful.
        /// </summary>
        public IList<int> SuccessfulResponseCodes { get; } = new List<int>();

        public TelemetryResponseCodeProcessor(ITelemetryProcessor next)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }

        public void Process(ITelemetry item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            ITelemetry nextItem = null;

            if (TryProcess(item as RequestTelemetry))
            {
            }
            else if (TryProcess(item as ExceptionTelemetry, out nextItem))
            {
                item = nextItem;
            }

            _next.Process(item);
        }

        private bool TryProcess(RequestTelemetry telemetry)
        {
            int responseCode;

            if (telemetry != null && int.TryParse(telemetry.ResponseCode, out responseCode))
            {
                if (SuccessfulResponseCodes.Contains(responseCode))
                {
                    telemetry.Success = true;

                    return true;
                }
            }

            return false;
        }

        private static bool TryProcess(ExceptionTelemetry telemetry, out ITelemetry nextTelemetry)
        {
            nextTelemetry = telemetry;

            if (telemetry != null)
            {
                var httpException = telemetry.Exception as HttpException;

                if (httpException != null)
                {
                    var httpCode = httpException.GetHttpCode();

                    if (httpCode < 500)
                    {
                        // Logging exception telemetry for a request that resulted in an HTTP response code under 500
                        // adds noise to exception telemetry analysis.  Log it as trace telemetry instead.
                        nextTelemetry = Convert(telemetry, httpException);
                    }

                    return true;
                }
            }

            return false;
        }

        private static TraceTelemetry Convert(ExceptionTelemetry exceptionTelemetry, HttpException exception)
        {
            var traceTelemetry = new TraceTelemetry(exception.Message, SeverityLevel.Warning);

            traceTelemetry.Context.Cloud.RoleInstance = exceptionTelemetry.Context.Cloud.RoleInstance;
            traceTelemetry.Context.Cloud.RoleName = exceptionTelemetry.Context.Cloud.RoleName;
            traceTelemetry.Context.GetInternalContext().SdkVersion = exceptionTelemetry.Context.GetInternalContext().SdkVersion;
            traceTelemetry.Context.InstrumentationKey = exceptionTelemetry.Context.InstrumentationKey;
            traceTelemetry.Context.Operation.Id = exceptionTelemetry.Context.Operation.Id;
            traceTelemetry.Context.Operation.Name = exceptionTelemetry.Context.Operation.Name;
            traceTelemetry.Context.Operation.ParentId = exceptionTelemetry.Context.Operation.ParentId;
            traceTelemetry.Context.Location.Ip = exceptionTelemetry.Context.Location.Ip;

            foreach (var property in exceptionTelemetry.Properties)
            {
                traceTelemetry.Properties.Add(property.Key, property.Value);
            }

            traceTelemetry.Properties.Add("exception", JObject.FromObject(exception).ToString(Formatting.None));

            traceTelemetry.Sequence = exceptionTelemetry.Sequence;
            traceTelemetry.Timestamp = exceptionTelemetry.Timestamp;

            return traceTelemetry;
        }
    }
}