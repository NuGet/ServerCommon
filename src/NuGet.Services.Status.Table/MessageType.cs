namespace NuGet.Services.Status.Table
{
    public enum MessageType
    {
        /// <summary>
        /// A message that was manually posted or edited.
        /// </summary>
        Manual = 0,

        /// <summary>
        /// A message that marks the start of an <see cref="Event"/>.
        /// </summary>
        Start = 1,

        /// <summary>
        /// A message that marks the end of an <see cref="Event"/>.
        /// </summary>
        End = 2
    }
}
