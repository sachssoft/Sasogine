using Sachssoft.Sasogine.Graphics.Text;
using System;
using System.IO;

namespace Sachssoft.Sasogine.Assets.Graphics
{
    /// <summary>
    /// Represents a font asset.
    /// </summary>
    /// <remarks>
    /// Font loading is currently not implemented and will be added later.
    /// </remarks>
    public sealed class FontAsset : AssetBase<FontFace, FontAssetDefinition>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FontAsset"/> class.
        /// </summary>
        public FontAsset()
            : base(new FontAssetDefinition())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FontAsset"/> class
        /// using the specified definition.
        /// </summary>
        /// <param name="definition">
        /// The definition used to configure the font asset.
        /// </param>
        public FontAsset(FontAssetDefinition definition)
            : base(definition)
        {
        }

        /// <summary>
        /// Resolves the definition used by this asset.
        /// </summary>
        /// <returns>
        /// A new font asset definition.
        /// </returns>
        protected override FontAssetDefinition ResolveDefinition()
        {
            return new FontAssetDefinition();
        }

        /// <summary>
        /// Builds the font from the specified resource stream.
        /// </summary>
        /// <param name="stream">
        /// The stream containing the font data.
        /// </param>
        /// <returns>
        /// The created font face.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// Font loading is not implemented yet.
        /// </exception>
        protected override FontFace? Build(Stream stream)
        {
            throw new NotImplementedException();
        }
    }
}