namespace Sachssoft.Sasogine.Common
{
    /// <summary>
    /// Represents an object that exposes a <see cref="DirtyMark"/>
    /// for tracking whether its state has changed.
    /// </summary>
    public interface IHasDirtyMark
    {
        /// <summary>
        /// Gets the dirty-state tracker associated with this object.
        /// </summary>
        DirtyMark Dirty { get; }
    }
}