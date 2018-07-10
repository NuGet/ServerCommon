namespace NuGet.Services.Status
{
    /// <summary>
    /// A <see cref="Component"/> that has no children.
    /// </summary>
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
