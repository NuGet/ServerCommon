using System;

namespace NuGet.Services.Status.Table
{
    public interface IComponentAffectingEntity
    {
        string AffectedComponentPath { get; }

        /// <remarks>
        /// This should be a <see cref="ComponentStatus"/> converted to an enum.
        /// See https://github.com/Azure/azure-storage-net/issues/383
        /// </remarks>
        int AffectedComponentStatus { get; }

        DateTime StartTime { get; }

        DateTime? EndTime { get; }
        
        bool IsActive { get; }
    }
}
