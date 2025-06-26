using Microsoft.Xna.Framework;

namespace sachssoft.Sasogine.Surface.Visuals;

public interface IBrush
{
    void Draw(RenderContext context, Rectangle dest, Color color);
}

public static class IBrushExtensions
{
    public static void Draw(this IBrush brush, RenderContext context, Rectangle dest)
    {
        brush.Draw(context, dest, Color.White);
    }
}
