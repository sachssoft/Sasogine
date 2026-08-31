namespace Sachssoft.Sasogine.Common
{
    /// <summary>
    /// Defines common configuration data for an engine object,
    /// including its identifier and classification.
    /// </summary>
    public interface IEngineObjectDefinition : IDefinition, IEngineReferenceable
    {
        /// <summary>
        /// Gets or sets the unique identifier of the engine object.
        /// </summary>
        new string? Id { get; set; }

        /// <summary>
        /// Gets or sets the classification or category of the engine object.
        /// </summary>
        string? Class { get; set; }
    }
}