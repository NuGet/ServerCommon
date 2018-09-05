// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using NuGet.Services.Logging;

namespace NuGet.Services.FeatureFlags
{
    public class FeatureFlagService : IFeatureFlagService
    {
        private readonly CloudBlobContainer _container;
        private readonly ITelemetryClient _telemetry;
        private readonly ILogger<FeatureFlagService> _logger;

        public FeatureFlagService(CloudBlobContainer container, ITelemetryClient telemetry, ILogger<FeatureFlagService> logger)
        {
            _container = container;
            _telemetry = telemetry;
            _logger = logger;
        }

        public IFeatureFlagResult GetLatestResult()
        {
            var blob = _container.GetBlockBlobReference("flags.json");
            
            using (var stream = new MemoryStream())
            {
                blob.DownloadToStream(stream);
                stream.Position = 0;

                using (var streamReader = new StreamReader(stream))
                using (var jsonReader = new JsonTextReader(streamReader))
                {
                    _telemetry.TrackMetric("FeatureFlags.Test", 123);

                    var flags = new JsonSerializer().Deserialize<FeatureFlags>(jsonReader);

                    return new FeatureFlagResult
                    {
                        Status = FeatureFlagsStatus.Ok,
                        Flags = flags
                    };
                }
            }
        }
    }

    public class FeatureFlagResult : IFeatureFlagResult
    {
        public FeatureFlagsStatus Status { get; set; }

        public IFeatureFlags Flags {  get; set; }
    }

    public class FeatureFlags : IFeatureFlags
    {
        public IReadOnlyList<string> Killswitches { get; set; }
    }
}
