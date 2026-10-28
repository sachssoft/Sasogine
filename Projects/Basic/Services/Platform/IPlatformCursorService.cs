using Microsoft.Xna.Framework.Graphics;

namespace Sachssoft.Engine.Services.Platform;

/// <summary>
/// Controls the platform cursor independently of pointer input and software cursor rendering.
/// </summary>
/// <remarks>
/// Invoke this service on the game thread after window initialization. Cursor identifiers are
/// 0: arrow, 1: hand, 2: text, 3: crosshair, 4: move, 5: horizontal resize,
/// 6: vertical resize, 7: northwest/southeast resize, 8: northeast/southwest resize,
/// 9: unavailable, 10: wait, and 11: arrow with wait. Unsupported identifiers use the arrow.
/// </remarks>
public interface IPlatformCursorService
{
    /// <summary>
    /// Gets or sets whether the platform cursor is visible, independently of its selected image.
    /// </summary>
    bool IsVisible
    {
        get;
        set;
    }

    /// <summary>
    /// Selects a standard platform cursor without changing its visibility.
    /// </summary>
    /// <param name="cursorId">The platform-independent cursor identifier described by this interface.</param>
    void SetCursor(int cursorId);

    /// <summary>
    /// Creates and selects a platform cursor from the supplied texture without changing visibility.
    /// </summary>
    /// <param name="texture">The color texture whose pixels are copied; ownership stays with the caller.</param>
    /// <param name="hotspot">The click position in pixels, relative to the texture's top-left corner.</param>
    /// <remarks>
    /// The hotspot must lie inside the texture. Call when the image changes, rather than every frame.
    /// The service owns the resulting native cursor and releases it when replaced or reset.
    /// </remarks>
    void SetCustomCursor(Texture2D texture, PixelPoint2 hotspot);

    /// <summary>
    /// Restores the standard arrow and releases the custom cursor without changing visibility.
    /// </summary>
    void ResetCursor();
}
