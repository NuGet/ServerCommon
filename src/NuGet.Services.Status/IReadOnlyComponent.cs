using System.Collections.Generic;

namespace NuGet.Services.Status
{
    public interface IReadOnlyComponent
    {
        string Name { get; }
        string Description { get; }
        ComponentStatus Status { get; }
        IEnumerable<IReadOnlyComponent> SubComponents { get; }
        string Path { get; }
    }
}
