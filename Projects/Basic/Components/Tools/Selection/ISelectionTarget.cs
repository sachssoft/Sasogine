namespace Sachssoft.Sasogine.Components.Tools.Selection
{
    /// <summary>
    /// Represents an object that can be selected by a selection tool.
    /// </summary>
    public interface ISelectionTarget
    {
        /// <summary>
        /// Gets or sets a value indicating whether the selection target is selected.
        /// </summary>
        bool IsSelected { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the selection target is locked.
        /// </summary>
        bool IsLocked { get; set; }
    }
}