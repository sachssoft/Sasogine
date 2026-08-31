namespace Sachssoft.Sasogine.Common
{
    /// <summary>
    /// Specifies how a value is interpreted.
    /// </summary>
    public enum ValueMode
    {
        /// <summary>
        /// The value represents a direct or fixed value.
        /// </summary>
        Absolute,

        /// <summary>
        /// The value is interpreted relative to another value or reference.
        /// </summary>
        Relative
    }
}