using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace NuGet.Services.Status
{
    public class Status
    {
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

        public DateTime LastUpdated { get; }
        public IReadOnlyComponent RootComponent { get; }
        public IEnumerable<IEvent> Events { get; }
    }
}
