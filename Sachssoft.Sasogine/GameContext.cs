using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Diagnostics;
using Sachssoft.Sasogine.Graphics;
using System;

namespace Sachssoft.Sasogine
{
    /// <summary>
    /// Core runtime context that exposes fundamental engine information.
    /// No SceneBase reference is included to keep the backend isolated.
    /// </summary>
    public class GameContext : IDisposable
    {
        private readonly FrameCounter _frameCounter = new FrameCounter();
        private readonly GameApplication _application;
        private GameTime _gameTime;

        public GameContext(GameApplication application)
        {
            _application = application ?? throw new ArgumentNullException(nameof(application));
        }

        public void BeginBenchmark()
        {
        }

        public void EndBenchmark()
        {
        }

        public virtual void Update(GameTime gameTime)
        {
            _gameTime = gameTime ?? throw new ArgumentNullException(nameof(gameTime));
            _frameCounter.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
        }

        /// <summary>
        /// The <see cref="GameTime"/> associated with this frame.
        /// </summary>
        public GameTime GameTime => _gameTime;

        /// <summary>
        /// Elapsed time since the last frame in seconds.
        /// </summary>
        public float ElapsedTimeInSeconds => (float)GameTime.ElapsedGameTime.TotalSeconds;

        /// <summary>
        /// Elapsed time since the last frame in milliseconds.
        /// </summary>
        public float ElapsedTimeInMilliseconds => (float)GameTime.ElapsedGameTime.TotalMilliseconds;

        /// <summary>
        /// The frame counter tracking frame rate and frame time statistics.
        /// </summary>
        public FrameCounter FrameCounter => _frameCounter;

        /// <summary>
        /// Direct reference to the MonoGame GraphicsDevice.
        /// Cached for performance instead of using IGameApplication.Current.
        /// </summary>
        public GraphicsDevice GraphicsDevice => _application.GraphicsDevice;

        /// <summary>
        /// Optional benchmark time (set by external system like GameFrameContext).
        /// </summary>
        public TimeSpan BenchmarkTime { get; init; }

        public virtual void Dispose()
        {
        }
    }
}
