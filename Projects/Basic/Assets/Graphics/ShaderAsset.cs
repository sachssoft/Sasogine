using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Graphics.Rendering;
using System;
using System.IO;

namespace Sachssoft.Sasogine.Assets.Graphics;

/// <summary>
/// Represents a shader asset that creates an <see cref="IShader"/>
/// instance for the configured graphics device.
/// </summary>
public class ShaderAsset : AssetBase<IShader, ShaderAssetDefinition>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ShaderAsset"/> class.
    /// </summary>
    public ShaderAsset()
        : base(new ShaderAssetDefinition())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ShaderAsset"/> class
    /// using the specified definition.
    /// </summary>
    /// <param name="definition">
    /// The definition used to configure the shader asset.
    /// </param>
    public ShaderAsset(ShaderAssetDefinition definition)
        : base(definition)
    {
    }

    /// <summary>
    /// Gets or sets the graphics device used to create the shader.
    /// </summary>
    public GraphicsDevice? GraphicsDevice { get; set; }

    /// <summary>
    /// Resolves the definition used by this asset.
    /// </summary>
    /// <returns>
    /// A new shader asset definition.
    /// </returns>
    protected override ShaderAssetDefinition ResolveDefinition()
    {
        return new ShaderAssetDefinition();
    }

    /// <summary>
    /// Creates the shader instance from the specified resource stream.
    /// </summary>
    /// <param name="stream">
    /// The stream containing the shader resource.
    /// </param>
    /// <returns>
    /// The created shader instance, or <see langword="null"/> when no
    /// shader template is configured.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no graphics device has been assigned.
    /// </exception>
    protected override IShader? Build(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        if (GraphicsDevice == null)
        {
            throw new InvalidOperationException(
                $"{nameof(ShaderAsset)} requires a valid " +
                $"{nameof(GraphicsDevice)} before loading.");
        }

        if (Definition.Template == null)
            return null;

        var shader = Definition.Template.Create();

        shader.GraphicsDevice = GraphicsDevice;

        return shader;
    }
}