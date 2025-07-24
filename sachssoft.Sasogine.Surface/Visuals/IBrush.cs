using Microsoft.Xna.Framework;

namespace sachssoft.Sasogine.Surface.Visuals;

public interface IBrush
{
    void Draw(RenderContext context, Rectangle dest, Color color);
}