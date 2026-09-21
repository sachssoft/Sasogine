using Sachssoft.Sasogine.Common;
using Sachssoft.Sasogine.Graphics.Rendering;

namespace Sachssoft.Sasogine.Components.Tools;

/// <summary>
/// Defines the movement properties of a selection target.
/// </summary>
public interface ISelectionMovable2Definition : ISelectionTarget2Definition, ITransformMovable2Definition, ITransformPosition2
{
}