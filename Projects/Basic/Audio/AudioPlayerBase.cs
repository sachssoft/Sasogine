using System;
using System.IO;

namespace Sachssoft.Sasogine.Audio
{
    /// <summary>
    /// Provides a base implementation for audio players that operate
    /// on an audio resource stream.
    /// </summary>
    public abstract class AudioPlayerBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AudioPlayerBase"/> class
        /// using the specified audio stream.
        /// </summary>
        /// <param name="stream">
        /// The stream containing the audio data used for playback.
        /// </param>
        protected AudioPlayerBase(Stream stream)
        {
            ArgumentNullException.ThrowIfNull(stream);

            Stream = stream;
        }

        /// <summary>
        /// Gets the audio stream used by this player.
        /// </summary>
        protected Stream Stream { get; }

        /// <summary>
        /// Starts playback of the audio stream.
        /// </summary>
        public abstract void Play();

        /// <summary>
        /// Stops playback.
        /// </summary>
        public abstract void Stop();

        /// <summary>
        /// Pauses playback.
        /// </summary>
        public abstract void Pause();

        /// <summary>
        /// Resumes playback after it has been paused.
        /// </summary>
        public abstract void Resume();

        /// <summary>
        /// Gets or sets the playback volume.
        /// </summary>
        /// <remarks>
        /// A value of <c>0</c> represents silence and <c>1</c> represents
        /// the default full volume.
        /// </remarks>
        public virtual float Volume { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether playback repeats
        /// after reaching the end of the audio stream.
        /// </summary>
        public virtual bool IsLooping { get; set; }

        /// <summary>
        /// Gets or sets the playback pitch multiplier.
        /// </summary>
        /// <remarks>
        /// A value of <c>1</c> represents the normal playback pitch.
        /// </remarks>
        public virtual float Pitch { get; set; }

        /// <summary>
        /// Gets a value indicating whether audio is currently playing.
        /// </summary>
        public abstract bool IsPlaying { get; }

        /// <summary>
        /// Gets or sets the position from which playback starts.
        /// </summary>
        public TimeSpan StartOffset { get; set; }

        /// <summary>
        /// Gets the current playback position, in seconds.
        /// </summary>
        public abstract double Position { get; }
    }
}