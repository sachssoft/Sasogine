using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Engine.Graphics.Text;

namespace Sachssoft.Engine.Controls.Benchmarks;
/// <summary>Shares the exact FontStashSharp font instance between both renderers.</summary>
internal sealed class SharedFont : IFont
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
    public bool CanDrawGlyphs => true;
    /// <inheritdoc/>
    public void DrawGlyphs(IFontGlyphSink sink, string text, TextTransform transform, Color color,
        CharacterLayoutOptions? layout = null) =>
        Font.DrawText(SharedFontGlyphRenderer.Get(sink), text, new Vector2(transform.Position.X, transform.Position.Y),
            color, rotation: transform.Rotation, origin: new Vector2(transform.Pivot.X, transform.Pivot.Y),
            scale: transform.Scale, characterSpacing: layout?.CharacterSpacing ?? 0f, lineSpacing: layout?.LineSpacing ?? 0f);
}
