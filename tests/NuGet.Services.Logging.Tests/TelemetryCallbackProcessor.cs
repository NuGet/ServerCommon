using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using System;

namespace NuGet.Services.Logging.Tests
{
    class TelemetryCallbackProcessor : ITelemetryProcessor
    {
        private Action<ITelemetry> _callback;

        public TelemetryCallbackProcessor(Action<ITelemetry> callback)
        {
            _callback = callback;
        }

        public void Process(ITelemetry item)
        {
            _callback(item);
        }
    }
}
