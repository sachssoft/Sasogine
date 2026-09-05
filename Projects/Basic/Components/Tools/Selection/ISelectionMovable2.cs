using Sachssoft.Sasogine.Common;

namespace Sachssoft.Sasogine.Components.Tools.Selection
{
    /// <summary>
    /// Represents a selection target that can be moved by the Selection Tool.
    /// </summary>
    public interface ISelectionMovable2 : ISelectionTarget2
    {
        /// <summary>
        /// Gets or sets the position of the selection target.
        /// </summary>
        Point2 Position { get; set; }

        /// <summary>
        /// Gets a value indicating whether movement is currently allowed.
        /// </summary>
        bool AllowMove { get; }
    }
}