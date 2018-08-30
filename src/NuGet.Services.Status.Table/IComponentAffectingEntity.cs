using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace NuGet.Services.Status.Table
{
    public interface IComponentAffectingEntity : ITableEntity
    {
        string AffectedComponentPath { get; set; }

        /// <remarks>
        /// This should be a <see cref="ComponentStatus"/> converted to an enum.
        /// See https://github.com/Azure/azure-storage-net/issues/383
        /// </remarks>
        int AffectedComponentStatus { get; set; }

        DateTime StartTime { get; set; }

        DateTime? EndTime { get; set; }
        
        bool IsActive { get; set; }
    }
}
