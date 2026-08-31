namespace Sachssoft.Sasogine.Common
{
    /// <summary>
    /// Represents a template capable of creating object instances.
    /// </summary>
    public interface ITemplate
    {
        /// <summary>
        /// Creates a new object instance from this template.
        /// </summary>
        /// <returns>The newly created object instance.</returns>
        object Create();
    }
}