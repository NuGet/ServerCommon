namespace NuGet.Services.Status.Tests
{
    public class LeafComponentTests : ComponentTests
    {
        protected override IComponent CreateComponent(string name, string description)
        {
            return new LeafComponent(name, description);
        }
    }
}
