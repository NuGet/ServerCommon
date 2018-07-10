// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace NuGet.Services.Status
{
    public class ReadOnlyComponent : IReadOnlyComponent
    {
        public string Name { get; }
        public string Description { get; }
        public ComponentStatus Status { get; }
        public IEnumerable<IReadOnlyComponent> SubComponents { get; }
        public string Path => Name;

        public ReadOnlyComponent(
            string name,
            string description,
            ComponentStatus status,
            IEnumerable<IReadOnlyComponent> subComponents)
        {
            Name = name;
            Description = description;
            Status = status;
            SubComponents = subComponents?.Select(s => new ReadOnlySubComponent(s, this))
                ?? Enumerable.Empty<IReadOnlyComponent>();
        }

        [JsonConstructor]
        public ReadOnlyComponent(
            string name, 
            string description, 
            ComponentStatus status, 
            IEnumerable<ReadOnlyComponent> subComponents)
            : this(name, description, status, subComponents.Cast<IReadOnlyComponent>())
        {
        }
    }
}
