using Sachssoft.Engine.Graphics.Rendering;

namespace Sachssoft.Engine.Components.Tools;

/// <summary>
/// Defines the rotation properties of a selection target.
/// </summary>
public interface ISelectionRotatable2Definition :
    ISelectionTarget2Definition, ITransformRotatable2Definition, ITransformRotationPivot2Definition
{
}