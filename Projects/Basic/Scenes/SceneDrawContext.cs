using Microsoft.Xna.Framework;
using Sachssoft.Engine.Graphics.Cameras;
using Sachssoft.Engine.Graphics.Materials;
using System;

namespace Sachssoft.Engine.Scenes;

/// <summary>
/// Provides contextual information required during scene rendering.
/// </summary>
/// <remarks>
/// The context exposes the current scene, runtime settings, active camera,
/// default material, and view information for multi-view rendering.
/// </remarks>
public class SceneDrawContext : GameContext
{
    /// <summary>Stores the camera for world rendering; absent for presentation scenes.</summary>
    private readonly ICamera? _viewCamera;

    /// <summary>Stores the material for camera-based rendering; absent for presentation scenes.</summary>
    private readonly IMaterial? _defaultMaterial;

    /// <summary>Creates a rendering context without a camera or world material.</summary>
    /// <param name="application">The application that owns the graphics device.</param>
    /// <param name="scene">A camera-free scene with exactly one view.</param>
    /// <param name="frameCounterSmoothing">The frame counter smoothing factor.</param>
    /// <exception cref="ArgumentNullException">The scene or application is null.</exception>
    /// <exception cref="ArgumentException">The scene requires a camera or has more than one view.</exception>
    public SceneDrawContext(GameApplicationBase application, IScene scene, float frameCounterSmoothing = 0.1f)
        : base(application, frameCounterSmoothing)
    {
        ArgumentNullException.ThrowIfNull(scene);
        if (scene.RequiresCamera || scene.ViewCount != 1)
            throw new ArgumentException("A camera-free context requires a camera-free scene with one view.", nameof(scene));

        Scene = scene;
        ViewCount = 1;
        if (scene is ISceneRuntimeSettings settings)
        {
            RuntimeMode = settings.RuntimeMode;
            RuntimeOptions = settings.RuntimeOptions;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SceneDrawContext"/> class.
    /// </summary>
    /// <param name="application">
    /// The game application associated with this rendering context.
    /// </param>
    /// <param name="scene">
    /// The scene currently being rendered.
    /// </param>
    /// <param name="viewCamera">
    /// The camera assigned to the current rendering view.
    /// </param>
    /// <param name="defaultMaterial">
    /// The default material used when a drawable object does not provide
    /// a custom material.
    /// </param>
    /// <param name="viewIndex">
    /// The zero-based index of the current rendering view.
    /// </param>
    /// <param name="viewCount">
    /// The total number of rendering views.
    /// </param>
    /// <param name="frameCounterSmoothing">
    /// The smoothing factor used by the internal frame counter.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="application"/>, <paramref name="scene"/>,
    /// <paramref name="viewCamera"/>, or <paramref name="defaultMaterial"/>
    /// is <see langword="null"/>.
    /// </exception>
    public SceneDrawContext(
        GameApplicationBase application,
        IScene scene,
        ICamera viewCamera,
        IMaterial defaultMaterial,
        int viewIndex,
        int viewCount,
        float frameCounterSmoothing = 0.1f)
        : base(application, frameCounterSmoothing)
    {
        Scene = scene ??
            throw new ArgumentNullException(nameof(scene));

        _viewCamera = viewCamera ??
            throw new ArgumentNullException(nameof(viewCamera));

        _defaultMaterial = defaultMaterial ??
            throw new ArgumentNullException(nameof(defaultMaterial));

        ViewIndex = viewIndex;
        ViewCount = viewCount;

        RuntimeMode = RuntimeMode.Game;
        RuntimeOptions = RuntimeOptions.None;

        if (scene is ISceneRuntimeSettings runtimeSettings)
        {
            RuntimeMode = runtimeSettings.RuntimeMode;
            RuntimeOptions = runtimeSettings.RuntimeOptions;
        }
    }

    /// <summary>
    /// Gets the scene currently being rendered.
    /// </summary>
    public IScene Scene { get; }

    /// <summary>
    /// Gets the runtime mode that defines how the current scene is executed.
    /// </summary>
    public RuntimeMode RuntimeMode { get; }

    /// <summary>
    /// Gets the runtime options enabled for the current scene execution.
    /// </summary>
    public RuntimeOptions RuntimeOptions { get; }

    /// <summary>
    /// Gets the camera assigned to the current rendering view.
    /// </summary>
    /// <exception cref="InvalidOperationException">The scene has no camera.</exception>
    public ICamera ViewCamera => _viewCamera ??
        throw new InvalidOperationException("This scene renders without a camera.");

    /// <summary>Gets whether this context provides a world camera and material.</summary>
    public bool HasCamera => _viewCamera is not null;

    /// <summary>
    /// Gets the zero-based index of the current rendering view.
    /// </summary>
    public int ViewIndex { get; }

    /// <summary>
    /// Gets the total number of rendering views.
    /// </summary>
    public int ViewCount { get; }

    /// <summary>
    /// Gets the default material used when a drawable object does not
    /// provide a custom material.
    /// </summary>
    /// <exception cref="InvalidOperationException">The scene has no world material.</exception>
    public IMaterial DefaultMaterial => _defaultMaterial ??
        throw new InvalidOperationException("This scene renders without a world material.");

    /// <summary>
    /// Calculates the viewport assigned to the current rendering view.
    /// </summary>
    /// <param name="screenBounds">
    /// The available screen bounds used as the base viewport region.
    /// </param>
    /// <returns>
    /// The viewport rectangle assigned to the current rendering view.
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// Thrown when the configured view count or view index is not supported.
    /// </exception>
    public Rectangle GetViewport(Rectangle screenBounds)
    {
        int width = screenBounds.Width;
        int height = screenBounds.Height;

        return ViewCount switch
        {
            1 => new Rectangle(
                screenBounds.X,
                screenBounds.Y,
                width,
                height),

            2 => ViewIndex switch
            {
                0 => new Rectangle(
                    screenBounds.X,
                    screenBounds.Y,
                    width,
                    height / 2),

                1 => new Rectangle(
                    screenBounds.X,
                    screenBounds.Y + height / 2,
                    width,
                    height / 2),

                _ => throw new NotSupportedException()
            },

            3 => ViewIndex switch
            {
                0 => new Rectangle(
                    screenBounds.X,
                    screenBounds.Y,
                    width / 2,
                    height / 2),

                1 => new Rectangle(
                    screenBounds.X + width / 2,
                    screenBounds.Y,
                    width / 2,
                    height / 2),

                2 => new Rectangle(
                    screenBounds.X,
                    screenBounds.Y + height / 2,
                    width,
                    height / 2),

                _ => throw new NotSupportedException()
            },

            4 => ViewIndex switch
            {
                0 => new Rectangle(
                    screenBounds.X,
                    screenBounds.Y,
                    width / 2,
                    height / 2),

                1 => new Rectangle(
                    screenBounds.X + width / 2,
                    screenBounds.Y,
                    width / 2,
                    height / 2),

                2 => new Rectangle(
                    screenBounds.X,
                    screenBounds.Y + height / 2,
                    width / 2,
                    height / 2),

                3 => new Rectangle(
                    screenBounds.X + width / 2,
                    screenBounds.Y + height / 2,
                    width / 2,
                    height / 2),

                _ => throw new NotSupportedException()
            },

            _ => throw new NotSupportedException()
        };
    }
}