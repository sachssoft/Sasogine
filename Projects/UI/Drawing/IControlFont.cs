using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sachssoft.Engine.Controls.Drawing;
/// <summary>
/// Measures and draws text using the same font metrics.
/// </summary>
public interface IControlFont
{
    /// <summary>
    /// Measures unwrapped text in logical UI units.
    /// </summary>
    Size2 Measure(string text);
    /// <summary>
    /// Draws into an already active batch without beginning or ending it.
    /// </summary>
    void Draw(SpriteBatch batch, string text, Vector2 position, Color color);
}
