using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace NuGet.Services.Status.Table
{
    /// <summary>
    /// Class used to serialize an incident in a table.
    /// </summary>
    public class IncidentEntity : TableEntity
    {
        public const string DefaultPartitionKey = "incidents";

        public IncidentEntity()
        {
        }

        public IncidentEntity(string id, string affectedComponentPath, ComponentStatus affectedComponentStatus, DateTime creationTime, DateTime? mitigationTime)
            : base(DefaultPartitionKey, GetRowKey(id, affectedComponentPath, affectedComponentStatus))
        {
            IncidentApiId = id;
            AffectedComponentPath = affectedComponentPath;
            AffectedComponentStatus = (int)affectedComponentStatus;
            CreationTime = creationTime;
            MitigationTime = mitigationTime;
        }

        public string EventRowKey { get; set; }
        public bool IsLinkedToEvent
        {
            get { return !string.IsNullOrEmpty(EventRowKey); }
            set { }
        }
        public string IncidentApiId { get; set; }
        public string AffectedComponentPath { get; set; }
        public int AffectedComponentStatus { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime? MitigationTime { get; set; }
        public bool IsActive
        {
            get { return MitigationTime == null; }
            set { }
        }

        private static string GetRowKey(string id, string affectedComponentPath, ComponentStatus affectedComponentStatus)
        {
            return $"{id}_{Utility.ToRowKeySafeComponentPath(affectedComponentPath)}_{affectedComponentStatus}";
        }
    }
}
