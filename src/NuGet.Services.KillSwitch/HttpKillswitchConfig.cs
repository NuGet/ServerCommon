// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace NuGet.Services.KillSwitch
{
    /// <summary>
    /// The configuration for <see cref="HttpKillswitchClient"/>.
    /// </summary>
    public class HttpKillswitchConfig
    {
        /// <summary>
        /// The REST endpoint that contains the list of activated killswitch.
        /// </summary>
        public Uri KillswitchEndpoint { get; set; }

        /// <summary>
        /// How frequently the endpoint should be queried.
        /// </summary>
        public TimeSpan RefreshInterval { get; set; } = TimeSpan.FromMinutes(5);

        /// <summary>
        /// The maximum allowed staleness by <see cref="HttpKillswitchClient.IsActive(string)"/>.
        /// The method will throw if this threshold is reached.
        /// </summary>
        public TimeSpan MaximumStaleness { get; set; } = TimeSpan.FromMinutes(20);
    }
}
