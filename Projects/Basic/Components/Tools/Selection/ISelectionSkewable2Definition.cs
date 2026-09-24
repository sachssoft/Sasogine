using Sachssoft.Engine;
using Sachssoft.Engine.Graphics.Rendering;

namespace Sachssoft.Engine.Components.Tools;

/// <summary>
/// Defines a 2D selection target definition that supports skew transformation.
/// </summary>
public interface ISelectionSkewable2Definition : ISelectionTarget2Definition, ITransformSkewable2Definition, ITransformSkew2
{
}