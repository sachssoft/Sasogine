using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sachssoft.Sasogine.Graphics.Meshes.Internal;

internal sealed class TextureCubeMesh
    : Mesh<VertexPositionTexture>
{
    public TextureCubeMesh(
        GraphicsDevice graphicsDevice,
        float size = 1f)
        : base(
            graphicsDevice,
            CreateVertices(size),
            CreateIndices())
    {
    }

    private static VertexPositionTexture[] CreateVertices(float size)
    {
        float h = size * 0.5f;

        return
        [
            // Front
            new(new Vector3(-h, -h, -h), new Vector2(0, 1)),
            new(new Vector3( h, -h, -h), new Vector2(1, 1)),
            new(new Vector3( h,  h, -h), new Vector2(1, 0)),
            new(new Vector3(-h,  h, -h), new Vector2(0, 0)),

            // Back
            new(new Vector3( h, -h, h), new Vector2(0, 1)),
            new(new Vector3(-h, -h, h), new Vector2(1, 1)),
            new(new Vector3(-h,  h, h), new Vector2(1, 0)),
            new(new Vector3( h,  h, h), new Vector2(0, 0)),

            // Top
            new(new Vector3(-h, h, -h), new Vector2(0, 1)),
            new(new Vector3( h, h, -h), new Vector2(1, 1)),
            new(new Vector3( h, h, h), new Vector2(1, 0)),
            new(new Vector3(-h, h, h), new Vector2(0, 0)),

            // Bottom
            new(new Vector3(-h, -h, h), new Vector2(0, 1)),
            new(new Vector3( h, -h, h), new Vector2(1, 1)),
            new(new Vector3( h, -h, -h), new Vector2(1, 0)),
            new(new Vector3(-h, -h, -h), new Vector2(0, 0)),

            // Right
            new(new Vector3(h, -h, -h), new Vector2(0, 1)),
            new(new Vector3(h, -h, h), new Vector2(1, 1)),
            new(new Vector3(h, h, h), new Vector2(1, 0)),
            new(new Vector3(h, h, -h), new Vector2(0, 0)),

            // Left
            new(new Vector3(-h, -h, h), new Vector2(0, 1)),
            new(new Vector3(-h, -h, -h), new Vector2(1, 1)),
            new(new Vector3(-h, h, -h), new Vector2(1, 0)),
            new(new Vector3(-h, h, h), new Vector2(0, 0))
        ];
    }

    private static short[] CreateIndices()
    {
        return
        [
            // Front
            0, 1, 2,
            0, 2, 3,

            // Back
            4, 5, 6,
            4, 6, 7,

            // Top
            8, 9, 10,
            8, 10, 11,

            // Bottom
            12, 13, 14,
            12, 14, 15,

            // Right
            16, 17, 18,
            16, 18, 19,

            // Left
            20, 21, 22,
            20, 22, 23
        ];
    }
}