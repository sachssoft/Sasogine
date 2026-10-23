using Microsoft.Xna.Framework;

namespace Sachssoft.Engine.Controls.Drawing;
/// <summary>
/// Draws glyphs directly through the native UI batch while retaining the legacy font contract.
/// </summary>
public interface IControlBatchFont : IControlFont
{
    /// <summary>
    /// Appends local glyph geometry to the active frame batch.
    /// </summary>
    void Draw(ControlFrameBatch batch, string text, Vector2 position, Color color);
}
