namespace NuGet.Services.Status.Table
{
    /// <summary>
    /// A <see cref="IComponentAffectingEntity"/> that is aggregated by <typeparamref name="T"/>.
    /// </summary>
    public interface IAggregatedEntity<T> : IChildEntity<T>, IComponentAffectingEntity
        where T : IComponentAffectingEntity
    {
    }
}
