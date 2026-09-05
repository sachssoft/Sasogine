namespace Sachssoft.Sasogine.Assets.Graphics
{
    /// <summary>
    /// Specifies the supported image formats.
    /// </summary>
    public enum ImageFormatType
    {
        /// <summary>
        /// The image format is unknown or could not be detected.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Portable Network Graphics (PNG).
        /// </summary>
        Png,

        /// <summary>
        /// Joint Photographic Experts Group (JPEG).
        /// </summary>
        Jpeg,

        /// <summary>
        /// Bitmap (BMP).
        /// </summary>
        Bmp,

        /// <summary>
        /// Truevision TGA (Targa).
        /// </summary>
        Tga,

        /// <summary>
        /// DirectDraw Surface (DDS).
        /// </summary>
        Dds
    }
}