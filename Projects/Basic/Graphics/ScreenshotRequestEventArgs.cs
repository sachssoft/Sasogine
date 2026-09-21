using Microsoft.Xna.Framework.Graphics;
using System;

namespace Sachssoft.Sasogine.Graphics;

/// <summary>
/// Provides data for a screenshot capture request event.
/// </summary>
public class ScreenshotRequestEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ScreenshotRequestEventArgs"/> class.
    /// </summary>
    /// <param name="capture">The texture containing the captured screenshot.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="capture"/> is <see langword="null"/>.
    /// </exception>
    public ScreenshotRequestEventArgs(Texture2D capture)
    {
        Capture = capture ?? throw new ArgumentNullException(nameof(capture));
    }

    /// <summary>
    /// Gets the texture containing the captured screenshot.
    /// </summary>
    public Texture2D Capture { get; }
}