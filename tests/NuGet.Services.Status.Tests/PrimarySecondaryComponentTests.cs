using System.Collections.Generic;

namespace NuGet.Services.Status.Tests
{
    public class PrimarySecondaryComponentTests : ComponentWithSubComponentsTests
    {
        protected override IComponent CreateComponent(string name, string description)
        {
            return new PrimarySecondaryComponent(name, description);
        }

        protected override IComponent CreateComponent(string name, string description, IEnumerable<IComponent> subComponents)
        {
            return new PrimarySecondaryComponent(name, description, subComponents);
        }

        protected override ComponentStatus GetExpectedStatusWithTwoSubComponents(ComponentStatus subStatus1, ComponentStatus subStatus2)
        {
            if (subStatus1 == ComponentStatus.Up)
            {
                return ComponentStatus.Up;
            }

            if (subStatus1 == ComponentStatus.Degraded)
            {
                return ComponentStatus.Degraded;
            }

            if (subStatus2 == ComponentStatus.Up)
            {
                return ComponentStatus.Degraded;
            }

            return subStatus2;
        }
    }
}
