namespace Sachssoft.Engine
{
    /// <summary>
    /// Represents an object that provides an engine-specific classification.
    /// </summary>
    public interface IEngineClass
    {
        /// <summary>
        /// Gets the classification or category of the engine object.
        /// </summary>
        string? Class { get; }
    }
}