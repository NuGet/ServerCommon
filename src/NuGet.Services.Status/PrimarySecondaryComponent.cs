using System.Collections.Generic;
using System.Linq;

namespace NuGet.Services.Status
{
    public class PrimarySecondaryComponent : Component
    {
        public PrimarySecondaryComponent(
            string name,
            string description)
            : base(name, description)
        {
        }

        public PrimarySecondaryComponent(
            string name,
            string description,
            IEnumerable<IComponent> subComponents)
            : base(name, description, subComponents)
        {
        }

        private ComponentStatus? _status = null;
        public override ComponentStatus Status
        {
            get
            {
                if (_status.HasValue)
                {
                    return _status.Value;
                }
                
                // Iterate through the list of subcomponents in order.
                var isFirst = true;
                foreach (var subComponent in SubComponents)
                {
                    if (subComponent.Status == ComponentStatus.Up)
                    {
                        // If the first component is up, the status is up.
                        // If any child component is up, the status is degraded.
                        return isFirst ? ComponentStatus.Up : ComponentStatus.Degraded;
                    }

                    // If any component is degraded, the status is degraded.
                    if (subComponent.Status == ComponentStatus.Degraded)
                    {
                        return ComponentStatus.Degraded;
                    }

                    isFirst = false;
                }

                // If all components are down, the status is down.
                return isFirst ? ComponentStatus.Up : ComponentStatus.Down;
            }
            set
            {
                _status = value;
            }
        }
    }
}
