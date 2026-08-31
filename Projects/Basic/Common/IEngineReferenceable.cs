namespace Sachssoft.Sasogine.Common
{
    /// <summary>
    /// Represents an engine object that can be referenced by an identifier.
    /// </summary>
    public interface IEngineReferenceable
    {
        /// <summary>
        /// Gets the unique identifier of the engine object.
        /// </summary>
        string? Id { get; }
    }
}