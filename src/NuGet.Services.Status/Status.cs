// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace NuGet.Services.Status
{
    /// <summary>
    /// Describes the status of an entire service.
    /// </summary>
    public class Status
    {
        /// <summary>
        /// The time this status was generated.
        /// </summary>
        public DateTime LastUpdated { get; }

        /// <summary>
        /// The <see cref="IReadOnlyComponent"/> that describes the entire service. Its <see cref="IReadOnlyComponent.SubComponents"/> represent portions of the service.
        /// </summary>
        public IReadOnlyComponent RootComponent { get; }

        /// <summary>
        /// A list of <see cref="IEvent"/>s that have affected the service recently.
        /// </summary>
        public IEnumerable<IEvent> Events { get; }

        public Status(IReadOnlyComponent rootComponent, IEnumerable<IEvent> events)
            : this(DateTime.Now, rootComponent, events)
        {
        }

        public Status(DateTime lastUpdated, IReadOnlyComponent rootComponent, IEnumerable<IEvent> events)
        {
            LastUpdated = lastUpdated;
            RootComponent = rootComponent;
            Events = events;
        }

        [JsonConstructor]
        public Status(DateTime lastUpdated, ReadOnlyComponent rootComponent, IEnumerable<Event> events)
            : this(lastUpdated, (IReadOnlyComponent)rootComponent, events)
        {
        }
    }
}
