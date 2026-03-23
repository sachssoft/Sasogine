namespace Sachssoft.Sasogine.Platform
{
    /// <summary>
    /// Platform-independent clipboard service for arbitrary data.
    /// All data is identified by a format string.
    /// </summary>
    public interface IClipboardService
    {
        /// <summary>
        /// Sets arbitrary data to the clipboard with a specified format identifier.
        /// </summary>
        /// <param name="format">Platform-independent format string (e.g., "text/plain", "image/png").</param>
        /// <param name="data">The object to store. Can be any type.</param>
        void SetData(string format, object? data);

        /// <summary>
        /// Gets data from the clipboard with the specified format.
        /// Returns null if not available.
        /// </summary>
        /// <param name="format">Format string.</param>
        /// <returns>The stored object, or null if none.</returns>
        object? GetData(string format);

        /// <summary>
        /// Checks if the clipboard contains data for the specified format.
        /// </summary>
        bool ContainsData(string format);

        /// <summary>
        /// Clears all clipboard data.
        /// </summary>
        void Clear();
    }
}
