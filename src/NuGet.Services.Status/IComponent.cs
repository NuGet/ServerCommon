using System.Collections.Generic;

namespace NuGet.Services.Status
{
    public interface IComponent : IReadOnlyComponent
    {
        new ComponentStatus Status { get; set; }
        new IEnumerable<IComponent> SubComponents { get; }
    }
}
