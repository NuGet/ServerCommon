using System;
using System.Collections.Generic;

namespace NuGet.Services.Status
{
    public interface IEvent
    {
        string AffectedComponentPath { get; }
        ComponentStatus AffectedComponentStatus { get; }
        DateTime StartTime { get; }
        DateTime? EndTime { get; }
        IEnumerable<IMessage> Messages { get; }
    }
}
