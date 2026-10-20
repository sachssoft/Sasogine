using Microsoft.Xna.Framework;

namespace Sachssoft.Engine.Controls.Examples;
/// <summary>Provides a deterministic color source for input and rollback checks.</summary>
internal sealed class InspectorColorProbe : IColorPickSource
{
    internal int Samples { get; private set; }
    public bool TryPick(Vector2 position, out Color color)
    {
        Samples++; color = Color.Red;
        return position.X >= 0 && position.Y >= 0;
    }
}
