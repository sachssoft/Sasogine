using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Engine;
using Sachssoft.Engine.Controls;
using Sachssoft.Engine.Controls.Drawing;
namespace LayoutCacheProbe;
/// <summary>Counts real rendering and measurement calls independently.</summary>
internal sealed class ProbeFont(SpriteFontBase font) : IControlBatchFont
{
    public int Measurements { get; private set; }
    public int Draws { get; private set; }
    public Vector2 LastPosition { get; private set; }
    public string LastText { get; private set; } = "";
    public Size2 Measure(string text) { Measurements++; return new Size2(font.MeasureString(text)); }
    public void Draw(SpriteBatch batch, string text, Vector2 position, Color color)
    { Draws++; LastPosition = position; LastText = text; font.DrawText(batch, text, position, color); }
    public void Draw(ControlFrameBatch batch, string text, Vector2 position, Color color)
    { Draws++; LastPosition=position; LastText=text; batch.DrawText(font,text,position,color); }
}

