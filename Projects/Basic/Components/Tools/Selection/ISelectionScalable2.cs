using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Common;

namespace Sachssoft.Sasogine.Components.Tools.Selection
{
    /// <summary>
    /// Represents a selection target that can be scaled by the Selection Tool.
    /// </summary>
    public interface ISelectionScalable2 : ISelectionTarget2, ITransformScale2
    {
        /// <summary>
        /// Gets a value indicating whether scaling is allowed.
        /// </summary>
        bool AllowScale { get; }
    }
}