namespace NuGet.Services.Status
{
    /// <summary>
    /// Describes whether or not a <see cref="IReadOnlyComponent"/> is performing as expected.
    /// </summary>
    public enum ComponentStatus
    {
        /// <summary>
        /// The <see cref="IReadOnlyComponent"/> is performing as expected.
        /// </summary>
        Up,

        /// <summary>
        /// Some portion of the <see cref="IReadOnlyComponent"/> is not performing as expected.
        /// </summary>
        Degraded,

        /// <summary>
        /// The <see cref="IReadOnlyComponent"/> is completely unfunctional.
        /// </summary>
        Down
    }
}
