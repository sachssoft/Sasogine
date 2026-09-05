namespace Sachssoft.Sasogine.Audio
{
    /// <summary>
    /// Defines a contract for sound effect playback.
    /// </summary>
    public interface ISoundPlayer
    {
        /// <summary>
        /// Starts sound playback.
        /// </summary>
        void Play();

        /// <summary>
        /// Stops sound playback.
        /// </summary>
        void Stop();

        /// <summary>
        /// Pauses sound playback.
        /// </summary>
        void Pause();

        /// <summary>
        /// Resumes sound playback after it has been paused.
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
        /// Gets a value indicating whether the sound is currently playing.
        /// </summary>
        bool IsPlaying { get; }
    }
}