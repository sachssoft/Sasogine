using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Diagnostics;
using System;

namespace Sachssoft.Sasogine;

/// <summary>
/// Provides runtime context information for the current game execution.
/// </summary>
/// <remarks>
/// The context exposes frame timing, frame rate statistics, graphics resources,
/// and the application registry.
/// </remarks>
public class GameContext
{
    private readonly IGameApplication _application;
    private readonly FrameCounter _frameCounter;

    private GameTime? _gameTime;

    /// <summary>
    /// Initializes a new instance of the <see cref="GameContext"/> class.
    /// </summary>
    /// <param name="application">
    /// The game application associated with this context.
    /// </param>
    /// <param name="frameCounterSmoothing">
    /// The smoothing factor used by the internal frame counter.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="application"/> is <see langword="null"/>.
    /// </exception>
    public GameContext(
        IGameApplication application,
        float frameCounterSmoothing = 0.1f)
    {
        _application = application ??
            throw new ArgumentNullException(nameof(application));

        _frameCounter = new FrameCounter(frameCounterSmoothing);
    }

    /// <summary>
    /// Gets the timing information associated with the current frame.
    /// </summary>
    /// <exception cref="GameException">
    /// Thrown when <see cref="SetFrameTime"/> has not been called yet.
    /// </exception>
    public GameTime GameTime =>
        _gameTime ??
        throw new GameException(
            "GameContext.SetFrameTime has not been called yet.");

    /// <summary>
    /// Gets the elapsed time of the current frame, in seconds.
    /// </summary>
    public float ElapsedTimeInSeconds =>
        (float)GameTime.ElapsedGameTime.TotalSeconds;

    /// <summary>
    /// Gets the elapsed time of the current frame, in milliseconds.
    /// </summary>
    public float ElapsedTimeInMilliseconds =>
        (float)GameTime.ElapsedGameTime.TotalMilliseconds;

    /// <summary>
    /// Gets the frame counter used to track frame rate statistics.
    /// </summary>
    public FrameCounter FrameCounter => _frameCounter;

    /// <summary>
    /// Gets the graphics device associated with the game application.
    /// </summary>
    public GraphicsDevice GraphicsDevice =>
        _application.GraphicsDevice;

    /// <summary>
    /// Gets the game registry associated with the application.
    /// </summary>
    public GameRegistry Registry =>
        _application.Registry;

    /// <summary>
    /// Gets the benchmark duration associated with the current frame.
    /// </summary>
    public TimeSpan BenchmarkTime { get; internal set; }

    /// <summary>
    /// Updates the frame timing information for this context.
    /// </summary>
    /// <param name="gameTime">
    /// The timing information for the current frame.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="gameTime"/> is <see langword="null"/>.
    /// </exception>
    public void SetFrameTime(GameTime gameTime)
    {
        ArgumentNullException.ThrowIfNull(gameTime);

        _gameTime = gameTime;

        _frameCounter.Update(
            (float)gameTime.ElapsedGameTime.TotalSeconds);
    }
}