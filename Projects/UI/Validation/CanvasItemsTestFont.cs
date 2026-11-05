using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Engine;
using Sachssoft.Engine.Controls.Drawing;

/// <summary>Supplies deterministic text metrics without graphics resources.</summary>
internal sealed class CanvasItemsTestFont : IControlFont
{
    /// <inheritdoc/>
    public Size2 Measure(string text) => new(text.Length * 8, 16);

    /// <inheritdoc/>
    public void Draw(SpriteBatch batch, string text, Vector2 position, Color color)
        => throw new InvalidOperationException("These checks must not draw.");
}
