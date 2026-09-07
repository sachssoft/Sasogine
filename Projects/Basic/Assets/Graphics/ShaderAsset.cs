using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Graphics.Rendering;
using System;
using System.IO;

namespace Sachssoft.Sasogine.Assets.Graphics;

/// <summary>
/// Represents a managed shader asset for the Sasogine graphics system.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="ShaderAsset"/> creates and configures an <see cref="IShader"/>
/// instance from a shader template defined by <see cref="ShaderAssetDefinition"/>.
/// </para>
/// <para>
/// A valid <see cref="GraphicsDevice"/> must be assigned before the runtime
/// shader resource can be created.
/// </para>
/// </remarks>
public class ShaderAsset : AssetBase<IShader, ShaderAssetDefinition>
{
    /// <summary>
    /// Initializes a new empty instance of the <see cref="ShaderAsset"/> class.
    /// </summary>
    /// <param name="id">
    /// The optional identifier of the asset.
    /// </param>
    /// <param name="class">
    /// The optional class of the asset.
    /// </param>
    public ShaderAsset(
        string? id = null,
        string? @class = null)
        : base(new ShaderAssetDefinition
        {
            Id = id,
            Class = @class,
        })
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ShaderAsset"/> class
    /// using the specified definition.
    /// </summary>
    /// <param name="definition">
    /// The asset definition containing the shader configuration.
    /// </param>
    public ShaderAsset(
        ShaderAssetDefinition definition)
        : base(definition)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ShaderAsset"/> class
    /// using the specified graphics device.
    /// </summary>
    /// <param name="graphicsDevice">
    /// The graphics device used to create the shader.
    /// </param>
    /// <param name="id">
    /// The optional identifier of the asset.
    /// </param>
    /// <param name="class">
    /// The optional class of the asset.
    /// </param>
    public ShaderAsset(
        GraphicsDevice graphicsDevice,
        string? id = null,
        string? @class = null)
        : this(id, @class)
    {
        GraphicsDevice = graphicsDevice;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ShaderAsset"/> class
    /// using the specified graphics device and asset definition.
    /// </summary>
    /// <param name="graphicsDevice">
    /// The graphics device used to create the shader.
    /// </param>
    /// <param name="definition">
    /// The asset definition containing the shader configuration.
    /// </param>
    public ShaderAsset(
        GraphicsDevice graphicsDevice,
        ShaderAssetDefinition definition)
        : base(definition)
    {
        GraphicsDevice = graphicsDevice;
    }

    /// <summary>
    /// Gets or sets the graphics device used to create the shader.
    /// </summary>
    /// <value>
    /// The graphics device, or <see langword="null"/> if no graphics device
    /// has been assigned.
    /// </value>
    public GraphicsDevice? GraphicsDevice { get; set; }

    /// <summary>
    /// Resolves the default definition used by this asset.
    /// </summary>
    /// <returns>
    /// A new <see cref="ShaderAssetDefinition"/> instance.
    /// </returns>
    protected override ShaderAssetDefinition ResolveDefinition()
    {
        return new ShaderAssetDefinition();
    }

    /// <summary>
    /// Creates the runtime shader resource from the supplied stream.
    /// </summary>
    /// <param name="stream">
    /// The stream associated with the shader resource.
    /// </param>
    /// <returns>
    /// The created <see cref="IShader"/> instance, or <see langword="null"/>
    /// if no shader template is configured.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="stream"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// No <see cref="GraphicsDevice"/> has been assigned.
    /// </exception>
    protected override IShader? Build(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        if (GraphicsDevice == null)
        {
            throw new InvalidOperationException(
                $"{nameof(ShaderAsset)} requires a valid {nameof(GraphicsDevice)} before loading.");
        }

        if (Definition.Template == null)
            return null;

        var shader = Definition.Template.Create();
        shader.GraphicsDevice = GraphicsDevice;

        return shader;
    }
}