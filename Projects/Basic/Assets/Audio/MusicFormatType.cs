namespace Sachssoft.Sasogine.Assets.Audio
{
    /// <summary>
    /// Specifies the audio format used by a music asset.
    /// </summary>
    public enum MusicFormatType
    {
        /// <summary>
        /// Automatically detects the music format from the audio data.
        /// </summary>
        Auto,

        /// <summary>
        /// Represents an Ogg audio stream.
        /// </summary>
        Ogg,

        /// <summary>
        /// Represents an MPEG Layer III (MP3) audio stream.
        /// </summary>
        Mp3
    }
}