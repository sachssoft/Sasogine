using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Engine;
using Sachssoft.Engine.Controls;

/// <summary>Counts text metric requests and supports explicit external font changes.</summary>
internal sealed class CountingFont : IControlFont
{
    private readonly TestFont _font = new();
    /// <summary>Gets the number of measurement calls.</summary>
    public int Measurements { get; private set; }
    /// <summary>Gets or sets a multiplier simulating mutable font metrics.</summary>
    public float Scale { get; set; } = 1;
    /// <inheritdoc/>
    public Size2 Measure(string text)
    {
        Measurements++;
        Size2 size = _font.Measure(text);
        return new Size2(size.Width * Scale, size.Height * Scale);
    }
    /// <inheritdoc/>
    public void Draw(SpriteBatch batch, string text, Vector2 position, Color color) => throw new NotSupportedException("Headless font");
}
