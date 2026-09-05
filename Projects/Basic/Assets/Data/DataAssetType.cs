namespace Sachssoft.Sasogine.Assets.Data
{
    /// <summary>
    /// Specifies the intended target or purpose of a data asset.
    /// </summary>
    public enum DataAssetTarget
    {
        /// <summary>
        /// Represents data describing a texture set,
        /// including texture regions or frame information.
        /// </summary>
        TextureSet,

        /// <summary>
        /// Represents data describing a sprite.
        /// </summary>
        Sprite,

        /// <summary>
        /// Represents data describing an animation.
        /// </summary>
        Animation,

        /// <summary>
        /// Represents data intended for a custom or application-specific target.
        /// </summary>
        Custom
    }
}