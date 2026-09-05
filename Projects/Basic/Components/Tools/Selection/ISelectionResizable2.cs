using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Common;

namespace Sachssoft.Sasogine.Components.Tools.Selection
{
    /// <summary>
    /// Represents a selection target that can be resized by the Selection Tool.
    /// </summary>
    public interface ISelectionResizable2 : ISelectionTarget2
    {
        /// <summary>
        /// Gets a value indicating whether resizing is allowed.
        /// </summary>
        bool AllowResize { get; }
    }
}