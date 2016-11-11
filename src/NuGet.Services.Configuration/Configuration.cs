using System;

namespace NuGet.Services.Configuration
{
    public class Configuration
    {
        public Configuration()
        {
            CreatedTime = DateTime.UtcNow;
        }

        public DateTime CreatedTime { get; }
    }
}
