using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Components.Models;
using Sachssoft.Sasogine.Graphics;
using System.ComponentModel;

namespace Sachssoft.Sasogine.Assets.Graphics
{
    /// <summary>
    /// Defines the configuration used to create a <see cref="Texture2DAsset"/>.
    /// </summary>
    public class Texture2DAssetDefinition : AssetDefinitionBase
    {
        /// <summary>
        /// Gets or sets the texture pattern.
        /// </summary>
        [Category(Categories.Appearance)]
        [DisplayName("Pattern")]
        public Texture2DPattern Pattern { get; set; } =
            Texture2DPattern.Stretch;

        /// <summary>
        /// Gets or sets the texture pattern mode.
        /// </summary>
        [Category(Categories.Appearance)]
        [DisplayName("Repeat")]
        public Texture2DPatternMode PatternMode { get; set; } =
            Texture2DPatternMode.Repeat;

        /// <summary>
        /// Gets or sets the diffuse color applied to the texture.
        /// </summary>
        [Category(Categories.Appearance)]
        [DisplayName("Diffuse Color")]
        public Color DiffuseColor { get; set; } =
            Color.White;

        /// <summary>
        /// Gets or sets the texture opacity.
        /// </summary>
        [Category(Categories.Appearance)]
        [DisplayName("Opacity")]
        public float Opacity { get; set; } = 1.0f;

        /// <summary>
        /// Gets or sets the texture filtering mode.
        /// </summary>
        [Category(Categories.Rendering)]
        [DisplayName("Filter Mode")]
        public Texture2DFilterMode FilterMode { get; set; } =
            Texture2DFilterMode.Point;

        /// <summary>
        /// Gets or sets the texture addressing mode.
        /// </summary>
        [Category(Categories.Rendering)]
        [DisplayName("Address Mode")]
        public Texture2DAddressMode AddressMode { get; set; } =
            Texture2DAddressMode.Clamp;

        /// <summary>
        /// Gets or sets the texture flip mode.
        /// </summary>
        [Category(Categories.Rendering)]
        [DisplayName("Flip Mode")]
        public Texture2DFlipMode FlipMode { get; set; } =
            Texture2DFlipMode.None;

        /// <summary>
        /// Gets or sets the texture blending mode.
        /// </summary>
        [Category(Categories.Rendering)]
        [DisplayName("Blend Mode")]
        public Texture2DBlendMode BlendMode { get; set; } =
            Texture2DBlendMode.AlphaBlend;

        /// <summary>
        /// Gets or sets a value indicating whether mipmaps are generated
        /// for the texture.
        /// </summary>
        [Category(Categories.Rendering)]
        [DisplayName("Use Mipmaps")]
        public bool UseMipmaps { get; set; } = false;
    }
}