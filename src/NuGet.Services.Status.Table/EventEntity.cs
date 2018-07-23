using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;

namespace NuGet.Services.Status.Table
{
    /// <summary>
    /// Class used to serialize an <see cref="Event"/> in a table.
    /// </summary>
    public class EventEntity : TableEntity
    {
        public const string DefaultPartitionKey = "events";

        public EventEntity()
        {
        }

        public EventEntity(
            string affectedComponentPath,
            int affectedComponentStatus,
            DateTime startTime,
            DateTime? endTime = null)
            : base(DefaultPartitionKey, GetRowKey(affectedComponentPath, startTime))
        {
            AffectedComponentPath = affectedComponentPath;
            AffectedComponentStatus = affectedComponentStatus;
            StartTime = startTime;
            EndTime = endTime;
        }

        public EventEntity(
            string affectedComponentPath,
            ComponentStatus affectedComponentStatus,
            DateTime startTime,
            DateTime? endTime = null)
            : this(affectedComponentPath, (int)affectedComponentStatus, startTime, endTime)
        {
        }

        public EventEntity(Event target)
            : this(target.AffectedComponentPath, target.AffectedComponentStatus, target.StartTime, target.EndTime)
        {
        }

        public EventEntity(IncidentEntity incidentEntity)
            : this(incidentEntity.AffectedComponentPath, incidentEntity.AffectedComponentStatus, incidentEntity.CreationTime)
        {
            incidentEntity.EventRowKey = RowKey;
        }

        public string AffectedComponentPath { get; set; }
        public int AffectedComponentStatus { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public bool IsActive
        {
            get { return EndTime == null; }
            set { }
        }

        public Event AsEvent(IEnumerable<Message> messages)
        {
            return new Event(AffectedComponentPath, (ComponentStatus)AffectedComponentStatus, StartTime, EndTime, messages);
        }

        private static string GetRowKey(string affectedComponentPath, DateTime startTime)
        {
            return $"{Utility.ToRowKeySafeComponentPath(affectedComponentPath)}_{startTime.ToString("o")}";
        }
    }
}
