using Sachssoft.Engine.Common;
using Sachssoft.Engine.Graphics.Rendering;

namespace Sachssoft.Engine.Components.Tools;

/// <summary>
/// Defines the resizing properties of a selection target.
/// </summary>
public interface ISelectionResizable2Definition : ISelectionTarget2Definition, ITransformResizable2Definition, ITransformSize2
{
}