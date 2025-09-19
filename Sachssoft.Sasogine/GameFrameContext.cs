using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Diagnostics;
using Sachssoft.Sasogine.Surface;
using System;

namespace Sachssoft.Sasogine
{
    /// <summary>
    /// Represents the game context for a single frame.
    /// Inherits from <see cref="GameBaseContext"/> and adds frame-specific information such as GameTime,
    /// elapsed time, frame counter, and UI visibility.
    /// </summary>
    public class GameFrameContext : GameBaseContext
    {
        /// <summary>
        /// Creates a new <see cref="GameFrameContext"/> by copying an existing instance.
        /// </summary>
        /// <param name="self">The existing context to copy from.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="self"/> is null.</exception>
        public GameFrameContext(GameFrameContext self)
            : this(self?.View ?? throw new ArgumentNullException(nameof(self)),
                   self.GameTime ?? throw new ArgumentNullException(nameof(self.GameTime)))
        {
        }

        /// <summary>
        /// Creates a new <see cref="GameFrameContext"/> for a specified View and GameTime.
        /// </summary>
        /// <param name="view">The View associated with this frame.</param>
        /// <param name="time">The GameTime for this frame.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="view"/> or <paramref name="time"/> is null.</exception>
        public GameFrameContext(ViewBase view, GameTime time)
            : base(view)
        {
            GameTime = time ?? throw new ArgumentNullException(nameof(time));
            IsUIVisibled = true;
            FrameCounter = new FrameCounter();

            FrameCounter.Update(ElapsedTimeInSeconds);
        }

        /// <summary>
        /// The <see cref="GameTime"/> associated with this frame.
        /// </summary>
        public GameTime GameTime { get; }

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
        public FrameCounter FrameCounter { get; }

        /// <summary>
        /// Indicates whether the UI elements should be visible during this frame.
        /// </summary>
        public bool IsUIVisibled { get; set; }
    }
}
