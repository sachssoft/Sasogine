namespace Sachssoft.Sasogine.Assets.Audio
{
    /// <summary>
    /// Specifies the audio format used by a sound asset.
    /// </summary>
    public enum SoundFormatType
    {
        /// <summary>
        /// Automatically detects the sound format from the audio data.
        /// </summary>
        Auto,

        /// <summary>
        /// Represents an Ogg audio stream.
        /// </summary>
        Ogg,

        /// <summary>
        /// Represents the Waveform Audio File Format (WAV).
        /// </summary>
        Wav
    }
}