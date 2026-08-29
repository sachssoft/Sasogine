using System;

namespace Sachssoft.Sasogine.Components.Tools.Selection
{
    /// <summary>
    /// Defines the transformation mode used by the selection tool.
    /// </summary>
    public enum SelectionToolMode
    {
        /// <summary>
        /// Disables transformation of the current selection.
        /// </summary>
        None,

        /// <summary>
        /// Enables resizing of the current selection.
        /// </summary>
        Resize,

        /// <summary>
        /// Enables rotation of the current selection.
        /// </summary>
        Rotate
    }
}