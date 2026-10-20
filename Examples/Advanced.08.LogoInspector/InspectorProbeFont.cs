using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sachssoft.Engine.Controls.Examples;
/// <summary>Provides deterministic headless layout metrics for inspector checks.</summary>
internal sealed class InspectorProbeFont : IControlFont
{
    public Size2 Measure(string text) => new(text.Length * 8, 18);
    public void Draw(SpriteBatch batch, string text, Vector2 position, Color color) => throw new NotSupportedException();
}
