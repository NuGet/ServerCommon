using System.Collections.Generic;
using System.Linq;

namespace NuGet.Services.Status
{
    /// <summary>
    /// Wrapper class for <see cref="IReadOnlyComponent"/> that sets <see cref="IReadOnlyComponent.Path"/> as expected.
    /// </summary>
    public class ReadOnlySubComponent : IReadOnlyComponent
    {
        private readonly IReadOnlyComponent _component;
        private readonly IReadOnlyComponent _parent;

        public string Name => _component.Name;
        public string Description => _component.Description;
        public ComponentStatus Status => _component.Status;
        public IEnumerable<IReadOnlyComponent> SubComponents => _component.SubComponents?.Select(s => new ReadOnlySubComponent(s, this));
        public string Path => _parent.Path + Component.ComponentPathDivider + Name;

        public ReadOnlySubComponent(IReadOnlyComponent component, IReadOnlyComponent parent)
        {
            _component = component;
            _parent = parent;
        }
    }
}
