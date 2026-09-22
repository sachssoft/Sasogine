using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Engine.Resources;
using System;
using System.IO;

namespace Sachssoft.Engine.Assets.Graphics;

/// <summary>
/// Represents a managed 3D model asset for the Sasogine graphics system.
/// </summary>
/// <remarks>
/// <see cref="ModelAsset"/> provides a managed <see cref="Model"/> resource
/// configured through a <see cref="ModelAssetDefinition"/>.
/// </remarks>
public class ModelAsset : AssetBase<Model, ModelAssetDefinition>
{
    /// <summary>
    /// Initializes a new empty instance of the <see cref="ModelAsset"/> class.
    /// </summary>
    /// <param name="id">The optional identifier of the asset.</param>
    /// <param name="class">The optional class of the asset.</param>
    public ModelAsset(string? id = null, string? @class = null)
        : base(new ModelAssetDefinition
        {
            Id = id,
            Class = @class,
        })
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModelAsset"/> class
    /// using the specified definition.
    /// </summary>
    /// <param name="definition">
    /// The asset definition containing the model configuration.
    /// </param>
    public ModelAsset(ModelAssetDefinition definition)
        : base(definition)
    {
    }

    /// <summary>
    /// Initializes a new model asset using the specified resource source.
    /// </summary>
    /// <param name="id">The optional identifier of the asset.</param>
    /// <param name="loaderSource">The resource source used to load the model.</param>
    public ModelAsset(string? id, ResourceSourceBase? loaderSource)
        : base(new ModelAssetDefinition { Id = id })
    {
        LoaderSource = loaderSource;
    }

    /// <summary>
    /// Initializes a new model asset using the specified definition and resource source.
    /// </summary>
    /// <param name="definition">The model asset definition.</param>
    /// <param name="loaderSource">The resource source used to load the model.</param>
    public ModelAsset(ModelAssetDefinition definition, ResourceSourceBase? loaderSource)
        : base(definition)
    {
        LoaderSource = loaderSource;
    }

    /// <summary>
    /// Builds the runtime model from the supplied stream.
    /// </summary>
    /// <param name="stream">The stream containing the model data.</param>
    /// <returns>The created runtime model.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="stream"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// The asset has not been initialized.
    /// </exception>
    protected override Model Build(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        GraphicsDevice graphicsDevice = Context?.GraphicsDevice ??
            throw new InvalidOperationException(
                $"{nameof(ModelAsset)} must be initialized before calling {nameof(Build)}.");

        // Model loading/importing belongs here.
        throw new NotImplementedException(
            "Model stream loading has not been implemented.");
    }
}