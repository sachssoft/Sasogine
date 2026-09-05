namespace Sachssoft.Sasogine.Assets.Audio
{
    /// <summary>
    /// Specifies an audio format detected from an audio resource.
    /// </summary>
    public enum AudioFormatType
    {
        /// <summary>
        /// The audio format is unknown or could not be detected.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Represents the Waveform Audio File Format (WAV).
        /// </summary>
        Wav = 1,

        /// <summary>
        /// Represents an Ogg audio stream.
        /// </summary>
        Ogg = 2,

        /// <summary>
        /// Represents an MPEG Layer III (MP3) audio stream.
        /// </summary>
        Mp3 = 3
    }
}