using Sachssoft.Sasogine.Graphics.Text;
using System;
using System.IO;

namespace Sachssoft.Sasogine.Assets.Graphics
{
    /// <summary>
    /// Represents a managed font asset for the Sasogine graphics system.
    /// </summary>
    /// <remarks>
    /// <see cref="FontAsset"/> loads binary font data from an asset stream and
    /// creates a self-contained runtime <see cref="FontFace"/>.
    /// </remarks>
    public sealed class FontAsset :
        AssetBase<FontFace, FontAssetDefinition>
    {
        /// <summary>
        /// Initializes a new empty instance of the <see cref="FontAsset"/> class.
        /// </summary>
        /// <param name="id">
        /// The optional identifier of the asset.
        /// </param>
        /// <param name="class">
        /// The optional class of the asset.
        /// </param>
        public FontAsset(
            string? id = null,
            string? @class = null)
            : base(new FontAssetDefinition
            {
                Id = id,
                Class = @class,
            })
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FontAsset"/> class
        /// using the specified definition.
        /// </summary>
        /// <param name="definition">
        /// The asset definition containing the font configuration.
        /// </param>
        public FontAsset(FontAssetDefinition definition)
            : base(definition)
        {
        }

        /// <summary>
        /// Resolves the default definition used by this asset.
        /// </summary>
        /// <returns>
        /// A new <see cref="FontAssetDefinition"/> instance.
        /// </returns>
        protected override FontAssetDefinition ResolveDefinition()
        {
            return new FontAssetDefinition();
        }

        /// <summary>
        /// Builds the runtime font face from the supplied font stream.
        /// </summary>
        /// <param name="stream">
        /// The stream containing the source font data.
        /// </param>
        /// <returns>
        /// The created <see cref="FontFace"/>.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// The font face name is not configured.
        /// </exception>
        protected override FontFace? Build(Stream stream)
        {
            ArgumentNullException.ThrowIfNull(stream);

            if (string.IsNullOrEmpty(Definition.Name))
            {
                throw new InvalidOperationException(
                    $"{nameof(FontAssetDefinition.Name)} is not configured.");
            }

            using var memory = new MemoryStream();

            stream.CopyTo(memory);

            return new FontFace(
                memory.ToArray(),
                Definition.Name,
                Definition.WeightDefinition,
                Definition.StyleDefinition);
        }
    }
}