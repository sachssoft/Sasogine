using Sachssoft.Sasogine.Graphics.Text;

namespace Sachssoft.Sasogine.Assets.Graphics
{
    /// <summary>
    /// Defines the configuration used to create a <see cref="FontAsset"/>.
    /// </summary>
    public class FontAssetDefinition : AssetDefinitionBase<FontAsset>
    {
        /// <summary>
        /// Gets or sets the font weight definition.
        /// </summary>
        public FontWeight WeightDefinition { get; set; }

        /// <summary>
        /// Gets or sets the font style definition.
        /// </summary>
        public FontStyle StyleDefinition { get; set; }
    }
}