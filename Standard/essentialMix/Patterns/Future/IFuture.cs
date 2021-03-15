namespace essentialMix.Patterns.Future
{
    /// <summary>
    /// Class representing a value which will be available some time in the future.
    /// </summary>
    public interface IFuture<out T>
    {
        /// <summary>
        /// Retrieves the value, if available, and throws InvalidOperationException
        /// otherwise.
        /// </summary>
        T Value { get; }
        bool HasValue { get; }
    }
}
