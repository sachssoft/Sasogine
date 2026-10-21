using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace Sachssoft.Engine.Controls.Drawing;
/// <summary>Preserves UI vertex data and adds a stable texture slot.</summary>
internal struct ControlTextureVertex : IVertexType
{
    internal Vector3 Position;
    internal Color Color;
    internal Vector2 TextureCoordinate;
    internal float TextureSlot;
    internal static readonly VertexDeclaration Declaration = new(
        new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
        new VertexElement(12, VertexElementFormat.Color, VertexElementUsage.Color, 0),
        new VertexElement(16, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
        new VertexElement(24, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 1));
    VertexDeclaration IVertexType.VertexDeclaration => Declaration;
}
