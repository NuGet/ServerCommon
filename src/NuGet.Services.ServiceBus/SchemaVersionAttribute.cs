using System;

namespace NuGet.Services.ServiceBus
{
    /// <summary>
    /// The version of the sche
    /// </summary>
    public class SchemaVersionAttribute : Attribute
    {
        public int Version { get; }

        public SchemaVersionAttribute(int version)
        {
            Version = version;
        }
    }
}
