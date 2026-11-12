using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Engine;
using Sachssoft.Engine.Graphics.Text;
namespace LayoutCacheProbe;
/// <summary>Counts real rendering and measurement calls independently.</summary>
internal sealed class ProbeFont(IFont font) : IFont
{
    public int Measurements { get; private set; }
    public int Draws { get; private set; }
    public Vector2 LastPosition { get; private set; }
    public string LastText { get; private set; } = "";
    public bool CanDrawGlyphs => font.CanDrawGlyphs;
    public Size2 Measure(string text) { Measurements++; return font.Measure(text); }
    public void Draw(SpriteBatch batch, string text, Vector2 position, Color color)
    { Draws++; LastPosition = position; LastText = text; font.Draw(batch, text, position, color); }
    public void DrawGlyphs(IFontGlyphSink sink, string text, TextTransform transform, Color color, CharacterLayoutOptions? layout = null)
    {
        Draws++;
        LastPosition = new Vector2(transform.Position.X, transform.Position.Y);
        LastText = text;
        font.DrawGlyphs(sink, text, transform, color, layout);
    }
}

