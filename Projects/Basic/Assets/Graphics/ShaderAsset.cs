using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Common;
using Sachssoft.Sasogine.Graphics.Rendering;
using Sachssoft.Sasogine.Resources;
using System;
using System.IO;

namespace Sachssoft.Sasogine.Assets.Graphics;

/// <summary>
/// Represents a managed shader asset for the Sasogine graphics system.
/// </summary>
/// <remarks>
/// <see cref="ShaderAsset"/> creates an <see cref="IShader"/> instance from
/// the context-aware template defined by <see cref="ShaderAssetDefinition"/>.
/// </remarks>
public class ShaderAsset : AssetBase<IShader, ShaderAssetDefinition>
{
    /// <summary>
    /// Initializes a new shader asset.
    /// </summary>
    /// <param name="id">The optional asset identifier.</param>
    public ShaderAsset(string? id)
        : base(new ShaderAssetDefinition { Id = id })
    {
    }

    /// <summary>
    /// Initializes a new shader asset using the specified definition.
    /// </summary>
    /// <param name="definition">The shader asset definition.</param>
    public ShaderAsset(ShaderAssetDefinition definition)
        : base(definition)
    {
    }

    /// <summary>
    /// Initializes a new shader asset using the specified resource source
    /// and context-aware shader template.
    /// </summary>
    /// <param name="id">The optional asset identifier.</param>
    /// <param name="loaderSource">The resource source associated with the shader.</param>
    /// <param name="template">The context-aware shader template.</param>
    public ShaderAsset(
        string? id,
        ResourceSourceBase? loaderSource,
        Template<IShader, ShaderAssetContext> template)
        : base(new ShaderAssetDefinition
        {
            Id = id,
            Template = template
        })
    {
        ArgumentNullException.ThrowIfNull(template);
        LoaderSource = loaderSource;
    }

    /// <summary>
    /// Initializes a new shader asset using the specified resource source
    /// and context-aware shader factory.
    /// </summary>
    /// <param name="id">The optional asset identifier.</param>
    /// <param name="loaderSource">The resource source associated with the shader.</param>
    /// <param name="factory">
    /// The factory used to create the shader using the supplied
    /// <see cref="ShaderAssetContext"/>.
    /// </param>
    public ShaderAsset(
        string? id,
        ResourceSourceBase? loaderSource,
        Func<ShaderAssetContext, IShader> factory)
        : this(id, loaderSource,
            new Template<IShader, ShaderAssetContext>(factory))
    {
    }

    /// <summary>
    /// Initializes a new shader asset using the specified definition,
    /// resource source, and context-aware shader template.
    /// </summary>
    /// <param name="definition">The shader asset definition.</param>
    /// <param name="loaderSource">The resource source associated with the shader.</param>
    /// <param name="template">The context-aware shader template.</param>
    public ShaderAsset(
        ShaderAssetDefinition definition,
        ResourceSourceBase? loaderSource,
        Template<IShader, ShaderAssetContext> template)
        : base(definition)
    {
        ArgumentNullException.ThrowIfNull(template);

        Definition.Template = template;
        LoaderSource = loaderSource;
    }

    /// <summary>
    /// Initializes a new shader asset using the specified definition,
    /// resource source, and context-aware shader factory.
    /// </summary>
    /// <param name="definition">The shader asset definition.</param>
    /// <param name="loaderSource">The resource source associated with the shader.</param>
    /// <param name="factory">
    /// The factory used to create the shader using the supplied
    /// <see cref="ShaderAssetContext"/>.
    /// </param>
    public ShaderAsset(
        ShaderAssetDefinition definition,
        ResourceSourceBase? loaderSource,
        Func<ShaderAssetContext, IShader> factory)
        : this(definition, loaderSource,
            new Template<IShader, ShaderAssetContext>(factory))
    {
    }

    /// <summary>
    /// Resolves the default definition used by this asset.
    /// </summary>
    /// <returns>A new <see cref="ShaderAssetDefinition"/> instance.</returns>
    protected override ShaderAssetDefinition ResolveDefinition()
    {
        return new ShaderAssetDefinition();
    }

    /// <summary>
    /// Creates the runtime shader instance from the supplied shader data.
    /// </summary>
    /// <param name="stream">The stream containing the compiled shader data.</param>
    /// <returns>The created <see cref="IShader"/> instance.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="stream"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// The asset has not been initialized or no shader template is configured.
    /// </exception>
    protected override IShader Build(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        AssetContext context = Context ??
            throw new InvalidOperationException(
                $"{nameof(ShaderAsset)} must be initialized before loading.");

        Template<IShader, ShaderAssetContext> template = Definition.Template ??
            throw new InvalidOperationException(
                $"{nameof(ShaderAssetDefinition.Template)} is not configured.");

        using var memoryStream = new MemoryStream();
        stream.CopyTo(memoryStream);

        var rawEffect = new Effect(context.GraphicsDevice, memoryStream.ToArray());
        var shaderContext = new ShaderAssetContext(rawEffect);

        IShader shader = template.Create(shaderContext);
        shader.GraphicsDevice = context.GraphicsDevice;

        return shader;
    }
}