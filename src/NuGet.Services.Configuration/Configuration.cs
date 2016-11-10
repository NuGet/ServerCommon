using System;

namespace NuGet.Services.Configuration
{
    /// <summary>
    /// Base class to extend when using an <see cref="IConfigurationFactory"/>.
    /// </summary>
    public class Configuration
    {
        public Configuration()
        {
            CreatedTime = DateTime.UtcNow;
        }

        public DateTime CreatedTime { get; }
    }
}
