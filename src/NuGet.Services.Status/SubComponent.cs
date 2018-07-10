using System.Collections.Generic;
using System.Linq;

namespace NuGet.Services.Status
{
    /// <summary>
    /// Wrapper class for <see cref="IComponent"/> that sets <see cref="IReadOnlyComponent.Path"/> as expected.
    /// </summary>
    public class SubComponent : ReadOnlySubComponent, IComponent
    {
        private readonly IComponent _component;
        private readonly IComponent _parent;

        public new ComponentStatus Status { get { return _component.Status; } set { _component.Status = value; } }
        public new IEnumerable<IComponent> SubComponents => _component.SubComponents?.Select(s => new SubComponent(s, this));

        public SubComponent(IComponent component, IComponent parent)
            : base(component, parent)
        {
            _component = component;
            _parent = parent;
        }
    }
}
