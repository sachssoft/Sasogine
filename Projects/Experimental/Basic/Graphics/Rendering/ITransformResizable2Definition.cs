using Sachssoft.Sasogine.Common;

namespace Sachssoft.Sasogine.Experimental.Graphics.Rendering;

/// <summary>
/// Defines a 2D transform definition that can be resized.
/// </summary>
public interface ITransformResizable2Definition
{
    /// <summary>
    /// Gets or sets the 2D size.
    /// </summary>
    Size2 Size { get; set; }
}