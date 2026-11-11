using Sachssoft.Engine.Components.Definitions;
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
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the font weight.
        /// </summary>
        public FontWeight Weight { get; set; } = FontWeight.Normal;

        /// <summary>
        /// Gets or sets the font style.
        /// </summary>
        public FontStyle Style { get; set; } = FontStyle.Normal;
    }
}