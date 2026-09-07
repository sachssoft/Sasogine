using Sachssoft.Sasogine.Graphics.Text;
using System;
using System.IO;

namespace Sachssoft.Sasogine.Assets.Graphics
{
    /// <summary>
    /// Represents a managed font asset for the Sasogine graphics system.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="FontAsset"/> is responsible for loading and configuring
    /// <see cref="FontFace"/> resources from asset streams.
    /// </para>
    /// <para>
    /// Font loading is currently not implemented and will be added later.
    /// </para>
    /// </remarks>
    public sealed class FontAsset : AssetBase<FontFace, FontAssetDefinition>
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
        public FontAsset(
            FontAssetDefinition definition)
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
        /// Builds the runtime font resource from the supplied stream.
        /// </summary>
        /// <param name="stream">
        /// The stream containing the source font data.
        /// </param>
        /// <returns>
        /// The created <see cref="FontFace"/> instance.
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