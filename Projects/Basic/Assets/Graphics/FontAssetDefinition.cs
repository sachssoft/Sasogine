using Sachssoft.Engine.Components.Models;
using Sachssoft.Engine.Graphics.Text;
using System.ComponentModel;

namespace Sachssoft.Engine.Assets.Graphics
{
    /// <summary>
    /// Defines the configuration used to create a <see cref="FontAsset"/>.
    /// </summary>
    public class FontAssetDefinition : AssetDefinitionBase
    {
        /// <summary>
        /// Gets or sets the name of the font face.
        /// </summary>
        [Category(Categories.Common)]
        [DisplayName("Name")]
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the font weight.
        /// </summary>
        [Category(Categories.Common)]
        [DisplayName("Weight")]
        public FontWeight WeightDefinition { get; set; } = FontWeight.Normal;

        /// <summary>
        /// Gets or sets the font style.
        /// </summary>
        [Category(Categories.Common)]
        [DisplayName("Style")]
        public FontStyle StyleDefinition { get; set; } = FontStyle.Normal;
    }
}