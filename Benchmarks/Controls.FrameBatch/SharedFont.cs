using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Engine.Controls.Drawing;

namespace Sachssoft.Engine.Controls.Benchmarks;
/// <summary>Shares the exact FontStashSharp font instance between both renderers.</summary>
internal sealed class SharedFont : IControlBatchFont
{
    /// <summary>Initializes a new shared font instance.</summary>
    public SharedFont(SpriteFontBase font) => Font = font;
    /// <summary>Gets the exact font instance used by both adapters.</summary>
    public SpriteFontBase Font { get; }

    /// <summary>Measures text with the shared FontStashSharp font.</summary>
    public Size2 Measure(string text) => new(Font.MeasureString(text));
    /// <summary>Draws text into the active Sasogine sprite batch using the shared font.</summary>
    public void Draw(SpriteBatch batch, string text, Vector2 position, Color color) => Font.DrawText(batch, text, position, color);
    /// <inheritdoc/>
    public void Draw(ControlFrameBatch batch, string text, Vector2 position, Color color) => batch.DrawText(Font, text, position, color);
}
