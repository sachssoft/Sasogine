using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Components.Services;
using Sachssoft.Sasogine.Graphics;
using Sachssoft.Sasogine.Input;

namespace Sachssoft.Sasogine.Components.Services
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