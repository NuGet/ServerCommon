using System.Collections.Generic;

namespace NuGet.Services.Status
{
    /// <summary>
    /// A writable <see cref="IReadOnlyComponent"/> that allows setting its status.
    /// </summary>
    public interface IComponent : IReadOnlyComponent
    {
        new ComponentStatus Status { get; set; }
        new IEnumerable<IComponent> SubComponents { get; }
    }
}
