namespace NuGet.Services.Status.Table
{
    public interface IAggregatedEntity<T> : IChildEntity<T>, IComponentAffectingEntity
        where T : IComponentAffectingEntity
    {
    }
}
