using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Engine.Common;
using System;

namespace Sachssoft.Engine.Assets;

/// <summary>
/// Provides runtime dependencies required for asset initialization and
/// runtime asset creation.
/// </summary>
public sealed class AssetContext : IEngineObjectContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AssetContext"/> class.
    /// </summary>
    /// <param name="source">
    /// The asset collection used to resolve asset references.
    /// </param>
    /// <param name="graphicsDevice">
    /// The graphics device used by graphics-related assets.
    /// </param>
    public AssetContext(
        AssetCollection source,
        GraphicsDevice graphicsDevice)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(graphicsDevice);

        Source = source;
        GraphicsDevice = graphicsDevice;
    }

    /// <summary>
    /// Gets the asset collection used to resolve asset references.
    /// </summary>
    public AssetCollection Source { get; }

    /// <summary>
    /// Gets the graphics device used by graphics-related assets.
    /// </summary>
    public GraphicsDevice GraphicsDevice { get; }
}