using System;

namespace NuGet.Services.Status
{
    /// <summary>
    /// A message associated with an <see cref="IEvent"/>.
    /// </summary>
    public interface IMessage
    {
        /// <summary>
        /// The time the message was posted.
        /// </summary>
        DateTime Time { get; }
        /// <summary>
        /// The contents of the message.
        /// </summary>
        string Contents { get; }
    }
}
