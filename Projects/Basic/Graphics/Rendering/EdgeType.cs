namespace Sachssoft.Sasogine.Graphics.Rendering
{
    // Für zukünftige Verwendung vorgesehen. Aktuell wird der Enum noch nicht
    // direkt von der Rendering-API verwendet. Zum Beispiel für abgerundete
    // Rechtecke vorgesehen.

    /// <summary>
    /// Defines the type of an edge.
    /// </summary>
    public enum EdgeType
    {
        /// <summary>
        /// Indicates that no specific edge type is defined.
        /// </summary>
        None,

        /// <summary>
        /// Indicates an edge positioned below the associated geometry.
        /// </summary>
        Below,

        /// <summary>
        /// Indicates a rounded edge.
        /// </summary>
        Rounded
    }
}