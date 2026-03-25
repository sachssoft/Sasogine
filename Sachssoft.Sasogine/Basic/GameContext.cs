using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Diagnostics;
using System;

namespace Sachssoft.Sasogine
{
    /// <summary>
    /// Core runtime context that exposes fundamental engine information.
    /// Developers can instantiate with custom FrameCounter parameters.
    /// </summary>
    public class GameContext : IDisposable
    {
        private readonly GameApplication _application;
        private readonly FrameCounter _frameCounter;
        private GameTime? _gameTime;

        /// <summary>
        /// Public constructor.
        /// Optionally configure the internal high-performance FrameCounter.
        /// </summary>
        /// <param name="application">Reference to the GameApplication.</param>
        /// <param name="frameCounterSmoothing">EMA smoothing factor for FrameCounter (0..1, default 0.1).</param>
        /// <param name="frameCounterFastWeight">Fast weight for FrameCounter (0..1, default 0.2).</param>
        public GameContext(GameApplication application, float frameCounterSmoothing = 0.1f, float frameCounterFastWeight = 0.2f)
        {
            _application = application ?? throw new ArgumentNullException(nameof(application));
            _frameCounter = new FrameCounter(frameCounterSmoothing, frameCounterFastWeight);
        }

        /// <summary>
        /// Update the GameContext with the current GameTime.
        /// Automatically updates the internal FrameCounter.
        /// </summary>
        public void Update(GameTime gameTime)
        {
            if (gameTime == null)
                throw new ArgumentNullException(nameof(gameTime));

            _gameTime = gameTime;
            _frameCounter.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
        }

        /// <summary>
        /// The GameTime associated with this frame.
        /// Throws if Update has not been called yet.
        /// </summary>
        public GameTime GameTime => _gameTime ?? throw new GameException("GameContext.Update has not been called yet.");

        /// <summary>
        /// Elapsed time since the last frame in seconds.
        /// </summary>
        public float ElapsedTimeInSeconds => (float)GameTime.ElapsedGameTime.TotalSeconds;

        /// <summary>
        /// Elapsed time since the last frame in milliseconds.
        /// </summary>
        public float ElapsedTimeInMilliseconds => (float)GameTime.ElapsedGameTime.TotalMilliseconds;

        /// <summary>
        /// FrameCounter tracking FPS and frame statistics (GC-free, high-performance).
        /// Readonly – configured via constructor.
        /// </summary>
        public FrameCounter FrameCounter => _frameCounter;

        /// <summary>
        /// Direct reference to the MonoGame GraphicsDevice.
        /// Cached for performance.
        /// </summary>
        public GraphicsDevice GraphicsDevice => _application.GraphicsDevice;

        public GameRegistry Registry => _application.Registry;

        /// <summary>
        /// Optional benchmark time set externally (read-only for developers).
        /// </summary>
        public TimeSpan BenchmarkTime { get; internal set; }

        public void Dispose()
        {
            // currently no disposable resources
        }
    }
}