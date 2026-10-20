using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Sachssoft.Engine;
using Sachssoft.Engine.Input;
using Sachssoft.Engine.Controls;

internal sealed class TestFont : IControlFont
{
    /// <summary>Performs measure using the current control state.</summary>
    public Size2 Measure(string text)
    {
        int lines = 1, current = 0, max = 0;
        foreach (char value in text)
        {
            if (value == '\n')
            {
                max = Math.Max(max, current);
                current = 0;
                lines++;
            }
            else
                current++;
        }

        return new Size2(Math.Max(max, current) * 8, lines * 16);
    }

    /// <summary>Performs draw using the current control state.</summary>
    public void Draw(SpriteBatch batch, string text, Vector2 position, Color color) => throw new NotSupportedException("Headless test font");
}
