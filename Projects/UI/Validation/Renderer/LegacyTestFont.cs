using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Engine.Controls.Drawing;
namespace Sachssoft.Engine.Controls.Examples;
/// <summary>Exercises the retained SpriteBatch-only font contract.</summary>
internal sealed class LegacyTestFont(IControlFont inner) : IControlFont
{
    public Size2 Measure(string text)=>inner.Measure(text);
    public void Draw(SpriteBatch batch,string text,Vector2 position,Color color)=>inner.Draw(batch,text,position,color);
}
