using System;
using System.Collections.Generic;
using System.Linq;

namespace NuGet.Services.Status
{
    public abstract class Component : IComponent
    {
        public static string ComponentPathDivider = "/";

        public string Name { get; }
        public string Description { get; }
        public abstract ComponentStatus Status { get; set; }
        public IEnumerable<IComponent> SubComponents { get; }
        IEnumerable<IReadOnlyComponent> IReadOnlyComponent.SubComponents => SubComponents;
        public string Path => Name;

        public Component(
            string name,
            string description)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Description = description ?? "";
            SubComponents = Enumerable.Empty<IComponent>();
        }

        public Component(
            string name,
            string description,
            IEnumerable<IComponent> subComponents)
            : this(name, description)
        {
            SubComponents = subComponents?.Select(s => new SubComponent(s, this))
                ?? throw new ArgumentNullException(nameof(subComponents));
        }
    }
}
