namespace Sachssoft.Engine
{
    /// <summary>
    /// Represents a template capable of creating object instances using an
    /// optional creation context.
    /// </summary>
    public interface ITemplate
    {
        /// <summary>
        /// Creates a new object instance using the specified context.
        /// </summary>
        /// <param name="context">
        /// The optional context used during object creation, or
        /// <see langword="null"/> if no context is required.
        /// </param>
        /// <returns>The newly created object instance.</returns>
        object Create(object? context);
    }
}