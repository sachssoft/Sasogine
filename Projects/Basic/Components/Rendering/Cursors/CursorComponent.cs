using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Scenes;
using System;

namespace Sachssoft.Sasogine.Components.Rendering;

/// <summary>
/// Represents the base class for runtime cursor components.
/// </summary>
public abstract class CursorComponent : ResourceComponentBase, IDrawableComponent
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CursorComponent"/> class.
    /// </summary>
    /// <param name="graphicsDevice">
    /// The graphics device associated with the cursor.
    /// </param>
    protected CursorComponent(GraphicsDevice graphicsDevice)
    {
        ArgumentNullException.ThrowIfNull(graphicsDevice);

        GraphicsDevice = graphicsDevice;
    }

    /// <summary>
    /// Gets the graphics device associated with the cursor.
    /// </summary>
    protected GraphicsDevice GraphicsDevice { get; }

    /// <summary>
    /// Gets or sets whether the cursor is visible.
    /// </summary>
    public bool IsVisible { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the cursor is enabled.
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the opacity of the cursor.
    /// </summary>
    public float Opacity { get; set; } = 1f;

    /// <summary>
    /// Gets or sets the diffuse color of the cursor.
    /// </summary>
    public Color DiffuseColor { get; set; } = Color.White;

    /// <summary>
    /// Gets or sets the texture used to render the cursor.
    /// </summary>
    public Texture2D? Texture { get; set; }
}