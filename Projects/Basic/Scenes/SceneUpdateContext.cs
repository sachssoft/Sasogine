using Sachssoft.Sasogine.Graphics.Cameras;
using System;

namespace Sachssoft.Sasogine.Scenes;

/// <summary>
/// Provides contextual information required during scene updates.
/// </summary>
/// <remarks>
/// The context exposes the current scene, associated cameras,
/// runtime mode, and runtime options for the current update cycle.
/// </remarks>
public class SceneUpdateContext : GameContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SceneUpdateContext"/> class.
    /// </summary>
    /// <param name="application">
    /// The game application associated with this update context.
    /// </param>
    /// <param name="scene">
    /// The scene currently being updated.
    /// </param>
    /// <param name="cameras">
    /// The cameras associated with the current scene update cycle.
    /// </param>
    /// <param name="frameCounterSmoothing">
    /// The smoothing factor used by the internal frame counter.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="application"/>, <paramref name="scene"/>,
    /// or <paramref name="cameras"/> is <see langword="null"/>.
    /// </exception>
    public SceneUpdateContext(
        IGameApplication application,
        IScene scene,
        ICamera[] cameras,
        float frameCounterSmoothing = 0.1f)
        : base(application, frameCounterSmoothing)
    {
        Scene = scene ??
            throw new ArgumentNullException(nameof(scene));

        Cameras = cameras ??
            throw new ArgumentNullException(nameof(cameras));

        RuntimeMode = RuntimeMode.Game;
        RuntimeOptions = RuntimeOptions.None;

        if (scene is ISceneRuntimeSettings runtimeSettings)
        {
            RuntimeMode = runtimeSettings.RuntimeMode;
            RuntimeOptions = runtimeSettings.RuntimeOptions;
        }
    }

    /// <summary>
    /// Gets the scene currently being updated.
    /// </summary>
    public IScene Scene { get; }

    /// <summary>
    /// Gets the cameras associated with the current scene update cycle.
    /// </summary>
    public ICamera[] Cameras { get; }

    /// <summary>
    /// Gets the runtime mode that defines how the current scene is executed.
    /// </summary>
    public RuntimeMode RuntimeMode { get; }

    /// <summary>
    /// Gets the runtime options enabled for the current scene execution.
    /// </summary>
    public RuntimeOptions RuntimeOptions { get; }
}