using Microsoft.Xna.Framework.Graphics;
using System;

namespace Sachssoft.Engine.Assets.Graphics;

/// <summary>
/// Provides the runtime context required to create a shader instance.
/// </summary>
public sealed class ShaderAssetContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ShaderAssetContext"/> class.
    /// </summary>
    /// <param name="rawEffect">
    /// The raw effect used to create the shader instance.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="rawEffect"/> is <see langword="null"/>.
    /// </exception>
    public ShaderAssetContext(Effect rawEffect)
    {
        ArgumentNullException.ThrowIfNull(rawEffect);

        RawEffect = rawEffect;
    }

    /// <summary>
    /// Gets the raw effect used to create the shader instance.
    /// </summary>
    public Effect RawEffect { get; }
}