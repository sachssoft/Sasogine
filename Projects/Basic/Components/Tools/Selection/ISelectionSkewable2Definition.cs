using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Graphics.Rendering;

namespace Sachssoft.Sasogine.Components.Tools.Selection
{
    /// <summary>
    /// Defines a 2D selection target definition that supports skew transformation.
    /// </summary>
    public interface ISelectionSkewable2Definition : ISelectionTarget2Definition, ITransformSkewable2Definition
    {
    }
}