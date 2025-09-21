namespace Sachssoft.Sasogine.Enums
{
    /// <summary>
    /// Determines the layer order of elements in a UI or scene.
    /// Specifies whether elements are considered behind or in front of others.
    /// </summary>
    public enum LayerOrder
    {
        /// <summary>
        /// Element is in the back. Drawn first; front elements will overlay it.
        /// </summary>
        Back,

        /// <summary>
        /// Element is in the front. Drawn last; appears on top of back elements.
        /// </summary>
        Front
    }
}
