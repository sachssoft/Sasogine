using Microsoft.Xna.Framework.Graphics;

namespace Sachssoft.Engine.Controls.Drawing;
/// <summary>
/// Links draw items sharing a texture and tracks conservative bounds for safe regrouping.
/// </summary>
internal struct ControlBatchGroup
{
    internal Texture2D? Texture;
    internal Bounds2 Bounds;
    internal int First, Last, StartIndex, IndexCount;
}
