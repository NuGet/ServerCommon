using System;

namespace NuGet.Services.Status.Table.Manual
{
    public class DeleteStatusMessageManualChangeEntity : ManualStatusChangeEntity
    {
        public DeleteStatusMessageManualChangeEntity(
            string eventAffectedComponentPath,
            DateTime eventStartTime,
            DateTime messageTimestamp)
            : base(ManualStatusChangeType.DeleteStatusMessage)
        {
            EventAffectedComponentPath = eventAffectedComponentPath;
            EventStartTime = eventStartTime;
            MessageTimestamp = messageTimestamp;
        }

        public string EventAffectedComponentPath { get; set; }

        public DateTime EventStartTime { get; set; }

        public DateTime MessageTimestamp { get; set; }
    }
}
