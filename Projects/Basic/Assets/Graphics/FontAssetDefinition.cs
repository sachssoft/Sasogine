using Sachssoft.Sasogine.Graphics.Text;

namespace Sachssoft.Sasogine.Assets.Graphics
{
    /// <summary>
    /// Defines the configuration used to create a <see cref="FontAsset"/>.
    /// </summary>
    public class FontAssetDefinition : AssetDefinitionBase<FontAsset>
    {
        /// <summary>
        /// Gets or sets the name of the font face.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the font weight.
        /// </summary>
        public FontWeight WeightDefinition { get; set; } = FontWeight.Normal;

        /// <summary>
        /// Gets or sets the font style.
        /// </summary>
        public FontStyle StyleDefinition { get; set; } = FontStyle.Normal;
    }
}