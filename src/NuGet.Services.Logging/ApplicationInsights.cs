// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.Implementation;
using Microsoft.ApplicationInsights.Extensibility.Implementation.Tracing;

namespace NuGet.Services.Logging
{
    public static class ApplicationInsights
    {
        public static IHeartbeatPropertyManager HeartbeatManager { get; private set; }

        public static bool Initialized { get; private set; }

        public static TelemetryConfiguration Initialize(string instrumentationKey)
        {
            return InitializeTelemetryConfiguration(instrumentationKey, heartbeatInterval: null);
        }

        public static TelemetryConfiguration Initialize(string instrumentationKey, TimeSpan heartbeatInterval)
        {
            return InitializeTelemetryConfiguration(instrumentationKey, heartbeatInterval);
        }

        private static TelemetryConfiguration InitializeTelemetryConfiguration(string instrumentationKey, TimeSpan? heartbeatInterval)
        {
            TelemetryConfiguration telemetryConfiguration = null;

            if (!string.IsNullOrWhiteSpace(instrumentationKey))
            {
                // Note: TelemetryConfiguration.Active is being deprecated
                // https://github.com/microsoft/ApplicationInsights-dotnet/issues/1152

                telemetryConfiguration = new TelemetryConfiguration(instrumentationKey);
                telemetryConfiguration.TelemetryInitializers.Add(new TelemetryContextInitializer());

                // Construct a TelemetryClient to emit traces so we can track and debug AI initialization.
                var telemetryClient = new TelemetryClient(telemetryConfiguration);

                telemetryClient.TrackTrace(
                            $"TelemetryConfiguration initialized using instrumentation key: {instrumentationKey}.",
                            SeverityLevel.Information);

                // Configure heartbeat interval if specified.
                // When not defined or null, the DiagnosticsTelemetryModule will use its internal defaults (heartbeat enabled, interval of 15 minutes).
                if (heartbeatInterval.HasValue)
                {
                    var diagnosticsTelemetryModule = new DiagnosticsTelemetryModule();
                    diagnosticsTelemetryModule.HeartbeatInterval = heartbeatInterval.Value;
                    diagnosticsTelemetryModule.Initialize(telemetryConfiguration);

                    telemetryClient.TrackTrace(
                            $"DiagnosticsTelemetryModule initialized using configured heartbeat interval: {heartbeatInterval.Value}.",
                            SeverityLevel.Information);

                    var heartbeatManager = GetHeartbeatPropertyManager(telemetryClient);
                    if (heartbeatManager != null)
                    {
                        heartbeatManager.HeartbeatInterval = heartbeatInterval.Value;

                        telemetryClient.TrackTrace(
                            $"IHeartbeatPropertyManager initialized using configured heartbeat interval: {heartbeatInterval.Value}.",
                            SeverityLevel.Information);
                    }
                }
                else
                {
                    telemetryClient.TrackTrace(
                        "Telemetry initialized using default heartbeat interval.",
                        SeverityLevel.Information);
                }

                Initialized = true;
            }
            else
            {
                Initialized = false;
            }

            return telemetryConfiguration;
        }

        private static IHeartbeatPropertyManager GetHeartbeatPropertyManager(TelemetryClient telemetryClient)
        {
            if (HeartbeatManager == null)
            {
                var telemetryModules = TelemetryModules.Instance;

                try
                {
                    foreach (var module in telemetryModules.Modules)
                    {
                        if (module is IHeartbeatPropertyManager heartbeatManager)
                        {
                            HeartbeatManager = heartbeatManager;
                        }
                    }
                }
                catch (Exception hearbeatManagerAccessException)
                {
                    // An non-critical, unexpected exception occurred trying to access the heartbeat manager.
                    telemetryClient.TrackTrace(
                        $"There was an error accessing heartbeat manager. Details: {hearbeatManagerAccessException.ToInvariantString()}",
                        SeverityLevel.Error);
                }

                if (HeartbeatManager == null)
                {
                    // Heartbeat manager unavailable: log warning.
                    telemetryClient.TrackTrace("Heartbeat manager unavailable", SeverityLevel.Warning);
                }
            }

            return HeartbeatManager;
        }
    }
}
