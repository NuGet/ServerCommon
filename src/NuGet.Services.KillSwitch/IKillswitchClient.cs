// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;

namespace NuGet.Services.KillSwitch
{
    /// <summary>
    /// Use killswitches to dynamically disable features in your service.
    /// </summary>
    public interface IKillswitchClient
    {
        /// <summary>
        /// Start tracking the killswitches. This method can be called only once per instance.
        /// </summary>
        /// <param name="cancellationToken">Token to stop tracking the killswitches.</param>
        /// <returns>A task that completes after the killswitches have been loaded.</returns>
        Task StartAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Force a refresh of the list of activated killswitches. This can be called as frequently
        /// as desired, regardless of whether <see cref="StartAsync(CancellationToken)"/> has been called.
        /// </summary>
        /// <param name="cancellationToken">Cancel the refresh operation.</param>
        /// <returns>A task that completes when the known killswitches have been refreshed.</returns>
        Task RefreshAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Check whether the killswitch is known to be active. The list of known activated killswitches MUST be
        /// loaded (through either <see cref="StartAsync(CancellationToken)"/> or <see cref="RefreshAsync"/>) prior
        /// to calling this method. If you'd like to guarantee the result isn't stale, call <see cref="RefreshAsync"/>
        /// prior to calling this method.
        /// </summary>
        /// <param name="name">The name of the killswitch.</param>
        /// <returns>True if the killswitch is known to be active.</returns>
        bool IsActive(string name);
    }
}
