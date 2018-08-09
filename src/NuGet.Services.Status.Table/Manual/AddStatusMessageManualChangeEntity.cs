using System;

namespace NuGet.Services.Status.Table.Manual
{
    public class AddStatusMessageManualChangeEntity : ManualStatusChangeEntity
    {
        public AddStatusMessageManualChangeEntity()
        {
        }

        public AddStatusMessageManualChangeEntity(
            string eventAffectedComponentPath,
            DateTime eventStartTime,
            string messageContents,
            bool eventIsActive)
            : base(ManualStatusChangeType.AddStatusMessage)
        {
            EventAffectedComponentPath = eventAffectedComponentPath;
            EventStartTime = eventStartTime;
            MessageContents = messageContents;
            EventIsActive = eventIsActive;
        }

        public string EventAffectedComponentPath { get; set; }

        public DateTime EventStartTime { get; set; }

        public string MessageContents { get; set; }

        public bool EventIsActive { get; set; }
    }
}
