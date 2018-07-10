namespace NuGet.Services.Status
{
    public class LeafComponent : Component
    {
        public override ComponentStatus Status { get; set; }

        public LeafComponent(
            string name,
            string description)
            : base(name, description)
        {
        }
    }
}
