using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Common;

namespace Sachssoft.Sasogine.Components.Tools.Selection
{
    /// <summary>
    /// Represents a 2D selection target that supports skew transformation.
    /// </summary>
    public interface ISelectionSkewable2 : ISelectionTarget2, ITransformSkew2
    {
        /// <summary>
        /// Gets a value indicating whether the target can be skewed.
        /// </summary>
        bool AllowSkew { get; }
    }
}