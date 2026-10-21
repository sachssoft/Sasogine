using Microsoft.Xna.Framework;
using Sachssoft.Engine.Controls;
/// <summary>Supplies a deterministic ready pipette color.</summary>
internal sealed class SceneViewportTestColorSource : IColorPickSource
{
    public bool TryPick(Vector2 position, out Color color) { color = Color.Red; return true; }
}
