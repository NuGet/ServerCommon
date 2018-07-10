using System;

namespace NuGet.Services.Status
{
    public interface IMessage
    {
        DateTime Time { get; }
        string Contents { get; }
    }
}
