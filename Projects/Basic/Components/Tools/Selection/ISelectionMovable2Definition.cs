using Sachssoft.Engine.Common;
using Sachssoft.Engine.Graphics.Rendering;

namespace Sachssoft.Engine.Components.Tools;

/// <summary>
/// Defines the movement properties of a selection target.
/// </summary>
public interface ISelectionMovable2Definition : ISelectionTarget2Definition, ITransformMovable2Definition, ITransformPosition2
{
}