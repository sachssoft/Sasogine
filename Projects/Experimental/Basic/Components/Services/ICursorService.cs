using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Common;
using Sachssoft.Sasogine.Components.Services;

namespace Sachssoft.Sasogine.Experimental.Components.Services
{
    /// <summary>
    /// Provides cursor state and coordinate information for components interacting
    /// with a rendered scene.
    /// </summary>
    /// <remarks>
    /// The service provides cursor positions in different coordinate spaces and
    /// information about the render area used to translate input coordinates
    /// into scene and world coordinates.
    /// </remarks>
    public interface ICursorService : IComponentService
    {
        /// <summary>
        /// Gets the size, in pixels, of the area used to render the scene.
        /// </summary>
        /// <remarks>
        /// The render size may differ from the size of the containing window or
        /// control, for example when the scene is rendered to a render target.
        /// </remarks>
        PixelSize2 RenderSize { get; }

        /// <summary>
        /// Gets the graphics viewport used to render the scene.
        /// </summary>
        /// <remarks>
        /// The viewport defines the portion of the render surface into which the
        /// scene is rendered and is used when converting between screen and world
        /// coordinates.
        /// </remarks>
        Viewport Viewport { get; }

        /// <summary>
        /// Gets the bounds of the container that presents the rendered scene,
        /// expressed in screen coordinates.
        /// </summary>
        /// <remarks>
        /// This can represent a window, control, editor panel, or another host
        /// containing the render surface.
        /// </remarks>
        PixelBounds2 ContainerBounds { get; }

        /// <summary>
        /// Gets the cursor position relative to the top-left corner of the
        /// render container.
        /// </summary>
        /// <remarks>
        /// Unlike <see cref="ScreenPosition"/>, this position is expressed in
        /// the local coordinate space of the container.
        /// </remarks>
        Point ContainerPosition { get; }

        /// <summary>
        /// Gets the cursor position relative to the rendered scene in screen
        /// coordinates.
        /// </summary>
        /// <remarks>
        /// This position represents the cursor after accounting for the render
        /// container and viewport, but before transformation into world space.
        /// </remarks>
        Vector2 ScreenPosition { get; }

        /// <summary>
        /// Gets the cursor position in world coordinates.
        /// </summary>
        /// <remarks>
        /// The position is obtained by transforming the corresponding
        /// <see cref="ScreenPosition"/> through the active scene or camera
        /// transformation.
        /// </remarks>
        Vector2 WorldPosition { get; }

        /// <summary>
        /// Gets the cursor movement since the previous update in screen
        /// coordinates.
        /// </summary>
        /// <remarks>
        /// Positive and negative components indicate the horizontal and vertical
        /// direction of movement respectively.
        /// </remarks>
        Vector2 ScreenDelta { get; }

        /// <summary>
        /// Gets a value indicating whether the cursor is currently inside the
        /// active rendering viewport.
        /// </summary>
        /// <remarks>
        /// Components can use this value to ignore cursor interactions that occur
        /// outside the area in which the scene is rendered.
        /// </remarks>
        bool IsInViewport { get; }
    }
}