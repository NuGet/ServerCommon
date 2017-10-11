using System;

namespace NuGet.Services.ServiceBus
{
    /// <summary>
    /// The attribute used to define the version of a schema. This version should be
    /// bumped whenever a schema's property is added, removed, or modified.
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
