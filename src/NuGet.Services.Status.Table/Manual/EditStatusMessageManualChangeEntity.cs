using System;

namespace NuGet.Services.Status.Table.Manual
{
    public class EditStatusMessageManualChangeEntity : ManualStatusChangeEntity
    {
        public EditStatusMessageManualChangeEntity(
            string eventAffectedComponentPath,
            DateTime eventStartTime,
            DateTime messageTimestamp,
            string messageContents)
            : base(ManualStatusChangeType.EditStatusMessage)
        {
            EventAffectedComponentPath = eventAffectedComponentPath;
            EventStartTime = eventStartTime;
            MessageTimestamp = messageTimestamp;
            MessageContents = messageContents;
        }

        public string EventAffectedComponentPath { get; set; }

        public DateTime EventStartTime { get; set; }

        public DateTime MessageTimestamp { get; set; }

        public string MessageContents { get; set; }
    }
}
