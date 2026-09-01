using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Components.Services;
using Sachssoft.Sasogine.Experimental.Graphics;
using Sachssoft.Sasogine.Experimental.Input;

namespace Sachssoft.Sasogine.Experimental.Components.Services
{
    /// <summary>
    /// Provides cursor state and coordinate information for a rendered scene.
    /// </summary>
    public interface ICursorService : IComponentService, ICursorState
    {
        /// <summary>
        /// Gets the render container associated with the cursor service.
        /// </summary>
        IRenderContainer Container { get; }

        /// <summary>
        /// Gets the graphics viewport used to render the scene.
        /// </summary>
        Viewport Viewport { get; }
    }
}