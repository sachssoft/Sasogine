using System;

namespace Sachssoft.Sasogine.Audio
{
    /// <summary>
    /// Defines a contract for music playback.
    /// </summary>
    public interface IMusicPlayer
    {
        /// <summary>
        /// Starts music playback.
        /// </summary>
        void Play();

        /// <summary>
        /// Stops music playback.
        /// </summary>
        void Stop();

        /// <summary>
        /// Pauses music playback.
        /// </summary>
        void Pause();

        /// <summary>
        /// Resumes music playback after it has been paused.
        /// </summary>
        void Resume();

        /// <summary>
        /// Gets or sets the playback volume.
        /// </summary>
        float Volume { get; set; }

        /// <summary>
        /// Gets or sets the playback pitch multiplier.
        /// </summary>
        /// <remarks>
        /// A value of <c>1</c> represents the normal playback pitch.
        /// </remarks>
        float Pitch { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether playback repeats
        /// after reaching the end of the music.
        /// </summary>
        bool IsLooping { get; set; }

        /// <summary>
        /// Gets or sets the position from which playback starts.
        /// </summary>
        TimeSpan StartOffset { get; set; }

        /// <summary>
        /// Gets a value indicating whether music is currently playing.
        /// </summary>
        bool IsPlaying { get; }

        /// <summary>
        /// Gets the current playback position, in seconds.
        /// </summary>
        double Position { get; }
    }
}